import type { ReactNode } from 'react'
import type { HoldStatus, SchoolClass } from '../api/types'

export function Alert({ kind, children }: { kind: 'error' | 'success' | 'info'; children: ReactNode }) {
  return (
    <div className={`alert alert-${kind}`} role={kind === 'error' ? 'alert' : 'status'}>
      {children}
    </div>
  )
}

export function Loading({ label = 'Loading…' }: { label?: string }) {
  return <p className="muted" aria-busy="true">{label}</p>
}

export function Empty({ children }: { children: ReactNode }) {
  return <p className="empty">{children}</p>
}

const STATUS_LABEL: Record<HoldStatus, string> = {
  HELD: 'Held',
  CONFIRMED: 'Admitted',
  EXPIRED: 'Expired',
  CANCELLED: 'Cancelled',
}

export function StatusBadge({ status }: { status: HoldStatus }) {
  return <span className={`badge badge-${status.toLowerCase()}`}>{STATUS_LABEL[status]}</span>
}

/** Stacked bar: confirmed | held | available. */
export function SeatBar({ cls }: { cls: SchoolClass }) {
  const pct = (n: number) => `${(n / cls.capacity) * 100}%`
  return (
    <div
      className="seatbar"
      role="img"
      aria-label={`${cls.confirmedSeats} admitted, ${cls.heldSeats} held, ${cls.availableSeats} available of ${cls.capacity}`}
    >
      <span className="seatbar-confirmed" style={{ width: pct(cls.confirmedSeats) }} />
      <span className="seatbar-held" style={{ width: pct(cls.heldSeats) }} />
    </div>
  )
}

export function formatDateTime(iso: string | null): string {
  return iso ? new Date(iso).toLocaleString() : '—'
}
