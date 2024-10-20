using Whisper.net;

namespace SpeechFlowCsharp.AudioProcessing
{
    public sealed class TranscriptionWorker
    {
        private readonly WhisperProcessor? _processor;

        private readonly TranscriptionQueue _transcriptionQueue = new();

        private Func<string, bool>? _filterText = null;

        public TranscriptionWorker(string modelPath, string language, Func<string, bool>? filterText)
        {
            WhisperFactory factory = WhisperFactory.FromPath(modelPath);
            // Créer et configurer le processeur Whisper avec le modèle et la langue spécifiée (français ici)
            _processor = factory.CreateBuilder()  // Créer un builder pour le processeur
                        .WithLanguage(language)      // Configurer la langue pour la transcription
                        .Build();                // Construire le processeur
            _filterText = filterText;
        }

        public void AddToQueue(short[] audioData)
        {
            _transcriptionQueue.Enqueue(audioData);
        }

        /// <summary>
        /// Méthode pour démarrer la transcription dans un thread séparé.
        /// </summary>
        /// <param name="queue">File d'attente contenant les segments audio à transcrire.</param>
        public async Task StartTranscription(CancellationToken cancellationToken)
        {
            // Boucle principale pour consommer la file d'attente et transcrire les segments
            while (!cancellationToken.IsCancellationRequested)
            {
                if (_transcriptionQueue.TryDequeue(out var audioSegment))
                {
                    // Transcrire le segment audio de manière asynchrone
                    await TranscribeAudioAsync(audioSegment!); // Attend la fin de la transcription
                }
                else
                {
                    await Task.Delay(50, cancellationToken); // Ajouter un petit délai si la file est vide
                }
            }
        }

        /// <summary>
        /// Méthode asynchrone pour transcrire les données audio en utilisant Whisper.
        /// </summary>
        /// <param name="audioData">Segment audio à transcrire.</param>
        /// <param name="filterText">Segment audio à transcrire.</param>
        private async Task TranscribeAudioAsync(short[] audioData)
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
                    if (segment.Text.Length > 0)
                    {
                        if (_filterText?.Invoke(segment.Text) ?? false)
                        {
                            break;
                        }
                        else
                        {
                            // Traiter chaque segment transcrit
                            Console.WriteLine($"Texte transcrit : {segment.Text}");
                        }
                    }
                    
                }
            }
        }
    }
}
