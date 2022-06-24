using System;


namespace SonosUPNPCore.Props
{
    public class SonosTimeSpan
    {
        private readonly TimeSpan _internalTimeSpan;
        #region ctor
        public SonosTimeSpan()
        {
            _internalTimeSpan = TimeSpan.Zero;
        }
        public SonosTimeSpan(TimeSpan timeSpan)
        {
            _internalTimeSpan = timeSpan;
        }

        public SonosTimeSpan(string timeSpan)
        {
            if (!TimeSpan.TryParse(timeSpan, out _internalTimeSpan))
            {
                throw new ArgumentException("Can not Parse String to TimeSpan", nameof(timeSpan));
            };
        }
        #endregion ctor
        #region Props
        public int Hours => _internalTimeSpan.Hours;
        public int Minutes => _internalTimeSpan.Minutes;
        public int Seconds => _internalTimeSpan.Seconds;
        public int Milliseconds => _internalTimeSpan.Milliseconds;
        public long Ticks => _internalTimeSpan.Ticks;
        public int Days => _internalTimeSpan.Days;
        public double TotalDays => _internalTimeSpan.TotalDays;
        public double TotalHours => _internalTimeSpan.TotalHours;
        public double TotalMilliseconds => _internalTimeSpan.TotalMilliseconds;
        public double TotalMinutes => _internalTimeSpan.TotalMinutes;
        public double TotalSeconds => _internalTimeSpan.TotalSeconds;
        public TimeSpan TimeSpan => _internalTimeSpan;
        public String String => _internalTimeSpan.ToString();
        public String StringWithoutZeroHours => GetStringWithoutZeroHours();
        public Boolean IsZero => _internalTimeSpan == TimeSpan.Zero;
        #endregion Props
        #region Public Methods
        public override String ToString()
        {
            return _internalTimeSpan.ToString();
        }
        #endregion Public Methods

        private String GetStringWithoutZeroHours()
        {
            if (_internalTimeSpan.TotalHours > 1)
                return _internalTimeSpan.ToString();

            String sec = String.Empty;
            String min = String.Empty;
            if (_internalTimeSpan.Seconds <10)
                sec = "0";
            sec += _internalTimeSpan.Seconds.ToString();
            if (_internalTimeSpan.Minutes < 10)
                min = "0";
            min += _internalTimeSpan.Minutes.ToString();
            return min + ":" + sec;
        }
    }
}
