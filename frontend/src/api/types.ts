export type HoldStatus = 'HELD' | 'CONFIRMED' | 'EXPIRED' | 'CANCELLED'

/** A class. Each shift is its own class, e.g. "Class 6 - Morning Shift". */
export interface SchoolClass {
  id: string
  name: string
  totalSeats: number
  heldSeats: number
  confirmedSeats: number
  availableSeats: number
  createdAt: string
}

export interface CreateClassInput {
  name: string
  totalSeats: number
}

export interface Hold {
  holdId: string
  classId: string
  className: string
  studentId: string
  status: HoldStatus
  createdAt: string
  expiresAt: string
  confirmedAt: string | null
  cancelledAt: string | null
  remainingSeconds: number
  serverTime: string
}
