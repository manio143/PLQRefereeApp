function parse_cookies() {
    var cookies = {};

    if (document.cookie && document.cookie !== '') {
        document.cookie.split(';').forEach(function (c) {
            var m = c.trim().match(/(\w+)=(.*)/);
            if (m !== undefined) {
                cookies[m[1]] = decodeURIComponent(m[2]);
            }
        });
    }
    return cookies;
}

function send(method, uri, content, onSuccess, onError, setUp) {
    var req = new XMLHttpRequest();
    req.open(method, uri, true);

    if (setUp !== undefined)
        setUp(req);
    req.onreadystatechange = function () {
        if (req.readyState !== 4) return;
        if (req.status >= 200 && req.status < 300) {
            //alert(req.responseText);
            onSuccess(req.status, req.responseText);
        }
        else {
            onError(req.status, req.responseText);
        }
    };
    if (method === "POST") {
        req.send(content);
    }
    else {
        req.send();
    }
    return req;
}

function parseform(form) {
    var formData = new FormData(form);
    var obj = {};
    for (var pair of formData.entries())
        obj[pair[0]] = pair[1];
    return JSON.stringify(obj);
}

function required() {
    let valid = true;
    for (chk of document.getElementsByTagName("input")) {
        if (chk.type == "radio") {
            let checked = false;
            for (chk2 of document.getElementsByName(chk.name)) {
                checked = checked || chk2.checked;
            }
            if (!checked) {
                valid = false;
                break;
            }
        }
    }
    if (!valid) {
        document.getElementById("submit-btn").setAttribute("disabled", "disabled");
    } else {
        document.getElementById("submit-btn").removeAttribute("disabled");
    }
}

function getTimeRemaining(endtime) {
    var t = Date.parse(endtime) - Date.parse(new Date());
    var seconds = Math.floor((t / 1000) % 60);
    var minutes = Math.floor((t / 1000 / 60) % 60);
    var hours = Math.floor((t / (1000 * 60 * 60)) % 24);
    var days = Math.floor(t / (1000 * 60 * 60 * 24));
    return {
        'total': t,
        'days': days,
        'hours': hours,
        'minutes': minutes,
        'seconds': seconds
    };
}

function initializeClock(time) {
    let endtime = new Date(Date.parse(new Date()) + time * 60 * 1000);
    var clockLabel = document.getElementById("clock-label");
    var clockBar = document.getElementById("clock-bar");

    function updateClock() {
        var t = getTimeRemaining(endtime);

        clockLabel.innerHTML = ('0' + t.minutes).slice(-2) + ":" + ('0' + t.seconds).slice(-2);
        let percent = Math.round((time * 60 - t.minutes * 60 - t.seconds) / (time * 60) * 100)
        clockBar.setAttribute("style", "width: " + percent + "%;");
        //console.log(percent + '%');

        if (t.total <= 0) {
            clearInterval(timeinterval);
            for (chk of document.getElementsByTagName("input")) {
                if (chk.type == "radio") {
                    chk.setAttribute("disabled", "disabled");
                }
            }
            document.getElementById("submit-btn").removeAttribute("disabled");
            document.getElementById("submit-btn").click();
        }
    }

    updateClock();
    var timeinterval = setInterval(updateClock, 1000);
}

let genericErrorHandler = (status, request) => { alert("ERROR " + status + "\n" + request); }
let withCSRF = (request) => { request.setRequestHeader("X-CSRF-Token", parse_cookies().CSRFToken); }

window.onload = function () {
    var startTestBtn = document.getElementById("start-test-btn");
    startTestBtn.onclick = () => {
        send("POST", "/test-start", "", (_, response) => {
            console.log(response);
            let testData = JSON.parse(response);
            let intro = document.getElementById("intro");
            let root = intro.parentNode;
            root.removeChild(intro);
            let wrapper = document.createElement("div");
            wrapper.id = "test";
            let clockLabel = document.createElement("p");
            clockLabel.className = "clock";
            clockLabel.id = "clock-label";
            wrapper.appendChild(clockLabel);
            let clockBarWrapper = document.createElement("div");
            clockBarWrapper.className = "clock-wrapper";
            let clockBar = document.createElement("div");
            clockBar.className = "clock";
            clockBar.id = "clock-bar";
            clockBarWrapper.appendChild(clockBar);
            wrapper.appendChild(clockBarWrapper);
            let counter = 1;
            for (q of testData.questions) {
                let qwrapper = document.createElement("div");
                let header = document.createElement("h4");
                header.innerHTML = "Pytanie " + (counter++);
                let question = document.createElement("p");
                question.innerHTML = q["Question@"];
                question.className = "question"
                let awrapper = document.createElement("div");
                awrapper.className = "answer-wrapper";
                for (a of q["Answers@"]) {
                    let isolator = document.createElement("div");
                    let id = q["Id@"] + ":" + a["Id@"];
                    let radio = document.createElement("input");
                    radio.type = "radio";
                    radio.id = id;
                    radio.value = a["Id@"];
                    radio.name = q["Id@"];
                    radio.onchange = () => {
                        send("POST", "/test-answer", JSON.stringify({ question: q["Id@"], answer: a["Id@"] }), () => { }, genericErrorHandler, withCSRF);
                        required();
                    };
                    let label = document.createElement("label");
                    label.for = id;
                    label.innerHTML = a["Answer@"];
                    isolator.appendChild(radio);
                    isolator.appendChild(label);
                    awrapper.appendChild(isolator);
                }
                qwrapper.appendChild(header);
                qwrapper.appendChild(question);
                qwrapper.appendChild(awrapper);
                wrapper.appendChild(qwrapper);
            }
            let submit = document.createElement("button");
            submit.type = "submit";
            submit.innerText = "Zakończ test";
            submit.id = "submit-btn";
            submit.onclick = () => { confirm("Jesteś pewny?"); }
            wrapper.appendChild(submit);
            root.appendChild(wrapper);
            required();
            initializeClock(testData.time);
        }, genericErrorHandler, withCSRF);
    };
}