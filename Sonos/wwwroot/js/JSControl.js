﻿"use strict";
//Wird benutzt um die Zeit zu rendern. 
//String.prototype.toHHMMSS = function () {
//    var sec_num = parseInt(this, 10); // don't forget the second param
//    var hours = Math.floor(sec_num / 3600);
//    var minutes = Math.floor((sec_num - (hours * 3600)) / 60);
//    var seconds = sec_num - (hours * 3600) - (minutes * 60);

//    if (hours < 10) { hours = "0" + hours; }
//    if (minutes < 10) { minutes = "0" + minutes; }
//    if (seconds < 10) { seconds = "0" + seconds; }
//    return hours + ':' + minutes + ':' + seconds;
//}


window.onerror = Fehlerbehandlung;
var debug = false; //Wenn true wird kein Refesh gemacht		
var wroteDebugInfos = false;
function Fehlerbehandlung(Nachricht, Datei, Zeile) {
    var fehler = "Fehlermeldung:\n" + Nachricht + "\n" + Datei + "\n" + Zeile;
    alert(fehler);

    return true;
}
function WroteSysteminfos() {
    var fehler = "SonosZones:ActiveSonosZone:" + SonosZones.ActiveZoneUUID + "<br />" +
        "SonosZones:ActiveName:" + SonosZones.ActiveZoneName + "<br />" +
        "SonosPlayer:Name:" + SonosPlayers[SonosZones.ActiveZoneUUID].ZoneName + "<br />" +
        "SonosPlayer:Baseurl:" + SonosPlayers[SonosZones.ActiveZoneUUID].baseUrl + "<br />" +
        "SonosPlayer:NumberofTRacks:" + SonosPlayers[SonosZones.ActiveZoneUUID].numberOfTracks + "<br />" +
        "SonosPlayer:CurrentTRackTitel:" + SonosPlayers[SonosZones.ActiveZoneUUID].playerProperties.currentTrack.title + "<br />" +
        "SonosPlayer:Active:" + SonosPlayers[SonosZones.ActiveZoneUUID].ActiveZone + "<br />" +
        "GetTopologieID:" + SoVa.TopologieChangeID + "<br />" +
        "GetAktsongInfoID:" + SoVa.GetAktSongInfoTimerID + "<br />" +
        "AktuellerSong aus der Playlist:" + SoVa.aktcurpopdown + "<br />" +
        "API PlayerURL:" + SoVa.apiPlayerURL + "<br />" +
        "innerHeight:" + window.innerHeight + "<br />" +
        "innerWidth:" + window.innerWidth + "<br />" +
        "clientHeight:" + document.body.clientHeight + "<br />" +
        "clientWidth:" + document.body.clientWidth + "<br />";
    return fehler;
}
function WroteDebugInfos() {
    if (wroteDebugInfos === false) {
        SoDo.lyricWrapper.innerHTML = "";
    }
    wroteDebugInfos = true;
    if (typeof SoDo.lyric === "undefined") {
        SoDo.lyric = document.getElementById("Lyricbox");
        console.log("SODO.Lyric war nicht definiert.");
    }
    if (typeof SoDo.lyricWrapper === "undefined") {
        SoDo.lyricWrapper = document.getElementById("Lyricboxwrapper");
        console.log("SODO.lyricWrapper war nicht definiert.");
    }
    SonosWindows(SoDo.lyric);
    if (IsVisible(SoDo.lyric)) {
        SoDo.lyricWrapper.innerHTML += '<div>' + WroteSysteminfos() + '</div>'
    }
}
function AddDebugInfos(mess) {
    if (wroteDebugInfos === true) {
        SoDo.lyricWrapper.innerHTML += '<div>' + mess + '</div>'
    }
}
/*
Erklärungen:
    	
URL Parameter
Mit dem URL Parameter device kann man eine vorauswahl treffen, welches Gerät ausgewählt werden soll.
Beispiel: device=Wohnzimmer
Ohne den Parameter wird immer der erste Eintrag vorausgewählt
    	
Java Variablen
apiDeviceURL

Dieser Parameter wird genommen um die API zu initialisieren. Diese liefert die Geräte zurück
apiPlayerURL	
Dieser Parameter liefert alle Informationen zurück und wird für fast alle Functionen benötigt
*/
function InitSystem() {

    //Polyfill IpadWand
    if (!String.prototype.startsWith) {
        String.prototype.startsWith = function (searchString, position) {
            position = position || 0;
            return this.indexOf(searchString, position) === position;
        };
    }

    window.SoVa = new SonosVariablen();
    window.SoDo = new SonosDOMObjects();
    window.SonosZones = new SonosZonesObject();
    window.addEventListener('beforeunload', function () {
        //Close SSE on site leaving
        if (typeof SoVa.SSE_Event_Source !== "undefined" && SoVa.SSE_Event_Source.readyState !== SoVa.SSE_Event_Source.CLOSE) {
            SoVa.SSE_Event_Source.close();
        }
    });
    LoadDevices();
    SoDo.lyricButton.addEventListener("click", function () {
        ShowPlaylistLyricCurrent();
    });
    //Changes für das Exportieren /Speichern von Playlisten festhalten
    SoDo.saveExportPlaylistSwitch.addEventListener("change", function () {
        var c = SoDo.saveExportPlaylistSwitch.checked;
        if (c) {
            SoDo.saveQueue.setAttribute("placeholder", SoVa.exportPlaylistInputText);
            SoVa.exportplaylist = true;
        } else {
            SoDo.saveQueue.setAttribute("placeholder", SoVa.savePlaylistInputText);
            SoVa.exportplaylist = false;
        }
    });
    //ScrollEvents
    //Prüfvariable wird gesetzt, wenn gescrollt wird. Manuelles Scrollen
    SoDo.currentplaylistwrapper.addEventListener("scroll", function () {
        SoVa.currentplaylistScrolled = true;
    });
    //Prüfen ob nur noch current Ratings angezeigt werden soll.
    SoDo.onlyCurrentSwitch.addEventListener("change", function () {
        var c = SoDo.onlyCurrentSwitch.checked;
        if (c) {
            SoVa.ratingonlycurrent = true;
        } else {
            SoVa.ratingonlycurrent = false;
        }
    });
    //Initialisierung Musikindexaktualisierung
    SoDo.musikIndex.addEventListener("click", function () {
        UpdateMusicIndex();
    });
    //Ratingmine änderungen abfangen
    SoDo.ratingMineSelector.addEventListener("change", function () {
        SetRatingMine(SoDo.ratingMineSelector.value);
    });
    SoDo.filterListButton.addEventListener("click", function () {
        SonosWindows(SoDo.filterListBox);
    });
    //Settingswurde gedrückt
    SoDo.settingsbutton.addEventListener("click", function () {
        SonosWindows(SoDo.settingsBox);
        SoDo.settingsbutton.classList.toggle("akt");
    });
    //Settingswurde gedrückt
    SoDo.settingsClosebutton.addEventListener("click", function () {
        SonosWindows(SoDo.settingsBox, true);
        SoDo.settingsbutton.classList.toggle("akt");
    });
    SoDo.BrowseClosebutton.addEventListener("click", function () {
        BrowsePress();
        console.log("ok");
    });
    //Events verarbeiten, wenn ein Button geklickt wurde.
    SoDo.nextButton.addEventListener("click", function () {
        //Curenttrack ändern, danach die Nummer Ändern und somit den Nexttrack rendern.
        if (SonosZones.CheckActiveZone()) {
            doit('Next');
            if (SoVa.ratingonlycurrent === false && IsVisible(SoDo.ratingListBox)) {
                SonosWindows(SoDo.ratingListBox, true);
            }
        }
    });
    SoDo.prevButton.addEventListener("click", function () {
        if (SonosZones.CheckActiveZone()) {
            SoVa.currentplaylistScrolled = false;
            if (SonosPlayers[SonosZones.ActiveZoneUUID].playerProperties.transportStateString !== "REPEAT_ALL" && SonosPlayers[SonosZones.ActiveZoneUUID].playerProperties.transportStateString !== "SHUFFLE" && SonosPlayers[SonosZones.ActiveZoneUUID].playerProperties.currentTrackNumber === 1) {
                return false;
            }
            doit('Previous');
            if (SoVa.ratingonlycurrent === false && IsVisible(SoDo.ratingListBox)) {
                SonosWindows(SoDo.ratingListBox, true);
            }
            return true;
        }
        return true;
    });

    //Abspieldauerslider
    SoDo.runtimeSlider.oninput = function () {
        let sec_num = parseInt(this.value, 10); // don't forget the second param
        let hours = Math.floor(sec_num / 3600);
        let minutes = Math.floor((sec_num - (hours * 3600)) / 60);
        let seconds = sec_num - (hours * 3600) - (minutes * 60);
        let convertedstring, hoursstring
        convertedstring = "";
        if (hours < 10) { hoursstring = "0" + hours; } else { hoursstring = hours; }
        if (minutes < 10) { minutes = "0" + minutes; }
        if (seconds < 10) { seconds = "0" + seconds; }
        if (hours > 0) {
            convertedstring = hoursstring + ":";
        }
        convertedstring += minutes + ':' + seconds;
        SoDo.runtimeRelTime.textContent = convertedstring;
    }
    SoDo.runtimeSlider.onchange = function () {
        let sec_num = parseInt(this.value, 10); // don't forget the second param
        let hours = Math.floor(sec_num / 3600);
        let minutes = Math.floor((sec_num - (hours * 3600)) / 60);
        let seconds = sec_num - (hours * 3600) - (minutes * 60);
        let convertedstring, hoursstring
        convertedstring = "";
        if (hours < 10) { hoursstring = "0" + hours; } else { hoursstring = hours; }
        if (minutes < 10) { minutes = "0" + minutes; }
        if (seconds < 10) { seconds = "0" + seconds; }
        if (hours > 0) {
            convertedstring = hoursstring + ":";
        }
        convertedstring += minutes + ':' + seconds;
        SoDo.runtimeRelTime.textContent = convertedstring;
        doitValue("Seek", this.value);
    }
    //Lautstärkeregler initialisieren.
    SoDo.volumeSlider.onchange = function () {
        //Prüfen, ob die Läutstärke über 20% verändert wird. 
        if (this.value > SonosPlayers[SonosZones.ActiveZoneUUID].playerProperties.groupRenderingControl_GroupVolume && this.value - SonosPlayers[SonosZones.ActiveZoneUUID].playerProperties.groupRenderingControl_GroupVolume > SoVa.volumeConfirmCounter) {
            var answer = confirm("Du willst die Lautstärke um " + SoVa.volumeConfirmCounter + " von 100 Schritten erhöhen. Klicke Ok, wenn das gewollt ist");
            if (!answer) {
                SoDo.labelVolume.textContent = SonosPlayers[SonosZones.ActiveZoneUUID].playerProperties.groupRenderingControl_GroupVolume;
                SoDo.volumeSlider.value = SonosPlayers[SonosZones.ActiveZoneUUID].playerProperties.groupRenderingControl_GroupVolume;
                return false;
            }
        }
        SonosPlayers[SonosZones.ActiveZoneUUID].playerProperties.groupRenderingControl_GroupVolume = this.value;
        SetGroupVolumeDevice(SonosZones.ActiveZoneUUID, this.value);
        return true;
    }
    SoDo.volumeSlider.oninput = function () {
        SoDo.labelVolume.textContent = this.value;
    }
    //Autovervollständigung
    SoDo.saveQueue.addEventListener("keyup", function (e) {
        try {
            // 'enter' key was pressed
            var suggest = SoDo.suggestionInput;
            var code = e.keyCode ? e.keyCode : e.which;
            if (code === 13) {
                SoDo.saveQueue.value = suggest.value;
                suggest.value = "";
                return false;
            }

            // some other key was pressed
            var needle = SoDo.saveQueue.value;

            // is the field empty?
            if ((needle.trim()).length < 1) {
                suggest.value = "";
                return false;
            }
            var foundeplaylist;
            var foundeplaylist;
            SonosZones.AllPlaylists.forEach(function (item) {
                if (item.title.toLowerCase().startsWith(needle.toLowerCase())) {
                    foundeplaylist = item;
                    return;
                }
            });
            if (typeof foundeplaylist !== "undefined") {
                suggest.value = foundeplaylist.title;
            } else {
                suggest.value = "";
            }
            return true;
        }
        catch (Ex) {
            alert("Es ist ein Fehler beim SoDo.saveQueue.keyup aufgetreten:<br>" + Ex.message);
        }
    });
    if (wroteDebugInfos === true) {
        SetVisible(SoDo.debug);
    }
} //Ende Init


