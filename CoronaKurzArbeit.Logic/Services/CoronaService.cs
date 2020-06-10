using CoronaKurzArbeit.Shared.Extensions;
using CoronaKurzArbeit.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CoronaKurzArbeit.Logic.Services
{
    public enum WorkDayType {
        Workday,
        KAday,
        Free
    }

    public interface ICoronaService
    {
        decimal GetKATime(DateTime value);
        List<(DateTime day, decimal arbeitsZeit, WorkDayType type)> GetWorkDaysForWeek(DateTime dayInWeek);
        decimal KAAusfallPerDay(DateTime value);
    }

    public class CoronaService : ICoronaService
    {
        private readonly KurzarbeitSettingsConfiguration _config;
        private readonly IFeiertagService _feiertag;

        public CoronaService(KurzarbeitSettingsConfiguration config, IFeiertagService feiertagService)
        {
            _config = config;
            _feiertag = feiertagService;
        }

        public decimal KAAusfallPerDay(DateTime value)
        {
            var wd = GetWorkDaysForWeek(value);
            var coronaSoll = GetKATime(value);
            var kaTageSum = wd.Where(x => x.type == WorkDayType.KAday).Sum(x => x.arbeitsZeit);
            var workDayKa = (coronaSoll - kaTageSum) / wd.Count(x => x.type == WorkDayType.Workday);
            var day = wd.Where(x => x.day.Date == value.Date).FirstOrDefault();
            if (day == default) return default; //Den Tag gibt es nicht!

            if (day.type == WorkDayType.Workday) return workDayKa; //Math.Round(workDayKa, 3); //Normaler Arbeitstag
            if (day.type == WorkDayType.KAday) return day.arbeitsZeit; //Math.Round(day.arbeitsZeit, 3); //Kurzarbeitstag

            return default;

        }

        public List<(DateTime day, decimal arbeitsZeit, WorkDayType type)> GetWorkDaysForWeek(DateTime dayInWeek)
        {
            var workDays = new List<(DateTime day, decimal arbeitsZeit, WorkDayType type)>();

            //Arbeitstage ermitteln
            var f = dayInWeek.FirstDayOfWeek(DayOfWeek.Monday);
            while (f.DayOfWeek != DayOfWeek.Saturday)
            {
                //Ist der Tag ein Feiertag?
                if (_feiertag.IsFeiertag(f))
                {
                    //Ja
                    workDays.Add((f, f.GetWorkhours(_config), WorkDayType.Free));
                }
                else
                {
                    //Nein
                    //Ein Fenstertag
                    if (_feiertag.IsFenstertag(f))
                    {
                        //Ja
                        workDays.Add((f, f.GetWorkhours(_config), WorkDayType.Free));
                    }
                    else
                    {
                        //Nein
                        //Ist der Tag ein KA Tag?
                        if (_config.CoronaDays.Contains(f.DayOfWeek) && f.Date >= _config.Started.Date)
                        {
                            //Ja
                            workDays.Add((f, f.GetWorkhours(_config), WorkDayType.KAday));
                        }
                        else
                        {
                            //Nein
                            //-> Arbeitstag
                            workDays.Add((f, f.GetWorkhours(_config), WorkDayType.Workday));
                        }
                    }
                }
                f = f.AddDays(1);
            }

            return workDays;
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
