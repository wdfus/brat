using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;
using Path = System.IO.Path;

namespace Brat
{
    /// <summary>
    /// Логика взаимодействия для CaptionPopup.xaml
    /// </summary>
    public partial class CaptionPopup : Window
    {
        string FilePath;
        BitmapImage ImageBit;
        public CaptionPopup(string FilePath)
        {

            this.FilePath = FilePath;
            InitializeComponent();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {

        }

        public static string SaveFile(string OriginalFilePath, string baseDir, bool Video)
        {
            string newFileName = null;
            string folderPath = null;
            string destPath = null;
            string extension = null;
            string fileName = null;
            try
            {

                // Структура каталогов: year/month/day
                string year = DateTime.Now.Year.ToString();
                string month = DateTime.Now.Month.ToString("D2");
                string day = DateTime.Now.Day.ToString("D2");
                folderPath = System.IO.Path.Combine(baseDir, year, month, day);

                Directory.CreateDirectory(folderPath);
                Debug.WriteLine(folderPath);

                // Имя файла с уникальным хвостом
                extension = System.IO.Path.GetExtension(OriginalFilePath);
                fileName = System.IO.Path.GetFileName(OriginalFilePath);
                if (!Video)
                {
                    newFileName = $"{fileName}";
                }
                else
                {
                    newFileName = $"video_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}{extension}";
                }

                    destPath = System.IO.Path.Combine(folderPath, newFileName);

                File.Copy(OriginalFilePath, destPath, overwrite: false);

                return destPath;
            }
            catch (System.IO.IOException ex)
            {
                MessageBox.Show(ex.Message);
                newFileName = $"{fileName}_{Date}{extension}";
                destPath = System.IO.Path.Combine(folderPath, newFileName);
                File.Copy(OriginalFilePath, destPath, overwrite: false);
                return destPath;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка сохранения файла: {ex.Message}");
                return null;
            }
        }



        public static async Task<string> SaveImageByDateAsync(BitmapImage image, string basePath, string fileName)
        {
            try
            {
                // Формируем путь: year/month/day
                string year = DateTime.Now.Year.ToString();
                string month = DateTime.Now.Month.ToString("D2");
                string day = DateTime.Now.Day.ToString("D2");

                string directoryPath = System.IO.Path.Combine(basePath, year, month, day);
                Debug.WriteLine($"ДИРЕКТОРИ ПАС: {directoryPath}");
                // Создаём папки, если их нет
                Directory.CreateDirectory(directoryPath);

                // Полный путь к файлу
                string filePath = System.IO.Path.Combine(directoryPath, fileName);

                // Сохраняем изображение
                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    BitmapEncoder encoder = new PngBitmapEncoder(); // Можно поменять на JpegBitmapEncoder
                    encoder.Frames.Add(BitmapFrame.Create(image));
                    encoder.Save(fileStream);
                }
                Debug.WriteLine($"Изображение сохранено по пути: {filePath}");
                return filePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сохранении изображения: {ex.Message}");
                return null;
            }
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            var myWindow = System.Windows.Application.Current.Windows
            .OfType<MainWindow>()
            .FirstOrDefault();

            if (myWindow != null)
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;

                // поднимаемся на 3 папки вверх: bin → Debug → net8.0 → Brat
                string basePath = Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\attachments"));
                FileType fileType = GetFileType(FilePath);
                Debug.WriteLine(fileType);
                switch (fileType)
                {
                    case FileType.Document:
                        string path = SaveFile(FilePath, basePath, false);
                        await myWindow.SendMessageFuck(path, CaptionTextBox);
                        break;
                    case FileType.Image:
                        string PhotoName = $"photo_{DateTime.Now:yyyyMMdd_HHmmss}.{System.IO.Path.GetExtension(FilePath)}";
                        ImageBit = new BitmapImage(new Uri(FilePath));
                        string path2 = await SaveImageByDateAsync(ImageBit, basePath, PhotoName);
                        await myWindow.SendMessageFuck(path2, CaptionTextBox);
                        break;
                    case FileType.Video:
                        string path3 = SaveFile(FilePath, basePath, true);
                        await myWindow.SendMessageFuck(path3, CaptionTextBox);
                        break;
                    case FileType.Audio:
                        string path4 = SaveFile(FilePath, basePath, false);
                        await myWindow.SendMessageFuck(path4, CaptionTextBox);
                        break;
                }


            }
            this.Close();
        }

        public enum FileType
        {
            Image,
            Video,
            Audio,
            Document,
            Unknown
        }

        public static FileType GetFileType(string filePath)
        {
            string ext = System.IO.Path.GetExtension(filePath)?.ToLower();
            if (ext == null) return FileType.Unknown;

            string[] imageExts = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
            string[] videoExts = { ".mp4", ".mov", ".avi", ".mkv" };
            string[] audioExts = { ".mp3", ".wav", ".ogg" };
            string[] documentExts = { ".pdf", ".doc", ".docx", ".txt", ".xls", ".xlsx", ".ppt", ".pptx", ".zip", ".sql" };

            if (imageExts.Contains(ext)) return FileType.Image;
            if (videoExts.Contains(ext)) return FileType.Video;
            if (audioExts.Contains(ext)) return FileType.Audio;
            if (documentExts.Contains(ext)) return FileType.Document;

            return FileType.Unknown;
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(FilePath))
            {
                try
                {
                    CapturePlace.Visibility = Visibility.Visible;
                    CapturePlace.Source = new BitmapImage(new Uri(this.FilePath));
                }
                catch
                {
                    VanyaChmo.Children.Clear();
                    VanyaChmo.Visibility = Visibility.Visible;
                    CapturePlace.Visibility = Visibility.Collapsed;
                    string fileName = System.IO.Path.GetFileName(FilePath);
                    var fileNameTextBlock = new TextBlock
                    {
                        FontSize = 12,
                        Foreground = Brushes.White,
                        Text = fileName,
                        HorizontalAlignment = HorizontalAlignment.Center
                    };

                    VanyaChmo.Children.Add(fileNameTextBlock);
                    VanyaChmo.Children.Add(new Separator
                    {
                        Margin = new Thickness(0, 8, 0, 8)
                    });

                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
