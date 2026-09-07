import { useEffect, useState } from 'react'
import { multiUpdate, readPath, writePath } from '../../rtdb'
import type { Tag } from '../../types'

const ITEM_TYPE_OPTIONS = ['characters', 'so', 'stories']

export function TagsPage() {
  const [tags, setTags] = useState<Tag[]>([])
  const [loading, setLoading] = useState(true)
  const [newId, setNewId] = useState('')
  const [newName, setNewName] = useState('')

  function load() {
    setLoading(true)
    readPath<Record<string, Tag>>('tags').then((data) => {
      setTags(data ? Object.values(data).sort((a, b) => a.name.localeCompare(b.name)) : [])
      setLoading(false)
    })
  }

  useEffect(load, [])

  async function createTag() {
    const id = newId.trim()
    const name = newName.trim()
    if (!id || !name) return
    await writePath(`tags/${id}`, { id, name })
    setNewId('')
    setNewName('')
    load()
  }

  async function renameTag(tag: Tag, name: string) {
    await writePath(`tags/${tag.id}/name`, name)
    load()
  }

  async function toggleItemType(tag: Tag, type: string) {
    const current = tag.itemTypes ?? []
    const next = current.includes(type) ? current.filter((t) => t !== type) : [...current, type]
    await writePath(`tags/${tag.id}/itemTypes`, next)
    load()
  }

  async function deleteTag(tag: Tag) {
    const ok = window.confirm(`¿Eliminar el tag "${tag.name}"? No se quita de contenido que ya lo tenga asignado.`)
    if (!ok) return
    await multiUpdate({ [`tags/${tag.id}`]: null })
    load()
  }

  return (
    <div>
      <h1 className="page-title">Tags</h1>

      <div className="toolbar">
        <input placeholder="id (ej: nature)" value={newId} onChange={(e) => setNewId(e.target.value)} />
        <input placeholder="Nombre (ej: Naturaleza)" value={newName} onChange={(e) => setNewName(e.target.value)} />
        <button className="btn primary" onClick={createTag} disabled={!newId.trim() || !newName.trim()}>
          Crear tag
        </button>
      </div>

      {loading ? (
        <p>Cargando…</p>
      ) : (
        <table>
          <thead>
            <tr>
              <th>ID</th>
              <th>Nombre</th>
              <th>Aplica a</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {tags.map((tag) => (
              <tr key={tag.id}>
                <td>{tag.id}</td>
                <td>
                  <input
                    defaultValue={tag.name}
                    onBlur={(e) => e.target.value !== tag.name && renameTag(tag, e.target.value)}
                  />
                </td>
                <td>
                  {ITEM_TYPE_OPTIONS.map((type) => (
                    <label key={type} style={{ marginRight: 10, fontSize: 13 }}>
                      <input
                        type="checkbox"
                        checked={tag.itemTypes?.includes(type) ?? false}
                        onChange={() => toggleItemType(tag, type)}
                      />{' '}
                      {type}
                    </label>
                  ))}
                </td>
                <td>
                  <button className="btn danger" onClick={() => deleteTag(tag)}>
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
