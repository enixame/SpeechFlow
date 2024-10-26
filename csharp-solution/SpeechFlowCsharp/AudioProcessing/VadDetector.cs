using WebRtcVadSharp;

namespace SpeechFlowCsharp.AudioProcessing
{
    public sealed class VadDetector : IVadDetector
    {
        // Instance de WebRtcVad qui gère la détection d'activité vocale
        private readonly WebRtcVad _vad;

        // Taille de trame en échantillons (160 pour 10 ms à 16 kHz)
        private readonly int _frameSize;

        private short[] _residualBuffer = [];

        /// <summary>
        /// Constructeur de VadDetector.
        /// Initialise l'objet WebRtcVad avec des paramètres par défaut.
        /// <param name="frameDurationMs">Durée de la trame en millisecondes (10, 20 ou 30)</param>
        /// </summary>
        public VadDetector(int frameDurationMs = 10)
        {
            // Vérifier que la durée de la trame est valide
            if (frameDurationMs != 10 && frameDurationMs != 20 && frameDurationMs != 30)
                throw new ArgumentException("La durée de la trame doit être 10, 20 ou 30 ms.", nameof(frameDurationMs));

            // Initialise avec les paramètres par défaut (agressivité définie dans l'objet WebRtcVad).
            _vad = new WebRtcVad();

            // Calculer la taille de la trame en échantillons
            _frameSize = frameDurationMs * 16; // 16 échantillons par ms à 16 kHz
        }

        /// <summary>
        /// Vérifie si un segment audio contient de la parole.
        /// </summary>
        /// <param name="audioData">Tableau d'échantillons audio (16 kHz, 16 bits) à analyser.</param>
        /// <returns>Vrai si la parole est détectée, faux sinon.</returns>
        public bool IsSpeech(float[] audioData)
        {
            // Convertir les données de float à short en les remettant dans la plage d'origine pour le VAD
            short[] shortBuffer = audioData.Select(f => (short)(f * 32768)).ToArray();

            // Combiner le tampon résiduel avec les nouvelles données
            if (_residualBuffer.Length > 0)
            {
                shortBuffer = [.. _residualBuffer, .. shortBuffer];
                _residualBuffer = [];
            }

            // Découper le signal en trames de la taille appropriée
            int i;
            for (i = 0; i + _frameSize <= shortBuffer.Length; i += _frameSize)
            {
                var frame = new short[_frameSize];
                Array.Copy(shortBuffer, i, frame, 0, _frameSize);

                // Utiliser WebRtcVad pour détecter la parole dans le frame
                if (_vad.HasSpeech(frame))
                {
                    return true;
                }
            }

            // Stocker les échantillons restants pour la prochaine fois
            int remainingSamples = shortBuffer.Length - i;
            if (remainingSamples > 0)
            {
                _residualBuffer = new short[remainingSamples];
                Array.Copy(shortBuffer, i, _residualBuffer, 0, remainingSamples);
            }

            return false;
        }
    }
}
