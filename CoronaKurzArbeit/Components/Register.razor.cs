using Blazored.Modal;
using Blazored.Modal.Services;
using CoronaKurzArbeit.Components.Modals;
using CoronaKurzArbeit.Logic.Services;
using CoronaKurzArbeit.Services;
using CoronaKurzArbeit.Shared.Models;
using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace CoronaKurzArbeit.Components
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1063:Implement IDisposable Correctly", Justification = "<Pending>")]
    public partial class Register : IDisposable
    {
        [Inject]
        public IModalService Modal { get; set; } = default!;

        [Inject]
        public IAppState AppState { get; set; } = default!;

        //[Inject]
        //public ApplicationDbContext Context { get; set; } = default!;

        [Inject]
        public ITimeBookingsService BookingsService { get; set; } = default!;

        [Parameter]
        public DateTime AtDate { get; set; } = DateTime.MinValue;

        [Parameter]
        public string Class { get; set; } = "";

        public DateTime CurrentDate { get; set; } = DateTime.Now.Date;

        private readonly string fixedClass = "btn btn-primary btn-block";
        private string Classes
        {
            get
            {
                return $"{fixedClass} {Class}";
            }
        }

        protected override void OnInitialized()
        {
            AppState.OnCurrentDayChanged += onCurrentDayChanged_event;
        }

        private async Task onCurrentDayChanged_event(DateTime theDay)
        {
            await InvokeAsync(() =>
            {
                CurrentDate = theDay;
                StateHasChanged();
            });
        }

        async Task registerTime()
        {
            var baseDate = AtDate == DateTime.MinValue ? CurrentDate : AtDate;
            var param = new ModalParameters();
            var tb = new TimeBooking
            {
                BookingTime = baseDate.Date.AddHours(DateTime.Now.Hour).AddMinutes(DateTime.Now.Minute)
            };
            param.Add(nameof(BookTime.Booking2Change), tb);
            var ret = Modal.Show<BookTime>("Zeit buchen", param);
            var result = await ret.Result;
            if (!result.Cancelled)
            {
                var changed = (TimeBooking)result.Data;
                //Context.TimeBookings?.Add(changed);
                //await Context.SaveChangesAsync();
                await BookingsService.AddBookingAsync(changed);
                await AppState.RegisteredAsync(changed);
                StateHasChanged();
            }
        }

        public void Dispose()
        {
            AppState.OnCurrentDayChanged -= onCurrentDayChanged_event;
        }
    }
}
