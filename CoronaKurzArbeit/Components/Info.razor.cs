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

        public double SollArbeitszeit { get; set; } = 0;

        public double KAAusfall { get; set; } = 0;

        public double Tagesarbeitszeit { get; set; } = 0;

        public double IstArbeitszeitBrutto { get; set; } = 0;

        public string IstArbeitszeit { get; set; } = string.Empty;

        public string KuaZeit { get; set; } = string.Empty;

        public double VAZeit { get; set; } = 0;

        public string GoHome { get; set; } = string.Empty;

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
                
                var grossWTime = await BookingsService.GetGrossWorkTimeForDayAsync(TheDate);
                var pauses = await BookingsService.GetPauseForDayAsync(TheDate);
                var istArbeitsZeit = BookingsService.GetNetWorkingTimeForDay(TheDate, grossWTime, pauses);

                IstArbeitszeitBrutto = grossWTime.grossWorkTime.TotalHours;
                IstArbeitszeit = $"{istArbeitsZeit.Hours}:{istArbeitsZeit.Minutes} ({istArbeitsZeit.TotalHours:N2})";
                if (KAAusfall > 0)
                {
                    var sa = TimeSpan.FromHours(Convert.ToDouble(SollArbeitszeit));
                    var kua = sa.Subtract(istArbeitsZeit);
                    KuaZeit = $"{kua.Hours}:{kua.Minutes} ({kua.TotalHours:N2})";
                    if (kua.TotalHours < 0)
                    {
                        VAZeit = kua.TotalHours * -1;
                        KuaZeit = string.Empty;
                    }
                }

                if(TheDate.Date == DateTime.Now.Date 
                    && grossWTime.inBooking != null
                    && grossWTime.outBooking != null )
                {
                    if (grossWTime.grossWorkTime.TotalHours >= 6 && (pauses.grossPauseDuration == TimeSpan.Zero || pauses.grossPauseDuration.TotalMinutes < 30))
                    {
                        //Keine Pause oder pause kleiner 30 Minuten, aber mehr als 6 Stunden
                        var diff = TimeSpan.FromMinutes(30).Subtract(pauses.grossPauseDuration);
                        var p = pauses.netPauseDuration.Add(diff);
                        var target = grossWTime.inBooking?.BookingTime.Add(istArbeitsZeit).Add(p);
                        GoHome = $"{target?.Hour}:{target?.Minute}";
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
