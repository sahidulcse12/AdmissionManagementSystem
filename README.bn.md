# স্কুল ভর্তি ব্যবস্থাপনা সিস্টেম

> English: [README.md](README.md)

"আগে আবেদন করলে আগে পাবে" (first-come-first-served) নিয়মে চলা একটি স্কুল ভর্তি সিস্টেম। অভিভাবক কোনো class-এ
আবেদন করলে সিটটি নির্দিষ্ট সময় (ডিফল্ট ১০ মিনিট) শিক্ষার্থীর নামে hold থাকে, এবং ভর্তি ফি দিলে ভর্তি নিশ্চিত হয়।
অ্যাডমিন class তৈরি করেন ও সিট পূরণ হওয়া দেখেন। একসাথে অনেক অভিভাবক আবেদন করলেও — এমনকি একই ডাটাবেসে দুটি
backend process চললেও — সিস্টেম কখনো `totalSeats`-এর বেশি শিক্ষার্থী ভর্তি করে না।

প্রতিটি shift একটি আলাদা class, যেমন "Class 6 - Morning Shift" ও "Class 6 - Day Shift"।

## বৈশিষ্ট্য

- **Page-এর উপরে শিক্ষার্থীর জন্ম নিবন্ধন নম্বরের ঘর** (১৭ অঙ্ক); কোনো login নেই।
- **অ্যাডমিন:** নাম ও মোট সিট দিয়ে class তৈরি; মোট / hold / নিশ্চিত / খালি সিটের লাইভ টেবিল; প্রতি class-এর
  আবেদন ও ভর্তির তালিকা।
- **অভিভাবক:** সিটের হিসাবসহ class তালিকা ও **"আবেদন করুন"** বোতাম; আবেদনে অবস্থা, বাকি সময়, **"ভর্তি ফি দিন"**
  ও **"বাতিল"** বোতাম; **"আমার আবেদন"**-এ সব আবেদন ও প্রতিটির অবস্থা।
- **কঠোর first-come-first-served**: কোনো লটারি নেই; অনুরোধ ডাটাবেসে যে ক্রমে পৌঁছায় সেটাই নির্ধারক।
- **সিট hold**, সময়সীমা কনফিগারযোগ্য; মেয়াদ শেষ হলে সিট নিজে থেকেই খালি হয়।
- **Concurrency-নিরাপদ**: row-level lock, transaction ও আংশিক unique index — আসল PostgreSQL-এ race টেস্ট দিয়ে যাচাই করা।
- **Idempotent নিশ্চিতকরণ** এবং প্রতিটি hold অপারেশনে **backend-এ মালিকানা যাচাই**।

## প্রযুক্তি

| স্তর | প্রযুক্তি |
|---|---|
| ব্যাকএন্ড | C# · .NET 9 · ASP.NET Core Minimal API · EF Core 9 · Clean Architecture |
| ডাটাবেস | PostgreSQL 16 |
| ফ্রন্টএন্ড | React 19 · TypeScript · Vite |
| টেস্ট | xUnit · Testcontainers (আসল PostgreSQL) · Vitest · Testing Library |
| রানটাইম | Docker Compose (db, api, web) |

## দ্রুত শুরু

প্রয়োজন: শুধু **Docker Desktop** (অথবা Compose v2 সহ Docker Engine)। আর কিছু লাগবে না।

```bash
cp .env.example .env        # ঐচ্ছিক — প্রতিটি মানের ডিফল্ট আছে
docker compose up --build
```

| সার্ভিস | URL |
|---|---|
| অভিভাবক UI | http://localhost:5173 |
| অ্যাডমিন UI | http://localhost:5173/#/admin |
| ব্যাকএন্ড API | http://localhost:8080 |
| **Swagger UI** (ব্রাউজারে API চালিয়ে দেখুন) | http://localhost:8080/swagger |
| Health check | http://localhost:8080/health |

প্রথমবার build-এ base image ডাউনলোড হয় বলে কয়েক মিনিট লাগতে পারে। API চালু হওয়ার সময় ডাটাবেস migration
নিজে থেকেই চলে। ডাটা `pgdata` Docker volume-এ থাকে এবং `docker compose down`-এর পরেও থেকে যায়; মুছতে চাইলে
`docker compose down -v` ব্যবহার করুন।

