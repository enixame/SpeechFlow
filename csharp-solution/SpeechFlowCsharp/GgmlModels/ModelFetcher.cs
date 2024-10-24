using SpeechFlowCsharp.EnumsMapping;
using Whisper.net.Ggml;

namespace SpeechFlowCsharp.GgmlModels
{
    public static class ModelFetcher
    {
        public static async Task<string> FetchModelAsync(GgmlType ggmlType)
        {
            var fileName = GgmlTypeStringMapper.ToGgmlString(ggmlType);

            if (!File.Exists(fileName))
            {
                EnsureDirectoryExists(fileName);

                using var modelStream = await WhisperGgmlDownloader.GetGgmlModelAsync(ggmlType);
                using var fileWriter = File.OpenWrite(fileName);
                await modelStream.CopyToAsync(fileWriter);
            }

            return fileName;
        }

        private static void EnsureDirectoryExists(string filePath)
        {
            // Extraire le chemin du répertoire à partir du chemin du fichier
            string? directoryPath = Path.GetDirectoryName(filePath) ?? throw new NullReferenceException("directoryPath is null");

            // Vérifier si le répertoire existe
            if (!Directory.Exists(directoryPath))
            {
                // Si le répertoire n'existe pas, le créer
                Directory.CreateDirectory(directoryPath);
               
            }
        }
    }
}