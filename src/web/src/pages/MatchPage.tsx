import { useParams } from 'react-router'
import { errorText } from '../api/errorText'
import { fetchMatch } from '../api/matches'
import { useApi } from '../api/useApi'
import { LoginLink } from '../features/account/LoginLink'
import { useCurrentUser } from '../features/account/useCurrentUser'
import { MatchTeams } from '../features/stats/MatchDetails'
import { formatDuration } from '../features/stats/format'

const dateFormat = new Intl.DateTimeFormat('pl-PL', { dateStyle: 'medium', timeStyle: 'short' })

// /mecz/:matchId: one game with both teams, the page Trevor's match reports link to
export function MatchPage() {
  const { matchId = '' } = useParams()
  const { state } = useCurrentUser()

  if (state.status === 'anonymous')
    return (
      <section className="panel">
        <p>Mecze są dla zalogowanych. Zaloguj się, to Trevor pokaże, kto zawalił.</p>
        <LoginLink />
      </section>
    )
  return <Match matchId={matchId} />
}

function Match({ matchId }: { matchId: string }) {
  const match = useApi(fetchMatch, matchId)

  if (match.status === 'loading') return <p className="muted">Ściągam mecz od Riota…</p>
  if (match.status === 'error')
    return (
      <section className="panel">
        <p className="error">{errorText(match.error, 'Nie ma takiego meczu albo Riot o nim zapomniał.')}</p>
      </section>
    )

  const m = match.data
  return (
    <>
      <header className="player-header">
        <h2 className="page-title">{m.queue}</h2>
        <span className="muted">
          {formatDuration(m.durationSeconds)} · {dateFormat.format(new Date(m.playedAt))}
        </span>
      </header>
      <MatchTeams match={m} />
    </>
  )
}
