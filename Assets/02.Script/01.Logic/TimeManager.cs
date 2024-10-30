using System.Collections.Generic;
using SCC.JsonIO;

//!<=================================================================================

namespace SCC
{
    //!<==============================================================================

    public class TimeManager : SCC.Singleton<TimeManager>
    {
        public static readonly System.DateTime OriginTime = 
            new System.DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);

        //!<=========================================================================
        protected int _PrevCheckDay = 0;

        private System.Action<System.DateTime> _OnChangeUpdateDateDay = (now) => { };

        public System.DateTime PreviousNowTime       { get; private set; } = System.DateTime.Now;

        //!<=========================================================================
        public static System.DateTime Now
        {
            get
            {
                var now = System.DateTime.Now;

                return now;
            }
        }

        public event System.Action<System.DateTime> OnChangeUpdateDateDay
        {
            add
            {
                foreach (var c in this._OnChangeUpdateDateDay.GetInvocationList())
                {
                    if (c.Equals(value))
                    {
                        return;
                    }
                }
                this._OnChangeUpdateDateDay += value;
            }
            remove
            {
                foreach (var c in this._OnChangeUpdateDateDay.GetInvocationList())
                {
                    if (c.Equals(value))
                    {
                        this._OnChangeUpdateDateDay -= value;
                        return;
                    }
                }
            }
        }

        //!<==========================================================================
        public TimeManager()
        {
            this.PreviousNowTime = System.DateTime.Now;
        }
        public void Clear()
        {
            this._PrevCheckDay           = 0;
            this._OnChangeUpdateDateDay = null;
            this._OnChangeUpdateDateDay = (now) => { };
            this.PreviousNowTime    = System.DateTime.Now;
        }
    }
    //!<==============================================================================
   
}