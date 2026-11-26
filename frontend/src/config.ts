export const API_URL: string = (
  window.__APP_CONFIG__?.apiUrl ||
  import.meta.env.VITE_API_URL ||
  'http://localhost:8080'
).replace(/\/+$/, '')
