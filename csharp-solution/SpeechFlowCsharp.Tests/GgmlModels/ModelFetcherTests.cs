using Moq;
using Whisper.net.Ggml;
using SpeechFlowCsharp.GgmlModels;

namespace SpeechFlowCsharp.Tests.GgmlModels
{
    public class ModelFetcherTests
    {
        [Fact]
        public async Task FetchModelAsync_ShouldReturnFilePath_WhenModelExists()
        {
            // Arrange
            var ggmlType = GgmlType.Base;
            var fileName = "models/ggml-base.bin";

            // Act
            var result = await ModelFetcher.FetchModelAsync(ggmlType);

            // Assert
            Assert.Equal(fileName, result); // Vérifie que le bon chemin de fichier est retourné
        }

        [Fact]
        public async Task FetchModelAsync_ShouldReturnFilePath_WhenModelDoesNotExist()
        {
            // Arrange
            var ggmlType = GgmlType.BaseEn;

            // Act
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await ModelFetcher.FetchModelAsync(ggmlType));

            // Vérifie que le message d'erreur contient le nom du paramètre attendu
            Assert.Equal("ggmlType", exception.ParamName);
        }
    }
}