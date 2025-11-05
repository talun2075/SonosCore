function PlayerDevicePropertiesTypes(_uuid, _val, _ty) {
    this.uuid = _uuid;
    this.value = _val;
    this.type = Types[_ty];
}
var Types = {
    buttonLockState: 0, bass: 1, ledState: 2, loudness: 3, outputFixed: 4, treble: 5, name: 6
};

function SettingClass() {
    let t = this;
    this.settings = null;
    this.settingprops = ["dateFormat", "timeFormat", "timeServer", "dailyIndexRefreshTime", "currentSonosTime", "currentUTCTime", "currentLocalTime", "timeZoneData", "externalString", "autoAdjustDst"];
    let NotRendered = "none";
    let PlayerDomCreated = NotRendered;
    let SettingsDomCreated = NotRendered;
    let Slidertext = "SliderText";
    this.AjaxRender = document.getElementById("Ajaxloader");
    let AjaxRenderSettings = document.getElementById("AjaxloaderSettings");
    this.Init = function () {

        Send("/Devices/GetPlayerNamesAndUUID").then(function (data) {
            console.log(data);
            t.RenderPlayerList(data);
            document.getElementById("LoadSettings").addEventListener("click", function (s) {
                t.RenderSettings();
            })
        }).catch(function (ex) {
            console.log(ex);
            alert(ex.statusText + " " + ex.responseText);
        })

    }
    this.RenderPlayerList = function (players) {
        let selectdom = document.createElement("select");
        selectdom.name = "PlayerSelection";
        selectdom.id = selectdom.name;

        let selectoptiondefault = document.createElement("option");
        selectoptiondefault.text = "Sonos Player auswählen";
        selectoptiondefault.value = NotRendered;
        selectdom.appendChild(selectoptiondefault);
        for (var propertyName in players) {
            let selectoptioplayer = document.createElement("option");
            selectoptioplayer.text = propertyName;
            selectoptioplayer.value = players[propertyName];
            selectdom.appendChild(selectoptioplayer);
        }
        var playersdiv = document.getElementById("Playerlist");
        selectdom.addEventListener("change", function (s) { t.SetPlayer(s.target.value) });
        playersdiv.appendChild(selectdom);
        this.AjaxRender.style.display = "none";
    }
    this.SetPlayer = function (uuid) {
        this.AjaxRender.style.display = "block";
        if (uuid === NotRendered) {
            this.HidePlayerDom(true);
            this.AjaxRender.style.display = "none";
            return;
        }
        //werte holen und an RenderPlayer geben
        Send("/Devices/GetPlayerProperties/" + uuid).then(function (data) {
            t.RenderPlayerDom(data);
        }).catch(function (ex) {
            console.log(ex);
            alert(ex.statusText + " " + ex.responseText);
        });
    }
    this.RenderPlayerDom = function (player) {
        console.log("RenderPlayerDom");
        console.log(player);
        if (PlayerDomCreated === NotRendered) {
            this.CreatePlayerDom(player);
        }
        this.HidePlayerDom(false);
        for (var prop in player) {
            if (prop === "supportsOutputFixed") continue;
            let val = player[prop];
            let propdom = document.getElementById(prop);
            if (typeof val == "boolean") {
                if (prop === "outputFixed") {
                    if (player.supportsOutputFixed === propdom.disabled) {
                        propdom.disabled = !player.supportsOutputFixed
                    }
                }
                if (prop === "headphoneConnected") {
                    propdom.disabled = true;
                }
                if (prop === "outputFixed" || prop === "headphoneConnected") {
                    let textdom = propdom.parentNode.parentNode.firstChild;
                    let dis = propdom.disabled;
                    if (dis === true) {
                        if (!textdom.classList.contains("deactive"))
                            textdom.classList.add("deactive");
                    } else {
                        if (textdom.classList.contains("deactive"))
                            textdom.classList.remove("deactive");
                    }
                }
                if (val !== propdom.checked) {
                    propdom.checked = val;
                }
            } else if (typeof val == "string") {
                propdom.value = val;
            }
            else {
                propdom.value = val;
                let slidertext = document.getElementById(prop + Slidertext);
                slidertext.innerText = val;
            }

        }
        this.AjaxRender.style.display = "none";
    }
    this.CreatePlayerDom = function (player) {
        //hier das dom einmal initialisieren.
        let wrapper = document.getElementById("PlayerProps");
        for (var prop in player) {
            if (prop === "supportsOutputFixed") continue;

            let val = player[prop];
            let domname = document.createElement("DIV");
            domname.textContent = prop.toUpperCase();
            domname.classList.add("playerprop");
            let dom;
            if (typeof val == "boolean") {
                dom = this.GiveMeaCheckbox(prop);
            } else if (typeof val == "string") {
                dom = this.GiveMeaTextbox(prop);
            }
            else {
                dom = this.GiveMeaSilder(prop);
            }
            dom.classList.add("playerpropval");
            let containerdom = document.createElement("DIV");
            containerdom.classList.add("playerpropwrapper");
            containerdom.appendChild(domname);
            containerdom.appendChild(dom);
            wrapper.appendChild(containerdom);

        }
        PlayerDomCreated = "YesItIs";
    }
    this.GiveMeaSilder = function (id) {
        let sliwrapper = document.createElement("DIV");
        sliwrapper.classList.add("sliderWrapper");
        let slielement = document.createElement("input");
        slielement.classList.add("sliderInput");
        slielement.type = "range";
        slielement.min = "-10";
        slielement.max = "10";
        slielement.value = "0";
        slielement.id = id;
        slielement.addEventListener("change", function (s) {
            let id = s.target.id;
            let val = s.target.value;
            let slidertext = document.getElementById(id + Slidertext);
            slidertext.innerText = val;
            t.SendRequest(id, val);
        });
        slielement.addEventListener("input", function (s) {
            let id = s.target.id;
            let val = s.target.value;
            let slidertext = document.getElementById(id + Slidertext);
            slidertext.innerText = val;
        })
        sliwrapper.appendChild(slielement);
        let slitextvalue = document.createElement("DIV");
        slitextvalue.classList.add("sliderText");
        slitextvalue.textContent = "0";
        slitextvalue.id = id + Slidertext;
        sliwrapper.appendChild(slitextvalue);
        return sliwrapper;
    }
    this.GiveMeaCheckbox = function (id) {
        let divwrapper = document.createElement("DIV");
        divwrapper.classList.add("onoffswitch");
        let checkbox = document.createElement("input");
        checkbox.type = "Checkbox";
        checkbox.id = id;
        checkbox.classList.add("onoffswitch-checkbox");
        divwrapper.appendChild(checkbox);
        let label = document.createElement("label");
        label.classList.add("onoffswitch-label");
        label.htmlFor = id;
        let divinner = document.createElement("DIV");
        divinner.classList.add("onoffswitch-inner");
        let divswitch = document.createElement("DIV");
        divswitch.classList.add("onoffswitch-switch");
        label.appendChild(divinner);
        label.appendChild(divswitch);
        divwrapper.appendChild(label);

        //eventing
        checkbox.addEventListener("click", function (s) {
            t.SendRequest(s.target.id, s.target.checked)
        })

        return divwrapper;
    }
    this.GiveMeaTextbox = function (id) {
        let divwrapper = document.createElement("DIV");
        //divwrapper.classList.add("onoffswitch");
        let txtbox = document.createElement("input");
        txtbox.type = "text";
        txtbox.id = id;
        //checkbox.classList.add("onoffswitch-checkbox");
        divwrapper.appendChild(txtbox);
        let label = document.createElement("label");
        //label.classList.add("onoffswitch-label");
        label.htmlFor = id;
        let divinner = document.createElement("DIV");
        //divinner.classList.add("onoffswitch-inner");
        let divswitch = document.createElement("DIV");
        //divswitch.classList.add("onoffswitch-switch");
        label.appendChild(divinner);
        label.appendChild(divswitch);
        divwrapper.appendChild(label);

        //eventing
        txtbox.addEventListener("change", function (s) {
            t.SendRequest(s.target.id, s.target.value)
        })

        return divwrapper;
    }

    this.HidePlayerDom = function (val = true) {
        let playerdom = document.getElementById("PlayerProps");
        if (val === true) {
            if (playerdom.style.display !== "none");
            playerdom.style.display = "none";
        } else {
            if (playerdom.style.display !== "block");
            playerdom.style.display = "block";
        }
    }
    this.LoadSettings = function () {
        AjaxRenderSettings.style.display = "block";
        Send("/settings/GetSonosSettings/").then(function (data) {
            t.settings = data.properties;
            t.RenderSettings();
        }).catch(function (ex) {
            console.log(ex);
            alert(ex.statusText + " " + ex.responseText);
        });
    }
    this.CreateSettingsDom = function () {
        if (this.settings == null) {
            this.LoadSettings();
        }
        let settingwrapper = document.getElementById("Settingscontent");
        for (var propertyName in this.settings) {
            if (this.settingprops.indexOf(propertyName) == -1) continue;
            let propval = this.settings[propertyName];

            let domwrapper = document.createElement("DIV");
            domwrapper.classList.add("settingprop");
            domwrapper.id = propertyName;
            let domname = document.createElement("DIV");
            domname.textContent = propertyName.toUpperCase();
            domname.classList.add(propertyName);
            domname.classList.add("propertyName");
            domwrapper.appendChild(domname);
            let domvalue = document.createElement("DIV");
            domvalue.appendChild(this.CreateSettingsValueDom(propval));
            domvalue.id = propertyName + "_value";
            domvalue.classList.add("propertyValue");
            domwrapper.appendChild(domvalue);
            settingwrapper.appendChild(domwrapper);
        }
        SettingsDomCreated = "yes";
        AjaxRenderSettings.style.display = "none";
    }
    this.CreateSettingsValueDom = function (data, count) {
        if (typeof count === "undefined") {
            count = 1;
        }
        let datawrapper = document.createElement("DIV");
        datawrapper.classList.add("dataWrapper");
        for (var propertyName in data) {
            if (this.settingprops.indexOf(propertyName) == -1) continue;

            let valwrapper = document.createElement("DIV");
            valwrapper.classList.add("valueWrapper" + count);
            let val = data[propertyName];
            let dataname = document.createElement("DIV");
            dataname.classList.add("settingValueName" + count);
            dataname.innerText = propertyName.toUpperCase();
            valwrapper.appendChild(dataname);
            let dataval = document.createElement("DIV");
            if (typeof val == "string") {
                if (val === "") {
                    val = "NotSet";
                }
                dataval.innerText = val;
                dataval.classList.add("stringValueConten");
            } else if (typeof val === "number") {
                dataval.innerText = val;
                dataval.classList.add("numberValueConten");

            } else if (typeof val === "boolean") {
                dataval.innerText = val;
                dataval.classList.add("booleanValueConten");
            }
            else {
                dataval.appendChild(this.CreateSettingsValueDom(val, count + 1));
                dataval.classList.add("ObjectValueConten");
            }
            valwrapper.appendChild(dataval);
            datawrapper.appendChild(valwrapper);
        }
        return datawrapper;
    }
    this.RenderSettings = function () {
        if (this.settings == null) {
            this.LoadSettings();
            return;
        }
        if (SettingsDomCreated == NotRendered) {
            this.CreateSettingsDom();
            return;
        }
        let settingscontent = document.getElementById("Settingscontent");
        settingscontent.innerHTML = "";
        SettingsDomCreated = NotRendered;
        this.RenderSettings();
        return;
    }
    this.SendRequest = function (proper, value) {
        let selectedplayer = document.getElementById("PlayerSelection").value;
        let ppr = new PlayerDevicePropertiesTypes();

        ppr.value = value.toString();
        ppr.uuid = selectedplayer;
        ppr.type = Types[proper];
        Send("/devices/SetPlayerProperties", ppr, "POST").then(function () { console.log("Playerdaten erfolgreich Update"); }).catch(function (ex) {
            t.SetPlayer(ppr.uuid);
            alert(ex.statusText + " " + ex.responseText);
        });
    }
}