import { useMemo } from 'react'
import { asset } from '../common/asset'
import { UnityAppOverlay } from '../common/UnityAppOverlay'

// The Unity WebGL build of the scene Editor. By default it is served by this site from public/editor
const EDITOR_URL = import.meta.env.VITE_EDITOR_URL || asset('editor/index.html')

// What the editor opens (WebEditorController.WorkType). Without id it creates a new one
export type EditorWorkType = 'story' | 'character' | 'object' | 'background'

export interface EditorRequest {
  type: EditorWorkType
  id?: string
}

const texts = {
  missing: 'El editor todavía no está publicado.',
  error: 'No pudimos abrir el editor.',
  signedOut: 'Tenés que ingresar para editar.',
}

interface Props {
  request: EditorRequest | null
  // The editor was left. It may have saved changes
  onClose: () => void
}

// Every edition starts a new editor: Unity loads again with the work in its URL
export function EditorOverlay({ request, onClose }: Props) {
  const url = useMemo(() => {
    if (!request) return null
    const params = new URLSearchParams({ type: request.type })
    if (request.id) params.set('id', request.id)
    return `${EDITOR_URL}?${params}`
  }, [request])

  return <UnityAppOverlay url={url} source="fabulab-editor" label="Editor" texts={texts} onClose={onClose} />
}