function RepairActiveZone() {
    SonosAjax("FillPlayerPropertiesDefaults", "", SonosZones.ActiveZoneUUID, true);
}
//Gerät Url ermitteln und laden
var GetZonesTimer = 0;

function LoadDevices() {
    try {
        SoVa.urldevice = GetURLParameter('device').toLowerCase();
        clearTimeout(SoVa.TopologieChangeID);
        SoVa.GetZonesTimer = window.setTimeout("GetZones()", 100);
    }
    catch (Ex) {
        alert("Es ist ein Fehler beim LoadDevices aufgetreten:<br>" + Ex.message);
    }
} //Ende LoadDevices
/*Zonemanagement*/

var SonosPlayers = {};
function GetZones() {
    try {
        clearTimeout(SoVa.GetZonesTimer);
        SonosAjax("GetPlayers").success(function (data) {
            if (typeof Promise === "undefined") {
                alert("No Promise JS Controll273");
                return;//Problem beim IE
            }
            var plpromise = new Promise(function (resolve, reject) {
                for (var i = 0; i < data.length; i++) {
                    var u = data[i].uuid;
                    if (typeof SonosPlayers[u] === "undefined") {
                        SonosPlayers[u] = new SonosPlayer(data[i].uuid, data[i].name, data[i].softwareGeneration);
                        SonosPlayers[u].CheckPlayerProperties(data[i].playerProperties);
                        SonosPlayers[u].RatingFilter = data[i].ratingFilter;
                    }
                }
                resolve(true);
            });
            plpromise.then(function () {
                window.setTimeout("SonosZones.RenderDevices()", 300);
                window.setTimeout("Eventing()", 1500);
                SoVa.TopologieChangeID = window.setTimeout("GetPlayerLastChanges()", 20000);
                window.setTimeout("SonosZones.CheckVisibility(false)", 1000);
                window.setTimeout("GetMusicIndexInProgress()", 4000);
            })
        }).fail(function (data) {
            console.log("getplayers Fehler");
            console.log(data);
            window.setTimeout("GetZones()", 1500);
        });
    }
    catch (Ex) {
        alert("Es ist ein Fehler bei GetZones aufgetreten:<br>" + Ex.message);
    }
}
//Erweitert das Device Fenster um die Gruppen Buttons
function GroupDeviceShow() {
    try {
        if (SoVa.groupDeviceShowBool === false) {
            var wbydevice = "350px";
            AddClass(SoDo.devicesWrapper, "groupdevicesshown");
            SoDo.devicesWrapper.style.zIndex = SoVa.szindex + 100;
            SoDo.devicesWrapper.style.maxHeight = "400px";
            SoDo.devices.style.maxHeight = "400px";
            SoDo.devices.style.maxWidth = wbydevice;
            let gdv = SoDo.devicesWrapper.querySelectorAll(".groupdeviceclass");
            SoDo.devicesWrapper.querySelectorAll(".groupdeviceclass").forEach(function (item) { item.style.display = "table" });
            SoDo.groupDeviceShow.textContent = "<<";
            SoVa.groupDeviceShowBool = true;
        } else {
            RemoveClass(SoDo.devicesWrapper, "groupdevicesshown");
            SoDo.devicesWrapper.style.zIndex = 100;
            SoDo.devicesWrapper.style.maxHeight = "230px";
            SoDo.devices.style.maxHeight = "230px";
            SoDo.devices.style.maxWidth = "180px";
            SoDo.groupDeviceShow.textContent = ">>";
            SoVa.groupDeviceShowBool = false;
            SoDo.devicesWrapper.querySelectorAll(".groupdeviceclass").forEach(function (item) { item.style.display = "none" });
        }
    }
    catch (Ex) {
        alert("Es ist ein Fehler beim GroupDeviceShow aufgetreten:<br>" + Ex.message);
    }
};//done
//Auswahlliste um Gruppen zu bilden.
function SetDeviceGroupFor(v) {
    try {
        var player = SonosPlayers[v];
        if (SoVa.setGroupMemberInitSoftWareGen != player.SoftwareGeneration) {
            SoDo.setGroupMembers.innerHTML = "";
            SoDo.setGroupMembers.innerHTML = '<br>';
            var prop = Object.getOwnPropertyNames(SonosPlayers);
            let setgroumemberstext = "";
            for (var i = 0; i < prop.length; i++) {
                var p = prop[i];
                var feplayer = SonosPlayers[p];
                if (feplayer.SoftwareGeneration == player.SoftwareGeneration) {
                    setgroumemberstext += '<div class="groupcheck"><input type="checkbox" id="groupcheckchecker_' + p + '" class="groupcheckchecker" value="' + p + '"><span onclick="this.previousElementSibling.checked = !this.previousElementSibling.checked;">' + SonosPlayers[p].name + '</span></div>';
                }
            }
            SoDo.setGroupMembers.innerHTML += setgroumemberstext;
            SoDo.setGroupMembers.innerHTML += '<div id="Groupcheckset" onclick="SetGroup()">Set</div>';
            SoDo.setGroupMembers.innerHTML += '<div id="GroupCheckClose" onclick="HideGroupFor()">X</div>';
            SoVa.setGroupMemberInitSoftWareGen = player.SoftwareGeneration;
        }
        SoDo.setGroupMembers.querySelectorAll(".groupcheck").forEach(function (item) { if (item.checked) item.checked = false; });
        //Zonen durchlaufen
        var cordzones = SonosZones.ZonesList[v].CoordinatedUUIDS;
        for (var z = 0; z < cordzones.length; z++) {
            document.getElementById("groupcheckchecker_" + cordzones[z].uuid).checked = true;
        }
        SoVa.masterPlayer = v;
        SonosWindows(SoDo.setGroupMembers, false, { overlay: true });
    }
    catch (Ex) {
        alert("Es ist ein Fehler beim SetDeviceGroupFor aufgetreten:<br>" + Ex.message);
    }
};//done
//Schließen der Gruppenauswahl
function HideGroupFor() {
    SonosWindows(SoDo.setGroupMembers, true);
};//done
//Setzen der Gruppen.
function SetGroup() {
    try {
        HideGroupFor();
        //setGroupMemberInitialisierung=false;
        GroupDeviceShow();
        //Ausgewählte ermitteln
        var g = [];
        SoDo.setGroupMembers.querySelectorAll(".groupcheckchecker:checked").forEach(function (item) { 
            g.push(item.value);
        })
        if (g.length === 0) {
            var ret = confirm("Wenn nicht mindestens ein Player ausgewählt ist, werden alle Player angehalten. Soll das passieren?");
            if (ret === false) return;
            g = "leer";
        }
        //Daten senden
        SonosAjax("SetGroups", { '': g }).success(function () {});
    }
    catch (Ex) {
        alert("Es ist ein Fehler beim SetGroup aufgetreten:<br>" + Ex.message);
    }
}
//Setzt den entsprechenden Player als Abspieler
function SetDevice(dev) {
    //Volumne Bar reseten.
    try {
        SonosWindows(SoDo.multiVolume, true);
        SonosWindows(SoDo.ratingListBox, true);
        SoDo.onlyCurrentSwitch.checked = false;
        SoVa.ratingonlycurrent = false;
        RemoveClass(SoDo.devicesWrapper.querySelector('.akt_device'), "akt_device")
        AddClass(document.getElementById(dev), "akt_device");
        SonosZones.RenderActiveZone(dev);
    }
    catch (Ex) {
        alert("Es ist ein Fehler beim SetDevice aufgetreten:<br>" + Ex.message);
    }
} //Ende SetDevice
//Sucht den übergebenen Parameter in der URL um ein Device auszuwählen.
function GetURLParameter(sParam) {
    try {
        var sPageURL = window.location.search.substring(1);
        var sURLVariables = sPageURL.split('&');
        for (var i = 0; i < sURLVariables.length; i++) {
            var sParameterName = sURLVariables[i].split('=');
            var devicesParameterName = sParameterName[0].toLowerCase();
            if (devicesParameterName === sParam) {
                return decodeURIComponent(sParameterName[1]);
            }
        }
        return "leer";
    }
    catch (Ex) {
        alert("Es ist ein Fehler beim GetURLParameter aufgetreten:<br>" + Ex.message);
    }
}
//Wenn auf Play gedrückt wird.
function PlayPress() {
    if (SonosZones.CheckActiveZone()) {
        if (SonosPlayers[SonosZones.ActiveZoneUUID].playerProperties.transportStateString === "PLAYING") {
            SonosPlayers[SonosZones.ActiveZoneUUID].SendTransportState("PAUSED_PLAYBACK");
        } else {
            SonosPlayers[SonosZones.ActiveZoneUUID].SendTransportState("PLAYING");
        }
    }
}//done
//Es wurde einmal Play aus der Playlist gedrückt
function PlayPressSmall(k) {
    try {
        var PressKey = GetIDfromCurrentPlaylist(k.parentNode.parentElement.id) + 1;
        if (SonosPlayers[SonosZones.ActiveZoneUUID].playerProperties.currentTrackNumber === PressKey) {
            //Play bei aktuellem Song
            PlayPress();
        } else {
            doitValue("SetSongInPlaylist", PressKey);
        }
    }
    catch (Ex) {
        alert("Es ist ein Fehler beim PlayPressSmall aufgetreten:<br>" + Ex.message);
    }
};//done
//Setzen des Wiedergabemodus
function SetPlaymode(v) {
    SonosPlayers[SonosZones.ActiveZoneUUID].SendPlayMode(v);
};//done
//Übergangbutton geklickt.
function SetFade() {
    SonosAjax("SetFadeMode");
    SonosPlayers[SonosZones.ActiveZoneUUID].playerProperties.currentCrossFadeMode = !SonosPlayers[SonosZones.ActiveZoneUUID].playerProperties.currentCrossFadeMode
    SonosZones.RenderCrossFadeMode(SonosZones.ActiveZoneUUID);
};//done
//{ Lautstärke
//Setzt Mute
function SetMute(rincon) {
    if (typeof rincon === "undefined") {
        rincon = SonosZones.ActiveZoneUUID;
        SonosPlayers[rincon].playerProperties.groupRenderingControl_GroupMute != SonosPlayers[rincon].playerProperties.groupRenderingControl_GroupMute;
    }
    SonosPlayers[rincon].playerProperties.mute != SonosPlayers[rincon].playerProperties.mute;
    SonosAjax("SetMute", "", rincon).success(function () {
        SonosZones.RenderMute(rincon);
    });
};//done
//Lautstärke anpassen
function SetVolume(k) {
    //Multivolume
    k = parseInt(k);
    var cordplayer = SonosPlayers[SonosZones.ActiveZoneUUID].playerProperties.zoneGroupTopology_ZonePlayerUUIDsInGroup;
    if (cordplayer.length > 1) {
        SoDo.multiVolume.innerHTML = "";
        SonosWindows(SoDo.multiVolume, false, { overlay: true, selecteddivs: [SoDo.playButton, SoDo.muteButton, SoDo.nextButton] });
        SoDo.multiVolume.innerHTML = '<div id="multivolume_close" OnClick="SonosWindows(SoDo.multiVolume, true);">X</DIV>'
        //Hier nun den Player für alle machen.
        SoDo.multiVolume.innerHTML += '<div id="MultivolumeAll">Alle<DIV id="MultivolumesliderAll" class="multivolumeslider"></div><div class="multivolumesliderVolumeNumber" id="MultivolumeAllNumber">' + SonosPlayers[SonosZones.ActiveZoneUUID].playerProperties.groupRenderingControl_GroupVolume + '</DIV></DIV>';
        $("#MultivolumesliderAll").slider({
            orientation: "horizontal",
            range: "min",
            min: 1,
            max: 100,
            value: SonosPlayers[SonosZones.ActiveZoneUUID].playerProperties.groupRenderingControl_GroupVolume,
            stop: function (event, ui) {
                //Prüfen, ob die Läutstärke über 80% verändert wird. 
                if (ui.value > SonosPlayers[SonosZones.ActiveZoneUUID].playerProperties.groupRenderingControl_GroupVolume && ui.value - SonosPlayers[SonosZones.ActiveZoneUUID].playerProperties.groupRenderingControl_GroupVolume > SoVa.volumeConfirmCounter) {
                    var answer = confirm("Du willst die Lautstärke um " + SoVa.volumeConfirmCounter + " von 100 Schritten erhöhen. Klicke Ok, wenn das gewollt ist");
                    if (!answer) {
                        $("#multivolumesliderAll").slider({ value: SonosPlayers[SonosZones.ActiveZoneUUID].playerProperties.groupRenderingControl_GroupVolume });
                        return false;
                    }
                    SetGroupVolumeDevice(SonosZones.ActiveZoneUUID, ui.value);
                    SonosPlayers[SonosZones.ActiveZoneUUID].playerProperties.groupRenderingControl_GroupVolume = ui.value;
                    return true;
                }
                SetGroupVolumeDevice(SonosZones.ActiveZoneUUID, ui.value);
                return true;
            },
            slide: function (event, ui) {
                $("#MultivolumeAllNumber").html(ui.value);
                SoDo.volumeSlider.slider({ value: ui.value });
                SoDo.labelVolume.textContent = ui.value;
            }
        });


        $.each(cordplayer, function (i, item) {
            var player = SonosPlayers[item];
            var name = player.name;
            var volume = player.playerProperties.volume;
            var muteactive = "";
            if (player.playerProperties.mute === true) {
                muteactive = SoVa.aktiv;
            }
            SoDo.multiVolume.innerHTML += '<div id="multivolume_' + item + '"><DIV class="multiVolumeNameMuteWrapper"><DIV class="multiVolumeName">' + name + '</DIV><DIV class="multiVolumeMute ' + muteactive + '" id="MultiVolumeMute_' + item + '" onClick="SetMute(\'' + item + '\')"></DIV></DIV><DIV id="Multivolumeslider_' + item + '" class="multivolumeslider"></div><div class="multivolumesliderVolumeNumber" id="MultivolumesliderVolumeNumber_' + item + '">' + volume + '</div></DIV>';
            $("#Multivolumeslider_" + item).slider({
                orientation: "horizontal",
                range: "min",
                min: 1,
                max: 100,
                value: volume,
                stop: function (event, ui) {
                    //Prüfen, ob die Läutstärke über 80% verändert wird. 
                    if (ui.value > volume && ui.value - volume > SoVa.volumeConfirmCounter) {
                        var answer = confirm("Du willst die Lautstärke um " + SoVa.volumeConfirmCounter + " von 100 Schritten erhöhen. Klicke Ok, wenn das gewollt ist");
                        if (!answer) {
                            $("#Multivolumeslider_" + item).slider({ value: volume });
                            $("#MultivolumesliderVolumeNumber_" + item.uuid).html(volume);
                            return false;
                        }
                    }
                    SonosPlayers[item].playerProperties.volume = ui.value;
                    $("#MultivolumesliderVolumeNumber_" + item).html(ui.value);
                    SetVolumeDevice(item, ui.value);
                    return true;
                },
                slide: function (event, ui) {
                    $("#MultivolumesliderVolumeNumber_" + item).html(ui.value);
                }
            });
        });
    } else {
        //Steps von 5 oder 1
        var v = 5;
        if (SonosPlayers[SonosZones.ActiveZoneUUID].playerProperties.groupRenderingControl_GroupVolume >= 90 || SonosPlayers[SonosZones.ActiveZoneUUID].playerProperties.groupRenderingControl_GroupVolume <= 20) {
            v = 1;
        }
        var newvolume;
        if (k === 1) {
            newvolume = SonosPlayers[SonosZones.ActiveZoneUUID].playerProperties.groupRenderingControl_GroupVolume + v;
        } else {
            newvolume = SonosPlayers[SonosZones.ActiveZoneUUID].playerProperties.groupRenderingControl_GroupVolume - v;
        }
        SonosPlayers[SonosZones.ActiveZoneUUID].playerProperties.volume = newvolume;
        SonosPlayers[SonosZones.ActiveZoneUUID].playerProperties.groupRenderingControl_GroupVolume = newvolume;
        SonosZones.RenderVolume(SonosZones.ActiveZoneUUID);
        SetGroupVolumeDevice(SonosZones.ActiveZoneUUID, newvolume);
    }
};//done;
//Setzt die Lautstärke für ein spezielles Gerät.
function SetVolumeDevice(dev, v) {
    SonosAjax("SetVolume", "", dev, v).fail(function () { alert("Beim setzen der Lautstäke für Player " + dev + " ist ein Fehler aufgetreten."); });
};//done
function SetGroupVolumeDevice(dev, v) {
    SonosAjax("SetGroupVolume", "", dev, v).fail(function () { alert("Beim setzen der Lautstäke für Player " + dev + " ist ein Fehler aufgetreten."); });
};//done
//} Lautstärke

