"use strict";
function Zone() {
    this.uuid = "";
    this.CoordinatedUUIDS = []
}

//Alle SonosZonen
function SonosZonesObject() {
    var Checker = SoVa.RinconChecker;
    this.AllPlaylists = [];
    this.ZonesCount = 0;
    this.ActiveZoneUUID = "";
    this.ActiveZoneName = "";
    this.ZonesList = {}
    var t = this;
    this.VisibilityChangeTimer = new Date();
    this.RenderDevices = function () {
        //Alles leeren
        try {
            if (!IsVisible(SoDo.deviceLoader)) {
                SetVisible(SoDo.deviceLoader);
            }
            SoDo.devicesWrapper.innerHTML="";
            if (!SonosZones.CheckActiveZone()) {
                if (SonosZones.CheckStringIsNullOrEmpty(SoVa.urldevice)) {
                    SonosZones.SetFirstZonetoActive();
                } else {
                    SonosZones.SetZonetoActiveByName(SoVa.urldevice);
                }
            } else {
                //hier prüfen, wenn die aktive Zone nicht mehr aktiv sein kann, weil in gruppe.
                if (SonosPlayers[this.ActiveZoneUUID].playerProperties.localGroupUUID !== SonosPlayers[this.ActiveZoneUUID].uuid) {
                    this.ActiveZoneUUID = SonosPlayers[this.ActiveZoneUUID].playerProperties.localGroupUUID;
                }
            }
            //hier mal alle Zonen laden und in eine liste legen.
            this.ZonesList = {};
            this.ZonesListOverGroups = {}
            var request = SonosAjax("GetZones");
            request.success(function (data) {
                if (data === null || typeof data === "undefined") {
                    console.log("Fehler bei GetZones, Data null");
                    return;
                }
                SoDo.devicesWrapper.innerHTML ="";
                for (var i = 0; i < data.zoneGroupStates.length; i++) {
                    var zuuid = data.zoneGroupStates[i].coordinatorUUID;
                    var zonemember = data.zoneGroupStates[i].zoneGroupMember;
                    SonosZones.ZonesList[zuuid] = new Zone();
                    SonosZones.ZonesList[zuuid].uuid = zuuid;
                    SonosZones.ZonesList[zuuid].CoordinatedUUIDS = zonemember;
                    SonosZones.ZonesList[zuuid].SwGen = data.zoneGroupStates[i].softwareGeneration;
                }
                var prop = Object.getOwnPropertyNames(SonosZones.ZonesList);
                var zcounter = 0; //Anzahl der Zonen;
                for (var i = 0; i < prop.length; i++) {
                    if (prop[i].substring(0, Checker.length) === Checker) {
                        //Es handelt sich um einen Sonosplayer
                        var p = prop[i];
                        var zone = SonosZones.ZonesList[p];
                        var uuid = zone.uuid;
                        var aktdev = "";
                        if (uuid === SonosZones.ActiveZoneUUID) {
                            aktdev = " akt_device";
                        }
                        if (typeof SonosPlayers[uuid] === "undefined") {
                            continue;
                        }
                        var playstateimg = 'style="opacity:0;"';
                        var playclass = "";
                        var playinternal = "PLAYING";
                        if (SonosPlayers[uuid].playerProperties.transportStateString === "PLAYING") {
                            playstateimg = 'style="opacity:1;"';
                            playclass = "active";
                            playinternal = "Pause";
                        }
                        var image = '<img class="deviceIcon" src="' + SonosPlayers[p].playerProperties.icon + '">';
                        SoDo.devicesWrapper.innerHTML += '<div class="groupdevicewrapper"><div id="' + SonosPlayers[p].uuid + '" class="device' + aktdev + '" onclick="SetDevice(\'' + uuid + '\');"><p>' + image + SonosPlayers[uuid].name + '</p>' + SonosZones.GetCordinatedPlayerasStringFormat(zone) + '</div><img id="deviceplayinggif_' + uuid + '" class="deviceplayinggif" ' + playstateimg + ' src="/images/playing.gif"><div id="GroupDevice_' + uuid + '" onclick="SetDeviceGroupFor(\'' + uuid + '\')" class="groupdeviceclass">&nbsp;&nbsp;Gruppe&nbsp;(' + SonosPlayers[uuid].SoftwareGeneration + ')&nbsp;</div><div class="groupdeviceclass groupdeviceclassplay ' + playclass + '" onclick="SonosPlayers[\'' + uuid + '\'].SendTransportState(\'' + playinternal + '\');" id="' + uuid + '_GroupPlayState"></div></div>';
                        zcounter++;
                    }
                }
                if (typeof SonosPlayers[SonosZones.ActiveZoneUUID] !== "undefined" && typeof SonosPlayers[SonosZones.ActiveZoneUUID].playerProperties.transportStateString !== "undefined" && SonosPlayers[SonosZones.ActiveZoneUUID].playerProperties.transportStateString === "PLAYING") {
                    AddClass(SoDo.playButton, SoVa.aktiv);
                } else {
                    RemoveClass(SoDo.playButton, SoVa.aktiv);
                }
                this.ZonesCount = zcounter;
                if (IsVisible(SoDo.deviceLoader)) {
                    SetHide(SoDo.deviceLoader);
                }
                if (!IsVisible(SoDo.groupDeviceShow)) {
                    SetVisible(SoDo.groupDeviceShow);
                }
                SonosZones.RenderActiveZone(SonosZones.ActiveZoneUUID, true);
            });
        } catch (fehlernachricht) {
            alert(fehlernachricht + " Fehler bei:" + fehlernachricht.fileName + "\n" + "Meldung:" + fehlernachricht.message + "\n" + "Zeile:" + fehlernachricht.lineNumber);
            console.log(fehlernachricht);
        }
    };//done
    this.GetCordinatedPlayerasStringFormat = function (zone) {
        var coorduuid = zone.uuid;
        var datas = zone.CoordinatedUUIDS;
        var sfcp = "";
        for (var i = 0; i < datas.length; i++) {
            var uuid = datas[i].uuid;
            if (typeof uuid !== "undefined" && uuid !== coorduuid) {
                sfcp += '<p class="childplayer">' + SonosPlayers[uuid].name + '</p>';
            }
        }
        return sfcp;
    };//done
    this.CheckActiveZone = function () {
        if (SonosZones.CheckStringIsNullOrEmpty(this.ActiveZoneUUID) || typeof SonosPlayers[this.ActiveZoneUUID] === "undefined") {
            return false;
        }
        return true;
    };//done
    this.CheckServerData = function (data) {
        var allrincon = this.ZonesRincon();
        var i;
        for (i = 0; i < data.length; i++) {
            var rincon = data[i].Coordinator.uuid;
            if (typeof SonosZones[rincon] === "undefined") {
                //Wenn nicht definiert ein neuen anlegen	
                SonosZones[rincon] = new SonosZone(rincon, data[i].Coordinator.name);
            }
            //Zonendaten neu schreiben. Aktiv Status merken.
            var ind = allrincon.indexOf(data[i].uuid);
            allrincon.splice(ind, 1); //einträge raus nehmen
            SonosZones[rincon].SetBySonosItem(data[i]);
        }
        //nun den rest löschen
        for (i = 0; i < allrincon.length; i++) {
            var rin = allrincon[i];
            delete SonosZones[rin];
        }
        //Prüfen, ob der Master nun in einer Gruppe enthalten und nicht mehr wählbar ist.
        if (SonosZones.CheckActiveZone()) {
            allrincon = this.ZonesRincon();
            for (i = 0; i < allrincon.length; i++) {
                var aktuuid = allrincon[i];
                var crdplayer = SonosZones[aktuuid].GetCordinatedPlayer();
                if (crdplayer.length > 0) {
                    for (var cp = 0; cp < crdplayer.length; cp++) {
                        var cpuuid = crdplayer[cp].uuid;
                        if (cpuuid === this.ActiveZoneUUID) {
                            SonosZones[aktuuid].ActiveZone = true; //Neuen Master definieren
                        }
                    }
                }
            }
        }
    };
    this.RenderActiveZone = function (rincon, override) {
        if (typeof override === "undefined") {
            override = false;
        }
        if (this.ActiveZoneUUID === rincon && !override) return;
        var n = "";
        if (typeof SonosPlayers[rincon] !== "undefined") {
            n = SonosPlayers[rincon].name;
        }
        //Die URl Ändern, damit diese beim reload genommen wird.
        window.history.pushState(null, "Sonos:" + n, location.origin + "/?device=" + n);

        this.ActiveZoneUUID = rincon;
        this.ActiveZoneName = n;
        document.title = 'Sonos:' + this.ActiveZoneName;
        this.RenderVolume(rincon);
        this.RenderPlayMode(rincon);
        this.RenderCrossFadeMode(rincon);
        this.RenderMute(rincon);
        this.RenderAudioIn(rincon);
        this.RenderRatingFilter(rincon);
        this.RenderCurrentTrack(rincon);
        this.RenderTrackTime(rincon);
        this.RenderPlaylistCounter(rincon);
        this.RenderPlaylist(rincon);
        this.RenderTransportState(rincon);
        this.RenderNextTrack(rincon);
        this.RenderSleepTimer(rincon);
        this.RenderAllPlaylist();
    };
    this.SetFirstZonetoActive = function () {
        //Alle Player durchlaufen und den ersten auf activ setzen, da noch keiner gewählt wurde.
        try {
            var prop = Object.getOwnPropertyNames(SonosPlayers);
            for (var i = 0; i < prop.length; i++) {
                var uuid = prop[i];
                if (uuid.substring(0, Checker.length) === Checker && SonosPlayers[uuid].playerProperties.groupCoordinatorIsLocal === true) {
                    //Es handelt sich um einen Sonosplayer
                    this.ActiveZoneUUID = uuid;
                    this.ActiveZoneName = SonosPlayers[this.ActiveZoneUUID].name;
                    break;
                }
            }
        }
        catch (ex) {
            console.log("Fehler bei:SetFirstZonetoActive");
            console.log(ex);
        }
    };//done
    this.SetZonetoActiveByName = function (s) {
        try {
            //Alle Player durchlaufen und nach Namen Prüfen
            var prop = Object.getOwnPropertyNames(SonosPlayers);
            var found = false;
            for (var i = 0; i < prop.length; i++) {
                var uuid = prop[i];
                if (uuid.substring(0, Checker.length) === Checker) {
                    //Es handelt sich um einen Sonosplayer
                    var tname = SonosPlayers[uuid].name.toLowerCase();
                    if (tname === s) {
                        //Player gefunden und nun prüfen welcher aktiv werden soll, falls nicht Alleine
                        if (SonosPlayers[uuid].playerProperties.groupCoordinatorIsLocal === true && SonosPlayers[uuid].playerProperties.localGroupUUID === SonosPlayers[uuid].uuid) {
                            this.ActiveZoneUUID = uuid;
                        } else {
                            this.ActiveZoneUUID = SonosPlayers[uuid].playerProperties.localGroupUUID;
                        }
                        found = true;
                        this.ActiveZoneName = s;
                        break;
                    }
                }
            }
            if (!found) {
                this.SetFirstZonetoActive();
            }
        }
        catch (ex) {
            console.log("Fehler bei:SetZonetoActiveByName");
            console.log(ex);
        }
    };//done
    this.SetActiveZonebyUUID = function (uuid) {
        //Alle Player durchlaufen und nach Namen Prüfen
        var player = SonosPlayers[uuid];
        if (!SonosZones.CheckStringIsNullOrEmpty(player)) {
            if (SonosPlayers[uuid].playerProperties.groupCoordinatorIsLocal === true) {
                this.ActiveZoneUUID = uuid;
            } else {
                this.ActiveZoneUUID = SonosPlayers[uuid].playerProperties.localGroupUUID;
            }
            this.ActiveZoneName = SonosPlayers[this.ActiveZoneUUID].name;
        }
    };
    this.CheckStringIsNullOrEmpty = function (s) {
        if (typeof s === "undefined" || s === null || s === "leer" || s === "Leer" || s === "") return true;

        return false;
    };//done
    this.CheckStreamShowElements = function (uuid) {
        //Soll prüfen, ob die Elemente der Oberfläche gezeichnet werden sollen.
        //new s= uuid return ture = anzeigen; false ausblenden
        var player = SonosPlayers[uuid];
        if (typeof player === "undefined") return false;
        var curtr = player.playerProperties.currentTrack; //Aktueller Song
        var etu = player.playerProperties.enqueuedTransportURIMetaData; //Metadaten als SonosItem.
        if (curtr.stream === false) return true; //kein Stream
        if (curtr.stream && this.CheckStringIsNullOrEmpty(curtr.protocolInfo)) return false //CurrentTrack ist noch leer.
        if (this.CheckStringIsNullOrEmpty(etu)) return false; //Stream aber keine Daten.
        if (etu.classType === "object.item.audioItem.audioBroadcast" || etu.protocolInfo.startsWith("http-get")) return false //Radio || UPNP Set wie durch MediaMonkey
        return true;//alles andere 
        //Ende New
    };
    this.CheckMP3IsEmpty = function (s) {
        return s.album === "" && s.artist == "" && s.pfad === "" && s.titel === "";
    };//done
    this.RenderAudioIn = function (uuid) {
        var player = SonosPlayers[uuid];
        if (typeof player === "undefined") return;
        if (player.playerProperties.AudioInput_LineInConnected === true) {
            
            if (!IsVisible(SoDo.audioInButton)) {
                SetVisible(SoDo.audioInButton);
            }
            if (player.playerProperties.currentTrack.stream === true && (player.playerProperties.currentTrack.streamContent === "Audio Eingang" || player.playerProperties.currentTrack.title === "Heimkino")) {
                AddClass(SoDo.audioInButton)
            } else {
                RemoveClass(SoDo.audioInButton)
            }
        } else {

            if (IsVisible(SoDo.audioInButton)) {
               SetHide(SoDo.audioInButton);
            }
        }
    };//done
    this.RenderTransportState = function (uuid) {
        var player = SonosPlayers[uuid];
        if (typeof player === "undefined") return;
        var value = player.playerProperties.transportStateString;
        if (value === "TRANSITIONING") {
            SonosAjax("FillPlayerPropertiesDefaults", "", uuid, true);
            return;
        }
        if (player.uuid !== player.playerProperties.localGroupUUID) return;
        //Playlist
        if ($(".currentplaylist").length > 0) {
            var apsnumber = SonosPlayers[uuid].playerProperties.currentTrackNumber - 1;
            var curr = $("#Currentplaylist_" + apsnumber + " > .curpopdown > .playlistplaysmall");
            if (value === "PLAYING") {
                if (!curr.hasClass("akt")) {
                    $("#Currentplaylist_" + apsnumber + " > .curpopdown > .playlistplaysmall").addClass("akt");
                }
            } else {
                if (curr.hasClass("akt")) {
                    $("#Currentplaylist_" + apsnumber + " > .curpopdown > .playlistplaysmall").removeClass("akt");
                }
            }
        }
        //if (value === "PLAYING") {
        //    $("#Currentplaylist_" + (apsnumber - 1) + " > .curpopdown > .playlistplaysmall").addClass("akt");
        //}
        //Großer Playbuttom
        if (uuid === this.ActiveZoneUUID) {
            if (value === "PLAYING") {
                AddClass(SoDo.playButton, SoVa.aktiv);
            } else {
                RemoveClass(SoDo.playButton, SoVa.aktiv);
            }
        }
        //Devicelist
        this.RenderDeviceTransportState(uuid);
    }//done
    this.RenderDeviceTransportState = function (uuid) {
        if (typeof SonosPlayers[uuid] === "undefined") return;
        var value = SonosPlayers[uuid].playerProperties.transportStateString;
        var op = 0;
        var button = $("#" + uuid + "_GroupPlayState");
        //var  = "Play";
        var playinternal = "PLAYING";
        if (value === "PLAYING") {
            op = 1; //Playstate anzeigen
            //playtext = "Pause";
            playinternal = "PAUSED_PLAYBACK";
            if (!button.hasClass("active")) {
                button.addClass("active");
            }
        } else {
            if (button.hasClass("active")) {
                button.removeClass("active");
            }
        }

        //Device Play
        $("#" + uuid).next('img').css("opacity", op);
        button.attr("onClick", "SonosPlayers['" + uuid + "'].SendTransportState('" + playinternal + "')");
    }
    this.RenderMute = function (uuid) {
        var player = SonosPlayers[uuid];
        if (typeof player === "undefined") return;
        if (uuid === this.ActiveZoneUUID) {
            if (player.playerProperties.groupRenderingControl_GroupMute === true) {
                AddClass(SoDo.muteButton, SoVa.aktiv);
            } else {
                RemoveClass(SoDo.muteButton, SoVa.aktiv);
            }
        }
        var multivolmute = $("#MultiVolumeMute_" + uuid);
        if (multivolmute.length === 1) {
            if (player.playerProperties.mute === true) {
                if (!multivolmute.hasClass(SoVa.aktiv))
                    multivolmute.addClass(SoVa.aktiv);
            } else {
                if (multivolmute.hasClass(SoVa.aktiv))
                    multivolmute.removeClass(SoVa.aktiv);
            }
        }
    };//done
    this.RenderTrackTime = function (uuid) {
        if (this.CheckStringIsNullOrEmpty(uuid)) {
            if (this.CheckStringIsNullOrEmpty(this.ActiveZoneUUID)) {
                return;
            }
            uuid = this.ActiveZoneUUID;
        }
        let player = SonosPlayers[uuid];
        if (typeof player === "undefined") return;
        if (typeof player.playerProperties === "undefined") {
            alert(player.name + " PlayerProps undefined");
            return;
        } 
        if (typeof player.playerProperties.currentTrack === "undefined") {
            alert(player.name + " currentTrack undefined");
            return;
        } 
        let curt = player.playerProperties.currentTrack;
        if (!this.CheckStreamShowElements(uuid) && (curt.duration === null || curt.duration.totalSeconds === 0)) {
            if (IsVisible(SoDo.runtimeCurrentSong)){
                SetHide(SoDo.runtimeCurrentSong);
            }
            if (SoDo.runtimeSlider.is(":visible")) {
                SoDo.runtimeSlider.hide();
            }
        } else {
            //Hier nun die Erweiterung machen. Um mit Stunden und Minuten zu Arbeiten
            if (!IsVisible(SoDo.runtimeCurrentSong)) {
                SetVisible(SoDo.runtimeCurrentSong);
            }
            if (SoDo.runtimeSlider.is(":hidden")) {
                SoDo.runtimeSlider.show();
            }
            if (curt.duration !== null) {
                SoDo.runtimeSlider.slider("option", "max", curt.duration.totalSeconds);
            }
            if (curt.relTime !== null) {
                SoDo.runtimeSlider.slider("option", "value", curt.relTime.totalSeconds);
            }
            let reltimedom = curt.relTime.stringWithoutZeroHours;
            if (SoDo.runtimeRelTime.textContent !== reltimedom) {
                SoDo.runtimeRelTime.textContent = reltimedom;
            }
            //let durtimedom = curt.duration.totalSeconds.toString().toHHMMSS();
            let durtimedom = curt.duration.stringWithoutZeroHours;
            if (SoDo.runtimeDuration.textContent !== durtimedom) {
                SoDo.runtimeDuration.textContent = durtimedom;
            }
        }
    };//done
    this.RenderCrossFadeMode = function (uuid) {
        if (typeof SonosPlayers[uuid] === "undefined") return;
        if (SonosPlayers[uuid].playerProperties.currentCrossFadeMode === true) {
            AddClass(SoDo.fadeButton, SoVa.aktiv);
        } else {
            RemoveClass(SoDo.fadeButton, SoVa.aktiv);
        }
    };//done
    this.RenderPlayMode = function (uuid) {
        if (typeof SonosPlayers[uuid] === "undefined") return;
        var v = SonosPlayers[uuid].playerProperties.currentPlayModeString;
        switch (v) {
            case "NORMAL":
                RemoveClass(SoDo.repeatButton, SoVa.aktiv);
                RemoveClass(SoDo.repeatButton, "aktiv_one");
                RemoveClass(SoDo.shuffleButton, SoVa.aktiv);
                break;
            case "REPEAT_ALL":
                AddClass(SoDo.repeatButton, SoVa.aktiv);
                RemoveClass(SoDo.repeatButton, "aktiv_one");
                RemoveClass(SoDo.shuffleButton, SoVa.aktiv);
                break;
            case "REPEAT_ONE":
                RemoveClass(SoDo.repeatButton, SoVa.aktiv)
                AddClass(SoDo.repeatButton, "aktiv_one");
                RemoveClass(SoDo.shuffleButton, SoVa.aktiv);
                break;
            case "SHUFFLE_NOREPEAT":
                RemoveClass(SoDo.repeatButton, SoVa.aktiv)
                RemoveClass(SoDo.repeatButton, "aktiv_one");
                AddClass(SoDo.shuffleButton, SoVa.aktiv);
                break;
            case "SHUFFLE":
                AddClass(SoDo.repeatButton, SoVa.aktiv);
                RemoveClass(SoDo.repeatButton, "aktiv_one");
                AddClass(SoDo.shuffleButton, SoVa.aktiv);
                break;
            case "SHUFFLE_REPEAT_ONE":
                RemoveClass(SoDo.repeatButton, SoVa.aktiv);
                AddClass(SoDo.repeatButton, "aktiv_one");
                AddClass(SoDo.shuffleButton, SoVa.aktiv);
                break;
            default:
                RemoveClass(SoDo.repeatButton, SoVa.aktiv);
                RemoveClass(SoDo.repeatButton, "aktiv_one");
                RemoveClass(SoDo.shuffleButton, SoVa.aktiv);
        }
    };//done;
    this.RenderVolume = function (uuid) {
        if (typeof uuid === "undefined" && typeof this.ActiveZoneUUID !== "undefined") {
            uuid = this.ActiveZoneUUID;
        }
        var player = SonosPlayers[uuid];
        if (typeof player === "undefined") {
            window.setTimeout("SonosZones.RenderVolume()", 1000);
            return;
        }
        if (player.playerProperties.groupRenderingControl_GroupVolume === 0) {
            SonosAjax("GetGroupVolume").success(function (data) {
                player.playerProperties.groupRenderingControl_GroupVolume = data;
                window.setTimeout("SonosZones.RenderVolume()", 1000);
            });
            return;
        }
        var val = SoDo.volumeSlider.slider("value");
        if (player.playerProperties.groupRenderingControl_GroupVolume !== val) {
            SoDo.volumeSlider.slider({ value: player.playerProperties.groupRenderingControl_GroupVolume });
        }
        var htmlval = parseInt(SoDo.labelVolume.textContent);
        if (isNaN(htmlval) || htmlval !== player.playerProperties.groupRenderingControl_GroupVolume) {
            SoDo.labelVolume.textContent = player.playerProperties.groupRenderingControl_GroupVolume;
        }
        var oldValue = $("#MultivolumesliderAll").slider("value");
        var gvol = player.playerProperties.groupRenderingControl_GroupVolume;
        if (!isNaN(oldValue) && oldValue !== gvol) {
            $("#MultivolumesliderAll").slider({ value: gvol });
            $("#MultivolumeAllNumber").html(gvol);
        }
        //MultiVolume falls schon irgendwie vorhanden war
        player.playerProperties.zoneGroupTopology_ZonePlayerUUIDsInGroup.forEach(function (element) {
            oldValue = $("#Multivolumeslider_" + element).slider("value");
            if (typeof SonosPlayers[element] !== "undefined") {
                var vol = SonosPlayers[element].playerProperties.volume;
                if (!isNaN(oldValue) && oldValue !== vol) {
                    $("#Multivolumeslider_" + element).slider({ value: vol });
                    $("#MultivolumesliderVolumeNumber_" + element).html(vol);
                }
            }
        });


    };//done
    this.RenderPlaylist = function (uuid) {
        if (typeof SonosPlayers[uuid] === "undefined") return;
        //Neu wegen Stream
        if (SonosZones.CheckStreamShowElements(uuid)) {
            SonosPlayers[uuid].playlist.RenderPlaylist(SonosPlayers[uuid], false);
        } else {
            if ($(".currentplaylist").length > 0) {
                $(".currentplaylist").remove();
            }
            if (IsVisible(SoDo.playlistLoader)) {
                SetHide(SoDo.playlistLoader)
            }
        }
        //Ende Neu wegen Stream
        return;
    };//done
    this.RenderPlaylistCounter = function (uuid) {
        if (typeof SonosPlayers[uuid] === "undefined") return;
        //Neu wegen Stream
        if (!this.CheckStreamShowElements(uuid)) {
            if (IsVisible(SoDo.playlistCount)) {
                SetHide(SoDo.playlistCount);
            }
        } else {
            if (!IsVisible(SoDo.playlistCount)) {
                SetVisible(SoDo.playlistCount);
            }
            if (parseInt(SoDo.playlistAkt.textContent) !== SonosPlayers[uuid].playerProperties.currentTrackNumber) {
                SoDo.playlistAkt.textContent=SonosPlayers[uuid].playerProperties.currentTrackNumber;
            }
            if (SonosPlayers[uuid].playerProperties.numberOfTracks == 0 && SonosPlayers[uuid].playerProperties.playlist.playListItems.length > 0) {
                SonosPlayers[uuid].playerProperties.numberOfTracks = SonosPlayers[uuid].playerProperties.playlist.playListItems.length;
            }
            if (parseInt(SoDo.playlistTotal.textContent) !== SonosPlayers[uuid].playerProperties.numberOfTracks) {
                SoDo.playlistTotal.textContent=SonosPlayers[uuid].playerProperties.numberOfTracks;
            }
        }

    };//done
    this.RenderNextTrack = function (uuid) {
        //Stream
        var player = SonosPlayers[uuid];
        if (typeof player === "undefined") return;
        //new
        if (!SonosZones.CheckStringIsNullOrEmpty(player.playerProperties.nextTrack.uri)) {
            if (!IsVisible(SoDo.nextSongWrapper)) {
                SetVisible(SoDo.nextSongWrapper);
            }
            var seap = " - ";
            var text = "";
            if (SonosZones.CheckStringIsNullOrEmpty(player.playerProperties.nextTrack.artist) || SonosZones.CheckStringIsNullOrEmpty(player.playerProperties.nextTrack.title)) {
                seap = '';
            }
            if (!SonosZones.CheckStringIsNullOrEmpty(player.playerProperties.nextTrack.artist)) {
                text = player.playerProperties.nextTrack.artist + seap;
            }
            if (!SonosZones.CheckStringIsNullOrEmpty(player.playerProperties.nextTrack.title)) {
                text = text + player.playerProperties.nextTrack.title;
            }
            if (SoDo.nextTitle.textContent !== text) {
                SoDo.nextTitle.textContent =text;
            }

            if (SonosZones.CheckStringIsNullOrEmpty(player.playerProperties.nextTrack.albumArtURI)) {
                if (IsVisible(SoDo.nextcover)) {
                   SetHide(SoDo.nextcover);
                }
            } else {
                if (!IsVisible(SoDo.nextcover)) {
                   SetVisible(SoDo.nextcover);
                }
                //Wenn nicht mit /getaa startet nicht BASEURL nehmen, dann ist das evtl. DLNA
                var albumart = 'http://' + player.playerProperties.baseUrl + player.playerProperties.nextTrack.albumArtURI;
                if (!player.playerProperties.nextTrack.albumArtURI.startsWith("/getaa")) {
                    albumart = player.playerProperties.nextTrack.albumArtURI;
                }
                if (SoDo.nextcover.getAttribute("src") !== albumart) {
                    SoDo.nextcover.setAttribute("src", albumart);
                }
                UpdateImageOnErrors();
            }

        } else {
            //todo: prüfen auf is empty und ob die playlist geladen ist, dann kann man ein nächten track anzeigen? Stream?
            if (IsVisible(SoDo.nextSongWrapper)) {
               SetHide(SoDo.nextSongWrapper);
            }
        }
        return;
    };//done
    this.RenderCurrentTrackinPlaylist = function (uuid, apsnumber, source) {
        var player = SonosPlayers[uuid];
        if (typeof player === "undefined") return;
        //Playlist prüfen
        if (player.playerProperties.playlist.playListItems.length === 0) {
            return;
        }
        if (typeof apsnumber === "undefined" || apsnumber === null) {
            if (typeof SonosZones[uuid] !== "undefined" && typeof SonosPlayers[uuid].currentTrackNumber !== "undefined" && SonosPlayers[uuid].currentTrackNumber !== null && SonosPlayers[uuid].currentTrackNumber !== 0) {
                apsnumber = SonosPlayers[uuid].currentTrackNumber;
            } else {
                return false;
            }
        } else {
            apsnumber = parseInt(apsnumber);
        }
        if (typeof source === "undefined") {
            source = "Unbekannt";
        }
        var contactTopPosition = $("#Currentplaylist_" + (apsnumber - 1));
        if (contactTopPosition.length !== 0) {
            //prüfen, ob es sich um den selben Song handelt.
            var NewEntry = $("#Currentplaylist_" + (apsnumber - 1) + " > .currentrackinplaylist");
            $(".playlistplaysmall").removeClass("akt");
            if (SonosPlayers[uuid].playerProperties.transportStateString === "PLAYING") {
                $("#Currentplaylist_" + (apsnumber - 1) + " > .curpopdown > .playlistplaysmall").addClass("akt");
            }
            if (NewEntry.hasClass("aktsonginplaylist")) {
                return false;
            }
            $(".currentrackinplaylist").removeClass("aktsonginplaylist");
            NewEntry.addClass("aktsonginplaylist");
            //Ermitteln der Position des aktuellen Songs und dahin scrollen, wenn nicht manuell gescrollt wurde
            if (SoVa.currentplaylistScrolled === false) {
                SoDo.currentplaylistwrapper.scrollTop = 0;
                var ctop = contactTopPosition.position().top;
                SoDo.currentplaylistwrapper.scrollTop = ctop - 30;
                window.setTimeout("SoVa.currentplaylistScrolled = false;", 100);//Beim Scrollen wird das auf true gesetzt, daher wieder rückgänig machen.
            }
        }
        return true;
    }//done
    this.RenderCurrentTrack = function (uuid) {
        if (typeof uuid === "undefined") {
            uuid = this.ActiveZoneUUID;
        }
        if (typeof SonosPlayers[uuid] === "undefined") return;
        //Neu wegen Stream
        SonosPlayers[uuid].RenderCurrentTrack();
        //Ende Neu

        //SonosPlayers[uuid].CheckCurrentTrack(SonosPlayers[uuid].playerProperties.currentTrack);
        //SonosPlayers[uuid].currentTrack.RenderCurrentTrack(SonosPlayers[uuid].playerProperties.currentTrackNumber);
    };//done
    this.RenderSleepTimer = function (uuid) {
        if (typeof SonosPlayers[uuid] === "undefined") return;
        var isrunning = SonosPlayers[uuid].playerProperties.sleepTimerRunning;
        var duration = SonosPlayers[uuid].playerProperties.remainingSleepTimerDuration
        if (duration !== "" && duration !== "aus" && duration !== "00:00:00" && isrunning === true) {
            AddClass(SoDo.sleepModeButton, SoVa.aktiv);
            if (SoDo.sleepModeState.textContent !== duration) {
                SoDo.sleepModeState.textContent =duration;
            }
        } else {
            RemoveClass(SoDo.sleepModeButton, SoVa.aktiv);
            if (SoDo.sleepModeState.textContent !== "") {
                SoDo.sleepModeState.textContent ="";
            }
        }
    };//done
    this.RenderRatingFilter = function (uuid) {
        var player = SonosPlayers[uuid];
        if (typeof player === "undefined") return;
        var plrating = player.RatingFilter;
        if (typeof plrating === "undefined") return;
        let frbar = SoDo.filterListRatingBar.querySelector(":scope > .rating_bar_aktiv");
        if (frbar !== null) {
            RemoveClass(frbar, "rating_bar_aktiv");
        }
        let fgelegenheit = SoDo.filterListGelegenheit.querySelector(":scope > .selected");
        if (fgelegenheit !== null) {
            RemoveClass(fgelegenheit, SoVa.selected);
        }
        let fgeschwindigkeit = SoDo.filterListGeschwindigkeit.querySelector(":scope > .selected");
        if (fgeschwindigkeit !== null) {
            RemoveClass(fgeschwindigkeit, SoVa.selected);
        }
        let fstimmung = SoDo.filterListStimmung.querySelector(":scope > .selected");
        if (fstimmung !== null) {
            RemoveClass(fstimmung, SoVa.selected);
        }
        let falbum = SoDo.filterListAlbumInterpret.querySelector(":scope > .selected");
        if (falbum !== null) {
            RemoveClass(falbum, SoVa.selected);
        }
        var def = true;
        RemoveClass(SoDo.filterListRatingBarBomb,"rating_bar_aktiv")
        if (plrating.rating > -2) {
            if (plrating.rating === -1) {
                AddClass(SoDo.filterListRatingBarBomb, "rating_bar_aktiv")
            } else {
                AddClass(document.getElementById("filter_rating_bar_" + plrating.rating), "rating_bar_aktiv");
            }
            def = false;
        }
        if (plrating.stimmung !== 6) {
            AddClass(document.getElementById("Filterstimmung_" + plrating.stimmung), SoVa.selected)
            def = false;
        }
        if (plrating.gelegenheit !== 5) {
            AddClass(document.getElementById("Filtergelegenheit_" + plrating.gelegenheit), SoVa.selected)
            def = false;
        }
        if (plrating.geschwindigkeit !== 6) {
            AddClass(document.getElementById("Filtergeschwindigkeit_" + plrating.geschwindigkeit), SoVa.selected)
            def = false;
        }
        if (plrating.albpumInterpretFilter !== "unset") {
            AddClass(document.getElementById("AlbumArtist" + plrating.albpumInterpretFilter), SoVa.selected)
            def = false;
        }
        if (def === false) {
            AddClass(SoDo.filterListButton, "akt");
        } else {
            RemoveClass(SoDo.filterListButton, "akt");
        }
    };//done
    this.RenderAllPlaylist = function (override) {
        try {
            if (override === true) {
                this.AllPlaylists = [];
            }
            if (typeof this.AllPlaylists === "undefined" || this.AllPlaylists.length === 0) {
                //hier laden und das rendern neu aufrufen
                SonosAjax("GetPlaylists").success(function (data) {
                    if (data === null || typeof data === "undefined" || data.length === 0) {
                        window.setTimeout("SonosZones.RenderAllPlaylist()", 1000);
                        return;
                    }
                    t.AllPlaylists = data;
                    window.setTimeout("SonosZones.RenderAllPlaylist()", 1000);
                    return;
                });
            }
            var player = SonosPlayers[this.ActiveZoneUUID];
            var uri = "";
            if (typeof player !== "undefined" && !this.CheckStringIsNullOrEmpty(player.playerProperties.enqueuedTransportURI)) {
                uri = player.playerProperties.enqueuedTransportURI;
            }
            if (!SoDo.playlistwrapper.hasChildNodes() || override === true) {
                if (!IsVisible(SoDo.globalPlaylistLoader)) {
                    SetVisible(SoDo.globalPlaylistLoader)
                }
                if (!SoDo.playlistwrapper.hasChildNodes()) {
                    SoDo.playlistwrapper.innerHTML = "";
                }
                if (this.AllPlaylists.length === 0) return;
                let domlelemts = "";
                this.AllPlaylists.forEach(function (item, i) { 
                    var playlisttype;
                    var acclass = "";
                    if (item.description === "M3U") {
                        playlisttype = "m3u";
                    } else {
                        playlisttype = "sonos";
                    }
                    if (item.uri === uri) {
                        acclass = SoVa.aktiv;
                    }
                    domlelemts += '<div id="Playlist_' + i + '" class="playlist ' + playlisttype + ' ' + acclass + '"><div onclick="SonosZones.ReplacePlaylist(' + i + ');">' + item.title + '</DIV></div>';
                });
                SoDo.playlistwrapper.innerHTML = domlelemts;
                //$.each(this.AllPlaylists, function (i, item) {
                //    var playlisttype;
                //    var acclass = "";
                //    if (item.description === "M3U") {
                //        playlisttype = "m3u";
                //    } else {
                //        playlisttype = "sonos";
                //    }
                //    if (item.uri === uri) {
                //        acclass = SoVa.aktiv;
                //    }
                //    //Wiedergabeliste befüllen
                //    $('<div id="Playlist_' + i + '" class="playlist ' + playlisttype + ' ' + acclass + '"><div onclick="SonosZones.ReplacePlaylist(' + i + ');">' + item.title + '</DIV></div>').appendTo(SoDo.playlistwrapper);
                //});
                if (IsVisible(SoDo.globalPlaylistLoader)) {
                    SetHide(SoDo.globalPlaylistLoader)
                }
            } else {
                //Es wurde schon gerendert und somit nur noch die angezeigte wechseln.
                $(".playlist").removeClass(SoVa.aktiv);
                if (this.AllPlaylists.length === 0 || this.CheckStringIsNullOrEmpty(uri)) return;
                for (var i = 0; i < this.AllPlaylists.length; i++) {
                    if (this.AllPlaylists[i].uri === uri) {
                        $("#Playlist_" + i).addClass(SoVa.aktiv);
                        break;
                    }
                }
            }
        }

        catch (er) {
            console.log(er);
        }
    }
    this.ReplacePlaylist = function (item) {
        SetVisible(SoDo.playlistLoader)
        if (!IsVisible(SoDo.browseLoader)) {
            SetVisible(SoDo.browseLoader);
        }
        $(".currentplaylist").remove();
        var uri = SonosZones.AllPlaylists[item].containerID;
        var player = SonosPlayers[this.ActiveZoneUUID];
        if (typeof player === "undefined" || SonosZones.AllPlaylists.length === 0) return;
        //Damit beim gleichem Lied kein Problem entsteht Artist leeren.
        //if (player.playerProperties.currentTrack !== null && player.playerProperties.currentTrack.artist !== null) {
        //    player.playerProperties.currentTrack.artist = "leer";
        //    player.playerProperties.currentTrack.albumArtURI = "leer";
        //}
        SonosAjax("ReplacePlaylist", { '': uri }).success(function (data) {
            if (data === false) {
                console.log("Beim Replace ist ein Fehleraufgetreten.");
            }
            player.playerProperties.enqueuedTransportURI = SonosZones.AllPlaylists[item].uri;
            SonosZones.RenderAllPlaylist();
            player.playlist.ClearPlaylist(player);
            if (IsVisible(SoDo.browseLoader)) {
                SetHide(SoDo.browseLoader);
            }
        });
    };
    this.CheckVisibility = function () {
        //define Property
        var prefixes = ['webkit', 'moz', 'ms', 'o'];
        // if 'hidden' is natively supported just return it
        if ('hidden' in document) {
            SoVa.VisibilityProperty = 'hidden';
        } else {
            // otherwise loop over all the known prefixes until we find one
            for (var i = 0; i < prefixes.length; i++) {
                if ((prefixes[i] + 'Hidden') in document)
                    SoVa.VisibilityProperty = prefixes[i] + 'Hidden';
                break;
            }
        }
        if (SoVa.VisibilityProperty !== "unknowing") {
            var evtname = SoVa.VisibilityProperty.replace(/[H|h]idden/, '') + 'visibilitychange';
            document.addEventListener(evtname, SonosZones.VisibilityChange);
        }
    };
    this.VisibilityChange = function () {
        if (SoVa.VisibilityProperty === "unknowing") return;
        if (document[SoVa.VisibilityProperty]) {
            //hidden somit Events komplett deaktivieren
            //clearTimeout(SoVa.TopologieChangeID);
            //SoVa.SSE_Event_Source.close();
        } else {
            //visibile 
            if (SoVa.SSE_Event_Source.readyState !== SoVa.SSE_Event_Source.OPEN) {
                Eventing();
            }
            var maxTimeToReload = 240000;//000;
            var oldtime = this.VisibilityChangeTimer;
            this.VisibilityChangeTimer = new Date();
            if ((this.VisibilityChangeTimer - oldtime) > maxTimeToReload) {
                console.log("Länger als 10 Minuten versteckt");
                if (typeof SoVa.SSE_Event_Source !== "undefined" && SoVa.SSE_Event_Source.readyState !== SoVa.SSE_Event_Source.CLOSE) {
                    SoVa.SSE_Event_Source.close()
                }
                location.reload();

            } else {
                GetLatestEvents();
            }
        }
    }
}