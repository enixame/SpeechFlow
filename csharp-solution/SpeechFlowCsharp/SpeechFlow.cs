using NAudio.Wave;
using SpeechFlowCsharp.AudioProcessing;
using SpeechFlowCsharp.Enums;
using SpeechFlowCsharp.GgmlModels;
using Whisper.net.Ggml;

namespace SpeechFlowCsharp
{
    /// <summary>
    /// Classe principale pour la gestion du flux de parole, intégrant la capture audio,
    /// la détection d'activité vocale, et la transcription.
    /// </summary>
    public sealed class SpeechFlow : IDisposable
    {
        // Variables membres privées

        /// <summary>
        /// Captureur audio utilisé pour capturer les données audio du microphone.
        /// </summary>
        private IAudioCapturer? _audioCapturer;

        /// <summary>
        /// Segmenteur de parole basé sur la détection d'activité vocale (VAD).
        /// </summary>
        private ISpeechSegmenter? _speechSegmenter;

        /// <summary>
        /// Outil de transcription qui gère le processus de conversion de la parole en texte.
        /// </summary>
        private ITranscriptionWorker? _transcriptionWorker;

        /// <summary>
        /// Action à exécuter lorsque la transcription est terminée.
        /// </summary>
        private Action<string>? _onTranscriptionCompleted;

        /// <summary>
        /// Action à exécuter lorsqu'un segment de parole est détecté.
        /// </summary>
        private Action<float[]>? _onSpeechSegmentDetected;

        /// <summary>
        /// Fonction asynchrone à exécuter lors de la capture d'audio brut.
        /// </summary>
        private Func<float[], Task>? _onAudioCaptured;

        /// <summary>
        /// Fonction pour filtrer le texte de la transcription avant affichage.
        /// </summary>
        private Func<string, bool>? _onFilterText;

        /// <summary>
        /// Source de jeton pour annuler les tâches asynchrones.
        /// </summary>
        private CancellationTokenSource? _cancellationTokenSource;

        /// <summary>
        /// Indicateur pour vérifier si l'objet a déjà été disposé.
        /// </summary>
        private bool _disposedValue;

        /// <summary>
        /// Type de modèle GGML à utiliser pour la transcription.
        /// </summary>
        private GgmlType _ggmlType;

        /// <summary>
        /// Langue utilisée pour la transcription.
        /// </summary>
        private string? _language;

        // Constructeurs et méthodes publiques

        /// <summary>
        /// Constructeur privé pour empêcher l'instanciation directe de la classe.
        /// Utilisez <see cref="Create"/> pour obtenir une instance.
        /// </summary>
        private SpeechFlow() { }

        /// <summary>
        /// Crée une nouvelle instance de <see cref="SpeechFlow"/>.
        /// </summary>
        /// <returns>Une nouvelle instance de <see cref="SpeechFlow"/>.</returns>
        public static SpeechFlow Create()
        {
            return new SpeechFlow();
        }

        /// <summary>
        /// Configure l'instance avec les paramètres par défaut.
        /// Initialise la capture audio, la détection VAD et la segmentation.
        /// </summary>
        /// <param name="sampleRate">Le taux d'échantillonnage en Hertz.</param>
        /// <param name="ggmlType">Le type de modèle GGML à utiliser pour la transcription.</param>
        /// <param name="language">La langue à utiliser pour la transcription.</param>
        /// <returns>L'instance configurée de <see cref="SpeechFlow"/>.</returns>
        public SpeechFlow WithDefault(int sampleRate, GgmlType ggmlType, Language language)
        {
            _ggmlType = ggmlType;
            _language = LanguageStringMapper.ToLanguageString(language);

            // Initialisation des composants audio et VAD
            var waveFormat = new WaveFormat(sampleRate, 1); // Format audio avec un seul canal
            _audioCapturer = new AudioCapturer(waveFormat);
            var vadDetector = new VadDetector(); // Détecteur d'activité vocale
            _speechSegmenter = new SpeechSegmenter(vadDetector, sampleRate);

            return this;
        }

        /// <summary>
        /// Configure l'instance avec des paramètres par défaut et la langue définie sur le français.
        /// </summary>
        /// <param name="sampleRate">Le taux d'échantillonnage en Hertz.</param>
        /// <param name="ggmlType">Le type de modèle GGML à utiliser pour la transcription.</param>
        /// <returns>L'instance configurée de <see cref="SpeechFlow"/>.</returns>
        public SpeechFlow WithDefaultFrench(int sampleRate, GgmlType ggmlType)
        {
            return WithDefault(sampleRate, ggmlType, Language.French);
        }

        /// <summary>
        /// Configure l'instance avec des paramètres par défaut pour le français et un taux d'échantillonnage de 16000 Hz.
        /// </summary>
        /// <param name="ggmlType">Le type de modèle GGML à utiliser pour la transcription.</param>
        /// <returns>L'instance configurée de <see cref="SpeechFlow"/>.</returns>
        public SpeechFlow WithDefaultFrenchAudioRate(GgmlType ggmlType)
        {
            return WithDefault(16000, ggmlType, Language.French);
        }

