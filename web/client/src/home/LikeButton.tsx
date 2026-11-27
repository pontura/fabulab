import type { ImageFolder } from '../data/images'
import type { LikeKind } from '../data/likes'
import { useLikes } from './LikesContext'

interface Props {
  kind: LikeKind
  workId: string
  // The likes it had when loaded (the ones given in this visit are added)
  likes: number
  // Default: story-likes, the one of the cards
  className?: string
}

export function isLikeKind(folder: ImageFolder): folder is LikeKind {
  return folder === 'stories' || folder === 'characters' || folder === 'so'
}

// The likes of a work: likes it, or takes the like back (once per user). On a card (bottom right of its picture) or
// in the story player
export function LikeButton({ kind, workId, likes, className }: Props) {
  const { isLiked, delta, toggle } = useLikes()
  const liked = isLiked(workId)
  const count = Math.max(0, likes + delta(workId))
  return (
    <button
      type="button"
      className={`${className ?? 'story-likes'} ${liked ? 'is-liked' : ''}`}
      onClick={() => toggle(kind, workId)}
      aria-pressed={liked}
      aria-label={`${count} me gusta. ${liked ? 'Quitar me gusta' : 'Me gusta'}`}
    >
      <HeartIcon filled={liked} />
      {count}
    </button>
  )
}

function HeartIcon({ filled }: { filled: boolean }) {
  return (
    <svg viewBox="0 0 24 24" width="16" height="16" aria-hidden="true">
      <path
        fill={filled ? 'currentColor' : 'none'}
        stroke="currentColor"
        strokeWidth="2"
        strokeLinejoin="round"
        d="M12 21s-7.5-4.6-9.6-9.2C.9 8.4 3 4.5 6.7 4.5c2.1 0 3.6 1.1 5.3 3 1.7-1.9 3.2-3 5.3-3 3.7 0 5.8 3.9 4.3 7.3C19.5 16.4 12 21 12 21z"
      />
    </svg>
  )
}
