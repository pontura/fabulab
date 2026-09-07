import { useEffect, useState } from 'react'
import { readPath } from '../../rtdb'

interface Counts {
  users: number
  characters: number
  so: number
  stories: number
  tags: number
  games: number
}

async function countChildren(path: string): Promise<number> {
  const data = await readPath<Record<string, unknown>>(path)
  return data ? Object.keys(data).length : 0
}

export function DashboardPage() {
  const [counts, setCounts] = useState<Counts | null>(null)

  useEffect(() => {
    Promise.all([
      countChildren('users'),
      countChildren('metadata/characters'),
      countChildren('metadata/so'),
      countChildren('metadata/stories'),
      countChildren('tags'),
      countChildren('games'),
    ]).then(([users, characters, so, stories, tags, games]) => {
      setCounts({ users, characters, so, stories, tags, games })
    })
  }, [])

  return (
    <div>
      <h1 className="page-title">Dashboard</h1>
      {!counts ? (
        <p>Cargando…</p>
      ) : (
        <div className="cards-grid">
          <div className="stat-card">
            <div className="num">{counts.users}</div>
            <div className="label">Usuarios</div>
          </div>
          <div className="stat-card">
            <div className="num">{counts.characters}</div>
            <div className="label">Personajes</div>
          </div>
          <div className="stat-card">
            <div className="num">{counts.so}</div>
            <div className="label">Objetos de escena</div>
          </div>
          <div className="stat-card">
            <div className="num">{counts.stories}</div>
            <div className="label">Historias</div>
          </div>
          <div className="stat-card">
            <div className="num">{counts.tags}</div>
            <div className="label">Tags</div>
          </div>
          <div className="stat-card">
            <div className="num">{counts.games}</div>
            <div className="label">Games</div>
          </div>
        </div>
      )}
    </div>
  )
}
