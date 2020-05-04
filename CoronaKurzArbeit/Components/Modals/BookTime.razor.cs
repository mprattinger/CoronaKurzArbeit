using Blazored.Modal;
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
        [Inject]
        public ApplicationDbContext Context { get; set; }

        [CascadingParameter]
        public BlazoredModalInstance? BlazoredModal { get; set; }

        public DateTime Time { get; set; } = DateTime.Now;

        public bool Pause { get; set; }

        public async Task SubmitForm()
        {
            var booking = new TimeBooking
            {
                BookingTime = Time,
                IsPause = Pause
            };
            Context.TimeBookings.Add(booking);
            await Context.SaveChangesAsync();
            BlazoredModal?.Close();
        }

        public void Cancel()
        {
            BlazoredModal?.Cancel();
        }
    }
}
