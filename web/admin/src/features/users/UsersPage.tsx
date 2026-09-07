import { useEffect, useMemo, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { readPath } from '../../rtdb'
import type { UserRecord } from '../../types'

export function UsersPage() {
  const navigate = useNavigate()
  const [users, setUsers] = useState<UserRecord[]>([])
  const [loading, setLoading] = useState(true)
  const [search, setSearch] = useState('')

  useEffect(() => {
    readPath<Record<string, UserRecord>>('users').then((data) => {
      setUsers(data ? Object.values(data) : [])
      setLoading(false)
    })
  }, [])

  const filtered = useMemo(() => {
    const q = search.toLowerCase()
    return users.filter(
      (u) => !q || (u.username ?? '').toLowerCase().includes(q) || (u.email ?? '').toLowerCase().includes(q),
    )
  }, [users, search])

  return (
    <div>
      <h1 className="page-title">Usuarios</h1>
      <div className="toolbar">
        <input placeholder="Buscar por username o email…" value={search} onChange={(e) => setSearch(e.target.value)} />
        <span>{filtered.length} usuarios</span>
      </div>

      {loading ? (
        <p>Cargando…</p>
      ) : (
        <table>
          <thead>
            <tr>
              <th>Username</th>
              <th>Email</th>
              <th>UID</th>
            </tr>
          </thead>
          <tbody>
            {filtered.map((u) => (
              <tr key={u.uid} className="clickable" onClick={() => navigate(`/users/${u.uid}`)}>
                <td>{u.username || '—'}</td>
                <td>{u.email}</td>
                <td>{u.uid}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  )
}
