using CoronaKurzArbeit.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}
