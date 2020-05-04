using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoronaKurzArbeit.Models.Exceptions
{
    public class InOutException : Exception
    {
        public InOutException(InOutType type) : this(type, null, null)
        {

        }

        public InOutException(InOutType type, string? msg) : this(type, msg, null)
        {
        }

        public InOutException(InOutType type, string? msg, Exception? innerException) : base(msg, innerException)
                {

        }
    }
}
