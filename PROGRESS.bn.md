# অগ্রগতি (Progress)

> English version: [PROGRESS.md](PROGRESS.md)

## বর্তমান অবস্থা

- **ধাপ:** সম্পন্ন — brief-এর সব প্রয়োজনীয়তা বাস্তবায়িত ও যাচাই করা; `develop` থেকে `main`-এ মার্জ হয়েছে
- **ব্যাকএন্ড আর্কিটেকচার:** Clean Architecture (`Domain` → `Application` → `Infrastructure` → `Api`)
- **বাকি feature:** নেই
- **কমিট hash:** `git log --oneline --graph --all` দেখুন (history কখনো পুনর্লিখিত হলেও এই ফাইল যেন সঠিক থাকে,
  তাই hash এখানে লেখা হয়নি)

## Git কর্মপ্রবাহ

`main → develop → feature/* → develop → main`। প্রতিটি feature ব্রাঞ্চ সর্বশেষ `develop` থেকে তৈরি, ছোট ছোট
কাজভিত্তিক কমিটে গড়া, এবং `--no-ff` দিয়ে মার্জ করা।

| # | ব্রাঞ্চ | যা দিয়েছে |
|---|---|---|
| ১ | `feature/project-setup` | .NET ও React কাঠামো, Docker Compose, `.env.example`, পরিকল্পনা |
| ২ | `feature/database-and-class-management` | Clean Architecture স্তর, EF Core + migration, class API, টেস্ট |
| ৩ | `feature/seat-availability-and-hold` | Row-lock দিয়ে সিট hold, মেয়াদ sweeper, hold query, concurrency টেস্ট |
| ৪ | `feature/admission-confirmation-and-cancellation` | Idempotent confirm, cancel, মালিকানা যাচাই, race টেস্ট |
| — | `develop` (ইন্টিগ্রেশন সংশোধন) | মার্জের পর পুনরায় টেস্টে ধরা পড়া সময়ের নির্ভুলতার সংশোধন |
| ৫ | `feature/frontend-admin` | API ক্লায়েন্ট, অ্যাডমিন স্ক্রিন, টেস্ট |
| ৬ | `feature/frontend-parent` | অভিভাবক স্ক্রিন, countdown, ইতিহাস, তথ্য সংরক্ষণ, টেস্ট |
| ৭ | `feature/testing-and-documentation` | Docker end-to-end যাচাই, ইংরেজি ও বাংলা ডকুমেন্ট |
| ৮ | `feature/api-contract-alignment` | হুবহু API contract (`totalSeats`, `holdId`, `204`), প্রতিটি shift আলাদা class, উপরে শিক্ষার্থী আইডির ঘর, দুই-backend bonus, `ContractTests`, `MultiInstanceTests` |
| — | `fix/sweeper-graceful-shutdown` | বন্ধের সময় expiry sweeper শান্তভাবে থামে; Docker-এর পাশাপাশি local-এ চালানোর নিয়ম README-তে |
| — | `chore/remove-doc-comments` | XML doc / JSDoc কমেন্ট মোছা |
| ৯ | `feature/swagger-and-api-versioning` | `/swagger`-এ Swagger UI, header/query দিয়ে API versioning (ডিফল্ট 1.0), টেস্ট |
| ১০ | `feature/controllers` | HTTP স্তর ASP.NET Core controller-এ (`ClassesController`, `ClassHoldsController`, `HoldsController`, `MeController`); contract অপরিবর্তিত |

## প্রয়োজনীয়তার তালিকা (brief §১.৩–§১.৭)

