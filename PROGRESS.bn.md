# অগ্রগতি (Progress)

> English version: [PROGRESS.md](PROGRESS.md)

## বর্তমান অবস্থা

- **বর্তমান ধাপ:** সব feature সম্পন্ন; `develop`-এ চূড়ান্ত যাচাই, তারপর `main`-এ মার্জ
- **বর্তমান ব্রাঞ্চ:** `feature/testing-and-documentation` → `develop`-এ মার্জ হচ্ছে
- **ব্যাকএন্ড আর্কিটেকচার:** Clean Architecture (`Domain` → `Application` → `Infrastructure` → `Api`)
- **বাকি feature:** নেই
- **পরবর্তী কাজ:** নেই — চূড়ান্ত মার্জ hash নিচে *চূড়ান্ত ইন্টিগ্রেশন* অংশে এবং `git log`-এ আছে

## Feature ব্রাঞ্চসমূহ

সব feature ব্রাঞ্চ সর্বশেষ `develop` থেকে তৈরি এবং `--no-ff` দিয়ে আবার মার্জ করা হয়েছে।

| # | ব্রাঞ্চ | অবস্থা | Feature কমিট | `develop`-এ মার্জ |
|---|---|---|---|---|
| ১ | `feature/project-setup` | সম্পন্ন | `03a3853`, `1c104b5`, `0ebe29b`, `3585ae0`, `b032521` | `b32c602` |
| ২ | `feature/database-and-class-management` | সম্পন্ন | `ba02a60`, `e3ff39c`, `f5f9134`, `d507856`, `97128ac` | `2af83aa` |
| ৩ | `feature/seat-availability-and-hold` | সম্পন্ন | `a748e5c`, `3937cbd`, `cac6b08`, `050f21e`, `cf7ba54`, `7ccc2ac` | `51af863` |
| ৪ | `feature/admission-confirmation-and-cancellation` | সম্পন্ন | `6878a9a`, `0e2240d`, `c454a1d`, `0502091` | `5251875` |
| — | `develop` (ইন্টিগ্রেশন সংশোধন) | সম্পন্ন | `40591d2` সময়ের নির্ভুলতা PostgreSQL অনুযায়ী করা | সরাসরি `develop`-এ |
| ৫ | `feature/frontend-admin` | সম্পন্ন | `258daee`, `ff3a7e5`, `56b3e9a`, অগ্রগতি কমিট | `804d9f5` |
| ৬ | `feature/frontend-parent` | সম্পন্ন | `3bbb3b4`, `39ba60e`, `f5c445c` | `106423d` |
| ৭ | `feature/testing-and-documentation` | সম্পন্ন | `01ca03b`, `41b3b77`, `269b7fd`, `8ed0691`, `5fda5bd`, এই অগ্রগতি কমিট | *চূড়ান্ত ইন্টিগ্রেশন* দেখুন |

`main`-এর ভিত্তি কমিট: `41838aa` (প্রাথমিক রিপোজিটরি সেটআপ)।

## সম্পন্ন feature

- শিফট ও ধারণক্ষমতাসহ ক্লাস ব্যবস্থাপনা (`POST/GET /classes`, `GET /classes/{id}`)
- কনফিগারযোগ্য সময়সীমাসহ first-come-first-served সিট hold (`POST /classes/{id}/holds`)
- Hold-এর মেয়াদ শেষ হওয়া (পড়ার সময় + background sweeper), hold দেখা ও ইতিহাস (`GET /holds/{id}`, `GET /me/holds`)
- মালিকানা যাচাইসহ idempotent নিশ্চিতকরণ ও বাতিল (`POST /holds/{id}/confirm`, `DELETE /holds/{id}`)
- অ্যাডমিনের জন্য একটি ক্লাসের hold দেখা (`GET /classes/{id}/holds`)
- লাইভ সিট, countdown, ইতিহাস এবং রিলোডের পরেও তথ্য সংরক্ষণসহ অ্যাডমিন UI ও অভিভাবক UI
- PostgreSQL volume, health check ও স্বয়ংক্রিয় migration-সহ Docker Compose স্ট্যাক
- ইংরেজি ও বাংলা ডকুমেন্টেশন

## চালানো টেস্ট

