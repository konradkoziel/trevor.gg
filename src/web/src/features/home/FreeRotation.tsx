import { Link } from 'react-router'
import { fetchRotation } from '../../api/home'
import { useApi } from '../../api/useApi'
import { championBuildsPath } from '../builds/buildLinks'

export function FreeRotation() {
  const rotation = useApi(fetchRotation)
  // No Riot key or Riot down: the home page just goes without it
  if (rotation.status !== 'ready' || rotation.data.length === 0) return null

  return (
    <section className="home-section">
      <h2 className="page-title">Darmowa rotacja tygodnia</h2>
      <ul className="rotation">
        {rotation.data.map((c) => (
          <li key={c.id}>
            <Link to={championBuildsPath(c.id)} title={`${c.name}, ${c.title}`}>
              <img src={c.iconUrl} alt="" width={48} height={48} loading="lazy" />
              <span>{c.name}</span>
            </Link>
          </li>
        ))}
      </ul>
    </section>
  )
}
