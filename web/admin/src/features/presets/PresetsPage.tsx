import { Fragment, useEffect, useState } from 'react'
import { multiUpdate, pushPath, readPath, writePath } from '../../rtdb'
import type { BodypartCategory, Preset } from '../../types'

const CATEGORIES: BodypartCategory[] = ['BODY', 'FACE', 'FOOT', 'HAIR', 'HAND', 'HEAD', 'none']

type Row = Preset & { id: string }

export function PresetsPage() {
  const [category, setCategory] = useState<BodypartCategory>('BODY')
  const [rows, setRows] = useState<Row[]>([])
  const [loading, setLoading] = useState(true)
  const [expandedId, setExpandedId] = useState<string | null>(null)
  const [draftItems, setDraftItems] = useState('')
  const [draftError, setDraftError] = useState<string | null>(null)

  function load() {
    setLoading(true)
    setExpandedId(null)
    readPath<Record<string, Preset>>(`presets/bodypart/${category}`).then((data) => {
      setRows(data ? Object.entries(data).map(([id, p]) => ({ id, ...p })) : [])
      setLoading(false)
    })
  }

  useEffect(load, [category])

  function expand(row: Row) {
    setExpandedId(row.id)
    setDraftItems(JSON.stringify(row.items, null, 2))
    setDraftError(null)
  }

  async function saveItems(row: Row) {
    try {
      const parsed = JSON.parse(draftItems)
      if (!Array.isArray(parsed)) throw new Error('Debe ser un array')
      await writePath(`presets/bodypart/${category}/${row.id}/items`, parsed)
      setDraftError(null)
      load()
    } catch (e) {
      setDraftError(e instanceof Error ? e.message : 'JSON inválido')
    }
  }

  async function saveBg(row: Row, bg: number) {
    await writePath(`presets/bodypart/${category}/${row.id}/bg`, bg)
    load()
  }

  async function deletePreset(row: Row) {
    const ok = window.confirm(`¿Eliminar este preset de ${category}?`)
    if (!ok) return
    await multiUpdate({ [`presets/bodypart/${category}/${row.id}`]: null })
    load()
  }

  async function createPreset() {
    await pushPath(`presets/bodypart/${category}`, { bg: 0, items: [] })
    load()
  }

  return (
    <div>
      <h1 className="page-title">Presets</h1>
      <div className="toolbar">
        <select value={category} onChange={(e) => setCategory(e.target.value as BodypartCategory)}>
          {CATEGORIES.map((c) => (
            <option key={c} value={c}>
              {c}
            </option>
          ))}
        </select>
        <button className="btn primary" onClick={createPreset}>
          Nuevo preset
        </button>
        <span>{rows.length} presets</span>
      </div>

      {loading ? (
        <p>Cargando…</p>
      ) : rows.length === 0 ? (
        <div className="empty-state">Sin presets en esta categoría</div>
      ) : (
        <table>
          <thead>
            <tr>
              <th>ID</th>
              <th>bg</th>
              <th>items</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {rows.map((row) => (
              <Fragment key={row.id}>
                <tr className="clickable" onClick={() => (expandedId === row.id ? setExpandedId(null) : expand(row))}>
                  <td>{row.id}</td>
                  <td>{row.bg}</td>
                  <td>{row.items?.length ?? 0}</td>
                  <td>
                    <button
                      className="btn danger"
                      onClick={(e) => {
                        e.stopPropagation()
                        deletePreset(row)
                      }}
                    >
                      Eliminar
                    </button>
                  </td>
                </tr>
                {expandedId === row.id && (
                  <tr>
                    <td colSpan={4}>
                      <div className="field-row">
                        <label>bg</label>
                        <input
                          type="number"
                          defaultValue={row.bg}
                          onBlur={(e) => saveBg(row, Number(e.target.value))}
                          style={{ width: 100 }}
                        />
                      </div>
                      <div className="field-row" style={{ marginTop: 10 }}>
                        <label>items (JSON)</label>
                        <textarea
                          rows={10}
                          value={draftItems}
                          onChange={(e) => setDraftItems(e.target.value)}
                        />
                        {draftError && <p className="error-text">{draftError}</p>}
                        <button className="btn primary" style={{ marginTop: 6, width: 140 }} onClick={() => saveItems(row)}>
                          Guardar items
                        </button>
                      </div>
                    </td>
                  </tr>
                )}
              </Fragment>
            ))}
          </tbody>
        </table>
      )}
    </div>
  )
}
