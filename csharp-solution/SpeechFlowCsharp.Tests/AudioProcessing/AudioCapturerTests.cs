using SpeechFlowCsharp.AudioProcessing;

public class AudioCapturerTests
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
