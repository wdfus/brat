using System;
using System.Diagnostics;
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
                    using var doc = JsonDocument.Parse(message);
                    int fromUserId = doc.RootElement.GetProperty("from_user_id").GetInt32();
                    string text = doc.RootElement.GetProperty("message_text").GetString();
                    Debug.WriteLine($"Получено сообщение: {text}");
                    Debug.WriteLine($"Получен ID: {fromUserId}");
                    MessageReceived?.Invoke(message);
                }
                catch (Exception ex)
                {
                    StatusChanged?.Invoke($"Ошибка приёмника: {ex.Message}");
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
    }
}

