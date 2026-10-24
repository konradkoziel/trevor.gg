import { fetchRecentBuilds } from '../../api/home'
import { useApi } from '../../api/useApi'
import { TeamBuildList } from '../builds/TeamBuildList'

export function RecentBuilds() {
  const builds = useApi(fetchRecentBuilds)

  if (builds.status !== 'ready' || builds.data.length === 0) return null

  return (
    <section className="home-section">
      <h2 className="page-title">Najnowsze buildy ekipy</h2>
      <TeamBuildList builds={builds.data} />
    </section>
  )
}
