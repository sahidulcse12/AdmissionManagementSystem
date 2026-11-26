import type { SchoolClass } from '../../api/types'
import { Empty, Loading, SeatBar } from '../../components/ui'

interface Props {
  classes: SchoolClass[] | undefined
  loading: boolean
  /** Class ids where the student already holds a seat or is admitted. */
  blockedClassIds: Set<string>
  reservingId: string | undefined
  onReserve: (cls: SchoolClass) => void
}

export function AvailableClasses({ classes, loading, blockedClassIds, reservingId, onReserve }: Props) {
  if (loading && !classes) return <Loading label="Loading classes…" />
  if (!classes?.length) return <Empty>No classes are open for admission yet.</Empty>

  return (
    <div className="class-grid">
      {classes.map((c) => {
        const blocked = blockedClassIds.has(c.id)
        const full = c.availableSeats === 0
        return (
          <article key={c.id} className="class-card">
            <header>
              <h3>{c.name}</h3>
              <span className="muted small">{c.shift} shift</span>
            </header>
            <p className={full ? 'seats full' : 'seats'}>
              <strong>{c.availableSeats}</strong> of {c.capacity} seats available
            </p>
            <SeatBar cls={c} />
            <button
              onClick={() => onReserve(c)}
              disabled={full || blocked || reservingId !== undefined}
              aria-label={`Reserve a seat in ${c.name} ${c.shift}`}
            >
              {reservingId === c.id ? 'Reserving…' : blocked ? 'Already reserved' : full ? 'Full' : 'Reserve seat'}
            </button>
          </article>
        )
      })}
    </div>
  )
}
