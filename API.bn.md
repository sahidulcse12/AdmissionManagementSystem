# API রেফারেন্স

> English version: [API.md](API.md)

Base URL: `http://localhost:8080`। সব body JSON। সময় ISO-8601 UTC ফরম্যাটে।

## নিয়মাবলি

**শিক্ষার্থী পরিচয়।** যেসব endpoint শিক্ষার্থীর পক্ষে কাজ করে, সেগুলোতে
`X-Student-Id: <১৭ অঙ্কের জন্ম নিবন্ধন নম্বর>` header লাগবে। অন্য কোনো authentication নেই।

**Error** সবসময় এই আকারে আসে:

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
| 500 | `INTERNAL_ERROR` |

## অবজেক্ট

**Class (ক্লাস)**

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

`status` হতে পারে `HELD`, `CONFIRMED`, `EXPIRED` বা `CANCELLED`। `expiresAt` পার হয়ে যাওয়া `HELD` hold
`EXPIRED` হিসেবে ফেরত আসে। hold `HELD` না হলে `remainingSeconds` হয় `0`।

## Endpoint সমূহ

### `POST /classes` — ক্লাস তৈরি (অ্যাডমিন)

```json
{ "name": "Class 1", "shift": "MORNING", "capacity": 30 }
```

- `shift`: `MORNING`, `DAY` অথবা `EVENING` (বড়/ছোট হাতের অক্ষর নির্বিশেষে)। `capacity`: ১–১০০০। `name`: ১–১০০ অক্ষর।
- `201 Created` + `Location: /classes/{id}` + Class।
- `400 VALIDATION_ERROR`; `409 CLASS_ALREADY_EXISTS` (একই শিফটে একই নাম)।

### `GET /classes` — বর্তমান সিট সংখ্যাসহ ক্লাসের তালিকা

`200` + `Class[]`, নাম ও শিফট অনুযায়ী সাজানো।

### `GET /classes/{classId}` — একটি ক্লাস

`200` + Class · `400 INVALID_ID` · `404 CLASS_NOT_FOUND`।

### `GET /classes/{classId}/holds` — একটি ক্লাসের সব hold ও ভর্তি (অ্যাডমিন)

`200` + `Hold[]`, নতুনগুলো আগে · `404 CLASS_NOT_FOUND`।

### `POST /classes/{classId}/holds` — সিট রিজার্ভ

`X-Student-Id` header বাধ্যতামূলক। কোনো body নেই।

- `201 Created` + `Location: /holds/{id}` + Hold (`status: HELD`, `expiresAt = now + HOLD_DURATION_SECONDS`)।
- `400 MISSING_STUDENT_ID | INVALID_STUDENT_ID | INVALID_ID`
- `404 CLASS_NOT_FOUND`
- `409 NO_SEATS_AVAILABLE` — ক্লাস পূর্ণ।
- `409 ACTIVE_HOLD_EXISTS` — শিক্ষার্থীর এই ক্লাসে আগে থেকেই একটি সক্রিয় hold আছে।
- `409 ALREADY_ADMITTED` — শিক্ষার্থী এই ক্লাসে আগেই ভর্তি হয়েছে।

### `GET /holds/{holdId}` — hold দেখা (শুধু মালিক)

`200` + Hold · `403 HOLD_NOT_OWNED` · `404 HOLD_NOT_FOUND`।

### `POST /holds/{holdId}/confirm` — ভর্তি নিশ্চিত (পেমেন্ট সফল ধরে নেওয়া হয়)

শুধু মালিক। **Idempotent**: আবার কল করলে একই body সহ `200` ফেরত আসে এবং নতুন ভর্তি তৈরি হয় না।

- `200` + Hold (`status: CONFIRMED`)
- `403 HOLD_NOT_OWNED` · `404 HOLD_NOT_FOUND`
- `409 HOLD_CANCELLED` — বাতিল hold confirm করা যায় না।
- `410 HOLD_EXPIRED` — মেয়াদোত্তীর্ণ hold confirm করা যায় না।

### `DELETE /holds/{holdId}` — hold বাতিল

শুধু মালিক। সাথে সাথে সিট খালি হয়। আবার কল করলে `200` আসে (কোনো পরিবর্তন হয় না)।

- `200` + Hold (`status: CANCELLED`)
- `403 HOLD_NOT_OWNED` · `404 HOLD_NOT_FOUND`
- `409 HOLD_ALREADY_CONFIRMED` — নিশ্চিত ভর্তি বাতিল করা যায় না।
- `410 HOLD_EXPIRED`

### `GET /me/holds` — শিক্ষার্থীর ইতিহাস

`X-Student-Id` header বাধ্যতামূলক। `200` + `Hold[]`, নতুনগুলো আগে, সব অবস্থার।

### `GET /health`

`200 {"status":"ok"}` — Docker health check-এর জন্য ব্যবহৃত।

## উদাহরণ (curl)

```bash
API=http://localhost:8080
STUDENT=20150123456789012

CLASS=$(curl -s -X POST $API/classes -H 'Content-Type: application/json' \
  -d '{"name":"Class 1","shift":"MORNING","capacity":2}' | jq -r .id)

HOLD=$(curl -s -X POST $API/classes/$CLASS/holds -H "X-Student-Id: $STUDENT" | jq -r .id)

curl -s -X POST $API/holds/$HOLD/confirm -H "X-Student-Id: $STUDENT" | jq .status   # "CONFIRMED"
curl -s $API/me/holds -H "X-Student-Id: $STUDENT" | jq '.[].status'
```
