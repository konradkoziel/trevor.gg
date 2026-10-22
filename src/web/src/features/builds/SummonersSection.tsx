import { useState } from 'react'
import type { SummonerSpell } from '../../api/buildData'
import { maxSummonerSpells } from './build'
import { Tooltip } from '../tooltip/Tooltip'

interface Props {
  spells: SummonerSpell[]
  selected: string[]
  // Shared build of someone else: the two slots only
  readOnly?: boolean
  onPick: (slot: number, spellId: string) => void
  onClear: (slot: number) => void
}

// Two slots like the game's client: click one, pick its spell from the list underneath
export function SummonersSection({ spells, selected, readOnly = false, onPick, onClear }: Props) {
  const [openSlot, setOpenSlot] = useState<number | null>(null)
  const byId = new Map(spells.map((s) => [s.id, s]))
  const pick = (spellId: string) => {
    if (openSlot === null) return
    onPick(openSlot, spellId)
    setOpenSlot(null)
  }

  return (
    <section className="editor-section">
      <h3>Czary przywoływacza</h3>
      <div className="spell-slots">
        {Array.from({ length: maxSummonerSpells }, (_, slot) => {
          const spell = byId.get(selected[slot] ?? '')
          const icon = spell ? (
            <img src={spell.iconUrl} alt={spell.name} width={52} height={52} />
          ) : (
            <span>{slot + 1}</span>
          )
          if (readOnly)
            return spell ? (
              <Tooltip key={slot} title={spell.name} text={spell.description}>
                <span className="spell-slot filled" tabIndex={0}>
                  {icon}
                </span>
              </Tooltip>
            ) : null
          return (
            <div key={slot} className="spell-slot-wrap">
              <button
                type="button"
                className={['spell-slot', spell && 'filled', openSlot === slot && 'open'].filter(Boolean).join(' ')}
                onClick={() => setOpenSlot(openSlot === slot ? null : slot)}
                aria-expanded={openSlot === slot}
                aria-label={spell ? `${spell.name}, zmień` : `Wybierz czar ${slot + 1}`}
              >
                {icon}
              </button>
              {spell && (
                <button type="button" className="spell-clear" aria-label="Usuń" onClick={() => onClear(slot)}>
                  ×
                </button>
              )}
            </div>
          )
        })}
        {!readOnly && openSlot === null && selected.length < maxSummonerSpells && (
          <p className="muted">Kliknij slot i wybierz czar.</p>
        )}
      </div>

      {openSlot !== null && (
        <ul className="spell-picker">
          {spells.map((spell) => (
            <li key={spell.id}>
              <Tooltip title={spell.name} text={spell.description}>
                <button
                  type="button"
                  className={selected.includes(spell.id) ? 'spell-option chosen' : 'spell-option'}
                  onClick={() => pick(spell.id)}
                >
                  <img src={spell.iconUrl} alt="" width={40} height={40} />
                  <span>{spell.name}</span>
                </button>
              </Tooltip>
            </li>
          ))}
        </ul>
      )}
    </section>
  )
}
