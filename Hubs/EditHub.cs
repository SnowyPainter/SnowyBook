using Microsoft.AspNetCore.SignalR;

namespace SnowyBook.Hubs
{
    public class EditHub:Hub
    {
        public async Task SendContent(string content)
        {
            await Clients.All.SendAsync("SendContent", content);
        }
        public async Task Connected(string id) {
            await Clients.All.SendAsync("WhoConnected", id);
        }
    }
}
