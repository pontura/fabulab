import { asset } from '../common/asset'
import { useCallback, useEffect, useMemo, useRef, useState } from 'react'
import { AuthScreen } from '../auth/AuthScreen'
import { signOut } from '../auth/authService'
import type { Session } from '../auth/useSession'
import { RemoteImage } from '../common/RemoteImage'
import { EditorOverlay, type EditorRequest, type EditorWorkType } from '../editor/EditorOverlay'
import { imageUrl, invalidateImages } from '../data/images'
import { invalidateStories, loadFeaturedStories, loadUserStories, type StoryMeta } from '../data/stories'
import { invalidateWorks, loadPublicWorks, loadUserWorks, ObjectType, type WorkMeta } from '../data/works'
import {
  invalidateMiniGames,
  loadCommunityMiniGames,
  loadOfficialMiniGames,
  loadUserMiniGames,
  type MiniGameMeta,
} from '../data/minigames'
import { useUrlParam } from '../common/useUrlParam'
import { MiniGamesOverlay } from '../minigames/MiniGamesOverlay'
import { StoryPlayerOverlay } from '../player/StoryPlayerOverlay'
import { useStoryUrl } from '../player/useStoryUrl'
import { CardSection } from './CardSection'
import type { ManagedKind } from '../data/workActions'
import { CreateCard, WorkCard } from './WorkCard'
import { WorkInfoDialog, type ManagedWork } from './WorkInfoDialog'

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
const itemKey = (item: StoryMeta | WorkMeta | MiniGameMeta) => item.id

// Why a guest must sign in, by what it tried to do
const signInReasons: Record<EditorRequest['type'], string> = {
  story: 'Ingresá o registrate para crear tus historias',
  character: 'Ingresá o registrate para crear tus personajes',
  object: 'Ingresá o registrate para crear tus objetos',
  background: 'Ingresá o registrate para crear tus fondos',
  minigame: 'Ingresá o registrate para hacer tus juegos',
}

// The popup of a work of the user: what it is, how the editor opens it and the work
interface ManagedRequest {
  kind: ManagedKind
  editorType: EditorWorkType
  kindName?: string
  work: ManagedWork
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
  const [managed, setManaged] = useState<ManagedRequest | null>(null)
  // The game being played lives in the URL (?minigame=<id>), so it can be shared by copying the link
  const { value: miniGameId, open: openMiniGame, close: closeMiniGames } = useUrlParam('minigame')
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

  // The minigames play with the user's characters and stories: a guest with a game in the URL (from the card or a
  // shared link) is asked to sign in first. Once signed in the game opens, since it is still in the URL
  useEffect(() => {
    if (miniGameId && isGuest && !signInPrompt)
      setSignInPrompt({ reason: 'Ingresá o registrate para jugar con tus personajes', then: 'minigames' })
  }, [miniGameId, isGuest, signInPrompt])

  // Once signed in, the panel closes and what the guest tried to do goes on
  useEffect(() => {
    if (isGuest || !signInPrompt) return
    setSignInPrompt(null)
    if (signInPrompt.then && signInPrompt.then !== 'minigames') setEditorRequest(signInPrompt.then)
  }, [isGuest, signInPrompt])

  // Closing the sign in panel without signing in: the game it was asked for is not played
  const closeSignIn = useCallback(() => {
    if (signInPrompt?.then === 'minigames') closeMiniGames()
    setSignInPrompt(null)
  }, [signInPrompt, closeMiniGames])

  // Another user (signed in or out): their own works must load again
  useEffect(() => {
    setDataVersion((v) => v + 1)
  }, [uid])

  // The works may have changed: the sections load again
  const reloadData = useCallback(() => {
    invalidateStories()
    invalidateWorks()
    invalidateMiniGames()
    invalidateImages()
    setDataVersion((v) => v + 1)
  }, [])

  const closeEditor = useCallback(() => {
    setEditorRequest(null)
    reloadData()
  }, [reloadData])

  // A work of the user: its popup, to change its info, edit, duplicate or delete it
  const manageWork = (kind: ManagedKind, editorType: EditorWorkType, work: ManagedWork, kindName?: string) =>
    setManaged({ kind, editorType, kindName, work })

