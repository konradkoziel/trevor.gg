import { fetchMatch, type MatchDetail, type MatchIcon, type MatchPlayer, type MatchTeam } from '../../api/matches'
import { useApi } from '../../api/useApi'
import { Tooltip } from '../tooltip/Tooltip'
import { formatDecimal, formatThousands, kdaRatio } from './format'

// Riot's id of the blue side
const BlueTeam = 100
const SecondsPerMinute = 60

interface Props {
  matchId: string
  // Whose stats page this is; their row is highlighted
  riotId?: string
}

// Loads one game for the expanded row of "Ostatnie gry"
export function MatchDetails({ matchId, riotId }: Props) {
  const match = useApi(fetchMatch, matchId)

  if (match.status === 'loading') return <p className="muted match-loading">Ściągam mecz od Riota…</p>
  if (match.status === 'error')
    return <p className="error match-loading">Riot nie oddał tego meczu. Spróbuj za chwilę.</p>
  return <MatchTeams match={match.data} riotId={riotId} />
}

// Both teams with everyone's score, damage, gold and final build
export function MatchTeams({ match, riotId }: { match: MatchDetail; riotId?: string }) {
  const players = match.teams.flatMap((t) => t.players)
  const topDamage = Math.max(...players.map((p) => p.damage), 1)
  const minutes = Math.max(match.durationSeconds / SecondsPerMinute, 1)

  return (
    <div className="match-details">
      {match.teams.map((team) => (
        <TeamTable key={team.teamId} team={team} riotId={riotId} topDamage={topDamage} minutes={minutes} />
      ))}
    </div>
  )
}

interface TeamProps {
  team: MatchTeam
  riotId?: string
  topDamage: number
  minutes: number
}

function TeamTable({ team, riotId, topDamage, minutes }: TeamProps) {
  return (
    <section className={team.win ? 'match-team win' : 'match-team loss'}>
      <header className="match-team-header">
        <strong>{team.win ? 'Wygrana' : 'Przegrana'}</strong>
        <span className="muted">{team.teamId === BlueTeam ? 'Niebieska drużyna' : 'Czerwona drużyna'}</span>
        <span className="match-team-totals">
          {team.kills} / {team.deaths} / {team.assists} · {formatThousands(team.gold)} golda
        </span>
      </header>
      <ul className="match-players">
        {team.players.map((p, i) => (
          <PlayerRow
            key={`${p.riotId}-${i}`}
            player={p}
            isOwner={!!riotId && p.riotId?.toLowerCase() === riotId.toLowerCase()}
            topDamage={topDamage}
            minutes={minutes}
          />
        ))}
      </ul>
    </section>
  )
}

interface PlayerProps {
  player: MatchPlayer
  isOwner: boolean
  topDamage: number
  minutes: number
}

function PlayerRow({ player: p, isOwner, topDamage, minutes }: PlayerProps) {
  return (
    <li className={isOwner ? 'match-player owner' : 'match-player'}>
      <span className="match-champion">
        {p.championIconUrl && <img src={p.championIconUrl} alt={p.championName} width={32} height={32} />}
        <span className="match-level">{p.championLevel}</span>
      </span>
      <span className="match-loadout">
        {p.spells.map((s) => (
          <Icon key={s.name} icon={s} size={16} />
        ))}
        {p.keystone && <Icon icon={p.keystone} size={16} round />}
        {p.secondaryTree && <Icon icon={p.secondaryTree} size={16} round />}
      </span>
      <span className="match-name">
        <strong>{p.riotId ?? p.championName}</strong>
        <span className="muted">{p.championName}</span>
      </span>
      <span className="match-kda">
        <strong>
          {p.kills} / <span className="deaths">{p.deaths}</span> / {p.assists}
        </strong>
        <span className="muted">{formatDecimal(kdaRatio(p.kills, p.deaths, p.assists))} KDA</span>
      </span>
      <span className="match-damage" title={`${p.damage} obrażeń w championów`}>
        <span>{formatThousands(p.damage)}</span>
        <span className="damage-bar" aria-hidden="true">
          <span style={{ width: `${(100 * p.damage) / topDamage}%` }} />
        </span>
      </span>
      <span className="match-cs muted">
        {p.creepScore} CS ({formatDecimal(p.creepScore / minutes)})
      </span>
      <span className="match-items">
        {p.items.map((item, i) =>
          item ? <Icon key={i} icon={item} size={24} /> : <span key={i} className="item-empty" />,
        )}
      </span>
    </li>
  )
}

function Icon({ icon, size, round = false }: { icon: MatchIcon; size: number; round?: boolean }) {
  if (!icon.iconUrl) return null
  const img = (
    <img
      src={icon.iconUrl}
      alt={icon.name}
      width={size}
      height={size}
      className={round ? 'round' : undefined}
      tabIndex={icon.name ? 0 : undefined}
    />
  )
  return icon.name ? <Tooltip title={icon.name}>{img}</Tooltip> : img
}
