using CoronaKurzArbeit.DAL.DataAccessSQL;
using CoronaKurzArbeit.Logic.Services;
using CoronaKurzArbeit.Shared.Models;
using CoronaKurzArbeit.Shared.Services;
using CoronaKurzArbeit.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using Xunit;

namespace CoronaKurzArbeit.Tests.Services
{
    public class TargetWorkTimeTests
    {
        private readonly NullLogger<TimeBookingsService> logger;
        private readonly ApplicationDbContext ctx;
        private readonly KurzarbeitSettingsConfiguration config;
        private readonly IFeiertagService fService;
        private readonly ITimeBookingsService tbs;

        public TargetWorkTimeTests()
        {
            logger = new NullLogger<TimeBookingsService>();
            ctx = DbContextHelper.GetContext("cst");

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
            tbs = new TimeBookingsService(logger, ctx, config, fService);
        }

        #region Day
        [Fact]
        public void NormalDayNoCorona()
        {
            var theDate = new DateTime(2020, 04, 15);
            var fService = new FeiertagService(new FakeDateTimeProvider(theDate));
            var corona = new CoronaService(config, fService, tbs);

            var sut = new TargetWorkTimeService(config, corona, tbs);

            var (plannedWorkTime, coronaDelta, targetWorkTime, targetPause) = sut.LoadData(theDate);

            plannedWorkTime.TotalHours.Should().Be(8.2);
            coronaDelta.TotalHours.Should().Be(0);
            targetWorkTime.TotalHours.Should().Be(8.2);
            targetPause.TotalMinutes.Should().Be(30);
        }

        [Fact]
        public void FridayDayNoCorona()
        {
            var theDate = new DateTime(2020, 04, 17);
            var fService = new FeiertagService(new FakeDateTimeProvider(theDate));
            var corona = new CoronaService(config, fService, tbs);

            var sut = new TargetWorkTimeService(config, corona, tbs);

            var (plannedWorkTime, coronaDelta, targetWorkTime, targetPause) = sut.LoadData(theDate);

            plannedWorkTime.TotalHours.Should().Be(5.7);
            coronaDelta.TotalHours.Should().Be(0);
            targetWorkTime.TotalHours.Should().Be(5.7);
            targetPause.TotalMinutes.Should().Be(0);
        }

        [Fact]
        public void NormalDay20()
        {
            var theDate = new DateTime(2020, 04, 21);
            var fService = new FeiertagService(new FakeDateTimeProvider(theDate));
            var corona = new CoronaService(config, fService, tbs);

            var sut = new TargetWorkTimeService(config, corona, tbs);

            var (plannedWorkTime, coronaDelta, targetWorkTime, targetPause) = sut.LoadData(theDate);

            plannedWorkTime.TotalHours.Should().Be(8.2);
            coronaDelta.TotalHours.Should().Be(0.5);
            targetWorkTime.TotalHours.Should().Be(7.7);
            targetPause.TotalMinutes.Should().Be(30);
        }

        [Fact]
        public void NormalDay202()
        {
            var theDate = new DateTime(2020, 04, 20);
            var fService = new FeiertagService(new FakeDateTimeProvider(theDate));
            var corona = new CoronaService(config, fService, tbs);

            var sut = new TargetWorkTimeService(config, corona, tbs);

            var (plannedWorkTime, coronaDelta, targetWorkTime, targetPause) = sut.LoadData(theDate);

            plannedWorkTime.TotalHours.Should().Be(8.2);
            coronaDelta.TotalHours.Should().Be(0.5);
            targetWorkTime.TotalHours.Should().Be(7.7);
            targetPause.TotalMinutes.Should().Be(30);
        }