### ব্যবহার করে দেখুন

১. **Admin** ট্যাবে একটি class তৈরি করুন, যেমন নাম `Class 6 - Morning Shift`, মোট সিট `2`।
২. Page-এর উপরের ঘরে ১৭ অঙ্কের একটি নম্বর দিন, যেমন `20150123456789012`।
৩. **Parent / Guardian** ট্যাবে **আবেদন করুন (Apply)** চাপুন — আবেদনে ১০ মিনিটের countdown দেখা যাবে।
৪. **ভর্তি ফি দিন (Pay admission fee)** চাপুন — "আমার আবেদন"-এ ও অ্যাডমিন টেবিলে অবস্থা *Admitted* হবে।
৫. উপরে **Change** চেপে অন্য নম্বর দিন, দেখুন সিট শেষ হলে class-টি **Full** দেখায়।

দ্রুত মেয়াদ শেষ হওয়া দেখতে: `HOLD_DURATION_SECONDS=30 docker compose up` — ৩০ সেকেন্ড পর page reload ছাড়াই
আবেদন **EXPIRED** দেখাবে এবং ফি দেওয়ার বোতাম বন্ধ হবে।

### Bonus: একই ডাটাবেসে দুটি backend process

```bash
docker compose --profile multi up --build
```

এতে একই PostgreSQL ব্যবহার করে http://localhost:8081-এ `api2`-ও চালু হয়। দুটি API একসাথে ব্যবহার করলেও
`held + confirmed` কখনো `totalSeats`-এর বেশি হয় না। কীভাবে নিশ্চিত করা হয়েছে তা
[ASSUMPTIONS.bn.md](ASSUMPTIONS.bn.md)-এ লেখা আছে।

### Backend বা frontend local-এ চালানো (development)

Docker-এর `api` container আগে থেকেই 8080 পোর্টে চলে, তাই Visual Studio / `dotnet run` থেকে একই পোর্টে backend
চালালে *"address already in use"* error আসে। শুধু ডাটাবেস Docker-এ রাখুন:

```bash
docker compose stop api api2 web     # 8080/8081/5173 পোর্ট খালি করে, PostgreSQL চালু থাকে
cd backend && dotnet run --project src/Admission.Api        # http://localhost:8080/swagger (Visual Studio-তে F5 দিলে খুলে যায়)
cd frontend && npm run dev                                  # http://localhost:5173
```

Local backend ডিফল্টভাবে `Host=localhost;Port=5432;Database=admission;Username=admission;Password=admission`
(Docker-এর ডাটাবেস)-এ যুক্ত হয়। প্রয়োজনে `DATABASE_URL` environment variable দিয়ে বদলানো যায়।

## কনফিগারেশন

সব সেটিং environment variable দিয়ে হয় ([.env.example](.env.example) দেখুন)।

| Variable | ডিফল্ট | ব্যবহারকারী | অর্থ |
|---|---|---|---|
| `HOLD_DURATION_SECONDS` | `600` | api | একটি hold কতক্ষণ সিট ধরে রাখে। |
| `EXPIRY_SWEEP_SECONDS` | `15` | api | কত সেকেন্ড পরপর মেয়াদোত্তীর্ণ hold ডাটাবেসে `EXPIRED` করা হয়। |
| `CORS_ORIGINS` | `*` | api | অনুমোদিত ব্রাউজার origin (কমা দিয়ে আলাদা)। |
| `API_PORT` | `8080` | api | API-এর host পোর্ট। |
| `API2_PORT` | `8081` | api2 | ঐচ্ছিক দ্বিতীয় API-এর host পোর্ট (`--profile multi`)। |
| `POSTGRES_DB` / `POSTGRES_USER` / `POSTGRES_PASSWORD` | `admission` | db, api | ডাটাবেসের তথ্য; API-এর connection string এগুলো থেকে তৈরি হয়। |
| `POSTGRES_PORT` | `5432` | db | PostgreSQL-এর host পোর্ট। |
| `API_URL` | `http://localhost:8080` | web | ব্রাউজার যে API ঠিকানা ব্যবহার করে; কন্টেইনার চালুর সময় `config.js`-এ লেখা হয়। |
| `FRONTEND_PORT` | `5173` | web | UI-এর host পোর্ট। |

