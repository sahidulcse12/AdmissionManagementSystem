import { API_URL } from '../config'
import type { CreateClassInput, Hold, SchoolClass } from './types'

export class ApiError extends Error {
  readonly status: number
  readonly code: string

  constructor(status: number, code: string, message: string) {
    super(message)
    this.status = status
    this.code = code
  }
}

const STUDENT_HEADER = 'X-Student-Id'

async function request<T>(path: string, init: RequestInit = {}, studentId?: string): Promise<T> {
  const headers = new Headers(init.headers)
  if (init.body) headers.set('Content-Type', 'application/json')
  if (studentId) headers.set(STUDENT_HEADER, studentId)

  let response: Response
  try {
    response = await fetch(`${API_URL}${path}`, { ...init, headers })
  } catch {
    throw new ApiError(0, 'NETWORK_ERROR', 'Cannot reach the server. Check your connection and try again.')
  }

  const body = parseJson(await response.text())
  if (!response.ok) {
    const error = body?.error
    throw new ApiError(response.status, error?.code ?? 'HTTP_ERROR', error?.message ?? `Request failed (${response.status}).`)
  }
  return body as T
}

function parseJson(text: string) {
  if (!text) return undefined
  try {
    return JSON.parse(text)
  } catch {
    return undefined
  }
}

export const api = {
  listClasses: () => request<SchoolClass[]>('/classes'),
  getClass: (classId: string) => request<SchoolClass>(`/classes/${classId}`),
  createClass: (input: CreateClassInput) =>
    request<SchoolClass>('/classes', { method: 'POST', body: JSON.stringify(input) }),
  listClassHolds: (classId: string) => request<Hold[]>(`/classes/${classId}/holds`),

  placeHold: (classId: string, studentId: string) =>
    request<Hold>(`/classes/${classId}/holds`, { method: 'POST' }, studentId),
  getHold: (holdId: string, studentId: string) => request<Hold>(`/holds/${holdId}`, {}, studentId),
  confirmHold: (holdId: string, studentId: string) =>
    request<Hold>(`/holds/${holdId}/confirm`, { method: 'POST' }, studentId),
  cancelHold: (holdId: string, studentId: string) =>
    request<void>(`/holds/${holdId}`, { method: 'DELETE' }, studentId),
  myHolds: (studentId: string) => request<Hold[]>('/me/holds', {}, studentId),
}

export function errorMessage(error: unknown): string {
  return error instanceof Error ? error.message : 'Something went wrong.'
}
