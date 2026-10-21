import { useMemo, useState } from 'react'
import type { Item } from '../../api/buildData'
import { maxItems } from './build'
import { filterItems } from './filterItems'

interface Props {
  items: Item[]
  selected: string[]
  onAdd: (itemId: string) => void
  onRemove: (index: number) => void
  onMove: (index: number, offset: -1 | 1) => void
}

export function ItemsSection({ items, selected, onAdd, onRemove, onMove }: Props) {
  const [query, setQuery] = useState('')
  const [finishedOnly, setFinishedOnly] = useState(true)
  const byId = useMemo(() => new Map(items.map((i) => [i.id, i])), [items])
  const shown = filterItems(items, query, finishedOnly)
  const full = selected.length >= maxItems

  return (
    <section className="editor-section">
      <h3>Itemy</h3>
      <ol className="item-slots">
        {Array.from({ length: maxItems }, (_, index) => {
          const item = byId.get(selected[index] ?? '')
          return (
            <li key={index} className={item ? 'slot filled' : 'slot'}>
              {item ? (
                <>
                  <img src={item.iconUrl} alt={item.name} title={item.name} width={48} height={48} />
                  <div className="slot-actions">
                    <button type="button" aria-label="Wcześniej" onClick={() => onMove(index, -1)} disabled={index === 0}>
                      ‹
                    </button>
                    <button type="button" aria-label="Usuń" onClick={() => onRemove(index)}>
                      ×
                    </button>
                    <button
                      type="button"
                      aria-label="Później"
                      onClick={() => onMove(index, 1)}
                      disabled={index === selected.length - 1}
                    >
                      ›
                    </button>
                  </div>
                </>
              ) : (
                <span className="muted">{index + 1}</span>
              )}
            </li>
          )
        })}
      </ol>

      <div className="item-filters">
        <input
          className="search"
          type="search"
          placeholder="Szukaj itemu…"
          value={query}
          onChange={(e) => setQuery(e.target.value)}
        />
        <label className="checkbox">
          <input type="checkbox" checked={finishedOnly} onChange={(e) => setFinishedOnly(e.target.checked)} />
          Tylko gotowe
        </label>
      </div>
      {full && <p className="muted">Sześć slotów i ani jednego więcej. Usuń coś, jeśli chcesz kupić kolejną zabawkę.</p>}
      <ul className="icon-grid">
        {shown.map((item) => (
          <li key={item.id}>
            <button
              type="button"
              className="icon-button"
              onClick={() => onAdd(item.id)}
              disabled={full}
              title={`${item.name} (${item.totalGold} g)\n\n${item.description}`}
            >
              <img src={item.iconUrl} alt="" loading="lazy" width={40} height={40} />
              <span>{item.name}</span>
              <span className="gold">{item.totalGold} g</span>
            </button>
          </li>
        ))}
      </ul>
    </section>
  )
}
