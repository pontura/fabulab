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
}

export function UnityAppOverlay({ url, source, label, texts, onClose }: Props) {
  const iframe = useRef<HTMLIFrameElement>(null)
  const [src, setSrc] = useState<string | null>(null)
  const [status, setStatus] = useState<Status>('checking')
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
      const data = e.data as { source?: string; event?: string; payload?: { message?: string } }
      if (!data) return
      // The template of the build reports its own loading errors as the story player does
      if (data.source === 'fabulab-story-player' && data.event === 'error') {
        console.error(`${label} error`, data.payload?.message)
        setStatus('error')
        return
      }
      if (data.source !== source) return
      switch (data.event) {
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
    return () => {
      document.body.style.overflow = overflow
    }
  }, [open])

  if (!open) return null

  return (
    <div className="editor-overlay" role="dialog" aria-modal="true" aria-label={label}>
      {src && <iframe ref={iframe} className="editor-frame" src={src} title={label} allow="autoplay; fullscreen" />}

      {status !== 'ready' && (
        <div className="player-status">
          {status === 'missing' && <p>{texts.missing}</p>}
          {status === 'error' && <p>{texts.error}</p>}
          {status === 'signedOut' && <p>{texts.signedOut}</p>}
          {(status === 'checking' || status === 'loading') && <div className="spinner" aria-label="Cargando" />}
        </div>
      )}

      {/* Once open, it is left with the back button of the app (it asks before losing changes) */}
      {status !== 'ready' && (
        <button type="button" className="player-close" onClick={onClose} aria-label="Cerrar">
          <svg viewBox="0 0 24 24" width="26" height="26" aria-hidden="true">
            <path d="M6 6l12 12M18 6L6 18" stroke="currentColor" strokeWidth="2.6" strokeLinecap="round" />
          </svg>
        </button>
      )}
    </div>
  )
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
