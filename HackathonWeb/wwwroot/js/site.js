// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

//GLOBAL VARIABLES
let transcribe = '';

//CUSTOM FUNCTIONS
function goBack() {
    window.history.back();
}

// Function to show the button

$(document).ready(function () {
    init();
});

function init() {
    console.log("INIT IS HERE")

    // Initialize the 'result' variable to store the captured speech
    const result = document.getElementById('result'); // Assuming you have an HTML element with the ID 'result'
    const toggle = document.getElementById('toggle'); // Assuming you have an HTML element with the ID 'toggle'
    const btnAssessment = document.getElementById('btnCompleteAssessment'); // Assuming you have an HTML element with the ID 'toggle'

    btnAssessment.addEventListener('click', () => {
        completeAssessment();
    })

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
        try {
            speech.recognition.addEventListener('result', (event) => {
                const audio = event.results[event.results.length - 1];
                speech.text = audio[0].transcript;
                result.innerText = speech.text;
                if (audio.isFinal) {
                    //delayedMessage();
                    transcribe = transcribe + " " + speech.text;
                    transcribe = transcribe.toLowerCase();
                    console.log("TRANSCRIBE: \n" + transcribe + "\n");
                    fromWebkitSpeechRecognition(transcribe);
                    //resetTranscribeAfterDelay();
                }
            });
        } catch (e) {
            console.log(e);
            throw e;
        }

        toggle.addEventListener('click', () => {
            speech.listening = !speech.listening;
            if (speech.listening) {
                transcribe = '';
                //document.getElementById('output').innerText = 'Listening...';
                speech.recognition.start();
            }
            else {
                transcribe = '';
                //document.getElementById('output').innerText = 'Listening has ended.';
                speech.recognition.stop();
            }
        })
    }
}

//function to populate the assessment using webkitSpeechRecnognition.
async function fromWebkitSpeechRecognition(result) {
    console.log("fromWebkitSpeechRecognition is here!")
    var el;
    var question = "";
    var answer = result;
    var getId = "";

    if (typeof result !== "string") {
        throw new Error("valueResult must be a string.");
    }

    //var tempString = "Have you experienced any medical conditions today? Yeah, I've got hypertension and arthritis. I take amlodipine or amlodipine. I'm quite unsure. I'm a Lady Pin. That's right. That medication for my blood pressure and ibuprofen when my joints act up. And I also had a nephritis when I was young. Sporadic migraines pop up sometimes too, but I manage them with rest and Sumatriptan when needed. This is noted.";

    //if (tempString.includes('medical conditions') && tempString.includes('this is noted')) {
    //    console.log("Both 'medical conditions' and 'this is noted' are present in the string.");
    //};

    //does the checking
    switch (true) {
        case result.includes('full name') && result.includes('patient') && result.includes('noted'):
            question = "What is the patient's full name?";
            getId = "inputPatientName";
            el = document.getElementById(getId);
            if (el.id == getId) {
                el.value = await getAzureQnA(question);
                transcribe = '';
            }
            break;
        case result.includes('health coordinator'):
            question = "What is the Health Coordinator's full name?";
            getId = "inputDoctorName";
            el = document.getElementById(getId);
            if (el.id == getId) {
                el.value = await getAzureQnA(question);
                transcribe = '';
            }
            break;
        case result.includes('allergies') && result.includes('noted'):
            question = "Do you have allergies?"
            getId = "inputAllergies";
            el = document.getElementById(getId);
            if (el.id == getId) {
                el.value = await getAzureQnA(question);
                transcribe = '';
            }
            break;
        case (result.includes('medications') || result.includes('medication')) && result.includes('noted'):
            question = "What are her medications?"
            getId = "inputMedications";
            el = document.getElementById(getId);
            if (el.id == getId) {
                el.value = await getAzureQnA(question);
                transcribe = '';
            }
            break;
        case result.includes('vaccine') && result.includes('noted'):
            question = "Describe the patient's vaccine history in detail. If possible, write it in bullet form."
            getId = "inputVaccine";
            el = document.getElementById(getId);
            if (el.id == getId) {
                el.value = await getPrompt(question, answer);
                transcribe = '';
            }
            break;
        case (result.includes('medical conditions') || result.includes('medical condition')) && result.includes('noted'):
            question = "Describe the patient's medical history in detail. If possible, write it in bullet form."
            getId = "inputMH";
            el = document.getElementById(getId);
            if (el.id == getId) {
                el.value = await getPrompt(question, answer)
                transcribe = '';
            }
            break;
        default:
            console.log('end of swtich');
            break;
    }
}

