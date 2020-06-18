using CoronaKurzArbeit.Shared.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CoronaKurzArbeit.Logic.Services
{
    public interface IInfoService2
    {
        Task<DateTime> GoHomeAsync(DateTime theDate);
        Task<InfoViewModel2> LoadInfoAsync(DateTime theDate);
    }

    public class InfoService2 : IInfoService2
    {
        private readonly IActualWorkTimeService _actualWorkTime;
        private readonly ITargetWorkTimeService _targetWorkTime;
        private readonly KurzarbeitSettingsConfiguration _config;

        public InfoService2(IActualWorkTimeService actualWorkTime, ITargetWorkTimeService targetWorkTime, KurzarbeitSettingsConfiguration config)
        {
            _actualWorkTime = actualWorkTime;
            _targetWorkTime = targetWorkTime;
            _config = config;
        }

        public async Task<DateTime> GoHomeAsync(DateTime theDate)
        {
            var (_, _, targetWorkTime, targetPause) = _targetWorkTime.LoadData(theDate);
            var actual = await _actualWorkTime.LoadDataAsync(theDate);

            if (actual.inTime == DateTime.MinValue) return DateTime.MinValue;

            TimeSpan worktime;
            //if (actual.workTime < targetWorkTime)
            //{
            //    worktime = targetWorkTime;
            //}
            //else
            //{
            //    worktime = actual.workTime;
            //}
            worktime = targetWorkTime;

            TimeSpan pauseTime;
            if (actual.pauseTime < targetPause)
            {
                pauseTime = targetPause;
            }
            else
            {
                pauseTime = actual.pauseTime;
            }

            if (actual.pauseTime >= TimeSpan.FromMinutes(_config.PauseFree))
            {
                //Die Firma schenkt 10 Minuten Pause ab 10 Minuten Pause
                pauseTime = pauseTime.Subtract(TimeSpan.FromMinutes(_config.PauseFree));
            }

            return actual.inTime.Add(worktime).Add(pauseTime);
        }

        public async Task<InfoViewModel2> LoadInfoAsync(DateTime theDate)
        {
            var ret = new InfoViewModel2();
            var (plannedWorkTime, coronaDelta, _, targetPause) = _targetWorkTime.LoadData(theDate);
            var (_, _, workTime, pauseTime) = await _actualWorkTime.LoadDataAsync(theDate, true);

            //Gearbeitete Zeit
            ret.Worked = workTime;
            //Diff zur Sollzeit
            //ret.TargetDiff = workTime.Subtract(targetWorkTime);
            ret.TargetDiff = workTime.Subtract(plannedWorkTime);

            //Pausen
            ret.Pause = pauseTime;
            //Diff zu Soll
            ret.PauseTargetDiff = pauseTime.Subtract(targetPause);
            if (ret.Pause >= TimeSpan.FromMinutes(_config.PauseFree))
            {
                ret.Pause = ret.Pause.Subtract(TimeSpan.FromMinutes(_config.PauseFree));
            }

            //Aktueller CoronaSaldo (eventuell mit aktueller Zeit vergleichen?)
            ret.KuaTarget = coronaDelta;
            if (ret.KuaTarget != TimeSpan.Zero)
            {
                ret.KuaActual = plannedWorkTime.Subtract(workTime);
                if(ret.KuaActual < TimeSpan.Zero)
                {
                    //Es kann kein negatives Kua geben
                    ret.KuaActual = TimeSpan.Zero;
                }
                if(workTime > plannedWorkTime)
                {
                    //Bei Kua kann es nur pos VAZ geben
                    ret.VAZ = workTime.Subtract(plannedWorkTime);
                }
                ret.KuaDiff = ret.KuaActual.Subtract(ret.KuaTarget);
            } else
            {
                //Kein Kua, gibt es VAZ?
                ret.VAZ = plannedWorkTime.Subtract(workTime) != TimeSpan.Zero ? workTime.Subtract(plannedWorkTime) : TimeSpan.Zero;
            }

            return ret;
        }
    }
}
