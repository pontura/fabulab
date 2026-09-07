import { get, push, ref, remove, set, update as fbUpdate } from 'firebase/database'
import { db } from './firebase'

export async function readPath<T>(path: string): Promise<T | null> {
  const snap = await get(ref(db, path))
  return snap.exists() ? (snap.val() as T) : null
}

export async function writePath(path: string, value: unknown): Promise<void> {
  await set(ref(db, path), value)
}

export async function removePath(path: string): Promise<void> {
  await remove(ref(db, path))
}

export async function multiUpdate(updates: Record<string, unknown>): Promise<void> {
  await fbUpdate(ref(db), updates)
}

export async function pushPath(path: string, value: unknown): Promise<string> {
  const newRef = push(ref(db, path))
  await set(newRef, value)
  return newRef.key as string
}
