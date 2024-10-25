namespace SpeechFlowCsharp.AudioProcessing
{
    public interface IAudioCapturer
    {
        bool IsCapturing { get; }
        event EventHandler<float[]>? AudioCaptured;
        void StartCapture();
        void StopCapture();
    }
}