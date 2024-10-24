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
        public event EventHandler<short[]>? AudioCaptured;

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
        /// Convertit les données audio brutes (bytes) en un tableau de short (16 bits) et déclenche l'événement AudioCaptured.
        /// </summary>
        /// <param name="sender">L'émetteur de l'événement (le périphérique audio).</param>
        /// <param name="e">Contient les données audio capturées sous forme de tableau d'octets (bytes).</param>
        private void OnDataAvailable(object? sender, WaveInEventArgs e)
        {
            // Crée un tableau de short de la même taille que le buffer audio (chaque sample audio occupe 2 octets, d'où /2).
            short[] buffer = new short[e.Buffer.Length / 2];

            // Convertit les données brutes en tableau de short (int16), attendu par les étapes suivantes du traitement audio.
            Buffer.BlockCopy(e.Buffer, 0, buffer, 0, e.Buffer.Length);

            // Déclenche l'événement AudioCaptured pour permettre le traitement par d'autres composants.
            AudioCaptured?.Invoke(this, buffer);
        }
    }
}
