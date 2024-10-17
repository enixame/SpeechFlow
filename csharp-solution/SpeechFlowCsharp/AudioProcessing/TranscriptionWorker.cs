using Whisper.net;

namespace SpeechFlowCsharp.AudioProcessing
{
    public static class TranscriptionWorker
    {
        private static WhisperProcessor? _processor;

        /// <summary>
        /// Méthode pour démarrer la transcription dans un thread séparé.
        /// </summary>
        /// <param name="queue">File d'attente contenant les segments audio à transcrire.</param>
        public static void StartTranscription(TranscriptionQueue queue)
        {
            string modelPath = "models/ggml-base.fr.bin"; // Chemin vers le modèle

            WhisperFactory factory = WhisperFactory.FromPath(modelPath);
            // Créer et configurer le processeur Whisper avec le modèle et la langue spécifiée (français ici)
            _processor = factory.CreateBuilder()  // Créer un builder pour le processeur
                        .WithLanguage("fr")      // Configurer la langue pour la transcription
                        .Build();                // Construire le processeur
            
            // Boucle principale pour consommer la file d'attente et transcrire les segments
            while (queue.IsRunning)
            {
                if (queue.TryDequeue(out var audioSegment))
                {
                    // Transcrire le segment audio de manière asynchrone
                    TranscribeAudioAsync(audioSegment!).Wait(); // Attend la fin de la transcription
                }
                else
                {
                    // Si la file d'attente est vide, attendre un petit moment
                    Thread.Sleep(100);
                }
            }
        }

        /// <summary>
        /// Méthode asynchrone pour transcrire les données audio en utilisant Whisper.
        /// </summary>
        /// <param name="audioData">Segment audio à transcrire.</param>
        private static async Task TranscribeAudioAsync(short[] audioData)
        {
            // Convertir les données audio en un format compatible (par exemple, en tableau de floats)
            float[] floatAudio = new float[audioData.Length];
            for (int i = 0; i < audioData.Length; i++)
            {
                floatAudio[i] = audioData[i] / 32768f; // Convertir short en float
            }

            if (_processor != null)
            {
                // Utiliser la méthode asynchrone ProcessAsync pour transcrire l'audio
                await foreach (var segment in _processor.ProcessAsync(floatAudio))
                {
                    // Traiter chaque segment transcrit
                    Console.WriteLine($"Texte transcrit : {segment.Text}");
                }
            }
        }
    }
}
