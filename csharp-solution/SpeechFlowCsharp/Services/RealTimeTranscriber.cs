using System.Collections.Concurrent;
using CSCore;
using CSCore.CoreAudioAPI;
using CSCore.SoundIn;
using SpeechFlowCsharp.Domain;
using Whisper.net;

namespace SpeechFlowCsharp.Services
{
    /// <summary>
    /// Classe principale : capture l'audio, l'envoie à la stratégie de découpage,
    /// puis transcrit chaque chunk via Whisper.Net, applique une correction,
    /// et déclenche OnFinalTextReady.
    /// 
    /// Gère également la logique "word-based" (compteur de mots).
    /// </summary>
    public class RealTimeTranscriber : IDisposable
    {
        private WasapiCapture? _capture;
        private readonly IAudioProcessingStrategy _processingStrategy;
        private readonly WhisperFactory _whisperFactory;
        private WhisperProcessorBuilder? _whisperBuilder;
        private WhisperProcessor? _whisperProcessor;

        private BlockingCollection<AudioChunk> _chunkQueue;
        private Thread? _transcriptionThread;
        private bool _isRunning;

        // On stocke les segments transcrits (pour le chunk en cours)
        private ConcurrentQueue<SegmentData> _currentSegments = new();

        private readonly AudioProcessingMode _mode;
        private readonly int _sampleRate;
        private readonly int _channelCount;

        // Evénement final
        public event Action<string>? OnFinalTextReady;

        // Pour le "Words" mode : on accumule le texte transcrit
        private string _wordBuffer = "";
        private readonly int _wordMaxCount;

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="strategy">Stratégie de découpage audio (Chunk, Silence ou mini-chunk Words).</param>
        /// <param name="mode">Mode choisi (pour savoir si on doit compter les mots ou pas).</param>
        /// <param name="modelPath">Chemin du modèle Whisper.</param>
        /// <param name="sampleRate">Fréquence (ex: 16000).</param>
        /// <param name="channelCount">Canaux (ex: 1 = mono).</param>
        /// <param name="wordMaxCount">Nombre de mots cible pour le mode Words.</param>
        public RealTimeTranscriber(
            IAudioProcessingStrategy strategy,
            AudioProcessingMode mode,
            string modelPath,
            int sampleRate = 16000,
            int channelCount = 1,
            int wordMaxCount = 10
        )
        {
            _processingStrategy = strategy;
            _mode = mode;
            _wordMaxCount = wordMaxCount;

            _sampleRate = sampleRate;
            _channelCount = channelCount;

            // Whisper
            _whisperFactory = WhisperFactory.FromPath(modelPath);
            _whisperBuilder = _whisperFactory.CreateBuilder()
                                             .WithLanguage("fr")
                                             //.WithThreads(4) // optionnel, à tester
                                             .WithSegmentEventHandler(OnSegmentReceived);;

            // Queue
            _chunkQueue = new BlockingCollection<AudioChunk>(boundedCapacity: 50);
        }

        /// <summary>
        /// Démarre la capture et le pipeline.
        /// </summary>
        public void Start()
        {
            if (_isRunning) return;
            _isRunning = true;

            // Whisper
            _whisperProcessor = _whisperBuilder?.Build();

            var desiredFormat = new WaveFormat(_sampleRate, 16, _channelCount);

            // CSCore capture
            _capture = new WasapiCapture(
                eventSync: false,
                shareMode: AudioClientShareMode.Shared,
                latency: 100,
                defaultFormat: desiredFormat
            );
            _capture.Initialize();
            _capture.DataAvailable += OnDataAvailable;
            _capture.Start();

            // Thread transcription
            _transcriptionThread = new Thread(TranscriptionWorker);
            _transcriptionThread.Start();

            Console.WriteLine("[RealTimeTranscriber] Started.");
        }

