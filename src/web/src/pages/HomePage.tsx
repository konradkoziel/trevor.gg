import { Link } from 'react-router'
import { LoginLink } from '../features/account/LoginLink'
import { useCurrentUser } from '../features/account/useCurrentUser'
import { ChampionOfTheDay } from '../features/home/ChampionOfTheDay'
import { FreeRotation } from '../features/home/FreeRotation'
import { RecentBuilds } from '../features/home/RecentBuilds'
import { ShameLeaderboard } from '../features/home/ShameLeaderboard'
import { TrevorQuote } from '../features/home/TrevorQuote'

export function HomePage() {
  const { state } = useCurrentUser()
  const loggedIn = state.status === 'loggedIn'

  return (
    <>
      <section className="hero">
        <div className="hero-text">
          <span className="eyebrow">Dla ekipy z serwera Trevora</span>
          <h1 className="hero-title">Buildy i statystyki, które Trevor i tak skrytykuje.</h1>
          <TrevorQuote />
        </div>
        <div className="hero-actions">
          <Link className="button gold" to="/buildy">
            Buildy
          </Link>
          {loggedIn ? (
            <Link className="button secondary" to="/statystyki">
              Moje statystyki
            </Link>
          ) : (
            state.status === 'anonymous' && <LoginLink />
          )}
        </div>
      </section>

      <div className="home-grid">
        <ChampionOfTheDay />
        {loggedIn ? (
          <ShameLeaderboard />
        ) : (
          <section className="card">
            <h2 className="page-title">Ranking zmarnowanego czasu</h2>
            <p className="muted">
              Kto z ekipy przegrał najwięcej życia w LoLa? Zaloguj się, żeby zobaczyć ranking wstydu.
            </p>
          </section>
        )}
      </div>

      <FreeRotation />
      {loggedIn && <RecentBuilds />}
    </>
  )
}
