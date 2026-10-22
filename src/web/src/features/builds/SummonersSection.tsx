import type { SummonerSpell } from '../../api/buildData'
import { Tooltip } from '../tooltip/Tooltip'

interface Props {
  spells: SummonerSpell[]
  selected: string[]
  // Shared build of someone else: only the chosen spells
  readOnly?: boolean
  onToggle: (spellId: string) => void
}

export function SummonersSection({ spells, selected, readOnly = false, onToggle }: Props) {
  const shown = readOnly ? spells.filter((s) => selected.includes(s.id)) : spells

  return (
    <section className="editor-section">
      <h3>Czary przywoływacza</h3>
      <div className="rune-row">
        {shown.map((spell) => (
          <Tooltip key={spell.id} title={spell.name} text={spell.description}>
            <button
              type="button"
              className={selected.includes(spell.id) ? 'rune selected' : 'rune'}
              onClick={() => onToggle(spell.id)}
              aria-pressed={selected.includes(spell.id)}
            >
              <img src={spell.iconUrl} alt={spell.name} width={40} height={40} />
            </button>
          </Tooltip>
        ))}
      </div>
    </section>
  )
}
