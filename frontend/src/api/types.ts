export type Shift = 'MORNING' | 'DAY' | 'EVENING'
export const SHIFTS: Shift[] = ['MORNING', 'DAY', 'EVENING']

export type HoldStatus = 'HELD' | 'CONFIRMED' | 'EXPIRED' | 'CANCELLED'

export interface SchoolClass {
  id: string
  name: string
  shift: Shift
  capacity: number
  heldSeats: number
  confirmedSeats: number
  availableSeats: number
  createdAt: string
}

export interface CreateClassInput {
  name: string
  shift: Shift
  capacity: number
}

export interface Hold {
  id: string
  classId: string
  className: string
  shift: Shift
  studentId: string
  status: HoldStatus
  createdAt: string
  expiresAt: string
  confirmedAt: string | null
  cancelledAt: string | null
  remainingSeconds: number
  serverTime: string
}
