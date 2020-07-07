using System;

namespace CoronaKurzArbeit.Shared.Services
{
    public interface IDateTimeProvider
    {
        DateTime GetCurrentTime();
    }

    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTime GetCurrentTime()
        {
            return DateTime.Now;
        }
    }
}
