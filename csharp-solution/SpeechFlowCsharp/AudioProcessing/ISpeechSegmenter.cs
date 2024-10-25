public interface ISpeechSegmenter
{
    event EventHandler<float[]>? SpeechSegmentDetected;
    Task ProcessAudioAsync(float[] audioData);
}