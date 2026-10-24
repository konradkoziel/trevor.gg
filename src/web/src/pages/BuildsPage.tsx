import { useNavigate } from 'react-router'
import { hasSession, useCurrentUser } from '../features/account/useCurrentUser'
import { newBuildPath } from '../features/builds/buildLinks'
import { MyBuilds } from '../features/builds/MyBuilds'
import { ChampionPicker } from '../features/champions/ChampionPicker'

export function BuildsPage() {
  const { state } = useCurrentUser()
  const navigate = useNavigate()

  return (
    <>
      {hasSession(state) && (
        <section className="home-section">
          <h2 className="page-title">Moje buildy</h2>
          <MyBuilds />
        </section>
      )}
      <section className="home-section">
        <h2 className="page-title">Nowy build: wybierz championa</h2>
        <ChampionPicker onSelect={(c) => navigate(newBuildPath(c.id))} />
      </section>
    </>
  )
}
