import type { ReactNode } from 'react'
import { useAuth } from './useAuth'
import { LoginPage } from './LoginPage'

export function RequireAuth({ children }: { children: ReactNode }) {
  const { user, loading } = useAuth()

  if (loading) return <div className="page-center">Cargando…</div>
  if (!user) return <LoginPage />
  return <>{children}</>
}
