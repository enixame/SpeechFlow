using SpeechFlowCsharp.AudioProcessing;

class Program
{
    static void Main(string[] args)
    {
        // Initialisation des composants
        var audioCapturer = new AudioCapturer();
        var noiseReducer = new NoiseReducer();
        var vadDetector = new VadDetector();
        var speechSegmenter = new SpeechSegmenter(vadDetector);
        var transcriptionQueue = new TranscriptionQueue();
        var transcriptionThread = new Thread(() => TranscriptionWorker.StartTranscription(transcriptionQueue));

        // Démarrer la transcription dans un thread séparé
        transcriptionThread.Start();

        // Abonnement à l'événement de segment de parole détecté
        speechSegmenter.SpeechSegmentDetected += (sender, segment) =>
        {
            transcriptionQueue.Enqueue(segment);
        };

        // Capturer l'audio en temps réel
        audioCapturer.AudioCaptured += (sender, audioData) =>
        {
            var reducedNoise = noiseReducer.ReduceNoise(audioData);
            speechSegmenter.ProcessAudio(reducedNoise);
        };

        // Démarrer la capture audio
        audioCapturer.StartCapture();

        // Attendre que l'utilisateur appuie sur Entrée pour arrêter
        Console.WriteLine("Appuyez sur Entrée pour arrêter...");
        Console.ReadLine();

        // Arrêter proprement la capture audio et la transcription
        audioCapturer.StopCapture();
        transcriptionQueue.Stop();
        transcriptionThread.Join();
    }
}
