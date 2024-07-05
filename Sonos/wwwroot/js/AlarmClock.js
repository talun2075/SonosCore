"use strict";
//Container etc hinterlegen.
var AlarmClock = {};
AlarmClock.Alarms = {};
AlarmClock.roomsAlarmTime = new Array(); //Pro Raum alle Zeiten definieren.
AlarmClock.Rooms = {};
//Läd und füllt den Kalender
var editalarmid; //Hält die ID, des Alarms, welcher gerade geändert wird
var editalarmidStartTime; //Hält zur Kontrolle die Starttime beim Beginn des editierens, da nur Pro Zone eine Zeit genommen werden darf und dies beim Speichern getestet werden muss.

function InitAlarms() {
    SoDo.alarmClockDOM.addEventListener("click", function () {
        AlarmLoadPlayers("click");
        SoDo.alarmClockDOM.classList.toggle(SoVa.aktiv);
    });
    SoDo.editAlarmStartTimeHour.addEventListener("change", function () {
        //eingabe am Browser prüfen. Ipad und co werden eine Drobdown anzeigen.
        var tval = this.value;
        var indp = tval.indexOf(":");
        if (tval.length < 4) { alert("Die Eingegebene Startzeit kann nicht interpretiert werden. Bitte anpassen"); }
        if (tval.length === 4) {
            if (indp !== -1) {
                alert("Die Eingegebene Startzeit kann nicht interpretiert werden. Bitte anpassen");
            } else {
                //var stunden = tval.substr(0,2);
                var modifiedtime = tval.substr(0, 2) + ":" + tval.substr(2, 2);
                this.value = modifiedtime;
            }
        }
        if (tval.length === 5) {
            if (indp !== 2) {
                alert("Die Eingegebene Startzeit kann nicht interpretiert werden. Bitte anpassen");
            }
        }
    });
    Array.from(SoDo.editAlarmDaysSelectionCLASS).forEach(function (item) {
        item.addEventListener("change", function (evt) {
            CheckDays(evt);
        });
    })


    SoDo.editAlarmVolumeSlider.oninput = function () {
        SoDo.editAlarmVolumeInput.value = this.value;
    };
    SoDo.editAlarmVolumeSlider.onchange = function () {
        SoDo.editAlarmVolumeInput.value = this.value;
    };
    SoDo.editAlarmVolumeInput.addEventListener("change", function () {
        var num = parseInt(this.value) || 1;
        if (num > 100) {
            num = 100;

        }
        this.value = num;
        SoDo.editAlarmVolumeSlider.value= num;
    });
}

