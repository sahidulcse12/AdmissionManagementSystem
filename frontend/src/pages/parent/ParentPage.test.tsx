import { act, render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, describe, expect, it, vi } from 'vitest'
import type { Hold } from '../../api/types'
import { mockFetch, route } from '../../test/fetchMock'
import { STUDENT, makeClass, makeHold } from '../../test/fixtures'
import { ParentPage } from './ParentPage'

afterEach(() => {
  vi.unstubAllGlobals()
  vi.useRealTimers()
})

const APPLY = /Apply for Class 6 - Morning Shift/
const PAY = 'ভর্তি ফি দিন (Pay admission fee)'
const CANCEL = 'বাতিল (Cancel)'

describe('ParentPage', () => {
  it('applies with the X-Student-Id header and shows the countdown', async () => {
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
    render(<ParentPage studentId={STUDENT} />)

    await user.click(await screen.findByRole('button', { name: APPLY }))

    expect(await screen.findByText(/Seat held in Class 6 - Morning Shift/)).toBeInTheDocument()
    expect(await screen.findByText('02:05')).toBeInTheDocument()
    expect(calls.find((c) => c.method === 'POST')!.headers.get('X-Student-Id')).toBe(STUDENT)
    expect(screen.getByRole('button', { name: APPLY })).toHaveTextContent('Already applied')
  })

  it('shows total, held, confirmed and available seats for each class', async () => {
    mockFetch([route('GET', '/classes', () => ({ body: [makeClass()] })), route('GET', '/me/holds', () => ({ body: [] }))])
    render(<ParentPage studentId={STUDENT} />)

    const card = (await screen.findByRole('heading', { name: 'Class 6 - Morning Shift' })).closest('article')!
    expect(card).toHaveTextContent('Total10')
    expect(card).toHaveTextContent('Held2')
    expect(card).toHaveTextContent('Admitted3')
    expect(card).toHaveTextContent('Available5')
  })

  it('pays the admission fee (confirms) and moves the application into history', async () => {
    let holds = [makeHold()]
    const { calls } = mockFetch([
      route('GET', '/classes', () => ({ body: [makeClass()] })),
      route('GET', '/me/holds', () => ({ body: holds })),
      route('POST', '/holds/h1/confirm', () => {
        holds = [makeHold({ status: 'CONFIRMED', confirmedAt: '2026-01-01T00:03:00Z', remainingSeconds: 0 })]
        return { body: { holdId: 'h1', status: 'CONFIRMED' } }
      }),
    ])
    const user = userEvent.setup()
    render(<ParentPage studentId={STUDENT} />)

    await user.click(await screen.findByRole('button', { name: PAY }))

    expect(await screen.findByText(/Admission confirmed for Class 6/)).toBeInTheDocument()
    await waitFor(() => expect(screen.queryByRole('button', { name: PAY })).not.toBeInTheDocument())
    expect(screen.getByText('Admitted', { selector: '.badge' })).toBeInTheDocument()
    expect(calls.find((c) => c.url === '/holds/h1/confirm')!.headers.get('X-Student-Id')).toBe(STUDENT)
  })

  it('cancels an application (204 No Content)', async () => {
    let holds = [makeHold()]
    mockFetch([
      route('GET', '/classes', () => ({ body: [makeClass()] })),
      route('GET', '/me/holds', () => ({ body: holds })),
      route('DELETE', '/holds/h1', () => {
        holds = [makeHold({ status: 'CANCELLED', cancelledAt: '2026-01-01T00:01:00Z' })]
        return { status: 204 }
      }),
    ])
    const user = userEvent.setup()
    render(<ParentPage studentId={STUDENT} />)

    await user.click(await screen.findByRole('button', { name: CANCEL }))

    expect(await screen.findByText(/cancelled\. The seat has been released/)).toBeInTheDocument()
    expect(await screen.findByText('Cancelled', { selector: '.badge' })).toBeInTheDocument()
  })

  it('shows the server error when the class is full', async () => {
    mockFetch([
      route('GET', '/classes', () => ({ body: [makeClass({ availableSeats: 1 })] })),
      route('GET', '/me/holds', () => ({ body: [] })),
      route('POST', '/classes/c1/holds', () => ({
        status: 409,
        body: { error: { code: 'NO_SEATS_AVAILABLE', message: 'No seats are available in this class.' } },
      })),
    ])
    const user = userEvent.setup()
    render(<ParentPage studentId={STUDENT} />)

    await user.click(await screen.findByRole('button', { name: APPLY }))

    expect(await screen.findByRole('alert')).toHaveTextContent('No seats are available in this class.')
  })

  it('disables applying for full classes', async () => {
    mockFetch([
      route('GET', '/classes', () => ({ body: [makeClass({ availableSeats: 0, heldSeats: 4, confirmedSeats: 6 })] })),
      route('GET', '/me/holds', () => ({ body: [] })),
    ])
    render(<ParentPage studentId={STUDENT} />)

    const button = await screen.findByRole('button', { name: APPLY })
    expect(button).toBeDisabled()
    expect(button).toHaveTextContent('Full')
  })

  it('shows EXPIRED and disables the pay button without a page reload when time runs out', async () => {
    vi.useFakeTimers({ shouldAdvanceTime: true })
    let holds = [makeHold({ remainingSeconds: 2 })]
    const { calls } = mockFetch([
      route('GET', '/classes', () => ({ body: [makeClass()] })),
      route('GET', '/me/holds', () => ({ body: holds })),
    ])
    render(<ParentPage studentId={STUDENT} />)
    expect(await screen.findByText('00:02')).toBeInTheDocument()

    // Keep the server answering HELD for a moment so the card's own expiry state is observable.
    await act(async () => {
      await vi.advanceTimersByTimeAsync(2100)
    })
    expect(screen.getByText('EXPIRED', { selector: '.countdown' })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: PAY })).toBeDisabled()

    // The page then re-reads the backend and history shows the EXPIRED status.
    holds = [makeHold({ status: 'EXPIRED', remainingSeconds: 0 })]
    const before = calls.filter((c) => c.url === '/me/holds').length
    await act(async () => {
      await vi.advanceTimersByTimeAsync(5100)
    })
    await waitFor(() => expect(calls.filter((c) => c.url === '/me/holds').length).toBeGreaterThan(before))
    expect(await screen.findByText('Expired', { selector: '.badge' })).toBeInTheDocument()
  })

  it('shows a readable message when the server is down', async () => {
    vi.stubGlobal('fetch', vi.fn().mockRejectedValue(new TypeError('Failed to fetch')))
    render(<ParentPage studentId={STUDENT} />)
    expect((await screen.findAllByRole('alert'))[0]).toHaveTextContent(/Cannot reach the server/)
  })
})
