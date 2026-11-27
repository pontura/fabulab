import { createContext, useCallback, useContext, useEffect, useMemo, useRef, useState, type ReactNode } from 'react'
import { loadLikedIds, setLike, type LikeKind } from '../data/likes'

interface Likes {
  isLiked: (workId: string) => boolean
  // Likes given or taken back in this visit, not yet in the loaded counts
  delta: (workId: string) => number
  toggle: (kind: LikeKind, workId: string) => void
}

const LikesContext = createContext<Likes>({
  isLiked: () => false,
  delta: () => 0,
  toggle: () => {},
})

export function useLikes(): Likes {
  return useContext(LikesContext)
}

interface Props {
  // The signed in user. null: a guest, that is asked to sign in to like
  uid: string | null
  onSignInNeeded: () => void
  // Changes when the works are loaded again: their counts already have the likes of this visit
  dataVersion: number
  children: ReactNode
}

// The likes of the user for every card: what it liked, and liking or taking a like back (once per work)
export function LikesProvider({ uid, onSignInNeeded, dataVersion, children }: Props) {
  const [liked, setLiked] = useState<Set<string>>(new Set())
  const [deltas, setDeltas] = useState<Map<string, number>>(new Map())
  // Works whose like is being saved: clicks on them wait, so a like is never given twice
  const saving = useRef(new Set<string>())

  useEffect(() => {
    setLiked(new Set())
    if (!uid) return
    let current = true
    loadLikedIds(uid)
      .then((ids) => current && setLiked(ids))
      .catch((e) => console.error('Could not load the likes', e))
    return () => {
      current = false
    }
  }, [uid])

  // The counts loaded again include them
  useEffect(() => {
    setDeltas(new Map())
  }, [dataVersion])

  const change = useCallback((workId: string, like: boolean) => {
    setLiked((set) => {
      const next = new Set(set)
      if (like) next.add(workId)
      else next.delete(workId)
      return next
    })
    setDeltas((map) => new Map(map).set(workId, (map.get(workId) ?? 0) + (like ? 1 : -1)))
  }, [])

  const toggle = useCallback(
    (kind: LikeKind, workId: string) => {
      if (!uid) {
        onSignInNeeded()
        return
      }
      if (saving.current.has(workId)) return
      saving.current.add(workId)
      const like = !liked.has(workId)
      change(workId, like) // shown at once; undone if it fails
      setLike(kind, workId, uid, like)
        .catch((e) => {
          console.error('Could not save the like', e)
          change(workId, !like)
        })
        .finally(() => saving.current.delete(workId))
    },
    [uid, liked, change, onSignInNeeded],
  )

  const value = useMemo<Likes>(
    () => ({
      isLiked: (workId) => liked.has(workId),
      delta: (workId) => deltas.get(workId) ?? 0,
      toggle,
    }),
    [liked, deltas, toggle],
  )

  return <LikesContext.Provider value={value}>{children}</LikesContext.Provider>
}
