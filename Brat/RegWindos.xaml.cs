using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Brat
{
    /// <summary>
    /// Логика взаимодействия для RegWindos.xaml
    /// </summary>
    public partial class RegWindos : Window
    {
        private IHost? _webHost;
        public RegWindos()
        {
            InitializeComponent();
        }

        private void OpenSite_Click(object sender, RoutedEventArgs e)
        {
            LoadUrl();
        }

        async Task<string> LoadUrl()
        {
            using var client = new HttpClient();

            // URL твоего API
            string url = "http://172.20.10.6:8000/desktop/auth/start";

            // Тело запроса (JSON)
            var json = "{\"username\": \"brat\", \"password\": \"1234\"}";

            // Упаковываем тело в HTTP-контент
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Отправляем POST-запрос
            HttpResponseMessage response = await client.PostAsync(url, content);

            // Читаем ответ как текст
            string responseText = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // чтобы не зависеть от регистра имён
            };

            // Вариант 1 — если хочешь использовать анонимный объект


            // Проверяем успешность
            if (response.IsSuccessStatusCode)
            {

                var doc = JsonDocument.Parse(json);
                Debug.WriteLine("✅ Успех! Ответ сервера: " + responseText);
                string LoginUrl = doc.RootElement.GetProperty("login_url").GetString();
                Process.Start(new ProcessStartInfo
                {
                    FileName = LoginUrl,
                    UseShellExecute = true // ← обязательно, иначе не откроется в .NET 5+
                });
                return responseText;
            }
            else
            {
                Debug.WriteLine($"❌ Ошибка {response}: {responseText}");
                return null;
            }
        }
    }
}
