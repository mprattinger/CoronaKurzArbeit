﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoronaKurzArbeit.Models
{
    public class TimeBooking
    {
        public int Id { get; set; }

        public DateTime BookingTime { get; set; }

        public bool IsPause { get; set; }
    }
}
