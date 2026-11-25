import { useCallback, useEffect, useMemo, useRef, useState, type FormEvent } from 'react'
import { RemoteImage } from '../common/RemoteImage'
import { imageUrl } from '../data/images'
import {
  deleteWork,
  duplicateWork,
  loadTags,
  saveWorkInfo,
  type ManagedKind,
  type Tag,
  type WorkInfo,
} from '../data/workActions'

// The work of the user shown by the popup
export interface ManagedWork {
  id: string
  userID: string
  name: string
  isPublic: boolean
  tags: string[]
}

const kindNames: Record<ManagedKind, string> = {
  stories: 'historia',
  characters: 'personaje',
  so: 'objeto',
}

interface Props {
  kind: ManagedKind
  // What the work is called in the texts (ex: "fondo"). By default, by its kind
  kindName?: string
  work: ManagedWork
  // Open the work in the editor (the changes of the popup are already saved)
  onEdit: () => void
  // A copy was made: the popup shows it from now on
  onDuplicated: (copy: ManagedWork) => void
  // Something was saved, copied or deleted: the lists must load again
  onChanged: () => void
  onClose: () => void
}

type Step = 'info' | 'confirmDelete'

// Popup of a work of the user (InfoDataScreen in the app): its name, privacy and tags, and buttons to edit,
// duplicate or delete it
export function WorkInfoDialog({ kind, kindName, work, onEdit, onDuplicated, onChanged, onClose }: Props) {
  const label = kindName ?? kindNames[kind]
  const [name, setName] = useState(work.name)
  const [isPublic, setIsPublic] = useState(work.isPublic)
  const [selected, setSelected] = useState<string[]>(work.tags)
  const [tags, setTags] = useState<Tag[] | null>(null)
  const [step, setStep] = useState<Step>('info')
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState('')
  const panel = useRef<HTMLDivElement>(null)

  // Another work (ex: the copy just made): its own values
  useEffect(() => {
    setName(work.name)
    setIsPublic(work.isPublic)
    setSelected(work.tags)
    setStep('info')
    setError('')
  }, [work])

  useEffect(() => {
    let current = true
    loadTags(kind)
      .then((result) => current && setTags(result))
      .catch((e) => {
        console.error('Could not load the tags', e)
        if (current) setTags([])
      })
    return () => {
      current = false
    }
  }, [kind])

  useEffect(() => {
    panel.current?.focus()
  }, [])

  // Tags the user cannot choose here (ex: "games" of the stories made in a game) are kept as they are
  const info = useMemo<WorkInfo>(() => {
    const choosable = new Set((tags ?? []).map((t) => t.id))
    const kept = work.tags.filter((t) => !choosable.has(t))
    return {
      name: name.trim(),
      isPublic,
      tags: [...kept, ...selected.filter((t) => choosable.has(t))],
    }
  }, [name, isPublic, selected, tags, work.tags])

  const dirty =
    info.name !== work.name.trim() ||
    info.isPublic !== work.isPublic ||
    info.tags.length !== work.tags.length ||
    info.tags.some((t) => !work.tags.includes(t))

  // Runs an action of the popup: one at a time, with its error shown
  const run = useCallback(
    async (action: () => Promise<void>, failure: string) => {
      if (busy) return
      setBusy(true)
      setError('')
      try {
        await action()
      } catch (e) {
        console.error(failure, e)
        setError(failure)
      } finally {
        setBusy(false)
      }
    },
    [busy],
  )

  const saveIfDirty = useCallback(async () => {
    if (!dirty || tags === null) return
    await saveWorkInfo(kind, work.id, info)
    onChanged()
  }, [dirty, tags, kind, work.id, info, onChanged])

  const close = useCallback(() => {
    if (!dirty) return onClose()
    run(async () => {
      await saveIfDirty()
      onClose()
    }, 'No pudimos guardar los cambios.')
  }, [dirty, onClose, run, saveIfDirty])

  useEffect(() => {
    function onKey(e: KeyboardEvent) {
      if (e.key !== 'Escape') return
      if (step === 'confirmDelete') setStep('info')
      else close()
    }
    document.addEventListener('keydown', onKey)
    return () => document.removeEventListener('keydown', onKey)
  }, [step, close])

  function handleSubmit(e: FormEvent) {
    e.preventDefault()
    close()
  }

  const edit = () =>
    run(async () => {
      await saveIfDirty()
      onEdit()
    }, 'No pudimos guardar los cambios.')

  const duplicate = () =>
    run(async () => {
      await saveIfDirty()
      const copyId = await duplicateWork(kind, work.userID, work.id, info.name)
      onChanged()
      onDuplicated({
        id: copyId,
        userID: work.userID,
        name: info.name ? `${info.name} (copia)` : '',
        isPublic: false,
        tags: info.tags,
      })
    }, `No pudimos duplicar ${articleFor(label)} ${label}.`)

  const remove = () =>
    run(async () => {
      await deleteWork(kind, work.userID, work.id)
      onChanged()
      onClose()
    }, `No pudimos borrar ${articleFor(label)} ${label}.`)

  const loadThumb = useCallback(() => imageUrl(work.userID, kind, work.id), [work.userID, kind, work.id])
  const title = info.name || (kind === 'stories' ? 'Sin título' : 'Sin nombre')
  const feminine = label === 'historia'

  return (
    <div className="work-dialog-backdrop" onMouseDown={(e) => e.target === e.currentTarget && close()}>
      <div
        ref={panel}
        className="work-dialog"
        role="dialog"
        aria-modal="true"
        aria-labelledby="work-dialog-title"
        tabIndex={-1}
      >
        <button type="button" className="work-dialog-close" onClick={close} disabled={busy} aria-label="Cerrar">
          <svg viewBox="0 0 24 24" width="22" height="22" aria-hidden="true">
            <path d="M6 6l12 12M18 6L6 18" stroke="currentColor" strokeWidth="2.6" strokeLinecap="round" />
          </svg>
        </button>

        <div className="work-dialog-thumb">
          <RemoteImage key={work.id} load={loadThumb} alt={title} />
        </div>

        {step === 'confirmDelete' ? (
          <div className="work-dialog-confirm">
            <h2 id="work-dialog-title" className="work-dialog-heading">
              ¿Borrar {articleFor(label)} {label} “{title}”?
            </h2>
            <p className="work-dialog-text">No se puede deshacer.</p>
            {error && <p className="work-dialog-error">{error}</p>}
            <div className="work-dialog-actions">
              <button type="button" className="button button-nav" onClick={() => setStep('info')} disabled={busy}>
                Cancelar
              </button>
              <button type="button" className="button button-danger" onClick={remove} disabled={busy}>
                {busy ? 'Borrando…' : 'Sí, borrar'}
              </button>
            </div>
          </div>
        ) : (
          <form className="work-dialog-form" onSubmit={handleSubmit}>
            <h2 id="work-dialog-title" className="visually-hidden">
              {title}
            </h2>

            <label className="work-dialog-label" htmlFor="work-dialog-name">
              {kind === 'stories' ? 'Título' : 'Nombre'}
            </label>
            <input
              id="work-dialog-name"
              className="field"
              value={name}
              maxLength={60}
              placeholder={kind === 'stories' ? 'Sin título' : 'Sin nombre'}
              onChange={(e) => setName(e.target.value)}
              disabled={busy}
            />

            <span className="work-dialog-label">Visibilidad</span>
            <div className="work-dialog-privacy" role="radiogroup" aria-label="Visibilidad">
              <button
                type="button"
                role="radio"
                aria-checked={!isPublic}
                className={`chip ${!isPublic ? 'is-on' : ''}`}
                onClick={() => setIsPublic(false)}
                disabled={busy}
              >
                {feminine ? 'Privada' : 'Privado'}
              </button>
              <button
                type="button"
                role="radio"
                aria-checked={isPublic}
                className={`chip ${isPublic ? 'is-on' : ''}`}
                onClick={() => setIsPublic(true)}
                disabled={busy}
              >
                {feminine ? 'Pública' : 'Público'}
              </button>
            </div>

            {(tags === null || tags.length > 0) && (
              <>
                <span className="work-dialog-label">Tags</span>
                <div className="work-dialog-tags" aria-busy={tags === null}>
                  {tags === null && <span className="work-dialog-text">Cargando…</span>}
                  {tags?.map((tag) => {
                    const on = selected.includes(tag.id)
                    return (
                      <button
                        key={tag.id}
                        type="button"
                        className={`chip ${on ? 'is-on' : ''}`}
                        aria-pressed={on}
                        onClick={() =>
                          setSelected((s) => (on ? s.filter((t) => t !== tag.id) : [...s, tag.id]))
                        }
                        disabled={busy}
                      >
                        {tag.name}
                      </button>
                    )
                  })}
                </div>
              </>
            )}

            {error && <p className="work-dialog-error">{error}</p>}

            <div className="work-dialog-actions">
              <button type="button" className="button button-primary" onClick={edit} disabled={busy}>
                Editar
              </button>
              <button type="button" className="button button-nav" onClick={duplicate} disabled={busy}>
                Duplicar
              </button>
              <button
                type="button"
                className="button button-danger"
                onClick={() => {
                  setError('')
                  setStep('confirmDelete')
                }}
                disabled={busy}
              >
                Borrar
              </button>
            </div>
            <button type="submit" className="button button-nav work-dialog-save" disabled={busy || !dirty}>
              {busy ? 'Guardando…' : dirty ? 'Guardar cambios' : 'Sin cambios'}
            </button>
          </form>
        )}
      </div>
    </div>
  )
}

function articleFor(kindName: string): string {
  return kindName === 'historia' ? 'la' : 'el'
}
