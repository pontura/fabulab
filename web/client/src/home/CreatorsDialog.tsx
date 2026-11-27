import { useEffect, useRef } from 'react'
import { CreatorsList } from './CreatorsList'

interface Props {
  // The title of the work
  title: string
  // The users that made it, in the order they last modified it (the last one modified it last)
  creators: string[]
  // When it was modified last (by the last creator). The others have no date saved
  timestamp?: string
  // Opens the work to modify it, saved as a new work of the user. Without it, no button (ex: an own work)
  onModify?: () => void
  onClose: () => void
}

// Everyone that took part in a work: picture, name and, for the last one, when it modified it
export function CreatorsDialog({ title, creators, timestamp, onModify, onClose }: Props) {
  const panel = useRef<HTMLDivElement>(null)

  useEffect(() => {
    panel.current?.focus()
    function onKey(e: KeyboardEvent) {
      if (e.key === 'Escape') onClose()
    }
    document.addEventListener('keydown', onKey)
    return () => document.removeEventListener('keydown', onKey)
  }, [onClose])

  return (
    <div className="work-dialog-backdrop" onMouseDown={(e) => e.target === e.currentTarget && onClose()}>
      <div
        ref={panel}
        className="work-dialog creators-dialog"
        role="dialog"
        aria-modal="true"
        aria-labelledby="creators-dialog-title"
        tabIndex={-1}
      >
        <button type="button" className="work-dialog-close" onClick={onClose} aria-label="Cerrar">
          <svg viewBox="0 0 24 24" width="22" height="22" aria-hidden="true">
            <path d="M6 6l12 12M18 6L6 18" stroke="currentColor" strokeWidth="2.6" strokeLinecap="round" />
          </svg>
        </button>

        <h2 id="creators-dialog-title" className="work-dialog-heading creators-dialog-title">
          Quiénes hicieron “{title}”
        </h2>

        <CreatorsList creators={creators} timestamp={timestamp} />

        {onModify && (
          <>
            <button type="button" className="button button-primary creators-modify" onClick={onModify}>
              Modificar
            </button>
            <p className="creators-modify-note">Se guarda como una obra tuya, y quedás en la lista de quienes la hicieron.</p>
          </>
        )}
      </div>
    </div>
  )
}
