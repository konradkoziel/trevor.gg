import { useState, type FormEvent } from 'react'
import { errorText } from '../../api/errorText'
import { fetchRiotAccount, linkRiotAccount, unlinkRiotAccount, type RiotAccountView } from '../../api/riotAccount'
import { useApi } from '../../api/useApi'

// Link a League account by Riot ID; only Trevor's server can log in, so there is no ownership check
export function RiotAccountPanel() {
  const loaded = useApi(fetchRiotAccount)
  // After an action the panel shows the server's answer instead of the first load
  const [changed, setChanged] = useState<RiotAccountView>()
  const [riotId, setRiotId] = useState('')
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState<string>()

  if (!changed && loaded.status === 'loading') return <p className="muted">Ładuję…</p>
  if (!changed && loaded.status === 'error') return <p className="error">Nie udało się sprawdzić Twojego konta.</p>
  const view = changed ?? (loaded.status === 'ready' ? loaded.data : { linkedRiotId: null })

  const run = async (action: () => Promise<RiotAccountView>, fallback: string) => {
    setBusy(true)
    setError(undefined)
    try {
      setChanged(await action())
    } catch (e) {
      setError(errorText(e, fallback))
    } finally {
      setBusy(false)
    }
  }

  const link = (e: FormEvent) => {
    e.preventDefault()
    void run(() => linkRiotAccount(riotId), 'Nie udało się podpiąć konta.')
  }
  const unlink = () =>
    run(async () => {
      await unlinkRiotAccount()
      return { linkedRiotId: null }
    }, 'Nie udało się odłączyć konta.')

  return (
    <section className="panel">
      <h2 className="page-title">Konto League of Legends</h2>

      {view.linkedRiotId ? (
        <div className="panel-row">
          <p>
            Podpięte: <strong>{view.linkedRiotId}</strong>
          </p>
          <button type="button" className="button secondary" onClick={unlink} disabled={busy}>
            Odłącz
          </button>
        </div>
      ) : (
        <>
          <p className="muted">Podaj swój Riot ID. Trevor uwierzy Ci na słowo, ale kumple widzą, czyje to konto.</p>
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
      )}

      {busy && <p className="muted">Pytam Riota…</p>}
      {error && <p className="error">{error}</p>}
    </section>
  )
}
