import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import App from './App'
import { mockFetch, route } from './test/fetchMock'
import { STUDENT, makeClass, makeHold } from './test/fixtures'

beforeEach(() => {
  window.localStorage.clear()
  window.location.hash = '#/'
})
afterEach(() => vi.unstubAllGlobals())

describe('App', () => {
  it('has a student ID box at the top that validates and remembers the 17-digit number', async () => {
    mockFetch([route('GET', '/classes', () => ({ body: [makeClass()] })), route('GET', '/me/holds', () => ({ body: [] }))])
    const user = userEvent.setup()
    render(<App />)

    // Classes are visible before entering an ID, but applying is disabled.
    expect(await screen.findByRole('button', { name: /Apply for Class 6/ })).toBeDisabled()

    const input = screen.getByLabelText(/Student birth registration number/)
    await user.type(input, '12345')
    await user.click(screen.getByRole('button', { name: 'Use this ID' }))
    expect(screen.getByRole('alert')).toHaveTextContent(/17-digit/)

    await user.clear(input)
    await user.type(input, STUDENT)
    await user.click(screen.getByRole('button', { name: 'Use this ID' }))

    expect(await screen.findByText(STUDENT)).toBeInTheDocument()
    expect(window.localStorage.getItem('admission.studentId')).toBe(STUDENT)
    expect(await screen.findByRole('button', { name: /Apply for Class 6/ })).toBeEnabled()
  })

  it('restores the student and their running application after a reload / browser restart', async () => {
    window.localStorage.setItem('admission.studentId', STUDENT)
    mockFetch([
      route('GET', '/classes', () => ({ body: [makeClass()] })),
      route('GET', '/me/holds', () => ({ body: [makeHold({ remainingSeconds: 300 })] })),
    ])

    render(<App />)

    expect(screen.getByText(STUDENT)).toBeInTheDocument()
    expect(await screen.findByText('05:00')).toBeInTheDocument()
  })
})
