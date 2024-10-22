using SpeechFlowCsharp.AudioProcessing;

namespace SpeechFlowCsharp.Tests.AudioProcessing
{
    public sealed class AudioCapturerTests
    {
        [Fact]
        public void TestAudioCapture_StartsAndStopsCorrectly()
        {
            // Arrange
            var audioCapturer = new AudioCapturer();

            // Act
            audioCapturer.StartCapture();
            bool isCapturing = audioCapturer.IsCapturing;
            audioCapturer.StopCapture();

            // Assert
            Assert.True(isCapturing);
            Assert.False(audioCapturer.IsCapturing);
        }
    }
}