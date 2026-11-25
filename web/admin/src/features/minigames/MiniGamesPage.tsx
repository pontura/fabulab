import { useEffect, useState } from 'react'
import { readPath, writePath } from '../../rtdb'

// The minigames (metadata/minigames/{id}; their content in minigames/{uid}/{id}). The originals of Fabulab
// (isOfficial) are uploaded from the Unity editor (MiniGame component: "Subir como minijuego oficial"), and the users
// make their versions of them (copies) in the web editor of the public site. None is edited here

interface ServerMiniGameMeta {
  name?: string
  userID?: string
  mechanic?: string
  isPublic?: boolean
  timestamp?: string
  sourceId?: string
  isOfficial?: boolean
}

type Row = Required<ServerMiniGameMeta> & { id: string }

// The public site (web/client), where they are played and edited. Ex: https://fabulab.example.com/
const SITE_URL = import.meta.env.VITE_SITE_URL as string | undefined

function siteUrl(param: string, value: string): string | null {
  if (!SITE_URL) return null
  const url = new URL(SITE_URL)
  url.searchParams.set(param, value)
  return url.toString()
}

export function MiniGamesPage() {
  const [rows, setRows] = useState<Row[]>([])
  const [loading, setLoading] = useState(true)
  const [copied, setCopied] = useState<string | null>(null)

  function load() {
    setLoading(true)
    readPath<Record<string, ServerMiniGameMeta>>('metadata/minigames').then((data) => {
      const list = Object.entries(data ?? {}).map(([id, m]) => ({
        id,
        name: m.name ?? '',
        userID: m.userID ?? '',
        mechanic: m.mechanic ?? '',
        isPublic: m.isPublic === true,
        timestamp: m.timestamp ?? '',
        sourceId: m.sourceId ?? '',
        isOfficial: m.isOfficial === true,
      }))
      // The originals first, then the newest
      list.sort((a, b) => Number(b.isOfficial) - Number(a.isOfficial) || b.timestamp.localeCompare(a.timestamp))
      setRows(list)
      setLoading(false)
    })
  }

  useEffect(load, [])

  async function setPublic(row: Row, isPublic: boolean) {
    await writePath(`metadata/minigames/${row.id}/isPublic`, isPublic)
    load()
  }

  async function copy(id: string, url: string) {
    await navigator.clipboard.writeText(url)
    setCopied(id)
    window.setTimeout(() => setCopied((c) => (c === id ? null : c)), 1500)
  }

  const names = new Map(rows.map((r) => [r.id, r.name]))

  return (
    <div>
      <h1 className="page-title">Minijuegos</h1>
      <p>
        Los oficiales se suben desde Unity (componente del minijuego, click derecho → "Subir como minijuego oficial",
        en Play y con la cuenta de Fabulab). Cada usuario hace su versión copiándolos en el editor web del sitio. Para
        jugarlos hay que tener cuenta en el sitio.
      </p>

      {!SITE_URL && (
        <div className="empty-state">
          Falta configurar <code>VITE_SITE_URL</code> en <code>web/admin/.env</code> con la dirección del sitio público
          para armar los links.
        </div>
      )}

      {loading ? (
        <p>Cargando…</p>
      ) : rows.length === 0 ? (
        <div className="empty-state">Todavía no hay minijuegos.</div>
      ) : (
        <table>
          <thead>
            <tr>
              <th>Nombre</th>
              <th>Mecánica</th>
              <th>Autor</th>
              <th>Origen</th>
              <th>Público</th>
              <th>Link para compartir</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {rows.map((row) => {
              const url = siteUrl('minigame', row.id)
              return (
                <tr key={row.id}>
                  <td>
                    {row.name || 'Sin nombre'}
                    <div className="id-cell">{row.id}</div>
                  </td>
                  <td>{row.mechanic}</td>
                  <td className="id-cell">{row.userID}</td>
                  <td>
                    {row.isOfficial ? <strong>Oficial</strong> : row.sourceId ? names.get(row.sourceId) || row.sourceId : '—'}
                  </td>
                  <td>
                    <input type="checkbox" checked={row.isPublic} onChange={(e) => setPublic(row, e.target.checked)} />
                  </td>
                  <td>{url ? <code>{url}</code> : <code>?minigame={row.id}</code>}</td>
                  <td style={{ whiteSpace: 'nowrap' }}>
                    {url && (
                      <>
                        <a className="btn primary" href={url} target="_blank" rel="noreferrer">
                          Jugar
                        </a>{' '}
                        <button className="btn" onClick={() => copy(row.id, url)}>
                          {copied === row.id ? 'Copiado' : 'Copiar link'}
                        </button>
                      </>
                    )}
                  </td>
                </tr>
              )
            })}
          </tbody>
        </table>
      )}
    </div>
  )
}
