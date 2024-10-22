using SpeechFlowCsharp.AudioProcessing;

namespace SpeechFlowCsharp.Tests.AudioProcessing
{
    public sealed class TranscriptionQueueTests
    {
        [Fact]
        public void TestTranscriptionQueue_EnqueueAndDequeue()
        {
            // Arrange
            var queue = new TranscriptionQueue();
            short[] samples = [100, 200, 300];

            // Act
            queue.Enqueue(samples);
            var result = queue.TryDequeue(out _);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void TestTranscriptionQueue_DequeueEmptyQueueReturnsFalse()
        {
            // Arrange
            var queue = new TranscriptionQueue();

            // Act
            var result = queue.TryDequeue(out _);

            // Assert
            Assert.False(result);
        }
    }
}