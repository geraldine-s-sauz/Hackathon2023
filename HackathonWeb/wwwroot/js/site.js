// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function goBack() {
    window.history.back();
}

$(document).ready(function () {
    var recognizer;

    $("#stopButton").click(function () {
        console.log("STOP RECORDING SHOULD WORK");
        recognizer.
    })
    $("#startButton").click(function () {
        if (!recognizer) {
            var SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;
            recognizer = new SpeechRecognition();
            recognizer.continuous = true;
            recognizer.interimResults = true;
            recognizer.lang = "en-US";
            recognizer.start();

            recognizer.onresult = function (event) {
                var result = event.results[event.results.length - 1];
                if (result.isFinal) {
                    $("#result").text(result[0].transcript);
                    $("#output").text(result[0].transcript);
                    $.ajax({
                        url: "/api/SpeechToText",
                        type: "POST",
                        data: JSON.stringify({ text: result[0].transcript }),
                        contentType: "application/json; charset=utf-8",
                        dataType: "json",
                        success: function (response) {
                            $("#output").text("Recognition result: " + response);
                        },
                        error: function (error) {
                            console.log("Recognition error: " + error);
                            $("#output").text("Recognition error: " + error);
                        }
                    });
                } else {
                    console.log("else was used..")
                    $("#result").text(result[0].transcript);
                }
            };
        }
    });
})