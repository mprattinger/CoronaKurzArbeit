using CoronaKurzArbeit.Data;
using CoronaKurzArbeit.Extensions;
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
        Task AddBookingAsync(TimeBooking newBooking);
        Task UpdateBookingAsync(TimeBooking changed);
        Task DeleteBookingAsync(TimeBooking booking);
        Task<TimeBooking?> GetInBookingForDayAsync(DateTime theDate);
        Task<TimeBooking?> GetOutBookingForDayAsync(DateTime theDate);
        Task<List<TimeBooking>> GetPauseBookingsForDayAsync(DateTime theDate);
        Task<(TimeBooking? inBooking, TimeBooking? outBooking, TimeSpan grossWorkTime)> GetGrossWorkTimeForDayAsync(DateTime theDate);
        Task<(TimeSpan grossPauseDuration, TimeSpan netPauseDuration, List<(TimeBooking pauseStart, TimeBooking pauseEnd, TimeSpan duration)> pauseList)> GetPauseForDayAsync(DateTime theDate);
        Task<TimeSpan> GetNetWorkingTimeForDayAsync(DateTime theDate);
    }

    public class TimeBookingsService : ITimeBookingsService
    {
        private readonly ILogger<TimeBookingsService> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly KurzarbeitSettingsConfiguration _config;

        public TimeBookingsService(ILogger<TimeBookingsService> logger, ApplicationDbContext context, IDateTimeProvider dateTimeProvider, KurzarbeitSettingsConfiguration config)
        {
            _logger = logger;
            _context = context;
            _dateTimeProvider = dateTimeProvider;
            _config = config;
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

        public async Task AddBookingAsync(TimeBooking newBooking)
        {
            try
            {
                _context.TimeBookings?.Add(newBooking);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                var msg = $"Error adding new time booking for {newBooking.BookingTime.ToShortTimeString()}";
                _logger.LogError(ex, msg);
                throw new Exception(msg, ex);
            }
        }

        public async Task UpdateBookingAsync(TimeBooking changed)
        {
            try
            {
                _context.Update(changed);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                var msg = $"Error updating booking with new time {changed.BookingTime.ToShortTimeString()}";
                _logger.LogError(ex, msg);
                throw new Exception(msg, ex);
            }
        }

        public async Task DeleteBookingAsync(TimeBooking booking)
        {
            try
            {
                _context.TimeBookings?.Remove(booking);
                await _context.SaveChangesAsync();
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                var msg = $"Error deleting booking for time {booking.BookingTime.ToShortTimeString()}";
                _logger.LogError(ex, msg);
                throw new Exception(msg, ex);
            }
        }

        public async Task<(TimeBooking? inBooking, TimeBooking? outBooking, TimeSpan grossWorkTime)> GetGrossWorkTimeForDayAsync(DateTime theDate)
        {
            try
            {
                var inTime = await GetInBookingForDayAsync(theDate);
                if (inTime == null) return (null, null, TimeSpan.Zero);
                var outTime = await GetOutBookingForDayAsync(theDate);
                if (outTime == null)
                {
                    var workHours = inTime.BookingTime.AddHours(Convert.ToDouble(theDate.GetWorkhours(_config)));
                    if(workHours.Subtract(inTime.BookingTime) >= TimeSpan.FromHours(6))
                    {
                        workHours = workHours.AddMinutes(30);
                    }
                    var target = _dateTimeProvider.GetCurrentTime() > workHours ? workHours : _dateTimeProvider.GetCurrentTime();
                    outTime = new TimeBooking { BookingTime = target };
                }
                return (inTime, outTime, outTime.BookingTime.Subtract(inTime.BookingTime));
            }
            catch (Exception ex)
            {
                var msg = $"Error reading in gross worktime for day {theDate.Date.ToShortDateString()}";
                _logger.LogError(ex, msg);
                throw new Exception(msg, ex);
            }
        }

        public async Task<TimeSpan> GetNetWorkingTimeForDayAsync(DateTime theDate)
        {
            try
            {
                var (inBooking, outBooking, grossWorkTime) = await GetGrossWorkTimeForDayAsync(theDate);
                if (grossWorkTime == TimeSpan.Zero) return TimeSpan.Zero;
                var (grossPauseDuration, netPauseDuration, pauseList) = await GetPauseForDayAsync(theDate);

                if(grossWorkTime.TotalHours >= 6 && (netPauseDuration == TimeSpan.Zero || netPauseDuration.TotalMinutes < 30))
                {
                    //Keine Pause oder pause kleiner 30 Minuten, aber mehr als 6 Stunden
                    return grossWorkTime.Subtract(TimeSpan.FromMinutes(30));
                } else 
                {
                    return grossWorkTime.Subtract(netPauseDuration);
                }
            }
            catch (Exception ex)
            {
                var msg = $"Error reading net working time for day {theDate.Date.ToShortDateString()}";
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

        public async Task<(TimeSpan grossPauseDuration, TimeSpan netPauseDuration, List<(TimeBooking pauseStart, TimeBooking pauseEnd, TimeSpan duration)> pauseList)> GetPauseForDayAsync(DateTime theDate)
        {
            try
            {
                var pauseRaw = new List<(TimeBooking pauseStart, TimeBooking pauseEnd, TimeSpan pauseDuration)>();
                var pauses = await GetPauseBookingsForDayAsync(theDate);
                if (pauses.Count == 0) return (TimeSpan.Zero, TimeSpan.Zero, pauseRaw);
                var c = 0;
                while(c <= pauses.Count - 2)
                {
                    var data = pauses.Skip(c).Take(2);
                    pauseRaw.Add((data.First(), data.Last(), data.Last().BookingTime.Subtract(data.First().BookingTime)));
                    c += 2;
                }

                var grossPause = new TimeSpan(pauseRaw.Sum(x => x.pauseDuration.Ticks));
                var netPause = grossPause;

                if (grossPause.TotalMinutes >= _config.PauseFree)
                {
                    netPause = netPause.Subtract(TimeSpan.FromMinutes(_config.PauseFree));
                }

                return (grossPause, netPause, pauseRaw);
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
