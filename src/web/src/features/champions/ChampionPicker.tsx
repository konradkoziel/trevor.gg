import { useState } from 'react'
import type { Champion } from '../../api/champions'
import { filterChampions } from './filterChampions'
import { useChampions } from './useChampions'

interface Props {
  selectedId?: string
  onSelect: (champion: Champion) => void
}

export function ChampionPicker({ selectedId, onSelect }: Props) {
  const state = useChampions()
  const [query, setQuery] = useState('')

  if (state.status === 'loading') return <p className="muted">Ładuję championów…</p>
  if (state.status === 'error')
    return <p className="error">Nie udało się pobrać championów. Riot znowu coś zepsuł, odśwież za chwilę.</p>

  const champions = filterChampions(state.champions, query)

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
                className={c.id === selectedId ? 'champion selected' : 'champion'}
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
