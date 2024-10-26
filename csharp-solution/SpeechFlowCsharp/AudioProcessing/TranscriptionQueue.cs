using System.Collections.Concurrent;

namespace SpeechFlowCsharp.AudioProcessing
{
    public sealed class TranscriptionQueue
    {
        private readonly ConcurrentQueue<float[]> _queue = new();
       
        public void Enqueue(float[] segment)
        {
            _queue.Enqueue(segment);
        }

        public bool TryDequeue(out float[]? segment)
        {
            return _queue.TryDequeue(out segment);
        }
    }
}
