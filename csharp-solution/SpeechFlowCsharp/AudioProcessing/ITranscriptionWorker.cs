namespace SpeechFlowCsharp.AudioProcessing
{
    public interface ITranscriptionWorker
    {
        event EventHandler<string>? TranscriptionCompleted;

        void AddToQueue(short[] audioData);

        Task StartTranscriptionAsync(CancellationToken cancellationToken);
    }
}