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
        private readonly KurzarbeitSettingsConfiguration config;

        public ActualWorkTimeTests()
        {
            tbsLogger = new NullLogger<TimeBookingsService>();
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
                Monday = 8.2m,
                Tuesday = 8.2m,
                Wednesday = 8.2m,
                Thursday = 8.2m,
                Friday = 5.7m,
                CoronaDays = new List<DayOfWeek> { DayOfWeek.Friday }
            };
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
            var coronaService = new CoronaService(config, new FeiertagService(timeProvider));
            var tbs = new TimeBookingsService(tbsLogger, ctx, timeProvider, config, coronaService);

            var sut = new ActualWorkTimeService(tbs);
            var res = await sut.LoadData(theDay);

            res.inTime.Hour.Should().Be(6);
            res.inTime.Minute.Should().Be(3);
            res.workTime.Should().Be(TimeSpan.Zero);
            res.pauseTime.Should().Be(TimeSpan.Zero);
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
            var coronaService = new CoronaService(config, new FeiertagService(timeProvider));
            var tbs = new TimeBookingsService(tbsLogger, ctx, timeProvider, config, coronaService);

            var sut = new ActualWorkTimeService(tbs);
            var res = await sut.LoadData(theDay);

            res.inTime.Hour.Should().Be(6);
            res.inTime.Minute.Should().Be(3);
            res.workTime.Should().Be(TimeSpan.FromHours(4));
            res.pauseTime.Should().Be(TimeSpan.Zero);
            res.outTime.Hour.Should().Be(10);
            res.outTime.Minute.Should().Be(3);
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
            var coronaService = new CoronaService(config, new FeiertagService(timeProvider));
            var tbs = new TimeBookingsService(tbsLogger, ctx, timeProvider, config, coronaService);

            var sut = new ActualWorkTimeService(tbs);
            var res = await sut.LoadData(theDay);

            res.inTime.Hour.Should().Be(6);
            res.inTime.Minute.Should().Be(3);
            res.workTime.Should().Be(TimeSpan.FromHours(4));
            res.pauseTime.Should().Be(TimeSpan.FromMinutes(30));
            res.outTime.Should().Be(DateTime.MinValue);
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
            var coronaService = new CoronaService(config, new FeiertagService(timeProvider));
            var tbs = new TimeBookingsService(tbsLogger, ctx, timeProvider, config, coronaService);

            var sut = new ActualWorkTimeService(tbs);
            var res = await sut.LoadData(theDay);

            res.inTime.Hour.Should().Be(6);
            res.inTime.Minute.Should().Be(3);
            res.workTime.Should().Be(TimeSpan.FromHours(6));
            res.pauseTime.Should().Be(TimeSpan.FromMinutes(30));
            res.outTime.Hour.Should().Be(12);
            res.outTime.Minute.Should().Be(33);
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
            var coronaService = new CoronaService(config, new FeiertagService(timeProvider));
            var tbs = new TimeBookingsService(tbsLogger, ctx, timeProvider, config, coronaService);

            var sut = new ActualWorkTimeService(tbs);
            var res = await sut.LoadData(theDay);

            res.inTime.Hour.Should().Be(6);
            res.inTime.Minute.Should().Be(3);
            res.workTime.Should().Be(TimeSpan.FromHours(6));
            res.pauseTime.Should().Be(TimeSpan.FromMinutes(50));
            res.outTime.Should().Be(DateTime.MinValue);
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
            var coronaService = new CoronaService(config, new FeiertagService(timeProvider));
            var tbs = new TimeBookingsService(tbsLogger, ctx, timeProvider, config, coronaService);

            var sut = new ActualWorkTimeService(tbs);
            var res = await sut.LoadData(theDay);

            res.inTime.Hour.Should().Be(6);
            res.inTime.Minute.Should().Be(3);
            res.workTime.Should().Be(TimeSpan.FromHours(8));
            res.pauseTime.Should().Be(TimeSpan.FromMinutes(50));
            res.outTime.Hour.Should().Be(14);
            res.outTime.Minute.Should().Be(53);
        }
    }
}
