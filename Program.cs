using PuntosPopularWeb.Gateway.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ✅ AGREGAR SIGNALR
builder.Services.AddSignalR();

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var seguridadUrl = Environment.GetEnvironmentVariable("UrlBaseSeguridadCorresponsalApi");

if (!string.IsNullOrWhiteSpace(seguridadUrl))
{
    builder.Configuration["ReverseProxy:Clusters:clusterSeguridadCorresponsalApi:Destinations:seguridadCorresponsalApi:Address"] = seguridadUrl;
}

// ✅ CORS CONFIGURADO CORRECTAMENTE
var allowedOrigins = Environment.GetEnvironmentVariable("UrlPuntosPopularTest");
var originsArray = string.IsNullOrWhiteSpace(allowedOrigins)
    ? new[] { "http://localhost:3000", "http://localhost:3001" }
    : allowedOrigins.Split("|", StringSplitOptions.RemoveEmptyEntries);

Console.WriteLine($"🔐 CORS permitido para: {string.Join(", ", originsArray)}");

builder.Services.AddCors(options =>
{
    options.AddPolicy("Allow", policy =>
    {
        policy
            .WithOrigins(originsArray)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // ✅ IMPORTANTE para WebSocket y SignalR
    });
});

var app = builder.Build();

app.UseRouting();
app.UseCors("Allow"); // ✅ CORS ANTES de mapear hubs
app.UseMiddleware<SessionMiddleware>();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ✅ MAPEAR HUB SIGNALR AQUÍ
app.MapHub<NotificationSesionMiddleware>(
    "/api/Notificaciones/v1/BancoPopular/inicio-sesion-corresponsal"
);

app.MapReverseProxy();
app.MapControllers();

app.Run();