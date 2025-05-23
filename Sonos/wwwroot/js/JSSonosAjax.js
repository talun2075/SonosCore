﻿function SonosAjax(_url, _data, _para1, _para2) {
    /*
	_url= URL als interner Interpreter
	_data= Daten, die an den Server sollen. Meherere als Objekt: {variable1: "value1", variable2:"value2"}
    _para1 und 2 Optionale Parameter, die nur verwendung finden bei speziellen Cases.
	*/
    if (typeof _url === "undefined" || _url === null || _url === "") {
        return false;
    }
    var url;
    var type;
    switch (_url) {
        case "GetListById":
            url = SoVa.apiEventURL + "GetListById/" + _data;
            break;
        case "BaseUrl":
            url = SoVa.apiPlayerURL + "/BaseURL/" + _data;
            break;
        case "LoadDevice":
            url = SoVa.apiDeviceURL + "get";
            break;
        case "GetCoordinators":
            url = SoVa.apiDeviceURL + "GetCoordinators";
            break;
        case "GetLastChangesDateTimes":
            url = SoVa.apiDeviceURL + _url;
            break;
        case "GetPlayers":
            url = SoVa.apiDeviceURL + "GetPlayers/";
            break;
        case "GetPlayer":
        case "GetLongPlayer":
            url = SoVa.apiDeviceURL + _url +"/"+ _para1;
            break;
        case "SetGroups":
            url = SoVa.apiPlayerURL + "SetGroups/" + SoVa.masterPlayer;
            type = "POST";
            break;
        case "SetFadeMode":
            url = SoVa.apiPlayerURL + "SetFadeMode/" + SonosZones.ActiveZoneUUID + "/" + !SonosPlayers[SonosZones.ActiveZoneUUID].playerProperties.currentCrossFadeMode;
            break;
        case "SetVolume":
            if (typeof _para1 === "undefined" || typeof _para2 === "undefined") {
                return false;
            }
            url = SoVa.apiPlayerURL + "SetVolume/" + _para1 + "/" + _para2;
            break;
        case "SetGroupVolume":
            if (typeof _para1 === "undefined" || typeof _para2 === "undefined") {
                return false;
            }
            url = SoVa.apiPlayerURL + "SetGroupVolume/" + _para1 + "/" + _para2;
            break;
        case "SetAlarm":
            url = SoVa.apiSettingURL + _url;
            type = "POST";
            break;
        case "SaveQueue":
        case "ExportQueue":
        case "Enqueue":
        case "ReplacePlaylist":
        case "AddFavItem":
        case "SetSongMeta":
        case "SetSleepTimer":
        case "Seek":
        case "SetFilterRating":
        case "SetRatingFilter":
            url = SoVa.apiPlayerURL + _url + "/" + SonosZones.ActiveZoneUUID;
            type = "POST";
            break;
        case "CheckPlayerPropertiesWithClient":
            url = SoVa.apiPlayerURL + _url + "/" + _para1;
            type = "POST";
            break;
        case "GetSongMeta":
            url = SoVa.apiPlayerURL + _url;
            type = "POST";
            break;
        case "Browsing":
            url = SoVa.apiPlayerURL + _url + "/" + SonosZones.ActiveZoneUUID;
            type = "POST";
            break;
        case "GetPlaylists":
        case "GetZones":
        case "GetFavorites":
            url = SoVa.apiZoneURL + _url;
            break;
        case "RemoveFavItem":
            url = SoVa.apiPlayerURL + _url;
            break;
        case "RemoveSongInPlaylist":
        case "SetSongInPlaylist":
            url = SoVa.apiPlayerURL + _url+"/" + SonosZones.ActiveZoneUUID + "/" + _para1;
            break;
        case "ReorderTracksinQueue":
            url = SoVa.apiPlayerURL + "ReorderTracksinQueue/" + SonosZones.ActiveZoneUUID + "/" + _para1 + "/" + _para2;
            break;
        case "SetAudioIn":
        case "Next":
        case "Previous":
        case "GetGroupVolume":
        case "GetVolume":
            url = SoVa.apiPlayerURL + _url + "/" + SonosZones.ActiveZoneUUID;
            break;
        case "SetMute":
            url = SoVa.apiPlayerURL + _url + "/" + _para1;
            break;
        case "FillPlayerPropertiesDefaults":
            url = SoVa.apiPlayerURL + _url + "/" + _para1 + "/" + _para2;
            break;
        case "DestroyAlarm":
            url = SoVa.apiSettingURL + _url + "/" + _data;
            break;
        case "GetAlarms":
            url = SoVa.apiSettingURL + _url;
            break;
        case "GetSonosSettings":
            url = SoVa.apiSettingURL + _url;
            break;
        case "SetUpdateMusicIndex":
        case "GetUpdateIndexInProgress":
            url = SoVa.apiSettingURL + _url;
            break;
        case "GetZonebyRincon":
        case "GetPlayerPlaylist":
        case "GetPlayState":
        case "Play":
        case "Pause":
            url = SoVa.apiPlayerURL + _url + "/" + _para1;
            break;
        case "GetAktSongInfo":
            var link = SonosZones.ActiveZoneUUID;
            if (typeof link === "undefined" || link === "" || link === null)
                link = _para1;
            url = SoVa.apiPlayerURL + "GetAktSongInfo/" + link;
            break;
        case "AlarmEnable":
            url = SoVa.apiSettingURL + _url + "/" + _para1 + "/" + _para2;
            break;
        case "SetPlaymode":
            url = SoVa.apiPlayerURL + _url + "/" + SonosZones.ActiveZoneUUID + "/" + _para1;
            break;
        case "aagb":
            url = "";
            break;
        default:
            if (typeof _url === "undefined" || _url === "") {
                alert("Übergeben url ist nicht definiert und kann somit nicht interpretiert werden.");
                return false;
            }
            url = _url;
    }
    if (typeof _type !== "undefined") {
        type = _type;
    }
    //Es werden aktuell nur Get und Post Supportet
    if (type !== "GET" && type !== "POST") {
        type = "GET";
    }
    //Wenn keine Daten definiert sind, dann leer überegeben
    if (typeof _data === "undefined") {
        _data = "";
    }
    //if (typeof _dataType === "undefined") {
    //    _dataType = "json";
    //}
    //return $.ajax({
    //    type: type,
    //    url: url,
    //    data: _data,
    //    dataType: _dataType
    //});
    return Send(url, _data, type);
}
async function Send(url = '', data = {}, t = 'GET') {
    if (typeof BasePath !== "undefined" && BasePath !== "" && BasePath !== null) {
        url = BasePath + url;
    }
    var fetchparams = {
        method: t,
        mode: 'cors',
        cache: 'no-cache',
        credentials: 'same-origin',
        headers: {
            'Content-Type': 'application/json'
        },
        redirect: 'follow',
        referrerPolicy: 'no-referrer'
    };
    if (t === "POST") {
        fetchparams.body = JSON.stringify(data);
    }
    const response = await fetch(url, fetchparams);

    if (!response.ok) {
        const errorText = await response.text();
        throw new Error(`Status: ${response.status},\nMessage: ${errorText}`);
    }

    const responseText = await response.text();
    try {
        return JSON.parse(responseText);
    } catch (e) {
        return responseText;
    }
}

//async function Send(url = '', data = {}, t = 'GET') {
//    // Default options are marked with *
//    if (typeof BasePath !== "undefined" && BasePath !== "" && BasePath !== null) {
//        url = BasePath + url;
//    }
//    var fetchparams = {
//        method: t, // *GET, POST, PUT, DELETE, etc.
//        mode: 'cors', // no-cors, *cors, same-origin
//        cache: 'no-cache', // *default, no-cache, reload, force-cache, only-if-cached
//        credentials: 'same-origin', // include, *same-origin, omit
//        headers: {
//            'Content-Type': 'application/json'
//        },
//        redirect: 'follow', // manual, *follow, error
//        referrerPolicy: 'no-referrer' // no-referrer, *no-referrer-when-downgrade, origin, origin-when-cross-origin, same-origin, strict-origin, strict-origin-when-cross-origin, unsafe-url
//    };
//    if (t === "POST") {
//        fetchparams.body = JSON.stringify(data); // body data type must match "Content-Type" header
//    }
//    const response = await fetch(url, fetchparams);

//    var res = await response.text(); //take text

//    try {
//        return JSON.parse(res);// parses JSON response into native JavaScript objects
//    } catch (e) {
//        return res;
//    }
//}