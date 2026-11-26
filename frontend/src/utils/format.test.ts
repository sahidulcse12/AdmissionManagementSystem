import { describe, expect, it } from 'vitest'
import { formatCountdown } from './format'

describe('formatCountdown', () => {
  it.each([
    [0, '00:00'],
    [5, '00:05'],
    [65, '01:05'],
    [600, '10:00'],
    [-3, '00:00'],
  ])('%i seconds → %s', (input, expected) => {
    expect(formatCountdown(input)).toBe(expected)
  })
})
