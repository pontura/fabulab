import type { ContentKind } from '../../types'

export const KIND_LABELS: Record<ContentKind, string> = {
  characters: 'Personajes',
  so: 'Objetos de escena',
  stories: 'Historias',
}

export function isContentKind(value: string | undefined): value is ContentKind {
  return value === 'characters' || value === 'so' || value === 'stories'
}
