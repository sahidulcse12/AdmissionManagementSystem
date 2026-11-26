import type { SchoolClass } from '../../api/types'
import { Empty, Loading, SeatBar } from '../../components/ui'

interface Props {
  classes: SchoolClass[] | undefined
  loading: boolean
  blockedClassIds: Set<string>
  reservingId: string | undefined
  disabled?: boolean
  onReserve: (cls: SchoolClass) => void
}

export function AvailableClasses({ classes, loading, blockedClassIds, reservingId, disabled = false, onReserve }: Props) {
  if (loading && !classes) return <Loading label="Loading classes…" />
  if (!classes?.length) return <Empty>No classes are open for admission yet.</Empty>

  return (
    <div className="class-grid">
      {classes.map((c) => {
        const blocked = blockedClassIds.has(c.id)
        const full = c.availableSeats === 0
        return (
          <article key={c.id} className="class-card">
            <h3>{c.name}</h3>
            <dl className="seat-facts">
              <div><dt>Total</dt><dd>{c.totalSeats}</dd></div>
              <div><dt>Held</dt><dd>{c.heldSeats}</dd></div>
              <div><dt>Admitted</dt><dd>{c.confirmedSeats}</dd></div>
              <div className={full ? 'full' : 'free'}><dt>Available</dt><dd>{c.availableSeats}</dd></div>
            </dl>
            <SeatBar cls={c} />
            <button
              onClick={() => onReserve(c)}
              disabled={disabled || full || blocked || reservingId !== undefined}
              aria-label={`Apply for ${c.name}`}
            >
              {reservingId === c.id ? 'Applying…' : blocked ? 'Already applied' : full ? 'Full (seat শেষ)' : 'আবেদন করুন (Apply)'}
            </button>
          </article>
        )
      })}
    </div>
  )
}
