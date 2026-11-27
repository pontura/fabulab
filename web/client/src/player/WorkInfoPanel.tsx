import { useEffect, useState } from 'react'
import { loadMiniGameCredits, loadStoryCredits, type Credit } from '../data/credits'
import type { LikeKind } from '../data/likes'
import { findMiniGame } from '../data/minigames'
import { findStory } from '../data/stories'
import type { EditorWorkType } from '../editor/EditorOverlay'
import { CreatorsList } from '../home/CreatorsList'
import { CreditsList } from '../home/CreditsList'
import { useHomeActions } from '../home/HomeActions'
import { LikeButton } from '../home/LikeButton'

// Same as the CSS of .story-info: the windows where the player leaves a column free at its left
const WIDE = '(min-aspect-ratio: 17/10)'

// What the panel shows of a work
interface PanelWork {
  id: string
  name: string
  userID: string
  creators: string[]
  timestamp: string
  // Without them, no likes (the minigames have none)
  likes?: number
}

// What is being played: a story (story player) or a minigame
const kinds = {
  story: {
    find: findStory as (id: string) => Promise<PanelWork | null>,
    credits: loadStoryCredits,
    likeKind: 'stories' as LikeKind | null,
    editor: 'story' as EditorWorkType,
    label: 'Sobre esta historia',
    untitled: 'Sin título',
    makers: 'Quiénes la hicieron',
    copyNote: 'Se guarda como una historia tuya, y quedás en la lista de quienes la hicieron.',
  },
  minigame: {
    find: findMiniGame as (id: string) => Promise<PanelWork | null>,
    credits: loadMiniGameCredits,
    likeKind: null,
    editor: 'minigame' as EditorWorkType,
    label: 'Sobre este juego',
    untitled: 'Sin nombre',
    makers: 'Quiénes lo hicieron',
    copyNote: 'Se guarda como un juego tuyo, y quedás en la lista de quienes lo hicieron.',
  },
}

interface Props {
  kind: keyof typeof kinds
  workId: string
  // The player is closed before the editor opens
  onCloseForEditor: () => void
}

// Shown next to the story or minigame being played: its name, its likes (and liking it), everyone that made it and,
// for a work of another user, "Modificar" (the editor saves it as a new work of the user), like the popup of the
// creators. Nothing while it loads, or when the work is not a saved one (ex: the original of a minigame mechanic)
export function WorkInfoPanel({ kind, workId, onCloseForEditor }: Props) {
  const config = kinds[kind]
  const [work, setWork] = useState<PanelWork | null>(null)
  // The works of other users it uses
  const [credits, setCredits] = useState<Credit[]>([])
  // Wide windows have room next to the player: everything is shown. Elsewhere the panel is over the work, so it
  // starts folded (name and likes) and opens on demand
  const [expanded, setExpanded] = useState(() => window.matchMedia(WIDE).matches)
  const { uid, openEditor } = useHomeActions()

  useEffect(() => {
    let current = true
    setWork(null)
    setCredits([])
    config
      .find(workId)
      .then((found) => {
        if (!current) return
        setWork(found)
        if (found)
          config
            .credits(found.userID, found.id)
            .then((list) => current && setCredits(list))
            .catch((e) => console.error('Could not load the credits', e))
      })
      .catch((e) => console.error('Could not load the work', e))
    return () => {
      current = false
    }
  }, [config, workId])

  if (!work) return null

  const title = work.name.trim() || config.untitled
  // Works saved before there were creators: their author
  const creators = work.creators.length > 0 ? work.creators : work.userID ? [work.userID] : []
  const isOthers = work.userID !== uid

  return (
    <aside className={`story-info ${expanded ? 'is-expanded' : ''}`} aria-label={config.label}>
      <div className="story-info-head">
        <h2 className="story-info-title">{title}</h2>
        {config.likeKind && work.likes !== undefined && (
          <LikeButton kind={config.likeKind} workId={work.id} likes={work.likes} className="story-info-likes" />
        )}
        <button
          type="button"
          className="story-info-toggle"
          onClick={() => setExpanded((v) => !v)}
          aria-expanded={expanded}
          aria-label={expanded ? `Ocultar ${config.makers.toLowerCase()}` : `Ver ${config.makers.toLowerCase()}`}
        >
          <svg viewBox="0 0 24 24" width="20" height="20" aria-hidden="true">
            <path d="M6 15l6-6 6 6" fill="none" stroke="currentColor" strokeWidth="2.6" strokeLinecap="round" strokeLinejoin="round" />
          </svg>
        </button>
      </div>

      {/* Folded: hidden by the CSS (always shown in wide windows) */}
      <div className="story-info-body">
        <h3 className="story-info-subtitle">{config.makers}</h3>
        <CreatorsList creators={creators} timestamp={work.timestamp} />

        {credits.length > 0 && (
          <>
            <h3 className="story-info-subtitle credits-title">Créditos</h3>
            <CreditsList credits={credits} />
          </>
        )}

        {isOthers && (
          <>
            <button
              type="button"
              className="button button-primary creators-modify"
              onClick={() => {
                onCloseForEditor()
                openEditor({ type: config.editor, id: work.id })
              }}
            >
              Modificar
            </button>
            <p className="creators-modify-note">{config.copyNote}</p>
          </>
        )}
      </div>
    </aside>
  )
}
