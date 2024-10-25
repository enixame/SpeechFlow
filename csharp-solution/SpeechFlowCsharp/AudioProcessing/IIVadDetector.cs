
namespace SpeechFlowCsharp.AudioProcessing
{
    public interface IVadDetector
    {
        bool IsSpeech(float[] samples);
    }
}