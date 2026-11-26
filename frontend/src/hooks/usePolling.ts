import { useCallback, useEffect, useRef, useState } from 'react'
import { errorMessage } from '../api/client'

export const DEFAULT_REFRESH_MS = 5000

interface PollingState<T> {
  data: T | undefined
  error: string | undefined
  loading: boolean
  refresh: () => Promise<void>
}

export function usePolling<T>(load: () => Promise<T>, intervalMs = DEFAULT_REFRESH_MS, enabled = true): PollingState<T> {
  const [data, setData] = useState<T>()
  const [error, setError] = useState<string>()
  const [loading, setLoading] = useState(enabled)
  const loadRef = useRef(load)
  loadRef.current = load

  const refresh = useCallback(async () => {
    try {
      const result = await loadRef.current()
      setData(result)
      setError(undefined)
    } catch (e) {
      setError(errorMessage(e))
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    if (!enabled) return
    setLoading(true)
    void refresh()
    const id = window.setInterval(() => {
      if (document.visibilityState === 'visible') void refresh()
    }, intervalMs)
    return () => window.clearInterval(id)
  }, [enabled, intervalMs, refresh])

  return { data, error, loading, refresh }
}
