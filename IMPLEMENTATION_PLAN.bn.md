# বাস্তবায়ন পরিকল্পনা (Implementation Plan)

> English version: [IMPLEMENTATION_PLAN.md](IMPLEMENTATION_PLAN.md)

## লক্ষ্য

একটি স্কুল ভর্তি ব্যবস্থাপনা সিস্টেম, যেখানে অভিভাবকরা "আগে এলে আগে পাবেন" (first-come-first-served)
নিয়মে সিট রিজার্ভ করবেন, নির্দিষ্ট সময়ের জন্য সিট ধরে রাখবেন (hold) এবং ভর্তি নিশ্চিত করবেন।
অ্যাডমিন ক্লাস, শিফট ও সিট সংখ্যা নির্ধারণ করবেন। একসাথে অনেক অনুরোধ এলেও সিস্টেম কখনোই
ক্লাসের ধারণক্ষমতার বেশি সিট বরাদ্দ করবে না।

## প্রযুক্তি

| স্তর | পছন্দ |
|---|---|
| ব্যাকএন্ড | C# / .NET 9, ASP.NET Core Minimal API, EF Core 9 + Npgsql |
| ডাটাবেস | PostgreSQL 16 |
| ফ্রন্টএন্ড | React 19 + TypeScript + Vite, Vitest + Testing Library |
| ইনফ্রাস্ট্রাকচার | Docker Compose (db, api, web), নামযুক্ত ভলিউম `pgdata` |
| টেস্ট | xUnit + WebApplicationFactory + Testcontainers (আসল PostgreSQL) |

## Git কর্মপ্রবাহ

`main → develop → feature/* → develop → main`। প্রতিটি feature ব্রাঞ্চ সর্বশেষ `develop` থেকে তৈরি হয়,
ছোট ছোট কাজভিত্তিক কমিট করা হয়, `--no-ff` দিয়ে `develop`-এ মার্জ করা হয়, এবং সম্পূর্ণ যাচাইয়ের পরেই
কেবল `develop` থেকে `main`-এ মার্জ করা হয়। কোনো রিমোটে push করা হয় না।

## Feature ব্রাঞ্চ ও কাজের পরিধি

| # | ব্রাঞ্চ | কাজ |
|---|---|---|
| ১ | `feature/project-setup` | সলিউশন ও ফ্রন্টএন্ড কাঠামো, Docker Compose, `.env.example`, পরিকল্পনা/অগ্রগতি ডকুমেন্ট |
| ২ | `feature/database-and-class-management` | EF Core মডেল, মাইগ্রেশন, স্টার্টআপে মাইগ্রেশন, এরর ফরম্যাট, `POST/GET /classes`, `GET /classes/{id}`, টেস্ট |
| ৩ | `feature/seat-availability-and-hold` | সিট গণনা, `POST /classes/{id}/holds`, `GET /holds/{id}`, `GET /me/holds`, row lock, মেয়াদোত্তীর্ণ hold পরিষ্কারক, concurrency টেস্ট |
| ৪ | `feature/admission-confirmation-and-cancellation` | `POST /holds/{id}/confirm` (idempotent), `DELETE /holds/{id}`, মালিকানা যাচাই, টেস্ট |
| ৫ | `feature/frontend-admin` | API ক্লায়েন্ট, অ্যাডমিন পেজ: ক্লাস তৈরি, সিটের অবস্থা ও ভর্তি তথ্য দেখা |
| ৬ | `feature/frontend-parent` | অভিভাবক পেজ: শিক্ষার্থী আইডি, রিজার্ভ, কাউন্টডাউন, নিশ্চিতকরণ, বাতিল, ইতিহাস, রিফ্রেশের পরেও তথ্য সংরক্ষণ, টেস্ট |
| ৭ | `feature/testing-and-documentation` | Docker দিয়ে end-to-end যাচাই, ইংরেজি ও বাংলায় সম্পূর্ণ ডকুমেন্টেশন |

## অগ্রাধিকার

- **P0:** Docker দিয়ে চালু হওয়া, ডেটা সংরক্ষণ, ক্লাস ব্যবস্থাপনা, ধারণক্ষমতা মানা, hold, মেয়াদোত্তীর্ণ হওয়া,
  ভর্তি নিশ্চিতকরণ, ডুপ্লিকেট প্রতিরোধ, মালিকানা যাচাই, একসাথে অনুরোধেও অতিরিক্ত বুকিং না হওয়া।
- **P1:** অ্যাডমিন ও অভিভাবক UI, ইতিহাস, বাতিলকরণ, স্বয়ংক্রিয় টেস্ট, দ্বিভাষিক ডকুমেন্ট।
- **P2:** সৌন্দর্যবর্ধন — শুধুমাত্র P0/P1 শেষ হওয়ার পরে।

## মূল ডিজাইন সিদ্ধান্ত

১. **ক্লাসভিত্তিক row lock** (ক্লাসের সারিতে `SELECT … FOR UPDATE`) একটি ক্লাসের সব hold তৈরির অনুরোধকে
   একটির পর একটি চালায়, ফলে সিট পরীক্ষা ও hold তৈরি একসাথে (atomic) হয়।
২. `(class_id, student_id) WHERE status IN ('HELD','CONFIRMED')`-এর উপর **আংশিক unique index**
   ডাটাবেস স্তরে একই শিক্ষার্থীর ডুপ্লিকেট সক্রিয় ভর্তি আটকায়।
৩. নিশ্চিতকরণ/বাতিলের সময় **hold সারিতে lock** থাকায় এই পরিবর্তনগুলো race-মুক্ত ও idempotent।
৪. **মেয়াদ পড়া ও লেখা দুই সময়েই যাচাই হয়** (`expires_at <= now` হলে মেয়াদোত্তীর্ণ ধরা হয়), আর একটি
   background কাজ `EXPIRED` অবস্থা ডাটাবেসে লিখে দেয়, ফলে কোনো অনুরোধ না এলেও সিট খালি হয়ে যায়।
৫. **Injectable `TimeProvider`** ব্যবহারের ফলে টেস্টে অপেক্ষা না করে সময় এগিয়ে নেওয়া যায়।
