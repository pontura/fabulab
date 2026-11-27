import { asset } from '../common/asset'
import { UnityAppOverlay } from '../common/UnityAppOverlay'
import { WorkInfoPanel } from '../player/WorkInfoPanel'

// The Unity WebGL build of the scene MiniGames. By default it is served by this site from public/minigames
const MINIGAMES_URL = import.meta.env.VITE_MINIGAMES_URL || asset('minigames/index.html')

const texts = {
  missing: 'Los juegos todavía no están publicados.',
  error: 'No pudimos abrir este juego.',
  signedOut: 'Tenés que ingresar para jugar.',
}

interface Props {
  // The minigame to play (its id in metadata/minigames, or a mechanic like runner). null: closed
  gameId: string | null
  onClose: () => void
}

// The minigames play with the characters and stories of the signed in user. Each opening loads Unity again
export function MiniGamesOverlay({ gameId, onClose }: Props) {
  return (
    <UnityAppOverlay
      url={gameId ? `${MINIGAMES_URL}?game=${encodeURIComponent(gameId)}` : null}
      source="fabulab-minigames"
      label="Juegos"
      texts={texts}
      onClose={onClose}
      layout="player"
      info={gameId && <WorkInfoPanel kind="minigame" workId={gameId} onCloseForEditor={onClose} />}
    />
  )
}