function EditAlarm(t) {
    SonosLog("EditAlarm");

    if (IsVisible(SoDo.editAlarmDIV)) {
        SonosWindows(SoDo.editAlarmDIV);
        SonosLog("EditAlarm Hide");
        return;
    }
    editalarmid = t;
    Array.from(SoDo.editAlarmDaysSelectionCLASS).forEach(function (item) {
        item.checked = false;
    })
    if (t === "Neu") {
        SoDo.editAlarmEnabledTrue.checked = true;
        SoDo.editAlarmStartTimeHour.value ="09:00";
        SoDo.editAlarmVolumeSlider.value = 5 ;
        SoDo.editAlarmVolumeInput.value = 5;
        SoDo.editAlarmPlaylist.value = "";
        SoDo.editAlarmPlaylist.dataset.uri = "";
        SoDo.editAlarmPlaylist.dataset.containerid = "";
        SoDo.editAlarmRandomChecker.checked = false;
        SoDo.editAlarmIncludeRoomsChecker.checked = false;
        SoDo.AlarmSonosPlayerSelector.value = "";
        SoDo.editAlarmDurationSelection.value = "01:00:00";
    } else {
        var dataitem = GetAlarmByID(t); //Dataholder
        //Enabled setzen
        if (dataitem.enabled) {
            SoDo.editAlarmEnabledTrue.checked = true;
            SoDo.editAlarmEnabledFalse.checked = false;
        } else {
            SoDo.editAlarmEnabledTrue.checked = false;
            SoDo.editAlarmEnabledFalse.checked = false;
        }
        //Startzeit setzen
        var starttimetemp = dataitem.startTime;
        editalarmidStartTime = starttimetemp;
        var splittime = starttimetemp.split(':');
        SoDo.editAlarmStartTimeHour.value =splittime[0] + ":" + splittime[1];
        //Raum auswählen
        SoDo.AlarmSonosPlayerSelector.value = dataitem.roomUUID;
        //Playlist auswählen
        var programuri = dataitem.programURI;
        var lastslash = programuri.lastIndexOf("/");
        SoDo.editAlarmPlaylist.value =programuri.substr(lastslash + 1);
        SoDo.editAlarmPlaylist.dataset.containerid = dataitem.containerID;
        SoDo.editAlarmPlaylist.dataset.uri = programuri;
        //Tage
        var days = dataitem.recurrence;
        console.log("Switch EDit Days:" + days);
        switch (days) {
            case 'ONCE':
                SoDo.editAlarmDaysSelectionOnce.checked= true;
                break;
            case 'WEEKDAYS':
                Array.from(SoDo.editAlarmDaysSelectionWeekCLASS).forEach(function (item) {
                    item.checked = true;
                })
                break;
            case 'WEEKENDS':
                Array.from(SoDo.editAlarmDaysSelectionWeekEndCLASS).forEach(function (item) {
                    item.checked = true;
                })
                break;
            case 'DAILY':
                Array.from(SoDo.editAlarmDaysCLASS).forEach(function (item) {
                    item.checked = true;
                })
                break;
            default:
                //einzeltage verarbeiten
                var tagestring = days.substr(3);
                var tage = tagestring.split("");
                for (var i = 0; i < tage.length; i++) {
                   document.getElementById("editAlarmDaysSelection" + tage[i]).checked = true;

                }
        }
        //Lautstärke
        var volume = dataitem.volume;
        SoDo.editAlarmVolumeSlider.value= volume;
        SoDo.editAlarmVolumeInput.value =volume;
        //Dauer
        SoDo.editAlarmDurationSelection.value = dataitem.duration;
        //IncludeLinkedZones
        var includelinkedzones = dataitem.includeLinkedZones;
        if (includelinkedzones === "true") {
            SoDo.editAlarmIncludeRoomsChecker.checked = true;
        } else {
            SoDo.editAlarmIncludeRoomsChecker.checked = false;
        }
        //Random
        var random = dataitem.playMode;
        if (random === "NORMAL") {
            SoDo.editAlarmRandomChecker.checked = false;
        } else {
            SoDo.editAlarmRandomChecker.checked = true;
        }
    }
    SonosLog("EditAlarm Data Loaded");
    SonosWindows(SoDo.editAlarmDIV, false, { overlay: true });
}
//Als erstes die Player laden

