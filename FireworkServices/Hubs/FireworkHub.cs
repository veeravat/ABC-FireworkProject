using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace FireworkServices
{
    public class FireworkSignalR : Hub
    {
        public async Task BroadcastMessage(string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", message);
        }

    }
}