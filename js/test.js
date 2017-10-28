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

function disableAnswering() {
    for (chk of document.getElementsByTagName("input")) {
        if (chk.type == "radio") {
            chk.setAttribute("disabled", "disabled");
        }
    }
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
            disableClock();
            disableAnswering();
            let submit = document.getElementById("submit-btn");
            submit.setAttribute("disabled", "disabled");
            if(submit.mDone == undefined) {
                submit.mDone = true;
                submit.onclick();
            }
        }
    }

    updateClock();
    window.timeinterval = setInterval(updateClock, 1000);
}

function disableClock() {
    clearInterval(window.timeinterval);
    document.getElementById("clock-label").remove();
    document.getElementById("clock-bar").remove();
}

let genericErrorHandler = (status, request) => { alert("ERROR " + status + "\n" + request); }
let withCSRF = (request) => { request.setRequestHeader("X-CSRF-Token", parse_cookies().CSRFToken); }

let nextChar = (char) => {
    if(char == "A") return "B";
    else if(char == "B") return "C";
    else return "D";
}

let createElement = (type, id, className) => {
    let element = document.createElement(type);
    if(id != undefined)
        element.id = id;
    if(className != undefined)
        element.className = className;
    return element
}

window.onload = function () {
    var startTestBtn = document.getElementById("start-test-btn");
    startTestBtn.onclick = () => {
        send("POST", "/test-start", "", (_, response) => {
            let testData = JSON.parse(response);
            let intro = document.getElementById("intro");
            let root = intro.parentNode;
            root.removeChild(intro);
            generateView(testData, root, true);
            required();
            initializeClock(testData.time.minutes);
        }, genericErrorHandler, withCSRF);
    };
}

function generateView(testData, root, submitAndClock) {
    let wrapper = createElement("div", "test");
    
    if(submitAndClock) {
        let clockLabel = createElement("p", "clock-label", "clock");
        wrapper.appendChild(clockLabel);
        let clockBarWrapper = createElement("div", undefined, "clock-wrapper");
        let clockBar = createElement("div", "clock-bar", "clock");
        clockBarWrapper.appendChild(clockBar);
        wrapper.appendChild(clockBarWrapper);
    } else {
        let finishLabel = createElement("p", "finish-label");
        let numberOfQuestions = testData.mark["Count@"];
        let numberOfCorrect = testData.mark["CorrectlyAnswered@"]
        finishLabel.innerText = "Zakończyłeś test! Odpowiedziałeś poprawnie na " + numberOfCorrect + "/" + numberOfQuestions +
                                " pytań. Jest to " + (numberOfCorrect/numberOfQuestions >= 0.8 ? "ponad 80%, czyli zaliczyłeś ten test. Gratulacje!" : "poniżej 80%, czyli nie udało ci się zaliczyć testu.");
        let anotherLabel = createElement("p");
        anotherLabel.innerText = "Poniżej znajdują się pytania, na które nie odpowiedziałeś poprawnie."
        let buttonFinish = createElement("button");
        buttonFinish.innerText = "Powrót do portalu";
        buttonFinish.onclick = () => { window.location = "/account/details"; }
        wrapper.appendChild(finishLabel);
        wrapper.appendChild(anotherLabel);
        wrapper.appendChild(buttonFinish);
    }
    
    let counter = 1;
    for (q of testData.questions) {
        let qwrapper = createElement("div");
        let header = createElement("h4");
        header.innerHTML = "Pytanie " + (counter++);
        let question = createElement("p", undefined, "question");
        question.innerHTML = q["Question@"];
        let awrapper = createElement("div", undefined, "answer-wrapper");
        let char = 'A';
        let isCorrected = false;
        for (a of q["Answers@"]) {
            let isolator = document.createElement("div");
            let id = q["Id@"] + ":" + a["Id@"];
            if(submitAndClock) {
                let radio = createElement("input", id);
                radio.type = "radio";
                radio.value = a["Id@"];
                radio.name = q["Id@"];
                radio.onchange = () => {
                    send("POST", "/test-answer", "q:"+radio.name+";a:"+radio.value+";", () => { }, genericErrorHandler, withCSRF);
                    required();
                };
                isolator.appendChild(radio);
            }
            let label = document.createElement("label");
            label.for = id;
            label.innerHTML = (a["Correct@"] ? "<span class=\"red bold\">":"") + char + ". " +
                            (a["Correct@"] ? "</span>":"") + a["Answer@"];
            if(submitAndClock) label.onclick = () => document.getElementById(id).click();
            char = nextChar(char);
            if(a["Correct@"])
                isCorrected = true;
            isolator.appendChild(label);
            awrapper.appendChild(isolator);
        }
        qwrapper.appendChild(header);
        qwrapper.appendChild(question);
        qwrapper.appendChild(awrapper);
        if(isCorrected) {
            let info = createElement("p", undefined, "question-info");
            info.innerHTML = q["Information@"];
            qwrapper.appendChild(info);
        }
        wrapper.appendChild(qwrapper);
    }
    if(submitAndClock) {
        let submit = createElement("button", "submit-btn");
        submit.type = "submit";
        submit.innerText = "Zakończ test";
        submit.onclick = () => { 
            let go = true;
            if(submit.mDone == undefined)
                go = confirm("Jesteś pewny?");
            if(go) {
                submit.mDone = true;
                disableAnswering();
                disableClock();
                submit.setAttribute("disabled", "disabled");
                send("POST", "/test-finish", "", (status, response) => {
                    let testData = JSON.parse(response);
                    wrapper.remove();
                    generateView(testData, root, false);
                    document.scrollingElement.scrollTop = 0;
                }, genericErrorHandler, withCSRF);
            }
        }
        wrapper.appendChild(submit);
    }
    root.appendChild(wrapper);
}