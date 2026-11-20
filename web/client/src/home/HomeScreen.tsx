import { useCallback, useEffect, useRef, useState } from 'react'
import type { User } from 'firebase/auth'
import { signOut, type UserProfile } from '../auth/authService'
import { RemoteImage } from '../common/RemoteImage'
import { imageUrl } from '../data/images'
import { StoryPlayerOverlay } from '../player/StoryPlayerOverlay'
import { FeaturedStories } from './FeaturedStories'

type Tab = 'home' | 'stories' | 'characters' | 'drawings'

const tabs: { id: Tab; label: string }[] = [
  { id: 'home', label: 'Home' },
  { id: 'stories', label: 'Historias' },
  { id: 'characters', label: 'Personajes' },
  { id: 'drawings', label: 'Dibujos' },
]

interface Props {
  user: User
  profile: UserProfile | null
}

export function HomeScreen({ user, profile }: Props) {
  const [tab, setTab] = useState<Tab>('home')
  const [openStoryId, setOpenStoryId] = useState<string | null>(null)
  const closePlayer = useCallback(() => setOpenStoryId(null), [])
  const name = profile?.username || user.email || ''

  return (
    <div className="app">
      <header className="app-header">
        <img className="header-logo" src="/images/logo.png" alt="Fabulab" />

        <nav className="tabs" aria-label="Secciones">
          {tabs.map((t) => (
            <button
              key={t.id}
              type="button"
              className={`tab ${tab === t.id ? 'is-active' : ''}`}
              aria-current={tab === t.id ? 'page' : undefined}
              onClick={() => setTab(t.id)}
            >
              {t.label}
            </button>
          ))}
        </nav>

        <UserMenu uid={user.uid} name={name} />
      </header>

      <main className="app-content">
        {tab === 'home' ? (
          <FeaturedStories onOpenStory={(story) => setOpenStoryId(story.id)} />
        ) : (
          <ComingSoon label={tabs.find((t) => t.id === tab)!.label} />
        )}
      </main>

      <StoryPlayerOverlay storyId={openStoryId} onClose={closePlayer} />
    </div>
  )
}

function ComingSoon({ label }: { label: string }) {
  return (
    <section className="section">
      <h2 className="section-title">{label}</h2>
      <p className="section-message">Muy pronto.</p>
    </section>
  )
}

function UserMenu({ uid, name }: { uid: string; name: string }) {
  const [open, setOpen] = useState(false)
  const menu = useRef<HTMLDivElement>(null)
  const loadPicture = useCallback(() => imageUrl(uid, 'profile', uid), [uid])

  useEffect(() => {
    if (!open) return
    function close(e: MouseEvent) {
      if (menu.current && !menu.current.contains(e.target as Node)) setOpen(false)
    }
    function closeOnEscape(e: KeyboardEvent) {
      if (e.key === 'Escape') setOpen(false)
    }
    document.addEventListener('mousedown', close)
    document.addEventListener('keydown', closeOnEscape)
    return () => {
      document.removeEventListener('mousedown', close)
      document.removeEventListener('keydown', closeOnEscape)
    }
  }, [open])

  return (
    <div className="user-menu" ref={menu}>
      <button
        type="button"
        className="user-button"
        aria-haspopup="menu"
        aria-expanded={open}
        onClick={() => setOpen((v) => !v)}
      >
        <span className="user-name">{name}</span>
        <span className="avatar">
          <span className="avatar-initial" aria-hidden="true">{name.charAt(0).toUpperCase()}</span>
          <RemoteImage load={loadPicture} alt="" />
        </span>
      </button>
      {open && (
        <div className="user-dropdown" role="menu">
          <button type="button" role="menuitem" onClick={() => signOut()}>
            Cerrar sesión
          </button>
        </div>
      )}
    </div>
  )
}
