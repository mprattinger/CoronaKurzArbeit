using CoronaKurzArbeit.Shared.Models;
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
        Task InfoLoadedFinishedAsync(DateTime newDate);
        Task DayTimeListLoadedFinishedAsync(DateTime newDate);

        event Func<List<TimeBooking>, Task> OnBookingListChanged;
        event Func<TimeBooking, Task> OnRegistered;
        event Func<DateTime, Task> OnCurrentDayChanged;
        event Func<DateTime, Task> OnInfoLoadedFinished;
        event Func<DateTime, Task> OnDayTimeListLoadedFinished;
    }

    public class AppState : IAppState
    {
        public event Func<List<TimeBooking>, Task>? OnBookingListChanged;
        public event Func<TimeBooking, Task>? OnRegistered;
        public event Func<DateTime, Task>? OnCurrentDayChanged;
        public event Func<DateTime, Task>? OnInfoLoadedFinished;
        public event Func<DateTime, Task>? OnDayTimeListLoadedFinished;

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

        public async Task InfoLoadedFinishedAsync(DateTime newDate)
        {
            if(OnInfoLoadedFinished != null)
            {
                await OnInfoLoadedFinished.Invoke(newDate);
            }
        }

        public async Task DayTimeListLoadedFinishedAsync(DateTime newDate)
        {
            if(OnDayTimeListLoadedFinished != null)
            {
                await OnDayTimeListLoadedFinished.Invoke(newDate);
            }
        }
    }
}
