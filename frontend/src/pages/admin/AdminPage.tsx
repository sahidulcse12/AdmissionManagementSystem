import { useState } from 'react'
import { api } from '../../api/client'
import type { SchoolClass } from '../../api/types'
import { Alert, Empty, Loading, SeatBar } from '../../components/ui'
import { usePolling } from '../../hooks/usePolling'
import { ClassHoldsPanel } from './ClassHoldsPanel'
import { CreateClassForm } from './CreateClassForm'

export function AdminPage() {
  const { data: classes, error, loading, refresh } = usePolling(api.listClasses)
  const [selected, setSelected] = useState<SchoolClass>()

  const totals = (classes ?? []).reduce(
    (t, c) => ({
      total: t.total + c.totalSeats,
      confirmed: t.confirmed + c.confirmedSeats,
      held: t.held + c.heldSeats,
      available: t.available + c.availableSeats,
    }),
    { total: 0, confirmed: 0, held: 0, available: 0 },
  )

  return (
    <div className="stack">
      <CreateClassForm onCreated={() => void refresh()} />

      <section className="card">
        <div className="card-header">
          <h2>Classes and seats</h2>
          <span className="muted small">Auto-refreshes every 5 seconds</span>
        </div>

        {classes && classes.length > 0 && (
          <dl className="stats">
            <div><dt>Total seats</dt><dd>{totals.total}</dd></div>
            <div><dt>Admitted</dt><dd>{totals.confirmed}</dd></div>
            <div><dt>Held</dt><dd>{totals.held}</dd></div>
            <div><dt>Available</dt><dd>{totals.available}</dd></div>
          </dl>
        )}

        {error && <Alert kind="error">{error}</Alert>}
        {loading && !classes ? (
          <Loading label="Loading classes…" />
        ) : !classes?.length ? (
          <Empty>No classes yet. Create the first one above.</Empty>
        ) : (
          <div className="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>Class</th>
                  <th className="num">Total seats</th>
                  <th className="num">Admitted</th>
                  <th className="num">Held</th>
                  <th className="num">Available</th>
                  <th>Usage</th>
                  <th />
                </tr>
              </thead>
              <tbody>
                {classes.map((c) => (
                  <tr key={c.id} className={selected?.id === c.id ? 'selected' : undefined}>
                    <td>{c.name}</td>
                    <td className="num">{c.totalSeats}</td>
                    <td className="num">{c.confirmedSeats}</td>
                    <td className="num">{c.heldSeats}</td>
                    <td className="num">{c.availableSeats}</td>
                    <td><SeatBar cls={c} /></td>
                    <td>
                      <button className="link" onClick={() => setSelected(c)}>View admissions</button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>

      {selected && <ClassHoldsPanel cls={selected} onClose={() => setSelected(undefined)} />}
    </div>
  )
}
