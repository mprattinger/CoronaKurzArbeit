using CoronaKurzArbeit.Logic.Services;
using CoronaKurzArbeit.Services;
using CoronaKurzArbeit.Shared.Extensions;
using CoronaKurzArbeit.Shared.Models;
using Microsoft.AspNetCore.Components;
using System;
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

        public string IstArbeitszeit { get; set; } = string.Empty;

        public string KuaZeit { get; set; } = string.Empty;

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
                IstArbeitszeit = $"{istArbeitsZeit.Hours}:{istArbeitsZeit.Minutes} ({istArbeitsZeit.TotalHours.ToString("N2")})";
                if (KAAusfall > 0)
                {
                    var sa = TimeSpan.FromHours(Convert.ToDouble(SollArbeitszeit));
                    var kua = sa.Subtract(istArbeitsZeit); // SollArbeitszeit - Convert.ToDecimal(istArbeitsZeit.TotalHours);
                    KuaZeit = $"{kua.Hours}:{kua.Minutes} ({kua.TotalHours.ToString("N2")})";
                    if (kua.TotalHours < 0)
                    {
                        VAZeit = Convert.ToDecimal(kua.TotalHours * -1);
                        KuaZeit = string.Empty;
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
