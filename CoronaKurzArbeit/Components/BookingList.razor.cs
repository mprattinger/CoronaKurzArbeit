using Blazored.Modal;
using Blazored.Modal.Services;
using CoronaKurzArbeit.Components.Modals;
using CoronaKurzArbeit.Data;
using CoronaKurzArbeit.Extensions;
using CoronaKurzArbeit.Models;
using CoronaKurzArbeit.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoronaKurzArbeit.Components
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1063:Implement IDisposable Correctly", Justification = "<Pending>")]
    public partial class BookingList : IDisposable
    {
        [Inject]
        public IModalService Modal { get; set; } = default!;

        [Inject]
        public ApplicationDbContext Context { get; set; } = default!;

        [Inject]
        public IAppState AppState { get; set; } = default!;

        [Parameter]
        public DateTime AtDate { get; set; } = DateTime.Now;

        public List<TimeBooking> Bookings { get; set; } = new List<TimeBooking>();

        protected override void OnInitialized()
        {
            AppState.OnRegistered += appState_OnInitialized;
        }

        protected override async Task OnInitializedAsync()
        {
            await loadData();
        }

        public async Task Edit(TimeBooking bTime)
        {
            var param = new ModalParameters();
            param.Add(nameof(BookTime.Booking2Change), bTime);
            var ret = Modal.Show<BookTime>("Zeit ändern", param);
            var result = await ret.Result;
            if (!result.Cancelled)
            {
                var changed = (TimeBooking)result.Data;
                Context.Update(changed);
                await Context.SaveChangesAsync();
                await loadData();
                StateHasChanged();
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
                    Context.TimeBookings?.Remove(bTime);
                    await Context.SaveChangesAsync();
                    await loadData();
                    StateHasChanged();
                }
            }
        }

        private async Task loadData()
        {
            Bookings = await Context.TimeBookings
                .Where(x => x.BookingTime > AtDate.NormalizeAsOnlyDate() && x.BookingTime <= AtDate.NormalizeAsOnlyDate().AddDays(1))
                .OrderBy(x => x.BookingTime)
                .ToListAsync();
        }

        private async Task appState_OnInitialized(TimeBooking timeBooking)
        {
            await InvokeAsync(async () =>
            {
                await loadData();
                StateHasChanged();
            });
        }

        public void Dispose()
        {
            AppState.OnRegistered -= appState_OnInitialized;
        }
    }
}
