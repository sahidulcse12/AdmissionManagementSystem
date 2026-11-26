# অগ্রগতি (Progress)

> English version: [PROGRESS.md](PROGRESS.md)

## বর্তমান অবস্থা

- **বর্তমান ধাপ:** প্রজেক্ট সেটআপ সম্পন্ন
- **বর্তমান ব্রাঞ্চ:** `feature/project-setup` → `develop`-এ মার্জ হচ্ছে
- **পরবর্তী কাজ:** `develop` থেকে `feature/database-and-class-management` তৈরি করা

## Feature ব্রাঞ্চসমূহ

| # | ব্রাঞ্চ | অবস্থা | Feature কমিট | `develop`-এ মার্জ |
|---|---|---|---|---|
| ১ | `feature/project-setup` | সম্পন্ন | `03a3853`, `1c104b5`, `0ebe29b`, `3585ae0` | পরবর্তী আপডেটে লেখা হবে |
| ২ | `feature/database-and-class-management` | বাকি | | |
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

## জানা সমস্যা

- এখনো নেই।
