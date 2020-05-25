using Blazored.Modal;
using Blazored.Modal.Services;
using CoronaKurzArbeit.Shared.Models;
using Microsoft.AspNetCore.Components;
using System;

namespace CoronaKurzArbeit.Components.Modals
{
    public partial class BookTime
    {
        [CascadingParameter]
        public BlazoredModalInstance? BlazoredModal { get; set; }

        [Parameter]
        public TimeBooking Booking2Change { get; set; } = default!;

        public DateTime Time { get; set; } = DateTime.Now;

        protected override void OnInitialized()
        {
            Time = Booking2Change.BookingTime;
            Console.WriteLine("Test");
        }

        public void SubmitForm()
        {
            var newDate = new DateTime(Booking2Change.BookingTime.Year, Booking2Change.BookingTime.Month, Booking2Change.BookingTime.Day, Time.Hour, Time.Minute, 0);
            Booking2Change.BookingTime = newDate;
            BlazoredModal?.Close(ModalResult.Ok(Booking2Change));
        }

        public void Cancel()
        {
            BlazoredModal?.Cancel();
        }
    }
}
