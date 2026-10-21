import { Link, useParams } from 'react-router'
import { fetchBuild } from '../api/builds'
import { fetchChampions } from '../api/champions'
import { useApi } from '../api/useApi'
import { BuildEditor } from '../features/builds/BuildEditor'

export function BuildPage() {
  const { id = '' } = useParams()
  const build = useApi(fetchBuild, id)
  const champions = useApi(fetchChampions)

  if (build.status === 'loading' || champions.status === 'loading') return <p className="muted">Ładuję build…</p>
  if (build.status === 'error' || champions.status === 'error')
    return (
      <p className="error">
        Tego buildu nie ma albo ktoś go usunął ze wstydu. <Link to="/">Strona główna</Link>.
      </p>
    )

  const champion = champions.data.find((c) => c.id === build.data.build.championId)
  if (!champion) return <p className="error">Ten champion zniknął z gry. Build też powinien.</p>

  const { data } = build
  // key: another build in the same tab starts from its own saved state
  return (
    <BuildEditor
      key={data.id}
      champion={champion}
      saved={{ id: data.id, build: data.build, ownerName: data.ownerName, isMine: data.isMine }}
    />
  )
}
