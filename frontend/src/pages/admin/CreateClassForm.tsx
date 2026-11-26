import { useState, type FormEvent } from 'react'
import { api, errorMessage } from '../../api/client'
import type { SchoolClass } from '../../api/types'
import { Alert } from '../../components/ui'

const MAX_SEATS = 10000

export function CreateClassForm({ onCreated }: { onCreated: (cls: SchoolClass) => void }) {
  const [name, setName] = useState('')
  const [totalSeats, setTotalSeats] = useState('120')
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState<string>()
  const [success, setSuccess] = useState<string>()

  async function submit(e: FormEvent) {
    e.preventDefault()
    setError(undefined)
    setSuccess(undefined)

    const seats = Number(totalSeats)
    if (!name.trim()) return setError('Class name is required.')
    if (!Number.isInteger(seats) || seats < 1 || seats > MAX_SEATS)
      return setError(`Total seats must be a whole number from 1 to ${MAX_SEATS}.`)

    setBusy(true)
    try {
      const created = await api.createClass({ name: name.trim(), totalSeats: seats })
      setSuccess(`Created "${created.name}" with ${created.totalSeats} seats.`)
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
      <h2>Create class (Class তৈরি)</h2>
      <p className="muted small">Create each shift as a separate class, e.g. "Class 6 - Morning Shift" and "Class 6 - Day Shift".</p>
      <div className="form-row">
        <label>
          Class name
          <input value={name} onChange={(e) => setName(e.target.value)} placeholder="Class 6 - Morning Shift" maxLength={100} />
        </label>
        <label>
          Total seats
          <input type="number" min={1} max={MAX_SEATS} value={totalSeats} onChange={(e) => setTotalSeats(e.target.value)} />
        </label>
        <button type="submit" disabled={busy}>{busy ? 'Creating…' : 'Create class'}</button>
      </div>
      {error && <Alert kind="error">{error}</Alert>}
      {success && <Alert kind="success">{success}</Alert>}
    </form>
  )
}
