using System;
using System.Collections.Generic;

namespace CoronaKurzArbeit.Shared.Models
{
    public class KurzarbeitSettingsConfiguration
    {
        public DateTime Started { get; set; }
        public decimal SollArbeitsZeit { get; set; }
        public int SollPause { get; set; }
        public int PauseFree { get; set; }
        public List<CoronaAusfall> CoronaSoll { get; set; } = new List<CoronaAusfall>();
        public decimal Monday { get; set; }
        public decimal Tuesday { get; set; }
        public decimal Wednesday { get; set; }
        public decimal Thursday { get; set; }
        public decimal Friday { get; set; }
        public List<DayOfWeek> CoronaDays { get; set; } = new List<DayOfWeek>();
    }

    public class CoronaAusfall
    {
        public DateTime Bis { get; set; }
        public decimal Ausfall { get; set; }
    }
}
