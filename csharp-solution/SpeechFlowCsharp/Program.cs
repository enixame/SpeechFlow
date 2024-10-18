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
        float cutoffFrequency = 1000.0f;  // Fréquence de coupure en Hz

        // Initialisation des composants
        var audioCapturer = new AudioCapturer();
        var noiseReducer = new NoiseReducer(sampleRate, cutoffFrequency);
        var vadDetector = new VadDetector();
        var voiceFilter = new VoiceFilter(vadDetector, sampleRate);
        var speechSegmenter = new SpeechSegmenter(voiceFilter);
        var transcriptionQueue = new TranscriptionQueue();
        var transcriptionWorker = new TranscriptionWorker(modelPath, "fr");
        _ = Task.Factory.StartNew(async () => await transcriptionWorker.StartTranscription(transcriptionQueue),
            TaskCreationOptions.LongRunning);
        
        // Abonnement à l'événement de segment de parole détecté
        speechSegmenter.SpeechSegmentDetected += (sender, segment) =>
        {
            transcriptionQueue.Enqueue(segment);
        };

        // Capturer l'audio en temps réel
        audioCapturer.AudioCaptured += (sender, audioData) =>
        {
            speechSegmenter.ProcessAudio(audioData);
        };

        // Démarrer la capture audio
        audioCapturer.StartCapture();

        // Attendre que l'utilisateur appuie sur Entrée pour arrêter
        Console.WriteLine("Appuyez sur Entrée pour arrêter...");
        Console.ReadLine();

        // Arrêter proprement la capture audio et la transcription
        audioCapturer.StopCapture();
        transcriptionQueue.Stop();
    }

    private static bool HasSound(short[] audioData, float threshold = 0.01f)
    {
        // Calculer l'amplitude moyenne ou maximale des échantillons audio
        float maxAmplitude = 0;

        for (int i = 0; i < audioData.Length; i++)
        {
            // Convertir les échantillons courts en valeurs flottantes
            float amplitude = Math.Abs(audioData[i] / 32768.0f);
            if (amplitude > maxAmplitude)
            {
                maxAmplitude = amplitude;
            }
        }

        // Comparer l'amplitude maximale au seuil spécifié
        return maxAmplitude > threshold;
    }

    private static async Task DownloadModel(string fileName, GgmlType ggmlType)
    {
        Console.WriteLine($"Downloading Model {fileName}");
        using var modelStream = await WhisperGgmlDownloader.GetGgmlModelAsync(ggmlType);
        using var fileWriter = File.OpenWrite(fileName);
        await modelStream.CopyToAsync(fileWriter);
    }
}
