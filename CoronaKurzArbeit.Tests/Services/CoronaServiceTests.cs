using CoronaKurzArbeit.Models;
using CoronaKurzArbeit.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace CoronaKurzArbeit.Tests.Services
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1063:Implement IDisposable Correctly", Justification = "<Pending>")]
    public class CoronaServiceTests : IDisposable
    {
        NullLogger<CoronaService> logger;
        KurzarbeitSettingsConfiguration config;
        IFeiertagService fService;

        #region Arrange
        public CoronaServiceTests()
        {
            logger = new NullLogger<CoronaService>();
            config = new KurzarbeitSettingsConfiguration
            {
                Started = new DateTime(2020, 4, 20),
                SollArbeitsZeit = 38.5M,
                CoronaSoll = 0.8M,
                Monday = 8.2M,
                Tuesday = 8.2M,
                Wednesday = 8.2M,
                Thursday = 8.2M,
                Friday = 5.7M,
                CoronaDays = new List<DayOfWeek> { DayOfWeek.Friday }
            };
            fService = new FeiertagService();
        }

        public void Dispose()
        {
        }
        #endregion

        #region Workdays
        [Fact]
        public void WorkDaysForNormalWeek_Starting()
        {
            var sut = new CoronaService(logger, config, fService);
            var res = sut.GetWorkDaysForWeek(new DateTime(2020, 4, 20));

            res.Count(x => x.type == WorkDayType.Workday).Should().Be(4);
            res.Where(x => x.type == WorkDayType.Workday).Sum(x => x.arbeitsZeit).Should().Be(32.8M);
        }

        [Fact]
        public void WorkDaysForNormalWeek_Middle()
        {
            var sut = new CoronaService(logger, config, fService);
            var res = sut.GetWorkDaysForWeek(new DateTime(2020, 4, 22));

            res.Count(x => x.type == WorkDayType.Workday).Should().Be(4);
            res.Where(x => x.type == WorkDayType.Workday).Sum(x => x.arbeitsZeit).Should().Be(32.8M);
        }

        [Fact]
        public void WorkDaysForNormalWeek_End()
        {
            var sut = new CoronaService(logger, config, fService);
            var res = sut.GetWorkDaysForWeek(new DateTime(2020, 4, 24));

            res.Count(x => x.type == WorkDayType.Workday).Should().Be(4);
            res.Where(x => x.type == WorkDayType.Workday).Sum(x => x.arbeitsZeit).Should().Be(32.8M);
        }

        [Fact]
        public void WorkDaysForWeekwithHolidayFriday()
        {
            var sut = new CoronaService(logger, config, fService);
            var res = sut.GetWorkDaysForWeek(new DateTime(2020, 4, 27));

            res.Count(x => x.type == WorkDayType.Workday).Should().Be(4);
            res.Where(x => x.type == WorkDayType.Workday).Sum(x => x.arbeitsZeit).Should().Be(32.8M);
        }

        [Fact]
        public void WorkDaysForWeekwithHolidayThursday()
        {
            var sut = new CoronaService(logger, config, fService);
            var res = sut.GetWorkDaysForWeek(new DateTime(2020, 5, 18));

            res.Count(x => x.type == WorkDayType.Workday).Should().Be(3);
            var sum = res.Where(x => x.type == WorkDayType.Workday).Sum(x => x.arbeitsZeit);
            sum.Should().Be(24.6M);
        }
        #endregion

        #region KA
        [Fact]
        public void Calculate_KATime()
        {
            var sut = new CoronaService(logger, config, fService);
            var res = sut.GetKATime();
            res.Should().Be(7.7M);
        }
        #endregion

        #region KAAusfall
        [Fact]
        public void KAAusfall_NormalWeekWeekDay()
        {
            var sut = new CoronaService(logger, config, fService);
            var res = sut.KAAusfallPerDay(new DateTime(2020, 4, 22));

            res.Should().Be(0.5M);
        }
        [Fact]
        public void KAAusfall_NormalWeekFriday()
        {
            var sut = new CoronaService(logger, config, fService);
            var res = sut.KAAusfallPerDay(new DateTime(2020, 4, 24));

            res.Should().Be(5.7M);
        }

        [Fact]
        public void KAAusfall_FridayHolidayWeekDay()
        {
            var sut = new CoronaService(logger, config, fService);
            var res = sut.KAAusfallPerDay(new DateTime(2020, 4, 27));

            res.Should().Be(1.925M);
        }
        [Fact]
        public void KAAusfall_FridayHolidayFriday()
        {
            var sut = new CoronaService(logger, config, fService);
            var res = sut.KAAusfallPerDay(new DateTime(2020, 5, 1));

            res.Should().Be(0M);
        }

        [Fact]
        public void KAAusfall_ThursdayHolidayWeekDay()
        {
            var sut = new CoronaService(logger, config, fService);
            var res = sut.KAAusfallPerDay(new DateTime(2020, 5, 20));

            res.Should().Be(2.567M);
        }
        [Fact]
        public void KAAusfall_ThursdayHolidayThursday()
        {
            var sut = new CoronaService(logger, config, fService);
            var res = sut.KAAusfallPerDay(new DateTime(2020, 5, 21));

            res.Should().Be(0M);
        }
        [Fact]
        public void KAAusfall_ThursdayHolidayFriday()
        {
            var sut = new CoronaService(logger, config, fService);
            var res = sut.KAAusfallPerDay(new DateTime(2020, 5, 22));

            res.Should().Be(0M);
        }
        #endregion
    }
}
