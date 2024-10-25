namespace SpeechFlowCsharp.Tests.Utils
{
    public static class AudioUtils
    {
        public static float[] LoadFloatArray(string filePath)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (var reader = new BinaryReader(fileStream))
            {
                var length = fileStream.Length / sizeof(short); // Calculer le nombre d'éléments short dans le fichier
                float[] data = new float[length];

                for (int i = 0; i < length; i++)
                {
                    short sample = reader.ReadInt16(); // Lire chaque short
                    data[i] = sample / 32768f; // Convertir en float dans la plage -1.0f à 1.0f
                }

                return data;
            }
        }
    }
}