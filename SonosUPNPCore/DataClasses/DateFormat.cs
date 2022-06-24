namespace SonosUPnP.DataClasses
{
    /// <summary>
    /// Klasse, die das Zeitformat hält
    /// </summary>
    public class DateFormat
    {
        private string _time = "24H";
        private string _date = "DMY";
        /// <summary>
        /// String 12H oder 24H
        /// </summary>
        public string Time
        {
            get
            {
                return _time;
            }
            set
            {
                if (value == "12H" || value == "24H")
                    _time = value;
                else
                    _time = "24H";
            }
        }
        /// <summary>
        /// Datumsformat Erlaubt: YMD, MDY, DMY
        /// </summary>
        public string Date
        {
            get
            {
                return _date;
            }
            set
            {
                if (value == "DMY" || value == "YMD" || value == "MDY")
                    _date = value;
                else
                    _date = "DMY";
            }
        }
    }
}
