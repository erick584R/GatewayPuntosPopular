using PuntosPopularWeb.Gateway.DTOs;
using System.Net;
using System.Text;
using System.Text.Json;

namespace PuntosPopularWeb.Gateway.Middlewares
{
    public class SessionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string[] publicPaths;
        private readonly IConfiguration _configuration;

        public SessionMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;

            // ✅ EXCLUIR RUTAS PÚBLICAS Y SIGNALR
            publicPaths = new[]
            {
                _configuration.GetSection("auth_path").GetRequiredSection("login").Value!,
                "/api/Notificaciones/v1/BancoPopular/inicio-sesion-corresponsal" // ✅ SIGNALR NEGOCIACIÓN
            };
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.ToString();

            // ✅ Si es ruta pública O es WebSocket, NO validar sesión
            if (publicPaths.Any(x => path.Contains(x, StringComparison.OrdinalIgnoreCase)) ||
                context.WebSockets.IsWebSocketRequest)
            {
                Console.WriteLine($"✅ Ruta pública/WebSocket: {path} - Sin validar sesión");
                await _next(context);
                return;
            }

            // Si no existe validar-sesion, no bloquear
            var validatePath = _configuration.GetSection("auth_path").GetSection("validate").Value;

            if (string.IsNullOrWhiteSpace(validatePath))
            {
                await _next(context);
                return;
            }

            context.Request.EnableBuffering();

            using var reader = new StreamReader(
                context.Request.Body,
                encoding: Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                bufferSize: 1024,
                leaveOpen: true);

            var body = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;

            if (!string.IsNullOrWhiteSpace(body))
            {
                try
                {
                    var root = JsonSerializer.Deserialize<RootRequest>(body, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (root?.bpInReq != null)
                    {
                        using HttpClient client = new HttpClient();

                        string bodystring = JsonSerializer.Serialize(root);
                        StringContent content = new StringContent(bodystring, Encoding.UTF8, "application/json");

                        var responseValidation = await client.PostAsync(
                            Environment.GetEnvironmentVariable("UrlBaseSeguridadCorresponsalApi") + validatePath,
                            content
                        );

                        if (responseValidation.IsSuccessStatusCode ||
                            (responseValidation.StatusCode == HttpStatusCode.BadRequest && responseValidation.Content != null))
                        {
                            string contentJson = await responseValidation.Content.ReadAsStringAsync();

                            var validation = JsonSerializer.Deserialize<RootResponse>(contentJson, new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });

                            if (validation != null &&
                                validation.bpOutReq != null &&
                                validation.bpOutReq.CodigoError == "0")
                            {
                                await _next(context);
                                return;
                            }

                            context.Response.StatusCode = StatusCodes.Status200OK;
                            context.Response.ContentType = "application/json";
                            await context.Response.WriteAsync(contentJson);
                            return;
                        }
                    }
                }
                catch
                {
                    // Manejar excepción silenciosamente
                }
            }

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        }
    }
}