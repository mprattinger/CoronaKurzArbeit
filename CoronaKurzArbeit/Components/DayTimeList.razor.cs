using CoronaKurzArbeit.Logic.Services;
using CoronaKurzArbeit.Services;
using CoronaKurzArbeit.Shared.Models;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoronaKurzArbeit.Components
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1063:Implement IDisposable Correctly", Justification = "<Pending>")]
    public partial class DayTimeList : IDisposable
    {
        [Inject]
        public IAppState AppState { get; set; } = default!;

        [Inject]
        public ITimeBookingsService BookingsService { get; set; } = default!;

        public DateTime CurrentDate { get; set; }

        public List<TimeBooking> Bookings { get; set; } = new List<TimeBooking>();

        protected override void OnInitialized()
        {
            AppState.OnInfoLoadedFinished += appState_OnInfoLoadedFinished;
        }

        private async Task appState_OnInfoLoadedFinished(DateTime arg)
        {
            await InvokeAsync(async () =>
            {
                CurrentDate = arg.Date;
                Bookings = await BookingsService.GetBookingsAsync(CurrentDate);
                StateHasChanged();
            });
        }

        private async Task newEntry()
        {

        }

        public void Dispose()
        {
            AppState.OnCurrentDayChanged -= appState_OnInfoLoadedFinished;
        }
    }
}
