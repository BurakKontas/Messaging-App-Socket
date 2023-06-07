using Microsoft.AspNetCore.SignalR;
using System.Text;
using System.Text.Json;

namespace SAT_Socket
{
    public class ChatHub : Hub
    {
        private string id = "";

        public override async Task OnConnectedAsync()
        {
            var token = Context.GetHttpContext()?.Request.Query["access_token"].FirstOrDefault()?.ToString();
            if (string.IsNullOrEmpty(token))
            {
                Context.Abort();
                return;
            }

            var httpClient = new HttpClient();
            var verifyTokenUrl = "http://localhost:3535/verify_token"; // Doğrulama endpointinin URL'sini buraya girin

            var request = new HttpRequestMessage(HttpMethod.Get, verifyTokenUrl);
            request.Headers.Add("Authorization", token);

            var response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var decodedToken = JsonDocument.Parse(responseContent).RootElement.GetProperty("decoded_token");
                id = decodedToken.GetProperty("id").ToString();

                string connectionId = Context.ConnectionId;
                if(Users.ContainsKey(id))
                {
                    Users.Remove(id);
                    Users.Add(id, connectionId);
                }

                await base.OnConnectedAsync();
            }
            else
            {
                Context.Abort();
                await base.OnConnectedAsync();
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            Users.Remove(id);

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string user, string message)
        {
            Users.TryGetValue(user, out var connectionId);
            if (connectionId != null)
            {
                await Clients.Client(connectionId).SendAsync("ReceiveMessage", message);
            }
        }

        private static Dictionary<string,string> Users = new();
    }
}
