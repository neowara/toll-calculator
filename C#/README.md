# Toll fee calculator (C#)

Run the tests from this folder:

    dotnet test

Needs the .NET 10 SDK.

## Assumptions

The task doesn't say what the fees are per time of day, so I used the table that the old code
was clearly aiming for:

| Time        | Fee |
|-------------|-----|
| 06:00-06:29 | 8   |
| 06:30-06:59 | 13  |
| 07:00-07:59 | 18  |
| 08:00-08:29 | 13  |
| 08:30-14:59 | 8   |
| 15:00-15:29 | 13  |
| 15:30-16:59 | 18  |
| 17:00-17:59 | 13  |
| 18:00-18:29 | 8   |
| otherwise   | 0   |

- "Once an hour" means an hour starts at the first pass and lasts 60 minutes (exactly 60 counts
  as the same hour). The next pass after that starts a new hour, and only the highest fee in
  each hour is charged.
  A pass that costs nothing (before 06:00 for example) doesn't start an hour.
- Passes from different days are handled separately, every day has its own 60 SEK limit.
- A pass with `DateTimeKind.Utc` is converted to Swedish time (Europe/Stockholm). Any other pass is
  assumed to be in Swedish time already.
- Free days are weekends, all of July, Swedish public holidays and the day before a public
  holiday. The holidays come from the PublicHoliday package.
  The package also counts midsummer, christmas and new year's eve as holidays, but the days
  before those are normal days (like in the old 2013 list).
