"use strict";
//Einzelner SonosPlayer
function SonosPlayer(_uuid, _name,_swgen) {
    try {
        this.name = _name;
        this.uuid = _uuid;
        this.LastChange = new Date();
        this.SoftwareGeneration = _swgen;
        this.GetPlayerChangeEventIsRunning = false;
        this.CheckActiveZone = function () {
            try {
                return this.uuid === SonosZones.ActiveZoneUUID;
            }
            catch (fehlernachricht) {
                alert(fehlernachricht + " Fehler bei:" + fehlernachricht.fileName + "\n" + "Meldung:" + fehlernachricht.message + "\n" + "Zeile:" + fehlernachricht.lineNumber);
            }
        };
        this.CheckPlayerProperties = function (pp) {
            try {
                var renderdevices = false;
                var init = false;
                if (typeof this.playerProperties === "undefined") {
                    this.playerProperties = {};
                    init = true;
                }
                if (!this.playerProperties.avTransportURI || this.playerProperties.avTransportURI !== pp.avTransportURI)
                    this.playerProperties.avTransportURI = pp.avTransportURI;
                if (!this.playerProperties.alarmRunning || this.playerProperties.alarmRunning !== pp.alarmRunning)
                    this.playerProperties.alarmRunning = pp.alarmRunning;
                if (!this.playerProperties.audioInput_Icon || this.playerProperties.audioInput_Icon !== pp.audioInput_Icon)
                    this.playerProperties.audioInput_Icon = pp.audioInput_Icon;
                if (!this.playerProperties.audioInput_LeftLineInLevel || this.playerProperties.audioInput_LeftLineInLevel !== pp.audioInput_LeftLineInLevel)
                    this.playerProperties.audioInput_LeftLineInLevel = pp.audioInput_LeftLineInLevel;
                if (!this.playerProperties.audioInput_LineInConnected || this.playerProperties.audioInput_LineInConnected !== pp.audioInput_LineInConnected)
                    this.playerProperties.audioInput_LineInConnected = pp.audioInput_LineInConnected;
                if (!this.playerProperties.audioInput_Name || this.playerProperties.audioInput_Name !== pp.audioInput_Name)
                    this.playerProperties.audioInput_Name = pp.audioInput_Name;
                if (!this.playerProperties.audioInput_RightLineInLevel || this.playerProperties.audioInput_RightLineInLevel !== pp.audioInput_RightLineInLevel)
                    this.playerProperties.audioInput_RightLineInLevel = pp.audioInput_RightLineInLevel;
                if (!this.playerProperties.baseUrl || this.playerProperties.baseUrl !== pp.baseUrl)
                    this.playerProperties.baseUrl = pp.baseUrl;
                if (!this.playerProperties.bass || this.playerProperties.Bass !== pp.bass)
                    this.playerProperties.bass = pp.bass;
                if (this.playerProperties.currentCrossFadeMode !== pp.currentCrossFadeMode) {
                    this.playerProperties.currentCrossFadeMode = pp.currentCrossFadeMode;
                    if (this.CheckActiveZone()) {
                        SonosZones.RenderCrossFadeMode(this.uuid);
                    }
                }
                if (!this.playerProperties.currentPlayMode || this.playerProperties.currentPlayMode !== pp.currentPlayMode)
                    this.playerProperties.currentPlayMode = pp.currentPlayMode;
                if (!this.playerProperties.currentPlayModeString || this.playerProperties.currentPlayModeString !== pp.currentPlayModeString) {
                    this.playerProperties.currentPlayModeString = pp.currentPlayModeString;
                    if (this.CheckActiveZone()) {
                        SonosZones.RenderPlayMode(SonosZones.ActiveZoneUUID);
                    }
                }
                if (!this.playerProperties.currentTrackNumber || this.playerProperties.currentTrackNumber !== pp.currentTrackNumber) {
                    this.playerProperties.currentTrackNumber = pp.currentTrackNumber;
                    if (this.CheckActiveZone()) {
                        SonosZones.RenderPlaylistCounter(this.uuid);
                    }
                }
                if (!this.playerProperties.deviceProperties_AirPlayEnabled || this.playerProperties.deviceProperties_AirPlayEnabled !== pp.deviceProperties_AirPlayEnabled)
                    this.playerProperties.deviceProperties_AirPlayEnabled = pp.deviceProperties_AirPlayEnabled;
                if (!this.playerProperties.deviceProperties_Icon || this.playerProperties.deviceProperties_Icon !== pp.deviceProperties_Icon)
                    this.playerProperties.deviceProperties_Icon = pp.deviceProperties_Icon;
                if (!this.playerProperties.deviceProperties_Invisible || this.playerProperties.deviceProperties_Invisible !== pp.deviceProperties_Invisible)
                    this.playerProperties.deviceProperties_Invisible = pp.deviceProperties_Invisible;
                if (!this.playerProperties.deviceProperties_IsIdle || this.playerProperties.deviceProperties_IsIdle !== pp.deviceProperties_IsIdle)
                    this.playerProperties.deviceProperties_IsIdle = pp.deviceProperties_IsIdle;
                if (!this.playerProperties.deviceProperties_IsZoneBridge || this.playerProperties.deviceProperties_IsZoneBridge !== pp.deviceProperties_IsZoneBridge)
                    this.playerProperties.deviceProperties_IsZoneBridge = pp.deviceProperties_IsZoneBridge;
                if (!this.playerProperties.deviceProperties_SupportsAudioIn || this.playerProperties.deviceProperties_SupportsAudioIn !== pp.deviceProperties_SupportsAudioIn)
                    this.playerProperties.deviceProperties_SupportsAudioIn = pp.deviceProperties_SupportsAudioIn;
                if (!this.playerProperties.deviceProperties_WifiEnabled || this.playerProperties.deviceProperties_WifiEnabled !== pp.deviceProperties_WifiEnabled)
                    this.playerProperties.deviceProperties_WifiEnabled = pp.deviceProperties_WifiEnabled;
                if (!this.playerProperties.deviceProperties_ZoneName || this.playerProperties.deviceProperties_ZoneName !== pp.deviceProperties_ZoneName)
                    this.playerProperties.deviceProperties_ZoneName = pp.deviceProperties_ZoneName;
                if (!this.playerProperties.enqueuedTransportURI || this.playerProperties.enqueuedTransportURI !== pp.enqueuedTransportURI)
                    this.playerProperties.enqueuedTransportURI = pp.enqueuedTransportURI;
                if (!this.playerProperties.enqueuedTransportURIMetaData || JSON.stringify(this.playerProperties.enqueuedTransportURIMetaData) !== JSON.stringify(pp.enqueuedTransportURIMetaData))
                    this.playerProperties.enqueuedTransportURIMetaData = pp.enqueuedTransportURIMetaData;
                if (!this.playerProperties.groupCoordinatorIsLocal || this.playerProperties.groupCoordinatorIsLocal !== pp.groupCoordinatorIsLocal) {
                    this.playerProperties.groupCoordinatorIsLocal = pp.groupCoordinatorIsLocal;
                    renderdevices = true;
                }
                if (!this.playerProperties.groupManagement_ResetVolumeAfter || this.playerProperties.groupManagement_ResetVolumeAfter !== pp.groupManagement_ResetVolumeAfter)
                    this.playerProperties.groupManagement_ResetVolumeAfter = pp.groupManagement_ResetVolumeAfter;
                if (!this.playerProperties.groupRenderingControl_GroupMute || this.playerProperties.groupRenderingControl_GroupMute !== pp.groupRenderingControl_GroupMute)
                    this.playerProperties.groupRenderingControl_GroupMute = pp.groupRenderingControl_GroupMute;
                if (!this.playerProperties.groupRenderingControl_GroupVolume || this.playerProperties.groupRenderingControl_GroupVolume !== pp.groupRenderingControl_GroupVolume) {
                    this.playerProperties.groupRenderingControl_GroupVolume = pp.groupRenderingControl_GroupVolume;
                    if (this.CheckActiveZone()) {
                        SonosZones.RenderVolume(this.uuid);
                    }
                }
                if (!this.playerProperties.groupRenderingControl_GroupVolumeChangeable || this.playerProperties.groupRenderingControl_GroupVolumeChangeable !== pp.groupRenderingControl_GroupVolumeChangeable)
                    this.playerProperties.groupRenderingControl_GroupVolumeChangeable = pp.groupRenderingControl_GroupVolumeChangeable;
                if (!this.playerProperties.headphoneConnected || this.playerProperties.headphoneConnected !== pp.headphoneConnected)
                    this.playerProperties.headphoneConnected = pp.headphoneConnected;
                if (!this.playerProperties.icon || this.playerProperties.icon !== pp.icon)
                    this.playerProperties.icon = pp.icon;
                if (!this.playerProperties.localGroupUUID || this.playerProperties.localGroupUUID !== pp.localGroupUUID) {
                    this.playerProperties.localGroupUUID = pp.localGroupUUID;
                    renderdevices = true;
                }
                if (!this.playerProperties.loudness || this.playerProperties.loudness !== pp.loudness)
                    this.playerProperties.loudness = pp.loudness;
                if (!this.playerProperties.mute || this.playerProperties.mute !== pp.mute)
                    this.playerProperties.mute = pp.mute;
                if (!this.playerProperties.playlist || this.playerProperties.playlist.numberOfTracks !== pp.playlist.numberOfTracks && this.playerProperties.playlist.totalMatches !== pp.playlist.totalMatches && this.playerProperties.playlist.playListItems.length !== pp.playlist.playListItems.length) {
                    this.playerProperties.playlist = pp.playlist;
                    if (this.CheckActiveZone()) {
                        if (this.playlist.CheckToRender(this))
                            //this.playlist.RenderPlaylist(this, this.currentTrack.stream);
                            SonosZones.RenderPlaylist(this.uuid);
                    }
                }
                if (!this.playerProperties.numberOfTracks || this.playerProperties.numberOfTracks !== pp.numberOfTracks) {
                    this.playerProperties.numberOfTracks = pp.numberOfTracks;
                    if (this.playerProperties.numberOfTracks !== this.playerProperties.playlist.playListItems.length) {
                        this.playlist.LoadPlaylist(this, false, "CheckPlayerProperties");
                    }
                    if (this.CheckActiveZone()) {
                        SonosZones.RenderPlaylistCounter(this.uuid);
                    }
                }
                if (!this.playerProperties.nextTrack || this.playerProperties.nextTrack !== pp.nextTrack) {
                    this.playerProperties.nextTrack = pp.nextTrack;
                    if (this.CheckActiveZone()) {
                        SonosZones.RenderNextTrack(this.uuid);
                    }
                }
                if (!this.playerProperties.remainingSleepTimerDuration || this.playerProperties.remainingSleepTimerDuration !== pp.remainingSleepTimerDuration) {
                    this.playerProperties.remainingSleepTimerDuration = pp.remainingSleepTimerDuration;
                    if (this.CheckActiveZone()) {
                        SonosZones.RenderSleepTimer(this.uuid);
                    }
                }
                if (!this.playerProperties.sleepTimerRunning || this.playerProperties.sleepTimerRunning !== pp.sleepTimerRunning)
                    this.playerProperties.sleepTimerRunning = pp.sleepTimerRunning;
                if (!this.playerProperties.snoozeRunning || this.playerProperties.snoozeRunning !== pp.snoozeRunning)
                    this.playerProperties.snoozeRunning = pp.snoozeRunning;
                if (!this.playerProperties.transportState || this.playerProperties.transportState !== pp.transportState)
                    this.playerProperties.transportState = pp.transportState;
                if (!this.playerProperties.transportStateString || this.playerProperties.transportStateString !== pp.transportStateString) {
                    this.playerProperties.transportStateString = pp.transportStateString;
                    if (this.CheckActiveZone()) {
                        SonosZones.RenderTransportState(this.uuid);
                    }
                }
                if (!this.playerProperties.volume || this.playerProperties.volume !== pp.volume) {
                    this.playerProperties.volume = pp.volume;
                    if (this.CheckActiveZone()) {
                        SonosZones.RenderVolume(this.uuid);
                    }
                }
                if (!this.playerProperties.zoneGroupTopology_ZonePlayerUUIDsInGroup || JSON.stringify(this.playerProperties.zoneGroupTopology_ZonePlayerUUIDsInGroup) !== JSON.stringify(pp.zoneGroupTopology_ZonePlayerUUIDsInGroup)) {

                    this.playerProperties.zoneGroupTopology_ZonePlayerUUIDsInGroup = pp.zoneGroupTopology_ZonePlayerUUIDsInGroup;
                    renderdevices = true;
                }
                if (!this.playerProperties.currentTrack || this.playerProperties.currentTrack !== pp.currentTrack) {
                    this.playerProperties.currentTrack = pp.currentTrack;
                    if (this.CheckActiveZone()) {
                        this.RenderCurrentTrack();
                    }
                }
                if (renderdevices && !init) {
                    SonosZones.RenderDevices();
                }
            }
            catch (fehlernachricht) {
                alert(fehlernachricht + " Fehler bei:" + fehlernachricht.fileName + "\n" + "Meldung:" + fehlernachricht.message + "\n" + "Zeile:" + fehlernachricht.lineNumber);
            }
        };
        this.CheckRatingFilter = function (s) {
            //todo. wird irgendwo benutzt?
        };
        this.ChangeRatingFilter = function (typ, wert) {
            try {
                //nimmt den übergeben Wert und setzt diesen an den Server sowie im Objekt hier.
                var changed = false;
                switch (typ) {
                    case "Reset":
                        this.RatingFilter.rating = -2;
                        this.RatingFilter.stimmung = 6;
                        this.RatingFilter.geschwindigkeit = 6;
                        this.RatingFilter.gelegenheit = 5;
                        this.RatingFilter.albpumInterpretFilter = "unset";
                        changed = true;
                        break;
                    case "Rating":
                        if (this.RatingFilter.rating !== wert) {
                            this.RatingFilter.rating = wert;
                            changed = true;
                        }
                        break;
                    case "Stimmung":
                        if (this.RatingFilter.stimmung !== wert) {
                            this.RatingFilter.stimmung = wert;
                            changed = true;
                        }
                        break;
                    case "Geschwindigkeit":
                        if (this.RatingFilter.geschwindigkeit !== wert) {
                            this.RatingFilter.geschwindigkeit = wert;
                            changed = true;
                        }
                        break;
                    case "Gelegenheit":
                        if (this.RatingFilter.gelegenheit !== wert) {
                            this.RatingFilter.gelegenheit = wert;
                            changed = true;
                        }
                        break;
                    case "AlbpumInterpretFilter":
                        if (this.RatingFilter.albpumInterpretFilter !== wert) {
                            this.RatingFilter.albpumInterpretFilter = wert;
                        } else {
                            this.RatingFilter.albpumInterpretFilter = "unset";
                        }
                        changed = true;
                        break;
                }
                if (changed === true) {
                    if (this.uuid === SonosZones.ActiveZoneUUID) {
                        SonosZones.RenderRatingFilter(this.uuid);
                        SonosAjax("SetRatingFilter", this.RatingFilter);
                    }

                }
            }
            catch (fehlernachricht) {
                alert(fehlernachricht + " Fehler bei:" + fehlernachricht.fileName + "\n" + "Meldung:" + fehlernachricht.message + "\n" + "Zeile:" + fehlernachricht.lineNumber);
            }
        };
        this.SendTransportState = function (value) {
            try {
                var t = this;
                if (value === "PLAYING") {
                    if (this.GetPlayerChangeEventIsRunning === false) {
                        //Ajax Request
                        var request = SonosAjax("Play", "", this.uuid);
                        request.catch(function () {
                            ReloadSite("SonosZone:SetPlayState:Play");
                        }).then(function () {
                            SonosPlayers[t.uuid].playerProperties.transportStateString = "PLAYING";
                            SonosZones.RenderTransportState(t.uuid);
                        });
                    }
                } else {
                    if (this.GetPlayerChangeEventIsRunning === false) {
                        //Ajax Request
                        var request2 = SonosAjax("Pause", "", this.uuid);
                        request2.catch(function () {
                            ReloadSite("SonosZone:SetPlayState:Pause");
                        }).then(function () {
                            SonosPlayers[t.uuid].playerProperties.transportStateString = "PAUSED_PLAYBACK";
                            SonosZones.RenderTransportState(t.uuid);
                        });
                    }
                }
            }
            catch (fehlernachricht) {
                alert(fehlernachricht + " Fehler bei:" + fehlernachricht.fileName + "\n" + "Meldung:" + fehlernachricht.message + "\n" + "Zeile:" + fehlernachricht.lineNumber);
            }
        };//done
        this.RenderCurrentTrack = function (retry) {
            //prüfen ob track gesetzt ist ansonsten laden.
            var curt = this.playerProperties.currentTrack;
            var uuid = this.uuid;
            if (curt.isEmpty === true) {
                this.HideCurrentTrack();
                if (retry === true) return;
                SonosAjax("GetAktSongInfo").then(function (data) { SonosPlayers[uuid].playerProperties.currentTrack = data; SonosPlayers[uuid].RenderCurrentTrack(true); });
                return;
            }
            //CurrentRatingBox
            if (IsVisible(SoDo.ratingListBox)) {
                ShowCurrentRating();
            }
            

            //Ende
            var etu = this.playerProperties.enqueuedTransportURIMetaData
            if (curt.stream === false && SonosZones.CheckStringIsNullOrEmpty(curt.artist) && SonosZones.CheckStringIsNullOrEmpty(curt.album) && SonosZones.CheckStringIsNullOrEmpty(curt.albumArtURI)) {
                this.HideCurrentTrack();
                return;
            }
            if (curt.stream === false || (curt.stream === true && (etu.classType !== "object.item.audioItem.audioBroadcast") && !etu.protocolInfo.startsWith("http-get"))) {
                this.ShowCurrentTrack();
                //AlbumCover
                //todo: Prüfen für Eingang
                if (!SonosZones.CheckStringIsNullOrEmpty(curt.albumArtURI)) {
                    var jack = "/images/35klinke.png";
                    var albumart = 'http://' + this.playerProperties.baseUrl + curt.albumArtURI;
                    if (!curt.albumArtURI.startsWith("/getaa") || curt.albumArtURI == jack) {
                        albumart = curt.albumArtURI;
                    }
                    if (SoDo.cover.getAttribute("src") !== albumart) {
                        SoDo.cover.setAttribute("src", albumart);
                    }
                    UpdateImageOnErrors();
                } else {
                    if (SoDo.cover.getAttribute("src") !== SoVa.nocoverpfad) {
                        SoDo.cover.setAttribute("src", SoVa.nocoverpfad);
                    }
                }
            } else {
                //Radio keine Bewertung und Titel anders definieren.
                if (etu.classType === "object.item.audioItem.audioBroadcast" || etu.protocolInfo.startsWith("http-get")) {
                    if (!SonosZones.CheckStringIsNullOrEmpty(this.playerProperties.avTransportURI)) {
                        var albumart = 'http://' + this.playerProperties.baseUrl + "/getaa?s=1&u=" + this.playerProperties.avTransportURI;
                        if (etu.protocolInfo.startsWith("http-get")) {
                            albumart = this.playerProperties.currentTrack.albumArtURI;
                        } else if (SonosZones.CheckStringIsNullOrEmpty(etu.protocolInfo)) {
                            albumart = 'http://' + this.playerProperties.baseUrl + this.playerProperties.currentTrack.albumArtURI;
                        }
                        if (SoDo.cover.getAttribute("src") !== albumart) {
                            SoDo.cover.setAttribute("src", albumart);
                        }
                        UpdateImageOnErrors();
                    } else {
                        if (SoDo.cover.getAttribute("src") !== SoVa.nocoverpfad) {
                            SoDo.cover.setAttribute("src", SoVa.nocoverpfad);
                        }
                    }
                    if (IsVisible(SoDo.bewertungWidth)) {
                        SetHide(SoDo.bewertungWidth);
                    }
                    if (IsVisible(SoDo.bewertungStars)) {
                       SetHide(SoDo.bewertungStars);
                    }
                    SoDo.lyricWrapper.innerHTML = "";
                    if (!SonosZones.CheckStringIsNullOrEmpty(etu.title)) {
                        if (SoDo.aktTitle.textContent !== etu.title) {
                            SoDo.aktTitle.textContent=etu.title;
                        }
                    } else if (!SonosZones.CheckStringIsNullOrEmpty(curt.title)) { }
                    if (SoDo.aktTitle.textContent !== curt.title) {
                        SoDo.aktTitle.textContent=curt.title;
                    }
                    else {
                        if (SoDo.aktTitle.textContent !== "") {
                            SoDo.aktTitle.textContent="";
                        }
                    }
                    if (!SonosZones.CheckStringIsNullOrEmpty(curt.streamContent)) {
                        //description / title
                        let streamcontent = curt.artist + " (" + curt.streamContent + ":" + etu.description + "/" + etu.title + ")";
                        if (SoDo.aktArtist.textContent !== streamcontent) {
                            SoDo.aktArtist.textContent = streamcontent;
                        }
                    } else {
                        if (!SonosZones.CheckStringIsNullOrEmpty(etu.artist)) {
                            if (SoDo.aktArtist.textContent !== etu.artist) {
                                SoDo.aktArtist.textContent=etu.artist;
                            }
                        } else if (!SonosZones.CheckStringIsNullOrEmpty(curt.artist)) {
                            if (SoDo.aktArtist.textContent !== curt.artist) {
                                SoDo.aktArtist.textContent=curt.artist;
                            }
                        }
                        else {
                            if (SoDo.aktArtist.textContent !== "") {
                                SoDo.aktArtist.textContent = "";
                            }
                        }
                    }//ende Radio
                }
            }
        };
        this.ShowCurrentTrack = function() {
            var curt = this.playerProperties.currentTrack;
            if (!SonosZones.CheckStringIsNullOrEmpty(curt.title)) {
                if (SoDo.aktTitle.textContent !== curt.title) {
                    SoDo.aktTitle.textContent=curt.title;
                }
            } else {
                if (SoDo.aktTitle.textContent !== "") {
                    SoDo.aktTitle.textContent="";
                }
            }
            if (!SonosZones.CheckStringIsNullOrEmpty(curt.artist)) {
                if (SoDo.aktArtist.textContent !== curt.artist) {
                    SoDo.aktArtist.textContent=curt.artist;
                }
            } else {
                if (SoDo.aktArtist.textContent !== "") {
                    SoDo.aktArtist.textContent="";
                }
            }
            SoDo.lyricWrapper.innerHTML = "";
            if (!SonosZones.CheckStringIsNullOrEmpty(curt.mP3.lyric)) {
                SoDo.lyricWrapper.innerHTML = '<div>' + curt.mP3.lyric + '</div>';
            } else {
                SoDo.lyricWrapper.innerHTML= '<div>No Lyrics in Song</div>';
            }
            if (curt.stream === true) {
                if (IsVisible(SoDo.bewertungWidth)) {
                    SetHide(SoDo.bewertungWidth);
                }
                if (IsVisible(SoDo.bewertungStars)) {
                   SetHide(SoDo.bewertungStars);
                }
                return;
            }
            if (!IsVisible(SoDo.bewertungWidth)) {
                SetVisible(SoDo.bewertungWidth);
            }
            if (!IsVisible(SoDo.bewertungStars)) {
                SetVisible(SoDo.bewertungStars);
            }
            SoDo.bewertungWidth.style.width =curt.mP3.bewertung + "%";
            if (parseInt(curt.mP3.bewertung) === -1) {
                if (!IsVisible(SoDo.currentBomb)) {
                    SetVisible(SoDo.currentBomb);
                } 
            } else {
                if (IsVisible(SoDo.currentBomb)) {
                    SetHide(SoDo.currentBomb);
                }
            }
        }
        this.HideCurrentTrack = function() {
            if (SoDo.cover.getAttribute("src") !== SoVa.nocoverpfad) {
                SoDo.cover.setAttribute("src", SoVa.nocoverpfad);
            }
            if (IsVisible(SoDo.bewertungWidth)) {
                SetHide(SoDo.bewertungWidth);
            }
            if (IsVisible(SoDo.bewertungStars)) {
               SetHide(SoDo.bewertungStars);
            }
            if (SoDo.aktTitle.textContent !== "") {
                SoDo.aktTitle.textContent="";
            }
            if (SoDo.aktArtist.textContent !== "") {
                SoDo.aktArtist.textContent="";
            }
        };
        this.playlist = {};
        this.playlist.CheckToRender = function (t) {
            try {
                var _playlist = t.playerProperties.playlist.playListItems;
                if (_playlist === null || typeof _playlist === "undefined") {
                    _playlist = [];
                }
                var c = document.getElementsByClassName("currentplaylist");
                var clength = c.length;
                var internalmax = (clength - 1);
                if (clength === _playlist.length && clength > 0) {
                    //wenn alles gleich, dann ersten und letzten Eintrag testen, evtl. auch noch zwei drei aus der mitte.
                    if (c[0].firstChild.innerHTML === _playlist[0].title && c[internalmax].firstChild.innerHTML === _playlist[internalmax].title) {
                        //hier ist der erste und letzte gleich nun noch ein in der mitte nehmen
                        var tei = Math.floor(internalmax / 2);
                        var tei3 = Math.floor(internalmax / 3);
                        if (c[tei].firstChild.innerHTML === _playlist[tei].title && c[tei3].firstChild.innerHTML === _playlist[tei3].title) {
                            return false;
                        } else {
                            return true;
                        }

                    } else {
                        return true;
                    }
                } else {
                    //hier ist nichts gleich, daher neu rendern
                    return true;
                }
            }
            catch (fehlernachricht) {
                alert(fehlernachricht + " Fehler bei:" + fehlernachricht.fileName + "\n" + "Meldung:" + fehlernachricht.message + "\n" + "Zeile:" + fehlernachricht.lineNumber);
            }
        };//done
        this.playlist.ClearPlaylist = function (t) {
            try {
                t.playerProperties.playlist.numberOfTracks = 0;
                t.playerProperties.playlist.totalMatches = 0;
                t.playerProperties.playListItems = [];
            }
            catch (fehlernachricht) {
                alert(fehlernachricht + " Fehler bei:" + fehlernachricht.fileName + "\n" + "Meldung:" + fehlernachricht.message + "\n" + "Zeile:" + fehlernachricht.lineNumber);
            }
        }
        this.playlist.CheckIsEmpty = function (t) {
            try {
                if (t.playerProperties.playlist.playListItems.length === 0 || (t.playerProperties.playlist.playListItems.length === 1 && t.playerProperties.playlist.playListItems[0].artist === "Leer" && t.playerProperties.playlist.playListItems[0].album === "Leer" && t.playerProperties.playlist.playListItems[0].title === "Leer")) {
                    return true;
                }
                return false;
            }
            catch (fehlernachricht) {
                alert(fehlernachricht + " Fehler bei:" + fehlernachricht.fileName + "\n" + "Meldung:" + fehlernachricht.message + "\n" + "Zeile:" + fehlernachricht.lineNumber);
            }

        };//done
        this.playlist.LoadPlaylist = function (t, override, source) {
            try {
                //Läd die Playlist und ruft dann die RenderMethode erneut auf.
                if (typeof override === "undefined") {
                    override = false;
                }
                if (typeof source === "undefined") {
                    source = "unbekannt";
                }
                console.log(source);
                var result = SonosAjax("GetPlayerPlaylist", "", t.uuid + "/" + override);
                result.then(function (data) {
                    if (data.numberReturned != data.totalMatches && data.totalMatches !== -1) {
                        t.playlist.LoadPlaylist(t, true, "LoadPlaylist cuz NumberReturned and TotalMatches not match");
                        return;
                    }
                    t.playerProperties.playlist = data;
                    t.playerProperties.numberOfTracks = data.numberReturned;
                    if (t.uuid === SonosZones.ActiveZoneUUID) {
                        //t.playlist.RenderPlaylist(t, t.currentTrack.stream);
                        SonosZones.RenderPlaylist(t.uuid);
                        SonosZones.RenderNextTrack(t.uuid);
                        SonosZones.RenderPlaylistCounter(t.uuid);
                    }
                })
                result.catch(function (data) {

                    console.log("Fehler Laden Playlist");
                    t.playlist.LoadPlaylist(t, false, "LoadPlaylistFail");//Beim Laden ein Fehler daher neu laden.


                });
            }
            catch (fehlernachricht) {
                alert(fehlernachricht + " Fehler bei:" + fehlernachricht.fileName + "\n" + "Meldung:" + fehlernachricht.message + "\n" + "Zeile:" + fehlernachricht.lineNumber);
            }
        };//done
        this.playlist.RenderPlaylist = function (t) {
            //Neu wegen Stream
            try {
                if (!IsVisible(SoDo.playlistLoader)) {
                   SetVisible(SoDo.playlistLoader);
                }
                if (SoDo.currentplaylistwrapper.hasChildNodes()) {
                    SoDo.currentplaylistwrapper.innerHTML = "";
                }

                var isempty = this.CheckIsEmpty(t);
                if ((isempty || t.playerProperties.numberOfTracks !== t.playerProperties.playlist.totalMatches) && t.playerProperties.playlist.totalMatches !== -1) {
                    this.LoadPlaylist(t, false, "RenderPlaylist");
                    return;
                }
                if (isempty || !SonosZones.CheckActiveZone()) {
                    if (IsVisible(SoDo.playlistLoader)) {
                        SetHide(SoDo.playlistLoader);
                    }
                    return;
                }
                var pl = t.playerProperties.playlist.playListItems;
                let plvalue =""
                for (var i = 0; i < pl.length; i++) {
                    var songcover = '';
                    var item = pl[i];
                    if (item !== null) {
                        if (item.albumArtURI != null && item.albumArtURI !== '' && item.albumArtURI !== "leer") {
                            songcover = item.albumArtURI;
                            if (!songcover.startsWith("/hashimages/")) {
                                songcover = 'http://' + t.playerProperties.baseUrl + item.albumArtURI;
                            }
                        }
                        plvalue += '<div id="Currentplaylist_' + (i) + '" class="currentplaylist"><DIV class="currentrackinplaylist" onclick="ShowSongInfos(this)">' + item.title.replace("</", "&#x3C;/") + '</div><DIV class="curpopdown"><DIV class="playlistcover" data-url="' + songcover + '" data-uri="' + item.uri + '" data-plid="' + item.itemID + '"></DIV><DIV class="playlistplaysmall" onclick="PlayPressSmall(this)"></DIV><DIV class="mediabuttonsmal" onclick="RemoveFromPlaylist(this);return false;"><img src="Images/erase_red.png"></DIV><div class="bomb" onclick="ShowPlaylistRating(this)"><img src="/images/bombe.png" alt="playlistbomb"/></DIV><DIV onclick="ShowPlaylistRating(this)" class="rating_bar" style="margin-top: 14px;" Style="float:left;"><DIV style="width:0%;"></DIV></DIV><div OnMouseOver="MakeCurrentPlaylistSortable()" OnTouchStart="MakeCurrentPlaylistSortable()" OnTouchEnd="ResortPlaylistDisable()" OnMouseOut="ResortPlaylistDisable()" class="moveCurrentPlaylistTrack"></div></DIV></div>';
                    }
                }
                SoDo.currentplaylistwrapper.innerHTML = plvalue
                SetHide(SoDo.playlistLoader);
                SoVa.currentplaylistScrolled = false;
                SonosZones.RenderCurrentTrackinPlaylist(SonosZones.ActiveZoneUUID, SonosPlayers[SonosZones.ActiveZoneUUID].playerProperties.currentTrackNumber, "RenderPlaylistFromPlayer");
            }
            catch (fehlernachricht) {
                alert(fehlernachricht + " Fehler bei:" + fehlernachricht.fileName + "\n" + "Meldung:" + fehlernachricht.message + "\n" + "Zeile:" + fehlernachricht.lineNumber);
            }


            //Ende Neu wegen Stream
            return;
        };//done
        this.playlist.RemoveFromPlaylist = function (t, rem) {
            try {
                if (t.playerProperties.playlist.playListItems.length === 0 || !SonosZones.CheckActiveZone()) {
                    return;
                }
                t.playerProperties.playlist.playListItems.splice(rem, 1);
                t.playerProperties.numberOfTracks = t.playerProperties.playlist.playListItems.length;
                //this.RenderPlaylist(t, t.currentTrack.stream);
                document.getElementById("Currentplaylist_" + rem).remove();
                Array.from(document.getElementsByClassName("currentplaylist")).forEach(function (item,i) {
                    item.id = "Currentplaylist_" + i;
                });

                if (t.playerProperties.currentTrackNumber === rem) {
                    SonosZones.RenderNextTrack(t.uuid);
                }
                SonosZones.RenderPlaylistCounter(t.uuid);
            }
            catch (fehlernachricht) {
                alert(fehlernachricht + " Fehler bei:" + fehlernachricht.fileName + "\n" + "Meldung:" + fehlernachricht.message + "\n" + "Zeile:" + fehlernachricht.lineNumber);
            }

        };//done
        this.playlist.ReorderPlaylist = function (t, oldValue, newValue) {
            t.playerProperties.playlist.playListItems.splice(newValue, 0, t.playerProperties.playlist.playListItems.splice(oldValue, 1)[0]);
        };//done
        this.SetCurrentTrack = function (s) {
            try {
                if (!SoDo.currentplaylistwrapper.hasChildNodes()) {
                    return;
                }
            }
            catch (fehlernachricht) {
                alert(fehlernachricht + " Fehler bei:" + fehlernachricht.fileName + "\n" + "Meldung:" + fehlernachricht.message + "\n" + "Zeile:" + fehlernachricht.lineNumber);
            }
        };//done
        this.SendPlayMode = function (value) {
            try {
                if (value === null) {
                    return;
                }
                var _PlayMode = this.playerProperties.currentPlayModeString;
                switch (value) {
                    case "Shuffle":
                        switch (_PlayMode) {
                            case "NORMAL":
                                value = "SHUFFLE_NOREPEAT";
                                break;
                            case "REPEAT_ALL":
                                value = "SHUFFLE";
                                break;
                            case "SHUFFLE_NOREPEAT":
                                value = "NORMAL";
                                break;
                            case "SHUFFLE":
                                value = "REPEAT_ALL";
                                break;
                            case "SHUFFLE_REPEAT_ONE":
                                value = "REPEAT_ONE";
                                break;
                            case "REPEAT_ONE":
                                value = "SHUFFLE_REPEAT_ONE";
                                break;
                        }
                        break;
                    case "Repeat":
                        switch (_PlayMode) {
                            case "NORMAL":
                                value = "REPEAT_ALL";
                                break;
                            case "REPEAT_ALL":
                                value = "REPEAT_ONE";
                                break;
                            case "SHUFFLE_NOREPEAT":
                                value = "SHUFFLE";
                                break;
                            case "SHUFFLE":
                                value = "SHUFFLE_REPEAT_ONE";
                                break;
                            case "SHUFFLE_REPEAT_ONE":
                                value = "SHUFFLE_NOREPEAT";
                                break;
                            case "REPEAT_ONE":
                                value = "NORMAL";
                                break;
                        }
                        break;
                    default:
                        alert("SendPlaymode Unbekannter Value:" + value);
                }
                var oldval = this.playerProperties.currentPlayModeString;
                this.playerProperties.currentPlayModeString = value;
                var player = this;
                SonosAjax("SetPlaymode", "", value).then(function (data) {
                    if (data !== true) {
                        return;
                    }
                    if (player.uuid === SonosZones.ActiveZoneUUID) {
                        SonosZones.RenderPlayMode(player.uuid);
                    }
                });
            }
            catch (fehlernachricht) {
                alert(fehlernachricht + " Fehler bei:" + fehlernachricht.fileName + "\n" + "Meldung:" + fehlernachricht.message + "\n" + "Zeile:" + fehlernachricht.lineNumber);
            }
        };//done
    }
    catch (fehlernachricht) {
        alert(fehlernachricht + " Fehler bei:" + fehlernachricht.fileName + "\n" + "Meldung:" + fehlernachricht.message + "\n" + "Zeile:" + fehlernachricht.lineNumber);
    }
}
