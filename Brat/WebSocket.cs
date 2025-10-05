using System;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;



namespace Brat
{
    public class WebSocketClient : IDisposable
    {
        private ClientWebSocket _client;
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
                    var result = await _client.ReceiveAsync(buffer, _cts.Token);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await _client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Закрыто сервером", _cts.Token);
                        StatusChanged?.Invoke("Соединение закрыто сервером");
                    }
                    else
                    {
                        string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        MessageReceived?.Invoke(message);
                    }
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
    }
}