| কখন | কমান্ড | ফলাফল |
|---|---|---|
| প্রজেক্ট সেটআপ | `dotnet build` (ব্যাকএন্ড) | সফল, ০টি এরর |
| প্রজেক্ট সেটআপ | `npm run build` (ফ্রন্টএন্ড) | সফল |
| প্রজেক্ট সেটআপ | `docker compose config -q` | বৈধ |
| ক্লাস ব্যবস্থাপনা | `dotnet test` (Testcontainers PostgreSQL 16) | ১২টি সফল, ০টি ব্যর্থ |
| সিট hold | `dotnet test` | ৩১টি সফল, ০টি ব্যর্থ |
| সিট hold | `ConcurrencyTests` ৩ বার পুনরাবৃত্তি | প্রতিবার ৪/৪ সফল |
| সিট hold (মার্জের পর `develop`-এ) | `dotnet test` | ৩১টি সফল, ০টি ব্যর্থ |
| নিশ্চিতকরণ ও বাতিল | `dotnet test` | ৫৬টি সফল, ০টি ব্যর্থ |
| মার্জের পর `develop`-এ | `dotnet test` | ২টি ব্যর্থ (সময়ের নির্ভুলতার বাগ) → `40591d2`-এ সংশোধন, এরপর ৩ বার ৫৬টি সফল |
| অ্যাডমিন ইন্টারফেস | `npm test`, `npm run lint`, `npm run build` | ৬টি সফল, lint পরিষ্কার, build সফল |
| অভিভাবক ইন্টারফেস | `npm test`, `npm run lint`, `npm run build` | ১৯টি সফল, lint পরিষ্কার, build সফল |

## Docker দিয়ে end-to-end যাচাই (`docker compose up --build`)

| যাচাই | ফলাফল |
|---|---|
| `db`, `api` healthy; `web` চালু | ✅ |
| স্টার্টআপে migration প্রয়োগ (`InitialClasses`, `AddHolds`) | ✅ |
| HTTP দিয়ে ক্লাস তৈরি → hold → confirm | ✅ `201`, `HELD` (৬০০ সেকেন্ড), `CONFIRMED` |
| একই শিক্ষার্থীর ডুপ্লিকেট hold | ✅ `409` |
| অন্য শিক্ষার্থী কারো hold confirm করে | ✅ `403 HOLD_NOT_OWNED` |
| দুবার confirm | ✅ `200`, হুবহু একই `confirmedAt` |
| `X-Student-Id` নেই / ভুল আইডি | ✅ `400 MISSING_STUDENT_ID` / `400 INVALID_ID` |
| ৩টি সিটের জন্য একসাথে ৩০টি `curl` hold | ✅ ৩টি `201`, ২৭টি `409`; ডাটাবেসে ৩টি সক্রিয় সারি |
| `docker compose down` + `up` | ✅ ক্লাস ও নিশ্চিত ভর্তি রয়ে গেছে (volume সংরক্ষিত) |
| `HOLD_DURATION_SECONDS=5 EXPIRY_SWEEP_SECONDS=2` | ✅ hold `remainingSeconds: 5`; ৮ সেকেন্ড পর ডাটাবেসে `EXPIRED`; confirm → `410`; সিট আবার খালি |
| ফ্রন্টএন্ড চালু, `API_URL` থেকে `config.js` তৈরি, SPA fallback | ✅ |
| CORS preflight-এ `X-Student-Id` অনুমোদিত | ✅ |

## চূড়ান্ত ইন্টিগ্রেশন

শেষ feature মার্জের পরে লেখা হয়েছে — নিচে [`develop`-এ চূড়ান্ত ইন্টিগ্রেশন](#develop-এ-চূড়ান্ত-ইন্টিগ্রেশন) দেখুন।

## জানা সমস্যা

- এখন কোনো খোলা সমস্যা নেই। সমাধান হয়েছে: লেখার ঠিক পরে ফেরত দেওয়া সময় ১০০ ন্যানোসেকেন্ড নির্ভুলতায়
  ছিল, কিন্তু পরে পড়লে PostgreSQL-এর মাইক্রোসেকেন্ড নির্ভুলতায় আসত, ফলে বারবার confirm করলে ভিন্ন ফলাফল দেখাত (`40591d2`)।
- ইচ্ছাকৃত সীমাবদ্ধতাগুলো [README.bn.md](README.bn.md#জানা-সীমাবদ্ধতা) ও [ASSUMPTIONS.bn.md](ASSUMPTIONS.bn.md)-এ আছে।

## `develop`-এ চূড়ান্ত ইন্টিগ্রেশন

| ধাপ | ফলাফল |
|---|---|
| `feature/testing-and-documentation` → `develop` | মার্জ `ba959eb` |
| `develop`-এ ব্যাকএন্ড `dotnet build` / `dotnet test` | ০টি warning, ০টি এরর / ৫৬টি সফল, ০টি ব্যর্থ |
| `develop`-এ ফ্রন্টএন্ড `npm test` / `npm run lint` / `npm run build` | ১৯টি সফল / পরিষ্কার / সফল |
| `develop` থেকে `docker compose up -d --build` + smoke টেস্ট | সব সার্ভিস চালু, hold → confirm → `CONFIRMED`, পূর্ণ ক্লাস → `409`, UI `200` |
| `develop` → `main` | এই কমিটের পরে `--no-ff` দিয়ে মার্জ; hash: `git log main -1` |
