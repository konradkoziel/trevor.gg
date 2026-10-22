import { useState } from 'react'
import { BrowserRouter, Route, Routes } from 'react-router'
import { completeLogin } from './auth/session'
import { CurrentUserProvider } from './features/account/CurrentUserProvider'
import { LoginErrorBanner } from './features/layout/LoginErrorBanner'
import { SiteFooter } from './features/layout/SiteFooter'
import { SiteHeader } from './features/layout/SiteHeader'
import { AccountPage } from './pages/AccountPage'
import { BuildPage } from './pages/BuildPage'
import { BuildsPage } from './pages/BuildsPage'
import { HomePage } from './pages/HomePage'
import { NewBuildPage } from './pages/NewBuildPage'
import { StatsPage } from './pages/StatsPage'

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
          <Routes>
            <Route path="/" element={<HomePage />} />
            <Route path="/buildy" element={<BuildsPage />} />
            <Route path="/nowy/:championId" element={<NewBuildPage />} />
            <Route path="/b/:id" element={<BuildPage />} />
            <Route path="/konto" element={<AccountPage />} />
            <Route path="/statystyki" element={<StatsPage />} />
            <Route path="*" element={<HomePage />} />
          </Routes>
        </main>
        <SiteFooter />
      </CurrentUserProvider>
    </BrowserRouter>
  )
}
