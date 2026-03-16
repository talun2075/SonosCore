const AdminStatus = Object.freeze({
    AKTIV: 2,
    INAKTIV: 1,
    NOTSET:0
});
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
        return "";
    }
    catch (Ex) {
        alert("Es ist ein Fehler beim GetURLParameter aufgetreten:<br>" + Ex.message);
    }
}

function InitParameters() {
    let sp = new SonosParameters()
    sp.UrlDevice = GetURLParameter('device');
    sp.AllowedPlayer = GetURLParameter('player');
    let admin = GetURLParameter('admin');
    if (admin === "true") {
        sp.IsAdmin = AdminStatus.AKTIV;
    } else if(admin === "false") {
        sp.IsAdmin = AdminStatus.INAKTIV;
    }
    CheckLocalStorage(sp);
}
function CheckLocalStorage(sp) {
    let store = getStore("Sonos", true);
    if (store === null) {
        setStore("Sonos", sp);
        store = sp;
    } else {
        //prüfen ob ein parameter geändert wurde.
        if (sp.IsAdmin !== AdminStatus.NOTSET) {
            store.IsAdmin = sp.IsAdmin;
        }
        if (sp.UrlDevice !== "") {
            store.UrlDevice = sp.UrlDevice
        }
        if (sp.AllowedPlayer !== "") {
            store.AllowedPlayer = sp.AllowedPlayer
        }
    }
    setStore("Sonos", store);
    SoVa.LocalStorage = store;
}

function SonosParameters() {
    this.IsAdmin = AdminStatus.NOTSET;
    this.UrlDevice = "";
    this.AllowedPlayer = "";
}