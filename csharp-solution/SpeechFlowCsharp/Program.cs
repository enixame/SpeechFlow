using SpeechFlowCsharp;
using Whisper.net.Ggml;

class Program
{
    static async Task Main(string[] args)
    {
        var ggmlType = GgmlType.Base;
        using var speechFlow = await SpeechFlow.Create()
                                               .WithDefaultFrenchAudioRate(ggmlType)
                                               //.OnFilterText((text) => text.StartsWith(" Sous-titrage") || text.Equals(" Merci."))
                                               .OnTranscriptionCompleted((transcription) => Console.WriteLine($"Text transcrit: {transcription}"))
                                               .StartAsync();

        // Attendre que l'utilisateur appuie sur Entrée pour arrêter
        Console.WriteLine("Appuyez sur Entrée pour arrêter...");
        Console.ReadLine();
    }
}
