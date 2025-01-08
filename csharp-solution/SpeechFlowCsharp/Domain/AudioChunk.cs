namespace SpeechFlowCsharp.Domain
{
    /// <summary>
    /// Représente un bloc (chunk) de données PCM.
    /// </summary>
    public sealed class AudioChunk
    {
        public int Id { get; }
        public byte[] Data { get; }

        public AudioChunk(int id, byte[] data)
        {
            Id = id;
            Data = data;
        }
    }
}
