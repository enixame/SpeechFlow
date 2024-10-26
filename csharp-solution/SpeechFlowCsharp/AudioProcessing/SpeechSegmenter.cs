using System.Collections.Concurrent;

namespace SpeechFlowCsharp.AudioProcessing
{
    /// <summary>
    /// Classe responsable de la segmentation de la parole en fonction de la détection d'activité vocale (VAD).
    /// Cette classe accumule les échantillons vocaux lorsque la parole est détectée et déclenche un événement lorsque
    /// le segment de parole est terminé (lorsqu'un silence est détecté).
    /// </summary>
    public sealed class SpeechSegmenter : ISpeechSegmenter
    {
        // Liste utilisée pour accumuler les échantillons vocaux détectés pendant qu'il y a de la parole.
        private ConcurrentQueue<float> _currentSegment = new();

        // Crée un filtre passe-bande pour les fréquences de la voix humaine (85 Hz à 255 Hz)
        private readonly IVoiceFilter _voiceFilter;

        // Indique si nous sommes actuellement en train de détecter de la parole
        private bool _isSpeaking = false;

        /// <summary>
        /// Événement déclenché lorsqu'un segment de parole complet est détecté.
        /// Les abonnés peuvent utiliser cet événement pour traiter les segments de parole identifiés.
        /// </summary>
        public event EventHandler<float[]>? SpeechSegmentDetected;

        /// <summary>
        /// Constructeur de la classe SpeechSegmenter.
        /// Reçoit une instance de VadDetector pour utiliser la détection d'activité vocale.
        /// </summary>
        /// <param name="vadDetector">Instance de VadDetector utilisée pour analyser les échantillons audio.</param>
        public SpeechSegmenter(IVadDetector vadDetector, int sampleRate)
        {
            _voiceFilter = new VoiceFilter(vadDetector, sampleRate);  // Crée un filtre de voix humaine
        }

        /// <summary>
        /// Constructeur de la classe SpeechSegmenter.
        /// Reçoit une instance de VoiceFilter pour utiliser la détection d'activité vocale.
        /// </summary>
        /// <param name="voiceFilter">Instance de VadDetector utilisée pour analyser les échantillons audio.</param>
        public SpeechSegmenter(IVoiceFilter voiceFilter)
        {
            _voiceFilter = voiceFilter;  // Crée un filtre de voix humaine
        }

        // Processus asynchrone pour gérer les segments audio
        public async Task ProcessAudioAsync(float[] audioData)
        {
            // Vérifie si la voix humaine est détectée dans les données audio
            bool speechDetected = _voiceFilter.IsHumanVoice(audioData);

            if (speechDetected)
            {
                if (!_isSpeaking)
                {
                    _isSpeaking = true;
                    // Début d'un nouveau segment de parole
                }
                // Accumuler les échantillons audio
                foreach (var sample in audioData)
                {
                    _currentSegment.Enqueue(sample);
                }
            }
            else if (_isSpeaking)
            {
                _isSpeaking = false;
                // Fin du segment de parole, déclencher l'événement
                var segmentArray = _currentSegment.ToArray();
                _currentSegment.Clear();
                OnSpeechSegmentDetected(segmentArray);
            }

            // Pas besoin d'attendre ici, mais on garde la signature asynchrone pour compatibilité
            await Task.CompletedTask;
        }

        // Déclenche l'événement lorsqu'un segment de parole est détecté
        private void OnSpeechSegmentDetected(float[] segment)
        {
            if (segment.Length > 0)
            {
                SpeechSegmentDetected?.Invoke(this, segment);
            }
        }
    }
}
