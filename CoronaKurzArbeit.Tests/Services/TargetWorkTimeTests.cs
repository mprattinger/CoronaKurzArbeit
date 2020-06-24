using CoronaKurzArbeit.Logic.Services;
using CoronaKurzArbeit.Shared.Models;
using CoronaKurzArbeit.Tests.Helpers;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace CoronaKurzArbeit.Tests.Services
{
    public class TargetWorkTimeTests
    {
        private readonly KurzarbeitSettingsConfiguration config;

        public TargetWorkTimeTests()
        {
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
        }

        [Fact]
        public void NormalDayNoCorona()
        {
            var theDate = new DateTime(2020, 04, 15);
            var fService = new FeiertagService(new FakeDateTimeProvider(theDate));
            var corona = new CoronaService(config, fService);

            var sut = new TargetWorkTimeService(config, corona);

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
            var corona = new CoronaService(config, fService);

            var sut = new TargetWorkTimeService(config, corona);

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
            var corona = new CoronaService(config, fService);

            var sut = new TargetWorkTimeService(config, corona);

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
            var corona = new CoronaService(config, fService);

            var sut = new TargetWorkTimeService(config, corona);

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
            var corona = new CoronaService(config, fService);

            var sut = new TargetWorkTimeService(config, corona);

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
            var corona = new CoronaService(config, fService);

            var sut = new TargetWorkTimeService(config, corona);

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
            var corona = new CoronaService(config, fService);

            var sut = new TargetWorkTimeService(config, corona);

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
            var corona = new CoronaService(config, fService);

            var sut = new TargetWorkTimeService(config, corona);

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
            var corona = new CoronaService(config, fService);

            var sut = new TargetWorkTimeService(config, corona);

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
            var corona = new CoronaService(config, fService);

            var sut = new TargetWorkTimeService(config, corona);

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
            var corona = new CoronaService(config, fService);

            var sut = new TargetWorkTimeService(config, corona);

            var (plannedWorkTime, coronaDelta, targetWorkTime, targetPause) = sut.LoadData(theDate);

            plannedWorkTime.TotalMinutes.Should().Be(342);
            coronaDelta.TotalMinutes.Should().Be(342);
            targetWorkTime.TotalMinutes.Should().Be(0);
            targetPause.TotalMinutes.Should().Be(0);
        }
    }
}
