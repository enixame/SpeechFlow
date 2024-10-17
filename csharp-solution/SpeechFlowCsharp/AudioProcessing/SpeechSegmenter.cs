namespace SpeechFlowCsharp.AudioProcessing
{
    /// <summary>
    /// Classe responsable de la segmentation de la parole en fonction de la détection d'activité vocale (VAD).
    /// Cette classe accumule les échantillons vocaux lorsque la parole est détectée et déclenche un événement lorsque
    /// le segment de parole est terminé (lorsqu'un silence est détecté).
    /// </summary>
    public class SpeechSegmenter
    {
        // Liste utilisée pour accumuler les échantillons vocaux détectés pendant qu'il y a de la parole.
        private List<short> _currentSegment = new();

        // Instance de VadDetector utilisée pour analyser les échantillons audio et déterminer s'il y a de la parole.
        private readonly VadDetector _vadDetector;

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
        public SpeechSegmenter(VadDetector vad)
        {
            _vadDetector = vad ?? throw new ArgumentNullException(nameof(vad)); // Vérification de null ici
        }

        /// <summary>
        /// Traite les données audio pour détecter la parole.
        /// Accumule les échantillons lorsque de la parole est détectée et déclenche un événement lorsque la parole cesse.
        /// </summary>
        /// <param name="audioData">Tableau d'échantillons audio (16 bits, 16 kHz) à analyser.</param>
        public void ProcessAudio(short[] audioData)
        {
            // Utilise VadDetector pour vérifier si le segment audio contient de la parole.
            if (_vadDetector.IsSpeech(audioData))
            {
                // Si de la parole est détectée, accumuler les échantillons dans le segment en cours.
                _currentSegment.AddRange(audioData);
            }
            else if (_currentSegment.Count > 0)
            {
                // Si un silence est détecté (et que nous avons un segment accumulé), déclencher l'événement.
                SpeechSegmentDetected?.Invoke(this, _currentSegment.ToArray());

                // Réinitialiser le segment en cours après avoir déclenché l'événement.
                _currentSegment.Clear();
            }
        }
    }
}
