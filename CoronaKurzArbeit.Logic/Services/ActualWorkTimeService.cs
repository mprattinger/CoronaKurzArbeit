﻿using CoronaKurzArbeit.Shared.Models;
using CoronaKurzArbeit.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoronaKurzArbeit.Logic.Services
{
    public interface IActualWorkTimeService
    {
        Task<(DateTime theDay, DateTime inTime, DateTime outTime, TimeSpan workTime, TimeSpan pauseTime)> LoadDataAsync(DateTime theDate, bool forInfo = false);
        Task<(TimeSpan workTime, TimeSpan pauseTime, List<(DateTime theDay, DateTime inTime, DateTime outTime, TimeSpan workTime, TimeSpan pauseTime)> entries)> LoadDataForWeekAsync(DateTime firstDay, DateTime lastDay, bool forInfo = false);
    }
    public class ActualWorkTimeService : IActualWorkTimeService
    {
        //private readonly ILogger<ActualWorkTimeService> _logger;
        private readonly ITimeBookingsService _timeBookings;
        private readonly IDateTimeProvider _dateTimeProvider;

        public ActualWorkTimeService(
            //ILogger<ActualWorkTimeService> logger,
            ITimeBookingsService timeBookingsService,
            IDateTimeProvider dateTimeProvider
            )
        {
            //_logger = logger;
            _timeBookings = timeBookingsService;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<(DateTime theDay, DateTime inTime, DateTime outTime, TimeSpan workTime, TimeSpan pauseTime)> LoadDataAsync(DateTime theDate, bool forInfo = false)
        {
            var bookings = await _timeBookings.GetBookingsAsync(theDate);
            return calcualateDataForDay(theDate, bookings, forInfo);
        }

        public async Task<(TimeSpan workTime, TimeSpan pauseTime, List<(DateTime theDay, DateTime inTime, DateTime outTime, TimeSpan workTime, TimeSpan pauseTime)> entries)> LoadDataForWeekAsync(DateTime firstDay, DateTime lastDay, bool forInfo = false)
        {
            var bookings = await _timeBookings.GetBookingsAsync(firstDay, lastDay);
            var current = firstDay.Date;

            var work = TimeSpan.Zero;
            var pause = TimeSpan.Zero;
            var entryList = new List<(DateTime theDay, DateTime inTime, DateTime outTime, TimeSpan workTime, TimeSpan pauseTime)>();

            while (current < lastDay.Date.AddDays(1))
            {
                var (_, inTime, outTime, workTime, pauseTime) = calcualateDataForDay(current.Date, bookings.Where(x => x.BookingTime >= current.Date && x.BookingTime < current.Date.AddDays(1)).ToList(), forInfo);

                work = work.Add(workTime);
                pause = pause.Add(pauseTime);
                entryList.Add((current.Date, inTime, outTime, workTime, pauseTime));

                current = current.AddDays(1);
            }

            return (work, pause, entryList);
        }

        private (DateTime theDay, DateTime inTime, DateTime outTime, TimeSpan workTime, TimeSpan pauseTime) calcualateDataForDay(DateTime theDay, List<TimeBooking> bookings, bool forInfo = false)
        {
            var work = TimeSpan.Zero;
            var pause = TimeSpan.Zero;
            var outT = DateTime.MinValue;

            DateTime inT;
            if (bookings.Count == 0)
            {
                inT = DateTime.MinValue;
            }
            else if (bookings.Count == 1)
            {
                //Nur In
                inT = bookings.First().BookingTime;
                if (forInfo)
                {
                    //outTime should be now
                    outT = _dateTimeProvider.GetCurrentTime();
                }
            }
            else
            {
                //In/Out
                inT = bookings.First().BookingTime;
                if (bookings.Count % 2 == 0)
                {
                    outT = bookings.Last().BookingTime;
                }
                else
                {
                    if (forInfo)
                    {
                        //Es gibt kein out booking aber für die info berechnung die aktuelle zeit nehmen
                        bookings.Add(new TimeBooking { BookingTime = _dateTimeProvider.GetCurrentTime() });
                        outT = bookings.Last().BookingTime;
                    }
                }
                var c = 0;
                TimeBooking? last = null;
                foreach (var b in bookings)
                {
                    if (c == 0)
                    {
                        if (last != null)
                        {
                            pause = pause.Add(b.BookingTime.Subtract(last.BookingTime));
                        }
                        last = b;
                        c++;
                    }
                    else
                    {
                        if (last != null)
                        {
                            work = work.Add(b.BookingTime.Subtract(last.BookingTime));
                        }
                        c = 0;
                        last = b;
                    }
                }
            }
            return (theDay, inT, outT, work, pause);
        }
    }
}
