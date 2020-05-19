using CoronaKurzArbeit.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoronaKurzArbeit.Services
{
    public interface IAppState
    {
        Task BookingListChangedAsync(List<TimeBooking> list);
        Task RegisteredAsync(TimeBooking newBooking);
        Task CurrentDayChangedAsync(DateTime newDate);

        event Func<List<TimeBooking>, Task> OnBookingListChanged;
        event Func<TimeBooking, Task> OnRegistered;
        event Func<DateTime, Task> OnCurrentDayChanged;
    }

    public class AppState : IAppState
    {
        public event Func<List<TimeBooking>, Task>? OnBookingListChanged;
        public event Func<TimeBooking, Task>? OnRegistered;
        public event Func<DateTime, Task>? OnCurrentDayChanged;

        public async Task BookingListChangedAsync(List<TimeBooking> list)
        {
            if(OnBookingListChanged != null)
            {
                await OnBookingListChanged.Invoke(list);
            }
        }

        public async Task RegisteredAsync(TimeBooking newBooking)
        {
            if(OnRegistered != null)
            {
                await OnRegistered.Invoke(newBooking);
            }
        }

        public async Task CurrentDayChangedAsync(DateTime newDate)
        {
            if(OnCurrentDayChanged != null)
            {
                await OnCurrentDayChanged.Invoke(newDate);
            }
        }
    }
}