        [Fact]
        public void FridayHoliday20()
        {
            var theDate = new DateTime(2020, 04, 28);
            var fService = new FeiertagService(new FakeDateTimeProvider(theDate));
            var corona = new CoronaService(config, fService, tbs);

            var sut = new TargetWorkTimeService(config, corona, tbs);

            var (plannedWorkTime, coronaDelta, targetWorkTime, targetPause) = sut.LoadData(theDate);

            plannedWorkTime.TotalHours.Should().Be(8.2);
            coronaDelta.TotalHours.Should().Be(1.925);
            targetWorkTime.TotalHours.Should().Be(6.275);
            targetPause.TotalMinutes.Should().Be(30);
        }

        [Fact]
        public void ThursdayHoliday20()
        {
            var theDate = new DateTime(2020, 05, 19);
            var fService = new FeiertagService(new FakeDateTimeProvider(theDate));
            var corona = new CoronaService(config, fService, tbs);

            var sut = new TargetWorkTimeService(config, corona, tbs);

            var (plannedWorkTime, coronaDelta, targetWorkTime, targetPause) = sut.LoadData(theDate);

            plannedWorkTime.TotalMinutes.Should().Be(492);
            coronaDelta.TotalMinutes.Should().Be(154);
            targetWorkTime.TotalMinutes.Should().Be(338);
            targetPause.TotalMinutes.Should().Be(0);
        }

        [Fact]
        public void Friday20()
        {
            var theDate = new DateTime(2020, 04, 24);
            var fService = new FeiertagService(new FakeDateTimeProvider(theDate));
            var corona = new CoronaService(config, fService, tbs);

            var sut = new TargetWorkTimeService(config, corona, tbs);

            var (plannedWorkTime, coronaDelta, targetWorkTime, targetPause) = sut.LoadData(theDate);

            plannedWorkTime.TotalMinutes.Should().Be(342);
            coronaDelta.TotalMinutes.Should().Be(342);
            targetWorkTime.TotalMinutes.Should().Be(0);
            targetPause.TotalMinutes.Should().Be(0);
        }

        [Fact]
        public void NormalDay30()
        {
            var theDate = new DateTime(2020, 06, 16);
            var fService = new FeiertagService(new FakeDateTimeProvider(theDate));
            var corona = new CoronaService(config, fService, tbs);

            var sut = new TargetWorkTimeService(config, corona, tbs);

            var (plannedWorkTime, coronaDelta, targetWorkTime, targetPause) = sut.LoadData(theDate);

            plannedWorkTime.TotalMinutes.Should().Be(492);
            coronaDelta.TotalMinutes.Should().Be(87.75);
            targetWorkTime.TotalMinutes.Should().Be(404.25);
            targetPause.TotalMinutes.Should().Be(30);
        }

        [Fact]
        public void MondayHoliday30()
        {
            var theDate = new DateTime(2020, 06, 2);
            var fService = new FeiertagService(new FakeDateTimeProvider(theDate));
            var corona = new CoronaService(config, fService, tbs);

            var sut = new TargetWorkTimeService(config, corona, tbs);

            var (plannedWorkTime, coronaDelta, targetWorkTime, targetPause) = sut.LoadData(theDate);

            plannedWorkTime.TotalMinutes.Should().Be(492);
            coronaDelta.TotalMinutes.Should().Be(117);
            targetWorkTime.TotalMinutes.Should().Be(375);
            targetPause.TotalMinutes.Should().Be(30);
        }

        [Fact]
        public void ThursdayHoliday30()
        {
            var theDate = new DateTime(2020, 06, 9);
            var fService = new FeiertagService(new FakeDateTimeProvider(theDate));
            var corona = new CoronaService(config, fService, tbs);

            var sut = new TargetWorkTimeService(config, corona, tbs);

            var (plannedWorkTime, coronaDelta, targetWorkTime, targetPause) = sut.LoadData(theDate);

            plannedWorkTime.TotalMinutes.Should().Be(492);
            coronaDelta.TotalMinutes.Should().Be(231);
            targetWorkTime.TotalMinutes.Should().Be(261);
            targetPause.TotalMinutes.Should().Be(0);
        }

