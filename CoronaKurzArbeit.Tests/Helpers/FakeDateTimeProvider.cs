using CoronaKurzArbeit.Logic.Services;
using System;

namespace CoronaKurzArbeit.Tests.Helpers
{
    public class FakeDateTimeProvider : IDateTimeProvider
    {
        private readonly DateTime _fakeTime;

        public FakeDateTimeProvider(DateTime fakeTime)
        {
            _fakeTime = fakeTime;
        }
        public DateTime GetCurrentTime()
        {
            return _fakeTime;
        }
    }
}
