using CoronaKurzArbeit.DAL.DataAccessSQL;
using CoronaKurzArbeit.Shared.Extensions;
using CoronaKurzArbeit.Shared.Models;
using CoronaKurzArbeit.Shared.Services;
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
        decimal GetWorkhours(DateTime current);
        List<(DateTime day, decimal arbeitsZeit, WorkDayType type)> GetWorkDaysForWeek(DateTime dayInWeek);
    }

    public class TimeBookingsService : ITimeBookingsService
    {
        private readonly ILogger<TimeBookingsService> _logger;
        private readonly ApplicationDbContext _context;
        private readonly KurzarbeitSettingsConfiguration _config;
        private readonly IFeiertagService _feiertagService;

        public TimeBookingsService(
            ILogger<TimeBookingsService> logger, 
            ApplicationDbContext context,
            KurzarbeitSettingsConfiguration config,
            IFeiertagService feiertagService)
        {
            _logger = logger;
            _context = context;
            _config = config;
            _feiertagService = feiertagService;
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

        public decimal GetWorkhours(DateTime current)
        {
            var wdays = GetWorkDaysForWeek(current);
            var (day, arbeitsZeit, type) = wdays.Where(x => x.day.Date == current.Date).FirstOrDefault();
            return type == WorkDayType.Workday || type == WorkDayType.KAday ? arbeitsZeit : 0;
        }

        public List<(DateTime day, decimal arbeitsZeit, WorkDayType type)> GetWorkDaysForWeek(DateTime dayInWeek)
        {
            var workDays = new List<(DateTime day, decimal arbeitsZeit, WorkDayType type)>();

            //Arbeitstage ermitteln
            var f = dayInWeek.FirstDayOfWeek(DayOfWeek.Monday);
            while (f.DayOfWeek != DayOfWeek.Saturday)
            {
                //Ist der Tag ein Feiertag?
                if (_feiertagService.IsFeiertag(f))
                {
                    //Ja
                    workDays.Add((f, getWorkhoursFromConfig(f), WorkDayType.Free));
                }
                else
                {
                    //Nein
                    //Ein Fenstertag
                    if (_feiertagService.IsFenstertag(f))
                    {
                        //Ja
                        workDays.Add((f, getWorkhoursFromConfig(f), WorkDayType.Free));
                    }
                    else
                    {
                        //Nein
                        //Ist der Tag ein KA Tag?
                        if (_config.CoronaDays.Contains(f.DayOfWeek) && f.Date >= _config.Started.Date)
                        {
                            //Ja
                            workDays.Add((f, getWorkhoursFromConfig(f), WorkDayType.KAday));
                        }
                        else
                        {
                            //Nein
                            //-> Arbeitstag
                            workDays.Add((f, getWorkhoursFromConfig(f), WorkDayType.Workday));
                        }
                    }
                }
                f = f.AddDays(1);
            }

            return workDays;
        }

        private decimal getWorkhoursFromConfig(DateTime theDate)
        {
            var props = _config.GetType().GetProperties();
            foreach (var p in props)
            {
                if (p.Name == theDate.DayOfWeek.ToString())
                {
                    return Convert.ToDecimal(p.GetValue(_config) ?? default(decimal));
                }
            }
            return 0;
        }
    }
}
