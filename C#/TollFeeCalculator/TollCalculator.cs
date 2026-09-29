using System;
using System.Collections.Generic;
using PublicHoliday;
using TollFeeCalculator;

public class TollCalculator
{
    private readonly SwedenPublicHoliday _holidays = new SwedenPublicHoliday();

    // Must be sorted by start time. Before 06:00 and from 18:30 it is free.
    private static readonly FeePeriod[] _feeSchedule =
    {
        new FeePeriod(6, 0, 8),
        new FeePeriod(6, 30, 13),
        new FeePeriod(7, 0, 18),
        new FeePeriod(8, 0, 13),
        new FeePeriod(8, 30, 8),
        new FeePeriod(15, 0, 13),
        new FeePeriod(15, 30, 18),
        new FeePeriod(17, 0, 13),
        new FeePeriod(18, 0, 8),
        new FeePeriod(18, 30, 0)
    };

    private class FeePeriod
    {
        public readonly TimeSpan Start;
        public readonly int Fee;

        public FeePeriod(int hour, int minute, int fee)
        {
            Start = new TimeSpan(hour, minute, 0);
            Fee = fee;
        }
    }

    /**
     * Calculate the total toll fee for the given passes
     * The passes can be in any order and from several days, each day is capped at 60
     *
     * @param vehicle - the vehicle
     * @param dates   - date and time of all passes
     * @return - the total toll fee
     */

    public int GetTollFee(Vehicle vehicle, DateTime[] dates)
    {
        if (dates == null) throw new ArgumentNullException(nameof(dates));

        // the 60 SEK limit is per day so we handle one day at a time
        Dictionary<DateTime, List<DateTime>> passesPerDay = new Dictionary<DateTime, List<DateTime>>();
        foreach (DateTime date in dates)
        {
            if (!passesPerDay.ContainsKey(date.Date))
            {
                passesPerDay[date.Date] = new List<DateTime>();
            }
            passesPerDay[date.Date].Add(date);
        }

        int totalFee = 0;
        foreach (List<DateTime> passes in passesPerDay.Values)
        {
            totalFee += GetTollFeeForOneDay(vehicle, passes);
        }
        return totalFee;
    }

    private int GetTollFeeForOneDay(Vehicle vehicle, List<DateTime> passes)
    {
        passes.Sort();

        // An hour starts with the first pass that costs something and lasts 60 minutes.
        // Only the highest fee in each hour is charged. The next pass after that
        // starts a new hour.
        int dayFee = 0;
        int hourFee = 0;
        DateTime hourStart = DateTime.MinValue;

        foreach (DateTime pass in passes)
        {
            int fee = GetTollFee(pass, vehicle);

            // a free pass (for example before 06:00) should not start an hour
            if (fee == 0) continue;

            if ((pass - hourStart).TotalMinutes > 60)
            {
                dayFee += hourFee;
                hourFee = 0;
                hourStart = pass;
            }

            if (fee > hourFee) hourFee = fee;
        }
        dayFee += hourFee;

        if (dayFee > 60) dayFee = 60;
        return dayFee;
    }

    private bool IsTollFreeVehicle(Vehicle vehicle)
    {
        VehicleType type = vehicle.GetVehicleType();
        return type == VehicleType.Motorbike ||
               type == VehicleType.Tractor ||
               type == VehicleType.Emergency ||
               type == VehicleType.Diplomat ||
               type == VehicleType.Foreign ||
               type == VehicleType.Military;
    }

    public int GetTollFee(DateTime date, Vehicle vehicle)
    {
        if (vehicle == null) throw new ArgumentNullException(nameof(vehicle));

        if (IsTollFreeDate(date) || IsTollFreeVehicle(vehicle)) return 0;

        // the fee is valid from the start time until the next one in the table
        int fee = 0;
        foreach (FeePeriod period in _feeSchedule)
        {
            if (date.TimeOfDay >= period.Start) fee = period.Fee;
        }
        return fee;
    }

    private bool IsTollFreeDate(DateTime date)
    {
        if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday) return true;

        if (date.Month == 7) return true;

        if (_holidays.IsPublicHoliday(date)) return true;

        // The day before a public holiday is free as well. The package also counts midsummer,
        // christmas and new year's eve as holidays, but the days before those are normal days.
        DateTime tomorrow = date.AddDays(1).Date;
        bool tomorrowIsAnEve = tomorrow == SwedenPublicHoliday.MidsummerEve(tomorrow.Year) ||
                               tomorrow == SwedenPublicHoliday.ChristmasEve(tomorrow.Year) ||
                               tomorrow == SwedenPublicHoliday.NewYearsEve(tomorrow.Year);
        return _holidays.IsPublicHoliday(tomorrow) && !tomorrowIsAnEve;
    }
}
