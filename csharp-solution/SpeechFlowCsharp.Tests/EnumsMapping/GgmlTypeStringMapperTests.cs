using Whisper.net.Ggml;
using SpeechFlowCsharp.EnumsMapping;

namespace SpeechFlowCsharp.Tests.EnumsMapping
{
    public class GgmlTypeStringMapperTests
    {
        [Fact]
        public void ToGgmlString_ShouldReturnTinyModelPath_WhenGgmlTypeIsTiny()
        {
            // Act
            var result = GgmlTypeStringMapper.ToGgmlString(GgmlType.Tiny);

            // Assert
            Assert.Equal("models/ggml-tiny.bin", result);
        }

        [Fact]
        public void ToGgmlString_ShouldReturnBaseModelPath_WhenGgmlTypeIsBase()
        {
            // Act
            var result = GgmlTypeStringMapper.ToGgmlString(GgmlType.Base);

            // Assert
            Assert.Equal("models/ggml-base.bin", result);
        }

        [Fact]
        public void ToGgmlString_ShouldReturnMediumModelPath_WhenGgmlTypeIsMedium()
        {
            // Act
            var result = GgmlTypeStringMapper.ToGgmlString(GgmlType.Medium);

            // Assert
            Assert.Equal("models/ggml-medium.bin", result);
        }

        [Fact]
        public void ToGgmlString_ShouldReturnSmallModelPath_WhenGgmlTypeIsSmall()
        {
            // Act
            var result = GgmlTypeStringMapper.ToGgmlString(GgmlType.Small);

            // Assert
            Assert.Equal("models/ggml-small.bin", result);
        }

        [Fact]
        public void ToGgmlString_ShouldReturnLargeV1ModelPath_WhenGgmlTypeIsLargeV1()
        {
            // Act
            var result = GgmlTypeStringMapper.ToGgmlString(GgmlType.LargeV1);

            // Assert
            Assert.Equal("models/ggml-large-v1.bin", result);
        }

        [Fact]
        public void ToGgmlString_ShouldReturnLargeV2ModelPath_WhenGgmlTypeIsLargeV2()
        {
            // Act
            var result = GgmlTypeStringMapper.ToGgmlString(GgmlType.LargeV2);

            // Assert
            Assert.Equal("models/ggml-large-v2.bin", result);
        }

        [Fact]
        public void ToGgmlString_ShouldReturnLargeV3ModelPath_WhenGgmlTypeIsLargeV3()
        {
            // Act
            var result = GgmlTypeStringMapper.ToGgmlString(GgmlType.LargeV3);

            // Assert
            Assert.Equal("models/ggml-large-v3.bin", result);
        }

        [Fact]
        public void ToGgmlString_ShouldReturnLargeV3TurboModelPath_WhenGgmlTypeIsLargeV3Turbo()
        {
            // Act
            var result = GgmlTypeStringMapper.ToGgmlString(GgmlType.LargeV3Turbo);

            // Assert
            Assert.Equal("models/ggml-large-v3-turbo.bin", result);
        }

        [Fact]
        public void ToGgmlString_ShouldThrowArgumentOutOfRangeException_WhenGgmlTypeIsInvalid()
        {
            // Arrange
            var invalidGgmlType = (GgmlType)999; // Un type GGML invalide

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => GgmlTypeStringMapper.ToGgmlString(invalidGgmlType));

            // Vérifie que l'exception contient le bon nom de paramètre
            Assert.Equal("ggmlType", exception.ParamName);
        }
    }
}