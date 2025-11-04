using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Media.Imaging;
using Renci.SshNet;

public static class SftpImageLoader
{
    public static BitmapImage LoadImageFromSftp(string host, string username, string password, string remoteFilePath)
    {
        try
        {
            using (var client = new SftpClient(host, username, password))
            {
                client.Connect();

                if (!client.Exists(remoteFilePath))
                    throw new FileNotFoundException($"Файл не найден на SFTP: {remoteFilePath}");

                using (var ms = new MemoryStream())
                {
                    // Скачиваем файл в поток
                    client.DownloadFile(remoteFilePath, ms);
                    Debug.WriteLine("Файл скачан");
                    ms.Position = 0;

                    // Создаём BitmapImage из потока
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = ms;
                    image.EndInit();
                    image.Freeze(); // Чтобы можно было безопасно использовать в UI-потоке

                    return image;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Ошибка при загрузке изображения: {ex.Message}");
            return null;
        }

    }
/*    public static async Task<BitmapImage> LoadImageFromSftpAsync(string host, string username, string password, string remoteFilePath)
    {
        return await Task.Run(() => LoadImageFromSftp(host, username, password, remoteFilePath));
    }*/

}
