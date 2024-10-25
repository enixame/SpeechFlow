
namespace SpeechFlowCsharp.AudioProcessing
{
    public interface IVoiceFilter
    {
        bool IsHumanVoice(float[] buffer);
    }
}