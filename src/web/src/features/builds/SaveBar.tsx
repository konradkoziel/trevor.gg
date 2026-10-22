import { useState } from 'react'
import { useNavigate } from 'react-router'
import { deleteBuild, saveBuild } from '../../api/builds'
import { errorText } from '../../api/errorText'
import { LoginLink } from '../account/LoginLink'
import { useCurrentUser } from '../account/useCurrentUser'
import type { Build } from './build'
import { buildPath, shareUrl } from './buildLinks'

interface Props {
  build: Build
  // Set once the build exists on the server
  buildId?: string
}

type Status = { kind: 'idle' } | { kind: 'saving' } | { kind: 'saved' } | { kind: 'copied' } | { kind: 'error'; message: string }

export function SaveBar({ build, buildId }: Props) {
  const { state } = useCurrentUser()
  const navigate = useNavigate()
  const [status, setStatus] = useState<Status>({ kind: 'idle' })

  if (state.status !== 'loggedIn')
    return (
      <footer className="editor-footer">
        <LoginLink>Zaloguj się, żeby zapisać</LoginLink>
        <span className="muted">Bez logowania Trevor nie zapamięta tego arcydzieła.</span>
      </footer>
    )

  const save = async () => {
    setStatus({ kind: 'saving' })
    try {
      const id = await saveBuild(build, buildId)
      setStatus({ kind: 'saved' })
      if (id !== buildId) navigate(buildPath(id), { replace: true })
    } catch (error) {
      setStatus({ kind: 'error', message: describe(error) })
    }
  }

  const copyLink = async () => {
    if (!buildId) return
    try {
      await navigator.clipboard.writeText(shareUrl(buildId))
      setStatus({ kind: 'copied' })
    } catch {
      setStatus({ kind: 'error', message: `Nie udało się skopiować. Link: ${shareUrl(buildId)}` })
    }
  }

  const remove = async () => {
    if (!buildId || !window.confirm(`Usunąć build „${build.name}”? Tego się nie da cofnąć.`)) return
    try {
      await deleteBuild(buildId)
      navigate('/buildy')
    } catch (error) {
      setStatus({ kind: 'error', message: describe(error) })
    }
  }

  return (
    <footer className="editor-footer">
      <button type="button" className="button gold" onClick={save} disabled={status.kind === 'saving'}>
        {buildId ? 'Zapisz zmiany' : 'Zapisz build'}
      </button>
      {buildId && (
        <>
          <button type="button" className="button secondary" onClick={copyLink}>
            Kopiuj link
          </button>
          <button type="button" className="button danger" onClick={remove}>
            Usuń
          </button>
        </>
      )}
      <StatusText status={status} />
    </footer>
  )
}

function StatusText({ status }: { status: Status }) {
  switch (status.kind) {
    case 'saving':
      return <span className="muted">Zapisuję…</span>
    case 'saved':
      return <span className="success">Zapisane.</span>
    case 'copied':
      return <span className="success">Link skopiowany. Wklej go komuś, kto potrzebuje lekcji.</span>
    case 'error':
      return <span className="error">{status.message}</span>
    default:
      return null
  }
}

const describe = (error: unknown) => errorText(error, 'Nie udało się zapisać. Spróbuj jeszcze raz.')
