import { get, ref } from 'firebase/database'
import { db } from '../firebase'

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

// The stories of the home of the app (AllStoriesScreen): the public ones, without the ones made in a game,
// the most liked first
export async function loadFeaturedStories(): Promise<StoryMeta[]> {
  const snapshot = await get(ref(db, 'metadata/stories'))
  if (!snapshot.exists()) return []

  const all = snapshot.val() as Record<string, ServerStoryMeta | null>
  return Object.entries(all)
    .filter((entry): entry is [string, ServerStoryMeta] => entry[1] !== null && typeof entry[1] === 'object')
    .map(([id, value]) => toStoryMeta(id, value))
    .filter((story) => story.isPublic && !story.tags.includes('games'))
    .sort((a, b) => b.likes - a.likes)
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
