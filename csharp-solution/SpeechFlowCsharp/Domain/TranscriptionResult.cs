namespace SpeechFlowCsharp.Domain
{
    /// <summary>
    /// Résultat brut de la transcription avant correction.
    /// </summary>
    public class TranscriptionResult
    {
        public int Id { get; }
        public string RawText { get; }

        public TranscriptionResult(int id, string rawText)
        {
            Id = id;
            RawText = rawText;
        }
    }
}
