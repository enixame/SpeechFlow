using Microsoft.AspNetCore.SignalR;
using Whisper.net.Ggml;

namespace SpeechFlowCsharp.SignalR
{
    /// <summary>
    /// TranscriptionHub est une classe SignalR Hub qui gère les connexions client en temps réel pour fournir des services de transcription.
    /// Elle démarre et arrête le service SpeechFlow en fonction du nombre de clients connectés, et utilise des mécanismes
    /// de synchronisation pour garantir la sécurité des threads.
    /// </summary>
    public class TranscriptionHub : Hub
    {
        // Compteur statique pour suivre le nombre de clients connectés au hub.
        // Utilisé pour démarrer ou arrêter le service de transcription en fonction des connexions actives.
        private static int _connectedClientsCount = 0;

        // Instance statique de SpeechFlow, utilisée pour gérer le service de transcription.
        // Cette instance est partagée entre toutes les connexions, donc des mécanismes de sécurité des threads sont nécessaires.
        private static IDisposable? _speechFlow;

        // Sémaphore pour gérer l'accès à l'instance de SpeechFlow et éviter les conflits de threads.
        private static readonly SemaphoreSlim _speechFlowSemaphore = new(1, 1);

        // Contexte du hub utilisé pour envoyer des messages aux clients connectés, même depuis des tâches asynchrones.
        private readonly IHubContext<TranscriptionHub> _hubContext;

        /// <summary>
        /// Constructeur du TranscriptionHub.
        /// Le constructeur injecte IHubContext, permettant d'envoyer des messages à tous les clients connectés
        /// de manière sécurisée et indépendante des méthodes de la classe Hub.
        /// </summary>
        /// <param name="hubContext">Le contexte du hub pour la communication avec les clients.</param>
        public TranscriptionHub(IHubContext<TranscriptionHub> hubContext)
        {
            _hubContext = hubContext;
        }

        /// <summary>
        /// Méthode appelée automatiquement par SignalR lorsque un client se connecte.
        /// Incrémente le compteur de clients, puis vérifie si le service SpeechFlow doit être démarré.
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            Console.WriteLine("Client connecté !");
            
            // Incrémente le compteur de clients de manière thread-safe.
            Interlocked.Increment(ref _connectedClientsCount);

            // Utilise un sémaphore pour s'assurer qu'une seule tâche à la fois peut démarrer ou arrêter SpeechFlow.
            await _speechFlowSemaphore.WaitAsync();
            try
            {
                // Si SpeechFlow n'est pas encore démarré, le démarrer.
                if (_speechFlow == null)
                {
                    try
                    {
                        var ggmlType = GgmlType.LargeV3Turbo; // Choisir le type GGML pour la transcription.
                        _speechFlow = await SpeechFlow.Create()
                                                    .WithDefaultFrenchAudioRate(ggmlType)
                                                    .OnTranscriptionCompleted(async (transcription) =>
                                                    {
                                                        // Utiliser IHubContext pour envoyer des messages à tous les clients connectés.
                                                        await _hubContext.Clients.All.SendAsync("ReceiveMessage", transcription);
                                                    })
                                                    .StartAsync();

                        Console.WriteLine("SpeechFlow démarré avec succès.");
                    }
                    catch (Exception ex)
                    {
                        // Gérer les erreurs lors du démarrage de SpeechFlow.
                        Console.WriteLine($"Erreur lors du démarrage de SpeechFlow : {ex.Message}");
                    }
                }
            }
            finally
            {
                // Relâcher le sémaphore, permettant à d'autres opérations de continuer.
                _speechFlowSemaphore.Release();
            }

            // Appeler la méthode de base pour continuer le traitement de connexion.
            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Méthode appelée automatiquement par SignalR lorsque un client se déconnecte.
        /// Décrémente le compteur de clients et arrête SpeechFlow s'il n'y a plus de clients connectés.
        /// </summary>
        /// <param name="exception">Exception qui a causé la déconnexion, le cas échéant.</param>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Console.WriteLine("Client déconnecté.");
            
            // Décrémente le compteur de clients de manière thread-safe.
            Interlocked.Decrement(ref _connectedClientsCount);

            // Utilise un sémaphore pour s'assurer qu'une seule tâche à la fois peut gérer la disposition de SpeechFlow.
            await _speechFlowSemaphore.WaitAsync();
            try
            {
                // Utilisation de CompareExchange pour vérifier le nombre actuel de clients connectés sans modification.
                int currentCount = Interlocked.CompareExchange(ref _connectedClientsCount, 0, 0);

                // Si aucun client n'est connecté, dispose de SpeechFlow pour libérer des ressources.
                if (currentCount <= 0 && _speechFlow != null)
                {
                    try
                    {
                        _speechFlow.Dispose(); // Dispose de l'instance de SpeechFlow.
                        _speechFlow = null;
                        Console.WriteLine("SpeechFlow est terminé car il n'y a plus de clients connectés.");
                    }
                    catch (Exception ex)
                    {
                        // Gérer les erreurs lors de la disposition de SpeechFlow.
                        Console.WriteLine($"Erreur lors de la disposition de SpeechFlow : {ex.Message}");
                    }
                }
            }
            finally
            {
                // Relâcher le sémaphore, permettant à d'autres opérations de continuer.
                _speechFlowSemaphore.Release();
            }

            // Appeler la méthode de base pour continuer le traitement de déconnexion.
            await base.OnDisconnectedAsync(exception);
        }
    }
}
