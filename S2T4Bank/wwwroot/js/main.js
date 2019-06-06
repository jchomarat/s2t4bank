(async function () {

    var locale = navigator.language;

    var searchParams = new URLSearchParams(window.location.search);
    var c = searchParams.get("l");
    if (c != null)
        locale = c;

    const parameters = await fetch('/params',
        {
            method: 'POST',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ locale: locale })
        });

    const { region, speechToken: authorizationToken, endpointId, directlineToken } = await parameters.json();

    const store = window.WebChat.createStore();

    window.WebChat.renderWebChat({
        directLine: window.WebChat.createDirectLine({ token: directlineToken }),
        store, locale: locale
    }, document.getElementById('webchat'));

    var SpeechSDK;

    // On document load resolve the Speech SDK dependency
    function Initialize(onComplete) {
        if (!!window.SpeechSDK) {
            onComplete(window.SpeechSDK);
        }
    }

    var isRecording = false;
    microphoneButton.init();
    microphoneButton.click(function () {
        if (!isRecording) {
            // Start recording
           microphoneButton.toggle();
            var lastRecognized = "";
            var audioConfig = SpeechSDK.AudioConfig.fromDefaultMicrophoneInput();
            
            var speechConfig;
            if (authorizationToken) {
                speechConfig = SpeechSDK.SpeechConfig.fromAuthorizationToken(authorizationToken, region);
            } else {
                alert("could not get a speech service token");
                return;
            }

            speechConfig.speechRecognitionLanguage = locale;
            speechConfig.endpointId = endpointId;
            reco = new SpeechSDK.SpeechRecognizer(speechConfig, audioConfig);

            reco.recognizing = function (s, e) {
                store.dispatch({
                    type: 'WEB_CHAT/SET_SEND_BOX',
                    payload: { text: e.result.text }
                });
            };

            reco.recognized = function (s, e) {
                lastRecognized += e.result.text + " ";
                store.dispatch({
                    type: 'WEB_CHAT/SET_SEND_BOX',
                    payload: { text: lastRecognized }
                });
            };

            // Signals that a new session has started with the speech service
            reco.sessionStarted = function (s, e) {
                microphoneButton.dictate();
                isRecording = true;
            };

            // Starts recognition
            reco.startContinuousRecognitionAsync();
        }
        else {
            // Stop recording
            reco.stopContinuousRecognitionAsync(
            function () {
                reco.close();
                reco = undefined;
                isRecording = false;
                microphoneButton.stop();
            },
            function (err) {
                reco.close();
                reco = undefined;
                isRecording = false;
                microphoneButton.stop();
            });
        }
    });

    Initialize(function (speechSdk) {
        SpeechSDK = speechSdk;
    });
})().catch(err => console.error(err));