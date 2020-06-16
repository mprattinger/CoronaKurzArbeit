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
            res.KuaActual.Should().Be(TimeSpan.Zero);
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
            res.KuaActual.Should().Be(TimeSpan.Zero);
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
            res.KuaActual.Should().Be(TimeSpan.Zero);
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
            res.KuaActual.Should().Be(TimeSpan.Zero);
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
            res.KuaActual.Should().Be(TimeSpan.FromMinutes(30));
            res.KuaDiff.Should().Be(TimeSpan.Zero);
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
            res.KuaActual.Should().Be(TimeSpan.Zero);
            res.KuaDiff.Should().Be(TimeSpan.FromMinutes(-30));
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
            res.KuaActual.Should().Be(TimeSpan.FromMinutes(10));
            res.KuaDiff.Should().Be(TimeSpan.FromMinutes(-20));
            res.VAZ.Should().Be(TimeSpan.Zero);
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
            res.KuaActual.Should().Be(TimeSpan.FromMinutes(10));
            res.KuaDiff.Should().Be(TimeSpan.FromMinutes(-20));
            res.VAZ.Should().Be(TimeSpan.Zero);
        }
        #endregion

        #region AlreadyHome Kua30
        [Fact]
        public async Task Kua30AlreadyHomeOptimalWork()
        {
            var theDay = new DateTime(2020, 6, 22);

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
                BookingTime = theDay.AddHours(13).AddMinutes(27)
            });
            await ctx.SaveChangesAsync();

            var (awt, twt) = prepare(theDay);

            var sut = new InfoService2(awt, twt, config);

            var res = await sut.LoadInfo(theDay);

            res.Worked.Should().Be(TimeSpan.FromHours(6).Add(TimeSpan.FromMinutes(44)));
            res.Pause.Should().Be(TimeSpan.FromMinutes(30));
            res.PauseTargetDiff.Should().Be(TimeSpan.FromMinutes(10));
            res.KuaActual.Should().Be(TimeSpan.FromMinutes(88));
            res.KuaDiff.Should().Be(TimeSpan.FromMinutes(0.25));
            res.VAZ.Should().Be(TimeSpan.Zero);
        }

        [Fact]
        public async Task Kua30AlreadyHomeVAZ()
        {
            var theDay = new DateTime(2020, 6, 22);

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
            res.KuaActual.Should().Be(TimeSpan.Zero);
            res.KuaDiff.Should().Be(TimeSpan.FromMinutes(-87.75));
            res.VAZ.Should().Be(TimeSpan.FromMinutes(10));
        }

        [Fact]
        public async Task Kua30AlreadyHomeLessKua()
        {
            var theDay = new DateTime(2020, 6, 22);

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
            res.KuaActual.Should().Be(TimeSpan.FromMinutes(10));
            res.KuaDiff.Should().Be(TimeSpan.FromMinutes(-77.75));
            res.VAZ.Should().Be(TimeSpan.Zero);
        }

        [Fact]
        public async Task Kua30AlreadyHomePauseLess30()
        {
            var theDay = new DateTime(2020, 6, 22);

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
            res.KuaActual.Should().Be(TimeSpan.FromMinutes(10));
            res.KuaDiff.Should().Be(TimeSpan.FromMinutes(-77.75));
            res.VAZ.Should().Be(TimeSpan.Zero);
        }
        #endregion

        #region StillWorking Normal
        [Fact]
        public async Task WorkingForNoon_Normal()
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
            await ctx.SaveChangesAsync();

            theDay = theDay.AddHours(10);
            var (awt, twt) = prepare(theDay);

            var sut = new InfoService2(awt, twt, config);

            var res = await sut.LoadInfo(theDay);

            res.Worked.Should().Be(TimeSpan.FromHours(3).Add(TimeSpan.FromMinutes(47)));
            res.Pause.Should().Be(TimeSpan.FromMinutes(0));
            res.PauseTargetDiff.Should().Be(TimeSpan.FromMinutes(-20));
            res.KuaActual.Should().Be(TimeSpan.Zero);
            res.VAZ.Should().Be(TimeSpan.FromMinutes(-265));
        }

        [Fact]
        public async Task WorkingForNoon_Normal_InPause()
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
            await ctx.SaveChangesAsync();

            theDay = theDay.AddHours(10);
            var (awt, twt) = prepare(theDay);

            var sut = new InfoService2(awt, twt, config);

            var res = await sut.LoadInfo(theDay);

            res.Worked.Should().Be(TimeSpan.FromHours(2).Add(TimeSpan.FromMinutes(57)));
            res.Pause.Should().Be(TimeSpan.Zero);
            res.PauseTargetDiff.Should().Be(TimeSpan.FromMinutes(-30));
            res.KuaActual.Should().Be(TimeSpan.Zero);
            res.VAZ.Should().Be(TimeSpan.FromMinutes(-315));

        }
        #endregion

        #region StillWorking Kua20
        [Fact]
        public async Task WorkingForNoon_Kua20()
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
            await ctx.SaveChangesAsync();

            theDay = theDay.AddHours(10);
            var (awt, twt) = prepare(theDay);

            var sut = new InfoService2(awt, twt, config);

            var res = await sut.LoadInfo(theDay);

            res.Worked.Should().Be(TimeSpan.FromHours(3).Add(TimeSpan.FromMinutes(47)));
            res.Pause.Should().Be(TimeSpan.FromMinutes(0));
            res.PauseTargetDiff.Should().Be(TimeSpan.FromMinutes(-20));
            res.KuaActual.Should().Be(TimeSpan.FromMinutes(265));
            res.KuaDiff.Should().Be(TimeSpan.FromMinutes(235));
            res.VAZ.Should().Be(TimeSpan.Zero);
        }

        [Fact]
        public async Task WorkingForNoon_Kua20_InPause()
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
            await ctx.SaveChangesAsync();

            theDay = theDay.AddHours(10);
            var (awt, twt) = prepare(theDay);

            var sut = new InfoService2(awt, twt, config);

            var res = await sut.LoadInfo(theDay);

            res.Worked.Should().Be(TimeSpan.FromHours(2).Add(TimeSpan.FromMinutes(57)));
            res.Pause.Should().Be(TimeSpan.Zero);
            res.PauseTargetDiff.Should().Be(TimeSpan.FromMinutes(-30));
            res.KuaActual.Should().Be(TimeSpan.FromMinutes(315));
            res.KuaDiff.Should().Be(TimeSpan.FromMinutes(285));
            res.VAZ.Should().Be(TimeSpan.Zero);

        }
        #endregion

        #region StillWorking Kua30
        [Fact]
        public async Task WorkingForNoon_Kua30()
        {
            var theDay = new DateTime(2020, 6, 22);

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
            await ctx.SaveChangesAsync();

            theDay = theDay.AddHours(10);
            var (awt, twt) = prepare(theDay);

            var sut = new InfoService2(awt, twt, config);

            var res = await sut.LoadInfo(theDay);

            res.Worked.Should().Be(TimeSpan.FromHours(3).Add(TimeSpan.FromMinutes(47)));
            res.Pause.Should().Be(TimeSpan.FromMinutes(0));
            res.PauseTargetDiff.Should().Be(TimeSpan.FromMinutes(-20));
            res.KuaActual.Should().Be(TimeSpan.FromMinutes(265));
            res.KuaDiff.Should().Be(TimeSpan.FromMinutes(177.25));
            res.VAZ.Should().Be(TimeSpan.Zero);
        }

        [Fact]
        public async Task WorkingForNoon_Kua30_InPause()
        {
            var theDay = new DateTime(2020, 6, 22);

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
            await ctx.SaveChangesAsync();

            theDay = theDay.AddHours(10);
            var (awt, twt) = prepare(theDay);

            var sut = new InfoService2(awt, twt, config);

            var res = await sut.LoadInfo(theDay);

            res.Worked.Should().Be(TimeSpan.FromHours(2).Add(TimeSpan.FromMinutes(57)));
            res.Pause.Should().Be(TimeSpan.Zero);
            res.PauseTargetDiff.Should().Be(TimeSpan.FromMinutes(-30));
            res.KuaActual.Should().Be(TimeSpan.FromMinutes(315));
            res.KuaDiff.Should().Be(TimeSpan.FromMinutes(227.25));
            res.VAZ.Should().Be(TimeSpan.Zero);

        }
        #endregion

        private (ActualWorkTimeService awt, TargetWorkTimeService twt) prepare(DateTime theDay)
        {
            var timeProvider = new FakeDateTimeProvider(theDay);

            var fs = new FeiertagService(timeProvider);
            var cs = new CoronaService(config, fs);
            var tbs = new TimeBookingsService(tbslogger, ctx);
            var awt = new ActualWorkTimeService(tbs, timeProvider);

            var twt = new TargetWorkTimeService(config, cs);

            return (awt, twt);
        }
    }
}
