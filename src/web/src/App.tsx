import { useState } from 'react'
import type { Champion } from './api/champions'
import { completeLogin } from './auth/session'
import { AccountMenu } from './features/account/AccountMenu'
import { ChampionPicker } from './features/champions/ChampionPicker'

// Before the first render, so the account menu already sees a token that just arrived
const login = completeLogin()

export default function App() {
  const [champion, setChampion] = useState<Champion>()
  const [loginFailed, setLoginFailed] = useState(login.kind === 'error')

  return (
    <>
      <header className="header">
        <div>
          <h1>Trevor.gg</h1>
          <p className="muted">Buildy i statystyki, które Trevor i tak skrytykuje.</p>
        </div>
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
        {champion && (
          <section className="selected-panel">
            <img src={champion.iconUrl} alt="" width={96} height={96} />
            <div>
              <h2>{champion.name}</h2>
              <p className="muted">{champion.title}</p>
              <p>Kreator buildów już wkrótce. Na razie zapamiętaj: full damage i modlitwa.</p>
            </div>
          </section>
        )}
        <ChampionPicker selectedId={champion?.id} onSelect={setChampion} />
      </main>
    </>
  )
}
