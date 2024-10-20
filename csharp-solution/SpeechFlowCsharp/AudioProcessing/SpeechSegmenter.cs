using System.Collections.Concurrent;

namespace SpeechFlowCsharp.AudioProcessing
{
    /// <summary>
    /// Classe responsable de la segmentation de la parole en fonction de la détection d'activité vocale (VAD).
    /// Cette classe accumule les échantillons vocaux lorsque la parole est détectée et déclenche un événement lorsque
    /// le segment de parole est terminé (lorsqu'un silence est détecté).
    /// </summary>
    public sealed class SpeechSegmenter
    {
        // Liste utilisée pour accumuler les échantillons vocaux détectés pendant qu'il y a de la parole.
        private ConcurrentQueue<short> _currentSegment = new();

        // Instance de VadDetector utilisée pour analyser les échantillons audio et déterminer s'il y a de la parole.
        private readonly VadDetector _vadDetector;

        // Crée un filtre passe-bande pour les fréquences de la voix humaine (85 Hz à 255 Hz)
        private readonly VoiceFilter _voiceFilter;

        /// <summary>
        /// Événement déclenché lorsqu'un segment de parole complet est détecté.
        /// Les abonnés peuvent utiliser cet événement pour traiter les segments de parole identifiés.
        /// </summary>
        public event EventHandler<short[]>? SpeechSegmentDetected;

        /// <summary>
        /// Constructeur de la classe SpeechSegmenter.
        /// Reçoit une instance de VadDetector pour utiliser la détection d'activité vocale.
        /// </summary>
        /// <param name="vad">Instance de VadDetector utilisée pour analyser les échantillons audio.</param>
        public SpeechSegmenter(VadDetector vadDetector, int sampleRate)
        {
            _vadDetector = vadDetector;
            _voiceFilter = new VoiceFilter(_vadDetector, sampleRate);  // Crée un filtre de voix humaine
        }

        // Processus asynchrone pour gérer les segments audio
        public async Task ProcessAudioAsync(short[] audioData)
        {
            // Appliquer le filtre de voix humaine avant la détection VAD
            if (_voiceFilter.IsHumanVoice(audioData))
            {
                // Si de la parole est détectée, accumuler les échantillons dans le segment en cours.
                // Ajouter les échantillons à la queue de manière thread-safe
                foreach (var sample in audioData)
                {
                    _currentSegment.Enqueue(sample);
                }
            }
            else if (!_currentSegment.IsEmpty)
            {
                // Créer une liste temporaire pour stocker les échantillons et vider la queue
                var segmentList = new List<short>();

                while (_currentSegment.TryDequeue(out var sample))
                {
                    segmentList.Add(sample);
                }

                if (segmentList.Count > 0)
                {
                    // Si un silence est détecté (et que nous avons un segment accumulé), déclencher l'événement.
                    OnSpeechSegmentDetected([.. segmentList]);
                }
            }
            await Task.CompletedTask;  // Utilisation d'une tâche asynchrone pour améliorer la réactivité
        }

        // Déclenche l'événement lorsqu'un segment de parole est détecté
        private void OnSpeechSegmentDetected(short[] segment)
        {
            SpeechSegmentDetected?.Invoke(this, segment);
        }
    }
}
