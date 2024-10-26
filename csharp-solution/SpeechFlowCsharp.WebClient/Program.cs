var builder = WebApplication.CreateBuilder(args);

// Ajouter le support des fichiers statiques
builder.Services.AddControllersWithViews(); // Optionnel si tu veux utiliser des vues ou des contrôleurs

var app = builder.Build();

// Servir les fichiers statiques du dossier wwwroot
app.UseStaticFiles();

// Configurer pour retourner index.htm par défaut
app.MapFallbackToFile("index.htm");

app.Run();