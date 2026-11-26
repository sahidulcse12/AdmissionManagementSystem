# API Reference

> Bangla version: [API.bn.md](API.bn.md)

Base URL: `http://localhost:8080`. All bodies are JSON. Timestamps are ISO-8601 UTC.

## Conventions

**Student identity.** Endpoints that act for a student require the header
`X-Student-Id: <17-digit birth registration number>`. There is no other authentication.

**Errors** always have this shape:

```json
{ "error": { "code": "NO_SEATS_AVAILABLE", "message": "No seats are available in this class." } }
```

| Status | Codes |
|---|---|
| 400 | `VALIDATION_ERROR`, `INVALID_REQUEST`, `INVALID_ID`, `MISSING_STUDENT_ID`, `INVALID_STUDENT_ID` |
| 403 | `HOLD_NOT_OWNED` |
| 404 | `CLASS_NOT_FOUND`, `HOLD_NOT_FOUND` |
| 409 | `CLASS_ALREADY_EXISTS`, `NO_SEATS_AVAILABLE`, `ACTIVE_HOLD_EXISTS`, `ALREADY_ADMITTED`, `HOLD_CANCELLED`, `HOLD_ALREADY_CONFIRMED` |
| 410 | `HOLD_EXPIRED` |
| 500 | `INTERNAL_ERROR` |

## Objects

**Class**

```json
{
  "id": "6b0f…", "name": "Class 1", "shift": "MORNING", "capacity": 30,
  "heldSeats": 2, "confirmedSeats": 10, "availableSeats": 18,
  "createdAt": "2026-11-27T05:00:00.123456+00:00"
}
```

**Hold**

```json
{
  "id": "a91c…", "classId": "6b0f…", "className": "Class 1", "shift": "MORNING",
  "studentId": "20150123456789012", "status": "HELD",
  "createdAt": "…", "expiresAt": "…", "confirmedAt": null, "cancelledAt": null,
  "remainingSeconds": 598, "serverTime": "…"
}
```

`status` is one of `HELD`, `CONFIRMED`, `EXPIRED`, `CANCELLED`. A `HELD` hold past `expiresAt`
is returned as `EXPIRED`. `remainingSeconds` is `0` unless the hold is `HELD`.

## Endpoints

### `POST /classes` — create a class (admin)

```json
{ "name": "Class 1", "shift": "MORNING", "capacity": 30 }
```

- `shift`: `MORNING`, `DAY` or `EVENING` (case-insensitive). `capacity`: 1–1000. `name`: 1–100 chars.
- `201 Created` + `Location: /classes/{id}` + Class.
- `400 VALIDATION_ERROR`; `409 CLASS_ALREADY_EXISTS` (same name in the same shift, case-insensitive).

### `GET /classes` — list classes with live seat counts

`200` + `Class[]`, sorted by name then shift.

### `GET /classes/{classId}` — one class

`200` + Class · `400 INVALID_ID` · `404 CLASS_NOT_FOUND`.

### `GET /classes/{classId}/holds` — holds and admissions of a class (admin)

`200` + `Hold[]`, newest first · `404 CLASS_NOT_FOUND`.

### `POST /classes/{classId}/holds` — reserve a seat

Header `X-Student-Id` required. No body.

- `201 Created` + `Location: /holds/{id}` + Hold (`status: HELD`, `expiresAt = now + HOLD_DURATION_SECONDS`).
- `400 MISSING_STUDENT_ID | INVALID_STUDENT_ID | INVALID_ID`
- `404 CLASS_NOT_FOUND`
- `409 NO_SEATS_AVAILABLE` — class is full.
- `409 ACTIVE_HOLD_EXISTS` — the student already holds a seat in this class.
- `409 ALREADY_ADMITTED` — the student is already admitted to this class.

### `GET /holds/{holdId}` — read a hold (owner only)

`200` + Hold · `403 HOLD_NOT_OWNED` · `404 HOLD_NOT_FOUND`.

### `POST /holds/{holdId}/confirm` — confirm admission (payment assumed successful)

Owner only. **Idempotent**: repeating the call returns `200` with the same body and does not
create another admission.

- `200` + Hold (`status: CONFIRMED`)
- `403 HOLD_NOT_OWNED` · `404 HOLD_NOT_FOUND`
- `409 HOLD_CANCELLED` — cancelled holds cannot be confirmed.
- `410 HOLD_EXPIRED` — expired holds cannot be confirmed.

### `DELETE /holds/{holdId}` — cancel a hold

Owner only. Releases the seat immediately. Repeating the call returns `200` (no-op).

- `200` + Hold (`status: CANCELLED`)
- `403 HOLD_NOT_OWNED` · `404 HOLD_NOT_FOUND`
- `409 HOLD_ALREADY_CONFIRMED` — a confirmed admission cannot be cancelled.
- `410 HOLD_EXPIRED`

### `GET /me/holds` — the student's history

Header `X-Student-Id` required. `200` + `Hold[]`, newest first, all statuses.

### `GET /health`

`200 {"status":"ok"}` — used by the Docker health check.

## Example session (curl)

```bash
API=http://localhost:8080
STUDENT=20150123456789012

CLASS=$(curl -s -X POST $API/classes -H 'Content-Type: application/json' \
  -d '{"name":"Class 1","shift":"MORNING","capacity":2}' | jq -r .id)

HOLD=$(curl -s -X POST $API/classes/$CLASS/holds -H "X-Student-Id: $STUDENT" | jq -r .id)

curl -s -X POST $API/holds/$HOLD/confirm -H "X-Student-Id: $STUDENT" | jq .status   # "CONFIRMED"
curl -s $API/me/holds -H "X-Student-Id: $STUDENT" | jq '.[].status'
```
