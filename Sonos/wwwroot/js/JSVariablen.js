﻿"use strict";
//Liste mit Allen Sonos Variablen
function SonosVariablen() {
    this.apiDeviceURL = '/Devices/'; //Url der API der Geräte
    this.apiPlayerURL = '/Player/'; //URL der API des einzelnen Players
    this.apiZoneURL = '/Zone/'; //URL der API des einzelnen Players
    this.apiSettingURL = '/settings/'; //URL der API des einzelnen Players
    this.apiEventURL = '/event/';//URL der API für Events
    this.nocoverpfad = 'images/no-cover.png'; //URL von Bild, wenn kein Cover vorhanden
    this.LastEventID = -1;
    this.EventSourceDiscovery = "Discovery"; //kommt, wenn etwas global geändert wurde.
    this.aktcurpopdown = "leer"; //Hält die Info ob und welcher Song in der Wiedergabeliste das Cover etc. anzeigt
    this.browsefirst = 0; //Wurde das Browseing das erste mal geladen
    this.browseParentID = "A:"; //Dient dazu um beim Durchsuchen einen Pfad nach oben zu gehen
    this.RinconChecker = "RINCON";//Zum Prüfen von Object Properties
    this.ratingerrorsCount = 0; //Gab es beim Rating Fehler wird dies zum Steuern genommen.
    this.ratingerrorsList = 0; //Steuerung ob Fehlerinformationen angezeigt wurden
    this.ratingonlycurrent = false; //Nur noch vom aktuellen Song die Ratings anzeigen.
    if (typeof MP3 !== "undefined")
    this.ratingMP3 =  new MP3(); //Wird für das Rating genommen. Evtl. Änderungen werden dann an den Server gesendet.
    this.urldevice = ""; //Wenn über URL Parameter aufgerufen.
    this.playlistLoadError = 0;
    this.savePlaylistInputText = "Wiedergabeliste Speichern als..."; //Placeholder Text für Save/Export der Playlist
    this.exportPlaylistInputText = "Wiedergabeliste Exportieren als...";
    this.exportplaylist = false; //Status ob eine Wiedergabeliste gespeichert oder exportiert werden soll.
    this.updateMusikIndex = false; // Status ob der Musikindex aktualisiert wird. 
    this.setGroupMemberInitialisierung = false; //Prüft, ob die GruppenSet mitglieder neu geladen werden müssen.
    this.setGroupMemberInitSoftWareGen = 0;
    this.getanker = "leer"; //Beim Browsing gesetzt um den Sprunganker zu merken.
    this.getAnkerArt = "leer"; //Merkt sich, ob es sich um eine Interpreten oder eine Playlist handelt.
    this.groupDeviceShowBool = false; //Prüft, ob die DEvice Gruppierung angezeigt wird.
    this.masterPlayer =""; //enthält das Device, welcher für die Gruppenbildung an den Server geschickt wird.
    this.swindowlist = new Array(); //Liste aller Fenster SonosWindow
    this.szindex = 100; //Start z-Index SonosWindow
    this.overlayDVIObject = ""; //Object, welches das Overlay initialisiert hat SonosWindow
    this.selectetdivs = []; //Liste mit Objecten, die zusätzlich über dem Overlay liegen sollen.SonosWindow	
    this.TopologieChangeID = 0; //TopologieChange SetTimeout ProzessID, damit nicht doppelt Topologiechange gemacht wird.
    this.TopologieChangeTime = 10000; //TopologieChange SetTimeout Timer in MS
    this.GetAktSongInfoTimerID = 0; //GetAktsongInfo Timer ID
    this.metaUse = new Array("jahr", "genre", "pfad", "komponist", "verlag", "album", "typ", "kommentar"); //Propertys aus Currenttrack.mP3 die als Details angezeigt werden sollen.
    this.GlobalPlaylistLoaded = false;
    this.eventErrorChangeID = 0;// Sollten Fehler auftreten beim JSSonosEvent.js wird dieser Prozess immer und immer wieder aufgerufen, bis der Fehler weg ist, danach werden die Events wieder gestartet.
    this.eventErrorsSource = "";//Quelle, die den Fehler ursächlich gemeldet hat.
    this.currentplaylistScrolled = false; //Zeigt an, ob ein Anwender manuel gescrollt hat.
    this.volumeConfirmCounter = 20; //Wenn die Lautstärke erhöht wird, gibt es diesen Schwellenwert ab dem gefragt wird ob die Lautstärke wirklich erhöht werden soll. 
    this.VisibilityProperty = "unkowing";//entspricht der Property um zu prüfen, ob die Seite gesehen wird oder nicht.
    this.SSE_Event_Source; //die SSE Variable extern gelagert um die Verbindung zu schließen.
    this.aktiv = "aktiv" //wird für die CSS Klassen benutzt um dinge aktiv zu setzen.
}

