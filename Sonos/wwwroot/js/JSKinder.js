function ChildPlayer(name) {
    this.SonosItemList = [];
    this.Name = name;
    this.baseUrl = "";
    let t = this;
    this.PlayState = 0;
    this.AjaxLoader = document.getElementById("AjaxLoader");
    let adminModeActive = false;
    let adminFor = 0;
    let RemoveOld = true;
    this.Buttons;
    this.hashPic = "/hashimages";
    this.Init = function () {
        //Initieren und Daten holen.
        var getbaseurl = Send("GetBaseURL");
        
        getbaseurl.then(function (data) {
            t.baseUrl = data;
            document.getElementById("vol1").addEventListener("click", function () {
                t.SetVolume(1);
            });
            document.getElementById("vol2").addEventListener("click", function () {
                t.SetVolume(2);
            });
            document.getElementById("vol3").addEventListener("click", function () {
                t.SetVolume(3);
            });
            document.getElementById("vol4").addEventListener("click", function () {
                t.SetPause();
            });
            document.getElementById("SettingsButton").addEventListener("click", function () {
                var x = document.getElementById("Settings");
                if (x.style.display === "none" || x.style.display === "") {
                    x.style.display = "block";
                } else {
                    x.style.display = "none";
                    this.ActivateAdmin("Reset");
                }
            }); 
            const buttons = document.querySelectorAll('.button');
            for (var i = 0; i < buttons.length; i++) {
                buttons[i].addEventListener('click', function (s) {
                    t.ActivateAdmin("zzz" + t.Name + s.target.id, s.currentTarget);
                }, false);
            }
            document.getElementById("RemoveOld").addEventListener("change", function (s) {
                RemoveOld = s.target.checked;
            }); 
            t.GetStart();
            Send("GetTransport").then(function (data) {
                t.PlayState = data;
                t.RenderPlayState();
            });

        }).catch(function (jqXHR) {
            console.log("fail");
            console.log(jqXHR);
        });
        
        
    }
    this.GetStart = function () {
        this.AjaxLoader.style.display = "block";
        Send("GetStart").then(function (data) {
            t.SonosItemList = data;
            t.RenderStart();
        });
    }
    this.RenderPlayState = function () {
        var p = document.getElementById("vol4");
        if (this.PlayState === 0) {
            if (p.classList.contains("playing")) {
                p.classList.remove("playing");
            }
        } else {
            if (!p.classList.contains("playing")) {
                p.classList.add("playing");
            }
        }
        
    }
    this.SetPause = function () {
        Send("SetPause");
        this.PlayState = 0;
        this.RenderPlayState();
    }
    var inLoadinInterval;
    this.RenderStart = function () {
        //only js
        clearInterval(inLoadinInterval);
        let parentsroot = document.getElementById("Parents");
        for (var i = 0; i < this.SonosItemList.length; i++) {
            if (this.SonosItemList[i].artist === "inLoading") {
                //Neu Laden in x Sekunden
                inLoadinInterval = window.setInterval(Child.GetStart, 10000);
                return;
            }

            var image = this.SonosItemList[i].childs[0].albumArtURI;
            if (!image.startsWith(this.hashPic)) {
                var image = "http://" + this.baseUrl + this.SonosItemList[i].childs[0].albumArtURI;
            }
            let imagedom = document.createElement("img");
            imagedom.setAttribute("src", image);
            imagedom.dataset.url = image;
            imagedom.classList.add("parentImage");

            let parentdom = document.createElement("div");
            parentdom.setAttribute("id", "Parent_" + i);
            parentdom.classList.add("parentItem");
            parentdom.setAttribute("onClick", "Child.RenderClickParent(" + i + ")");
            parentdom.append(imagedom);
            parentsroot.append(parentdom);
        }
        this.RenderClickParent(0);
    }
    this.SendClickChild = function (parent, child) {
        var uri = this.SonosItemList[parent].childs[child].containerID;
        console.log(parent);
        console.log(child);
        if (adminModeActive === true) {
            if (RemoveOld) {
                Send("SetButton/" + adminFor + "/true", uri, "POST").catch(function (jqXHR) {
                    console.log("SetButton fail for true");
                    console.log(jqXHR);
                });
               
            } else {
                Send("SetButton/" + adminFor + "/false", uri, "POST").catch(function (jqXHR) {
                    console.log("SetButton fail for false");
                    console.log(jqXHR);
                });
            }
        } else {
            Send("ReplacePlaylist", uri, "POST").then(function () {
                t.PlayState = 1;
                t.RenderPlayState();
            }).catch(function (jqXHR) {
                console.log("ReplacePlaylist fail");
                console.log(jqXHR);
            });
        }
        
    }
    this.RenderClickParent = function (parent) {
        let childwrapper = document.getElementById("Childwrapper");
        childwrapper.innerHTML = ''
        for (var i = 0; i < this.SonosItemList[parent].childs.length; i++) {
            let image = this.SonosItemList[parent].childs[i].albumArtURI;
            if (!image.startsWith(this.hashPic)) {
                image = "http://" + this.baseUrl + this.SonosItemList[parent].childs[i].albumArtURI;
            }
            let title = this.SonosItemList[parent].childs[i].title;
            //let hours = this.SonosItemList[parent].childs[i].duration.hours;
            //let minutes = this.SonosItemList[parent].childs[i].duration.minutes
            //let seconds = this.SonosItemList[parent].childs[i].duration.seconds;
            //let secondsstring = seconds;
            //let minutesstring = minutes;
            //let hoursstring = "";
            //if (seconds < 10) {
            //    secondsstring = "0" + seconds;
            //}
            //if (hours > 0) {
            //    hoursstring = hours+":";
            //}
            //if (hours > 0 && minutes < 10) {
            //    minutesstring = "0" + minutes;
            //}
            //let duration = hoursstring+minutesstring+ ":" + secondsstring;
            let imagedom = document.createElement("img");
            imagedom.setAttribute("src", image);
            imagedom.classList.add("childImage");
            let childdom = document.createElement("div");
            childdom.setAttribute("id", "Childt_" + i);
            childdom.classList.add("childItem");
            childdom.setAttribute("onClick", "Child.SendClickChild(" + parent + ',' + i + ")");
            let childcounter = document.createElement("DIV");
            childcounter.classList.add("childCounter");
            childcounter.innerHTML = i;
            let childtitle = document.createElement("DIV");
            childtitle.classList.add("childTitle");
            childtitle.innerHTML = title;
            let childduration = document.createElement("DIV");
            childduration.classList.add("childDuration");
            if (!this.SonosItemList[parent].childs[i].duration.isZero)
                childduration.innerHTML = this.SonosItemList[parent].childs[i].duration.stringWithoutZeroHours +" min";
            childdom.append(childcounter);
            childdom.append(childtitle);
            childdom.append(imagedom);
            childdom.append(childduration);
            childwrapper.append(childdom);
        }
        console.log("blub");
        this.AjaxLoader.style.display = "none";
    }
    this.SetVolume = function (v) {
        Send("SetVolume/"+v);
    }
    this.ActivateAdmin = function (v,sdom) {
        if (adminFor === v) {
            adminModeActive = false;
            adminFor = 0;
            if (sdom.classList.contains("active")) {
                sdom.classList.remove("active");
            }
            return;
        }
        if (adminFor !== 0 || v === "Reset") {
            //ein wechsel innerhalb der buttons, daher alle entfernen.
            var buttons = document.querySelectorAll(".button");
            buttons.forEach(function (button) {
                if (button.classList.contains("active")) {
                    button.classList.remove("active");
                }
            });
        }
        if (v === "Reset") {
            return;
        }
        adminFor = v;
        sdom.classList.add("active");
        if (adminModeActive !== true) {
            adminModeActive = true
        }

    }
}