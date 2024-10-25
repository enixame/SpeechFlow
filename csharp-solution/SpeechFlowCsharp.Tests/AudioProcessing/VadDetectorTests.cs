using SpeechFlowCsharp.AudioProcessing;

namespace SpeechFlowCsharp.Tests.AudioProcessing
{
    public sealed class VadDetectorTests
    {
        [Fact]
        public void TestVadDetector_DetectsSpeechCorrectly()
        {
            // Arrange
            var vadDetector = new VadDetector();
            float[] voiceSample = [100, 200, 300]; // Simuler un échantillon vocal

            // Act
            bool isSpeechDetected = vadDetector.IsSpeech(voiceSample);

            // Assert
            Assert.True(isSpeechDetected);
        }
    }
}