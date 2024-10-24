namespace SpeechFlowCsharp.AudioProcessing
{
    public interface IAudioCapturer
    {
        bool IsCapturing { get; }
        event EventHandler<short[]>? AudioCaptured;
        void StartCapture();
        void StopCapture();
    }
}