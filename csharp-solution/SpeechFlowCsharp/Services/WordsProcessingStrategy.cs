using System.Collections.Concurrent;
using SpeechFlowCsharp.Domain;

namespace SpeechFlowCsharp.Services
{
    /// <summary>
    /// Stratégie : on découpe simplement l'audio en "mini-chunks" de taille fixe
    /// (ex. 200 ms). Le "count de mots" se fera dans la classe RealTimeTranscriber
    /// en accumulant la transcription.
    /// </summary>
    public class WordsProcessingStrategy : IAudioProcessingStrategy
    {
        private readonly int _miniChunkSizeBytes;
        private readonly MemoryStream _accumulator;
        private int _chunkCounter;

        /// <summary>
        /// Si on envoie de petits mini-chunks de 300 ms => 16000*300/1000=4800 samples => 9600 octets
        ///    int miniChunkSamples = (16000 * 300) / 1000; // 4800
        ///    int miniChunkSizeBytes = miniChunkSamples * 2; // 9600
        ///    int maxWords = 10; // ex: 10 mots
        /// </summary>
        /// <param name="miniChunkSizeBytes"></param>
        public WordsProcessingStrategy(int miniChunkSizeBytes)
        {
            _miniChunkSizeBytes = miniChunkSizeBytes;
            _accumulator = new MemoryStream();
            _chunkCounter = 0;
        }

        public void ProcessAudio(byte[] buffer, int byteCount, BlockingCollection<AudioChunk> chunkQueue)
        {
            _accumulator.Write(buffer, 0, byteCount);

            while (_accumulator.Length >= _miniChunkSizeBytes)
            {
                byte[] miniChunk = new byte[_miniChunkSizeBytes];
                _accumulator.Position = 0;
                _accumulator.Read(miniChunk, 0, miniChunk.Length);

                if (!chunkQueue.IsAddingCompleted)
                {
                    var chunk = new AudioChunk(++_chunkCounter, miniChunk);
                    chunkQueue.Add(chunk);
                }

                int leftover = (int)(_accumulator.Length - _miniChunkSizeBytes);
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
