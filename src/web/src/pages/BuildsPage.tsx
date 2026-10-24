import { useNavigate } from 'react-router'
import { championBuildsPath } from '../features/builds/buildLinks'
import { ChampionPicker } from '../features/champions/ChampionPicker'

// /buildy: pick a champion to see the team's builds of it and to make a new one
export function BuildsPage() {
  const navigate = useNavigate()

  return (
    <section>
      <h2 className="page-title">Buildy: wybierz championa</h2>
      <ChampionPicker onSelect={(c) => navigate(championBuildsPath(c.id))} />
    </section>
  )
}
