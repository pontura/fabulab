import { onAuthStateChanged, type User } from 'firebase/auth'
import { useCallback, useEffect, useState } from 'react'
import { auth } from '../firebase'
import { loadUserProfile, type UserProfile } from './authService'

export type SessionStatus = 'loading' | 'signedOut' | 'signedIn'

export interface Session {
  status: SessionStatus
  user: User | null
  profile: UserProfile | null
  // The sign up writes the profile after the account exists: it sets it here once saved
  setProfile: (profile: UserProfile) => void
}

// The Firebase session (kept by the browser between visits) and the user's profile in the database
export function useSession(): Session {
  const [status, setStatus] = useState<SessionStatus>('loading')
  const [user, setUser] = useState<User | null>(null)
  const [profile, setProfileState] = useState<UserProfile | null>(null)

  useEffect(() => {
    let current = true
    const unsubscribe = onAuthStateChanged(auth, async (firebaseUser) => {
      setUser(firebaseUser)
      if (!firebaseUser) {
        setProfileState(null)
        setStatus('signedOut')
        return
      }
      setStatus('signedIn')
      try {
        const loaded = await loadUserProfile(firebaseUser.uid)
        // A sign up in progress may not have saved the profile yet: keep the one it sets
        if (current && loaded && auth.currentUser?.uid === firebaseUser.uid)
          setProfileState(loaded)
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

  return { status, user, profile, setProfile }
}
