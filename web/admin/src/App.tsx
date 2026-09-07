import { Navigate, Route, Routes } from 'react-router-dom'
import { RequireAuth } from './auth/RequireAuth'
import { AdminLayout } from './layout/AdminLayout'
import { DashboardPage } from './features/dashboard/DashboardPage'
import { ModerationListPage } from './features/moderation/ModerationListPage'
import { ModerationDetailPage } from './features/moderation/ModerationDetailPage'
import { TagsPage } from './features/tags/TagsPage'
import { PresetsPage } from './features/presets/PresetsPage'
import { GamesPage } from './features/games/GamesPage'
import { GameEditPage } from './features/games/GameEditPage'
import { UsersPage } from './features/users/UsersPage'
import { UserDetailPage } from './features/users/UserDetailPage'

export function App() {
  return (
    <RequireAuth>
      <Routes>
        <Route element={<AdminLayout />}>
          <Route index element={<DashboardPage />} />
          <Route path="moderation/:kind" element={<ModerationListPage />} />
          <Route path="moderation/:kind/:id" element={<ModerationDetailPage />} />
          <Route path="tags" element={<TagsPage />} />
          <Route path="presets" element={<PresetsPage />} />
          <Route path="games" element={<GamesPage />} />
          <Route path="games/:id" element={<GameEditPage />} />
          <Route path="users" element={<UsersPage />} />
          <Route path="users/:uid" element={<UserDetailPage />} />
          <Route path="*" element={<Navigate to="/" replace />} />
        </Route>
      </Routes>
    </RequireAuth>
  )
}