//Prüft, bei EditAlarms die Tage und reagiert auf bestimmte Änderungen.
function CheckDays(item) {
    if (item === undefined) return;
    var id = item.target.id;
    if (id === undefined) return;
    let daysselection = Array.from(SoDo.editAlarmDaysSelectionCLASS);
    switch (id) {
        case 'EditAlarmDaysSelectionOnce':
            daysselection.forEach(function (item) {
                item.checked = false;
            })
            SoDo.editAlarmDaysSelectionOnce.checked = true;
            break;
        case 'EditAlarmDaysSelectionDaily':
            daysselection.forEach(function (item) {
                item.checked = false;
            })
            Array.from(SoDo.editAlarmDaysSelectionWeekCLASS).forEach(function (item) {
                item.checked = true;
            });
            Array.from(SoDo.editAlarmDaysSelectionWeekEndCLASS).forEach(function (item) {
                item.checked = true;
            });
            SoDo.editAlarmDaysSelectionDaily.checked = true;
            break;
        default:
            SoDo.editAlarmDaysSelectionOnce.checked= false;
            SoDo.editAlarmDaysSelectionDaily.checked = false;
            var daycounter = document.querySelectorAll('input[name="editAlarmDays"]:checked').length;
            if (daycounter === 7) {
                SoDo.editAlarmDaysSelectionDaily.checked =true;
            }
            if (daycounter === 0) {
                SoDo.editAlarmDaysSelectionOnce.checked = true;
            }
            break;
    }

}
//Gibt die Tage zurück, wann der Wecker losläuft.
function StartDates(recurrence) {

    SonosLog("StartDates Insert:" + recurrence);
    var res;
    switch (recurrence) {
        case 'ONCE':
            res = "Einmalig";
            break;
        case 'WEEKDAYS':
            res = "Mo-Fr";
            break;
        case 'WEEKENDS':
        case 'ON_06':
            res = "Sa und So";
            break;
        case 'DAILY':
            res = "Täglich";
            break;
        default:
            //einzeltage verarbeiten
            var tagestring = recurrence.substr(3);
            var tage = tagestring.split("");
            var inrow = true;
            //Prüfen, ob ide Tage aufeinander folgend sind.
            var i;
            for (i = 0; i < tage.length; i++) {
                var next = parseInt(tage[i + 1]);
                var now = parseInt(tage[i]);
                if (!isNaN(next)) {
                    if (next - now !== 1) {
                        inrow = false;
                        i = tage.length;
                    }
                }
            }
            if (tage.length === 1) {
                inrow = false;
            }
            //String für die Tage bauen
            if (inrow === true) {
                res = GetDays(tage[0]) + " - " + GetDays(tage[tage.length - 1]);
            } else {
                res = "";
                for (i = 0; i < tage.length; i++) {
                    res += GetDays(tage[i]) + ",";
                }
            }

    }
    //Letztes Komma entfernen
    if (res.lastIndexOf(",") === (res.length - 1)) {
        res = res.substr(0, res.length - 1);
    }
    SonosLog("StartDates Return:" + res);
    return res;
}
//Gibt für die Übergebene Zahl einen Tag zurück
function GetDays(v) {
    SonosLog("GetDays Insert:" + v);
    var d = "KeinTagGewählt";
    switch (v) {
        case '0':
            d = "So";
            break;
        case '1':
            d = "Mo";
            break;
        case '2':
            d = "Di";
            break;
        case '3':
            d = "Mi";
            break;
        case '4':
            d = "Do";
            break;
        case '5':
            d = "Fr";
            break;
        case '6':
            d = "Sa";
            break;

    }
    SonosLog("GetDays Return:" + d);
    return d;
}
//Alarm aktivieren, deaktivieren im Backend
function AlarmEnabled(t) {
    SonosLog("AlarmEnabled");
    var ena = t.checked;
    var dataholder = t.parentElement.parentElement.parentElement;
    var id = dataholder.dataset.id;
    var inid = parseInt(id);
    if (isNaN(inid)) {
        alert("Es wurde als Alarm ID:" + id + " übergeben, welches nicht zu einem Int umgewandelt werden konnte (AlarmEnabled 442)");
        return;
    }
    dataholder.dataset.enabled = ena;
    AlarmClock.Alarms.forEach(function (item) {
        if (item.id === inid) {
            item.enabled = ena;
            return;
        }
    });
    SonosAjax("AlarmEnable", "", id, ena);
    SonosLog("AlarmEnabled ID:" + id + " Status:" + ena);
}
//Auswahl der Playlist für den Wecker
function SelectAlarmPlaylist() {
    SonosLog("SelectAlarmPlaylist");
    if (SoDo.selectAlarmPlaylistWrapper.hasChildNodes()) {
        SonosWindows(SoDo.selectAlarmPlaylistDIV);
        return;
    }
    SoDo.selectAlarmPlaylistWrapper.innerHTML ="";
    if (typeof SonosZones.AllPlaylists === "undefined" || SonosZones.AllPlaylists.length === 0) {
        var request = SonosAjax("Browsing", "A:PLAYLISTS");
        request.then(function (alarmplaylistdata) {
            SonosZones.AllPlaylists = alarmplaylistdata;
            let wrapper = "";
            SonosZones.AllPlaylists.forEach(function (item) {
                wrapper += '<DIV OnClick="SetAlarmPlaylist(this)" data-title="' + item.title + '" data-URI="' + item.uri + '" data-containerid="' + item.containerID + '">' + item.title + '</DIV>';
            });
            SoDo.selectAlarmPlaylistWrapper.innerHTML = wrapper;
            SonosWindows(SoDo.selectAlarmPlaylistDIV);
        });
    } else {
        let wrapper = "";
        SonosZones.AllPlaylists.forEach(function (item) {
            wrapper += '<DIV OnClick="SetAlarmPlaylist(this)" data-title="' + item.title + '" data-URI="' + item.uri + '" data-containerid="' + item.containerID + '">' + item.title + '</DIV>';
        });
        SoDo.selectAlarmPlaylistWrapper.innerHTML = wrapper;
        SonosWindows(SoDo.selectAlarmPlaylistDIV);
    }

}
//Playlist laden und zur Auswahl bereit machen.
function SetAlarmPlaylist(item) {
    SoDo.editAlarmPlaylist.value = item.dataset.title;
    SoDo.editAlarmPlaylist.dataset.uri = item.dataset.uri;
    SoDo.editAlarmPlaylist.dataset.containerid = item.dataset.containerid;
    SonosWindows(SoDo.selectAlarmPlaylistDIV, true);
}
function AlarmLoadPlayers(c) {
    if (c === "click") {
        if (IsVisible(SoDo.alarmClockDIV)) {
            SonosWindows(SoDo.alarmClockDIV, undefined, { UseFadeIn: true });
            return;
        }
    }
    var players = Object.getOwnPropertyNames(SonosPlayers);
    var counter = 0;
    for (var i = 0; i < players.length; i++) {
        var uuid = SonosPlayers[players[i]].uuid;
        var name = SonosPlayers[players[i]].name;
        AlarmClock.Rooms[uuid] = name;
        counter++;
    }
    let optionhtml = "";
    for (var i = 0; i < counter; i++) {
        var uuid = players[i];
        optionhtml+='<option value="' + uuid + '">' + AlarmClock.Rooms[uuid] + '</option>';
        AlarmClock.roomsAlarmTime[uuid] = new Array();
    }
    SoDo.AlarmSonosPlayerSelector.innerHTML = optionhtml
    AlarmClockShow(); //Alarme laden
}
function AlarmClockShow(rel) {
    SonosLog("AlarmClock Show");
    if (rel !== "reload") {
        SonosWindows(SoDo.alarmClockDIV, undefined, { UseFadeIn: true });
    }

    if (!IsVisible(SoDo.alarmClockDIV)) {
        return;
    }
    SetVisible(SoDo.alarmClockDIVLoader);
    if (SoDo.alarmClockWrapper.hasChildNodes()) {
        SoDo.alarmClockWrapper.innerHTML = "";
    }
    var request = SonosAjax("GetAlarms");
    request.then(function (data) {
        AlarmClock.Alarms = data;
        SonosLog("AlarmClock data loaded");
        let alarmclockwrapperdata = "";
        AlarmClock.Alarms.forEach(function (item) { 
            var check = "";
            if (item.enabled) {
                check = "checked";
            }
            if (typeof AlarmClock.roomsAlarmTime[item.roomUUID] === "undefined") {
                AlarmClock.roomsAlarmTime[item.roomUUID] = new Array();
                AlarmClock.Rooms[item.roomUUID] = "Player Offline";
            }
            AlarmClock.roomsAlarmTime[item.roomUUID].push(item.startTime);

            var datatag = 'data-id="' + item.id + '" data-starttime="' + item.startTime + '" data-duration="' + item.duration + '" data-recurrence="' + item.recurrence + '" data-roomUUID="' + item.roomUUID + '" data-enabled="' + item.enabled + '" data-programuri="' + item.programURI + '" data-playmode="' + item.playMode + '" data-volume="' + item.volume + '" data-includelinkedzones="' + item.includeLinkedZones + '"';
            alarmclockwrapperdata += '<div id="Alarm_' + item.id + '" class="alarm" ' + datatag + '><div class="displayflex"><div class="displayflex" OnClick="EditAlarm(' + item.id + ')"><div class="roomname">' + AlarmClock.Rooms[item.roomUUID] + '</div><div class="starttime">' + item.startTime + '</div><div class="startdate">' + StartDates(item.recurrence) + '</div></DIV><div class="alarmonoffswitch"><input type="checkbox" OnClick="AlarmEnabled(this)" name="alarmonoffswitch" class="alarmonoffswitch-checkbox" id="myonoffswitch' + item.id + '" ' + check + '/><label class="alarmonoffswitch-label" for="myonoffswitch' + item.id + '"><div class="alarmonoffswitch-inner"></div><div class="alarmonoffswitch-switch"></div></label></div></div></div>';
        });
        alarmclockwrapperdata +='<div id="Neu" class="alarm"><div class="displayflex" OnClick="EditAlarm(\'Neu\')"><div class="displayflex"><div class="roomname">Neu</div><div class="starttime"></div><div class="startdate">Never</div></DIV><div class="alarmonoffswitch"></div>';
        SoDo.alarmClockWrapper.innerHTML = alarmclockwrapperdata;
        SetHide(SoDo.alarmClockDIVLoader);
        SonosLog("AlarmClock each Alarm Loaded");
    });
    request.catch(function (jqXHR) {
        if (jqXHR.statusText === "Internal Server Error") {
            ReloadSite("ACShow");
        } else { alert("Beim laden von ACShow ist ein Fehler aufgetreten."); }
    });
}
function DestroyAlarm() {
    var retval = confirm("Soll der Alarm wirklich gelöscht werden?");
    if (retval) {
        var request = SonosAjax("DestroyAlarm", editalarmid);
        request.then(function () {
            SonosWindows(SoDo.editAlarmDIV);
            AlarmClockShow("reload");
        });
        request.catch(function () {
            SonosWindows(SoDo.editAlarmDIV);
            AlarmClockShow("reload");
        });
    }

}

