using CoronaKurzArbeit.Shared.Extensions;
using CoronaKurzArbeit.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CoronaKurzArbeit.Logic.Services
{
    public interface ITargetWorkTimeService
    {
        (TimeSpan plannedWorkTime, TimeSpan coronaDelta, TimeSpan targetWorkTime, TimeSpan targetPause) LoadData(DateTime theDate);
        (TimeSpan plannedWorkTime, TimeSpan coronaDelta, TimeSpan targetWorkTime, TimeSpan targetPause, List<(TimeSpan plannedWorkTime, TimeSpan coronaDelta, TimeSpan targetWorkTime, TimeSpan targetPause)>) LoadData(DateTime from, DateTime to);
    }

    public class TargetWorkTimeService : ITargetWorkTimeService
    {
        //private readonly ILogger<TargetWorkTimeService> _logger;
        private readonly KurzarbeitSettingsConfiguration _config;
        private readonly ICoronaService _coronaService;
        private readonly ITimeBookingsService _bookingsService;

        //public TimeSpan PlannedWorkTime { get; set; } = TimeSpan.Zero;
        //public TimeSpan CoronaDelta { get; set; } = TimeSpan.Zero;
        //public TimeSpan TargetWorkTime { get; set; } = TimeSpan.Zero;
        //public TimeSpan TargetPause { get; set; } = TimeSpan.Zero;

        public TargetWorkTimeService(
            //ILogger<TargetWorkTimeService> logger,
            KurzarbeitSettingsConfiguration config,
            ICoronaService coronaService,
            ITimeBookingsService bookingsService)
        {
            //_logger = logger;
            _config = config;
            _coronaService = coronaService;
            _bookingsService = bookingsService;
        }

        public (TimeSpan plannedWorkTime, TimeSpan coronaDelta, TimeSpan targetWorkTime, TimeSpan targetPause) LoadData(DateTime theDate)
        {
            var plannedWorkTime = TimeSpan.FromHours(_bookingsService.GetWorkhours(theDate).ToDouble());
            var coronaDelta = TimeSpan.FromHours(_coronaService.KAAusfallPerDay(theDate).ToDouble());
            var targetWorkTime = plannedWorkTime.Subtract(coronaDelta);
            var targetPause = TimeSpan.Zero;
            if (targetWorkTime.TotalHours >= 6)
            {
                targetPause = TimeSpan.FromMinutes(_config.SollPause);
            }
            return (plannedWorkTime, coronaDelta, targetWorkTime, targetPause);
        }

        public (TimeSpan plannedWorkTime, TimeSpan coronaDelta, TimeSpan targetWorkTime, TimeSpan targetPause, List<(TimeSpan plannedWorkTime, TimeSpan coronaDelta, TimeSpan targetWorkTime, TimeSpan targetPause)>) LoadData(DateTime from, DateTime to)
        {
            var ret = new List<(TimeSpan plannedWorkTime, TimeSpan coronaDelta, TimeSpan targetWorkTime, TimeSpan targetPause)>();
            var actual = from.Date;
            while(actual <= to.Date)
            {
                var d = LoadData(actual);
                ret.Add(d);
                actual = actual.AddDays(1);
            }

            var pw = new TimeSpan(ret.Sum(x => x.plannedWorkTime.Ticks));
            var cd = new TimeSpan(ret.Sum(x => x.coronaDelta.Ticks));
            var tw = new TimeSpan(ret.Sum(x => x.targetWorkTime.Ticks));
            var tp = new TimeSpan(ret.Sum(x => x.targetPause.Ticks));

            return (pw, cd, tw, tp, ret);
        }
    }
}
