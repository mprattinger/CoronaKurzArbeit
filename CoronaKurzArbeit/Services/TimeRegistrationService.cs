using CoronaKurzArbeit.Data;
using CoronaKurzArbeit.Extensions;
using CoronaKurzArbeit.Models;
using CoronaKurzArbeit.Models.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace CoronaKurzArbeit.Services
{
    [Obsolete]
    public interface ITimeRegistrationService
    {
        Task AddRegistration(TimeRegistration registration);
        Task<List<TimeRegistration>> GetRegistrationsOfDay(DateTime theDay);
    }

    [Obsolete]
    public class TimeRegistrationService : ITimeRegistrationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TimeRegistrationService> _logger;

        public TimeRegistrationService(ApplicationDbContext context, ILogger<TimeRegistrationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task AddRegistration(TimeRegistration registration)
        {
            try
            {
                if (registration.Type == InOutType.IN || registration.Type == InOutType.OUT)
                {
                    if (await _context.TimeRegistrations.CountAsync(x =>
                        x.Type == registration.Type &&
                        (
                            x.RegistrationMoment >= registration.RegistrationMoment.Date &&
                            x.RegistrationMoment < registration.RegistrationMoment.Date.AddDays(1)
                        )) > 0)
                    {
                        throw new InOutException(registration.Type, "Bereits registriert!");
                    }
                }

                _context.Add(registration);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in adding Registration");
                throw new Exception($"Error in adding Registration: {ex.Message}", ex);
            }
        }

        public async Task<List<TimeRegistration>> GetRegistrationsOfDay(DateTime theDay)
        {
            var ret = new List<TimeRegistration>();

            var regs = await _context.TimeRegistrations.Where(x => x.RegistrationMoment >= theDay.Date && x.RegistrationMoment < theDay.Date.AddDays(1)).ToListAsync();
                        
            if(regs.Any(x => x.Type == InOutType.IN))
            {
                ret.Add(regs.First(x => x.Type == InOutType.IN));
            }



            if (regs.Any(x => x.Type == InOutType.OUT))
            {
                ret.Add(regs.First(x => x.Type == InOutType.OUT));
            }

            return ret;
        }
    }
}
