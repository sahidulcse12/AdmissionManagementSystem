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
  if (!holds.length) return <Empty>No applications yet.</Empty>
  return (
    <div className="table-wrap">
      <table>
        <thead>
          <tr>
            <th>Class</th>
            <th>Status</th>
            <th>Applied at</th>
            <th>Expires at</th>
            <th>Completed / ended</th>
          </tr>
        </thead>
        <tbody>
          {holds.map((h) => (
            <tr key={h.holdId}>
              <td>{h.className}</td>
              <td><StatusBadge status={h.status} /></td>
              <td>{formatDateTime(h.createdAt)}</td>
              <td>{formatDateTime(h.expiresAt)}</td>
              <td>{outcomeTime(h)}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
