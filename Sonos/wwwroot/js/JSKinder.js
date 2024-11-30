function ChildPlayer(name) {
    window.BasePath = "/"+name + "Data/";
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
            document.getElementById("LocalStorage").addEventListener("click", function () {
                removeStore("Child");
                this.classList.remove("active");
            });
            document.getElementById("vol2").addEventListener("click", function () {
                t.SetVolume(2);
            });
            document.getElementById("vol3").addEventListener("click", function () {
                t.SetVolume(3);
            });
            document.getElementById("vol4").addEventListener("click", function () {
                t.SetTogglePlayPause();
            });
            document.getElementById("SettingsButton").addEventListener("click", function () {
                var x = document.getElementById("Settings");
                if (x.style.display === "none" || x.style.display === "") {
                    x.style.display = "block";
                } else {
                    x.style.display = "none";
                    t.ActivateAdmin("Reset");
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
            console.log("fail to get baseurl");
            console.log(jqXHR);
        });
        
        
    }
    this.GetStart = function () {
        this.AjaxLoader.style.display = "block";
        var localstore = getStore("Child");
        if (localstore === null || isUpdateRequired(localstore.lastUpdated)) {
            Send("GetStart").then(function (data) {
                t.SonosItemList = data;
                setStore("Child", data);
                t.RenderStart();
            });
        } else {
            document.getElementById("LocalStorage").classList.add("active");
            this.SonosItemList = localstore.value;
            this.RenderStart();
        }
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
    this.SetPlay = function () {
        Send("SetPlay");
        this.PlayState = 1;
        this.RenderPlayState();
    }
    this.SetTogglePlayPause = function () {
        if (this.PlayState === 0) {
            this.SetPlay();
        } else {
            this.SetPause();
        }
        
    }
    var inLoadinInterval;
    this.RenderStart = function () {
        //only js
        clearInterval(inLoadinInterval);
        let parentsroot = document.getElementById("Parents");
        let container1 = document.createElement("div");
        container1.classList.add("parentContainer");
        let container2 = document.createElement("div");
        container2.classList.add("parentContainer");
        let firstcontainer = this.SonosItemList[0].source;
        for (var i = 0; i < this.SonosItemList.length; i++) {
            if (this.SonosItemList[i].artist === "inLoading") {
                //Neu Laden in x Sekunden
                inLoadinInterval = window.setInterval(Child.GetStart, 10000);
                return;
            }
            if (this.SonosItemList[i].childs.length == 0) {
                continue;
            }
            var image = this.SonosItemList[i].childs[0].albumArtURI;
            var parentelement = this.CreateParentElement(image, i);
            if (this.SonosItemList[i].source === firstcontainer) {
                container1.append(parentelement);
            } else {
                container2.append(parentelement);
            }
            //parentsroot.append(parentelement);
        }
        parentsroot.append(container1);
        parentsroot.append(container2);
        this.RenderClickParent(0,0);
    }
    this.CreateParentElement = function (image, counter) {
        if (!image.startsWith(this.hashPic)) {
            var image = "http://" + this.baseUrl + this.SonosItemList[counter].childs[0].albumArtURI;
        }
        let imagedom = document.createElement("img");
        imagedom.setAttribute("src", image);
        imagedom.dataset.url = image;
        imagedom.classList.add("parentImage");

        let parentdom = document.createElement("div");
        parentdom.setAttribute("id", "Parent_" + counter);
        parentdom.classList.add("parentItem");
        parentdom.setAttribute("onClick", "Child.RenderClickParent(" + counter + ",1)");
        parentdom.append(imagedom);

        return parentdom;

    }
    this.SendClickChild = function (parent, child) {
        var uri = this.SonosItemList[parent].childs[child].containerID;
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
    this.RenderClickParent = function (parent, jump) {
        let childwrapper = document.getElementById("Childwrapper");
        childwrapper.innerHTML = ''
        for (var i = 0; i < this.SonosItemList[parent].childs.length; i++) {
            let image = this.SonosItemList[parent].childs[i].albumArtURI;
            if (!image.startsWith(this.hashPic)) {
                image = "http://" + this.baseUrl + this.SonosItemList[parent].childs[i].albumArtURI;
            }
            let title = this.SonosItemList[parent].childs[i].title;
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
        this.AjaxLoader.style.display = "none";
        if(jump ===1)
            window.scrollTo(0, childwrapper.offsetTop);
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