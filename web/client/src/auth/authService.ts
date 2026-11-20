import {
  createUserWithEmailAndPassword,
  sendPasswordResetEmail,
  signInWithEmailAndPassword,
  signOut as firebaseSignOut,
} from 'firebase/auth'
import { get, ref, set } from 'firebase/database'
import { auth, db } from '../firebase'

// The user node saved by the app in users/{uid} (FirebaseAuthManager.UserDataInDatabase)
export interface UserProfile {
  username: string
  email: string
  uid: string
}

export async function loadUserProfile(uid: string): Promise<UserProfile | null> {
  const snapshot = await get(ref(db, `users/${uid}`))
  if (!snapshot.exists()) return null
  const value = snapshot.val() as Partial<UserProfile>
  return {
    username: value.username ?? '',
    email: value.email ?? '',
    uid: value.uid ?? uid,
  }
}

export async function signIn(email: string, password: string): Promise<void> {
  await signInWithEmailAndPassword(auth, email.trim(), password)
}

// Creates the account and its users/{uid} node, like SignUpUserEmail + SaveUserToServer in the app
export async function signUp(username: string, email: string, password: string): Promise<UserProfile> {
  const credential = await createUserWithEmailAndPassword(auth, email.trim(), password)
  const profile: UserProfile = {
    username: username.trim(),
    email: email.trim(),
    uid: credential.user.uid,
  }
  await set(ref(db, `users/${profile.uid}`), profile)
  return profile
}

export async function resetPassword(email: string): Promise<void> {
  await sendPasswordResetEmail(auth, email.trim())
}

export async function signOut(): Promise<void> {
  await firebaseSignOut(auth)
}

export function errorCode(error: unknown): string {
  if (typeof error === 'object' && error !== null && 'code' in error)
    return String((error as { code: unknown }).code)
  return ''
}