        [Fact]
        public void Friday30()
        {
            var theDate = new DateTime(2020, 04, 24);
            var fService = new FeiertagService(new FakeDateTimeProvider(theDate));
            var corona = new CoronaService(config, fService, tbs);

            var sut = new TargetWorkTimeService(config, corona, tbs);

            var (plannedWorkTime, coronaDelta, targetWorkTime, targetPause) = sut.LoadData(theDate);

            plannedWorkTime.TotalMinutes.Should().Be(342);
            coronaDelta.TotalMinutes.Should().Be(342);
            targetWorkTime.TotalMinutes.Should().Be(0);
            targetPause.TotalMinutes.Should().Be(0);
        }
        #endregion

        #region Week
        [Fact]
        public void NormalWeekNoCorona()
        {
            var start = new DateTime(2020, 03, 30);
            var end = new DateTime(2020, 04, 5);
            var fService = new FeiertagService(new FakeDateTimeProvider(start)); //Für das Feiertagsservice brauchen wir e nur das Jahr aus dem DateTimeProvider
            var corona = new CoronaService(config, fService, tbs);

            var sut = new TargetWorkTimeService(config, corona, tbs);

            var res = sut.LoadData(start, end);

            res.plannedWorkTime.Should().Be(TimeSpan.FromHours(38.5));
            res.coronaDelta.Should().Be(TimeSpan.Zero);
            res.targetWorkTime.Should().Be(TimeSpan.FromHours(38.5));
            res.targetPause.Should().Be(TimeSpan.FromMinutes(120));
        }

        [Fact]
        public void NormalWeekCorona20()
        {
            var start = new DateTime(2020, 04, 20);
            var end = new DateTime(2020, 04, 26);
            var fService = new FeiertagService(new FakeDateTimeProvider(start)); //Für das Feiertagsservice brauchen wir e nur das Jahr aus dem DateTimeProvider
            var corona = new CoronaService(config, fService, tbs);

            var sut = new TargetWorkTimeService(config, corona, tbs);

            var res = sut.LoadData(start, end);

            res.plannedWorkTime.Should().Be(TimeSpan.FromHours(38.5));
            res.coronaDelta.Should().Be(TimeSpan.FromMinutes(462));
            res.targetWorkTime.Should().Be(TimeSpan.FromMinutes(1848));
            res.targetPause.Should().Be(TimeSpan.FromMinutes(120));
        }

        [Fact]
        public void WeekCorona20DoFt()
        {
            var start = new DateTime(2020, 05, 18);
            var end = new DateTime(2020, 05, 24);
            var fService = new FeiertagService(new FakeDateTimeProvider(start)); //Für das Feiertagsservice brauchen wir e nur das Jahr aus dem DateTimeProvider
            var corona = new CoronaService(config, fService, tbs);

            var sut = new TargetWorkTimeService(config, corona, tbs);

            var res = sut.LoadData(start, end);

            res.plannedWorkTime.Should().Be(TimeSpan.FromHours(24.6));
            res.coronaDelta.Should().Be(TimeSpan.FromMinutes(462));
            res.targetWorkTime.Should().Be(TimeSpan.FromMinutes(1014));
            res.targetPause.Should().Be(TimeSpan.Zero); //Die tägl. AZ ist unter 6h
        }

        [Fact]
        public void NormalWeekCorona30()
        {
            var start = new DateTime(2020, 06, 15);
            var end = new DateTime(2020, 06, 19);
            var fService = new FeiertagService(new FakeDateTimeProvider(start)); //Für das Feiertagsservice brauchen wir e nur das Jahr aus dem DateTimeProvider
            var corona = new CoronaService(config, fService, tbs);

            var sut = new TargetWorkTimeService(config, corona, tbs);

            var res = sut.LoadData(start, end);

            res.plannedWorkTime.Should().Be(TimeSpan.FromHours(38.5));
            res.coronaDelta.Should().Be(TimeSpan.FromMinutes(693));
            res.targetWorkTime.Should().Be(TimeSpan.FromMinutes(1617));
            res.targetPause.Should().Be(TimeSpan.FromMinutes(120));
        }
        #endregion
    }
}