        /// <summary>
        /// Arrête tout.
        /// </summary>
        public void Stop()
        {
            if (!_isRunning) return;
            _isRunning = false;

            _capture?.Stop();
            _capture?.Dispose();
            _capture = null;

            _chunkQueue.CompleteAdding();
            _transcriptionThread?.Join();

            _whisperProcessor?.Dispose();
            _whisperBuilder = null;

            Console.WriteLine("[RealTimeTranscriber] Stopped.");
        }

        /// <summary>
        /// Callback quand on reçoit du PCM brut. 
        /// On délègue le découpage à la stratégie choisie.
        /// </summary>
        private void OnDataAvailable(object? sender, DataAvailableEventArgs? e)
        {
            if (e?.Data == null) 
            {
                return;
            }

            _processingStrategy.ProcessAudio(e.Data, e.ByteCount, _chunkQueue);
        }

        /// <summary>
        /// Thread qui lit les AudioChunk dans la queue, appelle Whisper.Net,
        /// applique un post-traitement, et déclenche l'événement final.
        /// 
        /// Si on est en mode "Words", on accumule le texte jusqu'à X mots avant de déclencher l'événement.
        /// </summary>
        private void TranscriptionWorker()
        {
            while (!_chunkQueue.IsCompleted)
            {
                AudioChunk? chunk = null;
                try
                {
                    chunk = _chunkQueue.Take();
                }
                catch (InvalidOperationException)
                {
                    break;
                }
                if (chunk == null || chunk.Data.Length == 0)
                    continue;

                // Transcription

                // On réinitialise la liste de segments pour ce chunk
                _currentSegments.Clear();

                // 1) Convertir le chunk en float[]
                float[] floatBuffer = BytesToFloatArray(chunk.Data);

                // 2) Appel synchrone => *les segments arrivent au fur et à mesure* dans OnSegmentReceived
                _whisperProcessor?.Process(floatBuffer);

                // 3) Après Process(...), on concatène tous les segments accumulés pour ce chunk
                var segmentsList = new List<SegmentData>();
                while (_currentSegments.TryDequeue(out var segment))
                {
                    segmentsList.Add(segment);
                }
                
                string rawText = string.Concat(segmentsList.Select(s => s.Text));
                if (string.IsNullOrWhiteSpace(rawText))
                    continue;

                // Correction minimaliste
                string corrected = PostProcessText(rawText);

                if (_mode == AudioProcessingMode.Words)
                {
                    // Accumuler pour atteindre X mots
                    _wordBuffer += " " + corrected;
                    int currentWordCount = CountWords(_wordBuffer);
                    if (currentWordCount >= _wordMaxCount)
                    {
                        // Déclenche l'événement
                        OnFinalTextReady?.Invoke(_wordBuffer.Trim());
                        // Reset
                        _wordBuffer = "";
                    }
                }
                else
                {
                    // Chunk ou Silence => on déclenche direct
                    OnFinalTextReady?.Invoke(corrected);
                }
            }
        }

        // Cet event handler est appelé *à chaque segment* produit par Whisper.
        private void OnSegmentReceived(SegmentData segment)
        {
            // On accumule ce segment dans la file courante.
           _currentSegments.Enqueue(segment);
        }

        // Utilitaires

        // Conversion PCM16 -> float[-1..1]
        private static float[] BytesToFloatArray(byte[] buffer)
        {
            int sampleCount = buffer.Length / 2; // 16 bits
            float[] floatBuffer = new float[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                short sampleShort = BitConverter.ToInt16(buffer, i * 2);
                floatBuffer[i] = sampleShort / 32768f;
            }
            return floatBuffer;
        }

        private static string PostProcessText(string input)
        {
            string cleaned = input.Replace("  ", " ").Trim();
            if (cleaned.Length > 1)
            {
                cleaned = char.ToUpper(cleaned[0]) + cleaned.Substring(1);
            }
            return cleaned;
        }

        private static int CountWords(string text)
        {
            var parts = text.Split(new[] { ' ', '\t', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return parts.Length;
        }

        public void Dispose()
        {
            Stop();
            _chunkQueue?.Dispose();
            _whisperProcessor?.Dispose();
        }
    }
}
