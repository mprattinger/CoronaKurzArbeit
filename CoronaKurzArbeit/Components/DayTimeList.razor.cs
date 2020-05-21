using CoronaKurzArbeit.Services;
using Microsoft.AspNetCore.Components;
using System;

namespace CoronaKurzArbeit.Components
{
    public partial class DayTimeList
    {

        [Parameter]
        public DateTime TheDay { get; set; }

    }
}
