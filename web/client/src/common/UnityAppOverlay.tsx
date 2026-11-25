import { useEffect, useRef, useState } from 'react'
import { auth } from '../firebase'

// The full screen Unity WebGL builds that run the app as the signed in user (the editor, the minigames).
// Each opening loads Unity again. The build tells this page how it goes with
// postMessage({ source, event: 'opened' | 'closed' | 'error' })

type Status = 'checking' | 'loading' | 'ready' | 'error' | 'missing' | 'signedOut'

// The user signed in here, given to Unity (FirebaseRestHostSession reads it from this window)
declare global {
  interface Window {
    fabulabHostSession?: { refreshToken: string; uid: string; email: string | null }
  }
}

interface Props {
  // Page of the Unity build, with its parameters. null: closed
  url: string | null
  // The "source" of the messages of the build
  source: string
  label: string
  texts: { missing: string; error: string; signedOut: string }
  // The build was left
  onClose: () => void
  // full: the whole window, left with the back button of the app once open (the editor).
  // player: like the story player, a square frame with a close button always shown (the minigames)
  layout?: 'full' | 'player'
}

export function UnityAppOverlay({ url, source, label, texts, onClose, layout = 'full' }: Props) {
  const isPlayer = layout === 'player'
  const iframe = useRef<HTMLIFrameElement>(null)
  const closeButton = useRef<HTMLButtonElement>(null)
  const [src, setSrc] = useState<string | null>(null)
  const [status, setStatus] = useState<Status>('checking')
  // What the loading is doing now (ex: "Descargando… 4.2 de 18 MB"), told by the build
  const [step, setStep] = useState<string | null>(null)
  // The app already told its own step: the generic ones of the template must not replace it
  const appStepShown = useRef(false)
  const open = url !== null

  useEffect(() => {
    if (!url) {
      setSrc(null)
      delete window.fabulabHostSession
      return
    }
    const user = auth.currentUser
    // Guests (anonymous users) are asked to sign in before opening it
    if (!user || user.isAnonymous) {
      setStatus('signedOut')
      return
    }
    let current = true
    setStatus('checking')
    setStep(null)
    appStepShown.current = false
    isUnityBuild(url).then((built) => {
      if (!current) return
      if (!built) return setStatus('missing')
      window.fabulabHostSession = { refreshToken: user.refreshToken, uid: user.uid, email: user.email }
      setStatus('loading')
      setSrc(url)
    })
    return () => {
      current = false
    }
  }, [url])

  useEffect(() => {
    const origin = url ? new URL(url, window.location.href).origin : null
    function onMessage(e: MessageEvent) {
      if (e.origin !== origin || e.source !== iframe.current?.contentWindow) return
      const data = e.data as { source?: string; event?: string; payload?: LoadingPayload }
      if (!data) return
      // The template of the build (WebGLTemplates/FabulabPlayer) reports its own loading as the story player does
      if (data.source === 'fabulab-story-player') {
        switch (data.event) {
          case 'progress':
            if (data.payload && !appStepShown.current) setStep(describeProgress(data.payload))
            break
          case 'ready':
            // Unity started: now the app loads the data of the user
            if (!appStepShown.current) setStep('Abriendo…')
            break
          case 'error':
            console.error(`${label} error`, data.payload?.message)
            setStatus('error')
            break
        }
        return
      }
      if (data.source !== source) return
      switch (data.event) {
        case 'loading':
          if (data.payload?.message) {
            appStepShown.current = true
            setStep(data.payload.message)
          }
          break
        case 'opened':
          setStatus('ready')
          break
        case 'closed':
          onClose()
          break
        case 'error':
          console.error(`${label} error`, data.payload?.message)
          setStatus('error')
          break
      }
    }
    window.addEventListener('message', onMessage)
    return () => window.removeEventListener('message', onMessage)
  }, [url, source, label, onClose])

  useEffect(() => {
    if (!open) return
    const overflow = document.body.style.overflow
    document.body.style.overflow = 'hidden'
    // As the story player: Escape closes it (while the page has the focus, not the game)
    function onKey(e: KeyboardEvent) {
      if (e.key === 'Escape') onClose()
    }
    if (isPlayer) {
      closeButton.current?.focus()
      document.addEventListener('keydown', onKey)
    }
    return () => {
      document.body.style.overflow = overflow
      document.removeEventListener('keydown', onKey)
    }
  }, [open, isPlayer, onClose])

  if (!open) return null

  return (
    <div className={isPlayer ? 'player-overlay is-open' : 'editor-overlay'} role="dialog" aria-modal="true" aria-label={label}>
      {src && (
        <iframe
          ref={iframe}
          className={isPlayer ? 'player-frame' : 'editor-frame'}
          src={src}
          title={label}
          allow="autoplay; fullscreen"
        />
      )}

      {status !== 'ready' && (
        <div className="player-status">
          {status === 'missing' && <p>{texts.missing}</p>}
          {status === 'error' && <p>{texts.error}</p>}
          {status === 'signedOut' && <p>{texts.signedOut}</p>}
          {(status === 'checking' || status === 'loading') && (
            <div className="loading-steps">
              <div className="spinner" aria-label="Cargando" />
              {step && (
                <p className="loading-step" aria-live="polite">
                  {step}
                </p>
              )}
            </div>
          )}
        </div>
      )}

      {/* full: once open, it is left with the back button of the app (it asks before losing changes) */}
      {(isPlayer || status !== 'ready') && (
        <button ref={closeButton} type="button" className="player-close" onClick={onClose} aria-label="Cerrar">
          <svg viewBox="0 0 24 24" width="26" height="26" aria-hidden="true">
            <path d="M6 6l12 12M18 6L6 18" stroke="currentColor" strokeWidth="2.6" strokeLinecap="round" />
          </svg>
        </button>
      )}
    </div>
  )
}

// A step of the loading: the download and start of the build (from its template), or a message of the app
interface LoadingPayload {
  message?: string
  stage?: 'download' | 'starting'
  progress?: number
  loaded?: number
  total?: number
}

function megabytes(bytes: number) {
  return (bytes / (1024 * 1024)).toFixed(bytes < 10 * 1024 * 1024 ? 1 : 0)
}

function describeProgress({ stage, progress = 0, loaded = 0, total = 0 }: LoadingPayload): string {
  if (stage !== 'download') return 'Preparando…'
  if (total > 0) return `Descargando… ${megabytes(loaded)} de ${megabytes(total)} MB`
  return `Descargando… ${Math.floor((progress / 0.9) * 100)}%`
}

// The dev server answers any missing page with the site itself: check that it is the Unity build
async function isUnityBuild(url: string): Promise<boolean> {
  try {
    const response = await fetch(url, { cache: 'no-cache' })
    if (!response.ok) return false
    return (await response.text()).includes('createUnityInstance')
  } catch {
    return false
  }
}
