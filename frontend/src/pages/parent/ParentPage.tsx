import { api } from '../../api/client'
import { Alert } from '../../components/ui'
import { usePolling } from '../../hooks/usePolling'
import { AvailableClasses } from './AvailableClasses'
import { StudentDashboard } from './StudentDashboard'

export function ParentPage({ studentId }: { studentId: string | undefined }) {
  // `key` resets all dashboard state when the student changes.
  if (studentId) return <StudentDashboard key={studentId} studentId={studentId} />
  return <ClassesOnly />
}

function ClassesOnly() {
  const classes = usePolling(api.listClasses)
  return (
    <div className="stack">
      <Alert kind="info">
        Enter the student's 17-digit birth registration number at the top of the page to apply.
        (আবেদন করতে উপরের ঘরে শিক্ষার্থীর জন্ম নিবন্ধন নম্বর লিখুন।)
      </Alert>
      <section className="card">
        <h2>Classes (Class তালিকা)</h2>
        {classes.error && <Alert kind="error">{classes.error}</Alert>}
        <AvailableClasses
          classes={classes.data}
          loading={classes.loading}
          blockedClassIds={new Set()}
          reservingId={undefined}
          disabled
          onReserve={() => undefined}
        />
      </section>
    </div>
  )
}
