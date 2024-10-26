using WebRtcVadSharp;

namespace SpeechFlowCsharp.AudioProcessing
{
    public sealed class VadDetector : IVadDetector
    {
        // Instance de WebRtcVad qui gère la détection d'activité vocale
        private readonly WebRtcVad _vad;

        // Taille de trame en échantillons (160 pour 10 ms à 16 kHz)
        private readonly int _frameSize;

        // 16 échantillons par ms
        private const int SampleSize = 16;

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

            // Calculer la taille de la trame en échantillons
            _frameSize = frameDurationMs * SampleSize; // 16 échantillons par ms à 16 kHz

            // Initialise avec les paramètres par défaut (agressivité définie dans l'objet WebRtcVad).
            _vad = new WebRtcVad()
            {
                OperatingMode = OperatingMode.Aggressive,
                FrameLength = GetFrameLength(frameDurationMs),
                SampleRate = GetSampleRate(_frameSize)
            };
        }

        /// <summary>
        /// Récupère la tailled'une frame audio
        /// </summary>
        /// <param name="frameDurationMs">Durée de la trame en millisecondes (10, 20 ou 30)</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private static FrameLength GetFrameLength(int frameDurationMs)
        {
            return frameDurationMs switch
            {
                10 => FrameLength.Is10ms,
                20 => FrameLength.Is20ms,
                30 => FrameLength.Is30ms,
                _ => throw new ArgumentOutOfRangeException(nameof(frameDurationMs))
            };
        }

        /// <summary>
        /// Récuère le taux d'échantillonnage
        /// </summary>
        /// <param name="frameSize">taille d'une frame audio</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private static SampleRate GetSampleRate(int frameSize)
        {
            return frameSize switch
            {
                160 => SampleRate.Is16kHz,
                320 => SampleRate.Is32kHz,
                480 => SampleRate.Is48kHz,
                _ => throw new ArgumentOutOfRangeException(nameof(frameSize))
            };
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
