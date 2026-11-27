import { createContext, useContext } from 'react'
import type { EditorRequest } from '../editor/EditorOverlay'

// What any card of the home can do, without passing it through every section
interface HomeActions {
  // The signed in user. null: a guest
  uid: string | null
  // Opens a work in the editor (a guest is asked to sign in first). One of another user is saved as a new one
  openEditor: (request: EditorRequest) => void
}

export const HomeActionsContext = createContext<HomeActions>({ uid: null, openEditor: () => {} })

export function useHomeActions(): HomeActions {
  return useContext(HomeActionsContext)
}
