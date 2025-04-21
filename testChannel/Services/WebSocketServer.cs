using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace testChannel.Services
{
    public class WebSocketServer
    {
        private readonly RequestDelegate _next;
        private static readonly List<WebSocket> _sockets = [];
        private static readonly Dictionary<string, WebSocket> _clients = new();

        public WebSocketServer(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                _sockets.Add(webSocket);
                await HandleWebSocketConnectionClients(webSocket);
            }
            else
            {
                await _next(context);
            }
        }
        private async Task HandleWebSocketConnectionClients(WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            string? username = null;

            while (true)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.CloseStatus.HasValue)
                {
                    if (username != null)
                    {
                        _clients.Remove(username);
                    }
                    break;
                }

                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
               
                var data = JsonSerializer.Deserialize<Dictionary<string, string>>(message);

                if (data != null && data.ContainsKey("type"))
                {
                    if (data["type"] == "register" && data.ContainsKey("username"))
                    {
                        username = data["username"];
                        _clients[username] = webSocket;
                        Console.WriteLine($"User registered: {username}");
                    }
                    else if (data["type"] == "message" && data.ContainsKey("to") && data.ContainsKey("message"))
                    {
                        var recipient = data["to"];
                        var msg = data["message"];

                        if (_clients.TryGetValue(recipient, out var recipientSocket) && recipientSocket.State == WebSocketState.Open)
                        {
                            var response = JsonSerializer.Serialize(new
                            {
                                type = "message",
                                from = username,
                                message = msg
                            });

                            var responseBuffer = Encoding.UTF8.GetBytes(response);
                            await recipientSocket.SendAsync(new ArraySegment<byte>(responseBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
                        }
                    }
                }
            }

            if (username != null)
            {
                _clients.Remove(username);
            }

            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed", CancellationToken.None);
        }
        protected async Task HandleWebSocketConnection(WebSocket webSocket)
        {
            var buffer = new byte[1024 * 100];
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            while (!result.CloseStatus.HasValue)
            {
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                await BroadcastMessageAsync(message);
              
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }

            _sockets.Remove(webSocket);
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }

        // Método público para enviar mensajes a todos los clientes conectados
        public static async Task BroadcastMessageAsync(string message)
        {
            var buffer = Encoding.UTF8.GetBytes(message);
            var tasks = _sockets
                .Where(socket => socket.State == WebSocketState.Open)
                .Select(socket => socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None));

            await Task.WhenAll(tasks);
        }
    }
}
