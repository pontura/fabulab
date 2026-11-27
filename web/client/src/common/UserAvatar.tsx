import { useCallback, useEffect, useState } from 'react'
import { loadUsername } from '../data/creators'
import { imageUrl } from '../data/images'
import { RemoteImage } from './RemoteImage'

// The username of a user ('' while it loads or when it has none)
export function useUsername(uid: string): string {
  const [name, setName] = useState('')
  useEffect(() => {
    let current = true
    setName('')
    if (uid) loadUsername(uid).then((n) => current && setName(n))
    return () => {
      current = false
    }
  }, [uid])
  return name
}

interface Props {
  uid: string
  // Its name, when the caller already has it (otherwise it is loaded, for the initial)
  name?: string
  className?: string
}

// The profile picture of a user: its initial while it loads or when it has none
export function UserAvatar({ uid, name, className }: Props) {
  const loaded = useUsername(name === undefined ? uid : '')
  const shown = name ?? loaded
  const loadPicture = useCallback(() => imageUrl(uid, 'profile', uid), [uid])
  return (
    <span className={`user-avatar ${className ?? ''}`}>
      <span className="user-avatar-initial" aria-hidden="true">
        {shown.charAt(0).toUpperCase()}
      </span>
      <RemoteImage load={loadPicture} alt="" />
    </span>
  )
}
