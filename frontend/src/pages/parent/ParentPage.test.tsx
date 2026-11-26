import { act, render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import type { Hold } from '../../api/types'
import { mockFetch, route } from '../../test/fetchMock'
import { STUDENT, makeClass, makeHold } from '../../test/fixtures'
import { ParentPage } from './ParentPage'

beforeEach(() => window.localStorage.clear())
afterEach(() => {
  vi.unstubAllGlobals()
  vi.useRealTimers()
})

describe('ParentPage', () => {
  it('requires a 17-digit student id and remembers it', async () => {
    mockFetch([route('GET', '/classes', () => ({ body: [] })), route('GET', '/me/holds', () => ({ body: [] }))])
    const user = userEvent.setup()
    render(<ParentPage />)

    await user.type(screen.getByLabelText('Birth registration number'), '12345')
    await user.click(screen.getByRole('button', { name: 'Continue' }))
    expect(screen.getByRole('alert')).toHaveTextContent(/17-digit/)

    await user.clear(screen.getByLabelText('Birth registration number'))
    await user.type(screen.getByLabelText('Birth registration number'), STUDENT)
    await user.click(screen.getByRole('button', { name: 'Continue' }))

    expect(await screen.findByText(STUDENT)).toBeInTheDocument()
    expect(window.localStorage.getItem('admission.studentId')).toBe(STUDENT)
  })

  it('reserves a seat with the student id header and shows the countdown', async () => {
    window.localStorage.setItem('admission.studentId', STUDENT)
    let holds: Hold[] = []
    const { calls } = mockFetch([
      route('GET', '/classes', () => ({ body: [makeClass()] })),
      route('GET', '/me/holds', () => ({ body: holds })),
      route('POST', '/classes/c1/holds', () => {
        holds = [makeHold({ remainingSeconds: 125 })]
        return { status: 201, body: holds[0] }
      }),
    ])
    const user = userEvent.setup()
    render(<ParentPage />)

    await user.click(await screen.findByRole('button', { name: /Reserve a seat in Class 1/ }))

    expect(await screen.findByText(/Seat reserved in Class 1/)).toBeInTheDocument()
    expect(await screen.findByText('02:05')).toBeInTheDocument()
    const post = calls.find((c) => c.method === 'POST')!
    expect(post.headers.get('X-Student-Id')).toBe(STUDENT)
    expect(screen.getByRole('button', { name: /Reserve a seat in Class 1/ })).toHaveTextContent('Already reserved')
  })

  it('restores an active hold from the backend after a reload', async () => {
    window.localStorage.setItem('admission.studentId', STUDENT)
    mockFetch([
      route('GET', '/classes', () => ({ body: [makeClass()] })),
      route('GET', '/me/holds', () => ({ body: [makeHold({ remainingSeconds: 300 })] })),
    ])

    render(<ParentPage />)

    expect(await screen.findByText('Reserved seats awaiting confirmation')).toBeInTheDocument()
    expect(screen.getByText('05:00')).toBeInTheDocument()
  })

  it('confirms admission and moves the hold into history', async () => {
    window.localStorage.setItem('admission.studentId', STUDENT)
    let holds = [makeHold()]
    const { calls } = mockFetch([
      route('GET', '/classes', () => ({ body: [makeClass()] })),
      route('GET', '/me/holds', () => ({ body: holds })),
      route('POST', '/holds/h1/confirm', () => {
        holds = [makeHold({ status: 'CONFIRMED', confirmedAt: '2026-01-01T00:03:00Z', remainingSeconds: 0 })]
        return { body: holds[0] }
      }),
    ])
    const user = userEvent.setup()
    render(<ParentPage />)

    await user.click(await screen.findByRole('button', { name: 'Pay & confirm admission' }))

    expect(await screen.findByText(/Admission confirmed for Class 1/)).toBeInTheDocument()
    await waitFor(() => expect(screen.queryByText('Reserved seats awaiting confirmation')).not.toBeInTheDocument())
    expect(screen.getByText('Admitted', { selector: '.badge' })).toBeInTheDocument()
    expect(calls.find((c) => c.url === '/holds/h1/confirm')!.headers.get('X-Student-Id')).toBe(STUDENT)
  })

  it('cancels a hold', async () => {
    window.localStorage.setItem('admission.studentId', STUDENT)
    let holds = [makeHold()]
    mockFetch([
      route('GET', '/classes', () => ({ body: [makeClass()] })),
      route('GET', '/me/holds', () => ({ body: holds })),
      route('DELETE', '/holds/h1', () => {
        holds = [makeHold({ status: 'CANCELLED', cancelledAt: '2026-01-01T00:01:00Z' })]
        return { body: holds[0] }
      }),
    ])
    const user = userEvent.setup()
    render(<ParentPage />)

    await user.click(await screen.findByRole('button', { name: 'Cancel hold' }))

    expect(await screen.findByText(/cancelled\. The seat has been released/)).toBeInTheDocument()
    expect(await screen.findByText('Cancelled', { selector: '.badge' })).toBeInTheDocument()
  })

  it('shows the server error when the class is full', async () => {
    window.localStorage.setItem('admission.studentId', STUDENT)
    mockFetch([
      route('GET', '/classes', () => ({ body: [makeClass({ availableSeats: 1 })] })),
      route('GET', '/me/holds', () => ({ body: [] })),
      route('POST', '/classes/c1/holds', () => ({
        status: 409,
        body: { error: { code: 'NO_SEATS_AVAILABLE', message: 'No seats are available in this class.' } },
      })),
    ])
    const user = userEvent.setup()
    render(<ParentPage />)

    await user.click(await screen.findByRole('button', { name: /Reserve a seat/ }))

    expect(await screen.findByRole('alert')).toHaveTextContent('No seats are available in this class.')
  })

  it('disables reservation for full classes', async () => {
    window.localStorage.setItem('admission.studentId', STUDENT)
    mockFetch([
      route('GET', '/classes', () => ({ body: [makeClass({ availableSeats: 0, heldSeats: 4, confirmedSeats: 6 })] })),
      route('GET', '/me/holds', () => ({ body: [] })),
    ])
    render(<ParentPage />)

    const button = await screen.findByRole('button', { name: /Reserve a seat/ })
    expect(button).toBeDisabled()
    expect(button).toHaveTextContent('Full')
  })

  it('re-reads the backend when the countdown reaches zero', async () => {
    vi.useFakeTimers({ shouldAdvanceTime: true })
    window.localStorage.setItem('admission.studentId', STUDENT)
    let holds = [makeHold({ remainingSeconds: 2 })]
    const { calls } = mockFetch([
      route('GET', '/classes', () => ({ body: [makeClass()] })),
      route('GET', '/me/holds', () => ({ body: holds })),
    ])
    render(<ParentPage />)
    expect(await screen.findByText('00:02')).toBeInTheDocument()

    holds = [makeHold({ status: 'EXPIRED', remainingSeconds: 0 })]
    const before = calls.filter((c) => c.url === '/me/holds').length
    await act(async () => {
      await vi.advanceTimersByTimeAsync(2500)
    })

    await waitFor(() => expect(calls.filter((c) => c.url === '/me/holds').length).toBeGreaterThan(before))
    expect(await screen.findByText('Expired', { selector: '.badge' })).toBeInTheDocument()
  })
})
