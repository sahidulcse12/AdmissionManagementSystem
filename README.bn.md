# স্কুল ভর্তি ব্যবস্থাপনা সিস্টেম

> English: [README.md](README.md)

"আগে এলে আগে পাবেন" (first-come-first-served) নিয়মে চলা একটি স্কুল ভর্তি সিস্টেম। অভিভাবকরা একটি ক্লাস ও
শিফটে সিট রিজার্ভ করেন, নির্দিষ্ট সময় (ডিফল্ট ১০ মিনিট) সিটটি ধরে রাখেন (hold), এবং ভর্তি নিশ্চিত করেন।
অ্যাডমিন ক্লাস তৈরি করেন, সিট সংখ্যা ঠিক করেন এবং সিট পূরণ হওয়া দেখেন। একসাথে অনেক অভিভাবক ক্লিক করলেও
সিস্টেম কখনো ক্লাসের ধারণক্ষমতার বেশি শিক্ষার্থী ভর্তি করে না।

## বৈশিষ্ট্য

- **অ্যাডমিন:** শিফটভিত্তিক ক্লাস ও সিট সংখ্যা তৈরি; ভর্তি / held / খালি সিটের লাইভ টেবিল; প্রতি ক্লাসের hold ও ভর্তির তালিকা।
- **অভিভাবক:** জন্ম নিবন্ধন নম্বর দিয়ে শিক্ষার্থী শনাক্তকরণ; খালি সিট দেখা; সিট রিজার্ভ; লাইভ countdown;
  পেমেন্ট ও নিশ্চিতকরণ (পেমেন্ট সিমুলেটেড) বা বাতিল; ভর্তির ইতিহাস।
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
| Health check | http://localhost:8080/health |

প্রথমবার build-এ base image ডাউনলোড হয় বলে কয়েক মিনিট লাগতে পারে। API চালু হওয়ার সময় ডাটাবেস migration
নিজে থেকেই চলে। ডাটা `pgdata` Docker volume-এ থাকে এবং `docker compose down`-এর পরেও থেকে যায়; মুছতে চাইলে
`docker compose down -v` ব্যবহার করুন।

### ব্যবহার করে দেখুন

১. **Admin** ট্যাবে একটি ক্লাস তৈরি করুন, যেমন `Class 1`, `MORNING`, সিট `2`।
২. **Parent / Guardian** ট্যাবে ১৭ অঙ্কের একটি নম্বর দিন, যেমন `20150123456789012`।
৩. **Reserve seat** চাপুন — ১০ মিনিটের countdown দেখা যাবে।
৪. **Pay & confirm admission** চাপুন — ইতিহাসে ও অ্যাডমিন টেবিলে সিটটি *Admitted* হবে।
৫. **Change student** দিয়ে অন্য নম্বর ব্যবহার করে দেখুন সিট শেষ হলে ক্লাসটি **Full** দেখায়।

## কনফিগারেশন

সব সেটিং environment variable দিয়ে হয় ([.env.example](.env.example) দেখুন)।

| Variable | ডিফল্ট | ব্যবহারকারী | অর্থ |
|---|---|---|---|
| `HOLD_DURATION_SECONDS` | `600` | api | একটি hold কতক্ষণ সিট ধরে রাখে। |
| `EXPIRY_SWEEP_SECONDS` | `15` | api | কত সেকেন্ড পরপর মেয়াদোত্তীর্ণ hold ডাটাবেসে `EXPIRED` করা হয়। |
| `CORS_ORIGINS` | `*` | api | অনুমোদিত ব্রাউজার origin (কমা দিয়ে আলাদা)। |
| `API_PORT` | `8080` | api | API-এর host পোর্ট। |
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
| `POST` | `/classes` | ক্লাস তৈরি |
| `GET` | `/classes` | বর্তমান সিট সংখ্যাসহ ক্লাসের তালিকা |
| `GET` | `/classes/{classId}` | একটি ক্লাস |
| `POST` | `/classes/{classId}/holds` | সিট রিজার্ভ (`X-Student-Id` প্রয়োজন) |
| `GET` | `/holds/{holdId}` | hold দেখা (শুধু মালিক) |
| `POST` | `/holds/{holdId}/confirm` | ভর্তি নিশ্চিত (শুধু মালিক, idempotent) |
| `DELETE` | `/holds/{holdId}` | hold বাতিল (শুধু মালিক) |
| `GET` | `/me/holds` | শিক্ষার্থীর hold ও ভর্তির ইতিহাস |
| `GET` | `/classes/{classId}/holds` | অ্যাডমিন: একটি ক্লাসের hold ও ভর্তি |

Request/response ফরম্যাট ও সব error code: [API.bn.md](API.bn.md)।

## টেস্ট চালানো

ব্যাকএন্ড টেস্ট একটি অস্থায়ী PostgreSQL কন্টেইনার চালায়, তাই Docker চালু থাকতে হবে।

```bash
cd backend
dotnet test                       # ৫৬টি টেস্ট: domain নিয়ম, API, মেয়াদোত্তীর্ণ, race
```

```bash
cd frontend
npm ci
npm test                          # ১৯টি টেস্ট: অ্যাডমিন ও অভিভাবকের কাজের ধারা
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
- UI-এর লেখা শুধু ইংরেজিতে।

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