API তার connection string `DATABASE_URL` (Npgsql ফরম্যাট) থেকে পড়ে; Compose এটি নিজেই সেট করে দেয়।

## সিট রিজার্ভেশন কীভাবে সঠিক থাকে

১. `POST /classes/{id}/holds` একটি transaction খুলে `SELECT … FOR UPDATE` দিয়ে ক্লাসের সারি lock করে।
   ওই ক্লাসের একসাথে আসা অনুরোধগুলো অপেক্ষা করে এবং আগমনের ক্রমে একটির পর একটি চলে।
২. Lock-এর ভেতরে দখলকৃত সিট (`CONFIRMED` + মেয়াদ না পেরোনো `HELD`) গণনা করে, এবং সিট খালি থাকলেই কেবল hold তৈরি হয়।
৩. `(class_id, student_id) WHERE status IN ('HELD','CONFIRMED')`-এর উপর আংশিক unique index ডাটাবেস স্তরেই
   ডুপ্লিকেট সক্রিয় hold অসম্ভব করে।
৪. Confirm ও cancel hold-এর সারি lock করে, মালিকানা যাচাই করে, এবং lock পাওয়ার *পরে* মেয়াদ যাচাই করে।
   দুবার confirm করলে একই ফলাফল আসে।
৫. সিট সংখ্যা সবসময় ডাটাবেস থেকে গণনা হয়; একটি background কাজ মেয়াদোত্তীর্ণ hold-কে `EXPIRED` করে।

বিস্তারিত, ডায়াগ্রাম ও সব পরিস্থিতির টেবিল: [ARCHITECTURE.bn.md](ARCHITECTURE.bn.md)।

## API

| Method | Path | উদ্দেশ্য |
|---|---|---|
| `POST` | `/classes` | নতুন class তৈরি — body `{ "name", "totalSeats" }` → `201` |
| `GET` | `/classes` | `totalSeats`, `heldSeats`, `confirmedSeats`, `availableSeats` সহ সব class |
| `GET` | `/classes/{classId}` | একটি class-এর seat-এর অবস্থা |
| `POST` | `/classes/{classId}/holds` | আবেদন / seat hold (`X-Student-Id`) → `201`, seat না থাকলে `409` |
| `GET` | `/holds/{holdId}` | আবেদনের বর্তমান অবস্থা (`holdId`, `status`, `expiresAt`, …) |
| `POST` | `/holds/{holdId}/confirm` | ভর্তি ফি জমা, ভর্তি নিশ্চিত (শুধু মালিক, idempotent) → `200` |
| `DELETE` | `/holds/{holdId}` | আবেদন বাতিল (শুধু মালিক) → `204` |
| `GET` | `/me/holds` | এই শিক্ষার্থীর সব আবেদন |
| `GET` | `/classes/{classId}/holds` | বাড়তি: অ্যাডমিনের জন্য একটি class-এর আবেদনের তালিকা |

Request/response ফরম্যাট ও সব error code: [API.bn.md](API.bn.md)। ইন্টারঅ্যাকটিভ ডকুমেন্ট: **http://localhost:8080/swagger**।

**API versioning:** contract অনুযায়ী path-এ version নেই। Version বেছে নিতে `X-Api-Version: 1.0` header বা `?api-version=1.0` দিন; না দিলে `1.0` ধরা হয়। Response-এ `api-supported-versions` থাকে; অসমর্থিত version দিলে `400`। বিস্তারিত [API.bn.md](API.bn.md)-এ।

## টেস্ট চালানো

ব্যাকএন্ড টেস্ট একটি অস্থায়ী PostgreSQL কন্টেইনার চালায়, তাই Docker চালু থাকতে হবে।

```bash
cd backend
dotnet test                       # ৬৮টি টেস্ট: contract, domain নিয়ম, মেয়াদ, race, দুই instance, versioning, Swagger
```

Concurrency টেস্টগুলো হলো `ConcurrencyTests` (যেমন ৫টি সিটের জন্য ৫০ জন একসাথে → ঠিক ৫টি hold),
`AdmissionConcurrencyTests` এবং `MultiInstanceTests` (একই ডাটাবেসে দুটি backend host)।
ব্যাকএন্ড টেস্টের জন্য .NET 9 SDK এবং চালু Docker লাগবে।

