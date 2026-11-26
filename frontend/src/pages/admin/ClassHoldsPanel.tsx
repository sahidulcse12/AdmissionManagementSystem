import { useCallback } from 'react'
import { api } from '../../api/client'
import type { SchoolClass } from '../../api/types'
import { Alert, Empty, Loading, StatusBadge } from '../../components/ui'
import { usePolling } from '../../hooks/usePolling'
import { formatDateTime } from '../../utils/format'

export function ClassHoldsPanel({ cls, onClose }: { cls: SchoolClass; onClose: () => void }) {
  const load = useCallback(() => api.listClassHolds(cls.id), [cls.id])
  const { data: holds, error, loading } = usePolling(load)

  return (
    <section className="card">
      <div className="card-header">
        <h2>{cls.name} — admissions</h2>
        <button className="secondary" onClick={onClose}>Close</button>
      </div>
      {error && <Alert kind="error">{error}</Alert>}
      {loading && !holds ? (
        <Loading />
      ) : !holds?.length ? (
        <Empty>No holds or admissions yet.</Empty>
      ) : (
        <div className="table-wrap">
          <table>
            <thead>
              <tr>
                <th>Student ID</th>
                <th>Status</th>
                <th>Held at</th>
                <th>Expires / expired</th>
                <th>Admitted at</th>
              </tr>
            </thead>
            <tbody>
              {holds.map((h) => (
                <tr key={h.holdId}>
                  <td className="mono">{h.studentId}</td>
                  <td><StatusBadge status={h.status} /></td>
                  <td>{formatDateTime(h.createdAt)}</td>
                  <td>{h.status === 'CONFIRMED' ? '—' : formatDateTime(h.expiresAt)}</td>
                  <td>{formatDateTime(h.confirmedAt)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </section>
  )
}
