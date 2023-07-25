// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

//GLOBAL VARIABLES
let transcribe = '';

//CUSTOM FUNCTIONS
function goBack() {
    window.history.back();
}

$(function () {
    //used to call the init() function after a successful load from the HTML.
    init();
});

function init() {
    console.log("INIT IS HERE")

    // Initialize the 'result' variable to store the captured speech

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
            result.innerText = speech.text;
            if (audio.isFinal) {
                //delayedMessage();
                transcribe += speech.text;
                console.log("TRANSCRIBE: \n" + transcribe + "\n");
                populateAssessmentForms(transcribe);
                //resetTranscribeAfterDelay();
            }
        });

        toggle.addEventListener('click', () => {
            speech.listening = !speech.listening;
            if (speech.listening) {
                //toggle.classList.add('listening');
                //toggle.innerText = 'Listening ...';
                speech.recognition.start();
            }
            else {
                //toggle.classList.remove('listening');
                //toggle.innerText = 'Toggle listening';
                speech.recognition.stop();
                transcribe = '';
            }
        })
    }

    async function populateAssessmentForms(result) {
        console.log("populateAssessmentForms is here!")
        var el;
        var question = "";
        var answer = result;
        var getId = "";

        if (typeof result !== "string") {
            throw new Error("valueResult must be a string.");
        }

        //does the checking
        switch (true) {
            case result.includes("full name"):
                question = "What is the patient's full name?";
                getId = "inputPatientName";
                el = document.getElementById(getId);
                el.id == getId ? el.value = await getPrompt(question, answer) : console.log('DOM.el is NULL');
                transcribe = '';
                break;
            case result.includes("health care worker") || result.includes("healthcare worker"):
                question = "What is the health care worker's full name?";
                getId = "inputDoctorName";
                el = document.getElementById(getId);
                el.id == getId ? el.value = await getPrompt(question, answer) : console.log('DOM.el is NULL');
                transcribe = '';
                break;
            case (result.includes("medical conditions")) && (result.includes("this is noted")):
                question = "What are the patient's medical conditions? Write it in a bullet form.";
                getId = "inputMedicalConditions";
                el = document.getElementById(getId);
                el.id == getId ? el.value = await getPrompt(question, answer) : console.log('DOM.el is NULL');
                transcribe = '';
                break;
            default:
                console.log('end of swtich');
                break;
        }
        return null;
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