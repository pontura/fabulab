import { get, ref } from 'firebase/database'
import { db } from '../firebase'
import type { WorkKind } from './works'

// A work of another user used by a story or a minigame (a character, object or background, also the objects inside
// backgrounds and characters)
export interface Credit {
  kind: WorkKind
  id: string
  // The author of the original (images/{authorId}/{kind}/{id}.jpg)
  authorId: string
}

// The copies a story or minigame carries of every work it shows (StoryAssetsData in the app):
//   { characters: { [id]: { userID, version, data } }, so: { [id]: { userID, version, data } } }
interface CopiedWorks {
  characters?: Record<string, { userID?: string } | null>
  so?: Record<string, { userID?: string } | null>
}

// The works of other users that a story shows (its copies in storyAssets/{author}/{storyId})
export function loadStoryCredits(authorId: string, storyId: string): Promise<Credit[]> {
  return loadCredits(`storyAssets/${authorId}/${storyId}`, authorId)
}

// The works of other users that a minigame shows (its copies in minigames/{author}/{id}/works)
export function loadMiniGameCredits(authorId: string, gameId: string): Promise<Credit[]> {
  return loadCredits(`minigames/${authorId}/${gameId}/works`, authorId)
}

// Works saved before the copies existed have none: no credits
async function loadCredits(path: string, authorId: string): Promise<Credit[]> {
  const snapshot = await get(ref(db, path))
  const works = (snapshot.val() ?? {}) as CopiedWorks
  return [...fromCopies('characters', works.characters, authorId), ...fromCopies('so', works.so, authorId)]
}

function fromCopies(kind: WorkKind, copies: CopiedWorks['characters'], authorId: string): Credit[] {
  if (!copies || typeof copies !== 'object') return []
  return Object.entries(copies)
    .filter(([, copy]) => copy && typeof copy.userID === 'string' && copy.userID !== '' && copy.userID !== authorId)
    .map(([id, copy]) => ({ kind, id, authorId: copy!.userID! }))
}
