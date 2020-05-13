using CoronaKurzArbeit.Extensions;
using CoronaKurzArbeit.Models;
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
        public KurzarbeitSettingsConfiguration KAConfig { get; set; } = new KurzarbeitSettingsConfiguration();

        public decimal SollArbeitszeit { get; set; }

        public decimal CoronaSollArbeitszeit { get; set; }

        protected override void OnInitialized()
        {
            SollArbeitszeit = TheDate.GetWorkhours(KAConfig);
            CoronaSollArbeitszeit = TheDate.GetCoronaWorkhours(KAConfig);
        }
    }
}
