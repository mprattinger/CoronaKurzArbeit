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
        public IInfoService2 InfoService { get; set; } = default!;

        //[Inject]
        //public KurzarbeitSettingsConfiguration KAConfig { get; set; } = default!;

        //[Inject]
        //public ITargetWorkTimeService Target { get; set; } = default!;

        //[Inject]
        //public IActualWorkTimeService Actual { get; set; } = default!;

        public DateTime TheDate { get; set; } = DateTime.MinValue;

        public string SollArbeitszeit { get; set; } = string.Empty;

        public string KAAusfall { get; set; } = string.Empty;

        public string Tagesarbeitszeit { get; set; } = string.Empty;

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
                //var (plannedWorkTime, coronaDelta, targetWorkTime, targetPause) = Target.LoadData(TheDate);
                //SollArbeitszeit = plannedWorkTime.NiceTimespan();
                //KAAusfall = coronaDelta.NiceTimespan();
                //Tagesarbeitszeit = targetWorkTime.NiceTimespan();

                //var actual = await Actual.LoadDataAsync(TheDate);
                //IstArbeitszeit = actual.workTime.NiceTimespan();

                var info = await InfoService.LoadInfoAsync(TheDate);
                SollArbeitszeit = info.

            }
        }

        public void Dispose()
        {
            AppState.OnCurrentDayChanged -= appState_OnCurrentDayChanged;
            AppState.OnRegistered -= appState_OnRegistered;
        }
    }
}
