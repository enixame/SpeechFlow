namespace SpeechFlowCsharp.Tests.Utils
{
    public static class AudioUtils
    {
        public static short[] LoadShortArray(string filePath)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (var reader = new BinaryReader(fileStream))
            {
                var length = fileStream.Length / sizeof(short); // Calculer le nombre d'éléments short dans le fichier
                short[] data = new short[length];

                for (int i = 0; i < length; i++)
                {
                    data[i] = reader.ReadInt16(); // Lire chaque short
                }

                return data;
            }
        }
    }
}