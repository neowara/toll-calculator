using TollFeeCalculator;

namespace TollFeeCalculator.Tests
{
    public class TollCalculatorTests
    {
        private readonly TollCalculator _calculator = new TollCalculator();
        private readonly Vehicle _car = new Car();

        // A normal tuesday, not close to any holiday
        private static DateTime At(int hour, int minute)
        {
            return new DateTime(2024, 3, 5, hour, minute, 0);
        }

        // Only Car and Motorbike exist as classes, this covers the other types
        private class OtherVehicle : Vehicle
        {
            private readonly VehicleType _type;

            public OtherVehicle(VehicleType type)
            {
                _type = type;
            }

            public VehicleType GetVehicleType()
            {
                return _type;
            }
        }

        [Theory]
        [InlineData(5, 59, 0)]
        [InlineData(6, 0, 8)]
        [InlineData(6, 29, 8)]
        [InlineData(6, 30, 13)]
        [InlineData(6, 59, 13)]
        [InlineData(7, 0, 18)]
        [InlineData(7, 59, 18)]
        [InlineData(8, 0, 13)]
        [InlineData(8, 29, 13)]
        [InlineData(8, 30, 8)]
        [InlineData(9, 0, 8)]
        [InlineData(9, 15, 8)]
        [InlineData(12, 45, 8)]
        [InlineData(14, 59, 8)]
        [InlineData(15, 0, 13)]
        [InlineData(15, 29, 13)]
        [InlineData(15, 30, 18)]
        [InlineData(16, 59, 18)]
        [InlineData(17, 0, 13)]
        [InlineData(17, 59, 13)]
        [InlineData(18, 0, 8)]
        [InlineData(18, 29, 8)]
        [InlineData(18, 30, 0)]
        [InlineData(23, 0, 0)]
        public void FeeDependsOnTimeOfDay(int hour, int minute, int expectedFee)
        {
            int fee = _calculator.GetTollFee(At(hour, minute), _car);

            Assert.Equal(expectedFee, fee);
        }

        [Theory]
        [InlineData(VehicleType.Motorbike)]
        [InlineData(VehicleType.Tractor)]
        [InlineData(VehicleType.Emergency)]
        [InlineData(VehicleType.Diplomat)]
        [InlineData(VehicleType.Foreign)]
        [InlineData(VehicleType.Military)]
        public void TollFreeVehiclesPayNothing(VehicleType type)
        {
            Vehicle vehicle = new OtherVehicle(type);

            Assert.Equal(0, _calculator.GetTollFee(At(7, 30), vehicle));
        }

        [Fact]
        public void MotorbikeClassIsTollFree()
        {
            Assert.Equal(0, _calculator.GetTollFee(At(7, 30), new Motorbike()));
        }

        [Fact]
        public void NullVehicleThrows()
        {
            Assert.Throws<ArgumentNullException>(() => _calculator.GetTollFee(At(7, 30), null!));
        }

        [Theory]
        [InlineData(2024, 3, 9)]   // saturday
        [InlineData(2024, 3, 10)]  // sunday
        [InlineData(2024, 7, 10)]  // july is free
        public void WeekendsAndJulyAreFree(int year, int month, int day)
        {
            DateTime date = new DateTime(year, month, day, 7, 30, 0);

            Assert.Equal(0, _calculator.GetTollFee(date, _car));
        }

        [Theory]
        [InlineData(2025, 6, 30, 18)]
        [InlineData(2025, 7, 1, 0)]
        [InlineData(2025, 7, 31, 0)]
        [InlineData(2025, 8, 1, 18)]
        public void JulyIsFreeButNotTheDaysAround(int year, int month, int day, int expectedFee)
        {
            DateTime date = new DateTime(year, month, day, 7, 30, 0);

            Assert.Equal(expectedFee, _calculator.GetTollFee(date, _car));
        }

        // Every date the old code had hardcoded for 2013
        [Theory]
        [InlineData(2013, 1, 1)]
        [InlineData(2013, 3, 28)]
        [InlineData(2013, 3, 29)]
        [InlineData(2013, 4, 1)]
        [InlineData(2013, 4, 30)]
        [InlineData(2013, 5, 1)]
        [InlineData(2013, 5, 8)]
        [InlineData(2013, 5, 9)]
        [InlineData(2013, 6, 5)]
        [InlineData(2013, 6, 6)]
        [InlineData(2013, 6, 21)]
        [InlineData(2013, 11, 1)]
        [InlineData(2013, 12, 24)]
        [InlineData(2013, 12, 25)]
        [InlineData(2013, 12, 26)]
        [InlineData(2013, 12, 31)]
        public void HolidaysFrom2013AreStillFree(int year, int month, int day)
        {
            DateTime date = new DateTime(year, month, day, 7, 30, 0);

            Assert.Equal(0, _calculator.GetTollFee(date, _car));
        }

