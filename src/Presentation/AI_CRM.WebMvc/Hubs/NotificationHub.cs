using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace AI_CRM.WebMvc.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task BroadcastNotification(string title, string message, string type)
        {
            await Clients.All.SendAsync("ReceiveNotification", new
            {
                title,
                message,
                type = string.IsNullOrEmpty(type) ? "info" : type,
                timestamp = System.DateTime.Now.ToString("HH:mm:ss")
            });
        }
    }
}
