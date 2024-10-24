using WebRtcVadSharp;

namespace SpeechFlowCsharp.AudioProcessing
{
    public sealed class VadDetector : IVadDetector
    {
        // Instance de WebRtcVad qui gère la détection d'activité vocale
        private readonly WebRtcVad _vad;

        /// <summary>
        /// Constructeur de VadDetector.
        /// Initialise l'objet WebRtcVad avec des paramètres par défaut.
        /// </summary>
        public VadDetector()
        {
            // Initialise avec les paramètres par défaut (agressivité définie dans l'objet WebRtcVad).
            _vad = new WebRtcVad();
        }

        /// <summary>
        /// Vérifie si un segment audio contient de la parole.
        /// </summary>
        /// <param name="audioData">Tableau d'échantillons audio (16 kHz, 16 bits) à analyser.</param>
        /// <returns>Vrai si la parole est détectée, faux sinon.</returns>
        public bool IsSpeech(short[] audioData)
        {
            // Utilise WebRtcVad pour détecter la parole dans le tableau d'échantillons fourni.
            return _vad.HasSpeech(audioData);
        }
    }
}