//{ Aktuelle Wiedergabeliste
//Speichern/Exportieren der aktuellen Playlist
function SaveQueue() {
    try {
        var title = SoDo.saveQueue.value;
        var queuetype = "SaveQueue";
        if (title.length > 0) {
            SetVisible(SoDo.saveQueueLoader);
            if (SoVa.exportplaylist === true) {
                queuetype = "ExportQueue";
            }

            var request = SonosAjax(queuetype, { '': title });
            request.success(function (data) {
                if (data === true) {
                    SonosZones.RenderAllPlaylist(true);
                } else {
                    alert("Beim laden der Aktion:" + queuetype + "(" + title + ") ist ein Fehler aufgetreten.");
                }
                if (IsVisible(SoDo.saveQueueLoader)) {
                    SetHide(SoDo.saveQueueLoader);
                }
            });
            request.fail(function (jqXHR) {
                if (jqXHR.statusText === "Internal Server Error") {
                    ReloadSite("SaveQueue");
                } else { alert("Beim laden der Aktion:SaveQueue(" + title + ") ist ein Fehler aufgetreten."); }
                if (IsVisible(SoDo.saveQueueLoader)) {
                    SetHide(SoDo.saveQueueLoader);
                }
            });
        }
    }
    catch (Ex) {
        alert("Es ist ein Fehler beim SaveQueue aufgetreten:<br>" + Ex.message);
    }
}
//Items der Playlist zufügen
function AddToPlaylist(item) {
    try {
        var uri = item.parentNode.dataset.containerid
        var request = SonosAjax("Enqueue", { '': uri });
        request.success(function () {
        });
        request.fail(function (jqXHR) {
            if (jqXHR.statusText === "Internal Server Error") {
                ReloadSite("AddToPlaylist");
            } else { alert("Beim laden der Aktion:AddToPlaylist ist ein Fehler aufgetreten."); }
        });
    }
    catch (Ex) {
        alert("Es ist ein Fehler beim AddToPlaylist aufgetreten:<br>" + Ex.message);
    }
}
//Ersetzen der Playlist und entsprechend der Auswahl daten neu laden.
function ReplacePlaylist(item) {
    SetVisible(SoDo.playlistLoader)
    if (IsVisible(SoDo.browseLoader)) {
        SetVisible(SoDo.browseLoader);
    }
    SoDo.currentplaylistwrapper.innerHTML = "";
    var uri = item.parentNode.dataset.containerid
    //Damit beim gleichem Lied kein Problem entsteht Artist leeren.
    if (SonosPlayers[SonosZones.ActiveZoneUUID].playerProperties.currentTrack !== null && SonosPlayers[SonosZones.ActiveZoneUUID].playerProperties.currentTrack.artist !== null) {
        SonosPlayers[SonosZones.ActiveZoneUUID].playerProperties.currentTrack.artist = "leer";
        SonosPlayers[SonosZones.ActiveZoneUUID].playerProperties.currentTrack.albumArtURI = "leer";
    }
    SonosAjax("ReplacePlaylist", { '': uri }).success(function (data) {
        if (data === false) {
            console.log("Beim Replace ist ein Fehleraufgetreten.");
        }
        var player = SonosPlayers[SonosZones.ActiveZoneUUID];
        console.log(uri);
        player.playerProperties.enqueuedTransportURI = uri;
        player.playlist.ClearPlaylist(player);
        if (IsVisible(SoDo.browseLoader)) {
            SetHide(SoDo.browseLoader);
        }
    });
};//done
function RemoveFavItem(item) {
    var con = confirm("Dieser Favoriten Eintrag wird gelöscht!");
    if (!con) return;
    SonosAjax("RemoveFavItem", { '': item }).success(function () {
        LoadBrowse("FV:2");
    });
};//done
function AddFavItem(item, calltype) {
    if (typeof calltype === "undefined" || calltype === "") {
        calltype = "browse";
    }
    var itemdata;
    switch (calltype) {
        case "browse":
            itemdata = item.parentNode.dataset.containerid
            break;
        case "playlist":
            itemdata = item.dataset.uri;
            break;

    }
    if (typeof itemdata !== "undefined") {
        SonosAjax("AddFavItem", { '': itemdata }).success(function () { });
    }
};//done
//Songinfos anzeigen in der Playlist
function ShowSongInfos(t) {
    try {
        var newcurpopdown = t.parentElement.id;
        var plid = GetIDfromCurrentPlaylist(newcurpopdown);
        //alt ausblenden
        if (newcurpopdown === SoVa.aktcurpopdown) {
            let ctpl = document.getElementById(SoVa.aktcurpopdown);
            RemoveClass(ctpl.firstChild, SoVa.aktiv);
            SetHide(ctpl.lastChild);
            SoVa.aktcurpopdown = "leer";
            return;
        }
        if (SoVa.aktcurpopdown !== "leer") {
            let ctpl = document.getElementById(SoVa.aktcurpopdown);
            RemoveClass(ctpl.firstChild, SoVa.aktiv);
            SetHide(ctpl.lastChild);
        }
        let newcurpopdowndom = document.getElementById(newcurpopdown);
        let podo = newcurpopdowndom.lastChild;
        let podobomp = podo.querySelector(":scope > .bomb");
        let podoratingbar = podo.querySelector(":scope > .rating_bar");
        var plcover = podo.firstChild;
        SetVisible(podo);
        AddClass(t, SoVa.aktiv);
        SoVa.aktcurpopdown = newcurpopdown;

        //Cover Laden aus dem Data Attribut und dieses entsprechend leeren.
        if (plcover.dataset.url !== "geladen") {
            plcover.innerHTML='<img class="currentplaylistcover" onclick="ShowPlaylistLyric(this)" src="' + plcover.dataset.url + '">';
            plcover.dataset.url ="geladen";
        }
        //todo: wenn currentrack wechselt, dann müßte das gefüllt sein.
        var plmp3 = SonosPlayers[SonosZones.ActiveZoneUUID].playerProperties.playlist.playListItems[plid].mP3;
        if (!SonosZones.CheckMP3IsEmpty(plmp3)) {
            //MP3 vorhanden und somit rendern.
            if (parseInt(plmp3.bewertung) === -1) {
                if (!IsVisible(podobomp)) {
                    SetVisible(podobomp)
                }
                if (IsVisible(podoratingbar)) {
                    SetHide(podoratingbar)
                }
            } else {
                if (!IsVisible(podoratingbar)) {
                    SetVisible(podoratingbar)
                }
                podoratingbar.firstChild.style.width = plmp3.bewertung + "%"
            }
        } else {
            SetVisible(SoDo.playlistLoader)
            //Metadaten des Songs laden.

            var request = SonosAjax("GetSongMeta", { '': plcover.dataset.uri });
            request.success(function (data) {
                //Metadaten erhalten
                SonosPlayers[SonosZones.ActiveZoneUUID].playerProperties.playlist.playListItems[plid].mP3 = data;
                SonosPlayers[SonosZones.ActiveZoneUUID].playerProperties.playlist.playListItems[plid].mP3.tracknumber = plid;
                if (parseInt(plmp3.bewertung) === -1) {
                    if (!IsVisible(podobomp)) {
                        SetVisible(podobomp)
                    }
                    if (IsVisible(podoratingbar)) {
                        SetHide(podoratingbar)
                    }
                } else {
                    if (!IsVisible(podoratingbar)) {
                        SetVisible(podoratingbar)
                    }
                    podoratingbar.firstChild.style.width = data.bewertung + "%"
                }
                SetHide(SoDo.playlistLoader);

            });
            request.fail(function () {
                ReloadSite("JsControl:ShowSongInfos");
            });

        }
        UpdateImageOnErrors();
    }
    catch (Ex) {
        alert("Es ist ein Fehler beim ShowSongInfos aufgetreten:<br>" + Ex.message);
    }
};//done
//Popup laden mit den Lyrics aus Songs in der Wiedergabeliste
function ShowPlaylistLyric(t) {
    var curentid = GetIDfromCurrentPlaylist(t.parentElement.parentElement.parentElement.id);
    var player = SonosPlayers[SonosZones.ActiveZoneUUID];
    var curtracknumber = player.playerProperties.currentTrackNumber;
    if (curentid + 1 === curtracknumber) {
        //Wenn der gewählte Song dem current entspricht soll die currentbox aufgehen.
        if (!IsVisible(SoDo.lyric)) {
            ShowPlaylistLyricCurrent();
        }
    } else {
        //Es ist nicht current also die Lyric laden und anzeigen, wenn noch nicht vorhanden
        SoDo.lyricsPlaylist.innerHTML = "";
        SoDo.lyricsPlaylist.innerHTML = '<DIV class="righttopclose" onclick="ClosePlaylistLyric()">X</DIV>';
        var datalyric = player.playerProperties.playlist.playListItems[curentid].mP3.lyric;
        SoDo.lyricsPlaylist.innerHTML += '<DIV class="lyricplaylistclass">' + datalyric + '</DIV>';
        SonosWindows(SoDo.lyricsPlaylist);
    }
    MoveAktArtist();
};//done
function ShowPlaylistLyricCurrent() {
    if (SonosPlayers[SonosZones.ActiveZoneUUID].playerProperties.currentTrack.uri !== "leer") {
        SoDo.lyricButton.classList.toggle("akt");
        SonosWindows(SoDo.lyric);
        MoveAktArtist();
    }
};//done
//Neue Funktionen für Jquery remove
function IsVisible(DomElement) {
    //Kein OffSetParent keine Sichtbarkeit
    return DomElement.offsetParent !== null;
};//done
function SetVisible(DomElement) {
    DomElement.style.display = "block";

};//done
function SetHide(DomElement) {
    DomElement.style.display = "none";

};//done
function AddClass(DOMElement, cssClass) {
    if (cssClass === undefined) {
        alert("Bei AddClass wurde keine Klasse für " + DOMElement.id + " mitgegeben.")
    }
    if (!DOMElement.classList.contains(cssClass)) {
        DOMElement.classList.add(cssClass);
    }
}
function RemoveClass(DOMElement, cssClass) {
    if (cssClass === undefined) {
        alert("Bei RemoveClass wurde keine Klasse für " + DOMElement.id + " mitgegeben.")
    }
    if (DOMElement.classList.contains(cssClass)) {
        DOMElement.classList.remove(cssClass);
    }
}
function MoveAktArtist() {

    if (document.body.clientWidth < 850) {
        return;
    }
    var hivi = (!IsVisible(SoDo.lyric) || document.body.clientWidth > 1200) && !IsVisible(SoDo.browse) && !IsVisible(SoDo.lyricsPlaylist);
    if (SoVa.ratingonlycurrent === false && hivi === false) {
        AddClass(SoDo.aktSongInfo, "moveright");
        AddClass(SoDo.cover, "moveright");
        SoDo.playlistCount.classList.add("movedown");
    }
    if (SoVa.ratingonlycurrent === false && hivi === true) {
        RemoveClass(SoDo.aktSongInfo, "moveright")
        RemoveClass(SoDo.cover, "moveright");
        SoDo.playlistCount.classList.remove("movedown");
    }
    if (SoVa.ratingonlycurrent === true && hivi === false && !IsVisible(SoDo.ratingListBox)) {
        AddClass(SoDo.aktSongInfo, "moveright")
        AddClass(SoDo.cover, "moveright");
        SoDo.playlistCount.classList.add("movedown");
    }
    if (SoVa.ratingonlycurrent === true && hivi === true && !IsVisible(SoDo.ratingListBox)) {
        RemoveClass(SoDo.aktSongInfo, "moveright")
        RemoveClass(SoDo.cover, "moveright");
        SoDo.playlistCount.classList.remove("movedown");
    }
    if (SoVa.ratingonlycurrent === true && hivi === true && IsVisible(SoDo.ratingListBox)) {
        SoDo.playlistCount.classList.remove("movedown");
    }
    if (SoVa.ratingonlycurrent === true && hivi === false && IsVisible(SoDo.ratingListBox)) {
        SoDo.playlistCount.classList.add("movedown");
    }
};//done
//Entfernt ein Song aus der Playlist
function RemoveFromPlaylist(k) {
    SoVa.aktcurpopdown = "leer"; //Reset der Playlist Informationen
    var PressKey = GetIDfromCurrentPlaylist(k.parentElement.parentElement.id);
    SonosAjax("RemoveSongInPlaylist", "", PressKey + 1).success(function (data) {
        if (data === true) {
            SonosPlayers[SonosZones.ActiveZoneUUID].playerProperties.QueueChanged = SonosPlayers[SonosZones.ActiveZoneUUID].playerProperties.QueueChanged + 1;
            SonosPlayers[SonosZones.ActiveZoneUUID].playlist.RemoveFromPlaylist(SonosPlayers[SonosZones.ActiveZoneUUID], PressKey);
            if (PressKey < SonosPlayers[SonosZones.ActiveZoneUUID].currentTrackNumber) {
                SonosPlayers[SonosZones.ActiveZoneUUID].playerProperties.currentTrackNumber = SonosPlayers[SonosZones.ActiveZoneUUID].playerProperties.currentTrackNumber - 1;
                SonosPlayers.RenderCurrentTrackinPlaylist(SonosZones.ActiveZoneUUID, SonosZones[SonosZones.ActiveZoneUUID].playerProperties.currentTrackNumber, "Remove From Playlist");
            }
        }
    });
};//done
//Schließen der Lyric von Songs aus der Wiedergabeliste
function ClosePlaylistLyric() {
    SonosWindows(SoDo.lyricsPlaylist);
    MoveAktArtist();
};//done
//Helper um die aktuelle ID aus dem übergeben Item der Playlist zu erhalten.
function GetIDfromCurrentPlaylist(k) {
    var toRemove = 'Currentplaylist_';
    var t = k.replace(toRemove, '');
    return parseInt(t);
};//done

