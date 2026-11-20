import { get, ref } from 'firebase/database'
import { db } from '../firebase'

// metadata/characters/{id} and metadata/so/{id} (CharacterMetaData / PropMetaData in the app)
export interface WorkMeta {
  id: string
  name: string
  userID: string
  likes: number
  timestamp: string
  isPublic: boolean
  tags: string[]
  // Objects only: SObjectData.types (0 generic, 1 background)
  type: number
}

export type WorkKind = 'characters' | 'so'

export const ObjectType = {
  generic: 0,
  background: 1,
} as const

interface ServerWorkMeta {
  name?: string
  userID?: string
  likes?: number | string
  timestamp?: string
  isPublic?: boolean | string
  tags?: string[] | Record<string, string>
  type?: number | string
}

// Loaded once per visit
const cache = new Map<WorkKind, Promise<WorkMeta[]>>()

// The public characters or objects, the most liked first (AllCharactersScreen / AllObjectsScreen)
export function loadPublicWorks(kind: WorkKind): Promise<WorkMeta[]> {
  let works = cache.get(kind)
  if (!works) {
    works = fetchPublicWorks(kind).catch((error) => {
      cache.delete(kind) // a failed load can be retried
      throw error
    })
    cache.set(kind, works)
  }
  return works
}

async function fetchPublicWorks(kind: WorkKind): Promise<WorkMeta[]> {
  const snapshot = await get(ref(db, `metadata/${kind}`))
  if (!snapshot.exists()) return []

  const all = snapshot.val() as Record<string, ServerWorkMeta | null>
  return Object.entries(all)
    .filter((entry): entry is [string, ServerWorkMeta] => entry[1] !== null && typeof entry[1] === 'object')
    .map(([id, value]) => toWorkMeta(id, value))
    .filter((work) => work.isPublic)
    .sort((a, b) => b.likes - a.likes)
}

function toWorkMeta(id: string, value: ServerWorkMeta): WorkMeta {
  return {
    id,
    name: value.name ?? '',
    userID: value.userID ?? '',
    likes: Number(value.likes) || 0,
    timestamp: value.timestamp ?? '',
    isPublic: value.isPublic === true || value.isPublic === 'true',
    tags: Array.isArray(value.tags) ? value.tags : value.tags ? Object.values(value.tags) : [],
    type: Number(value.type) || 0,
  }
}
