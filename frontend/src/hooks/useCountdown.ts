import { useEffect, useState } from 'react'

/**
 * Seconds left until a deadline, ticking every second.
 * The deadline is anchored to the moment the server reported `remainingSeconds`,
 * so differences between the browser clock and the server clock do not matter.
 */
export function useCountdown(remainingSeconds: number, receivedAt: number): number {
  const compute = () => Math.max(0, Math.ceil(remainingSeconds - (Date.now() - receivedAt) / 1000))
  const [left, setLeft] = useState(compute)

  useEffect(() => {
    setLeft(compute())
    const id = window.setInterval(() => setLeft(compute()), 1000)
    return () => window.clearInterval(id)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [remainingSeconds, receivedAt])

  return left
}
