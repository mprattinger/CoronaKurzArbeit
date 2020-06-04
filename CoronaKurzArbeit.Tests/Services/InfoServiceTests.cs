using CoronaKurzArbeit.DAL.DataAccessSQL;
using CoronaKurzArbeit.Logic.Services;
using CoronaKurzArbeit.Shared.Models;
using CoronaKurzArbeit.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CoronaKurzArbeit.Tests.Services
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1063:Implement IDisposable Correctly", Justification = "<Pending>")]
    public class InfoServiceTests : IDisposable
    {
        private readonly NullLogger<InfoService> logger;
        private readonly ApplicationDbContext ctx;
        private readonly IDateTimeProvider timeProvider;
        private readonly KurzarbeitSettingsConfiguration config;
        private readonly ITimeBookingsService timeBookingsService;
        private readonly ICoronaService coronaService;

        #region Arrange
        public InfoServiceTests()
        {
            logger = new NullLogger<InfoService>();
            ctx = DbContextHelper.GetContext("infs");
            timeProvider = new FakeDateTimeProvider(new DateTime(2020, 05, 19));
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
                Monday = 8.2m,
                Tuesday = 8.2m,
                Wednesday = 8.2m,
                Thursday = 8.2m,
                Friday = 5.7m,
                CoronaDays = new List<DayOfWeek> { DayOfWeek.Friday }
            };
            coronaService = new CoronaService(config, new FeiertagService(timeProvider));
            timeBookingsService = new TimeBookingsService(new NullLogger<TimeBookingsService>(), ctx, timeProvider, config, coronaService);
        }

        public void Dispose()
        {
            ctx.Dispose();
        }
        #endregion

        #region GoHome
        [Fact]
        public async Task GoHomeOnlyIn()
        {
            var theDay = new DateTime(2020, 05, 19);
            ctx.TimeBookings.RemoveRange();
            await ctx.SaveChangesAsync();
            var inBooking = new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(3)
            };
            ctx.TimeBookings.Add(inBooking);
            await ctx.SaveChangesAsync();

            var sut = new InfoService(logger, config, null, coronaService, timeBookingsService);

            var grossWTime = await timeBookingsService.GetGrossWorkTimeForDayAsync(theDay);
            var pauses = await timeBookingsService.GetPauseForDayAsync(theDay);
            var netWTime = timeBookingsService.GetNetWorkingTimeForDay(theDay, grossWTime, pauses);

            var res = sut.CalculateGoHome(grossWTime.inBooking.BookingTime, netWTime, pauses.grossPauseDuration, pauses.netPauseDuration);
            res.Hour.Should().Be(11);
            res.Minute.Should().Be(40);
        }

        [Fact]
        public async Task GoHomeOnlyInOut()
        {
            var theDay = new DateTime(2020, 05, 19);
            ctx.TimeBookings.RemoveRange();
            await ctx.SaveChangesAsync();
            var inBooking = new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(3)
            };
            ctx.TimeBookings.Add(inBooking);
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(10).AddMinutes(3)
            });
            await ctx.SaveChangesAsync();

            var sut = new InfoService(logger, config, null, coronaService, timeBookingsService);

            var grossWTime = await timeBookingsService.GetGrossWorkTimeForDayAsync(theDay);
            var pauses = await timeBookingsService.GetPauseForDayAsync(theDay);
            var netWTime = timeBookingsService.GetNetWorkingTimeForDay(theDay, grossWTime, pauses);

            var res = sut.CalculateGoHome(grossWTime.inBooking.BookingTime, netWTime, pauses.grossPauseDuration, pauses.netPauseDuration);
            res.Hour.Should().Be(11);
            res.Minute.Should().Be(40);
        }

        [Fact]
        public async Task GoHomePauseLess10()
        {
            var theDay = new DateTime(2020, 05, 19);
            ctx.TimeBookings.RemoveRange();
            await ctx.SaveChangesAsync();
            var inBooking = new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(3)
            };
            ctx.TimeBookings.Add(inBooking);
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(10).AddMinutes(3)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(10).AddMinutes(10)
            });
            await ctx.SaveChangesAsync();

            var sut = new InfoService(logger, config, null, coronaService, timeBookingsService);

            var grossWTime = await timeBookingsService.GetGrossWorkTimeForDayAsync(theDay);
            var pauses = await timeBookingsService.GetPauseForDayAsync(theDay);
            var netWTime = timeBookingsService.GetNetWorkingTimeForDay(theDay, grossWTime, pauses);

            var res = sut.CalculateGoHome(grossWTime.inBooking.BookingTime, netWTime, pauses.grossPauseDuration, pauses.netPauseDuration);
            res.Hour.Should().Be(11);
            res.Minute.Should().Be(47);
        }
        #endregion
    }
}
