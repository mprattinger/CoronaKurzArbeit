using CoronaKurzArbeit.DAL.DataAccessSQL;
using CoronaKurzArbeit.Shared.Extensions;
using CoronaKurzArbeit.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoronaKurzArbeit.Logic.Services
{
    public interface ITimeBookingsService
    {
        Task<List<TimeBooking>> GetBookingsForDayAsync(DateTime theDate);
        Task AddBookingAsync(TimeBooking newBooking);
        Task UpdateBookingAsync(TimeBooking changed);
        Task DeleteBookingAsync(TimeBooking booking);
        Task<TimeBooking?> GetInBookingForDayAsync(DateTime theDate);
        Task<TimeBooking?> GetOutBookingForDayAsync(DateTime theDate);
        (TimeBooking? inBooking, TimeBooking? outBooking) GetInOutBookingForDay(DateTime theDate, List<TimeBooking> bookings);
        Task<(TimeBooking? inBooking, TimeBooking? outBooking)> GetInOutBookingForDayAsync(DateTime theDate);
        List<TimeBooking> GetPauseBookingsForDay(DateTime theDate, List<TimeBooking> timeBookings);
        Task<List<TimeBooking>> GetPauseBookingsForDayAsync(DateTime theDate);
        Task<(TimeBooking? inBooking, TimeBooking? outBooking, TimeSpan grossWorkTime)> GetGrossWorkTimeForDayAsync(DateTime theDate);
        (TimeBooking? inBooking, TimeBooking? outBooking, TimeSpan grossWorkTime) GetGrossWorkTimeForDay(DateTime theDate,
            (TimeBooking? inBooking, TimeBooking? outBooking) inout);
        Task<(TimeSpan grossPauseDuration, TimeSpan netPauseDuration, List<(TimeBooking pauseStart, TimeBooking pauseEnd, TimeSpan duration)> pauseList)> GetPauseForDayAsync(DateTime theDate);
        Task<TimeSpan> GetNetWorkingTimeForDayAsync(DateTime theDate);
        TimeSpan GetNetWorkingTimeForDay(
            DateTime theDate,
            (TimeBooking? inBooking, TimeBooking? outBooking, TimeSpan grossWorkTime) grossWorkingTimeData,
            (TimeSpan grossPauseDuration, TimeSpan netPauseDuration, List<(TimeBooking pauseStart, TimeBooking pauseEnd, TimeSpan duration)> pauseList) pauseData);
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
                //var inTime = await GetInBookingForDayAsync(theDate);
                //if (inTime == null) return (null, null, TimeSpan.Zero);
                //var outTime = await GetOutBookingForDayAsync(theDate);
                var inout = await GetInOutBookingForDayAsync(theDate);
                return GetGrossWorkTimeForDay(theDate, inout);
            }
            catch (Exception ex)
            {
                var msg = $"Error reading in gross worktime for day {theDate.Date.ToShortDateString()}";
                _logger.LogError(ex, msg);
                throw new Exception(msg, ex);
            }
        }

        public (TimeBooking? inBooking, TimeBooking? outBooking, TimeSpan grossWorkTime) GetGrossWorkTimeForDay(DateTime theDate,
            (TimeBooking? inBooking, TimeBooking? outBooking) inout)
        {
            try
            {
                //var inTime = await GetInBookingForDayAsync(theDate);
                //if (inTime == null) return (null, null, TimeSpan.Zero);
                //var outTime = await GetOutBookingForDayAsync(theDate);
                if (inout.inBooking == null) return (null, null, TimeSpan.Zero);
                if (inout.outBooking == null)
                {
                    var workHours = inout.inBooking.BookingTime.AddHours(Convert.ToDouble(theDate.GetWorkhours(_config)));
                    if (workHours.Subtract(inout.inBooking.BookingTime) >= TimeSpan.FromHours(6))
                    {
                        workHours = workHours.AddMinutes(30);
                    }
                    var target = _dateTimeProvider.GetCurrentTime() > workHours ? workHours : _dateTimeProvider.GetCurrentTime();
                    inout.outBooking = new TimeBooking { BookingTime = target };
                }
                return (inout.inBooking, inout.outBooking, inout.outBooking.BookingTime.Subtract(inout.inBooking.BookingTime));
            }
            catch (Exception ex)
            {
                var msg = $"Error reading in gross worktime for day {theDate.Date.ToShortDateString()}";
                _logger.LogError(ex, msg);
                throw new Exception(msg, ex);
            }
        }

        public TimeSpan GetNetWorkingTimeForDay(
            DateTime theDate,
            (TimeBooking? inBooking, TimeBooking? outBooking, TimeSpan grossWorkTime) grossWorkingTimeData,
            (TimeSpan grossPauseDuration, TimeSpan netPauseDuration, List<(TimeBooking pauseStart, TimeBooking pauseEnd, TimeSpan duration)> pauseList) pauseData)
        {
            try
            {
                if (grossWorkingTimeData.grossWorkTime == TimeSpan.Zero) return TimeSpan.Zero;
                

                if (grossWorkingTimeData.grossWorkTime.TotalHours >= 6 && (pauseData.grossPauseDuration == TimeSpan.Zero || pauseData.grossPauseDuration.TotalMinutes < 30))
                {
                    //Keine Pause oder pause kleiner 30 Minuten, aber mehr als 6 Stunden
                    var diff = TimeSpan.FromMinutes(30).Subtract(pauseData.grossPauseDuration);
                    var p = pauseData.netPauseDuration.Add(diff);
                    return grossWorkingTimeData.grossWorkTime.Subtract(p);
                }
                else
                {
                    return grossWorkingTimeData.grossWorkTime.Subtract(pauseData.netPauseDuration);
                }
            }
            catch (Exception ex)
            {
                var msg = $"Error reading net working time for day {theDate.Date.ToShortDateString()}";
                _logger.LogError(ex, msg);
                throw new Exception(msg, ex);
            }
        }

        public async Task<TimeSpan> GetNetWorkingTimeForDayAsync(DateTime theDate)
        {
            try
            {
                var grossWData = await GetGrossWorkTimeForDayAsync(theDate);
                var pauseData = await GetPauseForDayAsync(theDate);
                return GetNetWorkingTimeForDay(theDate, grossWData, pauseData);
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
                if (bookings.Count == 0 || bookings.Count % 2 != 0)
                {
                    //Es gibt kein out
                    return null;
                }
                else
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

        public (TimeBooking? inBooking, TimeBooking? outBooking) GetInOutBookingForDay(DateTime theDate, List<TimeBooking> bookings)
        {
            var inb = bookings.Where(x => x.BookingTime >= theDate.Date && x.BookingTime < theDate.Date.AddDays(1)).FirstOrDefault();
            TimeBooking? outb = null;
            if (bookings.Count == 0 || bookings.Count % 2 != 0)
            {
                //Es gibt kein out
                outb = null;
            }
            else
            {
                outb = bookings.LastOrDefault();
            }
            return (inb, outb);
        }

        public async Task<(TimeBooking? inBooking, TimeBooking? outBooking)> GetInOutBookingForDayAsync(DateTime theDate)
        {
            try
            {
                var bookings = await GetBookingsForDayAsync(theDate);
                return GetInOutBookingForDay(theDate, bookings);
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
            try
            {
                var bookings = await GetBookingsForDayAsync(theDate);
                return GetPauseBookingsForDay(theDate, bookings);
            }
            catch (Exception ex)
            {
                var msg = $"Error reading all pause bookings for day {theDate.Date.ToShortDateString()}";
                _logger.LogError(ex, msg);
                throw new Exception(msg, ex);
            }
        }

        public List<TimeBooking> GetPauseBookingsForDay(DateTime theDate, List<TimeBooking> timeBookings)
        {
            var ret = new List<TimeBooking>();
            try
            {
                if (timeBookings.Count == 0) return ret;

                if (timeBookings.Count > 2 && timeBookings.Count % 2 == 0) // Es gibt zumindest In - Out - In - Out
                {
                    ret.AddRange(timeBookings.GetRange(1, timeBookings.Count - 2));
                }
                else if (timeBookings.Count > 2 && timeBookings.Count % 2 != 0) // Es gibt zumindest In - Out - In
                {
                    ret.AddRange(timeBookings.GetRange(1, timeBookings.Count - 1));
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
                while (c <= pauses.Count - 2)
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
