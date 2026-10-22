import { useState } from 'react'
import { fetchAbilities } from '../../api/buildData'
import { useApi } from '../../api/useApi'
import { Tooltip } from '../tooltip/Tooltip'
import {
  basicSkills,
  orderFromPriority,
  priorityOf,
  togglePriority,
  type SkillKey,
  type SkillOrder,
} from './skillOrder'

interface Props {
  championId: string
  order: SkillOrder
  onChange: (order: SkillOrder) => void
}

const loadAbilities = (signal: AbortSignal, championId: string) => fetchAbilities(championId, signal)

// Pick the max order (Q > E > W); the level-by-level order follows from it and is shown below
export function SkillOrderSection({ championId, order, onChange }: Props) {
  const abilities = useApi(loadAbilities, championId)
  // A first pick isn't a whole order yet, so it waits here until the build can take it
  const [pending, setPending] = useState<SkillKey[]>([])
  const saved = priorityOf(order)
  const priority = saved.length === basicSkills.length ? saved : pending
  const abilityOf = (key: string) =>
    abilities.status === 'ready' ? abilities.data.find((a) => a.key === key) : undefined

  const pick = (key: SkillKey) => {
    const next = togglePriority(priority, key)
    setPending(next)
    onChange(orderFromPriority(next))
  }
  const complete = priority.length === basicSkills.length

  return (
    <section className="editor-section">
      <h3>Kolejność maksowania {complete && <span className="muted">({priority.join(' > ')})</span>}</h3>
      <p className="muted small">Klikaj umiejętności od tej maksowanej najpierw. Ult wchodzi sam na 6, 11 i 16.</p>

      <div className="max-order">
        {basicSkills.map((key) => {
          const ability = abilityOf(key)
          const rank = priority.indexOf(key)
          const button = (
            <button
              type="button"
              className={rank >= 0 ? 'max-pick chosen' : 'max-pick'}
              onClick={() => pick(key)}
              aria-pressed={rank >= 0}
              aria-label={ability ? `${key}: ${ability.name}` : key}
            >
              {ability ? (
                <img src={ability.iconUrl} alt="" width={48} height={48} />
              ) : (
                <span className="max-key">{key}</span>
              )}
              <span className="max-label">{key}</span>
              {rank >= 0 && <span className="max-rank">{rank + 1}</span>}
            </button>
          )
          return ability ? (
            <Tooltip key={key} title={`${key} · ${ability.name}`} text={ability.description}>
              {button}
            </Tooltip>
          ) : (
            <span key={key}>{button}</span>
          )
        })}
      </div>

      {order.some((k) => k !== null) && (
        <ol className="level-strip" aria-label="Punkty na kolejnych poziomach">
          {order.map((key, index) => (
            <li key={index} className={key === 'R' ? 'ultimate' : undefined}>
              <span className="muted">{index + 1}</span>
              <strong>{key ?? '·'}</strong>
            </li>
          ))}
        </ol>
      )}
    </section>
  )
}
