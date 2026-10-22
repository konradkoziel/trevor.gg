import { useState, type FormEvent } from 'react'
import { errorText } from '../../api/errorText'
import {
  fetchRiotAccounts,
  linkRiotAccount,
  setMainRiotAccount,
  setRiotAccountTracked,
  unlinkRiotAccount,
  type RiotAccountsView,
} from '../../api/riotAccount'
import { useApi } from '../../api/useApi'

// The user's League accounts: add one by Riot ID, pick the main one (used by /rank, /roast and Statystyki), switch
// match reports on Discord on or off, unlink.
// Only Trevor's server can log in, so there is no ownership check
export function RiotAccountPanel() {
  const loaded = useApi(fetchRiotAccounts)
  // After an action the panel shows the server's answer instead of the first load
  const [changed, setChanged] = useState<RiotAccountsView>()
  const [riotId, setRiotId] = useState('')
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState<string>()

  if (!changed && loaded.status === 'loading') return <p className="muted">Ładuję…</p>
  if (!changed && loaded.status === 'error') return <p className="error">Nie udało się sprawdzić Twoich kont.</p>
  const view = changed ?? (loaded.status === 'ready' ? loaded.data : { accounts: [], maxAccounts: 0 })
  const canAdd = view.accounts.length < view.maxAccounts

  const run = async (action: () => Promise<RiotAccountsView>, fallback: string) => {
    setBusy(true)
    setError(undefined)
    try {
      setChanged(await action())
      return true
    } catch (e) {
      setError(errorText(e, fallback))
      return false
    } finally {
      setBusy(false)
    }
  }

  const link = async (e: FormEvent) => {
    e.preventDefault()
    if (await run(() => linkRiotAccount(riotId), 'Nie udało się podpiąć konta.')) setRiotId('')
  }

  return (
    <section className="panel">
      <h2 className="page-title">Konta League of Legends</h2>

      {view.accounts.length > 0 && (
        <ul className="account-list">
          {view.accounts.map((a) => (
            <li key={a.id}>
              <strong>{a.riotId}</strong>
              <label className="checkbox" title="Trevor wrzuca każdy Twój mecz na kanał z meczami">
                <input
                  type="checkbox"
                  checked={a.tracked}
                  disabled={busy}
                  onChange={(e) => {
                    const tracked = e.target.checked
                    run(() => setRiotAccountTracked(a.id, tracked), 'Nie udało się zmienić raportów.')
                  }}
                />
                Raporty na Discordzie
              </label>
              {a.isMain ? (
                <span className="account-main">główne</span>
              ) : (
                <button
                  type="button"
                  className="button secondary small"
                  disabled={busy}
                  onClick={() => run(() => setMainRiotAccount(a.id), 'Nie udało się zmienić głównego konta.')}
                >
                  Ustaw jako główne
                </button>
              )}
              <button
                type="button"
                className="button secondary small"
                disabled={busy}
                onClick={() => run(() => unlinkRiotAccount(a.id), 'Nie udało się odłączyć konta.')}
              >
                Odłącz
              </button>
            </li>
          ))}
        </ul>
      )}

      {canAdd ? (
        <>
          <p className="muted">
            {view.accounts.length === 0
              ? 'Podaj swój Riot ID. Trevor uwierzy Ci na słowo, ale kumple widzą, czyje to konto.'
              : 'Masz smurfa? Dodaj go tutaj. Główne konto biorą /rank i /roast bez nicku.'}
          </p>
          <form className="panel-row" onSubmit={link}>
            <input
              className="search"
              placeholder="Riot ID, np. Nick#EUNE"
              value={riotId}
              onChange={(e) => setRiotId(e.target.value)}
              required
            />
            <button type="submit" className="button gold" disabled={busy || !riotId.trim()}>
              Podepnij
            </button>
          </form>
        </>
      ) : (
        <p className="muted">Komplet: {view.maxAccounts} kont. Wystarczy tych smurfów.</p>
      )}

      {busy && <p className="muted">Pytam Riota…</p>}
      {error && <p className="error">{error}</p>}
    </section>
  )
}
