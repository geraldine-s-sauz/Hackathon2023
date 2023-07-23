// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function goBack() {
    window.history.back();
}

const myElement = document.getElementById("result");
myElement.style.fontSize = "20px";

$(document).ready(function () {
    init();
})

function init() {
    console.log("INIT IS HERE")
    const result = document.getElementById('result'); // Assuming you have an HTML element with the ID 'result'
    const toggle = document.getElementById('toggle'); // Assuming you have an HTML element with the ID 'toggle'

    window.SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;
    if (('SpeechRecognition' in window || 'webkitSpeechRecognition' in window)) {
        let speech = {
            enabled: true,
            listening: false,
            recognition: new window.SpeechRecognition(),
            text: ''
        }
        speech.recognition.continuous = true;
        speech.recognition.interimResults = true;
        speech.recognition.lang = 'en-US';
        speech.recognition.addEventListener('result', (event) => {
            const audio = event.results[event.results.length - 1];
            speech.text = audio[0].transcript;
            const tag = document.activeElement.nodeName;
            console.log("TAG: " + tag);
            if (tag === 'INPUT' || tag === 'TEXTAREA') {
                if (audio.isFinal) {
                    document.activeElement.value += speech.text;
                }
            }
            result.innerText = speech.text;
            if (audio.isFinal)
                populateAssessmentForms(speech.text);
        });

        toggle.addEventListener('click', () => {
            speech.listening = !speech.listening;
            if (speech.listening) {
                toggle.classList.add('listening');
                toggle.innerText = 'Listening ...';
                speech.recognition.start();
            }
            else {
                toggle.classList.remove('listening');
                toggle.innerText = 'Toggle listening';
                speech.recognition.stop();
            }
        })
    }

    async function populateAssessmentForms(result) {
        console.log("populateAssessmentForms is here!")
        var el;
        var question = "";
        var answer = result;

        if (typeof result !== "string") {
            throw new Error("valueResult must be a string.");
        }

        switch (true) {
            case result.includes("full name"):
                question = "What is the patient's full name?";
                el = document.getElementById("firstNameInput");
                if (el.id == "firstNameInput") {
                    el.value = await getPrompt(question, answer);
                }
                else {
                    console.log('DOM.el is NULL')
                }
                break;
            default:
                console.log('Invalid grade.');
        }
        return null;

        //if (result.innerText.includes("first name")) {
        //    var el = document.getElementById('firstNameInput');
        //    el.value = speech.text;
        //    //el.getElementsByTagName('input').value = speech.text;
        //    //setFirstNameValue(speech.text)
        //    //el.getElementsByTagName('input').value += speech.text;
        //    //console.log("NAME: " + speech.text)
        //}
        //else if (result.innerText.includes("age")) {
        //    var el = document.getElementById('Age');
        //    el.getElementsByTagName('input')[0].value += speech.text;
        //}
        //else if (result.innerText.includes("sex")) {
        //    var el = document.getElementById('Sex');
        //    el.getElementsByTagName('input')[0].value = speech.text;
        //}
        //else if (result.innerText.includes("address")) {
        //    var el = document.getElementById('Address');
        //    el.getElementsByTagName('textarea')[0].value = speech.text;
        //}
    }

    async function getPrompt(question, answer) {
        console.log("I am in getPrompt");
        try {
            const response = await fetch(`https://localhost:44337/api/OpenAI?Question=${encodeURIComponent(question)}&Answer=${encodeURIComponent(answer)}`);

            if (response.ok) {
                const data = await response.text(); // Parse the response as text
                console.log("Response received: ", data);
                return data; // Return the parsed data
            } else {
                // Handle the error here
                document.getElementById('output').innerText = 'Error occurred during the API call.';
            }
        } catch (error) {
            // Handle any network or other errors here
            document.getElementById('output').innerText = 'An error occurred: ' + error.message;
        }
    }
}

