using Whisper.net;

namespace SpeechFlowCsharp.AudioProcessing
{
    public sealed class TranscriptionWorker : ITranscriptionWorker
    {
        private WhisperProcessor? _processor;

        private readonly TranscriptionQueue _transcriptionQueue = new();

        private readonly string? _modelPath;

        private readonly string? _language;

        private readonly Func<string, bool>? _filterText = null;

        /// <summary>
        /// Événement déclenché lorsqu'un segment de parole complet est détecté.
        /// Les abonnés peuvent utiliser cet événement pour traiter les segments de parole identifiés.
        /// </summary>
        public event EventHandler<string>? TranscriptionCompleted;

        public TranscriptionWorker(string modelPath, string language)
        {
            _modelPath = modelPath;
            _language = language;
        }

        public TranscriptionWorker(string modelPath, string language, Func<string, bool> filterText)
            : this(modelPath, language)
        {
            _filterText = filterText;
        }

        public void AddToQueue(float[] audioData)
        {
            _transcriptionQueue.Enqueue(audioData);
        }

        /// <summary>
        /// Build the model
        /// </summary>
        /// <param name="translate">translate into english</param>
        private void BuildModel(bool translate)
        {
            if(_processor != null)
            {
                return;
            }

            WhisperFactory factory = WhisperFactory.FromPath(_modelPath!);
            // Créer et configurer le processeur Whisper avec le modèle et la langue spécifiée (français ici)
            var builder = factory.CreateBuilder()           // Créer un builder pour le processeur
                                 .WithLanguage(_language!); // Configurer la langue pour la transcription

            if(translate) // traduire en anglais
            {
                builder = builder.WithTranslate();
            }

            _processor = builder.Build(); // Construire le processeur
        }

        /// <summary>
        /// Méthode pour démarrer la traduction.
        /// </summary>
        /// <param name="queue">File d'attente contenant les segments audio à traduire.</param>
        public async Task StartTranslationAsync(CancellationToken cancellationToken)
        {
            BuildModel(true);
            await StartAsync(cancellationToken);
        }
        
        /// <summary>
        /// Méthode pour démarrer la transcription.
        /// </summary>
        /// <param name="queue">File d'attente contenant les segments audio à transcrire.</param>
        public async Task StartTranscriptionAsync(CancellationToken cancellationToken)
        {
            BuildModel(false);
            await StartAsync(cancellationToken);
        }

        private async Task StartAsync(CancellationToken cancellationToken)
        {
            // Boucle principale pour consommer la file d'attente et transcrire les segments
            while (!cancellationToken.IsCancellationRequested)
            {
                if (_transcriptionQueue.TryDequeue(out var audioSegment))
                {
                    // Transcrire le segment audio de manière asynchrone et obtenir le texte transcrit
                    var transcript = await TranscribeAudioAsync(audioSegment!); // Attend la fin de la transcription
                    if (!string.IsNullOrEmpty(transcript))
                    {
                        // Déclencher l'événement lorsque la transcription est terminée
                        TranscriptionCompleted?.Invoke(this, transcript);
                    }
                }
                else
                {
                    await Task.Delay(50); // Ajouter un petit délai si la file est vide
                }
            }
        }

        /// <summary>
        /// Méthode asynchrone pour transcrire les données audio en utilisant Whisper.
        /// </summary>
        /// <param name="audioData">Segment audio à transcrire.</param>
        /// <param name="filterText">Segment audio à transcrire.</param>
        private async Task<string> TranscribeAudioAsync(float[] audioData)
        {
            if (_processor != null)
            {
                // Stocker le texte transcrit
                string transcript = string.Empty;

                // Utiliser la méthode asynchrone ProcessAsync pour transcrire l'audio
                await foreach (var segment in _processor.ProcessAsync(audioData))
                {
                    if (segment.Text.Length > 0)
                    {
                        if (_filterText?.Invoke(segment.Text) ?? false)
                        {
                            break;
                        }
                        else
                        {
                            // Ajouter le texte transcrit au résultat final
                            transcript += segment.Text;
                        }
                    }
                }

                 // Retourner le texte transcrit
                return transcript;
            }

            // Si aucun processeur n'est disponible, retourner une chaîne vide
            return string.Empty;
        }
    }
}
