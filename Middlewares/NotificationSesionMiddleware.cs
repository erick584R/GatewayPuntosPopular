using Microsoft.AspNetCore.SignalR;

namespace PuntosPopularWeb.Gateway.Middlewares
{
    /// <summary>
    /// Hub de SignalR para notificaciones de sesión en tiempo real
    /// Permite comunicación bidireccional entre dispositivos del mismo usuario
    /// </summary>
    public class NotificationSesionMiddleware : Hub
    {
        // Se ejecuta cuando un cliente se CONECTA
        public override async Task OnConnectedAsync()
        {
            var request = Context.GetHttpContext()?.Request!;

            // Obtener el usuario del query string: ?access_token=usuario123
            var userId = request.Query["access_token"].ToString();

            if (!string.IsNullOrEmpty(userId))
            {
                // Agregar esta conexión al grupo del usuario
                // Todos los dispositivos del usuario estarán en el mismo grupo
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
                Console.WriteLine($"✅ Usuario '{userId}' conectado. ConexionID: {Context.ConnectionId}");
            }

            await base.OnConnectedAsync();
        }

        // Se ejecuta cuando un cliente se DESCONECTA
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var request = Context.GetHttpContext()?.Request!;
            var userId = request.Query["access_token"].ToString();

            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
                Console.WriteLine($"❌ Usuario '{userId}' desconectado. ConexionID: {Context.ConnectionId}");
            }

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Notifica a TODOS LOS DEMÁS dispositivos del usuario
        /// Se usa cuando:
        /// - Código 27: Hay sesión activa en otro dispositivo (advertencia)
        /// - Código 0 con sesión activa: Se cierran otros dispositivos automáticamente
        /// </summary>
        public async Task NotificarDispositivos(string userId)
        {
            // OthersInGroup = Envía a TODOS en el grupo EXCEPTO quien lo invocó
            await Clients.OthersInGroup(userId).SendAsync(
                "RecibirNotificacion",
                "Se ha iniciado sesión desde otro dispositivo. Si no fuiste tú, " +
                "comunícate con el contact center para notificar este problema."
            );
        }
    }
}