//function to complete the assessment
async function completeAssessment() {
    var el;
    var question = "";
    var answer = result;
    var getId = "";

    //testing123

    switch (true) {
        case true:
            question = "Summarize the whole conversation between the health coordinator and patient. If possible, write the key points.";
            getId = "inputSummary";
            el = document.getElementById(getId);
            if (el.id == getId) {
                el.value = await getPromptFromTranscription(question)
                transcribe = '';
                break;
            };
        default:
            console.log('end of swtich');
            break;
    }
}

//function to call OpenAI based on question and answer.
async function getPrompt(question, answer) {
    console.log("I am in getPrompt");

    if (typeof question !== "string" || typeof answer !== "string") {
        throw new Error("value must be a string.");
    }
    try {
        const response = await fetch(`https://localhost:7290/api/OpenAI/GetPrompt?Question=${encodeURIComponent(question)}&Answer=${encodeURIComponent(answer)}`);

        if (response.ok) {
            const data = await response.text(); // Parse the response as text
            console.log("Response received: ", data);
            return data; // Return the parsed data
        } else {
            // Handle the error here
           // document.getElementById('output').innerText = 'Error occurred during the API call.';
        }
    } catch (error) {
        // Handle any network or other errors here
        //document.getElementById('output').innerText = 'An error occurred: ' + error.message;
    }
}

//function to call OpenAI based on question and sample transcript
async function getPromptFromTranscription(question) {
    if (typeof question !== "string") {
        throw new Error("valueResult must be a string.");
    }
    console.log("COMPLETING THE ASSESSMENT NOW");
    try {
        const completeAssessmentResponse = await fetch(`https://localhost:7290/api/OpenAI/GetPromptFromTranscript?Question=${encodeURIComponent(question)}`);

        if (completeAssessmentResponse.ok) {
            const data = await completeAssessmentResponse.text(); // Parse the response as text
            console.log("Response received: ", data);
            return data; // Return the parsed data
        } else {
            // Handle the error here
            //document.getElementById('output').innerText = 'Error occurred during the API call.';
        }
    } catch (error) {
        // Handle any network or other errors here
       // document.getElementById('output').innerText = 'An error occurred: ' + error.message;
    }
}

//function to getAzureQnA
async function getAzureQnA(question) {
    console.log("I am in get AzureQnA");
    try {
        const response = await fetch(`https://localhost:7290/api/QuestionAnswer/GetAzureQnA?QuestionUI=${encodeURIComponent(question)}`);

        if (response.ok) {
            const data = await response.text(); // Parse the response as text
            console.log("Response received from AzureQnA: ", data);
            return data; // Return the parsed data
        } else {
            // Handle the error here
            //document.getElementById('output').innerText = 'Error occurred during the API call.';
        }
    } catch (error) {
        // Handle any network or other errors here
       // document.getElementById('output').innerText = 'An error occurred: ' + error.message;
    }
}

//function to call Azure microphone
async function getFromMicrophone() {
    try {
      
        const response = await fetch(`https://localhost:7290/api/QuestionAnswer?QuestionUI=${encodeURIComponent(question)}`);

        if (response.ok) {
            const data = await response.text(); // Parse the response as text
            console.log("Response received: ", data);
            return data; // Return the parsed data
        } else {
            // Handle the error here
            //document.getElementById('output').innerText = 'Error occurred during the API call.';
        }
    } catch (error) {
        // Handle any network or other errors here
        //document.getElementById('output').innerText = 'An error occurred: ' + error.message;
    }
}