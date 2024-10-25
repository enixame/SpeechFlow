using NAudio.Wave;

namespace SpeechFlowCsharp.AudioProcessing
{
    /// <summary>
    /// Classe responsable de la capture audio en temps réel à partir du microphone.
    /// Utilise la bibliothèque NAudio pour accéder au périphérique audio et capturer les données audio.
    /// </summary>
    public sealed class AudioCapturer : IAudioCapturer
    {
        private IWaveIn? _waveIn;

        private WaveFormat? _waveFormat;

        public bool IsCapturing { get; private set; }

        
        public AudioCapturer(WaveFormat waveFormat)
        {
            _waveFormat = waveFormat ?? throw new ArgumentNullException(nameof(waveFormat));
        }

        public AudioCapturer()
            : this(new WaveFormat(16000, 1)) // Définit le format audio : 16 kHz en mono (1 canal) par défaut.
        {
        }

        /// <summary>
        /// Événement déclenché lorsqu'un nouveau lot de données audio est disponible.
        /// Les abonnés peuvent utiliser ces données pour traitement supplémentaire.
        /// </summary>
        public event EventHandler<float[]>? AudioCaptured;

        /// <summary>
        /// Démarre la capture audio en temps réel depuis le périphérique microphone.
        /// </summary>
        public void StartCapture()
        {
            // Initialise un événement WaveIn pour capturer l'audio à partir du microphone.
            _waveIn = new WaveInEvent
            {
                WaveFormat = _waveFormat
            };

            // Abonne l'événement OnDataAvailable pour traiter les données capturées.
            _waveIn.DataAvailable += OnDataAvailable;

            // Démarre la capture audio.
            _waveIn.StartRecording();

            IsCapturing = true;
        }

        /// <summary>
        /// Arrête la capture audio et libère les ressources associées.
        /// </summary>
        public void StopCapture()
        {
            if (_waveIn != null)
            {
                // Désabonnement de l'événement pour éviter des fuites
                _waveIn.DataAvailable -= OnDataAvailable;

                // Libération des ressources
                _waveIn.StopRecording();
                _waveIn.Dispose();
                _waveIn = null;
            }
            IsCapturing = false;
        }

        /// <summary>
        /// Méthode appelée lorsque des données audio sont disponibles.
        /// Convertit les données audio brutes (bytes) en un tableau de float et déclenche l'événement AudioCaptured.
        /// </summary>
        private void OnDataAvailable(object? sender, WaveInEventArgs e)
        {
            // Crée un tableau de float pour les données audio (chaque sample audio occupe 2 octets, d'où /2).
            float[] buffer = new float[e.Buffer.Length / 2];

            // Convertit les données brutes en float
            for (int i = 0; i < buffer.Length; i++)
            {
                // Lit chaque sample en tant que short et le normalise en float entre -1.0f et 1.0f
                buffer[i] = BitConverter.ToInt16(e.Buffer, i * 2) / 32768f;
            }

            // Déclenche l'événement AudioCaptured pour permettre le traitement par d'autres composants.
            AudioCaptured?.Invoke(this, buffer);
        }
    }
}
