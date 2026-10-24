import { Link } from 'react-router'
import { fetchLeaderboard } from '../../api/home'
import { useApi } from '../../api/useApi'
import { myAccountsPath, teamPath } from '../account/accountLinks'
import { Skeleton } from '../layout/Skeleton'
import { RankBadge } from '../ranks/RankBadge'
import { playerPath } from '../players/playerLinks'
import { formatWhole } from '../stats/format'

// limit: the home page shows the top few and links to the whole list on "Ekipa"
export function ShameLeaderboard({ limit }: { limit?: number }) {
  const board = useApi(fetchLeaderboard)
  const entries = board.status === 'ready' ? board.data.slice(0, limit) : []
  const cut = board.status === 'ready' && entries.length < board.data.length

  return (
    <section className="card">
      <h2 className="page-title">Ranking zmarnowanego czasu</h2>
      {board.status === 'loading' && <Skeleton rows={limit ?? 5} height={54} />}
      {board.status === 'error' && <p className="muted">Ranking chwilowo niedostępny. Riot też potrzebuje przerwy.</p>}
      {board.status === 'ready' && board.data.length === 0 && (
        <p className="muted">
          Nikt jeszcze nie podpiął konta. <Link to={myAccountsPath}>Bądź pierwszym, który się przyzna</Link>.
        </p>
      )}
      {entries.length > 0 && (
        <ol className="leaderboard">
          {entries.map((e, i) => (
            <li key={e.riotId}>
              <span className="place">{i + 1}.</span>
              {e.profileIconUrl && <img src={e.profileIconUrl} alt="" width={36} height={36} />}
              <div className="leader-main">
                <span className="leader-name">
                  <PlayerName riotId={e.riotId} />
                  <RankBadge rank={e.rank} />
                </span>
                <span className="muted">
                  poziom {e.level}
                  {e.accountCount > 1 && ` · ${e.accountCount} ${e.accountCount >= 5 ? 'kont' : 'konta'}`}
                  {e.topChampionName && ` · main: ${e.topChampionName}`}
                </span>
              </div>
              <div className="leader-points">
                <strong>{formatWhole(e.masteryPoints)}</strong>
                <span className="muted">pkt maestrii</span>
              </div>
            </li>
          ))}
        </ol>
      )}
      {cut && (
        <Link className="card-more" to={teamPath}>
          Cały ranking ekipy →
        </Link>
      )}
      <p className="muted small">
        Riot nie mówi, ile godzin przegrałeś, więc liczą się punkty maestrii i poziom konta. Wniosek i tak ten sam.
      </p>
    </section>
  )
}

function PlayerName({ riotId }: { riotId: string }) {
  const path = playerPath(riotId)
  return <strong>{path ? <Link to={path}>{riotId}</Link> : riotId}</strong>
}
