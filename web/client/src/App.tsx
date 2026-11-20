import { AuthScreen } from './auth/AuthScreen'
import { useSession } from './auth/useSession'
import { HomeScreen } from './home/HomeScreen'

export function App() {
  const session = useSession()

  if (session.status === 'loading')
    return <div className="screen"><div className="spinner" aria-label="Cargando" /></div>

  if (session.status === 'signedOut' || !session.user)
    return <AuthScreen onSignedUp={session.setProfile} />

  return <HomeScreen user={session.user} profile={session.profile} />
}
