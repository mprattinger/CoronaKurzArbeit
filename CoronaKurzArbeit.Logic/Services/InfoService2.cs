using CoronaKurzArbeit.Shared.Extensions;
using CoronaKurzArbeit.Shared.Models;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoronaKurzArbeit.Logic.Services
{
    public interface IInfoService2
    {
        Task<DateTime> GoHomeAsync(DateTime theDate);
        Task<InfoViewModel2> LoadInfoAsync(DateTime theDate);
        Task<InfoViewModel2> LoadWeekInfoAsync(DateTime dayInWeek);
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
            var (_, _, _, targetWorkTime, targetPause) = _targetWorkTime.LoadData(theDate);
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
            var target = _targetWorkTime.LoadData(theDate);
            var actual = await _actualWorkTime.LoadDataAsync(theDate, true);

            return calculateInfo(target, actual);   
        }

        public async Task<InfoViewModel2> LoadWeekInfoAsync(DateTime dayInWeek)
        {
            var ret = new InfoViewModel2();

            var starting = dayInWeek.FirstDayOfWeek(DayOfWeek.Monday);
            var ending = dayInWeek.Date.AddDays(7);
            var (plannedWorkTime, coronaDelta, targetWorkTime, targetPause, entries) = _targetWorkTime.LoadData(starting, ending);
            (TimeSpan workTime, TimeSpan pauseTime, List<(DateTime theDay, DateTime inTime, DateTime outTime, TimeSpan workTime, TimeSpan pauseTime)> entries) actual = await _actualWorkTime.LoadDataForWeekAsync(starting, ending);

            var data = new List<InfoViewModel2>();
            var current = starting.Date;
            while (current < ending.Date.AddDays(1))
            {
                var t = entries.Where(x => x.theDay.Date >= current.Date && x.theDay.Date < current.Date.AddDays(1)).First();
                var a = actual.entries.Where(x => x.theDay.Date >= current.Date && x.theDay.Date < current.Date.AddDays(1)).First();

                var info = calculateInfo(t, a);

                current = current.AddDays(1);
            }

            return ret;
        }

        private InfoViewModel2 calculateInfo(
            (DateTime theDay, TimeSpan plannedWorkTime, TimeSpan coronaDelta, TimeSpan targetWorkTime, TimeSpan targetPause) target,
            (DateTime theDay, DateTime inTime, DateTime outTime, TimeSpan workTime, TimeSpan pauseTime) actual
            )
        {
            var ret = new InfoViewModel2();

            //Basisinfo
            ret.Worktime = target.plannedWorkTime;
            ret.CoronaDelta = target.coronaDelta;
            ret.WorktimeCorona = target.targetWorkTime;

            //Pausen
            ret.Pause = actual.pauseTime;
            ret.PauseTargetDiff = actual.pauseTime.Subtract(target.targetPause);

            //Gearbeitete Zeit
            ret.Worked = actual.workTime;
            //Gibt es eine geschenkte Pause?
            if (ret.Pause >= TimeSpan.FromMinutes(_config.PauseFree))
            {
                ret.Worked = ret.Worked.Add(TimeSpan.FromMinutes(_config.PauseFree));
            }

            //Ist die Pause geringer als das soll?
            if (ret.Pause < TimeSpan.FromMinutes(_config.SollPause))
            {
                var add = TimeSpan.FromMinutes(_config.SollPause).Subtract(ret.Pause);
                ret.Worked = ret.Worked.Subtract(add);
            }

            //Diff zur Sollzeit
            //ret.TargetDiff = workTime.Subtract(targetWorkTime);
            ret.TargetDiff = ret.Worked.Subtract(target.plannedWorkTime);

            //Aktueller CoronaSaldo (eventuell mit aktueller Zeit vergleichen?)
            ret.KuaTarget = target.coronaDelta;
            if (ret.KuaTarget != TimeSpan.Zero)
            {
                ret.KuaActual = target.plannedWorkTime.Subtract(ret.Worked);
                if (ret.KuaActual < TimeSpan.Zero)
                {
                    //Es kann kein negatives Kua geben
                    ret.KuaActual = TimeSpan.Zero;
                }
                if (ret.Worked > target.plannedWorkTime)
                {
                    //Bei Kua kann es nur pos VAZ geben
                    ret.VAZ = ret.Worked.Subtract(target.plannedWorkTime);
                }
                ret.KuaDiff = ret.KuaActual.Subtract(ret.KuaTarget);
            }
            else
            {
                //Kein Kua, gibt es VAZ?
                ret.VAZ = target.plannedWorkTime.Subtract(ret.Worked) != TimeSpan.Zero ? ret.Worked.Subtract(target.plannedWorkTime) : TimeSpan.Zero;
            }

            return ret;
        }
    }
}
