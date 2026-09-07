import { useEffect, useMemo, useState } from 'react'
import { Navigate, useNavigate, useParams } from 'react-router-dom'
import { readPath } from '../../rtdb'
import type { MetadataEntry, UserRecord } from '../../types'
import { isContentKind, KIND_LABELS } from './shared'

type Row = MetadataEntry & { id: string }

export function ModerationListPage() {
  const { kind } = useParams()
  const navigate = useNavigate()
  const [rows, setRows] = useState<Row[]>([])
  const [users, setUsers] = useState<Record<string, UserRecord>>({})
  const [loading, setLoading] = useState(true)
  const [search, setSearch] = useState('')
  const [visibility, setVisibility] = useState<'all' | 'public' | 'private'>('all')
  const [author, setAuthor] = useState('')

  useEffect(() => {
    if (!isContentKind(kind)) return
    setLoading(true)
    setRows([])
    setSearch('')
    setVisibility('all')
    setAuthor('')
    Promise.all([
      readPath<Record<string, MetadataEntry>>(`metadata/${kind}`),
      readPath<Record<string, UserRecord>>('users'),
    ]).then(([data, userData]) => {
      const list = data ? Object.entries(data).map(([id, entry]) => ({ id, ...entry })) : []
      list.sort((a, b) => (b.timestamp ?? '').localeCompare(a.timestamp ?? ''))
      setRows(list)
      setUsers(userData ?? {})
      setLoading(false)
    })
  }, [kind])

  const authorLabel = (uid: string) => users[uid]?.username || users[uid]?.email || uid

  const authorOptions = useMemo(() => {
    const uids = Array.from(new Set(rows.map((r) => r.userID)))
    return uids.sort((a, b) => authorLabel(a).localeCompare(authorLabel(b)))
  }, [rows, users])

  const filtered = useMemo(() => {
    return rows.filter((r) => {
      if (visibility === 'public' && !r.isPublic) return false
      if (visibility === 'private' && r.isPublic) return false
      if (author && r.userID !== author) return false
      if (
        search &&
        !(r.name ?? '').toLowerCase().includes(search.toLowerCase()) &&
        !r.id.toLowerCase().includes(search.toLowerCase())
      )
        return false
      return true
    })
  }, [rows, search, visibility, author])

  if (!isContentKind(kind)) return <Navigate to="/" replace />

  return (
    <div>
      <h1 className="page-title">{KIND_LABELS[kind]}</h1>
      <div className="toolbar">
        <input
          placeholder="Buscar por nombre o id…"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
        <select value={visibility} onChange={(e) => setVisibility(e.target.value as typeof visibility)}>
          <option value="all">Todos</option>
          <option value="public">Públicos</option>
          <option value="private">Privados</option>
        </select>
        <select value={author} onChange={(e) => setAuthor(e.target.value)}>
          <option value="">Todos los autores</option>
          {authorOptions.map((uid) => (
            <option key={uid} value={uid}>
              {authorLabel(uid)}
            </option>
          ))}
        </select>
        <span>{filtered.length} resultados</span>
      </div>

      {loading ? (
        <p>Cargando…</p>
      ) : filtered.length === 0 ? (
        <div className="empty-state">Sin resultados</div>
      ) : (
        <table>
          <thead>
            <tr>
              <th>ID</th>
              <th>Nombre</th>
              <th>Autor</th>
              <th>Estado</th>
              <th>Likes</th>
              <th>Tags</th>
              <th>Fecha</th>
            </tr>
          </thead>
          <tbody>
            {filtered.map((r) => (
              <tr
                key={r.id}
                className="clickable"
                onClick={() => navigate(`/moderation/${kind}/${r.id}`)}
              >
                <td className="id-cell">{r.id}</td>
                <td>{r.name || '(sin nombre)'}</td>
                <td>{authorLabel(r.userID)}</td>
                <td>
                  <span className={`badge ${r.isPublic ? 'public' : 'private'}`}>
                    {r.isPublic ? 'Público' : 'Privado'}
                  </span>
                </td>
                <td>{r.likes ?? 0}</td>
                <td>{(r.tags ?? []).join(', ')}</td>
                <td>{r.timestamp ? new Date(r.timestamp).toLocaleDateString() : '—'}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  )
}
