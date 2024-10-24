public interface ISpeechSegmenter
{
    event EventHandler<short[]>? SpeechSegmentDetected;
    Task ProcessAudioAsync(short[] audioData);
}