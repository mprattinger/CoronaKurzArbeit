using CoronaKurzArbeit.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoronaKurzArbeit.Tests.Helpers
{
    public class FakeDateTimeProvider : IDateTimeProvider
    {
        private DateTime _fakeTime;

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
