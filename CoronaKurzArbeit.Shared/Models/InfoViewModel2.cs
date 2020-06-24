using System;
using System.Collections.Generic;
using System.Text;

namespace CoronaKurzArbeit.Shared.Models
{
    public class InfoViewModel2
    {
        public TimeSpan Worktime { get; set; } = TimeSpan.Zero;
        public TimeSpan CoronaDelta { get; set; } = TimeSpan.Zero;
        public TimeSpan WorktimeCorona { get; set; } = TimeSpan.Zero;

        public TimeSpan Worked { get; set; } = TimeSpan.Zero;
        
        public TimeSpan TargetDiff { get; set; } = TimeSpan.Zero;

        public TimeSpan Pause { get; set; } = TimeSpan.Zero;

        public TimeSpan PauseTargetDiff { get; set; } = TimeSpan.Zero;

        public TimeSpan KuaTarget { get; set; } = TimeSpan.Zero;
        public TimeSpan KuaActual { get; set; } = TimeSpan.Zero;
        public TimeSpan KuaDiff { get; set; } = TimeSpan.Zero;

        public TimeSpan VAZ { get; set; } = TimeSpan.Zero;
    }
}
