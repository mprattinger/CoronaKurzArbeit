using CoronaKurzArbeit.DAL.DataAccessSQL;
using CoronaKurzArbeit.Logic.Services;
using CoronaKurzArbeit.Shared.Models;
using CoronaKurzArbeit.Shared.Services;
using CoronaKurzArbeit.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace CoronaKurzArbeit.Tests.Services
{
    public class InfoService_GoHome_Tests
    {

        private readonly ApplicationDbContext ctx;
        private readonly KurzarbeitSettingsConfiguration config;
        private readonly NullLogger<TimeBookingsService> tbslogger;

        public InfoService_GoHome_Tests()
        {
            ctx = DbContextHelper.GetContext("info");
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

            tbslogger = new NullLogger<TimeBookingsService>();
            
        }
                
        [Fact]
        public async Task NoGoHomeNoIn()
        {
            var theDay = new DateTime(2020, 4, 15);
            var (awt, twt) = prepare(theDay);

            var sut = new InfoService2(awt, twt, config);
            
            var res = await sut.GoHomeAsync(theDay);

            res.Should().Be(DateTime.MinValue);
        }

        #region NoCorona Only In
        [Fact]
        public async Task GoHomeNoCoronaFullDayOnlyIn()
        {
            var theDay = new DateTime(2020, 4, 15);

            ctx.TimeBookings.RemoveRange();
            await ctx.SaveChangesAsync();
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(3)
            });
            await ctx.SaveChangesAsync();

            var (awt, twt) = prepare(theDay);

            var sut = new InfoService2(awt, twt, config);

            var res = await sut.GoHomeAsync(theDay);

            res.Should().Be(theDay.AddHours(14).AddMinutes(45));
        }

        [Fact]
        public async Task GoHomeNoCoronaFridayDayOnlyIn()
        {
            var theDay = new DateTime(2020, 4, 17);

            ctx.TimeBookings.RemoveRange();
            await ctx.SaveChangesAsync();
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(3)
            });
            await ctx.SaveChangesAsync();

            var (awt, twt) = prepare(theDay);

            var sut = new InfoService2(awt, twt, config);

            var res = await sut.GoHomeAsync(theDay);

            res.Should().Be(theDay.AddHours(11).AddMinutes(45));
        }
        #endregion

        #region Corona 80 Only In
        [Fact]
        public async Task GoHomeCoronaFullDay80()
        {
            var theDay = new DateTime(2020, 4, 20);

            ctx.TimeBookings.RemoveRange();
            await ctx.SaveChangesAsync();
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(3)
            });
            await ctx.SaveChangesAsync();

            var (awt, twt) = prepare(theDay);

            var sut = new InfoService2(awt, twt, config);

            var res = await sut.GoHomeAsync(theDay);

            res.Should().Be(theDay.AddHours(14).AddMinutes(15));
        }

        [Fact]
        public async Task GoHomeCoronaHolidayFriday80()
        {
            var theDay = new DateTime(2020, 4, 30);

            ctx.TimeBookings.RemoveRange();
            await ctx.SaveChangesAsync();
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(3)
            });
            await ctx.SaveChangesAsync();

            var (awt, twt) = prepare(theDay);

            var sut = new InfoService2(awt, twt, config);

            var res = await sut.GoHomeAsync(theDay);

            res.Should().Be(theDay.AddHours(12).AddMinutes(49.5));
        }

        [Fact]
        public async Task GoHomeCoronaHolidayThursday80()
        {
            var theDay = new DateTime(2020, 5, 20);

            ctx.TimeBookings.RemoveRange();
            await ctx.SaveChangesAsync();
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(3)
            });
            await ctx.SaveChangesAsync();

            var (awt, twt) = prepare(theDay);

            var sut = new InfoService2(awt, twt, config);

            var res = await sut.GoHomeAsync(theDay);

            res.Should().Be(theDay.AddHours(11).AddMinutes(41));
        }
        #endregion

        #region Corona 70 Only In
        [Fact]
        public async Task GoHomeCoronaFullDay70()
        {
            var theDay = new DateTime(2020, 6, 16);

            ctx.TimeBookings.RemoveRange();
            await ctx.SaveChangesAsync();
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(3)
            });
            await ctx.SaveChangesAsync();

            var (awt, twt) = prepare(theDay);

            var sut = new InfoService2(awt, twt, config);

            var res = await sut.GoHomeAsync(theDay);

            res.Should().Be(theDay.AddHours(13).AddMinutes(17.25));
        }

        [Fact]
        public async Task GoHomeCoronaHolidayMonday70()
        {
            var theDay = new DateTime(2020, 6, 2);

            ctx.TimeBookings.RemoveRange();
            await ctx.SaveChangesAsync();
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(3)
            });
            await ctx.SaveChangesAsync();

            var (awt, twt) = prepare(theDay);

            var sut = new InfoService2(awt, twt, config);

            var res = await sut.GoHomeAsync(theDay);

            res.Should().Be(theDay.AddHours(12).AddMinutes(48));
        }

        [Fact]
        public async Task GoHomeCoronaHolidayThursday70()
        {
            var theDay = new DateTime(2020, 6, 10);

            ctx.TimeBookings.RemoveRange();
            await ctx.SaveChangesAsync();
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(3)
            });
            await ctx.SaveChangesAsync();

            var (awt, twt) = prepare(theDay);

            var sut = new InfoService2(awt, twt, config);

            var res = await sut.GoHomeAsync(theDay);

            res.Should().Be(theDay.AddHours(10).AddMinutes(24));
        }
        #endregion

        #region InOut
        [Fact]
        public async Task InOut10()
        {
            var theDay = new DateTime(2020, 4, 20);

            ctx.TimeBookings.RemoveRange();
            await ctx.SaveChangesAsync();
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(3)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(13)
            });
            await ctx.SaveChangesAsync();

            var (awt, twt) = prepare(theDay);

            var sut = new InfoService2(awt, twt, config);

            var res = await sut.GoHomeAsync(theDay);

            res.Should().Be(theDay.AddHours(14).AddMinutes(15));
        }

        [Fact]
        public async Task InOut6()
        {
            var theDay = new DateTime(2020, 4, 20);

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

            var (awt, twt) = prepare(theDay);

            var sut = new InfoService2(awt, twt, config);

            var res = await sut.GoHomeAsync(theDay);

            res.Should().Be(theDay.AddHours(14).AddMinutes(15));
        }

        [Fact]
        public async Task InOut9()
        {
            var theDay = new DateTime(2020, 4, 20);

            ctx.TimeBookings.RemoveRange();
            await ctx.SaveChangesAsync();
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(3)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(15).AddMinutes(3)
            });
            await ctx.SaveChangesAsync();

            var (awt, twt) = prepare(theDay);

            var sut = new InfoService2(awt, twt, config);

            var res = await sut.GoHomeAsync(theDay);

            res.Should().Be(theDay.AddHours(14).AddMinutes(15));
        }
        #endregion

        #region InOutIn
        [Fact]
        public async Task InOutIn102()
        {
            var theDay = new DateTime(2020, 4, 20);

            ctx.TimeBookings.RemoveRange();
            await ctx.SaveChangesAsync();
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(3)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(13)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(15)
            });
            await ctx.SaveChangesAsync();

            var (awt, twt) = prepare(theDay);

            var sut = new InfoService2(awt, twt, config);

            var res = await sut.GoHomeAsync(theDay);

            res.Should().Be(theDay.AddHours(14).AddMinutes(15));
        }

        [Fact]
        public async Task InOutIn1010()
        {
            var theDay = new DateTime(2020, 4, 20);

            ctx.TimeBookings.RemoveRange();
            await ctx.SaveChangesAsync();
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(3)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(13)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(23)
            });
            await ctx.SaveChangesAsync();

            var (awt, twt) = prepare(theDay);

            var sut = new InfoService2(awt, twt, config);

            var res = await sut.GoHomeAsync(theDay);

            res.Should().Be(theDay.AddHours(14).AddMinutes(5));
        }

        [Fact]
        public async Task InOutIn1030()
        {
            var theDay = new DateTime(2020, 4, 20);

            ctx.TimeBookings.RemoveRange();
            await ctx.SaveChangesAsync();
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(3)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(13)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(43)
            });
            await ctx.SaveChangesAsync();

            var (awt, twt) = prepare(theDay);

            var sut = new InfoService2(awt, twt, config);

            var res = await sut.GoHomeAsync(theDay);

            res.Should().Be(theDay.AddHours(14).AddMinutes(5));
        }

        [Fact]
        public async Task InOutIn1040()
        {
            var theDay = new DateTime(2020, 4, 20);

            ctx.TimeBookings.RemoveRange();
            await ctx.SaveChangesAsync();
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(3)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(13)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(53)
            });
            await ctx.SaveChangesAsync();

            var (awt, twt) = prepare(theDay);

            var sut = new InfoService2(awt, twt, config);

            var res = await sut.GoHomeAsync(theDay);

            res.Should().Be(theDay.AddHours(14).AddMinutes(15));
        }
        #endregion

        #region InOutInOut
        [Fact]
        public async Task InOutInOut101010()
        {
            var theDay = new DateTime(2020, 4, 20);

            ctx.TimeBookings.RemoveRange();
            await ctx.SaveChangesAsync();
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(3)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(13)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(23)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(33)
            });
            await ctx.SaveChangesAsync();

            var (awt, twt) = prepare(theDay);

            var sut = new InfoService2(awt, twt, config);

            var res = await sut.GoHomeAsync(theDay);

            res.Should().Be(theDay.AddHours(14).AddMinutes(5));
        }

        [Fact]
        public async Task InOutInOutTooMuch()
        {
            var theDay = new DateTime(2020, 4, 20);

            ctx.TimeBookings.RemoveRange();
            await ctx.SaveChangesAsync();
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(3)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(13)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(23)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(16).AddMinutes(23)
            });
            await ctx.SaveChangesAsync();

            var (awt, twt) = prepare(theDay);

            var sut = new InfoService2(awt, twt, config);

            var res = await sut.GoHomeAsync(theDay);

            res.Should().Be(theDay.AddHours(14).AddMinutes(5));
        }
        #endregion

        private (ActualWorkTimeService awt, TargetWorkTimeService twt) prepare(DateTime theDay)
        {
            var timeProvider = new FakeDateTimeProvider(theDay);

            var fs = new FeiertagService(timeProvider);
            var tbs = new TimeBookingsService(tbslogger, ctx, config, fs);
            var cs = new CoronaService(config, tbs);
            var awt = new ActualWorkTimeService(tbs, timeProvider);

            var twt = new TargetWorkTimeService(config, cs, tbs);

            return (awt, twt);
        } 
    }
}
