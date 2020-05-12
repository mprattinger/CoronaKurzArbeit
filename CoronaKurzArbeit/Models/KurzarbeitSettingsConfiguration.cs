using System;
using System.Collections.Generic;

namespace CoronaKurzArbeit.Models
{
    public class KurzarbeitSettingsConfiguration
    {
        public DateTime Started { get; set; }
        public double SollArbeitsZeit { get; set; }
        public double CoronaSoll { get; set; }
        public double Monday { get; set; }
        public double Tuesday { get; set; }
        public double Wednesday { get; set; }
        public double Thursday { get; set; }
        public double Friday { get; set; }
        public List<DayOfWeek> CoronaDays { get; set; } = new List<DayOfWeek>();
    }
}
