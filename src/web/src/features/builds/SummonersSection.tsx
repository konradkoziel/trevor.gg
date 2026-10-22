import { useEffect, useRef, useState } from 'react'
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

// Two slots like the game's client: click one, pick its spell from a 3×3 bubble next to it
export function SummonersSection({ spells, selected, readOnly = false, onPick, onClear }: Props) {
  const [openSlot, setOpenSlot] = useState<number | null>(null)
  const slotsRef = useRef<HTMLDivElement>(null)
  const byId = new Map(spells.map((s) => [s.id, s]))
  const pick = (spellId: string) => {
    if (openSlot === null) return
    onPick(openSlot, spellId)
    setOpenSlot(null)
  }

  // The bubble closes on Escape or a click anywhere else
  useEffect(() => {
    if (openSlot === null) return
    const close = (e: Event) => {
      if (e instanceof KeyboardEvent ? e.key === 'Escape' : !slotsRef.current?.contains(e.target as Node))
        setOpenSlot(null)
    }
    document.addEventListener('mousedown', close)
    document.addEventListener('keydown', close)
    return () => {
      document.removeEventListener('mousedown', close)
      document.removeEventListener('keydown', close)
    }
  }, [openSlot])

  return (
    <section className="editor-section">
      <h3>Czary przywoływacza</h3>
      <div className="spell-slots" ref={slotsRef}>
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
              {openSlot === slot && (
                <ul className="spell-picker">
                  {spells.map((option) => (
                    <li key={option.id}>
                      <Tooltip title={option.name} text={option.description}>
                        <button
                          type="button"
                          className={selected.includes(option.id) ? 'spell-option chosen' : 'spell-option'}
                          onClick={() => pick(option.id)}
                          aria-label={option.name}
                        >
                          <img src={option.iconUrl} alt="" width={44} height={44} />
                        </button>
                      </Tooltip>
                    </li>
                  ))}
                </ul>
              )}
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
    </section>
  )
}
