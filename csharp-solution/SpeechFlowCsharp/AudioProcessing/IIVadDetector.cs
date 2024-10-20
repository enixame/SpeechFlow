
namespace SpeechFlowCsharp.AudioProcessing
{
    public interface IVadDetector
    {
        bool IsSpeech(short[] samples);
    }
}