        // The holidays used to only work for 2013
        [Theory]
        [InlineData(2024, 4, 30)]   // day before 1 May
        [InlineData(2024, 5, 1)]
        [InlineData(2024, 5, 8)]    // day before Ascension Day
        [InlineData(2024, 5, 9)]
        [InlineData(2024, 12, 24)]
        [InlineData(2024, 12, 25)]
        [InlineData(2025, 4, 17)]   // day before Good Friday
        [InlineData(2025, 4, 18)]
        [InlineData(2026, 6, 19)]
        public void HolidaysAndTheDayBeforeAreFreeInOtherYears(int year, int month, int day)
        {
            DateTime date = new DateTime(year, month, day, 7, 30, 0);

            Assert.Equal(0, _calculator.GetTollFee(date, _car));
        }

        // The old code did not have these as free days in 2013 either
        [Theory]
        [InlineData(2013, 6, 20)]   // day before Midsummer Eve
        [InlineData(2013, 12, 23)]  // day before Christmas Eve
        [InlineData(2013, 12, 30)]  // day before New Year's Eve
        [InlineData(2026, 6, 18)]
        [InlineData(2024, 12, 23)]
        [InlineData(2024, 12, 30)]
        public void DayBeforeAnEveIsCharged(int year, int month, int day)
        {
            DateTime date = new DateTime(year, month, day, 7, 30, 0);

            Assert.Equal(18, _calculator.GetTollFee(date, _car));
        }

        [Fact]
        public void DayAfterAHolidayIsCharged()
        {
            // 26 Dec 2024 is a holiday, friday the 27th is a normal day
            DateTime date = new DateTime(2024, 12, 27, 7, 30, 0);

            Assert.Equal(18, _calculator.GetTollFee(date, _car));
        }

        [Fact]
        public void SinglePassIsChargedItsFee()
        {
            DateTime[] passes = { At(7, 30) };

            Assert.Equal(18, _calculator.GetTollFee(_car, passes));
        }

        [Fact]
        public void NullPassesThrows()
        {
            Assert.Throws<ArgumentNullException>(() => _calculator.GetTollFee(_car, null!));
        }

        [Fact]
        public void NoPassesMeansNoFee()
        {
            Assert.Equal(0, _calculator.GetTollFee(_car, new DateTime[0]));
        }

        [Fact]
        public void HighestFeeInTheSameHourIsCharged()
        {
            DateTime[] passes = { At(6, 15), At(6, 45) };

            Assert.Equal(13, _calculator.GetTollFee(_car, passes));
        }

        [Fact]
        public void PassExactlyOneHourLaterCountsAsSameHour()
        {
            DateTime[] passes = { At(6, 15), At(7, 15) };

            Assert.Equal(18, _calculator.GetTollFee(_car, passes));
        }

        [Fact]
        public void PassMoreThanAnHourLaterIsChargedAgain()
        {
            DateTime[] passes = { At(6, 15), At(7, 16) };

            Assert.Equal(8 + 18, _calculator.GetTollFee(_car, passes));
        }

        [Fact]
        public void PassesAreGroupedFromTheFirstPassOfEachHour()
        {
            // 06:15 and 06:50 are one hour, 07:20 is more than 60 minutes after 06:15
            DateTime[] passes = { At(6, 15), At(6, 50), At(7, 20) };

            Assert.Equal(13 + 18, _calculator.GetTollFee(_car, passes));
        }

        [Fact]
        public void PassWithoutFeeDoesNotStartAnHour()
        {
            // 05:30 is free, so the hour starts at 06:20 and 06:50 is part of it
            DateTime[] passes = { At(5, 30), At(6, 20), At(6, 50), At(7, 10) };

            Assert.Equal(18, _calculator.GetTollFee(_car, passes));
        }

        [Fact]
        public void PassesInWrongOrderGiveTheSameResult()
        {
            DateTime[] passes = { At(7, 20), At(6, 50), At(6, 15) };

            Assert.Equal(13 + 18, _calculator.GetTollFee(_car, passes));
        }

        [Fact]
        public void DailyFeeIsCappedAt60()
        {
            // 18 + 13 + 13 + 18 = 62
            DateTime[] passes = { At(7, 0), At(8, 15), At(15, 15), At(16, 30) };

            Assert.Equal(60, _calculator.GetTollFee(_car, passes));
        }

        [Fact]
        public void TollFreeVehicleWithManyPassesPaysNothing()
        {
            DateTime[] passes = { At(7, 0), At(8, 15), At(15, 15) };

            Assert.Equal(0, _calculator.GetTollFee(new Motorbike(), passes));
        }

        [Fact]
        public void PassesOnDifferentDaysAreNotMixedUp()
        {
            DateTime[] passes = { At(7, 0), At(7, 0).AddDays(1) };

            Assert.Equal(36, _calculator.GetTollFee(_car, passes));
        }

        [Fact]
        public void EachDayHasItsOwnCap()
        {
            DateTime[] passes = { At(7, 0), At(8, 15), At(15, 15), At(16, 30), At(7, 0).AddDays(1) };

            Assert.Equal(60 + 18, _calculator.GetTollFee(_car, passes));
        }
    }
}
