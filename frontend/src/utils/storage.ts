const PREFIX = 'admission.'

export const storage = {
  get(key: string): string | null {
    try {
      return window.localStorage.getItem(PREFIX + key)
    } catch {
      return null
    }
  },
  set(key: string, value: string | null) {
    try {
      if (value === null) window.localStorage.removeItem(PREFIX + key)
      else window.localStorage.setItem(PREFIX + key, value)
    } catch {
      // Ignore: persistence is a convenience, not a requirement for correctness.
    }
  },
}

export const STUDENT_ID_KEY = 'studentId'

export const STUDENT_ID_PATTERN = /^\d{17}$/
