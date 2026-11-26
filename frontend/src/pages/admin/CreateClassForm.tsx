import { useState, type FormEvent } from 'react'
import { api, errorMessage } from '../../api/client'
import { SHIFTS, type SchoolClass, type Shift } from '../../api/types'
import { Alert } from '../../components/ui'

export function CreateClassForm({ onCreated }: { onCreated: (cls: SchoolClass) => void }) {
  const [name, setName] = useState('')
  const [shift, setShift] = useState<Shift>('MORNING')
  const [capacity, setCapacity] = useState('30')
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState<string>()
  const [success, setSuccess] = useState<string>()

  async function submit(e: FormEvent) {
    e.preventDefault()
    setError(undefined)
    setSuccess(undefined)

    const seats = Number(capacity)
    if (!name.trim()) return setError('Class name is required.')
    if (!Number.isInteger(seats) || seats < 1 || seats > 1000) return setError('Capacity must be a whole number from 1 to 1000.')

    setBusy(true)
    try {
      const created = await api.createClass({ name: name.trim(), shift, capacity: seats })
      setSuccess(`Created ${created.name} (${created.shift}) with ${created.capacity} seats.`)
      setName('')
      onCreated(created)
    } catch (err) {
      setError(errorMessage(err))
    } finally {
      setBusy(false)
    }
  }

  return (
    <form className="card form" onSubmit={submit} noValidate>
      <h2>Create class</h2>
      <div className="form-row">
        <label>
          Class name
          <input value={name} onChange={(e) => setName(e.target.value)} placeholder="e.g. Class 1" maxLength={100} />
        </label>
        <label>
          Shift
          <select value={shift} onChange={(e) => setShift(e.target.value as Shift)}>
            {SHIFTS.map((s) => (
              <option key={s} value={s}>{s}</option>
            ))}
          </select>
        </label>
        <label>
          Seat capacity
          <input type="number" min={1} max={1000} value={capacity} onChange={(e) => setCapacity(e.target.value)} />
        </label>
        <button type="submit" disabled={busy}>{busy ? 'Creating…' : 'Create class'}</button>
      </div>
      {error && <Alert kind="error">{error}</Alert>}
      {success && <Alert kind="success">{success}</Alert>}
    </form>
  )
}
