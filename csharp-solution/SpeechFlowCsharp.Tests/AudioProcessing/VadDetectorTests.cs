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
            float[] rawAudio = Utils.AudioUtils.LoadFloatArray(@"data\audioSample.bin");

            // Act
            bool isSpeechDetected = vadDetector.IsSpeech(rawAudio);

            // Assert
            Assert.True(isSpeechDetected);
        }
    }
}