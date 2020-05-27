using System;
using System.Collections.Generic;
using System.Text;

namespace CoronaKurzArbeit.Shared.Extensions
{
    public static class NumberExtensions
    {
        public static double ToDouble(this decimal value)
        {
            return Convert.ToDouble(value);
        }

        public static decimal ToDecimal(this double value)
        {
            return Convert.ToDecimal(value);
        }
    }
}
