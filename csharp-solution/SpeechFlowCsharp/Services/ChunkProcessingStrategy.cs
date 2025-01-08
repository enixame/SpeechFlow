using System.Collections.Concurrent;
using SpeechFlowCsharp.Domain;

namespace SpeechFlowCsharp.Services
{
    /// <summary>
    /// Stratégie : on découpe l'audio en chunks de taille fixe (en octets).
    /// </summary>
    public class ChunkProcessingStrategy : IAudioProcessingStrategy
    {
        private readonly int _chunkSizeBytes;
        private readonly MemoryStream _accumulator;
        private int _chunkCounter;

        /// <summary>
        ///  Supposons qu'on veut des chunks de 1 seconde (1000 ms) à 16 kHz, mono
        ///    int samplesPerChunk = (16000 * 1000) / 1000; // 16000
        ///    int chunkSizeBytes = samplesPerChunk * 2 * 1; // 16 bits = 2 octets, 1 canal
        /// </summary>
        /// <param name="chunkSizeBytes"></param>
        public ChunkProcessingStrategy(int chunkSizeBytes)
        {
            _chunkSizeBytes = chunkSizeBytes;
            _accumulator = new MemoryStream();
            _chunkCounter = 0;
        }

        public void ProcessAudio(byte[] buffer, int byteCount, BlockingCollection<AudioChunk> chunkQueue)
        {
            // Accumuler
            _accumulator.Write(buffer, 0, byteCount);

            // Tant qu'on a assez pour un chunk
            while (_accumulator.Length >= _chunkSizeBytes)
            {
                byte[] chunkData = new byte[_chunkSizeBytes];
                _accumulator.Position = 0;
                _accumulator.Read(chunkData, 0, chunkData.Length);

                // Envoyer dans la queue
                if (!chunkQueue.IsAddingCompleted)
                {
                    var chunk = new AudioChunk(++_chunkCounter, chunkData);
                    chunkQueue.Add(chunk);
                }

                // Gérer leftover
                int leftover = (int)(_accumulator.Length - _chunkSizeBytes);
                if (leftover > 0)
                {
                    byte[] leftoverData = new byte[leftover];
                    _accumulator.Read(leftoverData, 0, leftover);
                    _accumulator.SetLength(0);
                    _accumulator.Position = 0;
                    _accumulator.Write(leftoverData, 0, leftover);
                }
                else
                {
                    _accumulator.SetLength(0);
                }
                _accumulator.Position = _accumulator.Length;
            }
        }
    }
}