  const userLoaders = useMemo(() => {
    if (!uid) {
      const none = () => Promise.resolve([])
      return { stories: none, characters: none, objects: none, backgrounds: none, minigames: none }
    }
    return {
      stories: () => loadUserStories(uid),
      minigames: () => loadUserMiniGames(uid),
      characters: () => loadUserWorks('characters', uid),
      objects: () => loadUserWorks('so', uid).then((all) => all.filter((o) => o.type === ObjectType.generic)),
      backgrounds: () => loadUserWorks('so', uid).then((all) => all.filter((o) => o.type === ObjectType.background)),
    }
  }, [uid])

  const renderPublicMiniGame = (game: MiniGameMeta) => (
    <WorkCard
      {...game}
      folder="minigames"
      action="play"
      onOpen={() => openMiniGame(game.id)}
      // Saving a minigame of another user makes a copy in the account of the user
      secondaryLabel={game.userID === uid ? 'Editar' : 'Hacer mi versión'}
      onSecondary={() => openEditor({ type: 'minigame', id: game.id })}
    />
  )

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
          <>
            <CardSection
              title="Juegos de Fabulab"
              load={loadOfficialMiniGames}
              getKey={itemKey}
              renderCard={renderPublicMiniGame}
              emptyMessage="Todavía no hay juegos publicados."
            />
            <CardSection
              title="Juegos de la comunidad"
              load={loadCommunityMiniGames}
              getKey={itemKey}
              renderCard={renderPublicMiniGame}
              emptyMessage="Todavía nadie publicó su versión de un juego."
            />
            <CardSection
              title="Historias destacadas"
              load={loadFeaturedStories}
              getKey={itemKey}
              renderCard={playStory}
              emptyMessage="Todavía no hay historias publicadas."
            />
          </>
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
                  onOpen={() => manageWork('stories', 'story', story)}
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
                  onOpen={() => manageWork('characters', 'character', c)}
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
                  onOpen={() => manageWork('so', 'object', o)}
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
                  onOpen={() => manageWork('so', 'background', o, 'fondo')}
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
          <>
            <CardSection
              title="Mis juegos"
              load={userLoaders.minigames}
              getKey={itemKey}
              renderCard={(game) => (
                <WorkCard
                  {...game}
                  folder="minigames"
                  action="play"
                  showPrivacy
                  onOpen={() => openMiniGame(game.id)}
                  secondaryLabel="Editar"
                  onSecondary={() => openEditor({ type: 'minigame', id: game.id })}
                />
              )}
              emptyMessage="Todavía no hiciste juegos. Elegí uno de abajo y hacé tu versión."
            />
            <CardSection
              title="Juegos de Fabulab"
              load={loadOfficialMiniGames}
              getKey={itemKey}
              renderCard={renderPublicMiniGame}
              emptyMessage="Todavía no hay juegos publicados."
            />
            <CardSection
              title="Juegos de la comunidad"
              load={loadCommunityMiniGames}
              getKey={itemKey}
              renderCard={renderPublicMiniGame}
              emptyMessage="Todavía nadie publicó su versión de un juego."
            />
          </>
        )}
      </main>

      {managed && (
        <WorkInfoDialog
          kind={managed.kind}
          kindName={managed.kindName}
          work={managed.work}
          onEdit={() => {
            setManaged(null)
            openEditor({ type: managed.editorType, id: managed.work.id })
          }}
          onDuplicated={(copy) => setManaged({ ...managed, work: copy })}
          onChanged={reloadData}
          onClose={() => setManaged(null)}
        />
      )}

      <StoryPlayerOverlay storyId={openStoryId} onClose={closePlayer} />
      <EditorOverlay request={editorRequest} onClose={closeEditor} />
      <MiniGamesOverlay gameId={isGuest ? null : miniGameId} onClose={closeMiniGames} />

      {signInPrompt && (
        <AuthScreen
          reason={signInPrompt.reason}
          onSignedUp={setProfile}
          onClose={closeSignIn}
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
