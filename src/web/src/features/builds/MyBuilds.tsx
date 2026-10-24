import { fetchMyBuilds } from '../../api/builds'
import { useApi } from '../../api/useApi'
import { BuildList } from './BuildList'

export function MyBuilds() {
  const builds = useApi(fetchMyBuilds)

  if (builds.status === 'loading') return <p className="muted">Ładuję Twoje buildy…</p>
  if (builds.status === 'error') return <p className="error">Nie udało się pobrać Twoich buildów.</p>
  if (builds.data.length === 0)
    return <p className="muted">Nie masz jeszcze żadnego buildu. Trevor czeka, aż coś sknocisz.</p>

  return <BuildList builds={builds.data} />
}
