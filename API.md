# API Reference

> Bangla version: [API.bn.md](API.bn.md)

Base URL: `http://localhost:8080`. All requests and responses are JSON. Timestamps are ISO-8601 UTC.
Responses contain every field required by the assignment contract; a few extra fields are added
(allowed by the contract) to help the UI.

**Interactive docs (Swagger):** http://localhost:8080/swagger — try every endpoint from the browser.
OpenAPI document: http://localhost:8080/swagger/v1/swagger.json.

## API versioning

The API is versioned without changing any path, so the contract URLs below stay exactly as specified.

| How to choose a version | Example |
|---|---|
| Header | `X-Api-Version: 1.0` |
| Query string | `GET /classes?api-version=1.0` |
| Nothing | Version `1.0` is assumed |

- Every response carries `api-supported-versions: 1.0`.
- An unsupported version (e.g. `X-Api-Version: 9.0`) returns `400` with an "Unsupported API version"
  problem response.
- *Why not `/v1/classes`?* The assignment's automated tests call the exact paths `/classes`,
  `/holds/{holdId}`, … — a path prefix would break them. A future `2.0` can be added side by side and
  selected with the same header/query.

## Conventions

**Each shift is its own class.** "Class 6 - Morning Shift" and "Class 6 - Day Shift" are two classes,
each with its own `totalSeats`.

**Student identity.** Endpoints that act for a student take the header
`X-Student-Id: <17-digit birth registration number>`. There is no other authentication.

**Errors** always have this shape (never a 500 for bad input):

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

## Endpoints

### 1. `POST /classes` — create a class

```json
// request
{ "name": "Class 6 - Morning Shift", "totalSeats": 120 }

// 201 response (Location: /classes/{id})
{ "id": "6b0f…", "name": "Class 6 - Morning Shift", "totalSeats": 120,
  "heldSeats": 0, "confirmedSeats": 0, "availableSeats": 120, "createdAt": "…" }
```

- `name`: 1–100 characters, unique (case-insensitive). `totalSeats`: whole number 1–10000.
- `400 VALIDATION_ERROR` / `INVALID_REQUEST`; `409 CLASS_ALREADY_EXISTS`.

### 2. `GET /classes` — all classes with seat counts

```json
[ { "id": "6b0f…", "name": "Class 6 - Morning Shift", "totalSeats": 120,
    "heldSeats": 15, "confirmedSeats": 80, "availableSeats": 25, "createdAt": "…" } ]
```

### 3. `GET /classes/{classId}` — one class's seats

Same object as above. `400 INVALID_ID` · `404 CLASS_NOT_FOUND`.

### 4. `POST /classes/{classId}/holds` — apply (hold a seat)

Header `X-Student-Id` required. No body.

```json
// 201 response (Location: /holds/{holdId})
{ "holdId": "a91c…", "classId": "6b0f…", "className": "Class 6 - Morning Shift",
  "studentId": "20152691234567890", "status": "HELD", "expiresAt": "2026-11-01T04:10:00+00:00",
  "createdAt": "…", "confirmedAt": null, "cancelledAt": null, "remainingSeconds": 600, "serverTime": "…" }
```

- `409 NO_SEATS_AVAILABLE` — no seat left.
- `409 ACTIVE_HOLD_EXISTS` / `409 ALREADY_ADMITTED` — the student already has an active application or admission in this class.
- `400 MISSING_STUDENT_ID | INVALID_STUDENT_ID | INVALID_ID` · `404 CLASS_NOT_FOUND`.

### 5. `GET /holds/{holdId}` — current status of an application

```json
{ "holdId": "a91c…", "classId": "6b0f…", "status": "HELD", "expiresAt": "…", … }
```

`status` is `HELD`, `CONFIRMED`, `EXPIRED` or `CANCELLED`. A `HELD` hold past `expiresAt` is
reported as `EXPIRED` immediately. `X-Student-Id` is optional here (the contract does not list it);
if it is sent and belongs to another student the answer is `403 HOLD_NOT_OWNED`. `404 HOLD_NOT_FOUND`.

### 6. `POST /holds/{holdId}/confirm` — pay admission fee, confirm admission

Header `X-Student-Id` required (owner only). Payment is assumed successful.

```json
// 200 response
{ "holdId": "a91c…", "status": "CONFIRMED", "confirmedAt": "…", … }
```

- Idempotent: confirming again returns `200` with the same body; `confirmedSeats` increases only once.
- `410 HOLD_EXPIRED` · `409 HOLD_CANCELLED` · `403 HOLD_NOT_OWNED` · `404 HOLD_NOT_FOUND`.

### 7. `DELETE /holds/{holdId}` — cancel an application

Header `X-Student-Id` required (owner only). Success: **`204 No Content`**; the seat is released.
Repeating the call also returns `204`.

- `409 HOLD_ALREADY_CONFIRMED` (a confirmed admission is final) · `410 HOLD_EXPIRED` · `403` · `404`.

### 8. `GET /me/holds` — all applications of this student

Header `X-Student-Id` required. Newest first, every status.

```json
[ { "holdId": "a91c…", "classId": "6b0f…", "className": "Class 6 - Morning Shift",
    "status": "HELD", "expiresAt": "…", … } ]
```

### Extra endpoints

| Endpoint | Purpose |
|---|---|
| `GET /classes/{classId}/holds` | Admin view of every application/admission in a class |
| `GET /health` | `{"status":"ok"}` for the Docker health check |

## Example session (curl)

```bash
API=http://localhost:8080
STUDENT=20152691234567890

CLASS=$(curl -s -X POST $API/classes -H 'Content-Type: application/json' \
  -d '{"name":"Class 6 - Morning Shift","totalSeats":120}' | jq -r .id)

HOLD=$(curl -s -X POST $API/classes/$CLASS/holds -H "X-Student-Id: $STUDENT" | jq -r .holdId)

curl -s $API/holds/$HOLD | jq .status                                          # "HELD"
curl -s -X POST $API/holds/$HOLD/confirm -H "X-Student-Id: $STUDENT" | jq .status  # "CONFIRMED"
curl -s $API/me/holds -H "X-Student-Id: $STUDENT" | jq '.[].status'
```