//Playlist Sortierbar machen
function MakeCurrentPlaylistSortable() {
    //todo: Sortierbar machen
    //SoDo.currentplaylistwrapper.sortable({ disabled: false, axis: "y", placeholder: "currentplaylistplaceholder", stop: function (event, ui) { ResortPlaylist(ui); } });
}
//Playlist wurde umsortiert und nun neu geschrieben.
function ResortPlaylist(ui) {
    var cpl = ui.item.attr("id");
    $("#" + cpl).children(".curpopdown").hide();
    SoVa.aktcurpopdown = "leer";//Damit das nächste Aufgehen wieder ohne Probleme geht.
    $("#" + cpl).children(".currentrackinplaylist").removeClass(SoVa.aktiv);
    $(".currentplaylist").each(function (i, item) {
        var jitem = $(item).attr("id");
        if (jitem === cpl) {
            //Wenn man hier angekommen ist, dann ist man beim umsortierten objekt und wir zählen nun die Playlist neu durch und setzen den Song entsprechend neu.
            //var positionen = GetIDfromCurrentPlaylist(cpl) + "#" + (i + 1);
            var old = GetIDfromCurrentPlaylist(cpl);
            SonosPlayers[SonosZones.ActiveZoneUUID].playlist.ReorderPlaylist(SonosPlayers[SonosZones.ActiveZoneUUID], old, i);
            SonosAjax("ReorderTracksinQueue", "", old + 1, i + 1).fail(function (jqXHR) {
                if (jqXHR.statusText === "Internal Server Error") {
                    ReloadSite("ResortPlaylist");
                } else { alert("Beim der Aktion:ResortPlaylist(" + ui + ") ist ein Fehler aufgetreten."); }
            });
        }
        $(item).attr("id", "Currentplaylist_" + i);
    });
    ResortPlaylistDisable();
};//done
//Sortierbarkeit deaktivieren.
function ResortPlaylistDisable() {
    //todo: Sortierbar machen
    //SoDo.currentplaylistwrapper.sortable({ disabled: true });
}

