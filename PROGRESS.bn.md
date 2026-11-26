# অগ্রগতি (Progress)

> English version: [PROGRESS.md](PROGRESS.md)

## বর্তমান অবস্থা

- **বর্তমান ধাপ:** ফ্রন্টএন্ডের মূল কাজ সম্পন্ন (অভিভাবক ইন্টারফেস শেষ)
- **বর্তমান ব্রাঞ্চ:** `feature/frontend-parent` → `develop`-এ মার্জ হচ্ছে
- **পরবর্তী কাজ:** `develop` থেকে `feature/testing-and-documentation` তৈরি করা; Docker দিয়ে end-to-end যাচাই
- **ব্যাকএন্ড আর্কিটেকচার:** Clean Architecture (`Domain` → `Application` → `Infrastructure` → `Api`)

## Feature ব্রাঞ্চসমূহ

| # | ব্রাঞ্চ | অবস্থা | Feature কমিট | `develop`-এ মার্জ |
|---|---|---|---|---|
| ১ | `feature/project-setup` | সম্পন্ন | `03a3853`, `1c104b5`, `0ebe29b`, `3585ae0`, `b032521` | `b32c602` |
| ২ | `feature/database-and-class-management` | সম্পন্ন | `ba02a60`, `e3ff39c`, `f5f9134`, `d507856`, `97128ac` | `2af83aa` |
| ৩ | `feature/seat-availability-and-hold` | সম্পন্ন | `a748e5c`, `3937cbd`, `cac6b08`, `050f21e`, `cf7ba54`, `7ccc2ac` | `51af863` |
| ৪ | `feature/admission-confirmation-and-cancellation` | সম্পন্ন | `6878a9a`, `0e2240d`, `c454a1d`, `0502091` | `5251875` |
| — | `develop` (ইন্টিগ্রেশন সংশোধন) | সম্পন্ন | `40591d2` সময়ের নির্ভুলতা PostgreSQL অনুযায়ী করা | সরাসরি `develop`-এ |
| ৫ | `feature/frontend-admin` | সম্পন্ন | `258daee`, `ff3a7e5`, `56b3e9a`, অগ্রগতি কমিট | `804d9f5` |
| ৬ | `feature/frontend-parent` | সম্পন্ন | `3bbb3b4`, `39ba60e` | পরবর্তী আপডেটে লেখা হবে |
| ৭ | `feature/testing-and-documentation` | বাকি | | |

`main`-এর ভিত্তি কমিট: `41838aa` (প্রাথমিক রিপোজিটরি সেটআপ)।

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

## জানা সমস্যা

- এখন কোনো খোলা সমস্যা নেই। সমাধান হয়েছে: লেখার ঠিক পরে ফেরত দেওয়া সময় ১০০ ন্যানোসেকেন্ড নির্ভুলতায়
  ছিল, কিন্তু পরে পড়লে PostgreSQL-এর মাইক্রোসেকেন্ড নির্ভুলতায় আসত, ফলে বারবার confirm করলে ভিন্ন ফলাফল দেখাত।
