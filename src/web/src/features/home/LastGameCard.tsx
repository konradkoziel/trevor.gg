import { useState } from 'react'
import { Link } from 'react-router'
import { ApiRequestError } from '../../api/client'
import { errorText } from '../../api/errorText'
import { fetchMyStats, type RecentMatch } from '../../api/stats'
import { useApi } from '../../api/useApi'
import { mePath, myAccountsPath } from '../account/accountLinks'
import { Skeleton } from '../layout/Skeleton'
import { RankBadge } from '../ranks/RankBadge'
import { formatAgo, formatDecimal, formatPercent, kdaRatio } from '../stats/format'
import { matchPath } from '../stats/matchLinks'

// The logged-in home page's first card: the main account's rank and last game, or the way to link an account
export function LastGameCard() {
  const stats = useApi(fetchMyStats, null)
  // Fixed for the render, so "2 h temu" doesn't change between renders
  const [now] = useState(() => new Date())

  return (
    <section className="card last-game">
      <h2 className="page-title">Twoja ostatnia gra</h2>
      {stats.status === 'loading' && <Skeleton rows={2} height={56} />}
      {stats.status === 'error' &&
        (stats.error instanceof ApiRequestError && stats.error.status === 404 ? (
          <>
            <p className="muted">Nie masz podpiętego konta LoL, więc Trevor nie ma się z czego śmiać. Jeszcze.</p>
            <Link className="button gold" to={myAccountsPath}>
              Podepnij konto
            </Link>
          </>
        ) : (
          <p className="muted">{errorText(stats.error, 'Riot nie oddał Twoich gier. Spróbuj za chwilę.')}</p>
        ))}
      {stats.status === 'ready' && (
        <>
          <div className="last-game-who">
            <strong>{stats.data.riotId}</strong>
            <RankBadge rank={stats.data.ranks?.[0]} />
          </div>
          {stats.data.recent[0] ? (
            <LastGame game={stats.data.recent[0]} now={now} />
          ) : (
            <p className="muted">Ani jednej gry ostatnio. Podejrzanie zdrowy tryb życia.</p>
          )}
          {stats.data.games > 0 && (
            <p className="muted small">
              Ostatnie {stats.data.games} gier na Summoner&apos;s Rift: {formatPercent(stats.data.winRate)} wygranych,{' '}
              {formatDecimal(stats.data.kda)} KDA
            </p>
          )}
          <Link className="card-more" to={mePath}>
            Wszystkie statystyki →
          </Link>
        </>
      )}
    </section>
  )
}

function LastGame({ game: g, now }: { game: RecentMatch; now: Date }) {
  return (
    <Link to={matchPath(g.matchId)} className={g.win ? 'recent win' : 'recent loss'}>
      {g.iconUrl && <img src={g.iconUrl} alt="" width={40} height={40} />}
      <span className="recent-main">
        <strong>{g.championName}</strong>
        <span className="muted">
          {g.win ? 'Wygrana' : 'Przegrana'} · {g.queue}
        </span>
      </span>
      <span className="recent-score">
        <strong>
          {g.kills} / <span className="deaths">{g.deaths}</span> / {g.assists}
        </strong>
        <span className="muted">{formatDecimal(kdaRatio(g.kills, g.deaths, g.assists))} KDA</span>
      </span>
      <span className="recent-extra muted">{formatAgo(new Date(g.playedAt), now)}</span>
      <span className="recent-toggle" aria-hidden="true">
        ›
      </span>
    </Link>
  )
}
