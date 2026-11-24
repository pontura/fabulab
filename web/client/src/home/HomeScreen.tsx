import { asset } from '../common/asset'
import { useCallback, useEffect, useMemo, useRef, useState } from 'react'
import { AuthScreen } from '../auth/AuthScreen'
import { signOut } from '../auth/authService'
import type { Session } from '../auth/useSession'
import { RemoteImage } from '../common/RemoteImage'
import { EditorOverlay, type EditorRequest } from '../editor/EditorOverlay'
import { imageUrl, invalidateImages } from '../data/images'
import { invalidateStories, loadFeaturedStories, loadUserStories, type StoryMeta } from '../data/stories'
import { invalidateWorks, loadPublicWorks, loadUserWorks, ObjectType, type WorkMeta } from '../data/works'
import { MiniGamesOverlay } from '../minigames/MiniGamesOverlay'
import { StoryPlayerOverlay } from '../player/StoryPlayerOverlay'
import { useStoryUrl } from '../player/useStoryUrl'
import { CardSection } from './CardSection'
import { CreateCard, WorkCard } from './WorkCard'

type Tab = 'home' | 'stories' | 'characters' | 'objects' | 'games'

const tabs: { id: Tab; label: string }[] = [
  { id: 'home', label: 'Home' },
  { id: 'stories', label: 'Historias' },
  { id: 'characters', label: 'Personajes' },
  { id: 'objects', label: 'Objetos' },
  { id: 'games', label: 'Juegos' },
]

// Stable loaders: CardSection loads again when its loader changes
const loadCharacters = () => loadPublicWorks('characters')
const loadObjects = () => loadPublicWorks('so').then((all) => all.filter((o) => o.type === ObjectType.generic))
const loadBackgrounds = () =>
  loadPublicWorks('so').then((all) => all.filter((o) => o.type === ObjectType.background))
const itemKey = (item: StoryMeta | WorkMeta) => item.id

// Why a guest must sign in, by what it tried to do
const signInReasons: Record<EditorRequest['type'], string> = {
  story: 'Ingresá o registrate para crear tus historias',
  character: 'Ingresá o registrate para crear tus personajes',
  object: 'Ingresá o registrate para crear tus objetos',
  background: 'Ingresá o registrate para crear tus fondos',
}

// The sign in panel: open, with what to do once signed in
interface SignInPrompt {
  reason?: string
  then?: EditorRequest | 'minigames'
}

interface Props {
  session: Session
}

