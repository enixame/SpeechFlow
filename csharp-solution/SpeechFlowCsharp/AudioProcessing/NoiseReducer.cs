using NAudio.Dsp;

namespace SpeechFlowCsharp.AudioProcessing
{
    /// <summary>
    /// Classe responsable de la réduction du bruit dans les données audio capturées.
    /// </summary>
    public class NoiseReducer
    {
        private readonly BiQuadFilter lowPassFilter;
        private readonly int sampleRate;

        public NoiseReducer(int sampleRate, float cutoffFrequency)
        {
            this.sampleRate = sampleRate;
            // Crée un filtre passe-bas avec une fréquence de coupure donnée
            lowPassFilter = BiQuadFilter.LowPassFilter(sampleRate, cutoffFrequency, 1.0f);
        }

        /// <summary>
        /// Applique un filtre passe-bas sur les données audio capturées en short[], 
        /// et affiche la fréquence dominante.
        /// </summary>
        /// <param name="buffer">Le tableau contenant les échantillons audio (short[]).</param>
        /// <param name="offset">L'index de départ du tableau.</param>
        /// <param name="count">Le nombre d'échantillons à traiter.</param>
        public void ApplyLowPassFilterAndShowFrequency(short[] buffer, int offset, int count)
        {
            // Convertit le tableau short[] en float[] pour appliquer le filtre
            float[] floatBuffer = ConvertShortArrayToFloatArray(buffer);

            // Applique le filtre passe-bas
            for (int i = offset; i < offset + count; i++)
            {
                floatBuffer[i] = lowPassFilter.Transform(floatBuffer[i]);
            }

            // Calcule et affiche la fréquence dominante
            float dominantFrequency = CalculateDominantFrequency(floatBuffer, count);
            Console.WriteLine($"Fréquence dominante après filtrage: {dominantFrequency} Hz");

            // Reconvertit le tableau float[] en short[] si nécessaire
            for (int i = offset; i < offset + count; i++)
            {
                buffer[i] = Convert.ToInt16(floatBuffer[i] * 32768.0f);
            }
        }

        /// <summary>
        /// Convertit un tableau short[] en float[].
        /// </summary>
        private static float[] ConvertShortArrayToFloatArray(short[] shortArray)
        {
            float[] floatArray = new float[shortArray.Length];
            for (int i = 0; i < shortArray.Length; i++)
            {
                // Division par 32768.0f pour normaliser la plage [-32768, 32767] à [-1.0, 1.0]
                floatArray[i] = shortArray[i] / 32768.0f;
            }
            return floatArray;
        }

        /// <summary>
        /// Calcule la fréquence dominante dans un tableau d'échantillons audio.
        /// </summary>
        /// <param name="audioSamples">Tableau des échantillons audio en float.</param>
        /// <param name="length">Nombre d'échantillons à traiter.</param>
        /// <returns>La fréquence dominante en Hertz.</returns>
        private float CalculateDominantFrequency(float[] audioSamples, int length)
        {
            // Applique la FFT (Transformée de Fourier Rapide)
            Complex[] fftBuffer = new Complex[length];
            for (int i = 0; i < length; i++)
            {
                fftBuffer[i].X = (float)(audioSamples[i] * FastFourierTransform.HannWindow(i, length));  // Applique une fenêtre pour réduire les artefacts
                fftBuffer[i].Y = 0;  // Partie imaginaire à 0
            }

            // Effectue la FFT
            FastFourierTransform.FFT(true, (int)Math.Log(length, 2.0), fftBuffer);

            // Trouve l'amplitude maximale et la fréquence correspondante
            int maxIndex = 0;
            float maxMagnitude = 0.0f;
            for (int i = 0; i < length / 2; i++)  // Nous n'avons besoin que de la moitié de la FFT (symétrique)
            {
                float magnitude = fftBuffer[i].X * fftBuffer[i].X + fftBuffer[i].Y * fftBuffer[i].Y;
                if (magnitude > maxMagnitude)
                {
                    maxMagnitude = magnitude;
                    maxIndex = i;
                }
            }

            // Calcule la fréquence dominante
            float dominantFrequency = (float)maxIndex * sampleRate / length;
            return dominantFrequency;
        }
    }
}
