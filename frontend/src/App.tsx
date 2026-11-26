import { useEffect, useState } from 'react'
import { AdminPage } from './pages/admin/AdminPage'
import { ParentPage } from './pages/parent/ParentPage'

type Route = 'parent' | 'admin'

function currentRoute(): Route {
  return window.location.hash.startsWith('#/admin') ? 'admin' : 'parent'
}

export default function App() {
  const [route, setRoute] = useState<Route>(currentRoute)

  useEffect(() => {
    const onHash = () => setRoute(currentRoute())
    window.addEventListener('hashchange', onHash)
    return () => window.removeEventListener('hashchange', onHash)
  }, [])

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
      </header>
      <main className="container">
        {route === 'admin' ? <AdminPage /> : <ParentPage />}
      </main>
    </>
  )
}
