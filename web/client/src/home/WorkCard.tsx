import { useCallback } from 'react'
import { RemoteImage } from '../common/RemoteImage'
import { imageUrl, type ImageFolder } from '../data/images'

interface Props {
  id: string
  userID: string
  name: string
  // Without it, no likes are shown (works that have none, like the minigames)
  likes?: number
  isPublic?: boolean
  folder: ImageFolder
  // What the card does when pressed: play a story or open the editor. Without it, the card only shows
  action?: 'play' | 'edit'
  onOpen?: () => void
  // Show "Privado" on works that only the user sees
  showPrivacy?: boolean
  // A second action, as a button under the name (ex: edit a minigame)
  secondaryLabel?: string
  onSecondary?: () => void
}

// Card of a story, a character, an object or a minigame: its picture, likes and name
export function WorkCard({
  id,
  userID,
  name,
  likes,
  isPublic,
  folder,
  action,
  onOpen,
  showPrivacy,
  secondaryLabel,
  onSecondary,
}: Props) {
  const loadThumb = useCallback(() => imageUrl(userID, folder, id), [userID, folder, id])
  const title = name.trim() || (folder === 'stories' ? 'Sin título' : 'Sin nombre')

  const content = (
    <>
      <RemoteImage load={loadThumb} alt={title} />
      {likes !== undefined && (
        <span className="story-likes" aria-label={`${likes} me gusta`}>
          <HeartIcon />
          {likes}
        </span>
      )}
      {showPrivacy && !isPublic && <span className="work-private">Privado</span>}
      {action && (
        <span className="story-play" aria-hidden="true">
          {action === 'play' ? <PlayIcon /> : <EditIcon />}
        </span>
      )}
    </>
  )

  return (
    <article className="story-card">
      {onOpen ? (
        <button
          type="button"
          className="story-thumb"
          onClick={onOpen}
          aria-label={`${action === 'edit' ? 'Editar' : 'Ver'} ${title}`}
        >
          {content}
        </button>
      ) : (
        <div className="story-thumb">{content}</div>
      )}
      <h3 className="story-title">{title}</h3>
      {onSecondary && secondaryLabel && (
        <button type="button" className="card-secondary" onClick={onSecondary}>
          {secondaryLabel}
        </button>
      )}
    </article>
  )
}

interface CreateCardProps {
  label: string
  onCreate: () => void
  // plus: create a new work (default). play: open something to play
  icon?: 'plus' | 'play'
}

// First card of the user's works: creates a new one. Also a card that only opens something
export function CreateCard({ label, onCreate, icon = 'plus' }: CreateCardProps) {
  return (
    <article className="story-card">
      <button type="button" className="story-thumb create-thumb" onClick={onCreate}>
        <span className="create-plus" aria-hidden="true">
          {icon === 'play' ? (
            <svg viewBox="0 0 24 24" width="40" height="40">
              <path fill="currentColor" d="M8 5.5v13l11-6.5z" />
            </svg>
          ) : (
            <svg viewBox="0 0 24 24" width="40" height="40">
              <path d="M12 5v14M5 12h14" stroke="currentColor" strokeWidth="2.6" strokeLinecap="round" />
            </svg>
          )}
        </span>
        <span className="create-label">{label}</span>
      </button>
    </article>
  )
}

function HeartIcon() {
  return (
    <svg viewBox="0 0 24 24" width="16" height="16" aria-hidden="true">
      <path
        fill="currentColor"
        d="M12 21s-7.5-4.6-9.6-9.2C.9 8.4 3 4.5 6.7 4.5c2.1 0 3.6 1.1 5.3 3 1.7-1.9 3.2-3 5.3-3 3.7 0 5.8 3.9 4.3 7.3C19.5 16.4 12 21 12 21z"
      />
    </svg>
  )
}

function PlayIcon() {
  return (
    <svg viewBox="0 0 24 24" width="30" height="30">
      <path fill="currentColor" d="M8 5.5v13l11-6.5z" />
    </svg>
  )
}

function EditIcon() {
  return (
    <svg viewBox="0 0 24 24" width="26" height="26">
      <path
        fill="currentColor"
        d="M4 17.3V20h2.7l8.6-8.6-2.7-2.7L4 17.3zm15.7-9.4a1 1 0 0 0 0-1.4l-2.2-2.2a1 1 0 0 0-1.4 0l-1.6 1.6 2.7 2.7 2.5-2.7z"
      />
    </svg>
  )
}
