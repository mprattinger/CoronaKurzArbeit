using CoronaKurzArbeit.Shared.Models;
using CoronaKurzArbeit.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CoronaKurzArbeit.Shared.Extensions
{
    public static class TimeBookingExtensions
    {
        public static List<Tuple<TimeBooking, TimeBooking>> GetPauseCouples(this List<TimeBooking> bookings)
        {
            var ret = new List<Tuple<TimeBooking, TimeBooking>>();

            List<TimeBooking> pause = bookings;

            if (pause.Count % 2 > 0)
            {
                pause = pause.Take(pause.Count - 1).ToList();
            }

            var s = 0;
            while (s <= pause.Count - 2)
            {
                var p = pause.Skip(s).Take(2);
                var couple = new Tuple<TimeBooking, TimeBooking>(p.First(), p.Last());
                ret.Add(couple);
                s += 2;
            }

            return ret;
        }

        //public static decimal GetWorkhours(this DateTime current, KurzarbeitSettingsConfiguration config, IFeiertagService feiertagService)
        //{
        //    var props = config.GetType().GetProperties();
        //    //hier die funktion GetWorkDaysForWeek einfügen und entsprechend verarbeiten
        //    foreach (var p in props)
        //    {
        //        if (p.Name == current.DayOfWeek.ToString())
        //        {
        //            return Convert.ToDecimal(p.GetValue(config) ?? default(decimal));
        //        }
        //    }
        //    return 0;
        //}

        //public static List<(DateTime day, decimal arbeitsZeit, WorkDayType type)> GetWorkDaysForWeek(this DateTime dayInWeek, KurzarbeitSettingsConfiguration config, IFeiertagService feiertagService)
        //{
        //    var workDays = new List<(DateTime day, decimal arbeitsZeit, WorkDayType type)>();

        //    //Arbeitstage ermitteln
        //    var f = dayInWeek.FirstDayOfWeek(DayOfWeek.Monday);
        //    while (f.DayOfWeek != DayOfWeek.Saturday)
        //    {
        //        //Ist der Tag ein Feiertag?
        //        if (feiertagService.IsFeiertag(f))
        //        {
        //            //Ja
        //            workDays.Add((f, f.GetWorkhours(config), WorkDayType.Free));
        //        }
        //        else
        //        {
        //            //Nein
        //            //Ein Fenstertag
        //            if (feiertagService.IsFenstertag(f))
        //            {
        //                //Ja
        //                workDays.Add((f, f.GetWorkhours(config), WorkDayType.Free));
        //            }
        //            else
        //            {
        //                //Nein
        //                //Ist der Tag ein KA Tag?
        //                if (config.CoronaDays.Contains(f.DayOfWeek) && f.Date >= config.Started.Date)
        //                {
        //                    //Ja
        //                    workDays.Add((f, f.GetWorkhours(config), WorkDayType.KAday));
        //                }
        //                else
        //                {
        //                    //Nein
        //                    //-> Arbeitstag
        //                    workDays.Add((f, f.GetWorkhours(config), WorkDayType.Workday));
        //                }
        //            }
        //        }
        //        f = f.AddDays(1);
        //    }

        //    return workDays;
        //}
    }
}
