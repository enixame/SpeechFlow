namespace SpeechFlowCsharp.SignalR
{
    class Program
    {
        static void Main(string[] args)
        {
            string serverUrl = "http://0.0.0.0:5000";
            var signalRServer = new SignalRServer(serverUrl);

            // Démarrer le serveur SignalR
            signalRServer.Start();
            
            // Arrêter le serveur proprement
            signalRServer.Stop();
        }
    }
}