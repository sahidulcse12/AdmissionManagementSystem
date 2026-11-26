import { useCallback, useMemo, useState } from 'react'
import { api, errorMessage } from '../../api/client'
import type { SchoolClass } from '../../api/types'
import { Alert, Loading } from '../../components/ui'
import { usePolling } from '../../hooks/usePolling'
import { ActiveHoldCard } from './ActiveHoldCard'
import { AvailableClasses } from './AvailableClasses'
import { HistoryTable } from './HistoryTable'

export function StudentDashboard({ studentId }: { studentId: string }) {
  const [notice, setNotice] = useState<{ kind: 'success' | 'error'; text: string }>()
  const [reservingId, setReservingId] = useState<string>()

  const classes = usePolling(api.listClasses)

  // Holds are always read from the backend (on load and every few seconds),
  // so an active hold reappears after a page refresh or browser restart.
  const [holdsReceivedAt, setHoldsReceivedAt] = useState(() => Date.now())
  const loadHolds = useCallback(async () => {
    const result = await api.myHolds(studentId)
    setHoldsReceivedAt(Date.now())
    return result
  }, [studentId])
  const holds = usePolling(loadHolds)

  const active = useMemo(() => (holds.data ?? []).filter((h) => h.status === 'HELD'), [holds.data])
  const blockedClassIds = useMemo(
    () => new Set((holds.data ?? []).filter((h) => h.status === 'HELD' || h.status === 'CONFIRMED').map((h) => h.classId)),
    [holds.data],
  )

  const { refresh: refreshClasses } = classes
  const { refresh: refreshHolds } = holds
  const refreshAll = useCallback(async () => {
    await Promise.all([refreshClasses(), refreshHolds()])
  }, [refreshClasses, refreshHolds])

  async function reserve(cls: SchoolClass) {
    setReservingId(cls.id)
    setNotice(undefined)
    try {
      await api.placeHold(cls.id, studentId)
      setNotice({ kind: 'success', text: `Seat held in ${cls.name}. Pay the admission fee before the timer runs out.` })
    } catch (e) {
      setNotice({ kind: 'error', text: errorMessage(e) })
    } finally {
      setReservingId(undefined)
      await refreshAll()
    }
  }

  const onHoldChanged = useCallback(
    (message?: string) => {
      if (message) setNotice({ kind: 'success', text: message })
      void refreshAll()
    },
    [refreshAll],
  )

  return (
    <div className="stack">
      {notice && <Alert kind={notice.kind}>{notice.text}</Alert>}

      {active.length > 0 && (
        <section className="card">
          <h2>Application (আবেদন)</h2>
          <div className="stack">
            {active.map((h) => (
              <ActiveHoldCard key={h.holdId} hold={h} receivedAt={holdsReceivedAt} studentId={studentId} onChanged={onHoldChanged} />
            ))}
          </div>
        </section>
      )}

      <section className="card">
        <div className="card-header">
          <h2>Classes (Class তালিকা)</h2>
          <span className="muted small">Seats update automatically</span>
        </div>
        {classes.error && <Alert kind="error">{classes.error}</Alert>}
        <AvailableClasses
          classes={classes.data}
          loading={classes.loading}
          blockedClassIds={blockedClassIds}
          reservingId={reservingId}
          onReserve={reserve}
        />
      </section>

      <section className="card">
        <h2>My applications (আমার আবেদন)</h2>
        {holds.error && <Alert kind="error">{holds.error}</Alert>}
        {holds.loading && !holds.data ? <Loading /> : <HistoryTable holds={holds.data ?? []} />}
      </section>
    </div>
  )
}