function SetAudioIn() {
    //Übergabe an Methode, Server steuert das selber.
    SonosAjax("SetAudioIn");
}

//} Aktuelle Wiedergabeliste

//{ PrüfMethoden, Hintergrundaktualisierungen
//Seite neu laden
function ReloadSite(source) {
    location.reload();
}

//Prüft auf Fehler bei Covern und setzt das NoCoverBild
function UpdateImageOnErrors() {
    document.getElementsByTagName('img').forEach(function (item) {
        item.addEventListener('error', function () {
            this.setAttribute('src', SoVa.nocoverpfad);
        });
    });
}
//Wird bei einem Fehler aufgerufen um die Darstellung zurückzusetzen
function ResetAll() {
    SoDo.cover.setAttribute("src", SoVa.nocoverpfad);
    SoDo.nextcover.setAttribute("src", SoVa.nocoverpfad);
    SetHide(SoDo.nextcover);
    SoDo.runtimeDuration.textContent = "";
    SoDo.runtimeRelTime.textContent = "";
    SoDo.aktArtist.textContent = "";
    SoDo.aktTitle.textContent = "";
    SoDo.playlistAkt.textContent = "0";
    SoDo.playlistTotal.textContent = "0";
    SoDo.bewertungWidth.style.width("0%");
    SoDo.devicesWrapper.children(".groupdevicewrapper").remove();
    SetVisible(SoDo.deviceLoader);
} //Ende Reset

//} PrüfMethoden, Hintergrundaktualisierungen


