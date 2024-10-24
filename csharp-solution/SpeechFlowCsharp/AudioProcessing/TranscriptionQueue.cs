using System.Collections.Concurrent;

namespace SpeechFlowCsharp.AudioProcessing
{
    public sealed class TranscriptionQueue
    {
        private readonly ConcurrentQueue<short[]> _queue = new();
        private bool _isRunning = true;

        public void Enqueue(short[] segment)
        {
            _queue.Enqueue(segment);
        }

        public bool TryDequeue(out short[]? segment)
        {
            return _queue.TryDequeue(out segment);
        }

        public void Stop()
        {
            _isRunning = false;
        }

        public bool IsRunning => _isRunning;
    }
}
