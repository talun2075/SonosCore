function InitTest() {
    let wrapper = document.getElementById("Currentplaylist");
    wrapper.innerHTML = "";
    wrapper.innerHTML += "clientwidth"+ document.body.clientWidth;
    wrapper.innerHTML += "<br>Offset width: " + document.body.offsetWidth;
    wrapper.innerHTML += "<br> clientheight" + document.body.clientHeight;
    wrapper.innerHTML += "<br>offset height" + document.body.offsetHeight;
    wrapper.innerHTML += "<br> window innerheigth " + window.innerHeight;
    wrapper.innerHTML += "<br>windows innerwidht " + window.innerWidth;
    wrapper.innerHTML += "<br>";
}