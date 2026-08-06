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

var seguridadUrl = Environment.GetEnvironmentVariable("UrlBaseSeguridadPuntosPopularApi");

if (!string.IsNullOrWhiteSpace(seguridadUrl))
{
    builder.Configuration["ReverseProxy:Clusters:clusterSeguridadCorresponsalApi:Destinations:seguridadCorresponsalApi:Address"] = seguridadUrl;
}

// ✅ CORS CONFIGURADO CORRECTAMENTE
var allowedOrigins = Environment.GetEnvironmentVariable("UrlPuntosPopularTest");

var originsArray = string.IsNullOrWhiteSpace(allowedOrigins)
    ? new[]
    {
        "http://localhost:3000",
        "http://localhost:3001",
        "http://192.168.0.12:3000"
    }
    : allowedOrigins
        .Split("|", StringSplitOptions.RemoveEmptyEntries)
        .Append("http://192.168.0.12:3000")
        .Distinct()
        .ToArray();

Console.WriteLine($"🔐 CORS permitido para: {string.Join(", ", originsArray)}");

builder.Services.AddCors(options =>
{
    options.AddPolicy("Allow", policy =>
    {
        policy
            .WithOrigins(originsArray)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

app.UseRouting();

app.UseCors("Allow");

app.UseMiddleware<SessionMiddleware>();

app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ✅ MAPEAR HUB SIGNALR
app.MapHub<NotificationSesionMiddleware>(
    "/api/Notificaciones/v1/BancoPopular/inicio-sesion-corresponsal"
);

app.MapReverseProxy();

app.MapControllers();

/*// ✅ AGREGAR ESTO - Escuchar en HTTP en la IP de la red
app.Urls.Clear();
app.Urls.Add("http://0.0.0.0:5000");  // ← Escucha en todas las IPs en puerto 5000*/

app.Run();