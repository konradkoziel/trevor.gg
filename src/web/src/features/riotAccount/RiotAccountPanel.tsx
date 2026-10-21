import { useState, type FormEvent } from 'react'
import { errorText } from '../../api/errorText'
import {
  fetchRiotAccount,
  startRiotLink,
  unlinkRiotAccount,
  verifyRiotLink,
  type RiotAccountView,
} from '../../api/riotAccount'
import { useApi } from '../../api/useApi'

const timeFormat = new Intl.DateTimeFormat('pl-PL', { hour: '2-digit', minute: '2-digit' })

// Link a League account: Riot ID → set the given default profile icon in the client → check
export function RiotAccountPanel() {
  const loaded = useApi(fetchRiotAccount)
  // After an action the panel shows the server's answer instead of the first load
  const [changed, setChanged] = useState<RiotAccountView>()
  const [riotId, setRiotId] = useState('')
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState<string>()

  if (!changed && loaded.status === 'loading') return <p className="muted">Ładuję…</p>
  if (!changed && loaded.status === 'error') return <p className="error">Nie udało się sprawdzić Twojego konta.</p>
  const view = changed ?? (loaded.status === 'ready' ? loaded.data : { linkedRiotId: null, challenge: null })

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

  const start = (e: FormEvent) => {
    e.preventDefault()
    void run(async () => ({ ...view, challenge: await startRiotLink(riotId) }), 'Nie udało się znaleźć konta.')
  }
  const verify = () => run(verifyRiotLink, 'Nie udało się sprawdzić ikony.')
  const unlink = () =>
    run(async () => {
      await unlinkRiotAccount()
      return { linkedRiotId: null, challenge: null }
    }, 'Nie udało się odłączyć konta.')
  const restart = () => setChanged({ ...view, challenge: null })

  return (
    <section className="panel">
      <h2 className="page-title">Konto League of Legends</h2>

      {view.linkedRiotId && !view.challenge && (
        <div className="panel-row">
          <p>
            Podpięte: <strong>{view.linkedRiotId}</strong>
          </p>
          <button type="button" className="button secondary" onClick={unlink} disabled={busy}>
            Odłącz
          </button>
        </div>
      )}

      {view.challenge && (
        <div className="challenge">
          {view.challenge.iconUrl && <img src={view.challenge.iconUrl} alt="" width={96} height={96} />}
          <div>
            <p>
              Udowodnij, że <strong>{view.challenge.riotId}</strong> to Ty: w kliencie LoLa ustaw tę ikonę profilu
              (kliknij swoją ikonę w prawym górnym rogu). Każde konto ją ma.
            </p>
            <p className="muted">
              Masz czas do {timeFormat.format(new Date(view.challenge.expiresAt))}. Potem możesz wrócić do starej ikony.
            </p>
            <div className="panel-row">
              <button type="button" className="button discord" onClick={verify} disabled={busy}>
                Sprawdź
              </button>
              <button type="button" className="button secondary" onClick={restart} disabled={busy}>
                Inne konto
              </button>
            </div>
          </div>
        </div>
      )}

      {!view.linkedRiotId && !view.challenge && (
        <form className="panel-row" onSubmit={start}>
          <input
            className="search"
            placeholder="Riot ID, np. Nick#EUNE"
            value={riotId}
            onChange={(e) => setRiotId(e.target.value)}
            required
          />
          <button type="submit" className="button discord" disabled={busy || !riotId.trim()}>
            Dalej
          </button>
        </form>
      )}

      {busy && <p className="muted">Pytam Riota…</p>}
      {error && <p className="error">{error}</p>}
    </section>
  )
}