        /// <summary>
        /// Spécifie le type de modèle GGML à utiliser.
        /// </summary>
        /// <param name="ggmlType">Le type de modèle GGML à utiliser pour la transcription.</param>
        /// <returns>L'instance de <see cref="SpeechFlow"/>.</returns>
        public SpeechFlow WithGgmlType(GgmlType ggmlType)
        {
             _ggmlType = ggmlType;
            return this;
        }

        /// <summary>
        /// Définit la langue à utiliser pour la transcription.
        /// </summary>
        /// <param name="language">La langue à utiliser pour la transcription.</param>
        /// <returns>L'instance configurée de <see cref="SpeechFlow"/>.</returns>
        public SpeechFlow WithLanguage(Language language)
        {
            _language = LanguageStringMapper.ToLanguageString(language);
            return this;
        }

        /// <summary>
        /// Ajoute un <see cref="IAudioCapturer"/> personnalisé à l'instance.
        /// </summary>
        /// <param name="audioCapturer">L'instance de <see cref="IAudioCapturer"/>.</param>
        /// <returns>L'instance configurée de <see cref="SpeechFlow"/>.</returns>
        public SpeechFlow WithAudioCapturer(IAudioCapturer audioCapturer)
        {
            _audioCapturer = audioCapturer;
            return this;
        }

        /// <summary>
        /// Ajoute un <see cref="ISpeechSegmenter"/> personnalisé à l'instance.
        /// </summary>
        /// <param name="speechSegmenter">L'instance de <see cref="ISpeechSegmenter"/>.</param>
        /// <returns>L'instance configurée de <see cref="SpeechFlow"/>.</returns>
        public SpeechFlow WithSpeechSegmenter(ISpeechSegmenter speechSegmenter)
        {
            _speechSegmenter = speechSegmenter;
            return this;
        }

        /// <summary>
        /// Ajoute un <see cref="ITranscriptionWorker"/> personnalisé à l'instance.
        /// </summary>
        /// <param name="transcriptionWorker">L'instance de <see cref="ITranscriptionWorker"/>.</param>
        /// <returns>L'instance configurée de <see cref="SpeechFlow"/>.</returns>
        public SpeechFlow WithTranscriptionWorker(ITranscriptionWorker transcriptionWorker)
        {
            _transcriptionWorker = transcriptionWorker;
            return this;
        }

        /// <summary>
        /// Définit une action à exécuter lorsqu'un segment de parole est détecté.
        /// </summary>
        /// <param name="onSpeechSegmentDetected">Action à exécuter lorsqu'un segment de parole est détecté.</param>
        /// <returns>L'instance configurée de <see cref="SpeechFlow"/>.</returns>
        public SpeechFlow OnSpeechSegmentDetected(Action<float[]> onSpeechSegmentDetected)
        {
            _onSpeechSegmentDetected = onSpeechSegmentDetected;
            return this;
        }

        /// <summary>
        /// Définit une fonction asynchrone à exécuter lorsque l'audio est capturé.
        /// </summary>
        /// <param name="onAudioCaptured">Fonction asynchrone à exécuter lorsque l'audio est capturé.</param>
        /// <returns>L'instance configurée de <see cref="SpeechFlow"/>.</returns>
        public SpeechFlow OnAudioCaptured(Func<float[], Task> onAudioCaptured)
        {
            _onAudioCaptured = onAudioCaptured;
            return this;
        }

        /// <summary>
        /// Définit une action à exécuter lorsque la transcription est terminée.
        /// </summary>
        /// <param name="onTranscriptionCompleted">Action à exécuter à la fin de la transcription.</param>
        /// <returns>L'instance configurée de <see cref="SpeechFlow"/>.</returns>
        public SpeechFlow OnTranscriptionCompleted(Action<string> onTranscriptionCompleted)
        {
            _onTranscriptionCompleted = onTranscriptionCompleted;
            return this;
        }

        /// <summary>
        /// Définit une fonction pour filtrer le texte de la transcription.
        /// </summary>
        /// <param name="onFilterText">Fonction à utiliser pour filtrer le texte de la transcription.</param>
        /// <returns>L'instance configurée de <see cref="SpeechFlow"/>.</returns>
        public SpeechFlow OnFilterText(Func<string, bool> onFilterText)
        {
            _onFilterText = onFilterText;
            return this;
        }

        /// <summary>
        /// Souscrit aux événements pour détecter les segments de parole,
        /// capturer l'audio et gérer la transcription.
        /// </summary>
        private void SubscribeToEvents()
        {
            if (_speechSegmenter != null)
            {
                _speechSegmenter.SpeechSegmentDetected += HandleSpeechSegmentDetected;
            }

            if (_audioCapturer != null)
            {
                _audioCapturer.AudioCaptured += HandleAudioCaptured;
            }

            if (_transcriptionWorker != null)
            {
                _transcriptionWorker.TranscriptionCompleted += HandleTranscriptionCompleted;
            }
        }

