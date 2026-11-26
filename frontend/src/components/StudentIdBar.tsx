import { useState, type FormEvent } from 'react'
import { STUDENT_ID_PATTERN } from '../utils/storage'

interface Props {
  studentId: string | undefined
  onChange: (studentId: string | undefined) => void
}

export function StudentIdBar({ studentId, onChange }: Props) {
  const [value, setValue] = useState('')
  const [error, setError] = useState<string>()

  function submit(e: FormEvent) {
    e.preventDefault()
    const id = value.trim()
    if (!STUDENT_ID_PATTERN.test(id)) {
      setError('Enter the 17-digit birth registration number (১৭ অঙ্কের জন্ম নিবন্ধন নম্বর).')
      return
    }
    setError(undefined)
    setValue('')
    onChange(id)
  }

  if (studentId) {
    return (
      <div className="student-box">
        <span className="muted small">Student ID (জন্ম নিবন্ধন নম্বর)</span>
        <strong className="mono student-id">{studentId}</strong>
        <button className="secondary small-btn" onClick={() => onChange(undefined)}>Change</button>
      </div>
    )
  }

  return (
    <form className="student-box" onSubmit={submit} noValidate>
      <label className="student-label">
        Student birth registration number (জন্ম নিবন্ধন নম্বর)
        <input
          value={value}
          onChange={(e) => setValue(e.target.value.replace(/\D/g, ''))}
          inputMode="numeric"
          maxLength={17}
          placeholder="17 digits"
          autoComplete="off"
        />
      </label>
      <button type="submit">Use this ID</button>
      {error && <p className="field-error" role="alert">{error}</p>}
    </form>
  )
}
