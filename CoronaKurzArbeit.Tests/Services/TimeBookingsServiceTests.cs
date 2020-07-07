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
using System.Threading.Tasks;
using Xunit;

namespace CoronaKurzArbeit.Tests.Services
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1063:Implement IDisposable Correctly", Justification = "<Pending>")]
    public class TimeBookingsServiceTests : IDisposable
    {
        private readonly NullLogger<TimeBookingsService> logger;
        private readonly ApplicationDbContext ctx;
        private readonly KurzarbeitSettingsConfiguration config;
        private readonly IFeiertagService fService;

        #region Arrange
        public TimeBookingsServiceTests()
        {
            logger = new NullLogger<TimeBookingsService>();
            ctx = DbContextHelper.GetContext("tbs");

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
            fService = new FeiertagService(new FakeDateTimeProvider(new DateTime(2020, 05, 19)));
        }

        public void Dispose()
        {
            ctx.Dispose();
        }
        #endregion

        #region GetBookingsForDay
        [Fact]
        public async Task OneBooking()
        {
            //Prepare
            var theDay = new DateTime(2020, 05, 19);
            ctx.TimeBookings.RemoveRange();
            await ctx.SaveChangesAsync();
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(3)
            });
            await ctx.SaveChangesAsync();

            var sut = new TimeBookingsService(logger, ctx, config, fService);
            (await sut.GetBookingsForDayAsync(theDay.Date)).Count.Should().Be(1);
        }
        #endregion

        #region Workdays
        [Fact]
        public void WorkDaysForNormalWeek_Starting()
        {
            var sut = new TimeBookingsService(logger, ctx, config, fService);
            var res = sut.GetWorkDaysForWeek(new DateTime(2020, 4, 20));

            res.Count(x => x.type == WorkDayType.Workday).Should().Be(4);
            res.Where(x => x.type == WorkDayType.Workday).Sum(x => x.arbeitsZeit).Should().Be(32.8m);
        }

        [Fact]
        public void WorkDaysForNormalWeek_Middle()
        {
            var sut = new TimeBookingsService(logger, ctx, config, fService);
            var res = sut.GetWorkDaysForWeek(new DateTime(2020, 4, 22));

            res.Count(x => x.type == WorkDayType.Workday).Should().Be(4);
            res.Where(x => x.type == WorkDayType.Workday).Sum(x => x.arbeitsZeit).Should().Be(32.8m);
        }

        [Fact]
        public void WorkDaysForNormalWeek_End()
        {
            var sut = new TimeBookingsService(logger, ctx, config, fService);
            var res = sut.GetWorkDaysForWeek(new DateTime(2020, 4, 24));

            res.Count(x => x.type == WorkDayType.Workday).Should().Be(4);
            res.Where(x => x.type == WorkDayType.Workday).Sum(x => x.arbeitsZeit).Should().Be(32.8m);
        }

        [Fact]
        public void WorkDaysForWeekwithHolidayFriday()
        {
            var sut = new TimeBookingsService(logger, ctx, config, fService);
            var res = sut.GetWorkDaysForWeek(new DateTime(2020, 4, 27));

            res.Count(x => x.type == WorkDayType.Workday).Should().Be(4);
            res.Where(x => x.type == WorkDayType.Workday).Sum(x => x.arbeitsZeit).Should().Be(32.8m);
        }

        [Fact]
        public void WorkDaysForWeekwithHolidayThursday()
        {
            var sut = new TimeBookingsService(logger, ctx, config, fService);
            var res = sut.GetWorkDaysForWeek(new DateTime(2020, 5, 18));

            res.Count(x => x.type == WorkDayType.Workday).Should().Be(3);
            var sum = res.Where(x => x.type == WorkDayType.Workday).Sum(x => x.arbeitsZeit);
            sum.Should().Be(24.6m);
        }
        #endregion
    }
}
