using RNNoiseSharp;

namespace SpeechFlowCsharp.AudioProcessing
{
    /// <summary>
    /// Classe responsable de la réduction du bruit dans les données audio capturées.
    /// Utilise la bibliothèque RNNoise pour appliquer un filtrage neural aux données audio et améliorer la qualité en supprimant le bruit de fond.
    /// </summary>
    public class NoiseReducer
    {
        // Objet Denoiser fourni par la bibliothèque RNNoise.
        private readonly Denoiser _denoiser;

        /// <summary>
        /// Constructeur de la classe NoiseReducer.
        /// Initialise l'instance de Denoiser utilisée pour le traitement du bruit.
        /// </summary>
        public NoiseReducer()
        {
            // Crée une nouvelle instance de Denoiser pour traiter les frames audio.
            _denoiser = new Denoiser();
        }

        /// <summary>
        /// Réduit le bruit dans un tableau d'échantillons audio.
        /// Le traitement s'effectue par blocs de 480 échantillons, qui correspondent à des frames de 30 ms à 16 kHz.
        /// </summary>
        /// <param name="audioData">Tableau d'échantillons audio (format 16 bits, 16 kHz) à traiter.</param>
        /// <returns>Un tableau d'échantillons audio avec le bruit réduit.</returns>
        public short[] ReduceNoise(short[] audioData)
        {
            int frameSize = 480; // Taille de la frame que RNNoise traite (480 échantillons = 30 ms à 16 kHz)

            // Buffer de sortie où les échantillons audio traités (sans bruit) seront stockés.
            short[] outputBuffer = new short[audioData.Length];

            // Buffer temporaire pour contenir une frame d'audio convertie en flottants (le format attendu par RNNoise).
            float[] floatFrame = new float[frameSize];

            // Traiter l'audio par frames (blocs) de 480 échantillons.
            for (int i = 0; i < audioData.Length; i += frameSize)
            {
                // Déterminer la taille de la frame actuelle (à la fin des données, elle peut être plus petite que 480 échantillons).
                int frameEnd = Math.Min(i + frameSize, audioData.Length);

                // Remplir le floatFrame avec les échantillons audio de la frame actuelle, convertis de short (int16) en float.
                for (int j = 0; j < frameEnd - i; j++)
                {
                    floatFrame[j] = audioData[i + j] / 32768f; // Conversion des short en float (entre -1 et 1)
                }

                // Appliquer le débruitage sur la frame actuelle en appelant la méthode Denoise.
                int result = _denoiser.Denoise(floatFrame);

                // Vérifier si la réduction de bruit a été réussie (result == 0 signifie succès).
                if (result == 0)
                {
                    // Si succès, convertir les échantillons float débruités en short (16 bits) et les stocker dans le buffer de sortie.
                    for (int j = 0; j < frameEnd - i; j++)
                    {
                        outputBuffer[i + j] = (short)(floatFrame[j] * 32768f); // Conversion inverse de float à short
                    }
                }
                else
                {
                    // Si la réduction de bruit échoue, conserver les données originales dans le buffer de sortie.
                    Array.Copy(audioData, i, outputBuffer, i, frameEnd - i);
                }
            }

            // Retourner le tableau d'échantillons audio avec réduction du bruit (ou les données originales si échec).
            return outputBuffer;
        }
    }
}
