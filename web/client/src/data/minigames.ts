import { get, ref } from 'firebase/database'
import { db } from '../firebase'
import { byNewest } from './timestamps'

// metadata/minigames/{id} (MiniGameMetaData in the app). Their thumbnail: images/{userID}/minigames/{id}.jpg
export interface MiniGameMeta {
  id: string
  name: string
  userID: string
  // The game that plays it (MiniGame.Id in Unity, ex: runner)
  mechanic: string
  isPublic: boolean
  timestamp: string
  // The minigame it was copied from
  sourceId: string
  // An original of Fabulab, the model the users copy
  isOfficial: boolean
}

interface ServerMiniGameMeta {
  isOfficial?: boolean | string
  name?: string
  userID?: string
  mechanic?: string
  isPublic?: boolean | string
  timestamp?: string
  sourceId?: string
}

let cache: Promise<MiniGameMeta[]> | null = null

function loadAll(): Promise<MiniGameMeta[]> {
  if (!cache)
    cache = fetchAll().catch((error) => {
      cache = null // a failed load can be retried
      throw error
    })
  return cache
}

// After the editor changed something
export function invalidateMiniGames() {
  cache = null
}

// The originals of Fabulab (public), the newest first
export function loadOfficialMiniGames(): Promise<MiniGameMeta[]> {
  return loadAll().then((all) => all.filter((g) => g.isOfficial && g.isPublic).sort(byNewest))
}

// The public versions made by the users, the newest first
export function loadCommunityMiniGames(): Promise<MiniGameMeta[]> {
  return loadAll().then((all) => all.filter((g) => !g.isOfficial && g.isPublic).sort(byNewest))
}

// The minigames of a user, also the private ones, the newest first
export function loadUserMiniGames(uid: string): Promise<MiniGameMeta[]> {
  return loadAll().then((all) => all.filter((g) => g.userID === uid).sort(byNewest))
}

async function fetchAll(): Promise<MiniGameMeta[]> {
  const snapshot = await get(ref(db, 'metadata/minigames'))
  if (!snapshot.exists()) return []
  const values = snapshot.val() as Record<string, ServerMiniGameMeta | null>
  return Object.entries(values)
    .filter((entry): entry is [string, ServerMiniGameMeta] => entry[1] !== null && typeof entry[1] === 'object')
    .map(([id, v]) => ({
      id,
      name: v.name ?? '',
      userID: v.userID ?? '',
      mechanic: v.mechanic ?? '',
      isPublic: v.isPublic === true || v.isPublic === 'true',
      timestamp: v.timestamp ?? '',
      sourceId: v.sourceId ?? '',
      isOfficial: v.isOfficial === true || v.isOfficial === 'true',
    }))
}
