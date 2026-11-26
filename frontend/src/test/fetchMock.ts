import { vi } from 'vitest'

export interface MockCall {
  method: string
  url: string
  headers: Headers
  body: unknown
}

type Handler = (call: MockCall) => { status?: number; body?: unknown } | undefined

export function mockFetch(handlers: Handler[]) {
  const calls: MockCall[] = []
  const fn = vi.fn(async (input: RequestInfo | URL, init: RequestInit = {}) => {
    const call: MockCall = {
      method: (init.method ?? 'GET').toUpperCase(),
      url: String(input).replace(/^https?:\/\/[^/]+/, ''),
      headers: new Headers(init.headers),
      body: init.body ? JSON.parse(String(init.body)) : undefined,
    }
    calls.push(call)
    for (const handler of handlers) {
      const result = handler(call)
      if (result) {
        return new Response(result.body === undefined ? null : JSON.stringify(result.body), {
          status: result.status ?? 200,
          headers: { 'Content-Type': 'application/json' },
        })
      }
    }
    throw new Error(`Unhandled request ${call.method} ${call.url}`)
  })
  vi.stubGlobal('fetch', fn)
  return { calls, fn }
}

export const route = (method: string, url: string | RegExp, respond: (call: MockCall) => { status?: number; body?: unknown }): Handler =>
  (call) => (call.method === method && (typeof url === 'string' ? call.url === url : url.test(call.url)) ? respond(call) : undefined)
