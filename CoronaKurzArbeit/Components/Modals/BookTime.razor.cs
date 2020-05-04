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
        public TimeBooking Booking2Change { get; set; } = new TimeBooking();
                
        //public DateTime Time { get; set; } = DateTime.Now;

        //public bool Pause { get; set; }

        //protected override void OnInitialized()
        //{
        //    if(Booking2Change != null)
        //    {
        //        Time = Booking2Change.BookingTime;
        //        Pause = Booking2Change.IsPause;
        //    }
        //}

        public async Task SubmitForm()
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
            BlazoredModal?.Close(ModalResult.Ok(Booking2Change));
        }

        public void Cancel()
        {
            BlazoredModal?.Cancel();
        }
    }
}
