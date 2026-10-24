import { lazy, Suspense, useState } from 'react'
import { BrowserRouter, Route, Routes } from 'react-router'
import { completeLogin } from './auth/session'
import { CurrentUserProvider } from './features/account/CurrentUserProvider'
import { LoginErrorBanner } from './features/layout/LoginErrorBanner'
import { SiteFooter } from './features/layout/SiteFooter'
import { SiteHeader } from './features/layout/SiteHeader'
import { HomePage } from './pages/HomePage'

// Every page but the home page is its own file, downloaded on the first visit; the build editor is most of the code
const AccountPage = lazy(() => import('./pages/AccountPage').then((m) => ({ default: m.AccountPage })))
const BuildPage = lazy(() => import('./pages/BuildPage').then((m) => ({ default: m.BuildPage })))
const BuildsPage = lazy(() => import('./pages/BuildsPage').then((m) => ({ default: m.BuildsPage })))
const MatchPage = lazy(() => import('./pages/MatchPage').then((m) => ({ default: m.MatchPage })))
const NewBuildPage = lazy(() => import('./pages/NewBuildPage').then((m) => ({ default: m.NewBuildPage })))
const PlayerPage = lazy(() => import('./pages/PlayerPage').then((m) => ({ default: m.PlayerPage })))
const StatsPage = lazy(() => import('./pages/StatsPage').then((m) => ({ default: m.StatsPage })))

// Before the first render, so the account menu already sees a token that just arrived
const login = completeLogin()

export default function App() {
  const [loginError, setLoginError] = useState(login.kind === 'error' ? login.reason : undefined)

  return (
    <BrowserRouter>
      <CurrentUserProvider>
        <SiteHeader />
        <main className="main">
          {loginError && <LoginErrorBanner reason={loginError} onClose={() => setLoginError(undefined)} />}
          <Suspense fallback={<p className="muted">…</p>}>
            <Routes>
              <Route path="/" element={<HomePage />} />
              <Route path="/buildy" element={<BuildsPage />} />
              <Route path="/nowy/:championId" element={<NewBuildPage />} />
              <Route path="/b/:id" element={<BuildPage />} />
              <Route path="/konto" element={<AccountPage />} />
              <Route path="/statystyki" element={<StatsPage />} />
              <Route path="/gracz/:name/:tag" element={<PlayerPage />} />
              <Route path="/mecz/:matchId" element={<MatchPage />} />
              <Route path="*" element={<HomePage />} />
            </Routes>
          </Suspense>
        </main>
        <SiteFooter />
      </CurrentUserProvider>
    </BrowserRouter>
  )
}
