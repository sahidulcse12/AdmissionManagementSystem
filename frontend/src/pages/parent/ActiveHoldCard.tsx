import { useEffect, useState } from 'react'
import { api, errorMessage } from '../../api/client'
import type { Hold } from '../../api/types'
import { Alert } from '../../components/ui'
import { useCountdown } from '../../hooks/useCountdown'
import { formatCountdown } from '../../utils/format'

interface Props {
  hold: Hold
  /** Browser time (ms) when `hold` was fetched; anchors the countdown. */
  receivedAt: number
  studentId: string
  onChanged: (message?: string) => void
}

/** A HELD seat with a live countdown and the confirm / cancel actions. */
export function ActiveHoldCard({ hold, receivedAt, studentId, onChanged }: Props) {
  const left = useCountdown(hold.remainingSeconds, receivedAt)
  const [busy, setBusy] = useState<'confirm' | 'cancel'>()
  const [error, setError] = useState<string>()
  const expired = left <= 0

  // When the timer runs out, ask the backend for the real state.
  useEffect(() => {
    if (expired) onChanged()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [expired])

  async function run(action: 'confirm' | 'cancel') {
    setBusy(action)
    setError(undefined)
    try {
      if (action === 'confirm') {
        await api.confirmHold(hold.id, studentId)
        onChanged(`Admission confirmed for ${hold.className} (${hold.shift}).`)
      } else {
        await api.cancelHold(hold.id, studentId)
        onChanged(`Hold for ${hold.className} (${hold.shift}) cancelled. The seat has been released.`)
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
        <div className="hold-title">{hold.className} · {hold.shift}</div>
        <div className="muted small">Seat reserved. Complete payment before the timer ends.</div>
      </div>
      <div className="countdown" aria-live="polite" aria-label={`Time left ${formatCountdown(left)}`}>
        {expired ? 'Expired' : formatCountdown(left)}
      </div>
      <div className="hold-actions">
        <button onClick={() => run('confirm')} disabled={!!busy || expired}>
          {busy === 'confirm' ? 'Processing payment…' : 'Pay & confirm admission'}
        </button>
        <button className="danger" onClick={() => run('cancel')} disabled={!!busy || expired}>
          {busy === 'cancel' ? 'Cancelling…' : 'Cancel hold'}
        </button>
      </div>
      {error && <Alert kind="error">{error}</Alert>}
    </div>
  )
}
