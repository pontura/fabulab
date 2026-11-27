import { useCallback, useState } from 'react'
import { RemoteImage } from '../common/RemoteImage'
import { UserAvatar, useUsername } from '../common/UserAvatar'
import { lastCreator } from '../data/creators'
import { imageUrl, type ImageFolder } from '../data/images'
import type { EditorRequest } from '../editor/EditorOverlay'
import { CreatorsDialog } from './CreatorsDialog'
import { useHomeActions } from './HomeActions'
import { isLikeKind, LikeButton } from './LikeButton'

interface Props {
  id: string
  userID: string
  name: string
  // The users that made it: the last one is shown on the card. Without them, its author
  creators?: string[]
  // When it was modified last (shown in the list of its creators)
  timestamp?: string
  // Objects only: SObjectData.types (1: background)
  type?: number
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

// Card of a story, a character, an object or a minigame: its picture and name. On the picture: top right, the one
// that modified it last; bottom right, its likes; top left, "Privado"
export function WorkCard({
  id,
  userID,
  name,
  creators,
  timestamp,
  type,
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
  const [showCreators, setShowCreators] = useState(false)
  const { uid, openEditor } = useHomeActions()
  // A work of another user can be modified: the editor saves it as a new one of the user
  const modifyRequest = userID !== uid ? editorRequestFor(folder, id, type) : null
  // Works saved before there were creators: their author
  const allCreators = creators && creators.length > 0 ? creators : userID ? [userID] : []

  const content = (
    <>
      <RemoteImage load={loadThumb} alt={title} />
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
      {/* The picture and, over it, the one that modified it last: a button of its own (not inside the one of the
          picture), that shows everyone that took part */}
      <div className="story-thumb-frame">
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
      {likes !== undefined && isLikeKind(folder) && <LikeButton kind={folder} workId={id} likes={likes} />}
      {allCreators.length > 0 && (
        <CreatorButton uid={lastCreator(allCreators, userID)} onClick={() => setShowCreators(true)} />
      )}
      </div>
      <h3 className="story-title">{title}</h3>
      {onSecondary && secondaryLabel && (
        <button type="button" className="card-secondary" onClick={onSecondary}>
          {secondaryLabel}
        </button>
      )}
      {showCreators && (
        <CreatorsDialog
          title={title}
          creators={allCreators}
          timestamp={timestamp}
          onModify={
            modifyRequest
              ? () => {
                  setShowCreators(false)
                  openEditor(modifyRequest)
                }
              : undefined
          }
          onClose={() => setShowCreators(false)}
        />
      )}
    </article>
  )
}

// How the editor opens a work of this folder
function editorRequestFor(folder: ImageFolder, id: string, type?: number): EditorRequest | null {
  switch (folder) {
    case 'stories':
      return { type: 'story', id }
    case 'characters':
      return { type: 'character', id }
    case 'so':
      return { type: type === 1 ? 'background' : 'object', id }
    case 'minigames':
      return { type: 'minigame', id }
    default:
      return null
  }
}

// The one that modified the work last, with its name on hover: shows everyone that took part
function CreatorButton({ uid, onClick }: { uid: string; onClick: () => void }) {
  const name = useUsername(uid)
  return (
    <button
      type="button"
      className="work-creator"
      onClick={onClick}
      title={name || undefined}
      aria-label={name ? `Hecho por ${name}: ver quiénes participaron` : 'Ver quiénes participaron'}
    >
      <UserAvatar uid={uid} name={name} />
    </button>
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
