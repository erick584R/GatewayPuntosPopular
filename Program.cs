using PuntosPopularWeb.Gateway.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
              .AllowAnyMethod());
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

app.MapReverseProxy();
app.MapControllers();

app.Run();