//Globale Varibalen auf DOM Objekten
//Sonos DOM Elemente wird als SoDo bi Doc Ready initialisiert.
function SonosDOMObjects() {
    this.aktArtist = document.getElementById("Aktartist"); //Currenttrack Artist
    this.aktSongInfo = document.getElementById("AktSongInfo"); //Wrapper für aktartist, akttitle, currentmeta
    this.aktTitle = document.getElementById("Akttitle"); //Currenttrack Titel
    this.ankerlist = document.getElementById("Ankerlist"); //Beim Browsen die Buchstabenliste
    this.artistplSwitch = $(".artistplaylistonoffswitch-checkbox"); //Interpretenplaylist Schalter
    this.audioInButton = document.getElementById("AudioIn"); //Button um zu Zeigen, ob AudioIn verfügbar und ggf. aktiv bzw. aktivieren.
    this.aufweckenSwitch = $(".aufweckenonoffswitch-checkbox"); //AufweckenPlaylist Schalter
    this.bewertungWidth = document.getElementById("BewertungL"); // beim Currenttrack am Cover die Bewertungsbreite (goldene Sterne)
    this.bewertungStars = document.getElementById("BewertungN"); // beim Currenttrack am Cover die Bewertung (graue Sterne)
    this.bodydiv = $("#Bodydiv"); //Primärer Container in dem alles hinterlegt ist. //todo: Alarm
    this.browseBackButton = document.getElementById("Browseback"); //beim Browsing der Back Button
    this.browseButton = document.getElementById("Browse"); //Button um die Browsebox zu öffnen
    this.browse = document.getElementById("Browsebox"); //Browsebox, die alles fürs Brwosing hält, wie ankerlist, browseloader, warpper etc.
    this.browseWrapper = document.getElementById("Browseboxwrapper"); //Wrapper mit der Ergebnisliste.
    this.browseLoader = document.getElementById("BrowseLoader"); //Loader beim Browse
    this.cover = document.getElementById("Cover"); //Currentcover
    this.currentBomb = $("#CurrentBomb"); //Currentbomb
    this.currentMeta = $("#CurrentMeta"); //Zeigt mit klick die aktuellen Meta Daten eines Songs an.
    this.currentplaylistwrapper = $("#Currentplaylistwrapper"); //Wrapper für die current Playlist
    this.currentplaylist = $("#Currentplaylist");
    this.currentplaylistclose = $("#CurrentPlaylistClose");
    this.deviceClass = $(".device");//Muss später initialisiert werden, weil es noch keine Klassen gibt.
    this.deviceLoader = $("#Loader"); //Loader für die Devices
    this.devices = $("#Devices"); //Oberste Box für die Player und Zonen
    this.devicesWrapper = $("#DeviceWrapper"); //Wrapper der Player/Zonen
    this.errorlogging = $('<DIV id="Showerrorloging"><DIV id ="ShowerrorlogingWrapper"></DIV></DIV>'); //DIV für das Errorlogging
    this.errorloggingDOM = $('<DIV id="ShowerrorlogingButton" class="mediabuttonring">Log</DIV>'); //DIV für das Errorlogging
    this.errorloggingwrapper = $('#ShowerrorlogingWrapper');//DIV für das Errorlogging
    this.eventError = $("#EventErrorDiv");//Wird benutzt, wenn Fehler bei der Server Kommunikation auftreten.
    this.fadeButton = document.getElementById("Fade"); //Fade Mediabutton
    this.filterListBox = $("#Filterlist"); //Filter Box
    this.filterListRatingBar = $("#Filterlist .rating_bar"); //Rating in der FIlter Box
    this.filterListStimmungChilds = $("#FilterStimmungendiv > DIV");
    this.filterListGelegenheitChilds = $("#FilterGelegenheitendiv > DIV");
    this.filterListGeschwindigkeitChilds = $("#FilterGeschwindigkeitendiv > DIV");
    this.filterListAlbumInterpretChilds = $("#Artistplaylistfilter > DIV");
    this.filterListRatingBarBomb = $("#filter_rating_bar_bomb");
    this.filterListButton = $("#Bewertungsfilter"); //Button im Settings für die Filter der Bewertungen.
    this.gelegenheitenChildren = $("#Gelegenheitendiv > DIV");
    this.globalPlaylistLoader = $("#GlobalPlaylistLoader"); //Loader für die gespeicherten Wiedergabelisten
    this.groupDeviceShow = $("#GroupDeviceShow"); //Button um die Gruppierung zu aktivieren und einzelne Player zu Pausieren bzw. abzuspielen.
    this.geschwindigkeitChildren = $("#Geschwindigkeitendiv > DIV");
    this.labelVolume = $("#LabelVolume"); //Label unterhalb des Volume Sliders um die Lautstärke anzuzeigen.
    this.lyricButton = document.getElementById("Lyric"); //LyricButton
    this.lyric = document.getElementById("Lyricbox"); //Lyric Box
    this.lyricWrapper = document.getElementById("Lyricboxwrapper");//Wrapper in der Lyric Box
    this.lyricsPlaylist = document.getElementById("LyricPlaylist"); //Lyric Box aus Playlist
    this.multiVolume = $("#MultiVolume"); //Box mit der Lautstärke, falls mehr als ein Player in Zone
    this.musikIndex = $("#MusikIndex"); //Musik Index Box
    this.musikIndexCheck = $("#MIUCheck"); //Gibt Feedback, wenn Index Komplett
    this.musikIndexLoader = $("#MusikIndexLoader"); //Loader für den Musikindex
    this.muteButton = document.getElementById("SetMute");//Mediabutton
    this.nextButton = document.getElementById("Next");//Mediabutton
    this.playButton = document.getElementById("Play"); //Mediabutton
    this.prevButton = document.getElementById("Pre");//Mediabutton
    this.nextcover = document.getElementById("Nextcover"); //Nextsong Cover
    this.nextSongWrapper = document.getElementById("NextSongWrapper"); //Warpper für den Netxsong
    this.nextTitle = document.getElementById("Nextsong"); //titel für den NextSong
    this.onlyCurrentSwitch = $(".curonoffswitch-checkbox"); //Nur Currentrating anzeigen?
    this.overlay = document.getElementById("Overlay"); //Wenn beim SonosWindows ein Overlay benötigt wird.
    this.playlistCount = document.getElementById("PlaylistCount");//Playlist Box
    this.playlistAkt = document.getElementById("PlaylistCountAkt") //Aktuelle Titel Nummer
    this.playlistTotal = document.getElementById("PlaylistCountTotal");//Gesamtanzahl
    this.playlistLoader = document.getElementById("PlaylistLoader"); //Loader der currentplaylist
    this.playlistwrapper = $("#Playlistwrapper"); //Wrapper beim Container für alle Playlisten
    this.ratingCheck = $("#RatingCheck"); //Feedback, wenn Rating erfolgreich durchgeführt
    this.ratingErrorList = $("#RatingerrorsList"); //Wenn beim Rateing ein Fehlerauftritt auf dem Server, dann wird diese liste Gefüllt.
    this.ratingErrors = $("#Ratingerrors"); //Box mit der Liste der Errors.
    this.ratingBomb = $("#rating_id_bomb");//bombe in der Ratingliste
    this.ratingListBox = $("#Ratinglist"); //Liste für die Bewertung
    this.ratingListRatingBar = $("#Ratinglist .rating_bar"); //Aus der Ratingliste die Rating Bar
    this.ratingMineSelector = $("#RatingMineSelector"); //Selection für Mine Rating
    this.repeatButton = document.getElementById("Repeat");//Mediabutton
    this.runtimeCurrentSong = $("#CurrentSongRuntime"); //Box mit der Laufzeit
    this.runtimeRelTime = $("#CurrentSongRuntimeRelTime"); // Abgelaufene Zeit
    this.runtimeDuration = $("#CurrentSongRuntimeDuration"); //Gesamtzeit
    this.runtimeSlider = $("#Slider"); //Slider für die Laufzeit
    this.saveExportPlaylistSwitch = $(".onoffswitch-checkbox"); //Schalter der definiert ob Playlisten Exportiert oder gespeichert werden sollen.
    this.saveQueue = $("#SaveQueue"); //Inputfeld mit dem Namen der Wiedergabeliste
    this.saveQueueLoader = $("#SaveQueueLoader"); //Animation, wenn Playlist gespeichert bzw. exportiert werden soll.
    this.setGroupMembers = $("#SetGroupMembers"); //Box mit allen Playern um auszuwählen, wer mit wem zusammengehört.
    this.settingsBox = document.getElementById("Settingsbox"); //Inhalt der Settings DIV
    this.settingsbutton = document.getElementById("Settings"); //Settingsbutton
    this.settingsClosebutton = document.getElementById("Settingsclose"); //Settingsbutton
    this.BrowseClosebutton = document.getElementById("Browseclose");
    this.shuffleButton = document.getElementById("Shuffle");//Mediabutton
    this.sleepMode = document.getElementById("SleepMode"); //Box für Sleepmode
    this.sleepModeButton = document.getElementById("SleepModeButton"); //Button Sleepmode
    this.sleepModeSelection = document.getElementById("SleepModeSelection"); //Auswahl mit Zeiten
    this.sleepModeState = document.getElementById("SleepModeState"); //Text mit dem Status
    this.suggestionInput = $('#Suggestion'); //Enthält das Inputfeld, welches den Namen der zu speichernden/exportierenden Playlist enthält
    this.stimmungenChildren = $("#Stimmungendiv > DIV"); //Stimmungs DIV Kinder Elemente
    this.volumeSlider = $("#Volume");//Slider mit der Lautstärke
    this.debug = document.getElementById("Debug");//Debug Button
    //Wenn Fehler als Log angezeigt werden sollen.
    this.SetErrorLogging = function() {
        this.errorlogging.appendTo(this.bodydiv);
        this.errorloggingDOM.appendTo(this.bodydiv);
    };
}
//} Variablen Verarbeitung