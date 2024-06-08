/*
Alle Functions, die regelmäßig über SetTimeout aufgerufen werden
*/
//Prüft, ob sich an den Zonen etwas geändert hat und dann wird entsprechend GetZones aufgerufen
var rinconpropcounter = 0;
var rinconpropcounterReset = 0;
function CheckPlayersPP() {
    clearTimeout(SoVa.GetAktSongInfoTimerID);
    try {
        //Bei Refreshstop sich selber aufrufen bis das wieder normal ist
        var uuid = SonosZones.ActiveZoneUUID;
        SonosAjax("CheckPlayerPropertiesWithClient", SonosPlayers[uuid].playerProperties, uuid)
            .success(function () { console.log("CheckPlayersPP gut gelaufen") })
            .fail(console.log("CheckPlayersPP schlecht gelaufen"));


    } catch (ex) {
        //SoVa.GetAktSongInfoTimerID = window.setTimeout("GetAktSongInfo()", 30000);
        alert("GetAktSongInfo: " + ex.message);
    }
} //Ende CurrentState
function GetPlayerLastChanges() {
    clearTimeout(SoVa.TopologieChangeID);
    SonosAjax("GetLastChangesDateTimes").success(function (data) {
        try {
            if (typeof data === "undefined" || data === null) {
                console.log("GetPlayerLastChanges danten null");
                window.setTimeout("GetPlayerLastChanges()", SoVa.TopologieChangeTime);
                return;
            }
            var rincons = Object.getOwnPropertyNames(data);
            for (var i = 0; i < rincons.length; i++) {
                var uuid = rincons[i]
                if (uuid.substring(0, SoVa.RinconChecker.length) === SoVa.RinconChecker) {
                    //Rincon Property
                    var player = SonosPlayers[uuid];
                    if (typeof player === "undefined") {
                        SonosAjax("GetPlayer", "", uuid).success(function (datap) {
                            //hier nun Player anlegen.
                            var u = datap.uuid;
                            SonosPlayers[u] = new SonosPlayer(u, datap.name);
                            SonosPlayers[u].playerProperties = datap.playerProperties;
                            SonosPlayers[u].ratingFilter = datap.ratingFilter;
                            SonosZones.RenderCurrentTrack();
                            SonosZones.RenderDevices();
                        }).fail(function (datap) {
                            console.log(datap);
                        });

                        continue;
                    }
                    //hier ist also ein bekannter Player
                    var serverdate = new Date(data[uuid]);
                    if (serverdate.getMilliseconds() === 0) {
                        SonosAjax("FillPlayerPropertiesDefaults", "", player.uuid, true);
                        continue;
                    }
                    if (SonosZones.CheckStringIsNullOrEmpty(player.playerProperties.localGroupUUID) ||
                        (player.playerProperties.localGroupUUID !== player.uuid && player.playerProperties.groupCoordinatorIsLocal) ||
                        (player.playerProperties.localGroupUUID === player.uuid && !player.playerProperties.groupCoordinatorIsLocal)) {
                        SonosAjax("FillPlayerPropertiesDefaults", "", player.uuid, false);
                        continue;
                    }
                    /*                        
                     * Wenn die Playlist nicht mit der Anzahl der Tracks bzw.der CurrentTracknumber passt.
                     */
                    if (player.playerProperties.playlist.playListItems.length === 0 || (player.playerProperties.playlist.playListItems.length > 0 && (player.playerProperties.currentTrackNumber > player.playerProperties.playlist.playListItems.length ||
                        player.playerProperties.numberOfTracks !== player.playerProperties.playlist.playListItems.length))) {
                        if (player.playerProperties.playlist.playListItems.length === 1 && player.playerProperties.playlist.playListItems[0].album === "Leer" || player.playerProperties.playlist.isEmpty) {

                        } else {
                            player.playlist.LoadPlaylist(player, false,"Event Playlist nicht AnzahlTracks");
                        }
                    }
                    if (typeof player.LastChange !== "object") {
                        console.log(player.Name);
                    }
                    var timerdelta = (serverdate.getTime() - player.LastChange.getTime());
                    //So nun einen bereits aktualisierten Player.
                    if (timerdelta > 120000 && player.playerProperties.groupCoordinatorIsLocal === true) {
                        player.LastChange = serverdate;
                        //Player zu lange weg, daher nun GetLongPlayer aufrufen, wo auch ein Portscan gemacht wird.
                        SonosAjax("GetLongPlayer", "", uuid).success(function (data2) {
                            try {
                                console.log("playerladen getlast wegen zeit");
                                console.log(data2.LastChange);
                                SonosPlayers[uuid].LastChange = new Date(data2.LastChange);
                                SonosPlayers[uuid].CheckPlayerProperties(data2.playerProperties);
                            }
                            catch (ex) {
                                console.log("GetPlayerLastChanges:BekannterAktualisierterPlayer:Verarbeitung:Fehler")
                                console.log(ex);
                                console.log(data2);
                            }
                        }).fail(function (data2) {
                            console.log("GetPlayerLastChanges:BekannterAktualisierterPlayer:Laden:Fehler")
                            console.log(data2);
                        });
                    }
                }
            };
            //hier nun die SonosPlayers durchlaufen um zu sehen, ob einer gelöscht werden muss.
            var playersuuids = Object.getOwnPropertyNames(SonosPlayers);
            for (var i = 0; i < playersuuids.length; i++) {
                var playersuuid = playersuuids[i];
                if (typeof data[playersuuid] === "undefined") {
                    delete SonosPlayers[playersuuid];
                    SonosZones.RenderDevices();
                }
            }

        }
        catch (ex) {
            console.log("Fehler bei GetPlayerLastChanges:");
            console.log(ex);
            console.log(data);
        }
        SoVa.TopologieChangeID = window.setTimeout("GetPlayerLastChanges()", SoVa.TopologieChangeTime);
    }).fail(function (data) {
        console.log(data);
    });
}
function Eventing() {
    if (typeof window.EventSource === "undefined") {
        //ie also return
        return;
    }
    if (typeof SoVa.SSE_Event_Source == "undefined" || SoVa.SSE_Event_Source.readyState == SoVa.SSE_Event_Source.CLOSED) {
        SoVa.SSE_Event_Source = new window.EventSource(SoVa.apiEventURL);
    }
    SoVa.SSE_Event_Source.onopen = function () {
        console.log("Event:Connection Opened ");
    };
    SoVa.SSE_Event_Source.onClose = function () {
        console.log("Event:Connection Closed ");
    };
    SoVa.SSE_Event_Source.onerror = function (event) {
        if (event.eventPhase !== window.EventSource.CLOSED) {
            console.log("Event:Connection Error:");
            console.log(event);
        }
    };
    SoVa.SSE_Event_Source.addEventListener("sonos", function (event) {
        try {
            if (typeof event.data === "undefined" || event.data === "") {
                return;
            }
            CheckPlayerEventData(event);
        } catch (ex) {
            console.log("Fehlerhafte Event Daten:" + event.data);
            console.log(ex);
        }
    });
    //SoVa.SSE_Event_Source.onmessage = function (event) {
    //    // document.getElementById('test').innerHTML += event.data;  
    //    try {
    //        if (typeof event.data === "undefined" || event.data === "") {
    //            return;
    //        }
    //        CheckPlayerEventData(event);
    //    } catch (ex) {
    //        console.log("Fehlerhafte Event Daten:" + event.data);
    //        console.log(ex);
    //    }
    //};
    //console.log("SSE started");
};
function ChangeGlobalSettings(data) {
    switch (data.changeType) {
        case "SavedQueuesUpdateID":
            //SonosZones.RenderAllPlaylist(true);
            break;
        case "ShareListUpdateID":
            //if (IsVisible(SoDo.musikIndexLoader)) {
            //    SetHide(SoDo.musikIndexLoader);
            //    SetVisible(SoDo.musikIndexCheck);
            //    window.setTimeout("SetHide(SoDo.musikIndexCheck);", 1000);
            //    window.setTimeout("SonosZones.RenderAllPlaylist(true)", 2000);
            //}
            break;
        case "ShareIndexInProgress":
            var val = data.changedValues.ShareIndexInProgress === "True";
            if (val === true) {
                if (!IsVisible(SoDo.musikIndexLoader)) {
                    SetVisible(SoDo.musikIndexLoader);
                }
            } else {
                if (IsVisible(SoDo.musikIndexLoader)) {
                    SetHide(SoDo.musikIndexLoader);
                    SetVisible(SoDo.musikIndexCheck);
                    window.setTimeout("SetHide(SoDo.musikIndexCheck);", 1500);
                    window.setTimeout("SonosZones.RenderAllPlaylist(true)", 2000);
                }
            }
            break;
        case "AlarmListVersion":
            if (alarmClockDIV.is(":visible")) {
                ACShow("reload");
            }
            break;
        case "ReloadNeeded":
            window.setTimeout("ReloadSite()", 2000);
            break;
        default:
            console.log("Discovery Unbekannt:" + data.changeType);
            break;
    }
}
function ChangePlayerSettings(data) {
    let player = SonosPlayers[data.uuid];
    let dataLastChange = new Date(data.lastChange);
    if (player.LastChange !== dataLastChange) {
        player.LastChange = dataLastChange;
    }
    if (typeof player === "undefined") return;
    switch (data.changeType) {
        case "AlarmListVersion":
            if (alarmClockDIV.is(":visible")) {
                ACShow("reload");
            }
        case "RatingFilter":
            if (JSON.stringify(player.RatingFilter) !== JSON.stringify(data.changedValues.RatingFilter)) {
                player.RatingFilter = data.changedValues.RatingFilter;
                if (data.uuid === SonosZones.ActiveZoneUUID) {
                    SonosZones.RenderRatingFilter(data.uuid);
                }
            }
            break;
        case "QueueChangeResort":
            var val = data.changedValues.EnqueuedTransportURI;
            if (player.playerProperties.enqueuedTransportURI !== val) {
                player.playerProperties.enqueuedTransportURI = val;
                if (data.uuid === SonosZones.ActiveZoneUUID) {
                    SoVa.browsefirst = 0;
                    SonosZones.RenderAllPlaylist(false);
                    player.playlist.LoadPlaylist(player, true, "QueueChangeResort");
                }
            }
            break;
        case "IsIdle":
            var val = data.changedValues.IsIdle === "True";
            if (player.playerProperties.deviceProperties_IsIdle !== val) {
                player.playerProperties.deviceProperties_IsIdle = val;
                if (player.playerProperties.deviceProperties_IsIdle === true) {
                    //Player pausieren.
                    if (player.playerProperties.transportStateString === "PLAYING") {
                        player.playerProperties.transportStateString = "PAUSED_PLAYBACK";

                    }
                }
                if (data.uuid === SonosZones.ActiveZoneUUID) {
                    SonosZones.RenderTransportState(data.uuid);
                }
                SonosZones.RenderDeviceTransportState(data.uuid);
            }
            break;
        case "LastChangedPlayState":
            //ignorieren weil nicht benötigt.
            break;
        case "Volume":
            var vol = parseInt(data.changedValues.Volume);
            if (player.playerProperties.volume !== vol && !isNaN(vol)) {
                player.playerProperties.volume = vol;
                if (data.uuid === SonosZones.ActiveZoneUUID) {
                    SonosZones.RenderVolume(data.uuid);
                }
            }
            break;
        case "QueueChanged":
            var qc = parseInt(data.changedValues.QueueChanged);
            var override = data.changedValues.Override === "True";
            if (player.playerProperties.QueueChanged !== qc && !isNaN(qc)) {
                player.playerProperties.QueueChanged = qc;
                if (data.uuid === SonosZones.ActiveZoneUUID) {
                    player.playlist.LoadPlaylist(player, override, "QueueChanged");
                }
            }
            break;
        case "GroupVolume":
            var gvol = parseInt(data.changedValues.GroupVolume);
            if (player.playerProperties.groupRenderingControl_GroupVolume !== gvol && !isNaN(gvol)) {
                player.playerProperties.groupRenderingControl_GroupVolume = gvol;
                if (data.uuid === SonosZones.ActiveZoneUUID) {
                    SonosZones.RenderVolume(data.uuid);
                }
            }
            break;
        case "LineInConnected":
            var v = data.changedValues.LineInConnected === "True";
            if (player.playerProperties.AudioInput_LineInConnected !== v) {
                player.playerProperties.AudioInput_LineInConnected = v;
                if (data.uuid === SonosZones.ActiveZoneUUID) {
                    SonosZones.RenderAudioIn(data.uuid);
                }
            }
            break;
        case "AVTransportURI":
            if (player.playerProperties.avTransportURI !== data.changedValues.AvTransportURI) {
                player.playerProperties.avTransportURI = data.changedValues.AvTransportURI;
                //x-rincon:RINCON_000E5823E01C01400 == Spielt den Player mit der RINCON ab.
                if (data.uuid === SonosZones.ActiveZoneUUID) {
                    SonosZones.RenderPlaylist(data.uuid);
                    SonosZones.RenderPlaylistCounter(data.uuid);
                    SonosZones.RenderNextTrack(data.uuid);
                }
            }
            break;
        case "RemainingSleepTimerDuration":
            if (player.playerProperties.remainingSleepTimerDuration !== data.changedValues.RemainingSleepTimerDuration) {
                player.playerProperties.remainingSleepTimerDuration = data.changedValues.RemainingSleepTimerDuration;
                if (data.uuid === SonosZones.ActiveZoneUUID) {
                    SonosZones.RenderSleepTimer(data.uuid);
                }
            }
            break;
        case "TransportState":
            if (player.playerProperties.transportStateString !== data.changedValues.TransportState) {
                player.playerProperties.transportStateString = data.changedValues.TransportState;
                if (data.uuid === SonosZones.ActiveZoneUUID) {
                    SonosZones.RenderTransportState(data.uuid);
                }
                SonosZones.RenderDeviceTransportState(data.uuid);//hier unabhängig das play Rendern.
            }
            break;
        case "CurrentCrossFadeMode":
            var val = data.changedValues.CurrentCrossFadeMode === "True";
            if (player.playerProperties.currentCrossFadeMode !== val) {
                player.playerProperties.currentCrossFadeMode = val;
                if (data.uuid === SonosZones.ActiveZoneUUID) {
                    SonosZones.RenderCrossFadeMode(data.uuid);
                }
            }
            break;
        case "CurrentPlayMode":
            var newval = data.changedValues.CurrentPlayMode;
            var oldval = player.playerProperties.currentPlayModeString;
            if (oldval !== newval) {
                player.playerProperties.currentPlayModeString = newval;
                //hier nun prüfen, ob die Playlist neu geladen werden muss.
                if (newval.startsWith("SHUFFLE") || oldval.startsWith("SHUFFLE")) {
                    player.playlist.LoadPlaylist(player, false, "CurrentPlayMode Changed");
                }
                if (data.uuid === SonosZones.ActiveZoneUUID) {
                    SonosZones.RenderPlayMode(data.uuid);
                    SonosZones.RenderNextTrack(data.uuid);
                    if (newval.startsWith("SHUFFLE") || oldval.startsWith("SHUFFLE")) {
                        SonosZones.RenderPlaylist(data.uuid);
                    }

                }
            }
            break;
        case "CurrentTrackNumber":
            var ctn = parseInt(data.changedValues.CurrentTrackNumber);
            if (player.playerProperties.currentTrackNumber !== ctn && !isNaN(ctn)) {
                player.playerProperties.currentTrackNumber = ctn;
                if (data.uuid === SonosZones.ActiveZoneUUID) {
                    SonosZones.RenderPlaylistCounter(data.uuid);
                    SonosZones.RenderCurrentTrackinPlaylist(data.uuid, player.playerProperties.currentTrackNumber, "CheckPlayerEventData");
                    SonosZones.RenderNextTrack(data.uuid);
                }
            }
            break;
        case "RelTime":
            var ctn = parseInt(data.changedValues.CurrentTrackNumber);
            if (player.playerProperties.currentTrackNumber !== ctn && !isNaN(ctn) && ctn > 0) {
                player.playerProperties.currentTrackNumber = ctn;
                if (data.uuid === SonosZones.ActiveZoneUUID) {
                    SonosZones.RenderPlaylistCounter(data.uuid);
                    SonosZones.RenderCurrentTrackinPlaylist(data.uuid, player.playerProperties.currentTrackNumber, "CheckPlayerEventData");
                    SonosZones.RenderNextTrack(data.uuid);
                }
            }
            if (JSON.stringify(player.playerProperties.currentTrack.relTime) !== data.changedValues.RelTime) {
                player.playerProperties.currentTrack.relTime = JSON.parse(data.changedValues.RelTime);
                if (data.uuid === SonosZones.ActiveZoneUUID) {
                    SonosZones.RenderTrackTime(data.uuid);
                }
            }

            //let newReltime = JSON.parse(data.changedValues.RelTime)
            //var t = data.changedValues.RelTime.split(":");
            //for (var i = 0; i < t.length; i++) {
            //    t[i] = parseInt(t[i]);
            //}
            //var changed = false;
            //if (player.playerProperties.currentTrack.relTime.hours !== t[0]) {
            //    player.playerProperties.currentTrack.relTime.hours = t[0];
            //    changed = true;
            //}
            //if (player.playerProperties.currentTrack.relTime.minutes !== t[1]) {
            //    player.playerProperties.currentTrack.relTime.minutes = t[1];
            //    changed = true;
            //}
            //if (player.playerProperties.currentTrack.relTime.seconds !== t[2]) {
            //    player.playerProperties.currentTrack.relTime.seconds = t[2];
            //    changed = true;
            //}
            //if (changed) {
            //    var totalseconds = t[2] + t[1] * 60;
            //    if (t[0] > 0) {
            //        totalseconds += t[0] * 60 * 60;
            //    }
            //    player.playerProperties.currentTrack.relTime.totalSeconds = totalseconds;
            //    player.playerProperties.currentTrack.relTime.string = data.changedValues.RelTime;
            //}
            //if (data.uuid === SonosZones.ActiveZoneUUID && changed) {
            //    SonosZones.RenderTrackTime(data.uuid);
            //}
            break;
        case "NextTrack":
            var nexttrack = JSON.parse(data.changedValues.NextTrack);
            if (player.playerProperties.nextTrack.uri !== nexttrack.uri) {
                player.playerProperties.nextTrack = nexttrack;
                if (data.uuid === SonosZones.ActiveZoneUUID) {
                    SonosZones.RenderNextTrack(data.uuid);
                }
            }
            break;
        case "CurrentTrack":
            var cutrack = JSON.parse(data.changedValues.CurrentTrack);
            if (player.playerProperties.currentTrack.uri !== cutrack.uri) {
                player.playerProperties.currentTrack = cutrack;
                if (data.uuid === SonosZones.ActiveZoneUUID) {
                    SonosZones.RenderNextTrack(data.uuid);
                    SonosZones.RenderCurrentTrack();
                    SonosZones.RenderPlaylist(data.uuid);
                }
            }

            break;
        case "GroupMute":
            var val = data.changedValues.GroupMute === "True";
            if (player.playerProperties.groupRenderingControl_GroupMute !== val) {
                player.playerProperties.groupRenderingControl_GroupMute = val;
                if (player.playerProperties.groupCoordinatorIsLocal === true) {
                    player.playerProperties.mute = val;
                }
                if (data.uuid === SonosZones.ActiveZoneUUID) {
                    SonosZones.RenderMute(data.uuid);
                }
            }
            break;
        case "Mute":
            var val = data.changedValues.Mute === "True";
            if (player.playerProperties.mute !== val) {
                player.playerProperties.mute = val;
                //if (data.uuid === SonosZones.ActiveZoneUUID) {
                SonosZones.RenderMute(data.uuid)
                //}
            }
            break;
        case "LocalGroupUUID":
            var val = data.changedValues.LocalGroupUUID;
            if (!SonosZones.CheckStringIsNullOrEmpty(val) && player.playerProperties.localGroupUUID !== val) {
                player.playerProperties.localGroupUUID = val;
                SonosZones.RenderDevices();
            }
            break;
        case "EnqueuedTransportURI":
            var val = data.changedValues.EnqueuedTransportURI;
            if (player.playerProperties.enqueuedTransportURI !== val) {
                player.playerProperties.enqueuedTransportURI = val;
                if (data.uuid === SonosZones.ActiveZoneUUID) {
                    SoVa.browsefirst = 0;
                    SonosZones.RenderAllPlaylist(false);

                }
            }
            break;
        case "EnqueuedTransportURIMetaData":
            var val = data.changedValues.EnqueuedTransportURIMetaData;
            if (JSON.stringify(player.playerProperties.enqueuedTransportURIMetaData) !== val) {
                player.playerProperties.enqueuedTransportURIMetaData = JSON.parse(val);
                if (data.uuid === SonosZones.ActiveZoneUUID) {
                    SoVa.browsefirst = 0;
                    SonosZones.RenderNextTrack(data.uuid);
                    SonosZones.RenderPlaylist(data.uuid);
                    SonosZones.RenderPlaylistCounter(data.uuid);
                }
            }
            break;
        case "ZoneGroupName":
            var val = data.changedValues.ZoneGroupName;
            if (player.playerProperties.ZoneGroupTopology_ZoneGroupID !== val) {
                player.playerProperties.ZoneGroupTopology_ZoneGroupID = val;
                SonosZones.RenderDevices();
            }
            break;
        case "ZoneGroupID":
            var val = data.changedValues.ZoneGroupID;
            if (player.playerProperties.ZoneGroupTopology_ZoneGroupName !== val) {
                player.playerProperties.ZoneGroupTopology_ZoneGroupName = val;
                SonosZones.RenderDevices();
            }
            break;
        case "ZonePlayerUUIDsInGroup":
            var val = data.changedValues.ZonePlayerUUIDsInGroup;
            var cuvalstring = JSON.stringify(player.playerProperties.zoneGroupTopology_ZonePlayerUUIDsInGroup)
            if (cuvalstring !== val) {
                player.playerProperties.zoneGroupTopology_ZonePlayerUUIDsInGroup = JSON.parse(val);
                SonosZones.RenderDevices();
            }
            break;
        case "GroupCoordinatorIsLocal":
            var val = data.changedValues.GroupCoordinatorIsLocal === "True";
            if (player.playerProperties.groupCoordinatorIsLocal !== val) {
                player.playerProperties.groupCoordinatorIsLocal = val;
                SonosZones.RenderDevices();
            }
            break;
        case "SleepTimerRunning":
            var val = data.changedValues.SleepTimerRunning === "True";
            if (player.playerProperties.sleepTimerRunning !== val) {
                player.playerProperties.sleepTimerRunning = val;
                if (player.playerProperties.sleepTimerRunning === false) {
                    player.playerProperties.remainingSleepTimerDuration = "aus";
                }
                if (data.uuid === SonosZones.ActiveZoneUUID) {
                    SonosZones.RenderSleepTimer(data.uuid);
                }
            }
            break;
        case "QueueChangedEmpty":
            player.playerProperties.playlist.numberOfTracks = 0;
            player.playerProperties.playlist.totalMatches = 0;
            player.playerProperties.playlist.playListItems = [];
            if (data.uuid === SonosZones.ActiveZoneUUID) {
                SonosZones.RenderPlaylist(data.uuid);
                SonosZones.RenderPlaylistCounter(data.uuid);
            }
            break;
        case "QueueChangedSaved":
            SonosZones.RenderAllPlaylist(true);
            break;
        case "NumberOfTracks":
            var val = parseInt(data.changedValues.NumberOfTracks);
            if (player.playerProperties.numberOfTracks !== val && !isNaN(val)) {
                player.playerProperties.numberOfTracks = val;
                if (data.uuid === SonosZones.ActiveZoneUUID) {
                    SonosZones.RenderPlaylistCounter(data.uuid);
                    SonosZones.RenderCurrentTrackinPlaylist(data.uuid, player.playerProperties.currentTrackNumber, "CheckPlayerEventData");
                    SonosZones.RenderNextTrack(data.uuid);
                }
            }
            break;
        default:
            console.log("Unbekannt:" + data.changeType);
            break;
    }
}
function CheckPlayerEventData(event) {
    try {
        var data = JSON.parse(event.data);
        SoVa.LastEventID = parseInt(data.changedValues.EventID);
        if (data.changeType !== "RelTime") {
            console.log(data);
        }
        if (data.uuid === SoVa.EventSourceDiscovery) {
            ChangeGlobalSettings(data);
        } else {
            ChangePlayerSettings(data);
        }
    }
    catch (ex) {
        console.log(ex);
        console.log(event);
    }
}
function GetLatestEvents() {
    //hole die letzten Events ab.
    SonosAjax("GetListById", SoVa.LastEventID).success(function (eventlist) {
        try {
            let workedtype = [];
            while (eventlist.length > 0) {
                var curevent = eventlist.pop();
                if (typeof curevent !== "undefined" && curevent !== null) {
                    let tEventId = parseInt(curevent.changedValues.EventID)
                    if (SoVa.LastEventID < tEventId) {
                        SoVa.LastEventID = tEventId;
                    }
                    let workt = curevent.uuid + curevent.changeType;
                    if (!workedtype.includes(workt)) {
                        workedtype.push(workt);
                        if (curevent.uuid === SoVa.EventSourceDiscovery) {
                            ChangeGlobalSettings(curevent);
                        } else {
                            ChangePlayerSettings(curevent);
                        }
                    }
                }
            }
        }
        catch (ex) {
            console.log(ex);
        }
    });

}