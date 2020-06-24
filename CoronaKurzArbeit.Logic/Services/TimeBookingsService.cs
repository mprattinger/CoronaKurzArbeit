using CoronaKurzArbeit.DAL.DataAccessSQL;
using CoronaKurzArbeit.Shared.Extensions;
using CoronaKurzArbeit.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CoronaKurzArbeit.Logic.Services
{
    public interface ITimeBookingsService
    {
        Task<List<TimeBooking>> GetBookingsForDayAsync(DateTime theDate);
        Task AddBookingAsync(TimeBooking newBooking);
        Task UpdateBookingAsync(TimeBooking changed);
        Task DeleteBookingAsync(TimeBooking booking);
    }

    public class TimeBookingsService : ITimeBookingsService
    {
        private readonly ILogger<TimeBookingsService> _logger;
        private readonly ApplicationDbContext _context;

        public TimeBookingsService(
            ILogger<TimeBookingsService> logger, 
            ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
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
    }
}
