using SpeechFlowCsharp.AudioProcessing;

namespace SpeechFlowCsharp.Tests.AudioProcessing
{
    public sealed class TranscriptionWorkerTests
    {
        [Fact]
        public async void TestTranscriptionWorker_ProcessesQueueCorrectly()
        {
            // Arrange
            string modelPath = "models/ggml-tiny.bin"; // Chemin vers le modèle
            var worker = new TranscriptionWorker(modelPath, "fr");
            float[] samples = Utils.AudioUtils.LoadFloatArray(@"data\audioSample.bin");

            bool hasProcessed = false;
            string? transcriptValue = null;
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            worker.TranscriptionCompleted += (sender, transcript) =>
            {
                hasProcessed = true;
                transcriptValue = transcript;
            };

            // Act
            worker.AddToQueue(samples);
            await worker.StartTranscriptionAsync(cts.Token);
            // Stopper les tâches
            cts.Cancel();

            // Assert
            // Ici, nous vérifierions les effets secondaires du traitement,
            // par exemple, si un événement ou un fichier de transcription a été généré.
            Assert.True(hasProcessed); // Supposons que cette propriété existe pour simplifier le test.
            Assert.Equal(" Salut à tous ! [Musique]", transcriptValue);
        }
    }
}