import { useState } from 'react'
import type { Champion } from './api/champions'
import { ChampionPicker } from './features/champions/ChampionPicker'

export default function App() {
  const [champion, setChampion] = useState<Champion>()

  return (
    <>
      <header className="header">
        <h1>Trevor.gg</h1>
        <p className="muted">Buildy i statystyki, które Trevor i tak skrytykuje.</p>
      </header>
      <main className="main">
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
