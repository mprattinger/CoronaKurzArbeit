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
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1063:Implement IDisposable Correctly", Justification = "<Pending>")]
    public partial class Info : IDisposable
    {
        [Inject]
        public IAppState AppState { get; set; } = default!;

        [Inject]
        public KurzarbeitSettingsConfiguration KAConfig { get; set; } = default!;

        [Inject]
        public ICoronaService CoronaService { get; set; } = default!;

        [Inject]
        public ITimeBookingsService BookingsService { get; set; } = default!;

        public DateTime TheDate { get; set; } = DateTime.MinValue;

        public decimal SollArbeitszeit { get; set; }

        public decimal KAAusfall { get; set; }

        public decimal Tagesarbeitszeit { get; set; }

        public decimal IstArbeitszeitBrutto { get; set; }

        public decimal IstArbeitszeit { get; set; }

        protected override void OnInitialized()
        {
            AppState.OnCurrentDayChanged += appState_OnCurrentDayChanged;
        }

        private async Task appState_OnCurrentDayChanged(DateTime arg)
        {
            await InvokeAsync(async () =>
            {
                TheDate = arg;
                await calculateInfo();
                StateHasChanged();
            });
        }

        private async Task calculateInfo()
        {
            if (TheDate > DateTime.MinValue)
            {
                SollArbeitszeit = TheDate.GetWorkhours(KAConfig);
                KAAusfall = CoronaService.KAAusfallPerDay(TheDate);
                Tagesarbeitszeit = SollArbeitszeit - KAAusfall;
                var (_, _, grossWorkTime) = await BookingsService.GetGrossWorkTimeForDayAsync(TheDate);
                IstArbeitszeitBrutto = Convert.ToDecimal(grossWorkTime.TotalHours);
            }
        }

        public void Dispose()
        {
            AppState.OnCurrentDayChanged -= appState_OnCurrentDayChanged;
        }
    }
}
