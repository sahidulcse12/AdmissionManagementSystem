# API রেফারেন্স

> English version: [API.md](API.md)

Base URL: `http://localhost:8080`। সব request ও response JSON। সময় ISO-8601 UTC ফরম্যাটে।
Assignment contract-এর প্রতিটি field response-এ আছে; UI-এর সুবিধার জন্য কয়েকটি বাড়তি field যোগ করা হয়েছে
(contract অনুযায়ী এটি অনুমোদিত)।

## নিয়মাবলি

**প্রতিটি shift একটি আলাদা class।** "Class 6 - Morning Shift" আর "Class 6 - Day Shift" দুটি আলাদা class,
প্রত্যেকটির নিজস্ব `totalSeats`।

**শিক্ষার্থী পরিচয়।** শিক্ষার্থীর পক্ষে কাজ করা endpoint-এ
`X-Student-Id: <১৭ অঙ্কের জন্ম নিবন্ধন নম্বর>` header লাগে। অন্য কোনো authentication নেই।

**Error** সবসময় এই আকারে আসে (ভুল input-এ কখনো 500 নয়):

```json
{ "error": { "code": "NO_SEATS_AVAILABLE", "message": "No seats are available in this class." } }
```

| Status | Code |
|---|---|
| 400 | `VALIDATION_ERROR`, `INVALID_REQUEST`, `INVALID_ID`, `MISSING_STUDENT_ID`, `INVALID_STUDENT_ID` |
| 403 | `HOLD_NOT_OWNED` |
| 404 | `CLASS_NOT_FOUND`, `HOLD_NOT_FOUND` |
| 409 | `CLASS_ALREADY_EXISTS`, `NO_SEATS_AVAILABLE`, `ACTIVE_HOLD_EXISTS`, `ALREADY_ADMITTED`, `HOLD_CANCELLED`, `HOLD_ALREADY_CONFIRMED` |
| 410 | `HOLD_EXPIRED` |

## Endpoint সমূহ

### ১. `POST /classes` — নতুন class তৈরি

```json
// request
{ "name": "Class 6 - Morning Shift", "totalSeats": 120 }

// 201 response (Location: /classes/{id})
{ "id": "6b0f…", "name": "Class 6 - Morning Shift", "totalSeats": 120,
  "heldSeats": 0, "confirmedSeats": 0, "availableSeats": 120, "createdAt": "…" }
```

- `name`: ১–১০০ অক্ষর, অনন্য (বড়/ছোট হাতের অক্ষর নির্বিশেষে)। `totalSeats`: ১–১০০০০ পূর্ণসংখ্যা।
- `400 VALIDATION_ERROR` / `INVALID_REQUEST`; `409 CLASS_ALREADY_EXISTS`।

### ২. `GET /classes` — seat-এর হিসাবসহ সব class

```json
[ { "id": "6b0f…", "name": "Class 6 - Morning Shift", "totalSeats": 120,
    "heldSeats": 15, "confirmedSeats": 80, "availableSeats": 25, "createdAt": "…" } ]
```

### ৩. `GET /classes/{classId}` — একটি class-এর seat-এর অবস্থা

উপরের মতোই অবজেক্ট। `400 INVALID_ID` · `404 CLASS_NOT_FOUND`।

### ৪. `POST /classes/{classId}/holds` — আবেদন (seat hold)

`X-Student-Id` header বাধ্যতামূলক। কোনো body নেই।

```json
// 201 response (Location: /holds/{holdId})
{ "holdId": "a91c…", "classId": "6b0f…", "className": "Class 6 - Morning Shift",
  "studentId": "20152691234567890", "status": "HELD", "expiresAt": "2026-11-01T04:10:00+00:00",
  "createdAt": "…", "confirmedAt": null, "cancelledAt": null, "remainingSeconds": 600, "serverTime": "…" }
```

- `409 NO_SEATS_AVAILABLE` — seat নেই।
- `409 ACTIVE_HOLD_EXISTS` / `409 ALREADY_ADMITTED` — এই class-এ শিক্ষার্থীর আগে থেকেই সক্রিয় আবেদন বা ভর্তি আছে।
- `400 MISSING_STUDENT_ID | INVALID_STUDENT_ID | INVALID_ID` · `404 CLASS_NOT_FOUND`।

### ৫. `GET /holds/{holdId}` — আবেদনের বর্তমান অবস্থা

```json
{ "holdId": "a91c…", "classId": "6b0f…", "status": "HELD", "expiresAt": "…", … }
```

`status` হতে পারে `HELD`, `CONFIRMED`, `EXPIRED` বা `CANCELLED`। `expiresAt` পেরিয়ে যাওয়া `HELD` hold সাথে
সাথেই `EXPIRED` দেখায়। এখানে `X-Student-Id` ঐচ্ছিক (contract-এ এটি নেই); পাঠানো হলে এবং অন্য শিক্ষার্থীর হলে
`403 HOLD_NOT_OWNED`। `404 HOLD_NOT_FOUND`।

### ৬. `POST /holds/{holdId}/confirm` — ভর্তি ফি জমা, ভর্তি নিশ্চিত

`X-Student-Id` header বাধ্যতামূলক (শুধু মালিক)। পেমেন্ট সফল ধরে নেওয়া হয়।

```json
// 200 response
{ "holdId": "a91c…", "status": "CONFIRMED", "confirmedAt": "…", … }
```

- Idempotent: আবার confirm করলে একই body সহ `200`; `confirmedSeats` একবারই বাড়ে।
- `410 HOLD_EXPIRED` · `409 HOLD_CANCELLED` · `403 HOLD_NOT_OWNED` · `404 HOLD_NOT_FOUND`।

### ৭. `DELETE /holds/{holdId}` — আবেদন বাতিল

`X-Student-Id` header বাধ্যতামূলক (শুধু মালিক)। সফল হলে **`204 No Content`**; seat খালি হয়।
আবার কল করলেও `204`।

- `409 HOLD_ALREADY_CONFIRMED` (নিশ্চিত ভর্তি চূড়ান্ত) · `410 HOLD_EXPIRED` · `403` · `404`।

### ৮. `GET /me/holds` — এই শিক্ষার্থীর সব আবেদন

`X-Student-Id` header বাধ্যতামূলক। নতুনগুলো আগে, সব অবস্থার।

```json
[ { "holdId": "a91c…", "classId": "6b0f…", "className": "Class 6 - Morning Shift",
    "status": "HELD", "expiresAt": "…", … } ]
```

### বাড়তি endpoint

| Endpoint | উদ্দেশ্য |
|---|---|
| `GET /classes/{classId}/holds` | অ্যাডমিনের জন্য একটি class-এর সব আবেদন/ভর্তি |
| `GET /health` | Docker health check-এর জন্য `{"status":"ok"}` |

## উদাহরণ (curl)

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
