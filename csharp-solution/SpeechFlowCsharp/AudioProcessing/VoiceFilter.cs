using NAudio.Dsp;

namespace SpeechFlowCsharp.AudioProcessing
{
    public sealed class VoiceFilter : IVoiceFilter
    {
        private readonly BiQuadFilter _bandPassFilter;
        private readonly IVadDetector _vadDetector;

        public VoiceFilter(IVadDetector vad, int sampleRate)
        {
            // Crée un filtre passe-bande pour les fréquences de la voix humaine (85 Hz à 255 Hz)
            _bandPassFilter = BiQuadFilter.BandPassFilterConstantPeakGain(sampleRate, 255.0f, 85.0f);
            
            // Initialise le détecteur d'activité vocale (VAD)
            _vadDetector = vad;
        }

        public bool IsHumanVoice(float[] buffer)
        {
            float[] floatBuffer = new float[buffer.Length];
            // Appliquer le filtre passe-bande directement sur float[]
            for (int i = 0; i < buffer.Length; i++)
            {
                floatBuffer[i] = _bandPassFilter.Transform(buffer[i]);
            }

            // Passer les données au VAD
            return _vadDetector.IsSpeech(floatBuffer);
        }
    }
}
