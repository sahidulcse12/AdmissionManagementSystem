import { useState, type FormEvent } from 'react'
import { Alert } from '../../components/ui'
import { STUDENT_ID_PATTERN } from '../../utils/storage'

export function StudentIdForm({ onSubmit }: { onSubmit: (studentId: string) => void }) {
  const [value, setValue] = useState('')
  const [error, setError] = useState<string>()

  function submit(e: FormEvent) {
    e.preventDefault()
    const id = value.trim()
    if (!STUDENT_ID_PATTERN.test(id)) {
      setError('Enter the 17-digit birth registration number.')
      return
    }
    onSubmit(id)
  }

  return (
    <form className="card form" onSubmit={submit} noValidate>
      <h2>Student identification</h2>
      <p className="muted">Enter the student's birth registration number to reserve a seat and view admission history.</p>
      <div className="form-row">
        <label>
          Birth registration number
          <input
            value={value}
            onChange={(e) => setValue(e.target.value.replace(/\D/g, ''))}
            inputMode="numeric"
            maxLength={17}
            placeholder="17 digits"
            autoComplete="off"
          />
        </label>
        <button type="submit">Continue</button>
      </div>
      {error && <Alert kind="error">{error}</Alert>}
    </form>
  )
}
