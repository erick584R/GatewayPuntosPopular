using Microsoft.AspNetCore.SignalR;

namespace PuntosPopularWeb.Gateway.Middlewares
{
    public class NotificationSesionMiddleware : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var request = Context.GetHttpContext()?.Request!;
            var userId = request.Query["access_token"].ToString();

            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
                Console.WriteLine($"✅ Usuario '{userId}' conectado al Hub. ConnectionID: {Context.ConnectionId}");
            }
            else
            {
                Console.WriteLine($"⚠️ Conexión sin access_token. ConnectionID: {Context.ConnectionId}");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var request = Context.GetHttpContext()?.Request!;
            var userId = request.Query["access_token"].ToString();

            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
                Console.WriteLine($"❌ Usuario '{userId}' desconectado");
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task NotificarDispositivos(string userId)
        {
            Console.WriteLine($"📢 Notificando a otros dispositivos de {userId}");
            await Clients.OthersInGroup(userId).SendAsync(
                "RecibirNotificacion",
                "Se ha iniciado sesión desde otro dispositivo. Si no fuiste tú, " +
                "comunícate con el contact center para notificar este problema."
            );
        }
    }
}