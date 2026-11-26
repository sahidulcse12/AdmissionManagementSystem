import type { Hold, SchoolClass } from '../api/types'

export const STUDENT = '20150123456789012'

export function makeClass(overrides: Partial<SchoolClass> = {}): SchoolClass {
  return {
    id: 'c1',
    name: 'Class 1',
    shift: 'MORNING',
    capacity: 10,
    heldSeats: 2,
    confirmedSeats: 3,
    availableSeats: 5,
    createdAt: '2026-01-01T00:00:00Z',
    ...overrides,
  }
}

export function makeHold(overrides: Partial<Hold> = {}): Hold {
  return {
    id: 'h1',
    classId: 'c1',
    className: 'Class 1',
    shift: 'MORNING',
    studentId: STUDENT,
    status: 'HELD',
    createdAt: '2026-01-01T00:00:00Z',
    expiresAt: '2026-01-01T00:10:00Z',
    confirmedAt: null,
    cancelledAt: null,
    remainingSeconds: 600,
    serverTime: '2026-01-01T00:00:00Z',
    ...overrides,
  }
}
