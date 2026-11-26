import { render, screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { makeClass, makeHold } from '../../test/fixtures'
import { mockFetch, route } from '../../test/fetchMock'
import { AdminPage } from './AdminPage'

afterEach(() => vi.unstubAllGlobals())

describe('AdminPage', () => {
  it('shows total, held, admitted and available seats per class with totals', async () => {
    mockFetch([
      route('GET', '/classes', () => ({
        body: [makeClass(), makeClass({ id: 'c2', name: 'Class 6 - Day Shift', totalSeats: 5, heldSeats: 0, confirmedSeats: 5, availableSeats: 0 })],
      })),
    ])

    render(<AdminPage />)

    const row = (await screen.findByText('Class 6 - Day Shift')).closest('tr')!
    expect(within(row).getAllByRole('cell').slice(0, 5).map((c) => c.textContent)).toEqual(['Class 6 - Day Shift', '5', '5', '0', '0'])
    expect(screen.getByText('Total seats', { selector: 'dt' }).nextSibling).toHaveTextContent('15')
    expect(screen.getByText('Available', { selector: 'dt' }).nextSibling).toHaveTextContent('5')
  })

  it('shows an empty state when there are no classes', async () => {
    mockFetch([route('GET', '/classes', () => ({ body: [] }))])
    render(<AdminPage />)
    expect(await screen.findByText(/No classes yet/)).toBeInTheDocument()
  })

  it('shows an error when the API is unreachable', async () => {
    vi.stubGlobal('fetch', vi.fn().mockRejectedValue(new TypeError('Failed to fetch')))
    render(<AdminPage />)
    expect(await screen.findByRole('alert')).toHaveTextContent(/Cannot reach the server/)
  })

  it('creates a class with name and totalSeats and refreshes the list', async () => {
    let classes = [makeClass()]
    const { calls } = mockFetch([
      route('GET', '/classes', () => ({ body: classes })),
      route('POST', '/classes', (call) => {
        const created = makeClass({ id: 'new', ...(call.body as object), heldSeats: 0, confirmedSeats: 0, availableSeats: 120 })
        classes = [...classes, created]
        return { status: 201, body: created }
      }),
    ])
    const user = userEvent.setup()
    render(<AdminPage />)
    await screen.findByText('Class 6 - Morning Shift')

    await user.type(screen.getByLabelText('Class name'), 'Class 7 - Day Shift')
    await user.clear(screen.getByLabelText('Total seats'))
    await user.type(screen.getByLabelText('Total seats'), '120')
    await user.click(screen.getByRole('button', { name: 'Create class' }))

    expect(await screen.findByText(/Created "Class 7 - Day Shift" with 120 seats/)).toBeInTheDocument()
    expect(calls.find((c) => c.method === 'POST')?.body).toEqual({ name: 'Class 7 - Day Shift', totalSeats: 120 })
    expect(await screen.findByRole('cell', { name: 'Class 7 - Day Shift' })).toBeInTheDocument()
  })

  it('validates seats before calling the API and shows server errors', async () => {
    const { calls } = mockFetch([
      route('GET', '/classes', () => ({ body: [] })),
      route('POST', '/classes', () => ({ status: 409, body: { error: { code: 'CLASS_ALREADY_EXISTS', message: "A class named 'X' already exists." } } })),
    ])
    const user = userEvent.setup()
    render(<AdminPage />)

    await user.type(screen.getByLabelText('Class name'), 'X')
    await user.clear(screen.getByLabelText('Total seats'))
    await user.type(screen.getByLabelText('Total seats'), '0')
    await user.click(screen.getByRole('button', { name: 'Create class' }))
    expect(screen.getByRole('alert')).toHaveTextContent(/1 to 10000/)
    expect(calls.some((c) => c.method === 'POST')).toBe(false)

    await user.clear(screen.getByLabelText('Total seats'))
    await user.type(screen.getByLabelText('Total seats'), '10')
    await user.click(screen.getByRole('button', { name: 'Create class' }))
    expect(await screen.findByRole('alert')).toHaveTextContent(/already exists/)
  })

  it('lists admissions for a selected class', async () => {
    mockFetch([
      route('GET', '/classes', () => ({ body: [makeClass()] })),
      route('GET', '/classes/c1/holds', () => ({ body: [makeHold({ status: 'CONFIRMED', confirmedAt: '2026-01-01T00:05:00Z' })] })),
    ])
    const user = userEvent.setup()
    render(<AdminPage />)

    await user.click(await screen.findByRole('button', { name: 'View admissions' }))

    await waitFor(() => expect(screen.getByText('Admitted', { selector: '.badge' })).toBeInTheDocument())
    expect(screen.getByText('20150123456789012')).toBeInTheDocument()
  })
})
