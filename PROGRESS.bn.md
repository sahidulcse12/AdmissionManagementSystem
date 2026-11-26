# অগ্রগতি (Progress)

> English version: [PROGRESS.md](PROGRESS.md)

## বর্তমান অবস্থা

- **বর্তমান ধাপ:** ডাটাবেস ও ক্লাস ব্যবস্থাপনা সম্পন্ন
- **বর্তমান ব্রাঞ্চ:** `feature/database-and-class-management` → `develop`-এ মার্জ হচ্ছে
- **পরবর্তী কাজ:** `develop` থেকে `feature/seat-availability-and-hold` তৈরি করা
- **ব্যাকএন্ড আর্কিটেকচার:** Clean Architecture (`Domain` → `Application` → `Infrastructure` → `Api`)

## Feature ব্রাঞ্চসমূহ

| # | ব্রাঞ্চ | অবস্থা | Feature কমিট | `develop`-এ মার্জ |
|---|---|---|---|---|
| ১ | `feature/project-setup` | সম্পন্ন | `03a3853`, `1c104b5`, `0ebe29b`, `3585ae0`, `b032521` | `b32c602` |
| ২ | `feature/database-and-class-management` | সম্পন্ন | `ba02a60`, `e3ff39c`, `f5f9134`, `d507856` | পরবর্তী আপডেটে লেখা হবে |
| ৩ | `feature/seat-availability-and-hold` | বাকি | | |
| ৪ | `feature/admission-confirmation-and-cancellation` | বাকি | | |
| ৫ | `feature/frontend-admin` | বাকি | | |
| ৬ | `feature/frontend-parent` | বাকি | | |
| ৭ | `feature/testing-and-documentation` | বাকি | | |

`main`-এর ভিত্তি কমিট: `41838aa` (প্রাথমিক রিপোজিটরি সেটআপ)।

## চালানো টেস্ট

| কখন | কমান্ড | ফলাফল |
|---|---|---|
| প্রজেক্ট সেটআপ | `dotnet build` (ব্যাকএন্ড) | সফল, ০টি এরর |
| প্রজেক্ট সেটআপ | `npm run build` (ফ্রন্টএন্ড) | সফল |
| প্রজেক্ট সেটআপ | `docker compose config -q` | বৈধ |
| ক্লাস ব্যবস্থাপনা | `dotnet test` (Testcontainers PostgreSQL 16) | ১২টি সফল, ০টি ব্যর্থ |

## জানা সমস্যা

- এখনো নেই।
