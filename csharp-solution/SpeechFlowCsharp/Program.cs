using SpeechFlowCsharp.AudioProcessing;
using Whisper.net.Ggml;

class Program
{
    static async Task Main(string[] args)
    {
        string modelPath = "models/ggml-large-v3.bin"; // Chemin vers le modèle

        var ggmlType = GgmlType.LargeV3;
        if (!File.Exists(modelPath))
        {
            await DownloadModel(modelPath, ggmlType);
        }

        // Exemple d'utilisation dans le programme principal
        int sampleRate = 16000;  // Le taux d'échantillonnage de l'audio

        // Initialisation des composants
        var audioCapturer = new AudioCapturer();
        var vadDetector = new VadDetector();
        var speechSegmenter = new SpeechSegmenter(vadDetector, sampleRate);
        var transcriptionWorker = new TranscriptionWorker(modelPath, "fr", (text) => text.StartsWith(" Sous-titrage") || text.Equals(" Merci."));

        // Cancellation token pour arrêter proprement les tâches asynchrones
        var cts = new CancellationTokenSource();
        
        // Abonnement à l'événement de segment de parole détecté
        speechSegmenter.SpeechSegmentDetected += (sender, segment) =>
        {
            transcriptionWorker.AddToQueue(segment);
        };

        // Capturer l'audio en temps réel
        audioCapturer.AudioCaptured += async (sender, audioData) =>
        {
            await speechSegmenter.ProcessAudioAsync(audioData); // Traitement asynchrone des segments audio
        };

        // Démarrer la capture audio
        audioCapturer.StartCapture();

        // S'abonner à l'événement TranscriptionCompleted
        transcriptionWorker.TranscriptionCompleted += (sender, transcript) =>
        {
            // Traiter le texte transcrit ici
            Console.WriteLine($"Texte transcrit : {transcript}");
        };

        // Démarrer la transcription
        _ = transcriptionWorker.StartTranscriptionAsync(cts.Token);

        // Attendre que l'utilisateur appuie sur Entrée pour arrêter
        Console.WriteLine("Appuyez sur Entrée pour arrêter...");
        Console.ReadLine();

        // Stopper les tâches
        cts.Cancel();

        // Arrêter proprement la capture audio et la transcription
        audioCapturer.StopCapture();
    }

    private static async Task DownloadModel(string fileName, GgmlType ggmlType)
    {
        Console.WriteLine($"Downloading Model {fileName}");
        using var modelStream = await WhisperGgmlDownloader.GetGgmlModelAsync(ggmlType);
        using var fileWriter = File.OpenWrite(fileName);
        await modelStream.CopyToAsync(fileWriter);
    }
}
