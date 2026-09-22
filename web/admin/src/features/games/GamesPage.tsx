import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { multiUpdate, readPath, writePath } from '../../rtdb'
import type { GameDef } from '../../types'

type Row = GameDef & { id: string }

export function GamesPage() {
  const navigate = useNavigate()
  const [rows, setRows] = useState<Row[]>([])
  const [loading, setLoading] = useState(true)
  const [newId, setNewId] = useState('')

  function load() {
    setLoading(true)
    readPath<Record<string, Omit<GameDef, 'id'>>>('games').then((data) => {
      setRows(data ? Object.entries(data).map(([id, g]) => ({ id, ...g, ids: g.ids ?? {} })) : [])
      setLoading(false)
    })
  }

  useEffect(load, [])

  async function createGame() {
    const id = newId.trim()
    if (!id) return
    await writePath(`games/${id}`, {
      id,
      title: 'Nuevo juego',
      description: '',
      section: 'stories',
      ids: {},
    })
    setNewId('')
    navigate(`/games/${id}`)
  }

  async function deleteGame(row: Row) {
    const ok = window.confirm(`¿Eliminar el juego "${row.title}"?`)
    if (!ok) return
    await multiUpdate({ [`games/${row.id}`]: null })
    load()
  }

  return (
    <div>
      <h1 className="page-title">Games</h1>
      <div className="toolbar">
        <input placeholder="id (ej: como_termina)" value={newId} onChange={(e) => setNewId(e.target.value)} />
        <button className="btn primary" onClick={createGame} disabled={!newId.trim()}>
          Crear juego
        </button>
      </div>

      {loading ? (
        <p>Cargando…</p>
      ) : (
        <table>
          <thead>
            <tr>
              <th>ID</th>
              <th>Título</th>
              <th>Sección</th>
              <th>Historias vinculadas</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {rows.map((row) => (
              <tr key={row.id} className="clickable" onClick={() => navigate(`/games/${row.id}`)}>
                <td>{row.id}</td>
                <td>{row.title}</td>
                <td>{row.section}</td>
                <td>{Object.keys(row.ids ?? {}).length}</td>
                <td>
                  <button
                    className="btn danger"
                    onClick={(e) => {
                      e.stopPropagation()
                      deleteGame(row)
                    }}
                  >
                    Eliminar
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  )
}
