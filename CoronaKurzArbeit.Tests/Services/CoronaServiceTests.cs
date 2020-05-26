using CoronaKurzArbeit.Logic.Services;
using CoronaKurzArbeit.Shared.Models;
using CoronaKurzArbeit.Tests.Helpers;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace CoronaKurzArbeit.Tests.Services
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1063:Implement IDisposable Correctly", Justification = "<Pending>")]
    public class CoronaServiceTests : IDisposable
    {
        private readonly KurzarbeitSettingsConfiguration config;
        private readonly IFeiertagService fService;

        #region Arrange
        public CoronaServiceTests()
        {
            config = new KurzarbeitSettingsConfiguration
            {
                Started = new DateTime(2020, 4, 20),
                SollArbeitsZeit = 38.5,
                CoronaSoll = 0.8,
                Monday = 8.2,
                Tuesday = 8.2,
                Wednesday = 8.2,
                Thursday = 8.2,
                Friday = 5.7,
                CoronaDays = new List<DayOfWeek> { DayOfWeek.Friday }
            };
            fService = new FeiertagService(new FakeDateTimeProvider(new DateTime(2020,05,19)));
        }

        public void Dispose()
        {
        }
        #endregion

        #region Workdays
        [Fact]
        public void WorkDaysForNormalWeek_Starting()
        {
            var sut = new CoronaService(config, fService);
            var res = sut.GetWorkDaysForWeek(new DateTime(2020, 4, 20));

            res.Count(x => x.type == WorkDayType.Workday).Should().Be(4);
            res.Where(x => x.type == WorkDayType.Workday).Sum(x => x.arbeitsZeit).Should().Be(32.8);
        }

        [Fact]
        public void WorkDaysForNormalWeek_Middle()
        {
            var sut = new CoronaService(config, fService);
            var res = sut.GetWorkDaysForWeek(new DateTime(2020, 4, 22));

            res.Count(x => x.type == WorkDayType.Workday).Should().Be(4);
            res.Where(x => x.type == WorkDayType.Workday).Sum(x => x.arbeitsZeit).Should().Be(32.8);
        }

        [Fact]
        public void WorkDaysForNormalWeek_End()
        {
            var sut = new CoronaService(config, fService);
            var res = sut.GetWorkDaysForWeek(new DateTime(2020, 4, 24));

            res.Count(x => x.type == WorkDayType.Workday).Should().Be(4);
            res.Where(x => x.type == WorkDayType.Workday).Sum(x => x.arbeitsZeit).Should().Be(32.8);
        }

        [Fact]
        public void WorkDaysForWeekwithHolidayFriday()
        {
            var sut = new CoronaService(config, fService);
            var res = sut.GetWorkDaysForWeek(new DateTime(2020, 4, 27));

            res.Count(x => x.type == WorkDayType.Workday).Should().Be(4);
            res.Where(x => x.type == WorkDayType.Workday).Sum(x => x.arbeitsZeit).Should().Be(32.8);
        }

        [Fact]
        public void WorkDaysForWeekwithHolidayThursday()
        {
            var sut = new CoronaService(config, fService);
            var res = sut.GetWorkDaysForWeek(new DateTime(2020, 5, 18));

            res.Count(x => x.type == WorkDayType.Workday).Should().Be(3);
            var sum = res.Where(x => x.type == WorkDayType.Workday).Sum(x => x.arbeitsZeit);
            sum.Should().Be(24.6);
        }
        #endregion

        #region KA
        [Fact]
        public void Calculate_KATime()
        {
            var sut = new CoronaService(config, fService);
            var res = sut.GetKATime();
            res.Should().Be(7.7);
        }
        #endregion

        #region KAAusfall
        [Fact]
        public void KAAusfall_NormalWeekWeekDay()
        {
            var sut = new CoronaService(config, fService);
            var res = sut.KAAusfallPerDay(new DateTime(2020, 4, 22));

            res.Should().Be(0.5);
        }
        [Fact]
        public void KAAusfall_NormalWeekFriday()
        {
            var sut = new CoronaService(config, fService);
            var res = sut.KAAusfallPerDay(new DateTime(2020, 4, 24));

            res.Should().Be(5.7);
        }
        [Fact]
        public void KAAusfall_NormalWeekFriday2()
        {
            var sut = new CoronaService(config, fService);
            var res = sut.KAAusfallPerDay(new DateTime(2020, 5, 15));

            res.Should().Be(5.7);
        }

        [Fact]
        public void KAAusfall_FridayHolidayWeekDay()
        {
            var sut = new CoronaService(config, fService);
            var res = sut.KAAusfallPerDay(new DateTime(2020, 4, 27));

            res.Should().Be(1.925);
        }
        [Fact]
        public void KAAusfall_FridayHolidayFriday()
        {
            var sut = new CoronaService(config, fService);
            var res = sut.KAAusfallPerDay(new DateTime(2020, 5, 1));

            res.Should().Be(0);
        }

        [Fact]
        public void KAAusfall_ThursdayHolidayWeekDay()
        {
            var sut = new CoronaService(config, fService);
            var res = sut.KAAusfallPerDay(new DateTime(2020, 5, 20));

            res.Should().Be(2.567);
        }
        [Fact]
        public void KAAusfall_ThursdayHolidayThursday()
        {
            var sut = new CoronaService(config, fService);
            var res = sut.KAAusfallPerDay(new DateTime(2020, 5, 21));

            res.Should().Be(0);
        }
        [Fact]
        public void KAAusfall_ThursdayHolidayFriday()
        {
            var sut = new CoronaService(config, fService);
            var res = sut.KAAusfallPerDay(new DateTime(2020, 5, 22));

            res.Should().Be(0);
        }
        #endregion
    }
}
