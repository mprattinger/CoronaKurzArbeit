using System;

namespace CoronaKurzArbeit.Shared.Models
{
    public class InfoViewModel
    {
        public TimeSpan GrossTargetWorkTime { get; set; }

        public TimeSpan CoronaDelta { get; set; }

        public TimeSpan NetTargetWorkTime
        {
            get
            {
                return GrossTargetWorkTime.Subtract(CoronaDelta);
            }
        }

        public TimeSpan TargetPause { get; set; }

        public TimeSpan GrossActualWorkTime { get; set; }

        public TimeSpan NetActualWorktime { get; set; }

        public TimeSpan Kua
        {
            get
            {
                if (CoronaDelta > TimeSpan.Zero)
                {
                    return GrossActualWorkTime.Subtract(NetActualWorktime);
                }
                else
                {
                    return TimeSpan.Zero;
                }

            }
        }

        public TimeSpan VAZ { 
        get
            {
                if(Kua < TimeSpan.Zero)
                {
                    return Kua.Duration();
                } else
                {
                    return TimeSpan.Zero;
                }
            }
        }
    }
}
