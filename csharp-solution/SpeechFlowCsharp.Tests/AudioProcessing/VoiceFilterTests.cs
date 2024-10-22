using SpeechFlowCsharp.AudioProcessing;

namespace SpeechFlowCsharp.Tests.AudioProcessing
{
    public class VoiceFilterTests
    {
        [Fact]
        public void TestVoiceFilter_FiltersVoiceFrequenciesCorrectly()
        {
            // Arrange
            var vadDetector = new VadDetector();
            var filter = new VoiceFilter(vadDetector, 16000);
            short[] rawAudio = Utils.AudioUtils.LoadShortArray(@"data\audioSample.bin");

            // Act
            bool isHumanVoice = filter.IsHumanVoice(rawAudio);

            // Assert
            Assert.True(isHumanVoice);
        }
    }
}