using CoronaKurzArbeit.DAL.DataAccessSQL;
using CoronaKurzArbeit.Logic.Services;
using CoronaKurzArbeit.Shared.Models;
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
        private readonly IDateTimeProvider timeProvider;
        private readonly KurzarbeitSettingsConfiguration config;
        private readonly ICoronaService coronaService;

        #region Arrange
        public TimeBookingsServiceTests()
        {
            logger = new NullLogger<TimeBookingsService>();
            ctx = DbContextHelper.GetContext("tbs");
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

            var sut = new TimeBookingsService(logger, ctx, timeProvider, config, coronaService);
            (await sut.GetBookingsForDayAsync(theDay.Date)).Count.Should().Be(1);
        }
        #endregion

        #region InTimes
        [Fact]
        public async Task GetInTime()
        {
            //Prepare
            var theDay = new DateTime(2020, 05, 19);
            ctx.TimeBookings.RemoveRange();
            await ctx.SaveChangesAsync();
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(3)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(14).AddMinutes(3)
            });
            await ctx.SaveChangesAsync();

            var sut = new TimeBookingsService(logger, ctx, timeProvider, config, coronaService);
            var res = await sut.GetInBookingForDayAsync(theDay.Date);
            res.BookingTime.Should().Be(theDay.AddHours(6).AddMinutes(3));
        }

        [Fact]
        public async Task GetInTimeNullWhenNoBookings()
        {
            //Prepare
            var theDay = new DateTime(2020, 05, 19);
            ctx.TimeBookings.RemoveRange();
            await ctx.SaveChangesAsync();

            var sut = new TimeBookingsService(logger, ctx, timeProvider, config, coronaService);
            var res = await sut.GetInBookingForDayAsync(theDay.Date);
            res.Should().BeNull();
        }
        #endregion

        #region OutTimes
        [Fact]
        public async Task GetOutTime()
        {
            //Prepare
            var theDay = new DateTime(2020, 05, 19, 6, 3, 0);
            ctx.TimeBookings.RemoveRange();
            await ctx.SaveChangesAsync();
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(3)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(14).AddMinutes(3)
            });
            await ctx.SaveChangesAsync();

            var sut = new TimeBookingsService(logger, ctx, timeProvider, config, coronaService);
            var res = await sut.GetOutBookingForDayAsync(theDay.Date);
            res.BookingTime.Should().Be(theDay.AddHours(14).AddMinutes(3));
        }

        [Fact]
        public async Task OutTimeNullWhenNoBookings()
        {
            //Prepare
            var theDay = new DateTime(2020, 05, 19, 6, 3, 0);
            ctx.TimeBookings.RemoveRange();
            await ctx.SaveChangesAsync();

            var sut = new TimeBookingsService(logger, ctx, timeProvider, config, coronaService);
            var res = await sut.GetOutBookingForDayAsync(theDay.Date);
            res.Should().BeNull();
        }

        [Fact]
        public async Task OutTimeNullyWhenUneven()
        {
            //Prepare
            var theDay = new DateTime(2020, 05, 19, 6, 3, 0);
            ctx.TimeBookings.RemoveRange();
            await ctx.SaveChangesAsync();
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(3)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(12).AddMinutes(3)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(14).AddMinutes(3)
            });
            await ctx.SaveChangesAsync();

            var sut = new TimeBookingsService(logger, ctx, timeProvider, config, coronaService);
            var res = await sut.GetOutBookingForDayAsync(theDay.Date);
            res.Should().BeNull();
        }
        #endregion

        #region Pause
        [Fact]
        public async Task GetPause()
        {
            //Prepare
            var theDay = new DateTime(2020, 05, 19, 6, 3, 0);
            ctx.TimeBookings.RemoveRange();
            await ctx.SaveChangesAsync();
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(3)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(12).AddMinutes(3)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(12).AddMinutes(35)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(14).AddMinutes(3)
            });
            await ctx.SaveChangesAsync();

            var sut = new TimeBookingsService(logger, ctx, timeProvider, config, coronaService);
            var res = await sut.GetPauseBookingsForDayAsync(theDay.Date);
            res.Count.Should().Be(2);
        }

        [Fact]
        public async Task NoPauseWhenOnlyIn()
        {
            //Prepare
            var theDay = new DateTime(2020, 05, 19, 6, 3, 0);
            ctx.TimeBookings.RemoveRange();
            await ctx.SaveChangesAsync();
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(3)
            });
            await ctx.SaveChangesAsync();

            var sut = new TimeBookingsService(logger, ctx, timeProvider, config, coronaService);
            var res = await sut.GetPauseBookingsForDayAsync(theDay.Date);
            res.Count.Should().Be(0);
        }

        [Fact]
        public async Task NoPauseWhenEmpty()
        {
            //Prepare
            var theDay = new DateTime(2020, 05, 19, 6, 3, 0);
            ctx.TimeBookings.RemoveRange();
            await ctx.SaveChangesAsync();

            var sut = new TimeBookingsService(logger, ctx, timeProvider, config, coronaService);
            var res = await sut.GetPauseBookingsForDayAsync(theDay.Date);
            res.Count.Should().Be(0);
        }

        [Fact]
        public async Task PauseWhenNoOut()
        {
            //Prepare
            var theDay = new DateTime(2020, 05, 19, 6, 3, 0);
            ctx.TimeBookings.RemoveRange();
            await ctx.SaveChangesAsync();
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(3)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(12).AddMinutes(3)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(12).AddMinutes(35)
            });
            await ctx.SaveChangesAsync();

            var sut = new TimeBookingsService(logger, ctx, timeProvider, config, coronaService);
            var res = await sut.GetPauseBookingsForDayAsync(theDay.Date);
            res.Count.Should().Be(2);
        }

        [Fact]
        public async Task NoPauseWhenOnlyInOut()
        {
            //Prepare
            var theDay = new DateTime(2020, 05, 19, 6, 3, 0);
            ctx.TimeBookings.RemoveRange();
            await ctx.SaveChangesAsync();
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(3)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(12).AddMinutes(3)
            });
            await ctx.SaveChangesAsync();

            var sut = new TimeBookingsService(logger, ctx, timeProvider, config, coronaService);
            var res = await sut.GetPauseBookingsForDayAsync(theDay.Date);
            res.Count.Should().Be(0);
        }

        [Fact]
        public async Task TwoPauses()
        {
            //Prepare
            var theDay = new DateTime(2020, 05, 19, 6, 3, 0);
            ctx.TimeBookings.RemoveRange();
            await ctx.SaveChangesAsync();
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(3)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(12).AddMinutes(3)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(12).AddMinutes(35)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(13).AddMinutes(3)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(13).AddMinutes(35)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(16).AddMinutes(35)
            });
            await ctx.SaveChangesAsync();

            var sut = new TimeBookingsService(logger, ctx, timeProvider, config, coronaService);
            var res = await sut.GetPauseBookingsForDayAsync(theDay.Date);
            res.Count.Should().Be(4);
        }
        #endregion

        #region GrossTimeCalculations
        [Fact]
        public async Task GetGrossWorkTime()
        {
            //Prepare
            var theDay = new DateTime(2020, 05, 19);
            ctx.TimeBookings.RemoveRange();
            await ctx.SaveChangesAsync();
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(3)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(14).AddMinutes(3)
            });
            await ctx.SaveChangesAsync();

            var sut = new TimeBookingsService(logger, ctx, timeProvider, config, coronaService);
            var (inBooking, outBooking, grossWorkTime) = await sut.GetGrossWorkTimeForDayAsync(theDay);
            inBooking.BookingTime.Should().Be(theDay.AddHours(6).AddMinutes(3));
            outBooking.BookingTime.Should().Be(theDay.AddHours(14).AddMinutes(3));
            grossWorkTime.Should().Be(TimeSpan.FromHours(8));
        }
        [Fact]
        public async Task NoGrossWorkTimeIfNoBookings()
        {
            //Prepare
            var theDay = new DateTime(2020, 05, 19);
            ctx.TimeBookings.RemoveRange();
            await ctx.SaveChangesAsync();

            var sut = new TimeBookingsService(logger, ctx, timeProvider, config, coronaService);
            var (inBooking, outBooking, grossWorkTime) = await sut.GetGrossWorkTimeForDayAsync(theDay);
            inBooking.Should().BeNull();
            outBooking.Should().BeNull();
            grossWorkTime.Should().Be(TimeSpan.Zero);
        }
        [Fact]
        public async Task GrossWorkTimeNoOutBooking()
        {
            //Prepare
            var theDay = new DateTime(2020, 05, 19);
            ctx.TimeBookings.RemoveRange();
            await ctx.SaveChangesAsync();
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(3)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(12).AddMinutes(3)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(12).AddMinutes(35)
            });
            await ctx.SaveChangesAsync();

            //Fake TimeProvider 
            var tProvider = new FakeDateTimeProvider(theDay.AddHours(13).AddMinutes(3));

            var sut = new TimeBookingsService(logger, ctx, tProvider, config, coronaService);
            var (inBooking, outBooking, grossWorkTime) = await sut.GetGrossWorkTimeForDayAsync(theDay);
            inBooking.BookingTime.Should().Be(theDay.AddHours(6).AddMinutes(3));
            outBooking.BookingTime.Should().Be(theDay.AddHours(12).AddMinutes(02).AddSeconds(58).AddMilliseconds(800));
            grossWorkTime.Should().Be(TimeSpan.FromHours(6).Subtract(TimeSpan.FromMilliseconds(1200)));
        }
        #endregion

        #region PauseCalculations
        [Fact]
        public async Task GetPausesForDay()
        {
            //Prepare
            var theDay = new DateTime(2020, 05, 19);
            ctx.TimeBookings.RemoveRange();
            await ctx.SaveChangesAsync();
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(3)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(12).AddMinutes(3)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(12).AddMinutes(33)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(12).AddMinutes(35)
            });
            await ctx.SaveChangesAsync();

            var sut = new TimeBookingsService(logger, ctx, timeProvider, config, coronaService);
            var (grossPauseDuration, netPauseDuration, pauseList) = await sut.GetPauseForDayAsync(theDay);
            pauseList.Count.Should().Be(1);
            pauseList.First().duration.TotalMinutes.Should().Be(30);
            grossPauseDuration.TotalMinutes.Should().Be(30);
            netPauseDuration.TotalMinutes.Should().Be(20);
        }
        [Fact]
        public async Task GetPausesForDayIsNullWhenNoPause()
        {
            //Prepare
            var theDay = new DateTime(2020, 05, 19);
            ctx.TimeBookings.RemoveRange();
            await ctx.SaveChangesAsync();
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(3)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(12).AddMinutes(35)
            });
            await ctx.SaveChangesAsync();


            var sut = new TimeBookingsService(logger, ctx, timeProvider, config, coronaService);
            var (grossPauseDuration, netPauseDuration, pauseList) = await sut.GetPauseForDayAsync(theDay);
            grossPauseDuration.Should().Be(TimeSpan.Zero);
            netPauseDuration.Should().Be(TimeSpan.Zero);
            pauseList.Count.Should().Be(0);
        }
        [Fact]
        public async Task GetPausesForDayIsNullWhenOnlyIn()
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


            var sut = new TimeBookingsService(logger, ctx, timeProvider, config, coronaService);
            var (grossPauseDuration, netPauseDuration, _) = await sut.GetPauseForDayAsync(theDay);
            grossPauseDuration.Should().Be(TimeSpan.Zero);
            netPauseDuration.Should().Be(TimeSpan.Zero);
            grossPauseDuration.Should().Be(TimeSpan.Zero);
        }
        [Fact]
        public async Task GetPausesForDayOpenEnd()
        {
            //Prepare
            var theDay = new DateTime(2020, 05, 19);
            ctx.TimeBookings.RemoveRange();
            await ctx.SaveChangesAsync();
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(3)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(12).AddMinutes(3)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(12).AddMinutes(33)
            });
            await ctx.SaveChangesAsync();

            var sut = new TimeBookingsService(logger, ctx, timeProvider, config, coronaService);
            var (grossPauseDuration, netPauseDuration, pauseList) = await sut.GetPauseForDayAsync(theDay);
            pauseList.Count.Should().Be(1);
            pauseList.First().duration.TotalMinutes.Should().Be(30);
            grossPauseDuration.TotalMinutes.Should().Be(30);
            netPauseDuration.TotalMinutes.Should().Be(20);
        }

        [Fact]
        public async Task NoFreePauseIfPauseUnder10Min()
        {
            //Prepare
            var theDay = new DateTime(2020, 05, 19);
            ctx.TimeBookings.RemoveRange();
            await ctx.SaveChangesAsync();
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(3)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(12).AddMinutes(3)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(12).AddMinutes(8)
            });
            await ctx.SaveChangesAsync();

            var sut = new TimeBookingsService(logger, ctx, timeProvider, config, coronaService);
            var (grossPauseDuration, netPauseDuration, pauseList) = await sut.GetPauseForDayAsync(theDay);
            pauseList.Count.Should().Be(1);
            pauseList.First().duration.TotalMinutes.Should().Be(5);
            grossPauseDuration.TotalMinutes.Should().Be(5);
            netPauseDuration.TotalMinutes.Should().Be(5);
        }
        [Fact]
        public async Task FreePauseIfMultiplePauses0NetPause()
        {
            //Prepare
            var theDay = new DateTime(2020, 05, 19);
            ctx.TimeBookings.RemoveRange();
            await ctx.SaveChangesAsync();
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(3)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(12).AddMinutes(3)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(12).AddMinutes(8)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(12).AddMinutes(20)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(12).AddMinutes(25)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(14)
            });
            await ctx.SaveChangesAsync();

            var sut = new TimeBookingsService(logger, ctx, timeProvider, config, coronaService);
            var (grossPauseDuration, netPauseDuration, pauseList) = await sut.GetPauseForDayAsync(theDay);
            pauseList.Count.Should().Be(2);
            pauseList.First().duration.TotalMinutes.Should().Be(5);
            grossPauseDuration.TotalMinutes.Should().Be(10);
            netPauseDuration.TotalMinutes.Should().Be(0);
        }
        [Fact]
        public async Task FreePauseIfMultiplePauses()
        {
            //Prepare
            var theDay = new DateTime(2020, 05, 19);
            ctx.TimeBookings.RemoveRange();
            await ctx.SaveChangesAsync();
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(3)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(12).AddMinutes(3)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(12).AddMinutes(8)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(12).AddMinutes(20)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(12).AddMinutes(55)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(14)
            });
            await ctx.SaveChangesAsync();

            var sut = new TimeBookingsService(logger, ctx, timeProvider, config, coronaService);
            var (grossPauseDuration, netPauseDuration, pauseList) = await sut.GetPauseForDayAsync(theDay);
            pauseList.Count.Should().Be(2);
            pauseList.First().duration.TotalMinutes.Should().Be(5);
            grossPauseDuration.TotalMinutes.Should().Be(40);
            netPauseDuration.TotalMinutes.Should().Be(30);
        }
        #endregion

        #region NetWorkingTime
        [Fact]
        public async Task GetNetWorkingTime()
        {
            //Prepare
            var theDay = new DateTime(2020, 05, 19);
            ctx.TimeBookings.RemoveRange();
            await ctx.SaveChangesAsync();
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(3)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(12).AddMinutes(3)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(12).AddMinutes(43)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(14).AddMinutes(33)
            });
            await ctx.SaveChangesAsync();

            var sut = new TimeBookingsService(logger, ctx, timeProvider, config, coronaService);
            var res = await sut.GetNetWorkingTimeForDayAsync(theDay);
            res.TotalHours.Should().Be(8);
        }

        [Fact]
        public async Task NetWorkingTimeNoPauseLess6()
        {
            //Prepare
            var theDay = new DateTime(2020, 05, 19);
            ctx.TimeBookings.RemoveRange();
            await ctx.SaveChangesAsync();
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(3)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(12).AddMinutes(0)
            });
            await ctx.SaveChangesAsync();

            var sut = new TimeBookingsService(logger, ctx, timeProvider, config, coronaService);
            var res = await sut.GetNetWorkingTimeForDayAsync(theDay);
            res.TotalHours.Should().Be(5.95);
        }
        [Fact]
        public async Task NetWorkingTimeNoPause6()
        {
            //Prepare
            var theDay = new DateTime(2020, 05, 19);
            ctx.TimeBookings.RemoveRange();
            await ctx.SaveChangesAsync();
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(3)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(12).AddMinutes(3)
            });
            await ctx.SaveChangesAsync();

            var sut = new TimeBookingsService(logger, ctx, timeProvider, config, coronaService);
            var res = await sut.GetNetWorkingTimeForDayAsync(theDay);
            res.TotalHours.Should().Be(5.5);
        }
        [Fact]
        public async Task NetWorkingTimeNoPauseMoreAs6()
        {
            //Prepare
            var theDay = new DateTime(2020, 05, 19);
            ctx.TimeBookings.RemoveRange();
            await ctx.SaveChangesAsync();
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(3)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(12).AddMinutes(10)
            });
            await ctx.SaveChangesAsync();

            var sut = new TimeBookingsService(logger, ctx, timeProvider, config, coronaService);
            var res = await sut.GetNetWorkingTimeForDayAsync(theDay);
            res.Hours.Should().Be(5);
            res.Minutes.Should().Be(37);
        }
        [Fact]
        public async Task NetWorkingTimePauseLess30WorkMoreAs6()
        {
            //Prepare
            var theDay = new DateTime(2020, 05, 19);
            ctx.TimeBookings.RemoveRange();
            await ctx.SaveChangesAsync();
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(3)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(9).AddMinutes(3)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(9).AddMinutes(13)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(12).AddMinutes(10)
            });
            await ctx.SaveChangesAsync();

            var sut = new TimeBookingsService(logger, ctx, timeProvider, config, coronaService);
            var res = await sut.GetNetWorkingTimeForDayAsync(theDay);
            res.Hours.Should().Be(5);
            res.Minutes.Should().Be(47); //10 Minuten Pause gemacht -> 10 Minuten geschenkt -> auf die 30 Fehlen aber trotzdem 20
        }
        #endregion
    }
}
