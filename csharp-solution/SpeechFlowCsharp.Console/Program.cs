using SpeechFlowCsharp;
using Whisper.net.Ggml;

class Program
{
    static async Task Main(string[] args)
    {
        var ggmlType = GgmlType.LargeV3;
        using var speechFlow = await SpeechFlow.Create()
                                               .WithDefaultFrenchAudioRate(ggmlType)
                                               .WithEnglishTranslation()
                                               .OnTranscriptionCompleted((transcription) => Console.WriteLine($"Text transcrit: {transcription}"))
                                               .StartAsync();

        // Attendre que l'utilisateur appuie sur Entrée pour arrêter
        Console.WriteLine("Appuyez sur Entrée pour arrêter...");
        Console.ReadLine();
    }
}
