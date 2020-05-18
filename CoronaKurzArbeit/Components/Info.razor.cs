using CoronaKurzArbeit.Extensions;
using CoronaKurzArbeit.Models;
using CoronaKurzArbeit.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoronaKurzArbeit.Components
{
    public partial class Info
    {
        [CascadingParameter]
        public DateTime TheDate { get; set; } = DateTime.MinValue;

        [Inject]
        public KurzarbeitSettingsConfiguration KAConfig { get; set; } = default!;

        [Inject]
        public ICoronaService CoronaService { get; set; } = default!;

        public decimal SollArbeitszeit { get; set; }

        public decimal KAAusfall { get; set; }

        protected override void OnInitialized()
        {
            calculateInfo();
        }

        private void calculateInfo()
        {
            SollArbeitszeit = TheDate.GetWorkhours(KAConfig);
            KAAusfall = CoronaService.KAAusfallPerDay(TheDate);
        }

        protected override void OnAfterRender(bool firstRender)
        {
            //SollArbeitszeit = TheDate.GetWorkhours(KAConfig);
            //KAAusfall = CoronaService.KAAusfallPerDay(TheDate);
            //StateHasChanged();
        }
    }
}
