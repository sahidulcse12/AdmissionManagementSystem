import { useState } from 'react'
import { STUDENT_ID_KEY, STUDENT_ID_PATTERN, storage } from '../../utils/storage'
import { StudentDashboard } from './StudentDashboard'
import { StudentIdForm } from './StudentIdForm'

function initialStudentId(): string | undefined {
  const saved = storage.get(STUDENT_ID_KEY)
  return saved && STUDENT_ID_PATTERN.test(saved) ? saved : undefined
}

/** Remembers the student across refreshes and browser restarts; the dashboard re-reads all state from the API. */
export function ParentPage() {
  const [studentId, setStudentId] = useState(initialStudentId)

  function choose(id: string | undefined) {
    storage.set(STUDENT_ID_KEY, id ?? null)
    setStudentId(id)
  }

  if (!studentId) return <StudentIdForm onSubmit={choose} />
  // `key` resets all dashboard state when the student changes.
  return <StudentDashboard key={studentId} studentId={studentId} onChangeStudent={() => choose(undefined)} />
}
