import { useEffect, useState } from 'react'
import { StudentIdBar } from './components/StudentIdBar'
import { AdminPage } from './pages/admin/AdminPage'
import { ParentPage } from './pages/parent/ParentPage'
import { STUDENT_ID_KEY, STUDENT_ID_PATTERN, storage } from './utils/storage'

type Route = 'parent' | 'admin'

function currentRoute(): Route {
  return window.location.hash.startsWith('#/admin') ? 'admin' : 'parent'
}

function savedStudentId(): string | undefined {
  const saved = storage.get(STUDENT_ID_KEY)
  return saved && STUDENT_ID_PATTERN.test(saved) ? saved : undefined
}

export default function App() {
  const [route, setRoute] = useState<Route>(currentRoute)
  // Remembered across refreshes and browser restarts; hold state itself is always re-read from the API.
  const [studentId, setStudentId] = useState(savedStudentId)

  useEffect(() => {
    const onHash = () => setRoute(currentRoute())
    window.addEventListener('hashchange', onHash)
    return () => window.removeEventListener('hashchange', onHash)
  }, [])

  function changeStudent(id: string | undefined) {
    storage.set(STUDENT_ID_KEY, id ?? null)
    setStudentId(id)
  }

  return (
    <>
      <header className="topbar">
        <div className="topbar-inner">
          <strong className="brand">School Admission</strong>
          <nav>
            <a href="#/" className={route === 'parent' ? 'active' : undefined}>Parent / Guardian</a>
            <a href="#/admin" className={route === 'admin' ? 'active' : undefined}>Admin</a>
          </nav>
        </div>
        <div className="topbar-inner student-row">
          <StudentIdBar studentId={studentId} onChange={changeStudent} />
        </div>
      </header>
      <main className="container">
        {route === 'admin' ? <AdminPage /> : <ParentPage studentId={studentId} />}
      </main>
    </>
  )
}
