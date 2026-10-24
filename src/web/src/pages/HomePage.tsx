import { Link } from 'react-router'
import { LoginLink } from '../features/account/LoginLink'
import { hasSession, useCurrentUser } from '../features/account/useCurrentUser'
import { buildsPath } from '../features/builds/buildLinks'
import { ChampionOfTheDay } from '../features/home/ChampionOfTheDay'
import { FreeRotation } from '../features/home/FreeRotation'
import { LastGameCard } from '../features/home/LastGameCard'
import { RecentBuilds } from '../features/home/RecentBuilds'
import { ShameLeaderboard } from '../features/home/ShameLeaderboard'
import { TrevorQuote } from '../features/home/TrevorQuote'

// Players on the home page's leaderboard; "Ekipa" has all of them
const LeaderboardPreview = 5

// Logged in: your last game, the team's ranking and builds first. Otherwise: what the site is and the login button
export function HomePage() {
  const { state } = useCurrentUser()
  return hasSession(state) ? <TeamHome name={state.status === 'loggedIn' ? state.user.name : undefined} /> : <Welcome />
}

function TeamHome({ name }: { name?: string }) {
  return (
    <>
      <section className="greeting">
        <h1 className="greeting-title">{name ? `Siema, ${name}` : 'Siema'}</h1>
        <TrevorQuote />
      </section>

      <div className="home-grid">
        <LastGameCard />
        <ShameLeaderboard limit={LeaderboardPreview} />
      </div>

      <RecentBuilds />

      <div className="home-section">
        <ChampionOfTheDay />
      </div>
      <FreeRotation />
    </>
  )
}

function Welcome() {
  return (
    <>
      <section className="hero">
        <div className="hero-text">
          <span className="eyebrow">Dla ekipy z serwera Trevora</span>
          <h1 className="hero-title">Buildy i statystyki, które Trevor i tak skrytykuje.</h1>
          <TrevorQuote />
        </div>
        <div className="hero-actions">
          <LoginLink />
          <Link className="button secondary" to={buildsPath}>
            Zrób build
          </Link>
        </div>
      </section>

      <div className="home-grid">
        <ChampionOfTheDay />
        <section className="card">
          <h2 className="page-title">Po zalogowaniu</h2>
          <ul className="feature-list">
            <li>Twoje rangi, ostatnie gry i statystyki championów</li>
            <li>Ranking wstydu całej ekipy i profile znajomych</li>
            <li>Buildy ekipy na każdego championa</li>
            <li>Raporty z Twoich meczów na Discordzie</li>
          </ul>
          <p className="muted small">Wchodzą tylko ludzie z serwera Trevora, przez Discorda.</p>
        </section>
      </div>

      <FreeRotation />
    </>
  )
}
