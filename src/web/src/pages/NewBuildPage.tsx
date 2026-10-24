import { Link, useParams } from 'react-router'
import { fetchChampions } from '../api/champions'
import { useApi } from '../api/useApi'
import { BuildEditor } from '../features/builds/BuildEditor'
import { buildsPath } from '../features/builds/buildLinks'

export function NewBuildPage() {
  const { championId = '' } = useParams()
  const champions = useApi(fetchChampions)

  if (champions.status === 'loading') return <p className="muted">Ładuję…</p>
  if (champions.status === 'error') return <p className="error">Nie udało się pobrać championów.</p>

  const champion = champions.data.find((c) => c.id === championId)
  if (!champion)
    return (
      <p className="error">
        Nie ma takiego championa. <Link to={buildsPath}>Wybierz z listy</Link>.
      </p>
    )

  // key: another champion starts another build
  return <BuildEditor key={champion.id} champion={champion} />
}
