using CoronaKurzArbeit.Shared.Extensions;
using CoronaKurzArbeit.Shared.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CoronaKurzArbeit.Logic.Services
{
    public interface IInfoService
    {
        Task<InfoViewModel> GetInfo(DateTime theDate);
        DateTime CalculateGoHome(DateTime inBooking, TimeSpan netTargetWorkTime, TimeSpan grossPause, TimeSpan netPause);
    }

    public class InfoService : IInfoService
    {
        private readonly ILogger<InfoService> _logger;
        private readonly KurzarbeitSettingsConfiguration _config;
        private readonly ICoronaService _coronaService;
        private readonly ITimeBookingsService _bookingsService;

        public InfoService(ILogger<InfoService> logger, 
            KurzarbeitSettingsConfiguration config,
            ICoronaService coronaService,
            ITimeBookingsService bookingsService)
        {
            _logger = logger;
            _config = config;
            _coronaService = coronaService;
            _bookingsService = bookingsService;
        }

        public async Task<InfoViewModel> GetInfo(DateTime theDate)
        {
            var ret = new InfoViewModel();
            try
            {
                ret.GrossTargetWorkTime = TimeSpan.FromHours(theDate.GetWorkhours(_config).ToDouble());
                ret.CoronaDelta = TimeSpan.FromHours(_coronaService.KAAusfallPerDay(theDate).ToDouble());

                var grossWTime = await _bookingsService.GetGrossWorkTimeForDayAsync(theDate);
                ret.GrossActualWorkTime = grossWTime.grossWorkTime;

                var pauses = await _bookingsService.GetPauseForDayAsync(theDate);
                ret.NetActualWorktime = _bookingsService.GetNetWorkingTimeForDay(theDate, grossWTime, pauses);

            }
            catch (Exception ex)
            {
                var msg = $"Error calculating the info view model: {ex.Message}";
                _logger.LogError(ex, msg);
                throw new Exception(msg, ex);
            }
            return ret;
        }

        public DateTime CalculateGoHome(DateTime inBooking, TimeSpan netTargetWorkTime, TimeSpan grossPause, TimeSpan netPause)
        {
            var pause = netPause;
            if (netTargetWorkTime.TotalHours > 6)
            {
                //Wir brauchen eine Pause!
                if(grossPause > TimeSpan.Zero && grossPause < TimeSpan.FromMinutes(30))
                {
                    //Wir haben bereits Pause gemacht, aber zuwenig
                    var pDelta = grossPause.Subtract(netPause);
                    var rest = TimeSpan.FromMinutes(30).Subtract(grossPause);
                    pause = rest.Subtract(pDelta);
                } else if(grossPause == TimeSpan.Zero)
                {
                    //Noch keine Pause gemacht
                    pause = TimeSpan.FromMinutes(30);
                } else
                {
                    pause = netPause;
                }
            }

            return inBooking.Add(netTargetWorkTime).Add(pause);
        }
    }
}
