import { get, ref } from 'firebase/database'
import { db } from '../firebase'

// metadata/*/{id}/creators: the users that made a work, the one that modified it last at the end (WorkCreators in
// the app). The database may return the list as an array or as an object with the keys 0, 1, 2...
export function parseCreators(value: unknown): string[] {
  const list = Array.isArray(value) ? value : value && typeof value === 'object' ? Object.values(value) : []
  return list.filter((c): c is string => typeof c === 'string' && c !== '')
}

// The one that modified the work last. Works saved before there were creators: their author
export function lastCreator(creators: string[] | undefined, author: string): string {
  return creators && creators.length > 0 ? creators[creators.length - 1] : author
}

// users/{uid}/username (public). One request per user, also when many cards show it. '' when it has none
const usernames = new Map<string, Promise<string>>()

export function loadUsername(uid: string): Promise<string> {
  let name = usernames.get(uid)
  if (!name) {
    name = get(ref(db, `users/${uid}/username`))
      .then((snapshot) => (typeof snapshot.val() === 'string' ? (snapshot.val() as string) : ''))
      .catch(() => '')
    usernames.set(uid, name)
  }
  return name
}
