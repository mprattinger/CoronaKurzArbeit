using CoronaKurzArbeit.Components.Modals;
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
    public class ActualWorkTimeTests
    {
        private readonly NullLogger<TimeBookingsService> tbsLogger;
        private readonly ApplicationDbContext ctx;

        public ActualWorkTimeTests()
        {
            tbsLogger = new NullLogger<TimeBookingsService>();
            ctx = DbContextHelper.GetContext("tbs");
        }

        [Fact]
        public async Task OnlyIn()
        {
            var theDay = new DateTime(2020, 04, 21);
            ctx.TimeBookings.RemoveRange();
            await ctx.SaveChangesAsync();
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(3)
            });
            await ctx.SaveChangesAsync();
            var timeProvider = new FakeDateTimeProvider(theDay);
            var tbs = new TimeBookingsService(tbsLogger, ctx);

            var sut = new ActualWorkTimeService(tbs, timeProvider);
            var (inTime, _, workTime, pauseTime) = await sut.LoadDataAsync(theDay);

            inTime.Hour.Should().Be(6);
            inTime.Minute.Should().Be(3);
            workTime.Should().Be(TimeSpan.Zero);
            pauseTime.Should().Be(TimeSpan.Zero);
        }

        [Fact]
        public async Task InAndOut()
        {
            var theDay = new DateTime(2020, 04, 21);
            ctx.TimeBookings.RemoveRange();
            await ctx.SaveChangesAsync();
            
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(3),
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(10).AddMinutes(3)
            });
            await ctx.SaveChangesAsync();

            var timeProvider = new FakeDateTimeProvider(theDay);
            var tbs = new TimeBookingsService(tbsLogger, ctx);

            var sut = new ActualWorkTimeService(tbs, timeProvider);
            var (inTime, outTime, workTime, pauseTime) = await sut.LoadDataAsync(theDay);

            inTime.Hour.Should().Be(6);
            inTime.Minute.Should().Be(3);
            workTime.Should().Be(TimeSpan.FromHours(4));
            pauseTime.Should().Be(TimeSpan.Zero);
            outTime.Hour.Should().Be(10);
            outTime.Minute.Should().Be(3);
        }

        [Fact]
        public async Task InAndOut9()
        {
            var theDay = new DateTime(2020, 04, 21);
            ctx.TimeBookings.RemoveRange();
            await ctx.SaveChangesAsync();

            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(3),
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(15).AddMinutes(3)
            });
            await ctx.SaveChangesAsync();

            var timeProvider = new FakeDateTimeProvider(theDay);
            var tbs = new TimeBookingsService(tbsLogger, ctx);

            var sut = new ActualWorkTimeService(tbs, timeProvider);
            var (inTime, outTime, workTime, pauseTime) = await sut.LoadDataAsync(theDay);

            inTime.Hour.Should().Be(6);
            inTime.Minute.Should().Be(3);
            workTime.Should().Be(TimeSpan.FromMinutes(540));
            pauseTime.Should().Be(TimeSpan.Zero);
            outTime.Hour.Should().Be(15);
            outTime.Minute.Should().Be(3);
        }

        [Fact]
        public async Task InOutIn()
        {
            var theDay = new DateTime(2020, 04, 21);
            ctx.TimeBookings.RemoveRange();
            await ctx.SaveChangesAsync();

            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(3),
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(10).AddMinutes(3)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(10).AddMinutes(33)
            });
            await ctx.SaveChangesAsync();

            var timeProvider = new FakeDateTimeProvider(theDay);
            var tbs = new TimeBookingsService(tbsLogger, ctx);

            var sut = new ActualWorkTimeService(tbs, timeProvider);
            var (inTime, outTime, workTime, pauseTime) = await sut.LoadDataAsync(theDay);

            inTime.Hour.Should().Be(6);
            inTime.Minute.Should().Be(3);
            workTime.Should().Be(TimeSpan.FromHours(4));
            pauseTime.Should().Be(TimeSpan.FromMinutes(30));
            outTime.Should().Be(DateTime.MinValue);
        }

        [Fact]
        public async Task InOutInOut()
        {
            var theDay = new DateTime(2020, 04, 21);
            ctx.TimeBookings.RemoveRange();
            await ctx.SaveChangesAsync();

            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(3),
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(10).AddMinutes(3)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(10).AddMinutes(33)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(12).AddMinutes(33)
            });
            await ctx.SaveChangesAsync();

            var timeProvider = new FakeDateTimeProvider(theDay);
            var tbs = new TimeBookingsService(tbsLogger, ctx);

            var sut = new ActualWorkTimeService(tbs, timeProvider);
            var (inTime, outTime, workTime, pauseTime) = await sut.LoadDataAsync(theDay);

            inTime.Hour.Should().Be(6);
            inTime.Minute.Should().Be(3);
            workTime.Should().Be(TimeSpan.FromHours(6));
            pauseTime.Should().Be(TimeSpan.FromMinutes(30));
            outTime.Hour.Should().Be(12);
            outTime.Minute.Should().Be(33);
        }

        [Fact]
        public async Task InOutInMultiple()
        {
            var theDay = new DateTime(2020, 04, 21);
            ctx.TimeBookings.RemoveRange();
            await ctx.SaveChangesAsync();

            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(3),
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(10).AddMinutes(3)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(10).AddMinutes(33)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(12).AddMinutes(33)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(12).AddMinutes(53)
            });
            await ctx.SaveChangesAsync();

            var timeProvider = new FakeDateTimeProvider(theDay);
            var tbs = new TimeBookingsService(tbsLogger, ctx);

            var sut = new ActualWorkTimeService(tbs, timeProvider);
            var (inTime, outTime, workTime, pauseTime) = await sut.LoadDataAsync(theDay);

            inTime.Hour.Should().Be(6);
            inTime.Minute.Should().Be(3);
            workTime.Should().Be(TimeSpan.FromHours(6));
            pauseTime.Should().Be(TimeSpan.FromMinutes(50));
            outTime.Should().Be(DateTime.MinValue);
        }

        [Fact]
        public async Task InOutInOutMultiple()
        {
            var theDay = new DateTime(2020, 04, 21);
            ctx.TimeBookings.RemoveRange();
            await ctx.SaveChangesAsync();

            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(3),
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(10).AddMinutes(3)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(10).AddMinutes(33)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(12).AddMinutes(33)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(12).AddMinutes(53)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(14).AddMinutes(53)
            });
            await ctx.SaveChangesAsync();

            var timeProvider = new FakeDateTimeProvider(theDay);
            var tbs = new TimeBookingsService(tbsLogger, ctx);

            var sut = new ActualWorkTimeService(tbs, timeProvider);
            var (inTime, outTime, workTime, pauseTime) = await sut.LoadDataAsync(theDay);

            inTime.Hour.Should().Be(6);
            inTime.Minute.Should().Be(3);
            workTime.Should().Be(TimeSpan.FromHours(8));
            pauseTime.Should().Be(TimeSpan.FromMinutes(50));
            outTime.Hour.Should().Be(14);
            outTime.Minute.Should().Be(53);
        }
    }
}
