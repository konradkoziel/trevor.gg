import { useState } from 'react'
import { Link } from 'react-router'
import { fetchChampions } from '../../api/champions'
import { useApi } from '../../api/useApi'
import { newBuildPath } from '../builds/buildLinks'
import { pickDaily } from './daily'

export function ChampionOfTheDay() {
  const champions = useApi(fetchChampions)
  const [today] = useState(() => new Date())
  if (champions.status !== 'ready') return null

  const champion = pickDaily(champions.data, today, 'champion')
  if (!champion) return null

  return (
    <section className="card champion-of-day" style={{ backgroundImage: `url(${champion.splashUrl})` }}>
      <div className="champion-of-day-text">
        <span className="eyebrow">Champion dnia</span>
        <h2>{champion.name}</h2>
        <p className="muted">{champion.title}</p>
        {champion.blurb && <p className="blurb">{champion.blurb}</p>}
        <Link className="button gold" to={newBuildPath(champion.id)}>
          Zrób mu build
        </Link>
      </div>
    </section>
  )
}
