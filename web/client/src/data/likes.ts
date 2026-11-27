import { get, ref, runTransaction, set } from 'firebase/database'
import { db } from '../firebase'

// Likes, as the app does them (UserData.OnLikeUpdate, FirebaseFabulabDBManager):
//   users/{uid}/likes/{workId} = ""       the works the user liked (only once each)
//   metadata/{kind}/{workId}/likes       how many likes the work has, changed with a transaction (never below 0)
export type LikeKind = 'stories' | 'characters' | 'so'

// The ids of the works the user liked
export async function loadLikedIds(uid: string): Promise<Set<string>> {
  const snapshot = await get(ref(db, `users/${uid}/likes`))
  const value = snapshot.val()
  return new Set(value && typeof value === 'object' ? Object.keys(value) : [])
}

// Likes a work (like: true) or takes the like back. The caller makes sure it is not liked twice
export async function setLike(kind: LikeKind, workId: string, uid: string, like: boolean): Promise<void> {
  await set(ref(db, `users/${uid}/likes/${workId}`), like ? '' : null)
  await runTransaction(ref(db, `metadata/${kind}/${workId}/likes`), (current) => {
    const count = Number(current) || 0
    return like ? count + 1 : Math.max(0, count - 1)
  })
}
