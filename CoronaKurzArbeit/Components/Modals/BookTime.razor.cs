using Blazored.Modal;
using Blazored.Modal.Services;
using CoronaKurzArbeit.Data;
using CoronaKurzArbeit.Models;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoronaKurzArbeit.Components.Modals
{
    public partial class BookTime
    {
        [CascadingParameter]
        public BlazoredModalInstance? BlazoredModal { get; set; }

        [Parameter]
        public TimeBooking Booking2Change { get; set; } = default!;

        public DateTime Time { get; set; } = DateTime.Now;

        //public bool Pause { get; set; }

        protected override void OnInitialized()
        {
            Time = Booking2Change.BookingTime;
            Console.WriteLine("Test");
        }

        public void SubmitForm()
        {
            //if (Booking2Change != null)
            //{
            //    Booking2Change.BookingTime = Time;
            //    Booking2Change.IsPause = Pause;
            //    Context.Update(Booking2Change);
            //    await Context.SaveChangesAsync();
            //}
            //else
            //{
            //    var Booking2Change = new TimeBooking
            //    {
            //        BookingTime = Time,
            //        IsPause = Pause
            //    };
            //    Context.TimeBookings?.Add(booking);
            //    await Context.SaveChangesAsync();
            //}
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
