import { useSession } from './auth/useSession'
import { HomeScreen } from './home/HomeScreen'

// Everyone enters the home: visitors without account as guests, who sign in only to create or edit
export function App() {
  const session = useSession()

  if (!session.ready)
    return (
      <div className="screen">
        <div className="spinner" aria-label="Cargando" />
      </div>
    )

  return <HomeScreen session={session} />
}
