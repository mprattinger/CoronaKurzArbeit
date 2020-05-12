using CoronaKurzArbeit.Data;
using CoronaKurzArbeit.Models;
using CoronaKurzArbeit.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoronaKurzArbeit.Pages
{
    public partial class WeekLists
    {
        [Inject]
        public KurzarbeitSettingsConfiguration KAConfig { get; set; } = new KurzarbeitSettingsConfiguration();

        [Inject]
        public ApplicationDbContext Context { get; set; } = default!;

        [Inject]
        public IAppState AppState { get; set; } = default!;

        public List<DateTime> WeekStarts { get; set; } = new List<DateTime>();

        protected override void OnInitialized()
        {
            var start = KAConfig.Started;
            var end = start.AddMonths(3);
            while (start < end)
            {
                WeekStarts.Add(start);
                start = start.AddDays(7);
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if(firstRender)
            {
                var bl = await Context.TimeBookings.ToListAsync();
                await AppState.BookingListChangedAsync(bl);
                StateHasChanged();
            }
        }
    }
}