```bash
cd frontend
npm ci
npm test                          # ২১টি টেস্ট: অ্যাডমিন, অভিভাবক, শিক্ষার্থী আইডির ঘর, মেয়াদ, error
npm run lint && npm run build
```

স্থানীয়ভাবে .NET SDK বা Node না থাকলে কন্টেইনারে টেস্ট চালানো যায়:

```bash
docker run --rm -v "$PWD/frontend:/app" -w /app node:22-alpine sh -c "npm ci && npm test"
```

## প্রজেক্টের কাঠামো

```
.
├── backend/
│   ├── src/Admission.Domain/          # entity, অবস্থা পরিবর্তনের নিয়ম, ব্যবসায়িক নিয়ম
│   ├── src/Admission.Application/     # use case, DTO, repository interface
│   ├── src/Admission.Infrastructure/  # EF Core, PostgreSQL, migration, expiry কাজ
│   ├── src/Admission.Api/             # endpoint, error middleware, স্টার্টআপ
│   └── tests/Admission.Tests/         # unit + integration + concurrency টেস্ট
├── frontend/                          # React + TypeScript (Vite), nginx image
├── docker-compose.yml
└── .env.example
```

## Git কর্মপ্রবাহ

```
main ── develop ──┬── feature/project-setup
                  ├── feature/database-and-class-management
                  ├── feature/seat-availability-and-hold
                  ├── feature/admission-confirmation-and-cancellation
                  ├── feature/frontend-admin
                  ├── feature/frontend-parent
                  └── feature/testing-and-documentation
```

প্রতিটি feature ব্রাঞ্চ সর্বশেষ `develop` থেকে তৈরি, ছোট ছোট কাজভিত্তিক কমিটে গড়া, এবং `--no-ff` দিয়ে আবার
মার্জ করা হয়েছে। সম্পূর্ণ যাচাইয়ের পর `develop`-কে `main`-এ মার্জ করা হয়েছে।
ইতিহাস ও কমিট hash: [PROGRESS.bn.md](PROGRESS.bn.md)।

## জানা সীমাবদ্ধতা

- কোনো authentication নেই: `X-Student-Id` যা বলে সেটাই শিক্ষার্থী, এবং অ্যাডমিন endpoint উন্মুক্ত।
- পেমেন্ট সিমুলেটেড; নিশ্চিতকরণ সবসময় সফল।
- ক্লাস সম্পাদনা বা মুছে ফেলা যায় না; নিশ্চিত ভর্তি API দিয়ে প্রত্যাহার করা যায় না।
- UI push update-এর বদলে প্রতি ৫ সেকেন্ডে polling করে রিফ্রেশ হয়।
- UI-এর লেখা ইংরেজিতে, মূল বোতাম ও শিরোনামে brief-এর বাংলা শব্দসহ।

## ডকুমেন্টেশন

| বিষয় | English | বাংলা |
|---|---|---|
| সংক্ষিপ্ত বিবরণ (এই ফাইল) | [README.md](README.md) | [README.bn.md](README.bn.md) |
| আর্কিটেকচার, ডাটাবেস, concurrency | [ARCHITECTURE.md](ARCHITECTURE.md) | [ARCHITECTURE.bn.md](ARCHITECTURE.bn.md) |
| API রেফারেন্স | [API.md](API.md) | [API.bn.md](API.bn.md) |
| বাস্তবায়ন পরিকল্পনা | [IMPLEMENTATION_PLAN.md](IMPLEMENTATION_PLAN.md) | [IMPLEMENTATION_PLAN.bn.md](IMPLEMENTATION_PLAN.bn.md) |
| অগ্রগতি, কমিট, টেস্টের ফলাফল | [PROGRESS.md](PROGRESS.md) | [PROGRESS.bn.md](PROGRESS.bn.md) |
| অনুমানসমূহ | [ASSUMPTIONS.md](ASSUMPTIONS.md) | [ASSUMPTIONS.bn.md](ASSUMPTIONS.bn.md) |
| AI ব্যবহার | [AI_USAGE.md](AI_USAGE.md) | [AI_USAGE.bn.md](AI_USAGE.bn.md) |
