namespace SpeechFlowCsharp.AudioProcessing
{
    public interface ITranscriptionWorker
    {
        event EventHandler<string>? TranscriptionCompleted;

        void AddToQueue(float[] audioData);

        Task StartTranscriptionAsync(CancellationToken cancellationToken);
    }
}