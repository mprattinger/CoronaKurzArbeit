using CoronaKurzArbeit.DAL.DataAccessSQL;
using CoronaKurzArbeit.Logic.Services;
using CoronaKurzArbeit.Shared.Models;
using CoronaKurzArbeit.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CoronaKurzArbeit.Tests.Services
{
    public class InfoService_Info_Tests
    {

        private readonly ApplicationDbContext ctx;
        private readonly KurzarbeitSettingsConfiguration config;
        private readonly NullLogger<TimeBookingsService> tbslogger;

        public InfoService_Info_Tests()
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
                Monday = 8.2m,
                Tuesday = 8.2m,
                Wednesday = 8.2m,
                Thursday = 8.2m,
                Friday = 5.7m,
                CoronaDays = new List<DayOfWeek> { DayOfWeek.Friday }
            };

            tbslogger = new NullLogger<TimeBookingsService>();
            
        }

        #region AlreadyHome Normal
        [Fact]
        public async Task NormalDayAlreadyHomeOptimalWork()
        {
            var theDay = new DateTime(2020, 4, 15);

            ctx.TimeBookings.RemoveRange();
            await ctx.SaveChangesAsync();
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(3)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(9).AddMinutes(0)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(9).AddMinutes(10)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(11).AddMinutes(0)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(11).AddMinutes(30)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(14).AddMinutes(55)
            });
            await ctx.SaveChangesAsync();

            var (awt, twt) = prepare(theDay);

            var sut = new InfoService2(awt, twt, config);
            
            var res = await sut.LoadInfo(theDay);

            res.Worked.Should().Be(TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(12)));
            res.Pause.Should().Be(TimeSpan.FromMinutes(30));
            res.PauseTargetDiff.Should().Be(TimeSpan.FromMinutes(10));
            res.KuaDiff.Should().Be(TimeSpan.Zero);
            res.VAZ.Should().Be(TimeSpan.Zero);
        }

        [Fact]
        public async Task NormalDayAlreadyHomeVAZ()
        {
            var theDay = new DateTime(2020, 4, 15);

            ctx.TimeBookings.RemoveRange();
            await ctx.SaveChangesAsync();
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(3)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(9).AddMinutes(0)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(9).AddMinutes(10)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(11).AddMinutes(0)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(11).AddMinutes(30)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(15).AddMinutes(5)
            });
            await ctx.SaveChangesAsync();

            var (awt, twt) = prepare(theDay);

            var sut = new InfoService2(awt, twt, config);

            var res = await sut.LoadInfo(theDay);

            res.Worked.Should().Be(TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(22)));
            res.Pause.Should().Be(TimeSpan.FromMinutes(30));
            res.PauseTargetDiff.Should().Be(TimeSpan.FromMinutes(10));
            res.KuaDiff.Should().Be(TimeSpan.Zero);
            res.VAZ.Should().Be(TimeSpan.FromMinutes(10));
        }

        [Fact]
        public async Task NormalDayAlreadyHomeNegVAZ()
        {
            var theDay = new DateTime(2020, 4, 15);

            ctx.TimeBookings.RemoveRange();
            await ctx.SaveChangesAsync();
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(3)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(9).AddMinutes(0)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(9).AddMinutes(10)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(11).AddMinutes(0)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(11).AddMinutes(30)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(14).AddMinutes(45)
            });
            await ctx.SaveChangesAsync();

            var (awt, twt) = prepare(theDay);

            var sut = new InfoService2(awt, twt, config);

            var res = await sut.LoadInfo(theDay);

            res.Worked.Should().Be(TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(2)));
            res.Pause.Should().Be(TimeSpan.FromMinutes(30));
            res.PauseTargetDiff.Should().Be(TimeSpan.FromMinutes(10));
            res.KuaDiff.Should().Be(TimeSpan.Zero);
            res.VAZ.Should().Be(TimeSpan.FromMinutes(-10));
        }

        [Fact]
        public async Task NormalDayAlreadyHomePauseLess30()
        {
            var theDay = new DateTime(2020, 4, 15);

            ctx.TimeBookings.RemoveRange();
            await ctx.SaveChangesAsync();
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(3)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(11).AddMinutes(0)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(11).AddMinutes(20)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(14).AddMinutes(25)
            });
            await ctx.SaveChangesAsync();

            var (awt, twt) = prepare(theDay);

            var sut = new InfoService2(awt, twt, config);

            var res = await sut.LoadInfo(theDay);

            res.Worked.Should().Be(TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(2)));
            res.Pause.Should().Be(TimeSpan.FromMinutes(10));
            res.PauseTargetDiff.Should().Be(TimeSpan.FromMinutes(-10));
            res.KuaDiff.Should().Be(TimeSpan.Zero);
            res.VAZ.Should().Be(TimeSpan.FromMinutes(-10));
        }
        #endregion

        #region AlreadyHome Kua20
        [Fact]
        public async Task Kua20AlreadyHomeOptimalWork()
        {
            var theDay = new DateTime(2020, 4, 21);

            ctx.TimeBookings.RemoveRange();
            await ctx.SaveChangesAsync();
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(3)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(9).AddMinutes(0)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(9).AddMinutes(10)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(11).AddMinutes(0)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(11).AddMinutes(30)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(14).AddMinutes(25)
            });
            await ctx.SaveChangesAsync();

            var (awt, twt) = prepare(theDay);

            var sut = new InfoService2(awt, twt, config);

            var res = await sut.LoadInfo(theDay);

            res.Worked.Should().Be(TimeSpan.FromHours(7).Add(TimeSpan.FromMinutes(42)));
            res.Pause.Should().Be(TimeSpan.FromMinutes(30));
            res.PauseTargetDiff.Should().Be(TimeSpan.FromMinutes(10));
            res.KuaDiff.Should().Be(TimeSpan.FromMinutes(30));
            res.VAZ.Should().Be(TimeSpan.Zero);
        }

        [Fact]
        public async Task Kua20AlreadyHomeVAZ()
        {
            var theDay = new DateTime(2020, 4, 21);

            ctx.TimeBookings.RemoveRange();
            await ctx.SaveChangesAsync();
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(3)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(9).AddMinutes(0)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(9).AddMinutes(10)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(11).AddMinutes(0)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(11).AddMinutes(30)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(15).AddMinutes(5)
            });
            await ctx.SaveChangesAsync();

            var (awt, twt) = prepare(theDay);

            var sut = new InfoService2(awt, twt, config);

            var res = await sut.LoadInfo(theDay);

            res.Worked.Should().Be(TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(22)));
            res.Pause.Should().Be(TimeSpan.FromMinutes(30));
            res.PauseTargetDiff.Should().Be(TimeSpan.FromMinutes(10));
            res.KuaDiff.Should().Be(TimeSpan.Zero);
            res.VAZ.Should().Be(TimeSpan.FromMinutes(10));
        }

        [Fact]
        public async Task Kua20AlreadyHomeLessKua()
        {
            var theDay = new DateTime(2020, 4, 21);

            ctx.TimeBookings.RemoveRange();
            await ctx.SaveChangesAsync();
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(3)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(9).AddMinutes(0)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(9).AddMinutes(10)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(11).AddMinutes(0)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(11).AddMinutes(30)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(14).AddMinutes(45)
            });
            await ctx.SaveChangesAsync();

            var (awt, twt) = prepare(theDay);

            var sut = new InfoService2(awt, twt, config);

            var res = await sut.LoadInfo(theDay);

            res.Worked.Should().Be(TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(2)));
            res.Pause.Should().Be(TimeSpan.FromMinutes(30));
            res.PauseTargetDiff.Should().Be(TimeSpan.FromMinutes(10));
            res.KuaDiff.Should().Be(TimeSpan.Zero);
            res.VAZ.Should().Be(TimeSpan.FromMinutes(-10));
        }

        [Fact]
        public async Task Kua20AlreadyHomePauseLess30()
        {
            var theDay = new DateTime(2020, 4, 21);

            ctx.TimeBookings.RemoveRange();
            await ctx.SaveChangesAsync();
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(3)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(11).AddMinutes(0)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(11).AddMinutes(20)
            });
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(14).AddMinutes(25)
            });
            await ctx.SaveChangesAsync();

            var (awt, twt) = prepare(theDay);

            var sut = new InfoService2(awt, twt, config);

            var res = await sut.LoadInfo(theDay);

            res.Worked.Should().Be(TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(2)));
            res.Pause.Should().Be(TimeSpan.FromMinutes(10));
            res.PauseTargetDiff.Should().Be(TimeSpan.FromMinutes(-10));
            res.KuaDiff.Should().Be(TimeSpan.Zero);
            res.VAZ.Should().Be(TimeSpan.FromMinutes(-10));
        }
        #endregion


        private (ActualWorkTimeService awt, TargetWorkTimeService twt) prepare(DateTime theDay)
        {
            var timeProvider = new FakeDateTimeProvider(theDay);

            var fs = new FeiertagService(timeProvider);
            var cs = new CoronaService(config, fs);
            var tbs = new TimeBookingsService(tbslogger, ctx, timeProvider, config, cs);
            var awt = new ActualWorkTimeService(tbs);

            var twt = new TargetWorkTimeService(config, cs);

            return (awt, twt);
        } 
    }
}
