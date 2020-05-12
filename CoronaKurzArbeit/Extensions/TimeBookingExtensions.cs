using CoronaKurzArbeit.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace CoronaKurzArbeit.Extensions
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
                s = s + 2;
            }

            return ret;
        }

        public static double GetWorkhours(this DateTime current, KurzarbeitSettingsConfiguration config)
        {
            var props = config.GetType().GetProperties();
            foreach (var p in props)
            {
                if (p.Name == current.DayOfWeek.ToString())
                {
                    return Convert.ToDouble(p.GetValue(config) ?? default(double));
                }
            }
            return 0;
        }

        public static double GetCoronaWorkhours(this DateTime current, KurzarbeitSettingsConfiguration config)
        {
            var props = config.GetType().GetProperties();
            var wh = double.MinValue;
            foreach (var p in props)
            {
                if (p.Name == current.DayOfWeek.ToString())
                {
                    wh = Convert.ToDouble(p.GetValue(config) ?? default(double));
                }
            }
            if (wh == double.MinValue) return 0;

            return wh * config.CoronaSoll;
        }
    }
}
