import { get, ref } from 'firebase/database'
import { db } from '../firebase'
import { byNewest } from './timestamps'

// metadata/stories/{id} (ServerFilmData in the app)
export interface StoryMeta {
  id: string
  name: string
  userID: string
  likes: number
  timestamp: string
  isPublic: boolean
  tags: string[]
}

interface ServerStoryMeta {
  name?: string
  userID?: string
  likes?: number | string
  timestamp?: string
  isPublic?: boolean | string
  tags?: string[] | Record<string, string>
}

// All the stories, loaded once per visit (until invalidateStories)
let all: Promise<StoryMeta[]> | null = null

function loadAllStories(): Promise<StoryMeta[]> {
  if (!all) {
    all = fetchAllStories().catch((error) => {
      all = null // a failed load can be retried
      throw error
    })
  }
  return all
}

// After the editor changed something
export function invalidateStories() {
  all = null
}

// The stories of the home of the app (AllStoriesScreen): the public ones, without the ones made in a game,
// the most liked first
export function loadFeaturedStories(): Promise<StoryMeta[]> {
  return loadAllStories().then((stories) =>
    stories.filter((s) => s.isPublic && !s.tags.includes('games')).sort((a, b) => b.likes - a.likes),
  )
}

// The stories of a user, also the private ones, the newest first
export function loadUserStories(uid: string): Promise<StoryMeta[]> {
  return loadAllStories().then((stories) => stories.filter((s) => s.userID === uid).sort(byNewest))
}

async function fetchAllStories(): Promise<StoryMeta[]> {
  const snapshot = await get(ref(db, 'metadata/stories'))
  if (!snapshot.exists()) return []

  const values = snapshot.val() as Record<string, ServerStoryMeta | null>
  return Object.entries(values)
    .filter((entry): entry is [string, ServerStoryMeta] => entry[1] !== null && typeof entry[1] === 'object')
    .map(([id, value]) => toStoryMeta(id, value))
}

function toStoryMeta(id: string, value: ServerStoryMeta): StoryMeta {
  return {
    id,
    name: value.name ?? '',
    userID: value.userID ?? '',
    likes: Number(value.likes) || 0,
    timestamp: value.timestamp ?? '',
    isPublic: value.isPublic === true || value.isPublic === 'true',
    tags: Array.isArray(value.tags) ? value.tags : value.tags ? Object.values(value.tags) : [],
  }
}
