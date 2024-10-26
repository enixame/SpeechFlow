using Microsoft.AspNetCore.SignalR;
using Whisper.net.Ggml;

namespace SpeechFlowCsharp.SignalR
{
    public class TranscriptionHub : Hub
    {
        private static int _connectedClientsCount = 0; // Compteur de clients connecté

        private static IDisposable? _speechFlow;

        private readonly IHubContext<TranscriptionHub> _hubContext;

        public TranscriptionHub(IHubContext<TranscriptionHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public override async Task OnConnectedAsync()
        {
            Console.WriteLine("Client connecté !");
            _connectedClientsCount++;

            // Démarrer SpeechFlow si ce n'est pas encore fait
            if (_speechFlow == null)
            {
                try
                {
                    var ggmlType = GgmlType.Medium; // Ajuster selon le besoin
                    _speechFlow = await SpeechFlow.Create()
                                                .WithDefaultFrenchAudioRate(ggmlType)
                                                .OnTranscriptionCompleted(async (transcription) =>
                                                {
                                                    // Utiliser IHubContext pour envoyer des messages
                                                    await _hubContext.Clients.All.SendAsync("ReceiveMessage", transcription);
                                                })
                                                .StartAsync();

                    Console.WriteLine("SpeechFlow démarré avec succès.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur lors du démarrage de SpeechFlow : {ex.Message}");
                }
            }

            await base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            Console.WriteLine("Client déconnecté.");

            _connectedClientsCount--;
     
            // Si aucun client n'est connecté, disposer de SpeechFlow
            if (_connectedClientsCount <= 0 && _speechFlow != null)
            {
                try
                {
                    _speechFlow.Dispose(); // Dispose de SpeechFlow
                    _speechFlow = null;
                    Console.WriteLine("SpeechFlow est terminé car il n'y a plus de clients connectés.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur lors de la disposition de SpeechFlow : {ex.Message}");
                }
            }

            return base.OnDisconnectedAsync(exception);
        }
    }
}