| প্রয়োজনীয়তা | অবস্থা | প্রমাণ |
|---|---|---|
| প্রতিটি shift নিজস্ব সিটসহ আলাদা class | ✅ | `POST /classes { name, totalSeats }` |
| Page-এর উপরে শিক্ষার্থী আইডির ঘর; login নেই | ✅ | `StudentIdBar`, `App.test.tsx` |
| স্ক্রিন: class তৈরি, class তালিকা, আবেদন, আমার আবেদন | ✅ | অ্যাডমিন ও অভিভাবক পেজ |
| API contract: ৮টি endpoint, field-এর নাম, 201/200/204/409 | ✅ | `ContractTests`, Docker-এ `curl` |
| যেকোনো concurrency-তে `held + confirmed ≤ totalSeats` | ✅ | `ConcurrencyTests`, ৩০ ও ৬০টি সমান্তরাল `curl` |
| `HOLD_DURATION_SECONDS` দিয়ে মেয়াদ (ডিফল্ট ৬০০); নিজে থেকে সিট ফেরত | ✅ | মেয়াদের টেস্ট; Docker-এ ৫ সেকেন্ড দিয়ে চালানো |
| প্রতি class-এ একজন শিক্ষার্থীর একটিই সক্রিয় আবেদন/ভর্তি | ✅ | আংশিক unique index + টেস্ট |
| শুধু নিজের, মেয়াদ না পেরোনো `HELD` confirm করা যায় | ✅ | `403` / `410` / `409` টেস্ট |
| বারবার confirm করলে `confirmedSeats` একবারই বাড়ে | ✅ | `Parallel_confirmations_of_one_hold_produce_one_admission` |
| Restart ও `docker compose down` / `up`-এর পরেও ডেটা থাকে | ✅ | হাতে-কলমে যাচাই (volume `pgdata`) |
| ভুল input → 4xx, কখনো 500 নয় | ✅ | Validation ও ভুল-body টেস্ট |
| সব কাজ UI থেকে করা যায় | ✅ | অ্যাডমিন + অভিভাবক পেজ |
| মেয়াদ শেষে reload ছাড়াই EXPIRED ও "ভর্তি ফি দিন" বন্ধ | ✅ | `ParentPage.test.tsx` |
| সিট সংখ্যা কয়েক সেকেন্ড পরপর হালনাগাদ | ✅ | ৫ সেকেন্ডে polling |
| রিফ্রেশ / ব্রাউজার রিস্টার্টের পরেও চলমান আবেদন দেখা যায় | ✅ | `App.test.tsx` |
| বোঝার মতো error (সিট শেষ, মেয়াদ শেষ, ভুল input, server বন্ধ) | ✅ | ফ্রন্টএন্ড টেস্ট |
| Bonus: একই ডাটাবেসে দুটি backend process | ✅ | `MultiInstanceTests`, `docker compose --profile multi` |
| Bonus: ফ্রন্টএন্ডের automated test | ✅ | ২১টি Vitest টেস্ট |
| বাড়তি: Swagger UI ও API versioning | ✅ | `/swagger`, `ApiVersioningAndSwaggerTests` |
| README, ASSUMPTIONS, AI_USAGE, Dockerfile, compose | ✅ | রিপোজিটরির root |
| কমপক্ষে ৩টি ব্যাকএন্ড টেস্ট, একটি concurrency টেস্টসহ | ✅ | ৬৮টি টেস্ট |

## চালানো টেস্ট (সর্বশেষ)

| কমান্ড | ফলাফল |
|---|---|
| `dotnet test` (Testcontainers PostgreSQL 16) | ৬৮টি সফল, ০টি ব্যর্থ |
| Concurrency + multi-instance টেস্ট ৩ বার | প্রতিবার ১০/১০ সফল |
| `npm test` | ২১টি সফল |
| `npm run lint` / `npm run build` | পরিষ্কার / সফল |

আগের ধাপগুলোতে feature যোগ হওয়ার সাথে ব্যাকএন্ড টেস্ট ১২ → ৩১ → ৫৬ হয়েছে। Feature ৪ মার্জের পর `develop`-এ
২টি টেস্ট ব্যর্থ হয় (সময়ের নির্ভুলতার বাগ); সংশোধনের পর টানা তিনবার ৫৬টি সফল।

## Docker দিয়ে end-to-end যাচাই

| যাচাই | ফলাফল |
|---|---|
| `docker compose --profile multi up --build`: `db`, `api` healthy; `api2`, `web` চালু | ✅ |
| পুরনো ডেটাসহ ডাটাবেসে migration shift-কে class-এর নামে জুড়ে দেয়; নিশ্চিত ভর্তি থেকে যায় | ✅ |
| `curl` দিয়ে contract-এর ৮টি endpoint | ✅ `201`, `200`, `200`, `201`, `200` (header ছাড়া), `200`, `204` (খালি body), `200` |
| ভুল input (`{bad`, `totalSeats:"a"`, খালি নাম, সিট নেই) | ✅ সব `400` |
| ৩টি সিটের জন্য একসাথে ৩০টি `curl` hold (একটি API) | ✅ ৩টি `201`, ২৭টি `409` |
| ৫টি সিটের জন্য 8080 ও 8081 পোর্টে ভাগ করে একসাথে ৬০টি `curl` hold | ✅ ৫টি `201`, ৫৫টি `409`; দুই API-ই ৫টি held দেখায় |
| `docker compose down` + `up` | ✅ ডেটা রয়ে গেছে |
| `HOLD_DURATION_SECONDS=5 EXPIRY_SWEEP_SECONDS=2` | ✅ ৮ সেকেন্ড পর ডাটাবেসে `EXPIRED`; confirm → `410`; সিট আবার খালি |

## জানা সমস্যা

- কোনো খোলা সমস্যা নেই। ইচ্ছাকৃত সীমাবদ্ধতা [README.bn.md](README.bn.md#জানা-সীমাবদ্ধতা) ও
  [ASSUMPTIONS.bn.md](ASSUMPTIONS.bn.md)-এ আছে।
