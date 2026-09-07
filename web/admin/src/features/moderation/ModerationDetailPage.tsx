import { useEffect, useState } from 'react'
import { Link, Navigate, useNavigate, useParams } from 'react-router-dom'
import { multiUpdate, readPath, writePath } from '../../rtdb'
import type { MetadataEntry, Story, Tag, UserRecord } from '../../types'
import { isContentKind, KIND_LABELS } from './shared'

export function ModerationDetailPage() {
  const { kind, id } = useParams()
  const navigate = useNavigate()
  const [meta, setMeta] = useState<MetadataEntry | null>(null)
  const [raw, setRaw] = useState<unknown>(null)
  const [tags, setTags] = useState<Tag[]>([])
  const [owner, setOwner] = useState<UserRecord | null>(null)
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [newTag, setNewTag] = useState('')

  useEffect(() => {
    if (!isContentKind(kind) || !id) return
    setLoading(true)
    readPath<MetadataEntry>(`metadata/${kind}/${id}`).then(async (m) => {
      setMeta(m)
      const [tagList, rawData, ownerData] = await Promise.all([
        readPath<Record<string, Tag>>('tags'),
        m ? readPath(`${kind}/${m.userID}/${id}`) : Promise.resolve(null),
        m ? readPath<UserRecord>(`users/${m.userID}`) : Promise.resolve(null),
      ])
      setTags(tagList ? Object.values(tagList) : [])
      setRaw(rawData)
      setOwner(ownerData)
      setLoading(false)
    })
  }, [kind, id])

  if (!isContentKind(kind) || !id) return <Navigate to="/" replace />
  if (loading) return <p>Cargando…</p>
  if (!meta) return <div className="empty-state">No se encontró el contenido.</div>

  async function togglePublic() {
    if (!meta) return
    setSaving(true)
    const next = !meta.isPublic
    await writePath(`metadata/${kind}/${id}/isPublic`, next)
    setMeta({ ...meta, isPublic: next })
    setSaving(false)
  }

  async function addTag(tagId: string) {
    if (!meta || !tagId || meta.tags?.includes(tagId)) return
    const nextTags = [...(meta.tags ?? []), tagId]
    setSaving(true)
    await writePath(`metadata/${kind}/${id}/tags`, nextTags)
    setMeta({ ...meta, tags: nextTags })
    setNewTag('')
    setSaving(false)
  }

  async function removeTag(tagId: string) {
    if (!meta) return
    const nextTags = (meta.tags ?? []).filter((t) => t !== tagId)
    setSaving(true)
    await writePath(`metadata/${kind}/${id}/tags`, nextTags)
    setMeta({ ...meta, tags: nextTags })
    setSaving(false)
  }

  async function handleDelete() {
    if (!meta) return
    const ok = window.confirm(`¿Eliminar "${meta.name || id}" definitivamente? Esta acción no se puede deshacer.`)
    if (!ok) return
    setSaving(true)
    await multiUpdate({
      [`metadata/${kind}/${id}`]: null,
      [`${kind}/${meta.userID}/${id}`]: null,
    })
    navigate(`/moderation/${kind}`)
  }

  const speechLines =
    kind === 'stories' && Array.isArray(raw)
      ? (raw as Story)
          .filter((scene) => scene && Array.isArray(scene.scenesElements))
          .flatMap((scene) =>
            scene.scenesElements
              .filter((el) => el && el.type === 2 && el.input)
              .map((el) => el.input as string),
          )
      : []

  return (
    <div>
      <Link className="back-link" to={`/moderation/${kind}`}>
        ← Volver a {KIND_LABELS[kind]}
      </Link>
      <h1 className="page-title">{meta.name || '(sin nombre)'}</h1>

      <div className="detail-card">
        <div className="field-row">
          <label>ID</label>
          <span className="id-cell">{id}</span>
        </div>

        <div className="field-row">
          <label>Autor</label>
          <span>
            {owner ? (
              <Link to={`/users/${meta.userID}`}>{owner.username || owner.email}</Link>
            ) : (
              meta.userID
            )}
          </span>
        </div>

        <div className="field-row">
          <label>Visibilidad</label>
          <div>
            <span className={`badge ${meta.isPublic ? 'public' : 'private'}`}>
              {meta.isPublic ? 'Público' : 'Privado'}
            </span>{' '}
            <button className="btn" disabled={saving} onClick={togglePublic}>
              {meta.isPublic ? 'Hacer privado' : 'Hacer público'}
            </button>
          </div>
        </div>

        <div className="field-row">
          <label>Likes</label>
          <span>{meta.likes ?? 0}</span>
        </div>

        <div className="field-row">
          <label>Fecha</label>
          <span>{meta.timestamp ? new Date(meta.timestamp).toLocaleString() : '—'}</span>
        </div>

        <div className="field-row">
          <label>Tags</label>
          <div>
            {(meta.tags ?? []).map((t) => (
              <span className="tag-pill" key={t}>
                {tags.find((tg) => tg.id === t)?.name ?? t}
                <button onClick={() => removeTag(t)} disabled={saving}>
                  ×
                </button>
              </span>
            ))}
          </div>
          <div style={{ display: 'flex', gap: 8, marginTop: 6 }}>
            <select value={newTag} onChange={(e) => setNewTag(e.target.value)}>
              <option value="">Agregar tag…</option>
              {tags
                .filter((t) => !meta.tags?.includes(t.id))
                .map((t) => (
                  <option key={t.id} value={t.id}>
                    {t.name}
                  </option>
                ))}
            </select>
            <button className="btn" disabled={!newTag || saving} onClick={() => addTag(newTag)}>
              Agregar
            </button>
          </div>
        </div>

        {speechLines.length > 0 && (
          <div className="field-row">
            <label>Texto de la historia</label>
            <ul>
              {speechLines.map((line, i) => (
                <li key={i}>{line}</li>
              ))}
            </ul>
          </div>
        )}

        <div className="field-row">
          <label>JSON crudo</label>
          <pre className="json-viewer">{JSON.stringify(raw, null, 2)}</pre>
        </div>

        <button className="btn danger" disabled={saving} onClick={handleDelete}>
          Eliminar definitivamente
        </button>
      </div>
    </div>
  )
}
