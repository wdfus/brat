using System;
using System.Diagnostics;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;



namespace Brat
{
    public class WebSocketClient : IDisposable
    {
        public ClientWebSocket _client;
        private CancellationTokenSource _cts;

        public event Action<string>? MessageReceived;
        public event Action<string>? StatusChanged;

        public WebSocketClient()
        {
            _client = new ClientWebSocket();
            _cts = new CancellationTokenSource();
        }

        public async Task ConnectAsync(string url)
        {
            try
            {
                Uri uri = new Uri(url);
                string UserId = "UserId";
                var ClientId = new { UserId = MainWindow.Myid};
                string buffer = JsonSerializer.Serialize(ClientId);
                byte[] bytes = Encoding.UTF8.GetBytes(buffer);
                await _client.ConnectAsync(uri, _cts.Token);
                StatusChanged?.Invoke("Подключено к серверу WebSocket");
                await _client.SendAsync(bytes, WebSocketMessageType.Text, true, _cts.Token);
                StatusChanged?.Invoke("Id отправлен");


                // Запускаем приём сообщений
                _ = ReceiveLoop();
            }
            catch (Exception ex)
            {
                StatusChanged?.Invoke($"Ошибка подключения: {ex.Message}");
            }
        }

        private async Task ReceiveLoop()
        {
            var buffer = new byte[4096];

            while (_client.State == WebSocketState.Open)
            {
                try
                {
                    var messageBuffer = new List<byte>();

                    WebSocketReceiveResult result;
                    do
                    {
                        result = await _client.ReceiveAsync(new ArraySegment<byte>(buffer), _cts.Token);
                        messageBuffer.AddRange(buffer.Take(result.Count));


                    } while (!result.EndOfMessage);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await _client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Закрыто сервером", _cts.Token);
                        StatusChanged?.Invoke("Соединение закрыто сервером");
                        break;
                    }

                    string message = Encoding.UTF8.GetString(messageBuffer.ToArray());
                    Debug.WriteLine(message);
                    if (message.TrimStart().StartsWith("{"))
                    {
                        MessageReceived?.Invoke(message);
                    }
                    else
                    {
                        Debug.WriteLine($"Неподдерживаемое сообщение: {message}");
                }

            }
                catch (Exception ex)
                {
                    StatusChanged?.Invoke($"Ошибка приёмника: {ex}");
                    break;
                }
            }
        }


        public async Task SendMessageAsync(string message)
        {
            if (_client.State != WebSocketState.Open) return;

            var buffer = Encoding.UTF8.GetBytes(message);
            await _client.SendAsync(buffer, WebSocketMessageType.Text, true, _cts.Token);
        }

        public void Dispose()
        {
            _cts.Cancel();
            _client.Dispose();
        }


        public async Task CloseWebSocketAsync(ClientWebSocket wsClient)
        {
            if (wsClient.State == WebSocketState.Open)
            {
                // Отправляем Close Frame серверу
                await wsClient.CloseAsync(
                    WebSocketCloseStatus.NormalClosure, // статус закрытия
                    "Клиент завершил соединение",       // описание
                    CancellationToken.None
                );

                Console.WriteLine("WebSocket корректно закрыт");
            }
            else
            {
                Console.WriteLine("WebSocket уже закрыт или не подключён");
            }
        }

        public static string GetLocalIPv4()
        {
            string ipAddress = string.Empty;
            IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in ipHost.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    ipAddress = ip.ToString();
                    break;
                }
            }
            return ipAddress;
        }
    }
}

