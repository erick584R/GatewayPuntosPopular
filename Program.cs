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

builder.Services.AddCors(options =>
{
    options.AddPolicy("Allow", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()); // ✅ IMPORTANTE para WebSocket
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

app.Run();