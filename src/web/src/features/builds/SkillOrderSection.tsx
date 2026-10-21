import { fetchAbilities } from '../../api/buildData'
import { useApi } from '../../api/useApi'
import { assign, canAssign, maxLevel, maxOrder, skillKeys, type SkillOrder } from './skillOrder'

interface Props {
  championId: string
  order: SkillOrder
  onChange: (order: SkillOrder) => void
}

const loadAbilities = (signal: AbortSignal, championId: string) => fetchAbilities(championId, signal)

const levels = Array.from({ length: maxLevel }, (_, i) => i + 1)

export function SkillOrderSection({ championId, order, onChange }: Props) {
  const abilities = useApi(loadAbilities, championId)
  const maxed = maxOrder(order)

  return (
    <section className="editor-section">
      <h3>
        Kolejność skilli {maxed.length > 0 && <span className="muted">({maxed.join(' > ')})</span>}
      </h3>
      <div className="skill-grid-scroll">
        <table className="skill-grid">
          <thead>
            <tr>
              <th />
              {levels.map((level) => (
                <th key={level}>{level}</th>
              ))}
            </tr>
          </thead>
          <tbody>
            {skillKeys.map((key) => {
              const ability = abilities.status === 'ready' ? abilities.data.find((a) => a.key === key) : undefined
              return (
                <tr key={key}>
                  <th title={ability ? `${ability.name}\n\n${ability.description}` : key}>
                    {ability ? <img src={ability.iconUrl} alt={key} width={28} height={28} /> : key}
                  </th>
                  {levels.map((level) => {
                    const chosen = order[level - 1] === key
                    return (
                      <td key={level}>
                        <button
                          type="button"
                          className={chosen ? 'skill-cell chosen' : 'skill-cell'}
                          disabled={!chosen && !canAssign(order, level, key)}
                          onClick={() => onChange(assign(order, level, key))}
                          aria-label={`${key} na poziomie ${level}`}
                          aria-pressed={chosen}
                        >
                          {chosen ? key : ''}
                        </button>
                      </td>
                    )
                  })}
                </tr>
              )
            })}
          </tbody>
        </table>
      </div>
    </section>
  )
}
