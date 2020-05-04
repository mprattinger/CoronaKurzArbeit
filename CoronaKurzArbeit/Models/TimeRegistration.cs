using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoronaKurzArbeit.Models
{
    public enum InOutType
    {
        UNKNOWN,
        IN,
        OUT,
        PIN,
        POUT
    }

    public class TimeRegistration
    {
        public int Id { get; set; }
        public DateTime RegistrationMoment { get; set; }
        public InOutType Type { get; set; }

    }
}
