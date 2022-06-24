using System;
using MP3File;
namespace SonosUPnP
{
    /// <summary>
    /// Klasse, die jedem Player zugefügt wird um zu prüfen, ob für diesen Player ein Filter beim Rating eingestellt wurde.
    /// </summary>
    public class SonosRatingFilter
    {
        private const int unsetRating = -2;
        private const string unset = "unset";
        private int _rating = unsetRating;
        private string _albpumInterpretFilter = unset;
        private Enums.Stimmung _stimmung = Enums.Stimmung.unset;
        private Enums.Gelegenheit _gelegenheit = Enums.Gelegenheit.unset;
        private Enums.Geschwindigkeit _geschwindigkeit = Enums.Geschwindigkeit.unset;
        public event EventHandler<SonosRatingFilter> RatingFilter_Changed = delegate { };
        /// <summary>
        /// Das gesetzte Rating
        /// </summary>
        public int Rating
        {
            get { return _rating; }
            set
            {
                _rating = value;
                RatingFilter_Changed("Rating", this);
            }
        }
        /// <summary>
        /// Wurde der AlbumInterpretFilter gesetzt
        /// </summary>
        public string AlbpumInterpretFilter
        {
            get { return _albpumInterpretFilter; }
            set
            {
                _albpumInterpretFilter = value;
                RatingFilter_Changed("AlbpumInterpretFilter", this);
            }
        }
        /// <summary>
        /// Filter für die Stimmung eines einzelnes Liedes.
        /// </summary>
        public Enums.Stimmung Stimmung
        {
            get { return _stimmung; }
            set
            {
                _stimmung = value;
                RatingFilter_Changed("Stimmung", this);
            }
        }
        /// <summary>
        /// Filter für die Gelegenheit eines einzelnes Liedes.
        /// </summary>
        public Enums.Gelegenheit Gelegenheit
        {
            get { return _gelegenheit; }
            set
            {
                _gelegenheit = value;
                RatingFilter_Changed("Gelegenheit", this);
            }
        }
        /// <summary>
        /// Filter für die Geschwindigkeit eines einzelnes Liedes.
        /// </summary>
        public Enums.Geschwindigkeit Geschwindigkeit
        {
            get { return _geschwindigkeit; }
            set
            {
                _geschwindigkeit = value;
                RatingFilter_Changed("Geschwindigkeit", this);
            }
        }
        /// <summary>
        /// Prüft ob änderungen gemacht wurden
        /// </summary>
        public Boolean IsDefault
        {
            get
            {
                if (Rating == unsetRating && AlbpumInterpretFilter == unset && Stimmung == Enums.Stimmung.unset && Gelegenheit == Enums.Gelegenheit.unset && Geschwindigkeit == Enums.Geschwindigkeit.unset)
                    return true;

                return false;
            }
        }
        /// <summary>
        /// Prüft sich selbst nach erlaubten Werten.
        /// </summary>
        public Boolean IsValid
        {
            get
            {
                if (Rating < -2 || Rating > 100)
                    return false;
                return AlbpumInterpretFilter == unset || AlbpumInterpretFilter == "true" || AlbpumInterpretFilter == "false";
            }
        }

        /// <summary>
        /// Prüft, ob der Song den Filter kriterien entspricht.
        /// </summary>
        /// <param name="mp3"></param>
        /// <returns>Entspricht der Ratingfilter</returns>
        public Boolean CheckSong(MP3File.MP3File mp3)
        {
            //Rating
            if (Rating != unsetRating && Convert.ToInt16(mp3.Bewertung) < Rating)
                return false;
            //AlbumInterpret
            if (AlbpumInterpretFilter != unset && AlbpumInterpretFilter != mp3.ArtistPlaylist.ToString().ToLower())
                return false;
            if (Gelegenheit != Enums.Gelegenheit.unset && Gelegenheit != mp3.Gelegenheit)
                return false;
            if (Stimmung != Enums.Stimmung.unset && Stimmung != mp3.Stimmung)
                return false;
            if (Geschwindigkeit != Enums.Geschwindigkeit.unset && Geschwindigkeit != mp3.Geschwindigkeit)
                return false;
            return true;
        }
        /// <summary>
        /// Prüft, ob der übergeben gleich ist
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public Boolean CheckSonosRatingFilter(SonosRatingFilter v)
        {
            if (Rating != v.Rating)
                return false;
            if (AlbpumInterpretFilter != v.AlbpumInterpretFilter)
                return false;
            if (Stimmung != v.Stimmung)
                return false;
            if (Gelegenheit != v.Gelegenheit)
                return false;
            if (Geschwindigkeit != v.Geschwindigkeit)
                return false;

            return true;
        }


    }
}