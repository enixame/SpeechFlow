using SpeechFlowCsharp.AudioProcessing;
using Whisper.net.Ggml;

namespace SpeechFlowCsharp
{
    public sealed class SpeechFlow : IDisposable
    {
        private AudioCapturer? _audioCapturer;
        private SpeechSegmenter? _speechSegmenter;
        private TranscriptionWorker? _transcriptionWorker;
        private Action<string>? _onTranscriptionCompleted;
        private Action<short[]>? _onSpeechSegmentDetected;
        private Func<short[], Task>? _onAudioCaptured;
        private CancellationTokenSource? _cancellationTokenSource;
        private bool disposedValue;

        // Constructeur privé pour empêcher l'instanciation directe
        private SpeechFlow() { }

        // Méthode pour créer une instance de SpeechFlow
        public static SpeechFlow Create()
        {
            return new SpeechFlow();
        }

        public SpeechFlow WithDefault(int sampleRate, GgmlType ggmlType, string language)
        {
            // Initialisation des composants
            _audioCapturer = new AudioCapturer();
            var vadDetector = new VadDetector();
            _speechSegmenter = new SpeechSegmenter(vadDetector, sampleRate);
            _transcriptionWorker = new TranscriptionWorker(modelPath, "fr", (text) => text.StartsWith(" Sous-titrage") || text.Equals(" Merci."));
        }

        // Méthode pour ajouter AudioCapturer
        public SpeechFlow WithAudioCapturer(AudioCapturer audioCapturer)
        {
            _audioCapturer = audioCapturer;
            return this;
        }

        // Méthode pour ajouter SpeechSegmenter
        public SpeechFlow WithSpeechSegmenter(SpeechSegmenter speechSegmenter)
        {
            _speechSegmenter = speechSegmenter;
            return this;
        }

        // Méthode pour ajouter TranscriptionWorker
        public SpeechFlow WithTranscriptionWorker(TranscriptionWorker transcriptionWorker)
        {
            _transcriptionWorker = transcriptionWorker;
            return this;
        }

        // Méthode pour gérer un événement de détection de segment de parole
        public SpeechFlow OnSpeechSegmentDetected(Action<short[]> onSpeechSegmentDetected)
        {
            _onSpeechSegmentDetected = onSpeechSegmentDetected;
            return this;
        }

        // Méthode pour gérer un événement de capture audio
        public SpeechFlow OnAudioCaptured(Func<short[], Task> onAudioCaptured)
        {
            _onAudioCaptured = onAudioCaptured;
            return this;
        }

        // Méthode pour gérer un événement de transcription terminée
        public SpeechFlow OnTranscriptionCompleted(Action<string> onTranscriptionCompleted)
        {
            _onTranscriptionCompleted = onTranscriptionCompleted;
            return this;
        }

        // Souscrit aux événements
        private void SubscribeToEvents()
        {
            if (_speechSegmenter != null && _onSpeechSegmentDetected != null)
            {
                _speechSegmenter.SpeechSegmentDetected += HandleSpeechSegmentDetected;
            }

            if (_audioCapturer != null && _onAudioCaptured != null)
            {
                _audioCapturer.AudioCaptured += HandleAudioCaptured;
            }

            if (_transcriptionWorker != null && _onTranscriptionCompleted != null)
            {
                _transcriptionWorker.TranscriptionCompleted += HandleTranscriptionCompleted;
            }
        }

        // Désouscrit des événements
        private void UnsubscribeFromEvents()
        {
            if (_speechSegmenter != null && _onSpeechSegmentDetected != null)
            {
                _speechSegmenter.SpeechSegmentDetected -= HandleSpeechSegmentDetected;
            }

            if (_audioCapturer != null && _onAudioCaptured != null)
            {
                _audioCapturer.AudioCaptured -= HandleAudioCaptured;
            }

            if (_transcriptionWorker != null && _onTranscriptionCompleted != null)
            {
                _transcriptionWorker.TranscriptionCompleted -= HandleTranscriptionCompleted;
            }
        }

        // Démarre le processus d'exécution
        public SpeechFlow Start()
        {
            _cancellationTokenSource = new CancellationTokenSource();

            // Souscrit aux événements
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

        // Arrête les tâches de capture et de transcription
        private void Stop()
        {
            // Désouscrit des événements
            UnsubscribeFromEvents();

            // arrête la transcription
            _cancellationTokenSource?.Cancel();

            // Arrêter la capture audio
            _audioCapturer?.StopCapture();
        }

        // Gestionnaire d'événements pour la détection de segment de parole
        private void HandleSpeechSegmentDetected(object? sender, short[] segment)
        {
            if(_onSpeechSegmentDetected != null)
            {
                _onSpeechSegmentDetected.Invoke(segment);
            }
            else // comportement par défaut
            {
                _transcriptionWorker?.AddToQueue(segment);
            }
        }

        // Gestionnaire d'événements pour la capture audio
        private async void HandleAudioCaptured(object? sender, short[] audioData)
        {
            if (_onAudioCaptured != null)
            {
                await _onAudioCaptured(audioData);
            }
            else // comportement par défaut
            {
                if(_speechSegmenter != null)
                {
                    await _speechSegmenter.ProcessAudioAsync(audioData); // Traitement asynchrone des segments audio
                }         
            }
        }

        // Gestionnaire d'événements pour la transcription terminée
        private void HandleTranscriptionCompleted(object? sender, string transcript)
        {
            _onTranscriptionCompleted?.Invoke(transcript);
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects)
                    Stop();
                }

                // free unmanaged resources (unmanaged objects) and override finalizer
                // set large fields to null
                disposedValue = true;
            }
        }

        // override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~SpeechFlow()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}