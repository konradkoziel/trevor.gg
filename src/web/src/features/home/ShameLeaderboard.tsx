import { Link } from 'react-router'
import { fetchLeaderboard } from '../../api/home'
import { useApi } from '../../api/useApi'
import { RankBadge } from '../ranks/RankBadge'
import { formatWhole } from '../stats/format'

export function ShameLeaderboard() {
  const board = useApi(fetchLeaderboard)

  return (
    <section className="card">
      <h2 className="page-title">Ranking zmarnowanego czasu</h2>
      {board.status === 'loading' && <p className="muted">Liczę stracone godziny…</p>}
      {board.status === 'error' && <p className="muted">Ranking chwilowo niedostępny. Riot też potrzebuje przerwy.</p>}
      {board.status === 'ready' && board.data.length === 0 && (
        <p className="muted">
          Nikt jeszcze nie podpiął konta. <Link to="/konto">Bądź pierwszym, który się przyzna</Link>.
        </p>
      )}
      {board.status === 'ready' && board.data.length > 0 && (
        <ol className="leaderboard">
          {board.data.map((e, i) => (
            <li key={e.riotId}>
              <span className="place">{i + 1}.</span>
              {e.profileIconUrl && <img src={e.profileIconUrl} alt="" width={36} height={36} />}
              <div className="leader-main">
                <span className="leader-name">
                  <strong>{e.riotId}</strong>
                  <RankBadge rank={e.rank} compact />
                </span>
                <span className="muted">
                  poziom {e.level}
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
      <p className="muted small">
        Riot nie mówi, ile godzin przegrałeś, więc liczą się punkty maestrii i poziom konta. Wniosek i tak ten sam.
      </p>
    </section>
  )
}
