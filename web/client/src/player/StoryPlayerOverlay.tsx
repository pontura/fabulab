import { useEffect, useRef, useState } from 'react'

// The Unity WebGL build of the scene StoryPlayer (template Assets/WebGLTemplates/FabulabPlayer).
// By default it is served by this site from public/player
const PLAYER_URL = import.meta.env.VITE_STORY_PLAYER_URL || '/player/index.html'
const playerOrigin = new URL(PLAYER_URL, window.location.href).origin

type Status = 'checking' | 'starting' | 'loading' | 'ready' | 'error' | 'missing'

interface PlayerMessage {
  source: 'fabulab-story-player'
  event: string
  payload?: { message?: string }
}

interface Props {
  // The story to show. null: the player is closed (it stays loaded, hidden and paused)
  storyId: string | null
  onClose: () => void
}

export function StoryPlayerOverlay({ storyId, onClose }: Props) {
  const iframe = useRef<HTMLIFrameElement>(null)
  const [src, setSrc] = useState<string | null>(null)
  const [status, setStatus] = useState<Status>('checking')
  const closeButton = useRef<HTMLButtonElement>(null)
  const open = storyId !== null

  // First story: the player starts with it in its URL. Next ones: it is told to load them
  useEffect(() => {
    if (!storyId) {
      send(iframe.current, { action: 'pause' })
      return
    }
    if (src) {
      setStatus('loading')
      send(iframe.current, { action: 'load', id: storyId })
      return
    }
    let current = true
    setStatus('checking')
    isPlayerBuilt().then((built) => {
      if (!current) return
      if (!built) return setStatus('missing')
      setStatus('starting')
      setSrc(`${PLAYER_URL}?id=${encodeURIComponent(storyId)}`)
    })
    return () => {
      current = false
    }
    // Only storyId: the src is set once, changing it would reload Unity
  }, [storyId])

  useEffect(() => {
    function onMessage(e: MessageEvent) {
      if (e.origin !== playerOrigin || e.source !== iframe.current?.contentWindow) return
      const data = e.data as PlayerMessage
      if (!data || data.source !== 'fabulab-story-player') return
      switch (data.event) {
        case 'loading':
          setStatus('loading')
          break
        case 'loaded':
        case 'playing':
          setStatus('ready')
          break
        case 'error':
          console.error('Story player error', data.payload?.message)
          setStatus('error')
          break
      }
    }
    window.addEventListener('message', onMessage)
    return () => window.removeEventListener('message', onMessage)
  }, [])

  useEffect(() => {
    if (!open) return
    closeButton.current?.focus()
    function onKey(e: KeyboardEvent) {
      if (e.key === 'Escape') onClose()
    }
    document.addEventListener('keydown', onKey)
    const overflow = document.body.style.overflow
    document.body.style.overflow = 'hidden'
    return () => {
      document.removeEventListener('keydown', onKey)
      document.body.style.overflow = overflow
    }
  }, [open, onClose])

  return (
    <div
      className={`player-overlay ${open ? 'is-open' : ''}`}
      role="dialog"
      aria-modal="true"
      aria-label="Reproductor de historias"
      aria-hidden={!open}
    >
      {src && <iframe ref={iframe} className="player-frame" src={src} title="Reproductor de historias" allow="autoplay; fullscreen" />}

      {open && status !== 'ready' && (
        <div className="player-status">
          {status === 'missing' && <p>El reproductor todavía no está publicado.</p>}
          {status === 'error' && <p>No pudimos abrir esta historia.</p>}
          {(status === 'checking' || status === 'starting' || status === 'loading') && (
            <div className="spinner" aria-label="Cargando" />
          )}
        </div>
      )}

      <button ref={closeButton} type="button" className="player-close" onClick={onClose} aria-label="Cerrar">
        <svg viewBox="0 0 24 24" width="26" height="26" aria-hidden="true">
          <path d="M6 6l12 12M18 6L6 18" stroke="currentColor" strokeWidth="2.6" strokeLinecap="round" />
        </svg>
      </button>
    </div>
  )
}

function send(frame: HTMLIFrameElement | null, command: { action: 'load' | 'play' | 'pause' | 'restart'; id?: string }) {
  frame?.contentWindow?.postMessage({ source: 'fabulab-web', ...command }, playerOrigin)
}

// The dev server answers any missing page with the site itself: check that it is the Unity player
async function isPlayerBuilt(): Promise<boolean> {
  try {
    const response = await fetch(PLAYER_URL, { cache: 'no-cache' })
    if (!response.ok) return false
    return (await response.text()).includes('createUnityInstance')
  } catch {
    return false
  }
}
