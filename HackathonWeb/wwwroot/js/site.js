// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

//GLOBAL VARIABLES
let transcribe = '';
let fullTranscribe = '';
let lastField = null;
let listOfMentionField = new Set();


const fieldKeywords = {
    "Health Coordinator Name": "coordinator|health coordinator|doctor",
    "Patient Full Name": "patient|full name",
    "Allergies": "allergy|allergies",
    "Medications": "medication|medications",
    "Vaccine": "vaccine|vaccines",
    "Medical History": "medical history|medical conditions|medical condition",
    "Closing": "have a great day"
};


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
                    fullTranscribe = fullTranscribe + " " + transcribe;
                    fromWebkitSpeechRecognition(fullTranscribe);
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
                document.getElementById('output').innerText = 'Listening...';
                speech.recognition.start();
            }
            else {
                transcribe = '';
                document.getElementById('output').innerText = 'Listening has ended.';
                speech.recognition.stop();
            }
        })
    }
}
async function fillFormFieldsBasedOnTranscript(transcript) {
    for (const [key, value] of Object.entries(fieldKeywords)) {
        if (containsAnyKeywords(transcript, value) && !listOfMentionField.has(key)) {
            // Update the form field if a match is found
            listOfMentionField.add(key);
            if (lastField){
                console.log(`Answering ${lastField}`);
                await setFormField(lastField, transcript);
            }
            lastField = key
        }
    }
}

function containsAnyKeywords(text, keywords) {
    if (!text) return false;
    const regex = new RegExp(`\\b(${keywords})\\b`, "i"); // Case-insensitive whole word match
    return regex.test(text);
}


//function to populate the assessment using webkitSpeechRecnognition.
async function setFormField(fieldName, value) {
    switch (fieldName) {
        case "Health Coordinator Name":
            if (!$("#inputDoctorName").val()) {
                $("#inputDoctorName").val(await getAzureQnA2(
                    "What is the Health Coordinator's full name?",
                    value
                ));
            }
            break;
        case "Patient Full Name":
            if (!$("#inputPatientName").val()) {
                $("#inputPatientName").val(await getAzureQnA2(
                    "What is the patient's full name?",
                    value
                ));
            }
            break;
        case "Allergies":
            if (!$("#inputAllergies").val()) {
                $("#inputAllergies").val(await getPrompt(
                    "Does the patient has allergies? If yes, list provide the list of allegies.",
                    value
                ));
            }
            break;
        case "Medications":
            if (!$("#inputMedications").val()) {
                $("#inputMedications").val(await getPrompt(
                    "What are the medications of the patient, write in bullet form.",
                    value
                ));
            }
            break;
        case "Vaccine":
            if (!$("#inputVaccine").val()) {
                $("#inputVaccine").val(await getPrompt(
                    "Describe the patient's vaccine history in detail. If possible, write it in bullet form.",
                    value
                ));
            }
            break;
        case "Medical History":
            if (!$("#inputMH").val()) {
                $("#inputMH").val(await getPrompt(
                    "Describe the patient's medical history in detail. If possible, write it in bullet form.",
                    value
                ));
            }
            break;
        case "Closing":
            break;
    }
}

//function to populate the assessment using webkitSpeechRecnognition.
async function fromWebkitSpeechRecognition(result) {
    console.log("fromWebkitSpeechRecognition is here!")

    if (typeof result !== "string") {
        throw new Error("valueResult must be a string.");
    }
    fillFormFieldsBasedOnTranscript(transcribe)
    
}
function delay(ms) {
    return new Promise(resolve => setTimeout(resolve, ms));
}
//function to complete the assessment
async function completeAssessment() {
        $("#inputSummary").val(await getPrompt(
            "Summarize the whole conversation between the health coordinator and patient, include key points like vaccine history, medication, medical conditions.",
            transcribe
        ));
}

//function to call OpenAI based on question and answer.
async function getPrompt(question, answer) {
    console.log("I am in getPrompt");
    console.log("Question: " + question)

    if (typeof question !== "string" || typeof answer !== "string") {
        throw new Error("value must be a string.");
    }
    try {
        const response = await fetch(`https://localhost:7290/api/OpenAI/GetPrompt?Question=${encodeURIComponent(question)}&Answer=${encodeURIComponent(answer)}`);

        if (response.ok) {
            let data = await response.text(); // Parse the response as text
            data = data.replace(/^\s+/, ''); // Remove leading whitespace
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

//function to call OpenAI based on question and sample transcript
async function getPromptFromTranscription(question, fullconversation) {
    if (typeof question !== "string") {
        throw new Error("valueResult must be a string.");
    }
    console.log("COMPLETING THE ASSESSMENT NOW");
    try {
        //const completeAssessmentResponse = await fetch(`https://localhost:7290/api/OpenAI/GetPromptFromTranscript?Question=${encodeURIComponent(question)}`);
        const completeAssessmentResponse = await fetch(`https://localhost:7290/api/OpenAI/GetPromptFromTranscript2?Question=${encodeURIComponent(question)}&Transcript=${encodeURIComponent(fullconversation)}`);

        if (completeAssessmentResponse.ok) {
            let data = await completeAssessmentResponse.text(); // Parse the response as text
            data = data.replace(/^\s+/, ''); 
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
            document.getElementById('output').innerText = 'Error occurred during the API call.';
        }
    } catch (error) {
        // Handle any network or other errors here
        document.getElementById('output').innerText = 'An error occurred: ' + error.message;
    }
}

async function getAzureQnA2(question,transcribe) {
    console.log("I am in get AzureQnA2");
    try {
        const response = await fetch(`https://localhost:7290/api/QuestionAnswer/GetAzureQnA2?QuestionUI=${encodeURIComponent(question)}&TranscribeText=${encodeURIComponent(transcribe)}`);

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
            document.getElementById('output').innerText = 'Error occurred during the API call.';
        }
    } catch (error) {
        // Handle any network or other errors here
        document.getElementById('output').innerText = 'An error occurred: ' + error.message;
    }
}
