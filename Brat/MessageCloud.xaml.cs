using Brat.Models;
using FlyleafLib;
using FlyleafLib.MediaPlayer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
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
using Unosquare.FFME;
using static System.Net.Mime.MediaTypeNames;
using Path = System.IO.Path;
using Point = System.Windows.Point;

namespace Brat
{
    /// <summary>
    /// Логика взаимодействия для MessageCloud.xaml
    /// </summary>
    public partial class MessageCloud : UserControl
    {
        public string StatusRead;
        public int FromUserId;
        public MessageCloud()
        {
            InitializeComponent();

        }
        public MessageCloud(string dateTime, string text, string MessageType, int MessageId, int CurrentId, int FromUserId, string StatusRead = "", string FilePath = "", List<string> files = null) : this()
        {
            this.MessageId.Tag = MessageId;
            this.FromUserId = FromUserId;

            if (DateTime.TryParse(dateTime, out DateTime time))
            {
                MessageDate.Text = time.ToString("HH:mm");
            }
            if (MessageType == "sender")
            {
                messageText.Text = text;
                Bubble.Style = (Style)this.FindResource("BubbleSender");
                messageText.Style = (Style)this.FindResource("TextSender");
                ReadStatus.Text = StatusRead;

            }
            else if (MessageType == "reciever")
            {
                messageText.Text = text;
                Bubble.Style = (Style)this.FindResource("BubbleReciever");
                messageText.Style = (Style)this.FindResource("TextReciever");
                this.Tag = StatusRead;
                StackBubble.Children.Remove(ReadStatus);
            }

            try
            {
                CaptionPopup.FileType extension; ;
                string relativePath = "";
                if (files != null)
                {
                    extension = CaptionPopup.GetFileType(files[0]);
                    relativePath = files[0];
                }
                else
                {
                    extension = CaptionPopup.GetFileType(FilePath);
                    relativePath = FilePath;
                }
                string baseDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\.."));
                string fullPath = Path.Combine(baseDir, relativePath);
                if (extension == CaptionPopup.FileType.Image)
                {

                    Capture.Visibility = Visibility.Visible;
                    Capture.Source = new BitmapImage(new Uri(fullPath));
                    Bubble.Padding = new Thickness(0);
                    StackTimeStatus.Margin = new Thickness(0, 0, 10, 10);
                    messageText.Margin = new Thickness(10, 10, 0, 0);
                }
                if (extension == CaptionPopup.FileType.Video || extension == CaptionPopup.FileType.Document || extension == CaptionPopup.FileType.Audio)
                {
                    AttachmentText.Visibility = Visibility.Visible;
                    HyperLinkMessage.NavigateUri = new Uri(fullPath, UriKind.Absolute);
                    HyperLinkMessage.Inlines.Add(System.IO.Path.GetFileName(fullPath));
                    messageText.Text = text;
                }

            }
            catch (System.ArgumentOutOfRangeException ex)
            {
            }
        }






        private async void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            DeleteRequested?.Invoke(this, this);
            using (var context = new BratBaseContext())
            {
                try
                {
                    var DeletingMessage = context.Messages.Where(m => m.MessageId == (int)MessageId.Tag).FirstOrDefault();
                    var Attachment = context.MessageAttachments.Where(m => m.MessageId == (int)MessageId.Tag).FirstOrDefault();

                    if (Attachment != null)
                    {
                        var FileAsset = context.FileAssets.Where(m => m.Id == Attachment.FileId).FirstOrDefault();
                        context.FileAssets.Remove(FileAsset);
                        context.MessageAttachments.Remove(Attachment);
                        context.Messages.Remove(DeletingMessage);
                        Debug.WriteLine($"ПУТЬ К ФАЙЛУ: {FileAsset.File}");
                        await context.SaveChangesAsync();
                    }
                    else
                    {
                        context.Messages.Remove(DeletingMessage);
                        await context.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message.ToString());
                }
            }
        }

        public event EventHandler<MessageCloud> DeleteRequested;

        private void StackTimeStatus_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void Capture_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void AvatarImage_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateImageClip(sender as System.Windows.Controls.Image, 12); // радиус в px
        }

        private void AvatarImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateImageClip(sender as System.Windows.Controls.Image, 12);
        }

        private void UpdateImageClip(System.Windows.Controls.Image img, double radius)
        {
            if (img == null) return;

            double w = img.ActualWidth;
            double h = img.ActualHeight;
            if (w <= 0 || h <= 0) return;

            var figure = new PathFigure { StartPoint = new Point(radius, 0) };
            var segments = new PathSegmentCollection
                {
                    // Верхняя левая дуга
                    new QuadraticBezierSegment(new Point(0, 0), new Point(0, radius), true),

                    // Левая сторона
                    new LineSegment(new Point(0, h), true),

                    // Нижняя сторона
                    new LineSegment(new Point(w, h), true),

                    // Правая сторона вверх
                    new LineSegment(new Point(w, radius), true),

                    // Верхняя правая дуга
                    new QuadraticBezierSegment(new Point(w, 0), new Point(w - radius, 0), true)
                };

            figure.Segments = segments;
            figure.IsClosed = true;

            img.Clip = new PathGeometry(new[] { figure });
        }

        private void HyperLinkMessage_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                string path = e.Uri.LocalPath;

                if (File.Exists(path))
                    Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
                else
                    MessageBox.Show("Файл не найден: " + path);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии файла: {ex.Message}");
            }

            e.Handled = true;
        }

        private void VideoElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            //if (sender is MediaElement media)
            //{
            //    // Получаем реальные размеры видео
            //    double videoWidth = media.NaturalVideoWidth;
            //    double videoHeight = media.NaturalVideoHeight;

            //    if (videoWidth > 0 && videoHeight > 0)
            //    {
            //        // Расчёт пропорций
            //        double aspect = videoWidth / videoHeight;
            //        double maxWidth = 10; // ограничение под размер "пузыря"
            //        double newHeight = maxWidth / aspect;

            //        // Присваиваем
            //        media.Width = maxWidth;
            //        media.Height = newHeight;

            //        Debug.WriteLine($"Video scaled: {videoWidth}x{videoHeight} → {maxWidth}x{newHeight}");
            //    }
            //}
        }
    }
}
