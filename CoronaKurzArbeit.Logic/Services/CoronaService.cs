using CoronaKurzArbeit.Shared.Extensions;
using CoronaKurzArbeit.Shared.Models;
using CoronaKurzArbeit.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CoronaKurzArbeit.Logic.Services
{   
    public interface ICoronaService
    {
        decimal GetKATime(DateTime value);
        //List<(DateTime day, decimal arbeitsZeit, WorkDayType type)> GetWorkDaysForWeek(DateTime dayInWeek);
        decimal KAAusfallPerDay(DateTime value);
    }

    public class CoronaService : ICoronaService
    {
        private readonly KurzarbeitSettingsConfiguration _config;
        private readonly ITimeBookingsService _bookingsService;

        public CoronaService(KurzarbeitSettingsConfiguration config,
            ITimeBookingsService bookingsService)
        {
            _config = config;
            _bookingsService = bookingsService;
        }

        public decimal KAAusfallPerDay(DateTime value)
        {
            var wd = _bookingsService.GetWorkDaysForWeek(value); //GetWorkDaysForWeek(value);
            var coronaSoll = GetKATime(value);
            var kaTageSum = wd.Where(x => x.type == WorkDayType.KAday).Sum(x => x.arbeitsZeit);
            var workDayKa = (coronaSoll - kaTageSum) / wd.Count(x => x.type == WorkDayType.Workday);
            var day = wd.Where(x => x.day.Date == value.Date).FirstOrDefault();
            if (day == default) return default; //Den Tag gibt es nicht!

            if (day.type == WorkDayType.Workday) return workDayKa; //Math.Round(workDayKa, 3); //Normaler Arbeitstag
            if (day.type == WorkDayType.KAday) return day.arbeitsZeit; //Math.Round(day.arbeitsZeit, 3); //Kurzarbeitstag

            return default;

        }

        public decimal GetKATime(DateTime value)
        {
            if (value.Date < _config.Started.Date)
            {
                //Noch kein Corona
                return 0m;
            }
            var soll = _config.CoronaSoll.Where(x => x.Bis >= value).FirstOrDefault();
            return _config.SollArbeitsZeit * soll.Ausfall;
        }
    }
}
