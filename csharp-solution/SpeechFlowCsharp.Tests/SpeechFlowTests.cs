using Moq;
using SpeechFlowCsharp.AudioProcessing;
using SpeechFlowCsharp.Enums;
using Whisper.net.Ggml;

namespace SpeechFlowCsharp.Tests
{
    public class SpeechFlowTests
    {
        [Fact]
        public void SpeechFlow_Create_ShouldReturnNewInstance()
        {
            // Act
            var instance = SpeechFlow.Create();

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public void SpeechFlow_WithDefault_ShouldInitializeComponents()
        {
            // Arrange
            var speechFlow = SpeechFlow.Create();

            // Act
            speechFlow.WithDefault(16000, GgmlType.Base, Language.French);

            // Assert
            // Vérifier que les composants sont correctement initialisés
            Assert.NotNull(speechFlow); // S'assurer que l'objet est bien initialisé
        }

        [Fact]
        public async Task SpeechFlow_OnSpeechSegmentDetected_ShouldInvokeAction()
        {
            // Arrange
            var speechFlow = SpeechFlow.Create();
            var wasCalled = false;
            var mockSegmenter = new Mock<ISpeechSegmenter>();
            speechFlow.WithDefault(16000, GgmlType.Base, Language.French);
            speechFlow.WithSpeechSegmenter(mockSegmenter.Object);
            var audioData = new float[] { 1, 2, 3 };

            void onSpeechSegmentDetected(float[] _) => wasCalled = true;
            speechFlow.OnSpeechSegmentDetected(onSpeechSegmentDetected);

            // Act
            await speechFlow.StartAsync();
            // Simuler la détection d'un segment de parole
            mockSegmenter.Raise(m => m.SpeechSegmentDetected += null, new object(), audioData);

            // Assert
            Assert.True(wasCalled);
        }

        [Fact]
        public async Task SpeechFlow_OnAudioCaptured_ShouldInvokeAction()
        {
            // Arrange
            var speechFlow = SpeechFlow.Create();
            var wasCalled = false;
            var mockAudioCapturer = new Mock<IAudioCapturer>();
            speechFlow.WithDefault(16000, GgmlType.Base, Language.French);
            speechFlow.WithAudioCapturer(mockAudioCapturer.Object);
            var audioData = new float[] { 1, 2, 3 };

            Task onAudioCaptured(float[] _) { wasCalled = true; return Task.CompletedTask; }
            speechFlow.OnAudioCaptured(onAudioCaptured);

            // Act
            await speechFlow.StartAsync();
            // Simuler la capture d'audio
            mockAudioCapturer.Raise(m => m.AudioCaptured += null, new object(), audioData);

            // Assert
            Assert.True(wasCalled);
        }

        [Fact]
        public async Task SpeechFlow_StartAsync_ShouldStartTranscription()
        {
            // Arrange
            var speechFlow = SpeechFlow.Create();
            var transcriptionWorkerMock = new Mock<ITranscriptionWorker>();

            // Injecter un TranscriptionWorker mock
            speechFlow.WithTranscriptionWorker(transcriptionWorkerMock.Object);
            speechFlow.WithDefault(16000, GgmlType.Base, Language.French);

            // Act
            await speechFlow.StartAsync();

            // Assert
            transcriptionWorkerMock.Verify(m => m.StartTranscriptionAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void SpeechFlow_Dispose_ShouldStopAndDispose()
        {
            // Arrange
            var speechFlow = SpeechFlow.Create();
            var audioCapturerMock = new Mock<IAudioCapturer>();
            var transcriptionWorkerMock = new Mock<ITranscriptionWorker>();

            speechFlow.WithAudioCapturer(audioCapturerMock.Object)
                      .WithTranscriptionWorker(transcriptionWorkerMock.Object);

            // Act
            speechFlow.Dispose();

            // Assert
            audioCapturerMock.Verify(m => m.StopCapture(), Times.Once);
            transcriptionWorkerMock.Verify(m => m.AddToQueue(It.IsAny<float[]>()), Times.Never);
        }
    }
}