namespace SpeechFlowCsharp.SignalR
{
    public class SignalRServer
    {
        private readonly string? _url;
        private IHost? _host;

        public SignalRServer(string? url)
        {
            _url = url;
        }

        public void Start()
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Debug); // Pour plus de détails dans les logs
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseUrls(_url!);
                    webBuilder.ConfigureServices(services =>
                    {
                        services.AddSignalR();
                        services.AddCors(options =>
                        {
                            options.AddDefaultPolicy(policy =>
                            {
                                policy.AllowAnyHeader()
                                    .AllowAnyMethod()
                                    .AllowCredentials()
                                    .SetIsOriginAllowed(_ => true);
                            });
                        });
                    });

                    webBuilder.Configure(app =>
                    {
                        app.UseRouting();
                        app.UseCors();
                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapHub<TranscriptionHub>("/transcriptionHub");
                        });
                    });
                })
                .Build();

            _host.Run();
        }

        public void Stop()
        {
            if(_host != null)
            {
                _host.StopAsync().Wait();
                Console.WriteLine("Serveur arrêté.");
            }
        }
    }
}