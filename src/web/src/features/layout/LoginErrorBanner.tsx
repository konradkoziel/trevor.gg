import type { LoginErrorReason } from '../../auth/sessionFragment'

interface Props {
  reason: LoginErrorReason
  onClose: () => void
}

export function LoginErrorBanner({ reason, onClose }: Props) {
  return (
    <p className="banner error" role="alert">
      {reason === 'not-member'
        ? 'Ta strona jest tylko dla ludzi z serwera Trevora. Najpierw wbij na serwer, potem wracaj.'
        : 'Logowanie się nie udało. Discord albo Ty coś pomieszaliście, spróbuj jeszcze raz.'}
      <button type="button" className="link" onClick={onClose}>
        Zamknij
      </button>
    </p>
  )
}
