using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Translation;

const string subscriptionKey = "";
const string region = "WestEurope";

// Translation source language. Replace with a language of your choice.
const string fromLanguage = "fi-FI";

var stopTranslation = new TaskCompletionSource<int>();

// Creates an instance of a speech translation config with specified subscription key and service region.
// Replace with your own subscription key and service region (e.g., "westus").
var config = SpeechTranslationConfig.FromSubscription(subscriptionKey, region);
config.SpeechRecognitionLanguage = fromLanguage;

// Translation target language(s). Replace with language(s) of your choice.
config.AddTargetLanguage("en");
config.SetProperty(PropertyId.SpeechServiceConnection_ContinuousLanguageIdPriority, "Latency");

// Creates a translation recognizer using microphone as audio input.
using (var recognizer = new TranslationRecognizer(config))
{
    recognizer.Recognizing += (s, e) =>
    {
        Console.WriteLine($"{e.Result.Text}");
        foreach (var element in e.Result.Translations)
        {
            Console.WriteLine($"    {element.Value}");
        }
    };

    recognizer.Recognized += (s, e) =>
    {
        if (e.Result.Reason == ResultReason.TranslatedSpeech)
        {
            Console.WriteLine($"{e.Result.Text}");
            foreach (var element in e.Result.Translations)
            {
                Console.WriteLine($"    {element.Value}");
            }
        }
        else if (e.Result.Reason == ResultReason.RecognizedSpeech)
        {
            Console.WriteLine($"{e.Result.Text}");
            Console.WriteLine($"    Speech not translated.");
        }
        else if (e.Result.Reason == ResultReason.NoMatch)
        {
            Console.WriteLine($"NOMATCH: Speech could not be recognized.");
        }
    };

    recognizer.Canceled += (s, e) =>
    {
        Console.WriteLine($"CANCELED: Reason={e.Reason}");

        if (e.Reason == CancellationReason.Error)
        {
            Console.WriteLine($"CANCELED: ErrorCode={e.ErrorCode}");
            Console.WriteLine($"CANCELED: ErrorDetails={e.ErrorDetails}");
            Console.WriteLine($"CANCELED: Did you set the speech resource key and region values?");
        }
    };

    recognizer.SpeechStartDetected += (s, e) =>
    {
        Console.WriteLine("\nSpeech start detected event.");
    };

    recognizer.SpeechEndDetected += async (s, e) =>
    {
        Console.WriteLine("\nSpeech end detected event.");

        await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);

        await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);
    };

    recognizer.SessionStarted += (s, e) =>
    {
        Console.WriteLine("\nSession started event.");
    };

    recognizer.SessionStopped += (s, e) =>
    {
        Console.WriteLine("\nSession stopped event.");
        Console.WriteLine($"\nStop translation.");
        // stopTranslation.TrySetResult(0);
    };

    Console.WriteLine("Start translation...");
    await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);

    Task.WaitAny(new[] { stopTranslation.Task });
    await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
}