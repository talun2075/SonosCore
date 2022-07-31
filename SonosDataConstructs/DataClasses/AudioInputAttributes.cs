namespace SonosData.DataClasses
{
    public class AudioInputAttributes
    {
        public AudioInputAttributes() { }
        /// <summary>
        /// Konstruktor
        /// </summary>
        /// <param name="currentName"></param>
        /// <param name="currentIcon"></param>
        public AudioInputAttributes(string currentName, string currentIcon)
        {
            CurrentName = currentName;
            CurrentIcon = currentIcon;
        }
        /// <summary>
        /// Name des Audioeingangs
        /// </summary>
        public string CurrentName { get; set; } = "";
        /// <summary>
        /// Name des verwendetend Icons für den Audio Eingang
        /// </summary>
        public string CurrentIcon { get; set; } = "";
    }
}
