using CoronaKurzArbeit.Data;
using CoronaKurzArbeit.Models;
using CoronaKurzArbeit.Services;
using CoronaKurzArbeit.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
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

        #region Arrange
        public TimeBookingsServiceTests()
        {
            logger = new NullLogger<TimeBookingsService>();
            ctx = DbContextHelper.GetContext("tbs");
            timeProvider = new FakeDateTimeProvider(new DateTime(2020, 05, 19));
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
                BookingTime = theDay.AddHours(6).AddMinutes(3),
                IsPause = false
            });
            await ctx.SaveChangesAsync();

            var sut = new TimeBookingsService(logger, ctx, timeProvider);
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
                BookingTime = theDay.AddHours(6).AddMinutes(3),
                IsPause = false
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(14).AddMinutes(3),
                IsPause = false
            });
            await ctx.SaveChangesAsync();

            var sut = new TimeBookingsService(logger, ctx, timeProvider);
            var res = await sut.GetInBookingForDayAsync(theDay.Date);
            res.BookingTime.Should().Be(theDay.AddHours(6).AddMinutes(3));
        }

        [Fact]
        public async Task GetInTimeEmptyWhenNoBookings()
        {
            //Prepare
            var theDay = new DateTime(2020, 05, 19);
            ctx.TimeBookings.RemoveRange();
            await ctx.SaveChangesAsync();

            var sut = new TimeBookingsService(logger, ctx, timeProvider);
            var res = await sut.GetInBookingForDayAsync(theDay.Date);
            res.BookingTime.Should().Be(DateTime.MinValue);
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
                BookingTime = theDay.AddHours(6).AddMinutes(3),
                IsPause = false
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(14).AddMinutes(3),
                IsPause = false
            });
            await ctx.SaveChangesAsync();

            var sut = new TimeBookingsService(logger, ctx, timeProvider);
            var res = await sut.GetOutBookingForDayAsync(theDay.Date);
            res.BookingTime.Should().Be(theDay.AddHours(14).AddMinutes(3));
        }

        [Fact]
        public async Task OutTimeEmptyWhenNoBookings()
        {
            //Prepare
            var theDay = new DateTime(2020, 05, 19, 6, 3, 0);
            ctx.TimeBookings.RemoveRange();
            await ctx.SaveChangesAsync();

            var sut = new TimeBookingsService(logger, ctx, timeProvider);
            var res = await sut.GetOutBookingForDayAsync(theDay.Date);
            res.BookingTime.Should().Be(DateTime.MinValue);
        }

        [Fact]
        public async Task OutTimeEmptyWhenUneven()
        {
            //Prepare
            var theDay = new DateTime(2020, 05, 19, 6, 3, 0);
            ctx.TimeBookings.RemoveRange();
            await ctx.SaveChangesAsync();
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(3),
                IsPause = false
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(12).AddMinutes(3),
                IsPause = false
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(14).AddMinutes(3),
                IsPause = false
            });
            await ctx.SaveChangesAsync();

            var sut = new TimeBookingsService(logger, ctx, timeProvider);
            var res = await sut.GetOutBookingForDayAsync(theDay.Date);
            res.BookingTime.Should().Be(DateTime.MinValue);
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
                BookingTime = theDay.AddHours(6).AddMinutes(3),
                IsPause = false
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(12).AddMinutes(3),
                IsPause = false
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(12).AddMinutes(35),
                IsPause = false
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(14).AddMinutes(3),
                IsPause = false
            });
            await ctx.SaveChangesAsync();

            var sut = new TimeBookingsService(logger, ctx, timeProvider);
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
                BookingTime = theDay.AddHours(6).AddMinutes(3),
                IsPause = false
            });
            await ctx.SaveChangesAsync();

            var sut = new TimeBookingsService(logger, ctx, timeProvider);
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

            var sut = new TimeBookingsService(logger, ctx, timeProvider);
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
                BookingTime = theDay.AddHours(6).AddMinutes(3),
                IsPause = false
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(12).AddMinutes(3),
                IsPause = false
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(12).AddMinutes(35),
                IsPause = false
            });
            await ctx.SaveChangesAsync();

            var sut = new TimeBookingsService(logger, ctx, timeProvider);
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
                BookingTime = theDay.AddHours(6).AddMinutes(3),
                IsPause = false
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(12).AddMinutes(3),
                IsPause = false
            });
            await ctx.SaveChangesAsync();

            var sut = new TimeBookingsService(logger, ctx, timeProvider);
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
                BookingTime = theDay.AddHours(6).AddMinutes(3),
                IsPause = false
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(12).AddMinutes(3),
                IsPause = false
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(12).AddMinutes(35),
                IsPause = false
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(13).AddMinutes(3),
                IsPause = false
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(13).AddMinutes(35),
                IsPause = false
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(16).AddMinutes(35),
                IsPause = false
            });
            await ctx.SaveChangesAsync();

            var sut = new TimeBookingsService(logger, ctx, timeProvider);
            var res = await sut.GetPauseBookingsForDayAsync(theDay.Date);
            res.Count.Should().Be(4);
        }
        #endregion

        #region Calculations
        [Fact]
        public async Task GetGrossWorkTime()
        {
            //Prepare
            var theDay = new DateTime(2020, 05, 19);
            ctx.TimeBookings.RemoveRange();
            await ctx.SaveChangesAsync();
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(3),
                IsPause = false
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(14).AddMinutes(3),
                IsPause = false
            });
            await ctx.SaveChangesAsync();

            var sut = new TimeBookingsService(logger, ctx, timeProvider);
            var res = await sut.GetGrossWorkTimeForDayAsync(theDay);
            res.Should().NotBeNull();
            res?.inBooking.BookingTime.Should().Be(theDay.AddHours(6).AddMinutes(3));
            res?.outBooking.BookingTime.Should().Be(theDay.AddHours(14).AddMinutes(3));
            res?.grossWorkTime.Should().Be(TimeSpan.FromHours(8));
        }
        [Fact]
        public async Task NoGrossWorkTimeIfNoBookings()
        {
            //Prepare
            var theDay = new DateTime(2020, 05, 19);
            ctx.TimeBookings.RemoveRange();
            await ctx.SaveChangesAsync();

            var sut = new TimeBookingsService(logger, ctx, timeProvider);
            var res = await sut.GetGrossWorkTimeForDayAsync(theDay);
            res.Should().BeNull();
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
                BookingTime = theDay.AddHours(6).AddMinutes(3),
                IsPause = false
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(12).AddMinutes(3),
                IsPause = false
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(12).AddMinutes(35),
                IsPause = false
            });
            await ctx.SaveChangesAsync();

            //Fake TimeProvider 
            var tProvider = new FakeDateTimeProvider(theDay.AddHours(13).AddMinutes(3));

            var sut = new TimeBookingsService(logger, ctx, tProvider);
            var res = await sut.GetGrossWorkTimeForDayAsync(theDay);
            res.Should().NotBeNull();
            res?.inBooking.BookingTime.Should().Be(theDay.AddHours(6).AddMinutes(3));
            res?.outBooking.BookingTime.Should().Be(theDay.AddHours(13).AddMinutes(3));
            res?.grossWorkTime.Should().Be(TimeSpan.FromHours(7));
        }
        #endregion
    }
}
