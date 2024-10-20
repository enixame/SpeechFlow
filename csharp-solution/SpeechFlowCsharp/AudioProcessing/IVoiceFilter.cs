
namespace SpeechFlowCsharp.AudioProcessing
{
    public interface IVoiceFilter
    {
        bool IsHumanVoice(short[] buffer);
    }
}