export function HomeScreen({ session }: Props) {
  const { user, isGuest, profile, setProfile } = session
  // Guests have no works of their own: their "Mis ..." sections are empty
  const uid = isGuest || !user ? null : user.uid
  const [tab, setTab] = useState<Tab>('home')
  const { storyId: openStoryId, openStory, closeStory: closePlayer } = useStoryUrl()
  const [editorRequest, setEditorRequest] = useState<EditorRequest | null>(null)
  const [signInPrompt, setSignInPrompt] = useState<SignInPrompt | null>(null)
  const [miniGamesOpen, setMiniGamesOpen] = useState(false)
  // Changes when the editor closes or the user changes: the sections load again
  const [dataVersion, setDataVersion] = useState(0)
  const name = profile?.username || user?.email || ''

  // Creating or editing needs an account: guests are asked to sign in first
  const openEditor = useCallback(
    (request: EditorRequest) => {
      if (isGuest) setSignInPrompt({ reason: signInReasons[request.type], then: request })
      else setEditorRequest(request)
    },
    [isGuest],
  )

  // The minigames play with the user's characters and stories: guests are asked to sign in first
  const openMiniGames = useCallback(() => {
    if (isGuest) setSignInPrompt({ reason: 'Ingresá o registrate para jugar con tus personajes', then: 'minigames' })
    else setMiniGamesOpen(true)
  }, [isGuest])
  const closeMiniGames = useCallback(() => setMiniGamesOpen(false), [])

  // Once signed in, the panel closes and what the guest tried to do goes on
  useEffect(() => {
    if (isGuest || !signInPrompt) return
    setSignInPrompt(null)
    if (signInPrompt.then === 'minigames') setMiniGamesOpen(true)
    else if (signInPrompt.then) setEditorRequest(signInPrompt.then)
  }, [isGuest, signInPrompt])

  // Another user (signed in or out): their own works must load again
  useEffect(() => {
    setDataVersion((v) => v + 1)
  }, [uid])

  const closeEditor = useCallback(() => {
    setEditorRequest(null)
    invalidateStories()
    invalidateWorks()
    invalidateImages()
    setDataVersion((v) => v + 1)
  }, [])

  const userLoaders = useMemo(() => {
    if (!uid) {
      const none = () => Promise.resolve([])
      return { stories: none, characters: none, objects: none, backgrounds: none }
    }
    return {
      stories: () => loadUserStories(uid),
      characters: () => loadUserWorks('characters', uid),
      objects: () => loadUserWorks('so', uid).then((all) => all.filter((o) => o.type === ObjectType.generic)),
      backgrounds: () => loadUserWorks('so', uid).then((all) => all.filter((o) => o.type === ObjectType.background)),
    }
  }, [uid])

  const playStory = (story: StoryMeta) => (
    <WorkCard {...story} folder="stories" action="play" onOpen={() => openStory(story.id)} />
  )

  return (
    <div className="app">
      <header className="app-header">
        <img className="header-logo" src={asset('images/logo.png')} alt="Fabulab" />

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

        {uid ? (
          <UserMenu uid={uid} name={name} />
        ) : (
          <button type="button" className="button button-primary header-login" onClick={() => setSignInPrompt({})}>
            Ingresar
          </button>
        )}
      </header>

      <main className="app-content" key={dataVersion}>
        {tab === 'home' && (
          <CardSection
            title="Historias destacadas"
            load={loadFeaturedStories}
            getKey={itemKey}
            renderCard={playStory}
            emptyMessage="Todavía no hay historias publicadas."
          />
        )}

        {tab === 'stories' && (
          <>
            <CardSection
              title="Mis historias"
              load={userLoaders.stories}
              getKey={itemKey}
              firstCard={<CreateCard label="Crear historia" onCreate={() => openEditor({ type: 'story' })} />}
              renderCard={(story) => (
                <WorkCard
                  {...story}
                  folder="stories"
                  action="edit"
                  showPrivacy
                  onOpen={() => openEditor({ type: 'story', id: story.id })}
                />
              )}
              emptyMessage=""
            />
            <CardSection
              title="Historias"
              load={loadFeaturedStories}
              getKey={itemKey}
              renderCard={playStory}
              emptyMessage="Todavía no hay historias publicadas."
            />
          </>
        )}

        {tab === 'characters' && (
          <>
            <CardSection
              title="Mis personajes"
              load={userLoaders.characters}
              getKey={itemKey}
              firstCard={
                <CreateCard label="Crear personaje" onCreate={() => openEditor({ type: 'character' })} />
              }
              renderCard={(c) => (
                <WorkCard
                  {...c}
                  folder="characters"
                  action="edit"
                  showPrivacy
                  onOpen={() => openEditor({ type: 'character', id: c.id })}
                />
              )}
              emptyMessage=""
            />
            <CardSection
              title="Personajes"
              load={loadCharacters}
              getKey={itemKey}
              renderCard={(c) => <WorkCard {...c} folder="characters" />}
              emptyMessage="Todavía no hay personajes publicados."
            />
          </>
        )}

        {tab === 'objects' && (
          <>
            <CardSection
              title="Mis objetos"
              load={userLoaders.objects}
              getKey={itemKey}
              firstCard={<CreateCard label="Crear objeto" onCreate={() => openEditor({ type: 'object' })} />}
              renderCard={(o) => (
                <WorkCard
                  {...o}
                  folder="so"
                  action="edit"
                  showPrivacy
                  onOpen={() => openEditor({ type: 'object', id: o.id })}
                />
              )}
              emptyMessage=""
            />
            <CardSection
              title="Mis fondos"
              load={userLoaders.backgrounds}
              getKey={itemKey}
              firstCard={
                <CreateCard label="Crear fondo" onCreate={() => openEditor({ type: 'background' })} />
              }
              renderCard={(o) => (
                <WorkCard
                  {...o}
                  folder="so"
                  action="edit"
                  showPrivacy
                  onOpen={() => openEditor({ type: 'background', id: o.id })}
                />
              )}
              emptyMessage=""
            />
            <CardSection
              title="Objetos"
              load={loadObjects}
              getKey={itemKey}
              renderCard={(o) => <WorkCard {...o} folder="so" />}
              emptyMessage="Todavía no hay objetos publicados."
            />
            <CardSection
              title="Fondos"
              load={loadBackgrounds}
              getKey={itemKey}
              renderCard={(o) => <WorkCard {...o} folder="so" />}
              emptyMessage="Todavía no hay fondos publicados."
            />
          </>
        )}

        {tab === 'games' && (
          <section className="section">
            <h2 className="section-title">Juegos</h2>
            <div className="card-grid">
              <div className="card-grid-cell">
                <CreateCard label="Jugar" icon="play" onCreate={openMiniGames} />
              </div>
            </div>
          </section>
        )}
      </main>

      <StoryPlayerOverlay storyId={openStoryId} onClose={closePlayer} />
      <EditorOverlay request={editorRequest} onClose={closeEditor} />
      <MiniGamesOverlay open={miniGamesOpen} onClose={closeMiniGames} />

      {signInPrompt && (
        <AuthScreen
          reason={signInPrompt.reason}
          onSignedUp={setProfile}
          onClose={() => setSignInPrompt(null)}
        />
      )}
    </div>
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