        /// <summary>
        /// Se désabonne des événements pour éviter les fuites de mémoire
        /// et arrêter la capture et la transcription lorsque cela n'est plus nécessaire.
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            if (_speechSegmenter != null)
            {
                _speechSegmenter.SpeechSegmentDetected -= HandleSpeechSegmentDetected;
            }

            if (_audioCapturer != null)
            {
                _audioCapturer.AudioCaptured -= HandleAudioCaptured;
            }

            if (_transcriptionWorker != null)
            {
                _transcriptionWorker.TranscriptionCompleted -= HandleTranscriptionCompleted;
            }
        }

        /// <summary>
        /// Obtient ou crée un nouvel objet <see cref="ITranscriptionWorker"/> en fonction
        /// du modèle GGML spécifié.
        /// </summary>
        /// <returns>Un objet <see cref="ITranscriptionWorker"/> prêt pour la transcription.</returns>
        private async Task<ITranscriptionWorker> GetOrCreateTranscriptionWorkerAsync()
        {
            if(_transcriptionWorker == null)
            {
                // Téléchargement ou récupération du modèle GGML requis
                var modelPath = await ModelFetcher.FetchModelAsync(_ggmlType);
                if(_onFilterText != null)
                {
                    return new TranscriptionWorker(modelPath, _language!, _onFilterText);
                }
                else
                {
                    return new TranscriptionWorker(modelPath, _language!);
                }
            }

            return _transcriptionWorker;
        }

        /// <summary>
        /// Démarre la capture audio et la transcription.
        /// </summary>
        /// <returns>L'instance de <see cref="SpeechFlow"/> après avoir démarré la capture et la transcription.</returns>
        public async Task<SpeechFlow> StartAsync()
        {
            _transcriptionWorker = await GetOrCreateTranscriptionWorkerAsync();
            _cancellationTokenSource = new CancellationTokenSource();

            // Souscrit aux événements de capture et de traitement
            SubscribeToEvents();

            // Démarrer la capture audio
            _audioCapturer?.StartCapture();

            // Démarrer la transcription dans un thread séparé
            _ = Task.Run(async () => 
            {
                if(_transcriptionWorker != null)
                {
                    await _transcriptionWorker.StartTranscriptionAsync(_cancellationTokenSource.Token);
                }  
            });

            return this;
        }

        /// <summary>
        /// Arrête la capture audio et la transcription, et désouscrit des événements.
        /// </summary>
        private void Stop()
        {
            // Désouscrit des événements pour arrêter la détection de segments et la capture d'audio
            UnsubscribeFromEvents();

            // Annule la transcription en cours
            _cancellationTokenSource?.Cancel();

            // Arrête la capture audio
            _audioCapturer?.StopCapture();
        }

        /// <summary>
        /// Gestionnaire d'événements pour les segments de parole détectés.
        /// </summary>
        /// <param name="sender">L'expéditeur de l'événement.</param>
        /// <param name="segment">Le segment de parole détecté sous forme de tableau de float (données audio).</param>
        private void HandleSpeechSegmentDetected(object? sender, float[] segment)
        {
            if(_onSpeechSegmentDetected != null)
            {
                _onSpeechSegmentDetected.Invoke(segment);
            }
            else // Comportement par défaut : ajouter le segment au file d'attente pour la transcription
            {
                _transcriptionWorker?.AddToQueue(segment);
            }
        }

        /// <summary>
        /// Gestionnaire d'événements pour la capture audio.
        /// </summary>
        /// <param name="sender">L'expéditeur de l'événement.</param>
        /// <param name="audioData">Les données audio capturées sous forme de tableau de float.</param>
        private async void HandleAudioCaptured(object? sender, float[] audioData)
        {
            if (_onAudioCaptured != null)
            {
                await _onAudioCaptured(audioData);
            }
            else // Comportement par défaut : traitement des segments audio via VAD
            {
                if(_speechSegmenter != null)
                {
                    await _speechSegmenter.ProcessAudioAsync(audioData); // Traitement asynchrone des segments audio
                }         
            }
        }

        /// <summary>
        /// Gestionnaire d'événements pour la transcription terminée.
        /// </summary>
        /// <param name="sender">L'expéditeur de l'événement.</param>
        /// <param name="transcript">Le texte transcrit généré.</param>
        private void HandleTranscriptionCompleted(object? sender, string transcript)
        {
            _onTranscriptionCompleted?.Invoke(transcript);
        }

        /// <summary>
        /// Libère les ressources utilisées par l'objet <see cref="SpeechFlow"/>.
        /// </summary>
        /// <param name="disposing">Indique si les ressources managées doivent être libérées.</param>
        private void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // Dispose les ressources managées (objets managés)
                    Stop();
                }

                // Libère les ressources non managées et nettoie les objets de grande taille
                _disposedValue = true;
            }
        }

        /// <summary>
        /// Implémentation de la méthode <see cref="IDisposable.Dispose"/> pour nettoyer les ressources.
        /// </summary>
        public void Dispose()
        {
            // Appelle la méthode Dispose pour libérer les ressources
            Dispose(disposing: true);
            GC.SuppressFinalize(this); // Supprime l'objet de la file d'attente de finalisation
        }
    }
}