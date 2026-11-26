import { useEffect, useState } from 'react'
import { api, errorMessage } from '../../api/client'
import type { Hold } from '../../api/types'
import { Alert, StatusBadge } from '../../components/ui'
import { useCountdown } from '../../hooks/useCountdown'
import { formatCountdown } from '../../utils/format'

interface Props {
  hold: Hold
  receivedAt: number
  studentId: string
  onChanged: (message?: string) => void
}

export function ActiveHoldCard({ hold, receivedAt, studentId, onChanged }: Props) {
  const left = useCountdown(hold.remainingSeconds, receivedAt)
  const [busy, setBusy] = useState<'confirm' | 'cancel'>()
  const [error, setError] = useState<string>()
  // Expiry is final: once the deadline passes, a later (stale) response must not revive the card.
  const [expiredLatched, setExpiredLatched] = useState(false)
  const expired = expiredLatched || left <= 0

  // When the timer runs out, show EXPIRED immediately and ask the backend for the real state.
  useEffect(() => {
    if (left <= 0 && !expiredLatched) {
      setExpiredLatched(true)
      onChanged()
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [left])

  async function run(action: 'confirm' | 'cancel') {
    setBusy(action)
    setError(undefined)
    try {
      if (action === 'confirm') {
        await api.confirmHold(hold.holdId, studentId)
        onChanged(`Admission confirmed for ${hold.className}. (ভর্তি নিশ্চিত হয়েছে)`)
      } else {
        await api.cancelHold(hold.holdId, studentId)
        onChanged(`Application for ${hold.className} cancelled. The seat has been released.`)
      }
    } catch (e) {
      setError(errorMessage(e))
      onChanged()
    } finally {
      setBusy(undefined)
    }
  }

  return (
    <div className={`hold-card${left <= 60 ? ' urgent' : ''}`}>
      <div>
        <div className="hold-title">{hold.className}</div>
        <div className="muted small">
          Status: <StatusBadge status={expired ? 'EXPIRED' : hold.status} /> · Pay the admission fee before the timer ends.
        </div>
      </div>
      <div className="countdown" aria-live="polite" aria-label={`Time left ${formatCountdown(left)}`}>
        {expired ? 'EXPIRED' : formatCountdown(left)}
      </div>
      <div className="hold-actions">
        <button onClick={() => run('confirm')} disabled={!!busy || expired}>
          {busy === 'confirm' ? 'Processing payment…' : 'ভর্তি ফি দিন (Pay admission fee)'}
        </button>
        <button className="danger" onClick={() => run('cancel')} disabled={!!busy || expired}>
          {busy === 'cancel' ? 'Cancelling…' : 'বাতিল (Cancel)'}
        </button>
      </div>
      {error && <Alert kind="error">{error}</Alert>}
    </div>
  )
}
