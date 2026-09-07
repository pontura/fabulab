import { useEffect, useState } from 'react'
import { Link, Navigate, useNavigate, useParams } from 'react-router-dom'
import { multiUpdate, readPath } from '../../rtdb'
import type { ContentKind, MetadataEntry, UserRecord } from '../../types'

const KINDS: ContentKind[] = ['characters', 'so', 'stories']

interface OwnedItem {
  kind: ContentKind
  id: string
  name: string
}

export function UserDetailPage() {
  const { uid } = useParams()
  const navigate = useNavigate()
  const [user, setUser] = useState<UserRecord | null>(null)
  const [counts, setCounts] = useState<Record<ContentKind, number> | null>(null)
  const [owned, setOwned] = useState<OwnedItem[]>([])
  const [loading, setLoading] = useState(true)
  const [deleting, setDeleting] = useState(false)

  useEffect(() => {
    if (!uid) return
    setLoading(true)
    Promise.all([
      readPath<UserRecord>(`users/${uid}`),
      readPath<Record<string, unknown>>(`characters/${uid}`),
      readPath<Record<string, unknown>>(`so/${uid}`),
      readPath<Record<string, unknown>>(`stories/${uid}`),
      readPath<Record<string, MetadataEntry>>('metadata/characters'),
      readPath<Record<string, MetadataEntry>>('metadata/so'),
      readPath<Record<string, MetadataEntry>>('metadata/stories'),
    ]).then(([userData, chars, sos, stories, metaChars, metaSo, metaStories]) => {
      setUser(userData)
      setCounts({
        characters: chars ? Object.keys(chars).length : 0,
        so: sos ? Object.keys(sos).length : 0,
        stories: stories ? Object.keys(stories).length : 0,
      })
      const metaByKind: Record<ContentKind, Record<string, MetadataEntry> | null> = {
        characters: metaChars,
        so: metaSo,
        stories: metaStories,
      }
      const items: OwnedItem[] = []
      for (const kind of KINDS) {
        const entries = metaByKind[kind]
        if (!entries) continue
        for (const [id, entry] of Object.entries(entries)) {
          if (entry.userID === uid) items.push({ kind, id, name: entry.name })
        }
      }
      setOwned(items)
      setLoading(false)
    })
  }, [uid])

  if (!uid) return <Navigate to="/users" replace />
  if (loading) return <p>Cargando…</p>
  if (!user) return <div className="empty-state">Usuario no encontrado.</div>

  async function handleDeleteUser() {
    if (!uid || !user) return
    const ok = window.confirm(
      `¿Eliminar TODOS los datos de "${user.username || user.email}" (personajes, objetos, historias, metadata)? Esto no elimina la cuenta de Firebase Authentication. Esta acción no se puede deshacer.`,
    )
    if (!ok) return
    setDeleting(true)
    const updates: Record<string, null> = {
      [`users/${uid}`]: null,
      [`characters/${uid}`]: null,
      [`so/${uid}`]: null,
      [`stories/${uid}`]: null,
    }
    for (const item of owned) {
      updates[`metadata/${item.kind}/${item.id}`] = null
    }
    await multiUpdate(updates)
    navigate('/users')
  }

  return (
    <div>
      <Link className="back-link" to="/users">
        ← Volver a Usuarios
      </Link>
      <h1 className="page-title">{user.username || user.email}</h1>

      <div className="detail-card">
        <div className="field-row">
          <label>Email</label>
          <span>{user.email}</span>
        </div>
        <div className="field-row">
          <label>UID</label>
          <span>{user.uid}</span>
        </div>
        {counts && (
          <div className="field-row">
            <label>Contenido</label>
            <span>
              {counts.characters} personajes · {counts.so} objetos de escena · {counts.stories} historias
            </span>
          </div>
        )}

        <div className="field-row">
          <label>Items públicos/moderables</label>
          {owned.length === 0 ? (
            <span>Sin entradas en metadata.</span>
          ) : (
            <ul>
              {owned.map((item) => (
                <li key={`${item.kind}-${item.id}`}>
                  <Link to={`/moderation/${item.kind}/${item.id}`}>
                    [{item.kind}] {item.name || '(sin nombre)'}
                  </Link>{' '}
                  <span className="id-cell">{item.id}</span>
                </li>
              ))}
            </ul>
          )}
        </div>

        <button className="btn danger" disabled={deleting} onClick={handleDeleteUser}>
          Eliminar cuenta y todos sus datos
        </button>
        <p style={{ fontSize: 12, color: 'var(--muted)' }}>
          Esto borra los datos en Realtime Database. La cuenta de Firebase Authentication debe eliminarse aparte
          (consola de Firebase), ya que requiere permisos de administrador que este panel no tiene.
        </p>
      </div>
    </div>
  )
}
