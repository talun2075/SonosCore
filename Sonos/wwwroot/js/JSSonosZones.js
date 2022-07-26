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

            if (SoDo.deviceLoader.is(":hidden")) {
                SoDo.deviceLoader.show();
            }
            SoDo.devicesWrapper.empty();
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
                SoDo.devicesWrapper.empty();
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
                        $('<div class="groupdevicewrapper"><div id="' + SonosPlayers[p].uuid + '" class="device' + aktdev + '" onclick="SetDevice(\'' + uuid + '\');"><p>' + image + SonosPlayers[uuid].name + '</p>' + SonosZones.GetCordinatedPlayerasStringFormat(zone) + '</div><img id="deviceplayinggif_' + uuid + '" class="deviceplayinggif" ' + playstateimg + ' src="/images/playing.gif"><div id="GroupDevice_' + uuid + '" onclick="SetDeviceGroupFor(\'' + uuid + '\')" class="groupdeviceclass">&nbsp;&nbsp;Gruppe&nbsp;(' + SonosPlayers[uuid].SoftwareGeneration + ')&nbsp;</div><div class="groupdeviceclass groupdeviceclassplay ' + playclass + '" onclick="SonosPlayers[\'' + uuid + '\'].SendTransportState(\'' + playinternal + '\');" id="' + uuid + '_GroupPlayState"></div></div>').appendTo(SoDo.devicesWrapper);
                        zcounter++;
                    }
                }
                if (typeof SonosPlayers[SonosZones.ActiveZoneUUID] !== "undefined" && typeof SonosPlayers[SonosZones.ActiveZoneUUID].playerProperties.transportStateString !== "undefined" && SonosPlayers[SonosZones.ActiveZoneUUID].playerProperties.transportStateString === "PLAYING") {
                    if (!SoDo.playButton.hasClass("aktiv")) {
                        SoDo.playButton.addClass("aktiv");
                    }
                } else {
                    if (SoDo.playButton.hasClass("aktiv")) {
                        SoDo.playButton.removeClass("aktiv");
                    }
                }
                this.ZonesCount = zcounter;
                if (SoDo.deviceLoader.is(":visible")) {
                    SoDo.deviceLoader.hide();
                }
                if (SoDo.groupDeviceShow.is(":hidden")) {
                    SoDo.groupDeviceShow.show();
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
            if (SoDo.audioInButton.is(":hidden")) {
                SoDo.audioInButton.show();
            }
            if (player.playerProperties.currentTrack.stream === true && (player.playerProperties.currentTrack.streamContent === "Audio Eingang" || player.playerProperties.currentTrack.title === "Heimkino")) {
                if (!SoDo.audioInButton.hasClass("akt")) {
                    SoDo.audioInButton.addClass("akt");
                }
            } else {
                if (SoDo.audioInButton.hasClass("akt")) {
                    SoDo.audioInButton.removeClass("akt");
                }
            }
        } else {
            if (SoDo.audioInButton.is(":visible")) {
                SoDo.audioInButton.hide();
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
                if (!SoDo.playButton.hasClass("aktiv")) {
                    SoDo.playButton.addClass("aktiv");
                }
            } else {
                if (SoDo.playButton.hasClass("aktiv")) {
                    SoDo.playButton.removeClass("aktiv");
                }
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
                if (!SoDo.muteButton.hasClass("aktiv")) {
                    SoDo.muteButton.addClass("aktiv");
                }
            } else {
                if (SoDo.muteButton.hasClass("aktiv")) {
                    SoDo.muteButton.removeClass("aktiv");
                }
            }
        }
        var multivolmute = $("#MultiVolumeMute_" + uuid);
        if (multivolmute.length === 1) {
            if (player.playerProperties.mute === true) {
                if (!multivolmute.hasClass("aktiv"))
                    multivolmute.addClass("aktiv");
            } else {
                if (multivolmute.hasClass("aktiv"))
                    multivolmute.removeClass("aktiv");
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
        let curt = player.playerProperties.currentTrack;
        if (typeof player === "undefined") return;

        if (!this.CheckStreamShowElements(uuid) && (curt.duration === null || curt.duration.totalSeconds === 0)) {
            if (SoDo.runtimeCurrentSong.is(":visible")) {
                SoDo.runtimeCurrentSong.hide();
            }
            if (SoDo.runtimeSlider.is(":visible")) {
                SoDo.runtimeSlider.hide();
            }
        } else {
            //Hier nun die Erweiterung machen. Um mit Stunden und Minuten zu Arbeiten
            if (SoDo.runtimeCurrentSong.is(":hidden")) {
                SoDo.runtimeCurrentSong.show();
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
            if (SoDo.runtimeRelTime.html() !== reltimedom) {
                SoDo.runtimeRelTime.html(reltimedom);
            }
            //let durtimedom = curt.duration.totalSeconds.toString().toHHMMSS();
            let durtimedom = curt.duration.stringWithoutZeroHours;
            if (SoDo.runtimeDuration.html() !== durtimedom) {
                SoDo.runtimeDuration.html(durtimedom);
            }
        }
    };//done
    this.RenderCrossFadeMode = function (uuid) {
        if (typeof SonosPlayers[uuid] === "undefined") return;
        if (SonosPlayers[uuid].playerProperties.currentCrossFadeMode === true) {
            if (!SoDo.fadeButton.hasClass("aktiv")) {
                SoDo.fadeButton.addClass("aktiv");
            }
        } else {
            if (SoDo.fadeButton.hasClass("aktiv")) {
                SoDo.fadeButton.removeClass("aktiv");
            }
        }
    };//done
    this.RenderPlayMode = function (uuid) {
        if (typeof SonosPlayers[uuid] === "undefined") return;
        var v = SonosPlayers[uuid].playerProperties.currentPlayModeString;
        switch (v) {
            case "NORMAL":
                if (SoDo.repeatButton.hasClass("aktiv")) {
                    SoDo.repeatButton.removeClass("aktiv");
                }
                if (SoDo.repeatButton.hasClass("aktiv_one")) {
                    SoDo.repeatButton.removeClass("aktiv_one");
                }
                if (SoDo.shuffleButton.hasClass("aktiv")) {
                    SoDo.shuffleButton.removeClass("aktiv");
                }
                break;
            case "REPEAT_ALL":
                if (!SoDo.repeatButton.hasClass("aktiv")) {
                    SoDo.repeatButton.addClass("aktiv");
                }
                if (SoDo.repeatButton.hasClass("aktiv_one")) {
                    SoDo.repeatButton.removeClass("aktiv_one");
                }
                if (SoDo.shuffleButton.hasClass("aktiv")) {
                    SoDo.shuffleButton.removeClass("aktiv");
                }
                break;
            case "REPEAT_ONE":
                if (SoDo.repeatButton.hasClass("aktiv")) {
                    SoDo.repeatButton.removeClass("aktiv");
                }
                if (!SoDo.repeatButton.hasClass("aktiv_one")) {
                    SoDo.repeatButton.addClass("aktiv_one");
                }
                if (SoDo.shuffleButton.hasClass("aktiv")) {
                    SoDo.shuffleButton.removeClass("aktiv");
                }
                break;
            case "SHUFFLE_NOREPEAT":
                if (SoDo.repeatButton.hasClass("aktiv")) {
                    SoDo.repeatButton.removeClass("aktiv");
                }
                if (SoDo.repeatButton.hasClass("aktiv_one")) {
                    SoDo.repeatButton.removeClass("aktiv_one");
                }
                if (!SoDo.shuffleButton.hasClass("aktiv")) {
                    SoDo.shuffleButton.addClass("aktiv");
                }
                break;
            case "SHUFFLE":
                if (!SoDo.repeatButton.hasClass("aktiv")) {
                    SoDo.repeatButton.addClass("aktiv");
                }
                if (SoDo.repeatButton.hasClass("aktiv_one")) {
                    SoDo.repeatButton.removeClass("aktiv_one");
                }
                if (!SoDo.shuffleButton.hasClass("aktiv")) {
                    SoDo.shuffleButton.addClass("aktiv");
                }
                break;
            case "SHUFFLE_REPEAT_ONE":
                if (SoDo.repeatButton.hasClass("aktiv")) {
                    SoDo.repeatButton.removeClass("aktiv");
                }
                if (!SoDo.repeatButton.hasClass("aktiv_one")) {
                    SoDo.repeatButton.addClass("aktiv_one");
                }
                if (!SoDo.shuffleButton.hasClass("aktiv")) {
                    SoDo.shuffleButton.addClass("aktiv");
                }
                break;
            default:
                if (SoDo.repeatButton.hasClass("aktiv")) {
                    SoDo.repeatButton.removeClass("aktiv");
                }
                if (SoDo.repeatButton.hasClass("aktiv_one")) {
                    SoDo.repeatButton.removeClass("aktiv_one");
                }
                if (SoDo.shuffleButton.hasClass("aktiv")) {
                    SoDo.shuffleButton.removeClass("aktiv");
                }
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
        var htmlval = parseInt(SoDo.labelVolume.html());
        if (isNaN(htmlval) || htmlval !== player.playerProperties.groupRenderingControl_GroupVolume) {
            SoDo.labelVolume.html(player.playerProperties.groupRenderingControl_GroupVolume);
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
            if (SoDo.playlistLoader.is(":visible")) {
                SoDo.playlistLoader.slideUp();
            }
        }
        //Ende Neu wegen Stream
        return;
    };//done
    this.RenderPlaylistCounter = function (uuid) {
        if (typeof SonosPlayers[uuid] === "undefined") return;
        //Neu wegen Stream
        if (!this.CheckStreamShowElements(uuid)) {
            if (SoDo.playlistCount.is(":visible")) {
                SoDo.playlistCount.hide();
            }
        } else {
            if (SoDo.playlistCount.is(":hidden")) {
                SoDo.playlistCount.show();
            }
            if (parseInt(SoDo.playlistAkt.html()) !== SonosPlayers[uuid].playerProperties.currentTrackNumber) {
                SoDo.playlistAkt.html(SonosPlayers[uuid].playerProperties.currentTrackNumber);
            }
            if (SonosPlayers[uuid].playerProperties.numberOfTracks == 0 && SonosPlayers[uuid].playerProperties.playlist.playListItems.length > 0) {
                SonosPlayers[uuid].playerProperties.numberOfTracks = SonosPlayers[uuid].playerProperties.playlist.playListItems.length;
            }
            if (parseInt(SoDo.playlistTotal.html()) !== SonosPlayers[uuid].playerProperties.numberOfTracks) {
                SoDo.playlistTotal.html(SonosPlayers[uuid].playerProperties.numberOfTracks);
            }
        }

    };//done
    this.RenderNextTrack = function (uuid) {
        //Stream
        var player = SonosPlayers[uuid];
        if (typeof player === "undefined") return;
        //new
        if (!SonosZones.CheckStringIsNullOrEmpty(player.playerProperties.nextTrack.uri)) {
            if (SoDo.nextSongWrapper.is(":hidden")) {
                SoDo.nextSongWrapper.show();
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
            if (SoDo.nextTitle.text() !== text) {
                SoDo.nextTitle.text(text);
            }

            if (SonosZones.CheckStringIsNullOrEmpty(player.playerProperties.nextTrack.albumArtURI)) {
                if (SoDo.nextcover.is(":visible")) {
                    SoDo.nextcover.hide();
                }
            } else {
                if (SoDo.nextcover.is(":hidden")) {
                    SoDo.nextcover.show();
                }
                //Wenn nicht mit /getaa startet nicht BASEURL nehmen, dann ist das evtl. DLNA
                var albumart = 'http://' + player.playerProperties.baseUrl + player.playerProperties.nextTrack.albumArtURI;
                if (!player.playerProperties.nextTrack.albumArtURI.startsWith("/getaa")) {
                    albumart = player.playerProperties.nextTrack.albumArtURI;
                }
                if (SoDo.nextcover.attr("src") !== albumart) {
                    SoDo.nextcover.attr("src", albumart);
                }
                UpdateImageOnErrors();
            }

        } else {
            //todo: prüfen auf is empty und ob die playlist geladen ist, dann kann man ein nächten track anzeigen? Stream?
            if (SoDo.nextSongWrapper.is(":visible")) {
                SoDo.nextSongWrapper.hide();
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
                SoDo.currentplaylistwrapper.scrollTop(0);
                var ctop = contactTopPosition.position().top;
                SoDo.currentplaylistwrapper.scrollTop(ctop - 30);
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
            if (!SoDo.sleepModeButton.hasClass("aktiv")) {
                SoDo.sleepModeButton.addClass("aktiv");
            }
            if (SoDo.sleepModeState.text() !== duration) {
                SoDo.sleepModeState.text(duration);
            }
        } else {
            if (SoDo.sleepModeButton.hasClass("aktiv")) {
                SoDo.sleepModeButton.removeClass("aktiv");
            }
            if (SoDo.sleepModeState.text() !== "") {
                SoDo.sleepModeState.text("");
            }
        }
    };//done
    this.RenderRatingFilter = function (uuid) {
        var player = SonosPlayers[uuid];
        if (typeof player === "undefined") return;
        var plrating = player.RatingFilter;
        if (typeof plrating === "undefined") return;
        SoDo.filterListRatingBar.removeClass("rating_bar_aktiv");
        SoDo.filterListGelegenheitChilds.removeClass("selected");
        SoDo.filterListGeschwindigkeitChilds.removeClass("selected");
        SoDo.filterListStimmungChilds.removeClass("selected");
        SoDo.filterListAlbumInterpretChilds.removeClass("selected");
        var def = true;
        if (SoDo.filterListRatingBarBomb.hasClass("rating_bar_aktiv")) {
            SoDo.filterListRatingBarBomb.removeClass("rating_bar_aktiv");
        }
        if (plrating.rating > -2) {
            if (plrating.rating === -1) {
                SoDo.filterListRatingBarBomb.addClass("rating_bar_aktiv");
            } else {
                $("#filter_rating_bar_" + plrating.rating).addClass("rating_bar_aktiv");
            }
            def = false;
        }
        if (plrating.stimmung !== 6) {
            $("#Filterstimmung_" + plrating.stimmung).addClass("selected");
            def = false;
        }
        if (plrating.gelegenheit !== 5) {
            $("#Filtergelegenheit_" + plrating.gelegenheit).addClass("selected");
            def = false;
        }
        if (plrating.geschwindigkeit !== 6) {
            $("#Filtergeschwindigkeit_" + plrating.geschwindigkeit).addClass("selected");
            def = false;
        }
        if (plrating.albpumInterpretFilter !== "unset") {
            $("#AlbumArtist" + plrating.albpumInterpretFilter).addClass("selected");
            def = false;
        }
        if (def === false) {
            if (!SoDo.filterListButton.hasClass("akt")) {
                SoDo.filterListButton.addClass("akt");
            }
        } else {
            if (SoDo.filterListButton.hasClass("akt")) {
                SoDo.filterListButton.removeClass("akt");
            }
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
            if (SoDo.playlistwrapper.children().length === 0 || override === true) {
                SoDo.globalPlaylistLoader.slideDown();
                $(".playlist").remove();
                if (this.AllPlaylists.length === 0) return;
                $.each(this.AllPlaylists, function (i, item) {
                    var playlisttype;
                    var acclass = "";
                    if (item.description === "M3U") {
                        playlisttype = "m3u";
                    } else {
                        playlisttype = "sonos";
                    }
                    if (item.uri === uri) {
                        acclass = "aktiv";
                    }
                    //Wiedergabeliste befüllen
                    $('<div id="Playlist_' + i + '" class="playlist ' + playlisttype + ' ' + acclass + '"><div onclick="SonosZones.ReplacePlaylist(' + i + ');">' + item.title + '</DIV></div>').appendTo(SoDo.playlistwrapper);
                });
                SoDo.globalPlaylistLoader.slideUp();
            } else {
                //Es wurde schon gerendert und somit nur noch die angezeigte wechseln.
                $(".playlist").removeClass("aktiv");
                if (this.AllPlaylists.length === 0 || this.CheckStringIsNullOrEmpty(uri)) return;
                for (var i = 0; i < this.AllPlaylists.length; i++) {
                    if (this.AllPlaylists[i].uri === uri) {
                        $("#Playlist_" + i).addClass("aktiv");
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
        SoDo.playlistLoader.slideDown();
        SoDo.browseLoader.slideDown();
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
            SoDo.browseLoader.slideUp();
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
                location.reload();
            } else {
                GetLatestEvents();
            }
        }
    }
}