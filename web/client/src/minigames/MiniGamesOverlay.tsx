import { asset } from '../common/asset'
import { UnityAppOverlay } from '../common/UnityAppOverlay'

// The Unity WebGL build of the scene MiniGames. By default it is served by this site from public/minigames
const MINIGAMES_URL = import.meta.env.VITE_MINIGAMES_URL || asset('minigames/index.html')

const texts = {
  missing: 'Los juegos todavía no están publicados.',
  error: 'No pudimos abrir los juegos.',
  signedOut: 'Tenés que ingresar para jugar.',
}

interface Props {
  open: boolean
  onClose: () => void
}

// The minigames play with the characters and stories of the signed in user. Each opening loads Unity again
export function MiniGamesOverlay({ open, onClose }: Props) {
  return (
    <UnityAppOverlay
      url={open ? MINIGAMES_URL : null}
      source="fabulab-minigames"
      label="Juegos"
      texts={texts}
      onClose={onClose}
    />
  )
}
