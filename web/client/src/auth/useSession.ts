import { onAuthStateChanged, signInAnonymously, type User } from 'firebase/auth'
import { useCallback, useEffect, useState } from 'react'
import { auth } from '../firebase'
import { loadUserProfile, type UserProfile } from './authService'

export interface Session {
  // False only until Firebase says who the user is (on the first load)
  ready: boolean
  // The Firebase user. A visitor without account is an anonymous user (Storage only serves images to signed in
  // users); null only if the anonymous sign in failed
  user: User | null
  // True for visitors without account: they see everything but must sign in to create or edit
  isGuest: boolean
  profile: UserProfile | null
  // The sign up writes the profile after the account exists: it sets it here once saved
  setProfile: (profile: UserProfile) => void
}

// The Firebase session (kept by the browser between visits) and the user's profile in the database.
// Without a signed in user, the visitor enters as an anonymous user, like "Comenzar" in the app
export function useSession(): Session {
  const [ready, setReady] = useState(false)
  const [user, setUser] = useState<User | null>(null)
  const [profile, setProfileState] = useState<UserProfile | null>(null)

  useEffect(() => {
    let current = true
    let signingInAnonymously = false
    const unsubscribe = onAuthStateChanged(auth, async (firebaseUser) => {
      setUser(firebaseUser)
      setProfileState(null)

      if (!firebaseUser) {
        // Nobody signed in (first visit or after signing out): enter as a guest
        if (signingInAnonymously) return
        signingInAnonymously = true
        try {
          await signInAnonymously(auth) // onAuthStateChanged runs again with the anonymous user
        } catch (error) {
          console.error('Could not enter as a guest', error)
          if (current) setReady(true)
        } finally {
          signingInAnonymously = false
        }
        return
      }

      setReady(true)
      if (firebaseUser.isAnonymous) return
      try {
        const loaded = await loadUserProfile(firebaseUser.uid)
        // A sign up in progress may not have saved the profile yet: keep the one it sets
        if (current && loaded && auth.currentUser?.uid === firebaseUser.uid) setProfileState(loaded)
      } catch (error) {
        console.error('Could not load the user profile', error)
      }
    })
    return () => {
      current = false
      unsubscribe()
    }
  }, [])

  const setProfile = useCallback((value: UserProfile) => setProfileState(value), [])

  return { ready, user, isGuest: !user || user.isAnonymous, profile, setProfile }
}
