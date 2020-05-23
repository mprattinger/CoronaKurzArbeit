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

        public decimal SollArbeitszeit { get; set; } = 0m;

        public decimal KAAusfall { get; set; } = 0m;

        public decimal Tagesarbeitszeit { get; set; } = 0m;

        public decimal IstArbeitszeitBrutto { get; set; } = 0m;

        public decimal IstArbeitszeit { get; set; } = 0m;

        public decimal KuaZeit { get; set; } = 0m;

        public decimal VAZeit { get; set; } = 0m;

        protected override void OnInitialized()
        {
            AppState.OnCurrentDayChanged += appState_OnCurrentDayChanged;
            AppState.OnRegistered += appState_OnRegistered;
        }

        private async Task appState_OnCurrentDayChanged(DateTime arg)
        {
            await InvokeAsync(async () =>
            {
                TheDate = arg;
                await calculateInfo();
                await AppState.InfoLoadedFinishedAsync(arg);
                StateHasChanged();
            });
        }

        private async Task appState_OnRegistered(TimeBooking arg)
        {
            await InvokeAsync(async () =>
            {
                TheDate = arg.BookingTime.Date;
                await calculateInfo();
                await AppState.InfoLoadedFinishedAsync(arg.BookingTime);
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
                var istArbeitsZeit = await BookingsService.GetNetWorkingTimeForDayAsync(TheDate);
                IstArbeitszeit = Convert.ToDecimal(istArbeitsZeit.TotalHours);
                if (KAAusfall > 0)
                {
                    KuaZeit = SollArbeitszeit - IstArbeitszeit;
                    if(KuaZeit < 0)
                    {
                        VAZeit = KuaZeit * -1;
                        KuaZeit = 0;
                    }
                }
            }
        }

        public void Dispose()
        {
            AppState.OnCurrentDayChanged -= appState_OnCurrentDayChanged;
            AppState.OnRegistered -= appState_OnRegistered;
        }
    }
}
