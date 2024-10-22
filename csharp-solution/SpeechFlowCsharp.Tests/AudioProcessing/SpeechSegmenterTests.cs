using Moq;
using SpeechFlowCsharp.AudioProcessing;

namespace SpeechFlowCsharp.Tests.AudioProcessing
{
    public sealed class SpeechSegmenterTests
    {
        [Fact]
        public async Task TestSpeechSegmenter_AccumulatesSamplesCorrectly()
        {
            // Arrange
            int segmentCount = 0;

            // Simuler le VoiceFilter avec Moq
            var mockVoiceFilterDetector = new Mock<IVoiceFilter>();

            // Configurer le mock pour détecter la parole si la somme des échantillons est positive
            mockVoiceFilterDetector.Setup(v => v.IsHumanVoice(It.Is<short[]>(s => s.Sum(x => x) > 0))).Returns(true);  // Parole détectée
            mockVoiceFilterDetector.Setup(v => v.IsHumanVoice(It.Is<short[]>(s => s.Sum(x => x) <= 0))).Returns(false);  // Parole non détectée

            var segmenter = new SpeechSegmenter(mockVoiceFilterDetector.Object);
            segmenter.SpeechSegmentDetected += (sender, e) => segmentCount = e.Length;
            short[] samples = [100, 200, 300];
            short[] silenceSamples = [0, 0, 0];

            // Act
            // Simuler l'ajout d'échantillons vocaux et la détection de la parole.
            await segmenter.ProcessAudioAsync(samples);
            await segmenter.ProcessAudioAsync(silenceSamples); // no voice

            // Assert
            // Vérifier que les échantillons ont été correctement accumulés.
            Assert.Equal(3, segmentCount);
        }
    }
}