//Bewertung Mine
function SetRatingMine(rmine) {
    SoVa.ratingMP3.bewertungMine = rmine;
    SoDo.ratingMineSelector.value = rmine;
};//done
//Geschwindigkeit
function SetGeschwindigkeit(tempo) {
    SoVa.ratingMP3.geschwindigkeit = tempo;
    let speedselected = SoDo.geschwindigkeit.querySelector(":scope > .selected");
    if (speedselected !== null) {
        RemoveClass(speedselected, SoVa.selected);
    }
    AddClass(document.getElementById("geschwindigkeit_" + tempo), SoVa.selected);

};//done
//Stimmung setzen
function SetStimmung(stimmung) {
    SoVa.ratingMP3.stimmung = stimmung;
    let stimmungselected = SoDo.stimmungen.querySelector(":scope > .selected");
    if (stimmungselected !== null) {
        RemoveClass(stimmungselected, SoVa.selected);
    }
    AddClass(document.getElementById("stimmung_" + stimmung), SoVa.selected);
};//done
//Aufwecken setzten
function SetRatingAufwecken(aufwecken) {
    SoDo.aufweckenSwitch.checked = aufwecken;
};//done
//Interpretenplaylist setzten
function SetRatingArtistpl(arpl) {
    SoDo.artistplSwitch.checked = arpl;
};//done
//Wird aus der Ratinglistaufgerufen um entsprechend das Layout zu definieren.
function ChangeRating(v, c) {
    if (typeof c !== "undefined" && c === true && v > 50) {
        SetRatingArtistpl(true);
    }
    SoVa.ratingMP3.bewertung = v;
    let currentRating = SoDo.ratingListBox.querySelector(":scope > .rating_bar_aktiv");
    if (currentRating !== null) {
        RemoveClass(currentRating, "rating_bar_aktiv");
    }
    RemoveClass(SoDo.ratingBomb, "rating_bar_aktiv");
    if (v === -1) {
        //Bombe
        AddClass(SoDo.ratingBomb, "rating_bar_aktiv");
    } else {
        AddClass(document.getElementById("rating_id_" + v), "rating_bar_aktiv");
    }
};//done
//Gelegenheit in das Data beim Rating schreiben
function SetSituation(situation) {
    SoVa.ratingMP3.gelegenheit = situation;
    let gelselected = SoDo.gelegenheiten.querySelector(":scope > .selected");
    if (gelselected !== null) {
        RemoveClass(gelselected, SoVa.selected);
    }
    AddClass(document.getElementById("gelegenheit_" + situation), SoVa.selected);
};//done
//Ratinglist vorbereiten von Songs aus der Wiedergabeliste
function ShowPlaylistRating(t) {
    //Bei Streaming immer schließen und Return;
    if (SoVa.ratingonlycurrent === true) {
        return;
    }
    SonosWindows(SoDo.ratingListBox, false, { overlay: true, selecteddivs: [SoDo.nextButton, SoDo.playButton, SoDo.muteButton] });
    if (!IsVisible(SoDo.ratingListBox)) {
        return;
    }
    var plid = GetIDfromCurrentPlaylist(t.parentElement.parentElement.id);
    SoDo.ratingListBox.dataset.type = "playlist";
    SoDo.ratingListBox.dataset.playlistid = plid;
    SoVa.ratingMP3.FillFromMP3(SonosPlayers[SonosZones.ActiveZoneUUID].playerProperties.playlist.playListItems[plid].mP3);
    SetSituation(SoVa.ratingMP3.gelegenheit);
    SetGeschwindigkeit(SoVa.ratingMP3.geschwindigkeit);
    SetStimmung(SoVa.ratingMP3.stimmung);
    SetRatingAufwecken(SoVa.ratingMP3.aufwecken);
    SetRatingArtistpl(SoVa.ratingMP3.artistPlaylist);
    ChangeRating(SoVa.ratingMP3.bewertung);
    SetRatingMine(SoVa.ratingMP3.bewertungMine);
};//done
//Ratinglist vorbereiten vom current Song
function ShowCurrentRating(t) {
    //Bei Streaming immer schließen und Return;
    var player = SonosPlayers[SonosZones.ActiveZoneUUID];
    if (player.playerProperties.currentTrack.stream === true) {
        if (IsVisible(SoDo.ratingListBox)) {
            SonosWindows(SoDo.ratingListBox, true);
        }
        return;
    }
    //todo: bei currentonly wird beim liedwechsel das rating geschlossen.
    //Wenn nur current angezeigt werden soll, dann nicht schließen. 
    if (SoVa.ratingonlycurrent === true && t !== "hide") {
        if (!IsVisible(SoDo.ratingListBox)) {
            SonosWindows(SoDo.ratingListBox);
        }
        //Verschieben des Titels.
        AddClass(SoDo.aktSongInfo, SoVa.moveLeft)
        AddClass(SoDo.playlistCount, SoVa.moveLeft);
    }
    if (SoVa.ratingonlycurrent === true && t === "hide") {
        SonosWindows(SoDo.ratingListBox);
        MoveAktArtist();
        RemoveClass(SoDo.aktSongInfo, SoVa.moveLeft)
        SoDo.playlistCount.removeClass(SoVa.moveLeft);
        SoVa.ratingMP3 = new MP3();
        return;
    }
    if (SoVa.ratingonlycurrent === false) {
        if (!IsVisible(SoDo.ratingListBox)) {
            SonosWindows(SoDo.ratingListBox, false, { overlay: true, selecteddivs: [SoDo.nextButton, SoDo.playButton, SoDo.muteButton] });
        } else {
            SonosWindows(SoDo.ratingListBox);
        }
        //SonosWindows(ratinglist);
        if (t === "hide") {
            SonosWindows(SoDo.ratingListBox, true);
            return;
        }
    }
    SoDo.ratingListBox.dataset.type = "current";
    SoDo.ratingListBox.dataset.playlistid = (player.playerProperties.currentTrackNumber - 1);
    SoVa.ratingMP3.FillFromMP3(player.playerProperties.currentTrack.mP3);
    SetSituation(SoVa.ratingMP3.gelegenheit);
    SetGeschwindigkeit(SoVa.ratingMP3.geschwindigkeit);
    SetStimmung(SoVa.ratingMP3.stimmung);
    SetRatingAufwecken(SoVa.ratingMP3.aufwecken);
    SetRatingArtistpl(SoVa.ratingMP3.artistPlaylist);
    ChangeRating(SoVa.ratingMP3.bewertung);
    SetRatingMine(SoVa.ratingMP3.bewertungMine);
};//done;
//Filter für das Rating setzen und Filterlist schließen.
function SetRatingFilter(type, v) {
    if (type === "hide") {
        SonosWindows(SoDo.filterListBox, true);
        return;
    }
    SonosPlayers[SonosZones.ActiveZoneUUID].ChangeRatingFilter(type, v);
};//done
//Das Rating und Gelegenheiten für einen Song setzen
function SetRatingLyric() {
    var pid = parseInt(SoDo.ratingListBox.dataset.playlistid);
    var datatype = SoDo.ratingListBox.dataset.type;
    var mp3 = {};
    if (!isNaN(pid) && datatype !== "current") {
        //hier nun den Song aus der Playlist nehmen
        mp3 = Object.assign({}, SonosPlayers[SonosZones.ActiveZoneUUID].playerProperties.playlist.playListItems[pid].mP3);
        mp3.tracknumber = pid;
    } else {
        //hier nun den currenttrack nehmen.
        mp3 = Object.assign({}, SonosPlayers[SonosZones.ActiveZoneUUID].playerProperties.currentTrack.mP3);
        mp3.tracknumber = SonosPlayers[SonosZones.ActiveZoneUUID].playerProperties.currentTrackNumber - 1;
    }
    SoVa.ratingMP3.FillServerMP3fromThis(mp3);
    mp3.aufwecken = SoDo.aufweckenSwitch.checked;
    mp3.artistPlaylist = SoDo.artistplSwitch.checked;
    SonosAjax("SetSongMeta", mp3).done(function () {
        var player = SonosPlayers[SonosZones.ActiveZoneUUID];
        if (SoDo.ratingListBox.dataset.type === "current") {
            player.playerProperties.currentTrack.mP3 = mp3;
            player.RenderCurrentTrack();
        }
        //Setzen des Rating in der Playlist
        if (mp3.tracknumber !== -1) {
            player.playerProperties.playlist.playListItems[mp3.tracknumber].mP3 = mp3;
            var playlistsong = $("#Currentplaylist_" + mp3.tracknumber).children(".curpopdown");
            var plcover = playlistsong.children(".playlistcover");
            if (plcover.attr("data-url") !== "geladen") {
                $('<img class="currentplaylistcover" onclick="ShowPlaylistLyric(this)" src="' + plcover.attr("data-url") + '">').appendTo(plcover);
                plcover.attr("data-url", "geladen");
            }
            //Setzen einer Bombe
            if (mp3.bewertung !== -1) {
                if ($("#Currentplaylist_" + mp3.tracknumber).children(".curpopdown").children(".bomb").is(":visible")) {
                    $("#Currentplaylist_" + mp3.tracknumber).children(".curpopdown").children(".bomb").hide();
                }
                playlistsong.children(".moveCurrentPlaylistTrack").css("margin-left", "24px");
            } else {
                if ($("#Currentplaylist_" + mp3.tracknumber).children(".curpopdown").children(".bomb").is(":hidden")) {
                    $("#Currentplaylist_" + mp3.tracknumber).children(".curpopdown").children(".bomb").show();
                }
                playlistsong.children(".moveCurrentPlaylistTrack").css("margin-left", "0");
            }
            playlistsong.children(".rating_bar").children("DIV").width(mp3.bewertung + "%");
        }
        //Prüfen ob verarbeitungsfehler vorhanden sind
        if (SoVa.ratingonlycurrent === false) {
            //Wenn nur Current Rating genommen wird, dann nicht ausblenden.
            SonosWindows(SoDo.ratingListBox, true);
        } else {
            ShowCurrentRating("blub");
        }
        SetVisible(SoDo.ratingCheck)
        setTimeout("SetHide(SoDo.ratingCheck)", 2000);
    })
        .fail(function (jqXHR) {
            if (jqXHR.statusText === "Internal Server Error") {
                ReloadSite("SetRating Lyric");
            } else { SonosWindows(SoDo.ratingListBox, true); alert("Es ist ein Fehler bei SetRatingLyric aufgetreten"); }
        });
};//done

