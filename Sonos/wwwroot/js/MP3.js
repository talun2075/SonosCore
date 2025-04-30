function MP3() {
    this.album = "leer";
    this.artist = "leer";
    this.artistPlaylist = false;
    this.aufwecken = false;
    this.bewertung = 0;
    this.bewertungMine = 0;
    this.bewertungIan = 0;
    this.bewertungFinn = 0;
    this.gelegenheit = "None";
    this.genre = "leer";
    this.geschwindigkeit = "None";
    this.hatCover = false;
    this.jahr = 0;
    this.komponist = "leer";
    this.kommentar = "leer";
    this.laufzeit = "leer";
    this.lyric = "leer";
    this.pfad = "leer";
    this.stimmung = "None";
    this.titel = "leer";
    this.tracknumber = 0;
    this.typ = "leer";
    this.verarbeitungsFehler = false;
    this.verlag = "leer";
    this.FillFromMP3 = function (mp3) {
        this.gelegenheit = mp3.gelegenheit;
        this.geschwindigkeit = mp3.geschwindigkeit;
        this.stimmung = mp3.stimmung;
        this.aufwecken = mp3.aufwecken;
        this.artistPlaylist = mp3.artistPlaylist;
        this.bewertung = mp3.bewertung;
        this.bewertungMine = mp3.bewertungMine;
        this.bewertungIan = mp3.bewertungIan;
        this.bewertungFinn = mp3.bewertungFinn;
    }
    this.FillServerMP3fromThis = function (mp3) {
        mp3.gelegenheit = this.gelegenheit;
        mp3.geschwindigkeit = this.geschwindigkeit;
        mp3.stimmung = this.stimmung;
        mp3.aufwecken = this.aufwecken;
        mp3.artistPlaylist = this.artistPlaylist;
        mp3.bewertung = this.bewertung;
        mp3.bewertungMine = this.bewertungMine;
        mp3.bewertungIan = this.bewertungIan;
        mp3.bewertungFinn = this.bewertungFinn;
    }
}