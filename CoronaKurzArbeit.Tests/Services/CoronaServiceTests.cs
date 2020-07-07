using CoronaKurzArbeit.DAL.DataAccessSQL;
using CoronaKurzArbeit.Logic.Services;
using CoronaKurzArbeit.Shared.Models;
using CoronaKurzArbeit.Shared.Services;
using CoronaKurzArbeit.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace CoronaKurzArbeit.Tests.Services
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1063:Implement IDisposable Correctly", Justification = "<Pending>")]
    public class CoronaServiceTests : IDisposable
    {
        private readonly NullLogger<TimeBookingsService> logger;
        private readonly ApplicationDbContext ctx;
        private readonly KurzarbeitSettingsConfiguration config;
        private readonly ITimeBookingsService tbs;

        #region Arrange
        public CoronaServiceTests()
        {
            logger = new NullLogger<TimeBookingsService>();
            ctx = DbContextHelper.GetContext("cst");

            config = new KurzarbeitSettingsConfiguration
            {
                Started = new DateTime(2020, 4, 20),
                SollArbeitsZeit = 38.5m,
                CoronaSoll = new List<CoronaAusfall>
            {
                new CoronaAusfall { Ausfall = 0.2m, Bis = new DateTime(2020, 6, 1) },
                new CoronaAusfall { Ausfall = 0.3m, Bis = new DateTime(2099, 1, 1) }
            },
                PauseFree = 10,
                SollPause = 30,
                Monday = 8.2m,
                Tuesday = 8.2m,
                Wednesday = 8.2m,
                Thursday = 8.2m,
                Friday = 5.7m,
                CoronaDays = new List<DayOfWeek> { DayOfWeek.Friday }
            };
            var fService = new FeiertagService(new FakeDateTimeProvider(new DateTime(2020, 05, 19)));
            tbs = new TimeBookingsService(logger, ctx, config, fService);
        }

        public void Dispose()
        {
        }
        #endregion

        

        #region KA
        [Fact]
        public void Calculate_KATime()
        {
            var sut = new CoronaService(config, tbs);
            var res = sut.GetKATime(new DateTime(2020, 5, 18));
            res.Should().Be(7.7m);
        }
        #endregion

        #region KAAusfall
        [Fact]
        public void KAAusfall_NormalWeekWeekDay()
        {
            var sut = new CoronaService(config, tbs);
            var res = sut.KAAusfallPerDay(new DateTime(2020, 4, 22));

            res.Should().Be(0.5m);
        }
        [Fact]
        public void KAAusfall_NormalWeekFriday()
        {
            var sut = new CoronaService(config, tbs);
            var res = sut.KAAusfallPerDay(new DateTime(2020, 4, 24));

            res.Should().Be(5.7m);
        }
        [Fact]
        public void KAAusfall_NormalWeekFriday2()
        {
            var sut = new CoronaService(config, tbs);
            var res = sut.KAAusfallPerDay(new DateTime(2020, 5, 15));

            res.Should().Be(5.7m);
        }

        [Fact]
        public void KAAusfall_FridayHolidayWeekDay()
        {
            var sut = new CoronaService(config, tbs);
            var res = sut.KAAusfallPerDay(new DateTime(2020, 4, 27));

            res.Should().Be(1.925m);
        }
        [Fact]
        public void KAAusfall_FridayHolidayFriday()
        {
            var sut = new CoronaService(config, tbs);
            var res = sut.KAAusfallPerDay(new DateTime(2020, 5, 1));

            res.Should().Be(0);
        }

        [Fact]
        public void KAAusfall_ThursdayHolidayWeekDay()
        {
            var sut = new CoronaService(config, tbs);
            var res = sut.KAAusfallPerDay(new DateTime(2020, 5, 20));

            res.Should().Be(2.5666666666666666666666666666667m);
        }
        [Fact]
        public void KAAusfall_ThursdayHolidayThursday()
        {
            var sut = new CoronaService(config, tbs);
            var res = sut.KAAusfallPerDay(new DateTime(2020, 5, 21));

            res.Should().Be(0);
        }
        [Fact]
        public void KAAusfall_ThursdayHolidayFriday()
        {
            var sut = new CoronaService(config, tbs);
            var res = sut.KAAusfallPerDay(new DateTime(2020, 5, 22));

            res.Should().Be(0);
        }
        #endregion
    }
}
