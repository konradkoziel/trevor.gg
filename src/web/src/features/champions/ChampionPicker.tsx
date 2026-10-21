import { useState } from 'react'
import type { Champion } from '../../api/champions'
import { fetchChampions } from '../../api/champions'
import { useApi } from '../../api/useApi'
import { filterChampions } from './filterChampions'

interface Props {
  onSelect: (champion: Champion) => void
}

export function ChampionPicker({ onSelect }: Props) {
  const state = useApi(fetchChampions)
  const [query, setQuery] = useState('')

  if (state.status === 'loading') return <p className="muted">Ładuję championów…</p>
  if (state.status === 'error')
    return <p className="error">Nie udało się pobrać championów. Riot znowu coś zepsuł, odśwież za chwilę.</p>

  const champions = filterChampions(state.data, query)

  return (
    <section className="picker">
      <input
        className="search"
        type="search"
        placeholder="Szukaj championa…"
        value={query}
        onChange={(e) => setQuery(e.target.value)}
        autoFocus
      />
      {champions.length === 0 ? (
        <p className="muted">Nie ma takiego championa. Może wymyśl własnego.</p>
      ) : (
        <ul className="grid">
          {champions.map((c) => (
            <li key={c.id}>
              <button
                type="button"
                className="champion"
                onClick={() => onSelect(c)}
                title={`${c.name}, ${c.title}`}
              >
                <img src={c.iconUrl} alt="" loading="lazy" width={64} height={64} />
                <span>{c.name}</span>
              </button>
            </li>
          ))}
        </ul>
      )}
    </section>
  )
}