//} Bewertung

//{ Suchen nach Songs
//Durchsuchen der Bibliothek starten
function BrowsePress() {
    SonosWindows(SoDo.browse);
    if (document.body.clientWidth > 420) {
        MoveAktArtist(250);
    }
    SoDo.browseButton.classList.toggle("akt");
    if (SoVa.browsefirst === 0) {
        window.setTimeout("LoadBrowse('A:ALBUMARTIST')", 150);
        SoVa.browsefirst = 1;
    }
};//Done
//Bibliothekt durchsuchen und darstellen
function LoadBrowse(v) {
    if (!IsVisible(SoDo.browseLoader)) {
        SetVisible(SoDo.browseLoader);
    }
    var loadbrowse;
    if (v === "bb") {
        loadbrowse = SoDo.browseBackButton.dataset.parent;
    } else {
        if (v === "A:ALBUMARTIST" || v === "A:PLAYLISTS" || v === "A:GENRE" || v === "FV:2") {
            loadbrowse = v;
        } else {
            loadbrowse = $(v).parent().attr("data-containerid");
        }
    }
    if (typeof loadbrowse === 'undefined') {
        loadbrowse = v;
    }
    $(".currentbrowse").remove();
    if (IsVisible(SoDo.browseBackButton)) {
        SetHide(SoDo.browseBackButton);
    }
    SoDo.ankerlist.innerHTML = "";
    var request
    if (loadbrowse == "A:PLAYLISTS") {
        request = SonosAjax("GetPlaylists");
    } else if (loadbrowse == "FV:2") {
        request = SonosAjax("GetFavorites");
    }
    else {
        request = SonosAjax("Browsing", { '': loadbrowse });
    }
    request.success(function (data) {
        var abc = [];
        if (data.length > 0) {
            var browsecontent = "";
            $.each(data, function (i, item) {
                //Erster Durchlauf und nicht im Root
                if (i === 0 && item.parentID !== "A:ALBUMARTIST" && item.parentID !== "A:PLAYLISTS" && item.parentID !== "A:GENRE" && item.parentID !== "FV:2") {
                    //Es gibt auch noch ein Parenteintrag, diesen anpassen und entsprechend darstellen ansonsten den alten nehmen
                    if (item.parentID !== null) {
                        if (item.parentID.lastIndexOf("/") > 0) {
                            SoVa.browseParentID = item.parentID.substring(0, item.parentID.lastIndexOf("/"));
                        } else {
                            SoVa.browseParentID = "A:ALBUMARTIST";
                        }
                    } else {
                        if (loadbrowse.substring(0, 1) !== "S") {
                            SoVa.browseParentID = loadbrowse.substring(0, loadbrowse.lastIndexOf("/"));
                        } else {
                            SoVa.browseParentID = "A:PLAYLISTS";
                        }
                    }
                    SetVisible(SoDo.browseBackButton);
                    SoDo.browseBackButton.dataset.parent = SoVa.browseParentID
                }
                var im = "";
                //Entweder ein Container oder ein Song
                var browsetitlewidth;
                if (!SonosZones.CheckStringIsNullOrEmpty(item.containerID)) {
                    //Prüfen ob schon im Array.
                    var alink = "";
                    if (loadbrowse === "A:ALBUMARTIST" || loadbrowse === "A:PLAYLISTS" || loadbrowse === "A:GENRE") {
                        var buchstabe = item.title.substring(0, 1).toUpperCase();
                        if ($.inArray(buchstabe, abc) === -1) {
                            abc.push(buchstabe);
                            alink = '<a href="#" name="' + buchstabe + '"></a>';
                        }
                    }
                    browsetitlewidth = 320;
                    if (item.albumArtURI !== null && item.albumArtURI !== "" && item.mP3.hatCover === true) {
                        var img = "http://" + SonosPlayers[SonosZones.ActiveZoneUUID].playerProperties.baseUrl + item.albumArtURI;
                        if (!item.albumArtURI.startsWith("/getaa")) {
                            img = item.albumArtURI;
                        }
                        im = '<div class="browsingCover"><img src="' + SoVa.nocoverpfad + '" class="lazy" data-original="' + img + '"></div>';
                        browsetitlewidth = 280;
                    }
                    var currentplaying = "";
                    if (SonosPlayers[SonosZones.ActiveZoneUUID].playerProperties.enqueuedTransportURI.endsWith(item.containerID)) {
                        currentplaying = "curPlayEnqueuedTransportURI";
                    }
                    browsecontent += '<div id="Browsing' + (i + 1) + '" data-containerid="' + item.containerID + '" class="currentbrowse ' + currentplaying + '">' + im + '<DIV class="browsetitle" style="width: ' + browsetitlewidth + 'px;" onclick="LoadBrowse(this)">' + item.title + alink + '</div><DIV class="browseaddcontainertoplaylist" onclick="AddToPlaylist(this)"></DIV><DIV class="browsereplacecontainertoplaylist" onclick="ReplacePlaylist(this)"></DIV></DIV>';
                } else {
                    //Es handelt sich um einen song
                    //Rating wird mitgeliefert und angezeigt, wenn es . 
                    var rating = '<div class="bomb" Style="display:block;"><img src="/images/bombe.png" alt="playlistbomb"/></div>';
                    if (parseInt(item.mP3.bewertung) !== -1) {
                        rating = '<div style="margin-left: 10px;margin-top: 10px;" class="rating_bar"><div style="width:' + item.mP3.bewertung + '%;"></div></div>';
                    }
                    im = "";
                    if (item.albumArtURI !== null && item.mP3.hatCover === true) {
                        var itmAAURi = "http://" + SonosPlayers[SonosZones.ActiveZoneUUID].playerProperties.baseUrl + item.albumArtURI;
                        //Prüfen woher das cover stammt.
                        if (!item.albumArtURI.startsWith("/getaa?u=")) {
                            itmAAURi = item.albumArtURI;
                        }
                        im = '<div class="browsingCover"><img src="' + SoVa.nocoverpfad + '" class="lazy" data-original="' + itmAAURi + '"></div>';
                    } else {
                        im = '<div class="browsingCover"><img src="' + SoVa.nocoverpfad + '"></div>';
                    }
                    var itmuri = item.uri;
                    if (item.itemID.startsWith("FV:2")) {
                        itmuri = item.itemID;
                    }
                    browsecontent += '<div id="Browsing' + (i + 1) + '" data-containerid="' + itmuri + '" class="currentbrowse">' + im + '<DIV class="browsetitle" style="width:210px;">' + item.title + '</div><DIV class="browseaddcontainertoplaylist" onclick="AddToPlaylist(this)"></DIV><DIV class="browsereplacecontainertoplaylist" onclick="ReplacePlaylist(this)"></DIV>' + rating + '</DIV>';
                }


            }); //Ende each
            SoDo.browseWrapper.innerHTML = browsecontent;
            UpdateImageOnErrors();
            LazyLoad();
            //Anker wurde vorbereitet und kann nun angezeigt werden.
            if (abc.length > 0) {
                $.each(abc, function (i, item) {
                    SoDo.ankerlist.innerHTML += '<div onClick="SetAnker(\'' + item + '\',\'' + v + '\')" id="ankerlist_' + item + '"><a href="#' + item + '">' + item + '</a></DIV>';
                });
            }
        } else {
            //ES wurden keine Elemente zurückgeliefert
            if (loadbrowse.substring(0, 1) !== "S") {
                SoVa.browseParentID = loadbrowse.substring(0, loadbrowse.lastIndexOf("/"));
            } else {
                SoVa.browseParentID = "A:ALBUMARTIST";
            }
            SetVisible(SoDo.browseBackButton);
            SoDo.browseBackButton.dataset.parent = SoVa.browseParentID
        }
        //Sprunganker ansteuern.
        if (SoVa.getanker !== "leer") {
            //Prüfen, ob ein anderer Buchstabe gewählt wurde und diesen entsprechend setzen.      	
            var interpretfirstletterindex = loadbrowse.indexOf("/");
            var interpretfirstletter = loadbrowse.substr(interpretfirstletterindex + 1, 1);
            if (interpretfirstletterindex > -1 && interpretfirstletter !== SoVa.getanker) {
                SoVa.getanker = interpretfirstletter;
            }
            if (loadbrowse.indexOf(SoVa.getAnkerArt) > -1) {
                //Safari und Chrome können damit nicht umgehen, wenn es der gleiche Sprunganker ist, daher zweimal setzen.     	
                location.hash = "#leer";
                location.hash = "#" + SoVa.getanker;
            } else {
                // console.log("Funzt nicht:" + loadbrowse);
            }
        }
        if (IsVisible(SoDo.browseLoader)) {
            SetHide(SoDo.browseLoader);
        }
    });
    request.fail(function (jqXHR) {
        if (jqXHR.statusText === "Internal Server Error") {
            ReloadSite("LoadBrowse");
        } else { SetHide(SoDo.browseLoader); alert("Beim laden der Aktion:Browse(" + v + ") ist ein Fehler aufgetreten."); }
    });
};//done

