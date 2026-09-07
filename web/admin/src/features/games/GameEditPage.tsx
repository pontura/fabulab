import { useEffect, useRef, useState } from 'react'
import { Link, Navigate, useParams } from 'react-router-dom'
import { multiUpdate, readPath, writePath } from '../../rtdb'
import type { GameDef } from '../../types'

export function GameEditPage() {
  const { id } = useParams()
  const [game, setGame] = useState<GameDef | null>(null)
  const [loading, setLoading] = useState(true)
  const [newStoryId, setNewStoryId] = useState('')
  const storyInputRefs = useRef<Record<string, HTMLInputElement | null>>({})

  function load() {
    if (!id) return
    setLoading(true)
    readPath<Omit<GameDef, 'id'>>(`games/${id}`).then((data) => {
      setGame(data ? { id, ...data } : null)
      setLoading(false)
    })
  }

  useEffect(load, [id])

  if (!id) return <Navigate to="/games" replace />
  if (loading) return <p>Cargando…</p>
  if (!game) return <div className="empty-state">Juego no encontrado.</div>

  async function saveField(field: 'title' | 'description' | 'section', value: string) {
    await writePath(`games/${id}/${field}`, value)
    load()
  }

  async function saveStoryIds(storyId: string, raw: string) {
    let parsed: (string | null)[]
    try {
      parsed = JSON.parse(raw)
      if (!Array.isArray(parsed)) throw new Error()
    } catch {
      window.alert('Debe ser un array JSON válido, ej: ["-Or...", null]')
      return
    }
    await writePath(`games/${id}/ids/${storyId}`, parsed)
    load()
  }

  function updateStoryId(storyId: string) {
    const value = storyInputRefs.current[storyId]?.value ?? ''
    saveStoryIds(storyId, value)
  }

  async function removeStoryId(storyId: string) {
    const ok = window.confirm(`¿Quitar la historia "${storyId}" de este juego?`)
    if (!ok) return
    await multiUpdate({ [`games/${id}/ids/${storyId}`]: null })
    load()
  }

  async function addStoryId() {
    const key = newStoryId.trim()
    if (!key) return
    await writePath(`games/${id}/ids/${key}`, [])
    setNewStoryId('')
    load()
  }

  return (
    <div>
      <Link className="back-link" to="/games">
        ← Volver a Games
      </Link>
      <h1 className="page-title">{game.title}</h1>

      <div className="detail-card">
        <div className="field-row">
          <label>ID</label>
          <span className="id-cell">{game.id}</span>
        </div>
        <div className="field-row">
          <label>Título</label>
          <input defaultValue={game.title} onBlur={(e) => e.target.value !== game.title && saveField('title', e.target.value)} />
        </div>
        <div className="field-row">
          <label>Descripción</label>
          <textarea
            rows={3}
            defaultValue={game.description}
            onBlur={(e) => e.target.value !== game.description && saveField('description', e.target.value)}
          />
        </div>
        <div className="field-row">
          <label>Sección</label>
          <input defaultValue={game.section} onBlur={(e) => e.target.value !== game.section && saveField('section', e.target.value)} />
        </div>

        <div className="field-row">
          <label>Historias vinculadas (id de historia → array de finales alternativos)</label>
          {Object.entries(game.ids ?? {}).map(([storyId, values]) => (
            <div key={storyId} style={{ display: 'flex', gap: 8, alignItems: 'center', marginBottom: 6 }}>
              <code>{storyId}</code>
              <input
                ref={(el) => {
                  storyInputRefs.current[storyId] = el
                }}
                defaultValue={JSON.stringify(values)}
                style={{ flex: 1 }}
              />
              <button className="btn primary" onClick={() => updateStoryId(storyId)}>
                Actualizar
              </button>
              <button className="btn danger" onClick={() => removeStoryId(storyId)}>
                Quitar
              </button>
            </div>
          ))}
          <div style={{ display: 'flex', gap: 8, marginTop: 6 }}>
            <input
              placeholder="id de historia a agregar"
              value={newStoryId}
              onChange={(e) => setNewStoryId(e.target.value)}
            />
            <button className="btn primary" onClick={addStoryId} disabled={!newStoryId.trim()}>
              Agregar
            </button>
          </div>
        </div>
      </div>
    </div>
  )
}
