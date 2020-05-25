using System;
using System.Collections.Generic;

namespace CoronaKurzArbeit.Shared.Models
{
    public class KurzarbeitSettingsConfiguration
    {
        public DateTime Started { get; set; }
        public decimal SollArbeitsZeit { get; set; }
        public int PauseFree { get; set; }
        public decimal CoronaSoll { get; set; }
        public decimal Monday { get; set; }
        public decimal Tuesday { get; set; }
        public decimal Wednesday { get; set; }
        public decimal Thursday { get; set; }
        public decimal Friday { get; set; }
        public List<DayOfWeek> CoronaDays { get; set; } = new List<DayOfWeek>();
    }
}
