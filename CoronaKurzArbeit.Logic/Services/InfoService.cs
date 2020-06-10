using CoronaKurzArbeit.Shared.Extensions;
using CoronaKurzArbeit.Shared.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        private readonly ITargetWorkTimeService _targetWorkTime;
        private readonly ICoronaService _coronaService;
        private readonly ITimeBookingsService _bookingsService;

        public InfoService(ILogger<InfoService> logger, 
            KurzarbeitSettingsConfiguration config,
            ITargetWorkTimeService targetWorkTimeService,
            ICoronaService coronaService,
            ITimeBookingsService bookingsService)
        {
            _logger = logger;
            _config = config;
            _targetWorkTime = targetWorkTimeService;
            _coronaService = coronaService;
            _bookingsService = bookingsService;
        }

        public async Task<InfoViewModel> GetInfo(DateTime theDate)
        {
            var ret = new InfoViewModel();
            try
            {
                var (plannedWorkTime, coronaDelta, targetWorkTime, targetPause) = _targetWorkTime.LoadData(theDate);
                ret.GrossTargetWorkTime = plannedWorkTime;
                ret.CoronaDelta = coronaDelta;
                ret.TargetPause = targetPause;


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
