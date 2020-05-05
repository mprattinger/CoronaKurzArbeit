using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoronaKurzArbeit.Pages
{
    public partial class DayDetail
    {
        [Parameter]
        public string TheDay { get; set; } = string.Empty;

        public DateTime CurrentDay { get; set; }

        protected override void OnInitialized()
        {
            CurrentDay = DateTime.Parse(TheDay);
        }

    }
}
