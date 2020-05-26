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
                SollArbeitsZeit = 38.5,
                CoronaSoll = 0.8,
                PauseFree = 10,
                Monday = 8.2,
                Tuesday = 8.2,
                Wednesday = 8.2,
                Thursday = 8.2,
                Friday = 5.7,
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

            var sut = new InfoService(logger, config, coronaService, timeBookingsService);
            var res = sut.CalculateGoHome(inBooking.BookingTime, TimeSpan.FromHours(7.7), TimeSpan.Zero, TimeSpan.Zero);
            res.Hour.Should().Be(14);
            res.Minute.Should().Be(15);
        }
        #endregion
    }
}