function LazyLoad() {
    $("img").lazyload({ container: SoDo.browseWrapper });

};//done
//Vorbereitung um beim Backklick auch wieder zurück zu springen in der liste. 
function SetAnker(buchstabe, art) {
    SoVa.getanker = buchstabe;
    SoVa.getAnkerArt = art;
};//done
//} Suchen nach Songs

//{ Settings und Global Functions
//Aktualisieren des Musikindexes
function UpdateMusicIndex() {
    if (SoVa.updateMusikIndex === false) {
        if (!IsVisible(SoDo.musikIndexLoader)) {
            SetVisible(SoDo.musikIndexLoader)
        }
        var request = SonosAjax("SetUpdateMusicIndex");
        request.fail(function (jqXHR) {
            if (jqXHR.statusText === "Internal Server Error") {
                ReloadSite("UpdateMusicIndex");
            } else { alert("Beim aktualiseren des Musikindexes ist ein Fehler aufgetreten."); }
        });
    }
};//done
//Zeigt Songsdetails an aus dem Currenttrack, die in metaUse definiert wurden.
function ShowCurrentSongMeta() {
    let cut = SonosPlayers[SonosZones.ActiveZoneUUID].playerProperties.currentTrack;
    if (cut.stream === true) {
        SonosWindows(SoDo.currentMeta, true);
        return;
    }
    SonosWindows(SoDo.currentMeta);

    if (!IsVisible(SoDo.currentMeta)) return;
    SoDo.currentMeta.innerHTML = ""
    if (cut.mP3 !== null) {
        var data = cut.mP3;
        var prop = Object.getOwnPropertyNames(data);
        let wrapper = document.createElement("DIV");
        wrapper.id = "CurrentMetaWrapper";
        for (var i = 0; i < prop.length; i++) {
            var k = prop[i];
            if (SoVa.metaUse.indexOf(k) !== -1) {
                if (data[k] !== "" && data[k] !== null && data[k] !== "leer" && data[k] !== 0) {
                    //erstes zeichen groß schreiben
                    wrapper.innerHTML += "<div><b>" + k.charAt(0).toUpperCase() + k.slice(1) + "</b>: " + data[k] + "</div>";
                }
            }
        }
        SoDo.currentMeta.appendChild(wrapper);
    } else {
        SonosAjax("GetSongMeta", { '': cut.uri }).success(function (datanull) {
            var propnull = Object.getOwnPropertyNames(datanull);
            let wrapper = document.createElement("DIV");
            wrapper.id = "CurrentMetaWrapper";
            for (var y = 0; y < propnull.length; y++) {
                var kp = propnull[y];
                if (SoVa.metaUse.indexOf(kp) !== -1) {
                    if (datanull[kp] !== null && datanull[kp] !== "") {
                        //erstes zeichen groß schreiben
                        wrapper.innerHTML += "<div><b>" + kp.charAt(0).toUpperCase() + kp.slice(1) + "</b>: " + datanull[kp] + "</div>";
                    }
                }
            }
            SoDo.currentMeta.appendChild(wrapper);
        });
    }
};//done
//Ermittelt das aktuelle Verhalten des Gerätes und Songs
//Prüft, ob der Musikindex gerade  aktualisiert wird,
function GetMusicIndexInProgress() {
    var request = SonosAjax("GetUpdateIndexInProgress");
    request.success(function (data) {
        if (data === true) {
            if (!IsVisible(SoDo.musikIndexLoader)) {
                SetVisible(SoDo.musikIndexLoader);
            }
        } else {
            if (IsVisible(SoDo.musikIndexLoader)) {
                SetHide(SoDo.musikIndexLoader);
                SetVisible(SoDo.musikIndexCheck);
                window.setTimeout("SetHide(SoDo.musikIndexCheck);", 1000);
                window.setTimeout("SonosZones.RenderAllPlaylist(true)", 2000);
            }
        }
    });
    request.fail(function (jqXHR) {
        if (jqXHR.statusText === "Internal Server Error" || jqXHR.statusText === "error") {
            ReloadSite("GetMusicIndexInProgress");
        } else { alert("Beim aktualiseren des Musikindexes ist ein Fehler aufgetreten."); }
        if (IsVisible(SoDo.musikIndexLoader)) {
            SetHide(SoDo.musikIndexLoader);
        }
    });
};//done
//Funktion zum Absenden ohne Rückmeldung
function doit(d) {
    var request = SonosAjax(d);
    request.success(function (data) {
        if (data === "Fehler") {
            alert("Beim laden der Aktion:" + d + " wurde ein Fehler gemeldet.");
        }
    });
    request.fail(function (jqXHR) {
        if (jqXHR.statusText === "Internal Server Error") {
            ReloadSite("doit:" + d);
        } else { alert("Beim laden der doit Aktion:" + d + " ist ein Fehler aufgetreten."); }
    });
} //Ende von DO
//Funktion zum Absenden ohne Rückmeldung mit Wertübergabe
function doitValue(d, v) {
    var request = SonosAjax(d, { '': v });
    request.success(function () { });
    request.fail(function (jqXHR) {
        if (jqXHR.statusText === "Internal Server Error") {
            ReloadSite("doitvalue:" + d + " value:" + v);
        } else { alert("Beim laden der Aktion:" + d + " ist ein Fehler aufgetreten."); }
    });
} //Ende von DO2
//Beim DEbug In die Console loggen
function SonosLog(v) {
    if (debug === false) {
        return;
    }
    if (debug === true) {
        console.log(v);
    }

}
/*Folgender aufrufe als erklärung
SonosWindows(ratinglist,false,{overlay:true,selecteddivs:[$("#Next"),$("#Play")]});

param 1 = Object, was angezeigt wird DomElement oder der Text "overlay" bei Overlay wurde auf das Overlay geklickt und nun soll es geschlossen werden.
param 2 = Optional; soll das element auf jedenfall geschlossen werden?
param 3 = Object mit Parametern
Object Param 1 = overlay = Boolean = Zeitg, ob ein Overlay angezeigt werden soll, bei dem das entsprechende Fenster von param 1 drüber liegt.
Object Param 2 = selecteddivs = Array oder einzel Jquery Object = Objecte, die auch über dem Overlay angezeigt werden sollen. 
*/
function SonosWindows(sobj, remove, setobj) {
    if (sobj === "overlay") {
        sobj = SoVa.overlayDVIObject; //Vorhandene elemente schließen. 
    }
    var objectindex = SoVa.swindowlist.indexOf(sobj);
    var rem = "notset";
    if (typeof remove !== "undefined") {
        rem = remove;
    }
    var overlay = false;
    var settingsobject = setobj || false;
    //Hier dann die settings für das Div hinterlegen. 
    if (settingsobject !== false) {
        overlay = setobj.overlay || false;
        var tempselecteddivs = setobj.selecteddivs || false;
        if (tempselecteddivs !== false) {
            if ($.isArray(tempselecteddivs)) {
                SoVa.selectetdivs = tempselecteddivs;
            } else {
                SoVa.selectetdivs.push(tempselecteddivs);
            }
        }
    }
    var i;
    if (objectindex === -1 && rem !== true) {
        SoVa.swindowlist.push(sobj);
        if (overlay === true) {
            SetVisible(SoDo.overlay);
            SoDo.overlay.style.zIndex = SoVa.szindex;
            SoVa.overlayDVIObject = sobj;
            SoVa.szindex++;
            if (SoVa.selectetdivs.length > 0) {
                for (i = 0; i < SoVa.selectetdivs.length; i++) {
                    SoVa.selectetdivs[i].style.zIndex = SoVa.szindex;
                }
                SoVa.szindex++;
            }
        }
        SetVisible(sobj);
        sobj.style.zIndex = SoVa.szindex
        SoVa.szindex++;
    } else {
        if (rem !== false) {
            if (objectindex !== -1) {
                SoVa.swindowlist.splice(objectindex, 1);
            }
            if (sobj === SoVa.overlayDVIObject) {
                if (IsVisible(SoDo.overlay)) {
                    SetHide(SoDo.overlay);
                }
                SoVa.overlayDVIObject = "";
                if (SoVa.selectetdivs.length > 0) {
                    for (i = 0; i < SoVa.selectetdivs.length; i++) {
                        SoVa.selectetdivs[i].style.zIndex = 100;
                    }
                    SoVa.selectetdivs = [];
                }
            }
            SetHide(sobj);
        }
    }
    if (SoVa.swindowlist.length === 0) {
        SoVa.szindex = 100;
    }


}
function ShowSleepMode() {
    SonosWindows(SoDo.sleepMode);
}
function SetSleepModeState() {
    SonosPlayers[SonosZones.ActiveZoneUUID].playerProperties.remainingSleepTimerDuration = SoDo.sleepModeSelection.value;
    var request = SonosAjax("SetSleepTimer", { '': SonosPlayers[SonosZones.ActiveZoneUUID].playerProperties.remainingSleepTimerDuration });
    request.success(function () {
        SonosZones.RenderSleepTimer(SonosZones.ActiveZoneUUID);
    }).fail(function (jqXHR) {
        if (jqXHR.statusText === "Internal Server Error") {
            ReloadSite("SetSleepModeState");
        } else { alert("Beim laden der Aktion:GetSleepModeState ist ein Fehler aufgetreten."); }
    });

}
function ToggleCurrentPlaylist() {
    //Currentplaylist
    SoDo.currentplaylist.classList.toggle("active");
    if (IsVisible(SoDo.currentplaylistclose)) {
        SetHide(SoDo.currentplaylistclose);
    } else {
        SetVisible(SoDo.currentplaylistclose);
    }
};//done

//} In Work