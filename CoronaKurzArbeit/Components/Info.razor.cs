using CoronaKurzArbeit.Logic.Services;
using CoronaKurzArbeit.Services;
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

        public DateTime CurrentDate { get; set; }

        public InfoViewModel2 InfoData { get; set; } = new InfoViewModel2();

        protected override void OnInitialized()
        {
            AppState.OnCurrentDayChanged += appState_OnCurrentDayChanged;
            AppState.OnRegistered += appState_OnRegistered;
        }

        private async Task appState_OnRegistered(TimeBooking arg)
        {
            await InvokeAsync(async () =>
            {
                CurrentDate = arg.BookingTime.Date;
                await loadData(CurrentDate);
                StateHasChanged();
            });
        }

        private async Task appState_OnCurrentDayChanged(DateTime arg)
        {
            await InvokeAsync(async () =>
            {
                CurrentDate = arg.Date;
                await loadData(CurrentDate);
                StateHasChanged();
            });
        }

        private async Task loadData(DateTime arg)
        {
            InfoData = await InfoService.LoadInfoAsync(CurrentDate);
            await AppState.InfoLoadedFinishedAsync(arg);
        }

        public void Dispose()
        {
            AppState.OnCurrentDayChanged -= appState_OnCurrentDayChanged;
        }
    }
}
