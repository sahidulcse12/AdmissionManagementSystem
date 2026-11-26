import type { Hold } from '../../api/types'
import { Empty, StatusBadge } from '../../components/ui'
import { formatDateTime } from '../../utils/format'

function outcomeTime(h: Hold): string {
  switch (h.status) {
    case 'CONFIRMED': return formatDateTime(h.confirmedAt)
    case 'CANCELLED': return formatDateTime(h.cancelledAt)
    case 'EXPIRED': return formatDateTime(h.expiresAt)
    default: return '—'
  }
}

export function HistoryTable({ holds }: { holds: Hold[] }) {
  if (!holds.length) return <Empty>No reservations yet.</Empty>
  return (
    <div className="table-wrap">
      <table>
        <thead>
          <tr>
            <th>Class</th>
            <th>Shift</th>
            <th>Status</th>
            <th>Reserved at</th>
            <th>Completed / ended</th>
          </tr>
        </thead>
        <tbody>
          {holds.map((h) => (
            <tr key={h.id}>
              <td>{h.className}</td>
              <td>{h.shift}</td>
              <td><StatusBadge status={h.status} /></td>
              <td>{formatDateTime(h.createdAt)}</td>
              <td>{outcomeTime(h)}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
