using CoronaKurzArbeit.Services;
using Microsoft.AspNetCore.Components;
using System;

namespace CoronaKurzArbeit.Components
{
    public partial class DayTimeList
    {
        [Inject]
        protected TimeRegistrationService TimeRegistrationService { get; set; }

        [Parameter]
        public DateTime TheDay { get; set; }

    }
}
