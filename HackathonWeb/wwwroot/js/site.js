// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

//GLOBAL VARIABLES
let transcribe = '';

//CUSTOM FUNCTIONS
function goBack() {
    window.history.back();
}

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

    //function when user clicks 'Complete Assessment', it will complete the assessment
    async function completeAssessment() {
        var el;
        var question = "";
        var answer = result;
        var getId = "";

        //testing123

        switch (true) {
            //case false:
            //    question = "What is the patient's name?";
            //    getId = "inputPatientName";
            //    el = document.getElementById(getId);
            //    if ((el.id == getId) && (el.value == ''))
            //    {
            //        el.value = await getPromptFromTranscription(question)
            //        transcribe = '';
            //        break;
            //    };
            //case false:
            //    question = "What is the healthcare worker's name?";
            //    getId = "inputDoctorName";
            //    el = document.getElementById(getId);
            //    if ((el.id == getId) && (el.value == '')) {
            //        el.value = await getPromptFromTranscription(question)
            //        transcribe = '';
            //        break;
            //    };
            case true:
                question = "Briefy explain the patient's allergies, if any. If possible, write it in a bullet form.";
                getId = "inputAllergies";
                el = document.getElementById(getId);
                if ((el.id == getId) && (el.value == '')) {
                    el.value = await getPromptFromTranscription(question)
                    transcribe = '';
                    break;
                };
            //case false:
            //    question = "Briefy explain the medical conditions of the patient, if any. If possible, write it in a bullet form.";
            //    getId = "inputMedicalCondition";
            //    el = document.getElementById(getId);
            //    if ((el.id == getId) && (el.value == '')) {
            //        el.value = await getPromptFromTranscription(question)
            //        transcribe = '';
            //        break;
            //    };
            case true:
                question = "Briefy explain the vaccine history of the patient. If possible, write it in a bullet form.";
                getId = "inputVH";
                el = document.getElementById(getId);
                if ((el.id == getId) && (el.value == '')) {
                    el.value = await getPromptFromTranscription(question)
                    transcribe = '';
                    break;
                };
            case true:
                question = "Briefy explain the medical history of the patient. If possible, write it in a bullet form.";
                getId = "inputMH";
                el = document.getElementById(getId);
                if ((el.id == getId) && (el.value == '')) {
                    el.value = await getPromptFromTranscription(question)
                    transcribe = '';
                    break;
                };
            case true:
                question = "Briefly summarize the whole conversation between the healthcare worker and patient. If possible, write the key points.";
                getId = "inputSummary";
                el = document.getElementById(getId);
                if ((el.id == getId) && (el.value == '')) {
                    el.value = await getPromptFromTranscription(question)
                    transcribe = '';
                    break;
                };
            default:
                console.log('end of swtich');
                break;
        }
    }

    async function getFromMicrophone() {
        try {
            const response = await fetch(`https://localhost:44337/api/QuestionAnswer?QuestionUI=${encodeURIComponent(question)}`);

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

    async function getAzureQnA(question) {
        console.log("I am in get AzureQnA");
        try {
            const response = await fetch(`https://localhost:44337/api/QuestionAnswer/GetAzureQnA?QuestionUI=${encodeURIComponent(question)}`);

            if (response.ok) {
                const data = await response.text(); // Parse the response as text
                console.log("Response received from AzureQnA: ", data);
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

    //function to call OpenAI based on question and sample transcript
    async function getPromptFromTranscription(question) {
        if (typeof question !== "string") {
            throw new Error("valueResult must be a string.");
        }
        console.log("COMPLETING THE ASSESSMENT NOW");
        try {
            const completeAssessmentResponse = await fetch(`https://localhost:44337/api/OpenAI/GetPromptFromTranscript?Question=${encodeURIComponent(question)}`);

            if (completeAssessmentResponse.ok) {
                const data = await completeAssessmentResponse.text(); // Parse the response as text
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
            case result.includes('full name') && result.includes('patient'):
                question = "What is the patient's full name?";
                getId = "inputPatientName";
                el = document.getElementById(getId);
                el.id == getId ? el.value = await getAzureQnA(question) : console.log('DOM.el is NULL');
                transcribe = '';
                break;
            case result.includes('health coordinator') :
                question = "What is the Health Coordinator's full name?";
                getId = "inputDoctorName";
                el = document.getElementById(getId);
                el.id == getId ? el.value = await getAzureQnA(question) : console.log('DOM.el is NULL');
                transcribe = '';
                break;
            case result.includes('medications') && result.includes('this is noted'):
                //question = "What are the patient's medical conditions? Write it in a bullet form where Medication name is incidated paired with a brief explanation.";
                //question = "What are the patient's medical conditions? Enumerate them."
                question = "What are the medications of the patient? Enumerate them."
                getId = "inputMedications";
                el = document.getElementById(getId);
                el.id == getId ? el.value = await getAzureQnA(question) : console.log('DOM.el is NULL'); //getPrompt(question, answer)
                transcribe = '';
                break;

            default:
                console.log('end of swtich');
                break;
        }
        return null;
    }

    //function to call OpenAI based on question and answer.
    async function getPrompt(question, answer) {
        console.log("I am in getPrompt");
        try {
            const response = await fetch(`https://localhost:44337/api/OpenAI/GetPrompt?Question=${encodeURIComponent(question)}&Answer=${encodeURIComponent(answer)}`);

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