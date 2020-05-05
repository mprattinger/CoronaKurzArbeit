using CoronaKurzArbeit.Extensions;
using CoronaKurzArbeit.Models;
using CoronaKurzArbeit.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoronaKurzArbeit.Components.WeekList
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1063:Implement IDisposable Correctly", Justification = "<Pending>")]
    public partial class Day : IDisposable
    {
        [Inject]
        public IAppState AppState { get; set; } = default!;

        [Parameter]
        public DateTime TheDay { get; set; }

        public string Gekommen { get; set; } = string.Empty;

        public string Gegangen { get; set; } = string.Empty;

        public string Pause { get; set; } = string.Empty;


        protected override void OnInitialized()
        {
            AppState.OnBookingListChanged += appState_OnBookingListChanged;
        }

        private async Task appState_OnBookingListChanged(List<TimeBooking> list)
        {
            await InvokeAsync(() =>
            {
                var todayBookings = list.Where(x => x.BookingTime >= TheDay && x.BookingTime < TheDay.AddDays(1)).ToList();
                if (todayBookings.Count > 0)
                {
                    prepareData(todayBookings);
                    checkDayBookings(todayBookings);
                    StateHasChanged();
                } else
                {

                }
            });

            
        }

        private void prepareData(List<TimeBooking> todayBookings)
        {
            var gekgeg = todayBookings.Where(x => x.IsPause == false).OrderBy(x => x.BookingTime).ToList();
            Gekommen = gekgeg.First().BookingTime.ToShortTimeString();
            if (gekgeg.Count > 1) Gegangen = gekgeg.Last().BookingTime.ToShortTimeString();


            var ps = todayBookings.Where(x => x.IsPause == true).OrderBy(x => x.BookingTime).ToList();
            if (ps.Count > 0)
            {
                var PSum = ps.GetPauseCouples().Select(x => x.Item2.BookingTime.Subtract(x.Item1.BookingTime)).Aggregate(TimeSpan.Zero, (t1, t2) => t1 + t2);
                var h = PSum.Hours < 10 ? $"0{PSum.Hours}" : PSum.Hours.ToString();
                var m = PSum.Minutes < 10 ? $"0{PSum.Minutes}" : PSum.Minutes.ToString();
                var ph = string.Format("{0:N2}", PSum.TotalHours);
                Pause = $"{h}:{m} / {ph}";
            }
        }

        private void checkDayBookings(List<TimeBooking> todayBookings)
        {

        }

        public void Dispose()
        {
            AppState.OnBookingListChanged -= appState_OnBookingListChanged;
        }

    }
}
