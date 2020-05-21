using CoronaKurzArbeit.Data;
using CoronaKurzArbeit.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;

namespace CoronaKurzArbeit.Services
{
    public interface ITimeBookingsService
    {
        Task<List<TimeBooking>> GetBookingsForDayAsync(DateTime theDate);
        Task<TimeBooking?> GetInBookingForDayAsync(DateTime theDate);
        Task<TimeBooking?> GetOutBookingForDayAsync(DateTime theDate);
        Task<List<TimeBooking>> GetPauseBookingsForDayAsync(DateTime theDate);
        Task<(TimeBooking inBooking, TimeBooking outBooking, TimeSpan grossWorkTime)?> GetGrossWorkTimeForDayAsync(DateTime theDate);
        Task<List<(TimeBooking pauseStart, TimeBooking pauseEnd, TimeSpan pauseDuration)>?> GetPausesForDayAsync(DateTime theDate);
    }

    public class TimeBookingsService : ITimeBookingsService
    {
        private readonly ILogger<TimeBookingsService> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IDateTimeProvider _dateTimeProvider;

        public TimeBookingsService(ILogger<TimeBookingsService> logger, ApplicationDbContext context, IDateTimeProvider dateTimeProvider)
        {
            _logger = logger;
            _context = context;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<List<TimeBooking>> GetBookingsForDayAsync(DateTime theDate)
        {
            var ret = new List<TimeBooking>();
            try
            {
                ret = await _context.TimeBookings.Where(x => x.BookingTime >= theDate.Date && x.BookingTime < theDate.Date.AddDays(1)).ToListAsync();
            }
            catch (Exception ex)
            {
                var msg = $"Error reading all bookings for day {theDate.Date.ToShortDateString()}";
                _logger.LogError(ex, msg);
                throw new Exception(msg, ex);
            }
            return ret;
        }

        public async Task<(TimeBooking inBooking, TimeBooking outBooking, TimeSpan grossWorkTime)?> GetGrossWorkTimeForDayAsync(DateTime theDate)
        {
            try
            {
                var inTime = await GetInBookingForDayAsync(theDate);
                if (inTime == null) return null;
                var outTime = await GetOutBookingForDayAsync(theDate);
                if (outTime == null) outTime = new TimeBooking { BookingTime = _dateTimeProvider.GetCurrentTime() }; ;
                return (inTime, outTime, outTime.BookingTime.Subtract(inTime.BookingTime));
            }
            catch (Exception ex)
            {
                var msg = $"Error reading in gross worktime for day {theDate.Date.ToShortDateString()}";
                _logger.LogError(ex, msg);
                throw new Exception(msg, ex);
            }
        }

        public async Task<TimeBooking?> GetInBookingForDayAsync(DateTime theDate)
        { 
            try
            {
                return await _context.TimeBookings.Where(x => x.BookingTime >= theDate.Date && x.BookingTime < theDate.Date.AddDays(1)).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                var msg = $"Error reading in booking for day {theDate.Date.ToShortDateString()}";
                _logger.LogError(ex, msg);
                throw new Exception(msg, ex);
            }
        }

        public async Task<TimeBooking?> GetOutBookingForDayAsync(DateTime theDate)
        {
            try
            {
                var bookings = await GetBookingsForDayAsync(theDate);
                if(bookings.Count == 0 || bookings.Count % 2 != 0)
                {
                    //Es gibt kein out
                    return null;
                } else
                {
                    return bookings.LastOrDefault();
                }
            }
            catch (Exception ex)
            {
                var msg = $"Error reading out booking for day {theDate.Date.ToShortDateString()}";
                _logger.LogError(ex, msg);
                throw new Exception(msg, ex);
            }
        }

        public async Task<List<TimeBooking>> GetPauseBookingsForDayAsync(DateTime theDate)
        {
            var ret = new List<TimeBooking>();
            try
            {
                var bookings = await GetBookingsForDayAsync(theDate);
                if (bookings.Count == 0) return ret;

                if(bookings.Count > 2 && bookings.Count % 2 == 0) // Es gibt zumindest In - Out - In - Out
                {
                    ret.AddRange(bookings.GetRange(1, bookings.Count - 2));
                } else if(bookings.Count > 2 && bookings.Count % 2 != 0) // Es gibt zumindest In - Out - In
                {
                    ret.AddRange(bookings.GetRange(1, bookings.Count - 1));
                }
            }
            catch (Exception ex)
            {
                var msg = $"Error reading all pause bookings for day {theDate.Date.ToShortDateString()}";
                _logger.LogError(ex, msg);
                throw new Exception(msg, ex);
            }
            return ret;
        }

        public async Task<List<(TimeBooking pauseStart, TimeBooking pauseEnd, TimeSpan pauseDuration)>?> GetPausesForDayAsync(DateTime theDate)
        {
            try
            {
                var ret = new List<(TimeBooking pauseStart, TimeBooking pauseEnd, TimeSpan pauseDuration)>();
                var pauses = await GetPauseBookingsForDayAsync(theDate);
                if (pauses.Count == 0) return null;
                var c = 0;
                while(c <= pauses.Count - 2)
                {
                    var data = pauses.Skip(c).Take(2);
                    ret.Add((data.First(), data.Last(), data.Last().BookingTime.Subtract(data.First().BookingTime)));
                    c += 2;
                }
                return ret;
            }
            catch (Exception ex)
            {
                var msg = $"Error reading pause data for day {theDate.Date.ToShortDateString()}";
                _logger.LogError(ex, msg);
                throw new Exception(msg, ex);
            }
        }
    }
}
