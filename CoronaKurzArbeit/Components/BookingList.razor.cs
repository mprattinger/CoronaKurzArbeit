using Blazored.Modal;
using Blazored.Modal.Services;
using CoronaKurzArbeit.Components.Modals;
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
    public partial class BookingList : IDisposable
    {
        [Inject]
        public IModalService Modal { get; set; } = default!;

        //[Inject]
        //public ApplicationDbContext Context { get; set; } = default!;

        [Inject]
        public IAppState AppState { get; set; } = default!;

        [Inject]
        public ITimeBookingsService BookingsService { get; set; } = default!;

        [Parameter]
        public DateTime AtDate { get; set; } = DateTime.MinValue;

        [Parameter]
        public string Class { get; set; } = "";

        private readonly string fixedClass = "table";
        private string Classes
        {
            get
            {
                return $"{fixedClass} {Class}";
            }
        }

        public DateTime TheDate { get; set; } = DateTime.Now.Date;

        public List<TimeBooking> Bookings { get; set; } = new List<TimeBooking>();

        protected override void OnInitialized()
        {
            
            AppState.OnInfoLoadedFinished += appState_OnInfoLoadedFinished;
        }

        //protected override async Task OnInitializedAsync()
        //{
        //    await loadData();
        //}

        public async Task Edit(TimeBooking bTime)
        {
            var param = new ModalParameters();
            param.Add(nameof(BookTime.Booking2Change), bTime);
            var ret = Modal.Show<BookTime>("Zeit ändern", param);
            var result = await ret.Result;
            if (!result.Cancelled)
            {
                var changed = (TimeBooking)result.Data;
                //Context.Update(changed);
                //await Context.SaveChangesAsync();
                await BookingsService.UpdateBookingAsync(changed);
                await AppState.RegisteredAsync(changed);
            }
        }

        public async Task Delete(TimeBooking bTime)
        {
            var param = new ModalParameters();
            param.Add(nameof(Confirm.Message), "Wollen Sie diesen Zeitpunkt wirklich löschen?");
            param.Add(nameof(Confirm.OkButtonText), "Löschen");
            var ret = Modal.Show<Confirm>("Wirklich löschen", param);
            var result = await ret.Result;
            if (!result.Cancelled)
            {
                if ((bool)result.Data == true)
                {
                    await BookingsService.DeleteBookingAsync(bTime);
                    await AppState.RegisteredAsync(bTime);
                }
            }
        }

        private async Task loadData()
        {
            var baseDate = AtDate == DateTime.MinValue ? TheDate : AtDate;
            Bookings = await BookingsService.GetBookingsForDayAsync(baseDate);
            //Bookings = await Context.TimeBookings
            //    .Where(x => x.BookingTime > baseDate.Date && x.BookingTime <= baseDate.Date.AddDays(1))
            //    .OrderBy(x => x.BookingTime)
            //    .ToListAsync();
        }

        private async Task appState_OnInfoLoadedFinished(DateTime arg)
        {
            await InvokeAsync(async () =>
            {
                TheDate = arg.Date;
                await loadData();
                StateHasChanged();
            });
        }

        public void Dispose()
        {
            AppState.OnInfoLoadedFinished -= appState_OnInfoLoadedFinished;
        }
    }
}
