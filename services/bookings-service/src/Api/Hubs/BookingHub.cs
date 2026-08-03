using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace BookingService.Api.Hubs
{
    public class BookingHub : Hub
    {
        public Task SendStatusUpdate(object statusDto) => Clients.All.SendAsync("BookingStatusChanged", statusDto);
    }
}
