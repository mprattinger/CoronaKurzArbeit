using CoronaKurzArbeit.Shared.Extensions;
using CoronaKurzArbeit.Shared.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoronaKurzArbeit.Logic.Services
{
    public class TargetWorkTimeService
    {
        private readonly ILogger<TargetWorkTimeService> _logger;
        private readonly KurzarbeitSettingsConfiguration _config;
        private readonly ICoronaService _coronaService;

        public TimeSpan PlannedWorkTime { get; set; } = TimeSpan.Zero;
        public TimeSpan CoronaDelta { get; set; } = TimeSpan.Zero;
        public TimeSpan TargetWorkTime { get; set; } = TimeSpan.Zero;
        public TimeSpan TargetPause { get; set; } = TimeSpan.Zero;

        public TargetWorkTimeService(
            ILogger<TargetWorkTimeService> logger,
            KurzarbeitSettingsConfiguration config,
            ICoronaService coronaService)
        {
            _logger = logger;
            _config = config;
            _coronaService = coronaService;
        }

        public void LoadData(DateTime theDate)
        {
            PlannedWorkTime = TimeSpan.FromHours(theDate.GetWorkhours(_config).ToDouble());
            CoronaDelta = TimeSpan.FromHours(_coronaService.KAAusfallPerDay(theDate).ToDouble());
            TargetWorkTime = PlannedWorkTime.Subtract(CoronaDelta);
            if(TargetWorkTime.TotalHours >= 6)
            {
                TargetPause = TimeSpan.FromMinutes(30);
            }
        }
    }
}
