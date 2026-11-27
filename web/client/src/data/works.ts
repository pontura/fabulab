import { get, ref } from 'firebase/database'
import { db } from '../firebase'
import { parseCreators } from './creators'
import { byNewest } from './timestamps'

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
  // The users that made it, the one that modified it last at the end
  creators: string[]
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
  creators?: unknown
}

// All the characters / objects, loaded once per visit (until invalidateWorks)
const cache = new Map<WorkKind, Promise<WorkMeta[]>>()

function loadAllWorks(kind: WorkKind): Promise<WorkMeta[]> {
  let works = cache.get(kind)
  if (!works) {
    works = fetchAllWorks(kind).catch((error) => {
      cache.delete(kind) // a failed load can be retried
      throw error
    })
    cache.set(kind, works)
  }
  return works
}

// After the editor changed something
export function invalidateWorks() {
  cache.clear()
}

// The public characters or objects, the most liked first (AllCharactersScreen / AllObjectsScreen)
export function loadPublicWorks(kind: WorkKind): Promise<WorkMeta[]> {
  return loadAllWorks(kind).then((works) => works.filter((w) => w.isPublic).sort((a, b) => b.likes - a.likes))
}

// One character or object by its id (null: there is none, ex: deleted by its author)
export function findWork(kind: WorkKind, id: string): Promise<WorkMeta | null> {
  return loadAllWorks(kind).then((works) => works.find((w) => w.id === id) ?? null)
}

// The characters or objects of a user, also the private ones, the newest first
export function loadUserWorks(kind: WorkKind, uid: string): Promise<WorkMeta[]> {
  return loadAllWorks(kind).then((works) => works.filter((w) => w.userID === uid).sort(byNewest))
}

async function fetchAllWorks(kind: WorkKind): Promise<WorkMeta[]> {
  const snapshot = await get(ref(db, `metadata/${kind}`))
  if (!snapshot.exists()) return []

  const values = snapshot.val() as Record<string, ServerWorkMeta | null>
  return Object.entries(values)
    .filter((entry): entry is [string, ServerWorkMeta] => entry[1] !== null && typeof entry[1] === 'object')
    .map(([id, value]) => toWorkMeta(id, value))
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
    creators: parseCreators(value.creators),
  }
}
