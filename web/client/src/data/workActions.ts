import { get, push, ref, update } from 'firebase/database'
import { deleteObject, getBytes, ref as storageRef, uploadBytes } from 'firebase/storage'
import { db, storage } from '../firebase'

// The works of a user that can be managed from the home: their data is at {kind}/{uid}/{id}, their metadata at
// metadata/{kind}/{id} and their thumbnail at images/{uid}/{kind}/{id}.jpg (FirebaseFabulabDBManager)
export type ManagedKind = 'stories' | 'characters' | 'so'

export interface WorkInfo {
  name: string
  isPublic: boolean
  tags: string[]
}

export interface Tag {
  id: string
  name: string
}

interface ServerTag {
  id?: string
  name?: string
  // The kinds of works it applies to (MetadataTypes in the app). Without it, it applies to all of them
  itemTypes?: string[] | Record<string, string>
}

// The tags a work of this kind can have (TagsEditor in the app)
export async function loadTags(kind: ManagedKind): Promise<Tag[]> {
  const snapshot = await get(ref(db, 'tags'))
  if (!snapshot.exists()) return []
  const values = snapshot.val() as Record<string, ServerTag | null>
  return Object.entries(values)
    .filter((entry): entry is [string, ServerTag] => entry[1] !== null && typeof entry[1] === 'object')
    .filter(([, tag]) => {
      if (!tag.itemTypes) return true
      const types = Array.isArray(tag.itemTypes) ? tag.itemTypes : Object.values(tag.itemTypes)
      return types.includes(kind)
    })
    .map(([key, tag]) => ({ id: tag.id ?? key, name: tag.name ?? key }))
}

// Name, privacy and tags (InfoDataScreen in the app). Likes and the rest of the metadata are kept
export async function saveWorkInfo(kind: ManagedKind, id: string, info: WorkInfo): Promise<void> {
  await update(ref(db, `metadata/${kind}/${id}`), {
    name: info.name,
    isPublic: info.isPublic,
    tags: info.tags,
  })
}

// A private copy of a work of the user, with its data, its thumbnail and, for stories, the copies of the works
// it shows. Returns the id of the copy
export async function duplicateWork(kind: ManagedKind, uid: string, id: string, name: string): Promise<string> {
  const [data, meta, assets] = await Promise.all([
    readValue(`${kind}/${uid}/${id}`),
    readValue(`metadata/${kind}/${id}`),
    kind === 'stories' ? readValue(`storyAssets/${uid}/${id}`) : Promise.resolve(null),
  ])
  if (data === null || meta === null || typeof meta !== 'object') throw new Error(`Work not found: ${kind}/${id}`)

  const copyId = push(ref(db, `${kind}/${uid}`)).key
  if (!copyId) throw new Error('Could not create an id for the copy')

  // The thumbnail first, so the copy shows it as soon as it appears in the lists
  await copyImage(`images/${uid}/${kind}/${id}.jpg`, `images/${uid}/${kind}/${copyId}.jpg`)

  const { likes: _likes, ...rest } = meta as Record<string, unknown>
  const changes: Record<string, unknown> = {
    [`${kind}/${uid}/${copyId}`]: data,
    [`metadata/${kind}/${copyId}`]: {
      ...rest,
      userID: uid,
      name: copyName(name),
      isPublic: false,
      timestamp: new Date().toISOString(),
    },
  }
  if (assets !== null) changes[`storyAssets/${uid}/${copyId}`] = assets
  await update(ref(db), changes)
  return copyId
}

// Deletes a work of the user: its data, metadata, thumbnail and story copies (DeletePart / DeleteFilm in the app)
export async function deleteWork(kind: ManagedKind, uid: string, id: string): Promise<void> {
  const changes: Record<string, null> = {
    [`${kind}/${uid}/${id}`]: null,
    [`metadata/${kind}/${id}`]: null,
  }
  if (kind === 'stories') changes[`storyAssets/${uid}/${id}`] = null
  await update(ref(db), changes)
  // The work is already gone: a thumbnail that could not be deleted only takes space
  await deleteObject(storageRef(storage, `images/${uid}/${kind}/${id}.jpg`)).catch((e) =>
    console.warn('Could not delete the thumbnail', e),
  )
}

async function readValue(path: string): Promise<unknown> {
  const snapshot = await get(ref(db, path))
  return snapshot.exists() ? snapshot.val() : null
}

// A work without thumbnail still works: a failed copy is only logged
async function copyImage(from: string, to: string): Promise<void> {
  try {
    const bytes = await getBytes(storageRef(storage, from))
    await uploadBytes(storageRef(storage, to), bytes, { contentType: 'image/jpeg' })
  } catch (e) {
    console.warn('Could not copy the thumbnail', e)
  }
}

function copyName(name: string): string {
  const base = name.trim()
  return base ? `${base} (copia)` : ''
}
