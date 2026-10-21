import { useState } from 'react'
import { BrowserRouter, Link, Route, Routes } from 'react-router'
import { completeLogin } from './auth/session'
import { AccountMenu } from './features/account/AccountMenu'
import { CurrentUserProvider } from './features/account/CurrentUserProvider'
import { MainNav } from './features/account/MainNav'
import { AccountPage } from './pages/AccountPage'
import { BuildPage } from './pages/BuildPage'
import { HomePage } from './pages/HomePage'
import { NewBuildPage } from './pages/NewBuildPage'
import { StatsPage } from './pages/StatsPage'

// Before the first render, so the account menu already sees a token that just arrived
const login = completeLogin()

export default function App() {
  const [loginFailed, setLoginFailed] = useState(login.kind === 'error')

  return (
    <BrowserRouter>
      <CurrentUserProvider>
        <header className="header">
          <div>
            <h1>
              <Link to="/" className="home-link">
                Trevor.gg
              </Link>
            </h1>
            <p className="muted">Buildy i statystyki, które Trevor i tak skrytykuje.</p>
          </div>
          <MainNav />
          <AccountMenu />
        </header>
        <main className="main">
          {loginFailed && (
            <p className="banner error" role="alert">
              Logowanie się nie udało. Discord albo Ty coś pomieszaliście, spróbuj jeszcze raz.
              <button type="button" className="link" onClick={() => setLoginFailed(false)}>
                Zamknij
              </button>
            </p>
          )}
          <Routes>
            <Route path="/" element={<HomePage />} />
            <Route path="/nowy/:championId" element={<NewBuildPage />} />
            <Route path="/b/:id" element={<BuildPage />} />
            <Route path="/konto" element={<AccountPage />} />
            <Route path="/statystyki" element={<StatsPage />} />
            <Route path="*" element={<HomePage />} />
          </Routes>
        </main>
      </CurrentUserProvider>
    </BrowserRouter>
  )
}
