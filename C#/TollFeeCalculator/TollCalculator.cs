using System;
using System.Collections.Generic;
using System.Globalization;
using PublicHoliday;
using TollFeeCalculator;

public class TollCalculator
{
    private readonly SwedenPublicHoliday _holidays = new SwedenPublicHoliday();

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

        // An hour starts with the first pass and lasts 60 minutes. Only the highest
        // fee in each hour is charged. The next pass after that starts a new hour.
        int dayFee = 0;
        int hourFee = 0;
        DateTime hourStart = passes[0];

        foreach (DateTime pass in passes)
        {
            if ((pass - hourStart).TotalMinutes > 60)
            {
                dayFee += hourFee;
                hourFee = 0;
                hourStart = pass;
            }

            int fee = GetTollFee(pass, vehicle);
            if (fee > hourFee) hourFee = fee;
        }
        dayFee += hourFee;

        if (dayFee > 60) dayFee = 60;
        return dayFee;
    }

    private bool IsTollFreeVehicle(Vehicle vehicle)
    {
        if (vehicle == null) return false;
        String vehicleType = vehicle.GetVehicleType();
        return vehicleType.Equals(TollFreeVehicles.Motorbike.ToString()) ||
               vehicleType.Equals(TollFreeVehicles.Tractor.ToString()) ||
               vehicleType.Equals(TollFreeVehicles.Emergency.ToString()) ||
               vehicleType.Equals(TollFreeVehicles.Diplomat.ToString()) ||
               vehicleType.Equals(TollFreeVehicles.Foreign.ToString()) ||
               vehicleType.Equals(TollFreeVehicles.Military.ToString());
    }

    public int GetTollFee(DateTime date, Vehicle vehicle)
    {
        if (IsTollFreeDate(date) || IsTollFreeVehicle(vehicle)) return 0;

        int hour = date.Hour;
        int minute = date.Minute;

        if (hour == 6 && minute <= 29) return 8;
        else if (hour == 6) return 13;
        else if (hour == 7) return 18;
        else if (hour == 8 && minute <= 29) return 13;
        else if (hour == 8 || (hour >= 9 && hour <= 14)) return 8;
        else if (hour == 15 && minute <= 29) return 13;
        else if (hour == 15 || hour == 16) return 18;
        else if (hour == 17) return 13;
        else if (hour == 18 && minute <= 29) return 8;
        else return 0;
    }

    private Boolean IsTollFreeDate(DateTime date)
    {
        if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday) return true;

        if (date.Month == 7) return true;

        // the day before a public holiday is free as well
        return _holidays.IsPublicHoliday(date) || _holidays.IsPublicHoliday(date.AddDays(1));
    }

    private enum TollFreeVehicles
    {
        Motorbike = 0,
        Tractor = 1,
        Emergency = 2,
        Diplomat = 3,
        Foreign = 4,
        Military = 5
    }
}