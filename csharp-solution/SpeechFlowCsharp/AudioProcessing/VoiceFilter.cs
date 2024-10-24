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

        public bool IsHumanVoice(short[] buffer)
        {
            // Convertir une seule fois les données de short[] vers float[] 
            float[] floatBuffer = ConvertShortArrayToFloatArray(buffer);

            // Appliquer le filtre passe-bande directement sur float[]
            for (int i = 0; i < floatBuffer.Length; i++)
            {
                floatBuffer[i] = _bandPassFilter.Transform(floatBuffer[i]);
            }

            // Reconvertir les données filtrées en short[] une seule fois
            short[] filteredBuffer = ConvertFloatArrayToShortArray(floatBuffer);

            // Passer les données au VAD (qui utilise des short[])
            return _vadDetector.IsSpeech(filteredBuffer);
        }

        private static float[] ConvertShortArrayToFloatArray(short[] shortArray)
        {
            // Conversion des short[] vers float[] (normalisation des données)
            float[] floatArray = new float[shortArray.Length];
            for (int i = 0; i < shortArray.Length; i++)
            {
                floatArray[i] = shortArray[i] / 32768.0f;
            }
            return floatArray;
        }

        private static short[] ConvertFloatArrayToShortArray(float[] floatArray)
        {
            // Conversion inverse de float[] vers short[] après le filtrage
            short[] shortArray = new short[floatArray.Length];
            for (int i = 0; i < floatArray.Length; i++)
            {
                shortArray[i] = (short)(floatArray[i] * 32768.0f);
            }
            return shortArray;
        }
    }
}
