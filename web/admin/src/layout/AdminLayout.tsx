import { signOut } from 'firebase/auth'
import { NavLink, Outlet, useLocation } from 'react-router-dom'
import { auth } from '../firebase'
import { ErrorBoundary } from '../ErrorBoundary'

const links = [
  { to: '/', label: 'Dashboard', end: true },
  { to: '/moderation/characters', label: 'Personajes' },
  { to: '/moderation/so', label: 'Objetos de escena' },
  { to: '/moderation/stories', label: 'Historias' },
  { to: '/tags', label: 'Tags' },
  { to: '/presets', label: 'Presets' },
  { to: '/games', label: 'Games' },
  { to: '/users', label: 'Usuarios' },
]

export function AdminLayout() {
  const location = useLocation()

  return (
    <div className="admin-shell">
      <aside className="sidebar">
        <h2>Fabulab</h2>
        <nav>
          {links.map((l) => (
            <NavLink key={l.to} to={l.to} end={l.end} className={({ isActive }) => (isActive ? 'active' : '')}>
              {l.label}
            </NavLink>
          ))}
        </nav>
        <button className="signout" onClick={() => signOut(auth)}>
          Cerrar sesión
        </button>
      </aside>
      <main className="content">
        <ErrorBoundary key={location.pathname}>
          <Outlet />
        </ErrorBoundary>
      </main>
    </div>
  )
}
