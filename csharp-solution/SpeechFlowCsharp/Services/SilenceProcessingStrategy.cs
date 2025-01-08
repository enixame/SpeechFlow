using System.Collections.Concurrent;
using SpeechFlowCsharp.Domain;

namespace SpeechFlowCsharp.Services
{
    /// <summary>
    /// Stratégie : on accumule de l'audio. 
    /// Tant que c'est de la "voix" (RMS > threshold), on continue.
    /// Dès qu'on détecte X frames silencieuses de suite, on envoie tout.
    /// </summary>
    public class SilenceProcessingStrategy : IAudioProcessingStrategy
    {
        private readonly MemoryStream _accumulator;
        private readonly float _silenceThreshold;
        private readonly int _frameSizeBytes;
        private readonly int _maxSilentFrames;
        private int _silentFrameCount;
        private int _chunkCounter;

        /// <summary>
        /// Si on choisit 200 ms par frame => frameSizeBytes = 16000*200/1000 * 2 *1 = 6400
        ///    int frameSamples = (16000 * 200) / 1000; // 3200
        ///    int frameSizeBytes = frameSamples * 2;   // 6400
        ///    float silenceThreshold = 0.01f;
        ///    int maxSilentFrames = 3;
        /// </summary>
        /// <param name="silenceThreshold"></param>
        /// <param name="frameSizeBytes"></param>
        /// <param name="maxSilentFrames"></param>
        public SilenceProcessingStrategy(float silenceThreshold, int frameSizeBytes, int maxSilentFrames)
        {
            _accumulator = new MemoryStream();
            _silenceThreshold = silenceThreshold;
            _frameSizeBytes = frameSizeBytes;
            _maxSilentFrames = maxSilentFrames;
            _silentFrameCount = 0;
            _chunkCounter = 0;
        }

        public void ProcessAudio(byte[] buffer, int byteCount, BlockingCollection<AudioChunk> chunkQueue)
        {
            _accumulator.Write(buffer, 0, byteCount);

            // Tant qu'on a assez pour analyser 1 frame
            while (_accumulator.Length >= _frameSizeBytes)
            {
                // Extraire la frame
                byte[] frameData = new byte[_frameSizeBytes];
                _accumulator.Position = 0;
                _accumulator.Read(frameData, 0, frameData.Length);

                // Calcul RMS
                float rms = CalculateRms(frameData);
                bool isSilent = rms < _silenceThreshold;
                if (isSilent)
                {
                    _silentFrameCount++;
                }
                else
                {
                    _silentFrameCount = 0;
                }

                // Gérer leftover
                int leftover = (int)(_accumulator.Length - _frameSizeBytes);
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

                // Si on a atteint maxSilentFrames => on envoie TOUT
                if (_silentFrameCount >= _maxSilentFrames)
                {
                    byte[] totalData = _accumulator.ToArray();
                    if (totalData.Length > 0 && !chunkQueue.IsAddingCompleted)
                    {
                        var chunk = new AudioChunk(++_chunkCounter, totalData);
                        chunkQueue.Add(chunk);
                    }
                    // reset
                    _accumulator.SetLength(0);
                    _accumulator.Position = 0;
                    _silentFrameCount = 0;
                }
            }
        }

        private static float CalculateRms(byte[] buffer)
        {
            // Convertir en float
            int sampleCount = buffer.Length / 2; // 16 bits
            double sum = 0;
            for (int i = 0; i < sampleCount; i++)
            {
                short sampleShort = BitConverter.ToInt16(buffer, i * 2);
                float sampleFloat = sampleShort / 32768f;
                sum += sampleFloat * sampleFloat;
            }
            double mean = sum / sampleCount;
            double rms = Math.Sqrt(mean);
            return (float)rms;
        }
    }
}