function GetAlarmByID(id) {
    var alarm;
    var inid = parseInt(id);
    if (isNaN(inid)) {
        alert("Es wurde als Alarm ID:" + id + " übergeben, welches nicht zu einem Int umgewandelt werden konnte (AlarmClock 498)");
        return;
    }
    AlarmClock.Alarms.forEach(function (item) {
        if (item.id === inid) {
            //alarm = Object.assign({}, item);
            alarm = item;
            return;
        }
    });
    return alarm;
}

function SaveAlarmChanges() {
    var saveroomUUID = SoDo.AlarmSonosPlayerSelector.value;
    //StartTime
    var temptime = SoDo.editAlarmStartTimeHour.value;
    if (temptime.length !== 5) {
        alert("Die Eingegebene Startzeit kann nicht interpretiert werden. Bitte anpassen");
        return;
    }
    var savestarttime = temptime + ":00";
    if (SoDo.AlarmSonosPlayerSelector.value === "") {
        alert("ES wurde kein Raum ausgewählt");
        return;
    }
    //Prüfen, ob Startzeit für diesen Raum schon vorhanden ist
    if (typeof AlarmClock.roomsAlarmTime[saveroomUUID] !== "undefined" && AlarmClock.roomsAlarmTime[saveroomUUID].indexOf(savestarttime) > -1) {
        if (savestarttime === editalarmidStartTime) {
            //Die Zeit, die gefunden wurde, ist die Zeit des geöffneten Weckers.
        } else {
            alert("Es gibt schon einen Wecker für diese Zone mit der Startzeit " + temptime + ". Bitte wähle eine andere Uhrzeit");
            return;
        }
    }
    //Playlist
    if (typeof SoDo.editAlarmPlaylist.dataset.containerid === undefined || SoDo.editAlarmPlaylist.dataset.containerid ==="") {
        alert("Es fehlt die Wiedergabeliste, bitte eine auswählen.");
        return;
    }


    //Tage
    var tage = "";
    if (SoDo.editAlarmDaysSelectionDaily.checked) {
        tage = "DAILY";
    } else if (SoDo.editAlarmDaysSelectionOnce.checked) {
        tage = "ONCE";
    } else {
        var tagearray = new Array();
        var i;
        for (i = 0; i < 7; i++) {

            if (document.getElementById("editAlarmDaysSelection"+i).checked) {
                tagearray.push(i);
            }
        }
        if (tagearray.length > 1) {
            tage = "ON_";
            for (i = 0; i < tagearray.length; i++) {
                tage += tagearray[i];
            }

        } else {
            if (typeof tagearray[0] == "undefined") {
                alert("Es wurde nicht ausgewählt, wann der Wecker losgehen soll.");
                return;
            }
            tage = "ON_" + tagearray[0];
        }
    }
    if (SoDo.editAlarmVolumeInput.value === "") {
        SoDo.editAlarmVolumeInput.value = "0";
    }
    var alarm
    if (editalarmid === "Neu") {
        alarm = AlarmClock.Alarms[0];
        alarm.id = 99999;
    } else {
        alarm = GetAlarmByID(editalarmid);
    }
    alarm.enabled = SoDo.editAlarmEnabledTrue.checked;
    alarm.startTime = savestarttime;
    alarm.recurrence = tage;
    alarm.volume = SoDo.editAlarmVolumeInput.value;
    alarm.includeLinkedZones = SoDo.editAlarmIncludeRoomsChecker.checked;
    alarm.playMode = SoDo.editAlarmRandomChecker.checked?"SHUFFLE_NOREPEAT":"NORMAL";
    var programmcontainerid = SoDo.editAlarmPlaylist.dataset.containerid;
    if (alarm.containerID !== programmcontainerid) {
        alarm.programURI = SoDo.editAlarmPlaylist.dataset.uri;
        alarm.programMetaData = "";
        alarm.containerID = programmcontainerid;
    }
    alarm.roomUUID = saveroomUUID;
    alarm.roomName = "";
    alarm.duration = SoDo.editAlarmDurationSelection.value;
    var request = SonosAjax("SetAlarm", alarm);
    request.then(function () { /*ACShow("reload"); */});
    request.catch(function () { alert("Beim laden der Aktion:SaveAlarmChanges für Wecker " + editalarmid + " ist ein Fehler aufgetreten."); });
    SonosWindows(SoDo.editAlarmDIV);
}



