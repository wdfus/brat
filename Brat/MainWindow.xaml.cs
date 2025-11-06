using Brat;
using Brat.Models;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using NAudio.Wave;
using NAudio.Wave.Compression;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Xceed.Wpf.AvalonDock.Layout;
using Xceed.Wpf.Toolkit;
using static Brat.CaptionPopup;
using static System.Net.Mime.MediaTypeNames;
using static System.Net.WebRequestMethods;
using File = System.IO.File;
using MessageBox = System.Windows.MessageBox;
using Path = System.IO.Path;

namespace Brat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        private UserInfo popup;
        private CaptionPopup captionPopup;
        private UserRow SelectedUserRow;
        DateTime? LastDate = null;
        DateTime? FirstDate = null;
        private bool FirstLoadedMessages = false;
        private bool _isLoadingMessages = false;
        private int _lastLineCount = 1;
        public static int Myid = 1;
        private int SelectedToUserId;
        private int SelectedFromUserId;
        private int LoadedMessagesCount = 0;
        private int SelectedChatId;
        private string FilePath = string.Empty;
        private WebSocketClient _wsClient;
        private WaveInEvent waveSource;
        private WaveFileWriter waveFile;
        private string tempFilePath;
        public static class VisualHelper
        {
            public static T FindChildByTag<T>(DependencyObject parent, object tag) where T : FrameworkElement
            {
                if (parent == null) return null;

                int count = VisualTreeHelper.GetChildrenCount(parent);
                for (int i = 0; i < count; i++)
                {
                    var child = VisualTreeHelper.GetChild(parent, i);
                    if (child is T fe && fe.Tag != null && fe.Tag.Equals(tag))
                        return fe;

                    var result = FindChildByTag<T>(child, tag);
                    if (result != null)
                        return result;
                }

                return null;
            }
        }

        public class UserClass()
        {
            public string FirstName;
            public string SecondName;
            public int ChatId;
            public int FromUserId;
            public int ToUserId;
            public string LastText;
            public string LastMessageStatus;
            public string LastMessageTime;
            public string Status;
            public string AboutSelf;
            public string Birthday;
            public string PhoneNumber;
            public string Username;
        }

        public void UpdateLastText(string text, int fromUserId, string status = null)
        {
            foreach (var item in UsersList.Items)
            {
                if (item is UserRow userItem)
                {
                    var border = VisualHelper.FindChildByTag<Grid>(userItem, fromUserId);
                    if (border != null)
                    {
                        Debug.WriteLine($"Найден элемент с тегом {border.Tag}");
                        // можно выделить, подсветить, прокрутить
                        UsersList.ScrollIntoView(userItem);
                        if (status == null)
                        {
                            userItem.UpdateUserRow(text);
                            break;
                        }
                        else
                        {
                            userItem.UpdateUserRow(text, status);
                            break;

                        }
                    }
                }
            }
        }
        public MainWindow()
        {
            InitializeComponent();

            _wsClient = new WebSocketClient();
            _wsClient.MessageReceived += OnMessageReceived;
            _wsClient.StatusChanged += OnStatusChanged;
            //Myid = id;
            _ = _wsClient.ConnectAsync($"ws://172.20.10.2:6789");
            using (var context = new BratBaseContext())
            {
                // 1️⃣ Чаты с пользователями
                var baseChats = context.Chats
                    .Include(c => c.UserId1Navigation)
                    .Include(c => c.UserId2Navigation)
                    .Where(c => c.UserId1 == Myid || c.UserId2 == Myid)
                    .ToList();

                // 2️⃣ Сообщения нужных чатов, последние для каждого
                var chatIds = baseChats.Select(c => c.ChatId).ToList();

                var lastMessages = context.Messages
                    .Where(m => chatIds.Contains(m.ChatId))
                    .AsEnumerable() // 👈 тут
                    .GroupBy(m => m.ChatId)
                    .Select(g => g.OrderByDescending(m => m.MessageId).FirstOrDefault())
                    .ToList();

                // 3️⃣ Формируем результат
                var Result = baseChats.Select(c =>
                {
                    var user = c.UserId1 == Myid ? c.UserId2Navigation : c.UserId1Navigation;
                    var lastMessage = lastMessages.FirstOrDefault(m => m.ChatId == c.ChatId);

                    return new UserClass
                    {
                        ChatId = c.ChatId,
                        FromUserId = user.Id,
                        ToUserId = (c.UserId1 == user.Id) ? c.UserId2 : c.UserId1,
                        FirstName = user.FirstName,
                        SecondName = user.SecondName,
                        AboutSelf = user.AboutSelf,
                        Birthday = user.Birthday?.ToString(),
                        PhoneNumber = user.PhoneNumber?.ToString(),
                        Username = user.Username,

                        LastText = lastMessage?.MessageText ?? "",
                        LastMessageStatus = lastMessage?.Status?.ToString() ?? "",
                        LastMessageTime = lastMessage?.SentTime?.ToString() ?? "",
                        Status = lastMessage?.Status?.ToString() ?? ""
                    };
                }).ToList();



                foreach (UserClass user in Result)
                {
                    var useraaaaaa = new UserRow(user, Myid);
                    UsersList.Items.Add(useraaaaaa);
                }
            }
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        /*private void LoadMessages(int user_id = -1, int chatId = -1)
        {
            if (user_id != -1 && chatId != -1)
            {
                ChatField.Children.Clear();
                borderEnterField.Visibility = Visibility.Visible;
                ChatField.HorizontalAlignment = HorizontalAlignment.Left;
                ChatField.VerticalAlignment = VerticalAlignment.Bottom;
                chatScroll.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
                using (var context = new BratBaseContext())
                {
                    var Jujun = context.Messages.ToList().Where(x => x.ChatId == chatId);
                    try
                    {
                        foreach (Message chat in Jujun)
                        {
                            if (user_id == chat.FromUserId)
                            {
                                var receiver = new Receiver(chat.MessageText.ToString(), chat.SentTime.ToString(), chat.Status, chat.MessageId, Myid);
                                ChatField.Children.Add(receiver);
                            }
                            else
                            {
                                var sender = new Sender(chat.MessageText.ToString(), chat.Status, chat.SentTime.ToString());
                                ChatField.Children.Add(sender);
                            }
                        }
                        var HeaderName = context.Users.Where(x => x.Id == SelectedToUserId).FirstOrDefault();
                        TopRow.Visibility = Visibility.Visible;
                        var path = ButtonHeader.Template.FindName("HeaderChatText", ButtonHeader) as System.Windows.Controls.TextBlock;
                        if (path != null)
                        {
                            path.Text = $"{HeaderName.FirstName} {HeaderName.SecondName}";
                        }
                        chatScroll.ScrollToEnd();
                    }
                    catch { }

                }
            }
            else
            {
                ChatField.Children.Clear();
                ChatField.HorizontalAlignment = HorizontalAlignment.Center;
                ChatField.VerticalAlignment = VerticalAlignment.Center;
                ChatField.Children.Add(new TextBlock
                {
                    FontSize = 24,
                    Text = "Выберите, кому вы хотите написать...",
                    Foreground = new SolidColorBrush(Colors.White),
                });
                var path = ButtonHeader.Template.FindName("HeaderChatText", ButtonHeader) as System.Windows.Controls.TextBlock;
                if (path != null)
                {
                    path.Text = $"";
                }
                TopRow.Visibility = Visibility.Collapsed;
            }
        }*/


        private string GetDateLabel(DateTime dt)
        {
            var today = DateTime.Today;
            if (dt.Date == today) return "Сегодня";
            if (dt.Date == today.AddDays(-1)) return "Вчера";
            return dt.ToString("dd MMMM"); // например, "21 октября"
        }


        private void SetFaker(string text)
        {
            ChatField.Children.Clear();
            // Заглушка, если чат не выбран
            ChatField.HorizontalAlignment = HorizontalAlignment.Center;
            ChatField.VerticalAlignment = VerticalAlignment.Center;
            ChatField.Children.Add(new TextBlock
            {
                FontSize = 24,
                Text = text,
                Foreground = Brushes.White,
            });

            TopRow.Visibility = Visibility.Collapsed;
            SetHeaderText("");
        }

        private async Task LoadMessages(int userId = -1, int chatId = -1, bool LoadMore = false)
        {
            int SkipCount;
            if ((userId == -1 || chatId == -1) && !LoadMore)
            {
                SetFaker("Выберите, кому вы хотите написать...");
                return;
            }


            ChatField.HorizontalAlignment = HorizontalAlignment.Left;
            ChatField.VerticalAlignment = VerticalAlignment.Bottom;
            _isLoadingMessages = true;
            try
            {
                using var context = new BratBaseContext();
                {
                    SkipCount = LoadMore ? LoadedMessagesCount : 0;
                    if (!LoadMore)
                    {
                        ChatField.Children.Clear();
                        var headerName = await context.Users
                    .Where(u => u.Id == SelectedToUserId)
                    .Select(u => new { u.FirstName, u.SecondName })
                    .FirstOrDefaultAsync();




                        FirstLoadedMessages = true;
                        // Для первой загрузки

                        var messages = await context.Messages
                            .Where(x => x.ChatId == chatId)
                            .OrderByDescending(x => x.MessageId)
                            .Skip(SkipCount)
                            .Take(20)
                            .Include(m => m.MessageFiles)      // связи с файлами
                                .ThenInclude(ma => ma.File)     // подключаем сам файл
                            .ToListAsync();
                        LoadedMessagesCount = 0;
                        foreach (var chat in messages.AsEnumerable().Reverse())
                        {
                            if (LastDate == null || LastDate.Value.Date != chat.SentTime.Value.Date)
                            {
                                var dateLabel = new TextBlock
                                {
                                    Text = GetDateLabel((DateTime)chat.SentTime),
                                    HorizontalAlignment = HorizontalAlignment.Center,
                                    Foreground = Brushes.Gray,
                                    Margin = new Thickness(0, 8, 0, 8),
                                    FontWeight = FontWeights.Bold
                                };
                                ChatField.Children.Add(dateLabel);
                                LastDate = chat.SentTime.Value.Date;
                            }

                            // Формируем список путей файлов для MessageCloud
                            List<string> files = chat.MessageFiles
                                .Select(ma => ma.File.File)
                                .Where(f => !string.IsNullOrEmpty(f))
                                .ToList();
                            var bubble = new MessageCloud(
                                chat.SentTime?.ToString() ?? "",
                                chat.MessageText,
                                userId == chat.FromUserId ? "reciever" : "sender",
                                chat.MessageId,
                                Myid,
                                chat.FromUserId,
                                chat.Status,
                                "",
                                files
                            // передаем список файлов
                            );
                            bubble.DeleteRequested += Message_DeleteRequested;
                            ChatField.Children.Add(bubble);

                            if (chat == messages.First())
                                FirstDate = chat.SentTime.Value.Date;


                        }
                        if (headerName != null)
                            SetHeaderText($"{headerName.FirstName} {headerName.SecondName}");
                        TopRow.Visibility = Visibility.Visible;
                        chatScroll.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
                        LoadedMessagesCount += messages.Count;
                    }
                }



                if (LoadMore)
                {
                    Debug.WriteLine("Здесь фолс блять");
                    double prevExtentHeight = chatScroll.ExtentHeight;
                    double prevOffset = chatScroll.VerticalOffset;

                    var messages = await context.Messages
                        .Where(x => x.ChatId == chatId)
                        .OrderByDescending(x => x.MessageId)
                        .Skip(SkipCount)
                        .Take(20)
                        .Include(m => m.MessageFiles)
                            .ThenInclude(ma => ma.File)
                        .ToListAsync();

                    foreach (var chat in messages)
                    {
                        Debug.WriteLine(chat.MessageText);
                        // Получаем путь к файлу, если есть
                        string filePath = chat.MessageFiles?.FirstOrDefault()?.File?.File ?? "";

                        var bubble = new MessageCloud(
                            chat.SentTime?.ToString() ?? "",
                            chat.MessageText,
                            userId == chat.FromUserId ? "reciever" : "sender",
                            chat.MessageId,
                            Myid,
                            chat.FromUserId,
                            chat.Status,
                            filePath
                        );

                        bubble.DeleteRequested += Message_DeleteRequested;

                        // Добавляем метку даты, если она меняется
                        if (LastDate == null || LastDate.Value.Date != chat.SentTime.Value.Date)
                        {
                            var dateLabel = new TextBlock
                            {
                                Text = GetDateLabel((DateTime)chat.SentTime),
                                HorizontalAlignment = HorizontalAlignment.Center,
                                Foreground = Brushes.Gray,
                                Margin = new Thickness(0, 8, 0, 8),
                                FontWeight = FontWeights.Bold
                            };

                            if (LastDate != FirstDate)
                            {
                                ChatField.Children.Insert(0, bubble);
                            }
                            else
                            {
                                ChatField.Children.Insert(1, bubble);
                            }
                            if (LastDate != FirstDate)
                            {
                                ChatField.Children.Insert(0, dateLabel);
                            }

                            LastDate = chat.SentTime.Value.Date;
                        }
                        else
                        {
                            ChatField.Children.Insert(1, bubble);
                        }
                    }

                    LoadedMessagesCount += messages.Count;

                    chatScroll.UpdateLayout();
                    double newExtentHeight = chatScroll.ExtentHeight;

                    // Сохраняем смещение скролла
                    chatScroll.ScrollToVerticalOffset(prevOffset + (newExtentHeight - prevExtentHeight));
                }




                // Скролим вниз, если это первая загрузка
                if (!LoadMore)
                {
                    chatScroll.ScrollToEnd();
                }
                borderEnterField.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LoadMessagesAsync] Ошибка: {ex.Message}");
            }
            finally
            {
                _isLoadingMessages = false;
            }
        }


        private void Message_DeleteRequested(object sender, MessageCloud message)
        {
            ChatField.Children.Remove(message);
            ChatField.UpdateLayout();
            if (ChatField.Children.Count > 0)
            {
                var lastChild = ChatField.Children[ChatField.Children.Count - 1];
                // Например, привести к нужному типу
                var lastMessage = lastChild as MessageCloud;
                UpdateLastText(lastMessage.messageText.Text, SelectedToUserId, lastMessage.StatusRead);
            }
        }

        private void SetHeaderText(string text)
        {
            if (ButtonHeader.Template.FindName("HeaderChatText", ButtonHeader) is TextBlock path)
                path.Text = text;
        }


        private async void UsersList_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

            ListBoxItem a = ItemsControl.ContainerFromElement(UsersList, e.OriginalSource as DependencyObject) as ListBoxItem;
            if (a != null && a.Content is UserRow userrow)
            {
                if (userrow != SelectedUserRow)
                {
                    if (SelectedUserRow != null)
                    {
                        SelectedUserRow.gridFather.Background = Brushes.Transparent;
                    }
                    FirstDate = null;
                    LastDate = null;
                    LoadedMessagesCount = 0;
                    SelectedUserRow = userrow;
                    SelectedToUserId = (int)userrow.gridFather.Tag;
                    SelectedChatId = (int)userrow.Tag;
                    SelectedFromUserId = (int)userrow.TagToUserId.Tag;
                    userrow.gridFather.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF37587E"));
                    await LoadMessages(SelectedToUserId, SelectedChatId, false);
                }
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            chatScroll.ScrollToEnd();
        }

        // В code-behind
        private async void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && !_isLoadingMessages)
            {
                borderEnterField.Visibility = Visibility.Hidden;
                SelectedChatId = -1;
                SelectedFromUserId = -1;
                SelectedToUserId = -1;
                FirstDate = null;
                LastDate = null;
                await LoadMessages();
                FirstLoadedMessages = false;
                if (SelectedUserRow != null && UsersList.Items.Contains(SelectedUserRow))
                {
                    SelectedUserRow.gridFather.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00FFFFFF"));
                    chatScroll.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                    SelectedUserRow = null;
                }
            }
        }

        async private void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Content == "➤")
            {
                await SendMessageFuck(watermarkTextBox: mainTextBox);
            }

        }

        public async Task SendMessageFuck(List<CaptionPopup.SftpItem> sftps = null, WatermarkTextBox watermarkTextBox = null)
        {
            List<char> ForbiddenChars = new()
    {
        '\u0000','\u0001','\u0002','\u0003','\u0004','\u0005','\u0006','\u0007',
        '\u0008','\u0009','\u000A','\u000B','\u000C','\u000D','\u000E','\u000F',
        '\u0010','\u0011','\u0012','\u0013','\u0014','\u0015','\u0016','\u0017',
        '\u0018','\u0019','\u001A','\u001B','\u001C','\u001D','\u001E','\u001F',
        '\u007F'
    };
            if (sftps != null)
            {
                FilePath = sftps[0].Label;
            }

            // Если поле пустое и нет файла — ничего не отправляем
            if (string.IsNullOrWhiteSpace(FilePath) &&
                (watermarkTextBox == null || string.IsNullOrWhiteSpace(watermarkTextBox.Text)))
                return;

            await using var context = new BratBaseContext();

            DateTime sentTime = DateTime.Now;
            //try
            //{
            // создаём сообщение
            var newMessage = new Message
            {
                ChatId = SelectedChatId,
                FromUserId = SelectedFromUserId,
                UserId = SelectedToUserId,
                MessageText = watermarkTextBox?.Text ?? string.Empty,
                Status = "notread",
                Type = sftps == null ? "message" : "attachment",
                SentTime = sentTime,
            };

            await context.Messages.AddAsync(newMessage);
            await context.SaveChangesAsync(); // сохраняем, чтобы получить MessageId

            // если есть файл — создаём FileAsset и MessageAttachment
            if (!string.IsNullOrEmpty(FilePath))
            {
                string fileType = CaptionPopup.GetFileType(FilePath).ToString();
                var provider = new FileExtensionContentTypeProvider();
                if (!provider.TryGetContentType("voice.ogg", out var mimeType))
                {
                    mimeType = "application/octet-stream";
                }
                var fileAsset = new FileAsset
                {
                    File = FilePath, // относительный путь
                    Kind = fileType.ToLower(),
                    Mime = mimeType,
                    Size = (ulong)sftps[0].File.Attributes.Size,
                    CreatedAt = sentTime
                };

                await context.FileAssets.AddAsync(fileAsset);
                await context.SaveChangesAsync();

                var attachment = new MessageAttachment
                {
                    MessageId = newMessage.MessageId,
                    FileId = (int)fileAsset.Id,
                    CreatedAt = sentTime
                };

                await context.MessageAttachments.AddAsync(attachment);
                await context.SaveChangesAsync();
            }


            // создаём визуальное сообщение
            var isSender = SelectedFromUserId == newMessage.FromUserId;
            var bubble = new MessageCloud(
                newMessage.SentTime?.ToString() ?? "",
                newMessage.MessageText,
                isSender ? "sender" : "reciever",
                newMessage.MessageId,
                Myid,
                newMessage.UserId,
                newMessage.Status,
                FilePath ?? ""
            );

            bubble.DeleteRequested += Message_DeleteRequested;

            // если дата другая — вставляем метку даты
            if (LastDate == null || LastDate.Value.Date != sentTime.Date)
            {
                var dateLabel = new TextBlock
                {
                    Text = GetDateLabel(sentTime),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Foreground = Brushes.Gray,
                    Margin = new Thickness(0, 8, 0, 8),
                    FontWeight = FontWeights.Bold
                };
                ChatField.Children.Add(dateLabel);
                LastDate = sentTime.Date;
            }

            ChatField.Children.Add(bubble);
            ChatField.UpdateLayout();
            UpdateLastText(watermarkTextBox?.Text ?? "", SelectedToUserId);
            if (watermarkTextBox != null) watermarkTextBox.Text = string.Empty;
            chatScroll.ScrollToEnd();
            //}
            //catch (Exception ex)
            //{
            //    System.Windows.MessageBox.Show($"Ошибка при отправке: {ex.Message}");
            //}
        }



        private async void mainTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.Shift && borderEnterField.Visibility == Visibility.Visible)
            {
                // SHIFT+ENTER → перенос строки
                var tb = sender as TextBox;
                int caret = tb.CaretIndex;
                tb.Text = tb.Text.Insert(caret, Environment.NewLine);
                tb.CaretIndex = caret + Environment.NewLine.Length;
                e.Handled = true;
            }

            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                await SendMessageFuck(watermarkTextBox: mainTextBox);
            }
        }

        private void OnMessageReceived(string message)
        {
            Dispatcher.Invoke(() =>
            {
                using var doc = JsonDocument.Parse(message);
                try
                {
                    string Atts = null;
                    string MessageText = null;
                    int MessageId = doc.RootElement.GetProperty("id").GetInt32();
                    string timeString = doc.RootElement.GetProperty("sent_time").ToString();
                    int fromUserId = doc.RootElement.GetProperty("from_user_id").GetInt32();
                    string Status = doc.RootElement.GetProperty("status").GetString();
                    try
                    {
                        Atts = doc.RootElement.GetProperty("atts").ToString();
                        MessageText = doc.RootElement.GetProperty("message_text").ToString();
                    }
                    catch { }
                    if (MessageText == null)
                    {
                        MessageText = "";
                    }
                    if (Atts == null)
                    {
                        Atts = "";
                    }
                    if (DateTime.TryParse(timeString, out DateTime time))
                    {
                        Console.WriteLine(time.ToString("HH:mm"));

                        Debug.WriteLine($"Сообщение от {fromUserId}: {MessageText}");
                        UpdateLastText(MessageText, fromUserId);
                        if (SelectedToUserId == fromUserId)
                        {
                            var receiver = new MessageCloud(timeString.ToString(), MessageText, "sender", MessageId, Myid, fromUserId, Status, FilePath: Atts); ;
                            ChatField.Children.Add(receiver);
                        }
                    }
                }
                catch
                {

                }

            });
        }

        private void OnStatusChanged(string status)
        {
            Dispatcher.Invoke(() =>
            {
                Debug.WriteLine(status);
            });
        }

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            await _wsClient.CloseWebSocketAsync(_wsClient._client);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            UserClass userClass = new UserClass();
            using (var context = new BratBaseContext())
            {
                var UserInfo = context.Users.Where(x => x.Id == Myid).FirstOrDefault();
                if (UserInfo != null)
                {
                    userClass = new UserClass()
                    {
                        FirstName = UserInfo.FirstName,
                        SecondName = UserInfo.SecondName,
                        AboutSelf = UserInfo.AboutSelf,
                        Birthday = UserInfo.Birthday.ToString(),
                        PhoneNumber = UserInfo.PhoneNumber,
                        Username = UserInfo.Username,
                        ChatId = -1,
                        FromUserId = UserInfo.Id,
                        LastMessageStatus = "",
                        LastMessageTime = "",
                        LastText = "",
                        Status = "",
                        ToUserId = -1

                    };
                }

                popup = new UserInfo(userClass);
                popup.Topmost = false;
                popup.Owner = this;
                popup.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
                DimLayer.Visibility = Visibility.Visible;
                var fadeIn = new DoubleAnimation(0, 0.3, TimeSpan.FromMilliseconds(200));
                DimLayer.BeginAnimation(OpacityProperty, fadeIn);
                popup.Opacity = 0;
                popup.Loaded += (s, args) =>
                {
                    var popupFade = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
                    popup.BeginAnimation(Window.OpacityProperty, popupFade);
                };

                popup.Closed += (s, args) =>
                {
                    var FadeOut = new DoubleAnimation(0.3, 0, TimeSpan.FromMilliseconds(200));
                    FadeOut.Completed += (s2, e2) => DimLayer.Visibility = Visibility.Collapsed;
                    DimLayer.BeginAnimation(OpacityProperty, FadeOut);
                    this.Activate();
                    popup = null;
                };

                popup.Show();

                if (popup != null)
                {
                    this.LocationChanged += (s, e) =>
                    {
                        if (popup != null)
                        {
                            popup.Left = this.Left + (this.Width - popup.Width) / 2;
                            popup.Top = this.Top + (this.Height - popup.Height) / 2;
                        }
                    };

                    this.SizeChanged += (s, e) =>
                    {
                        if (popup != null)
                        {
                            popup.Left = this.Left + (this.Width - popup.Width) / 2;
                            popup.Top = this.Top + (this.Height - popup.Height) / 2;
                        }
                    };
                }
            }
        }

        private void TextBlock_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedUserRow != null && SelectedUserRow is UserRow userrow)
            {
                popup = new UserInfo(userrow.ThisUser);
                popup.Owner = this;
                popup.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
                DimLayer.Visibility = Visibility.Visible;
                var fadeIn = new DoubleAnimation(0, 0.3, TimeSpan.FromMilliseconds(200));
                DimLayer.BeginAnimation(OpacityProperty, fadeIn);
                popup.Opacity = 0;
                popup.Loaded += (s, args) =>
                {
                    var popupFade = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
                    popup.BeginAnimation(Window.OpacityProperty, popupFade);
                };

                popup.Closed += (s, args) =>
                {
                    var FadeOut = new DoubleAnimation(0.3, 0, TimeSpan.FromMilliseconds(200));
                    FadeOut.Completed += (s2, e2) => DimLayer.Visibility = Visibility.Collapsed;
                    DimLayer.BeginAnimation(OpacityProperty, FadeOut);
                    popup = null;
                };

                popup.Show();

                if (popup != null)
                {
                    this.LocationChanged += (s, e) =>
                    {
                        if (popup != null)
                        {
                            popup.Left = this.Left + (this.Width - popup.Width) / 2;
                            popup.Top = this.Top + (this.Height - popup.Height) / 2;
                        }
                    };

                    this.SizeChanged += (s, e) =>
                    {
                        if (popup != null)
                        {
                            popup.Left = this.Left + (this.Width - popup.Width) / 2;
                            popup.Top = this.Top + (this.Height - popup.Height) / 2;
                        }
                    };
                }
            }
        }

        private void DimLayer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (popup != null)
            {
                var FadeOut = new DoubleAnimation(popup.Opacity, 0, TimeSpan.FromMilliseconds(200));
                FadeOut.Completed += (s2, e2) => popup.Close();
                popup.BeginAnimation(Window.OpacityProperty, FadeOut);
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                e.Handled = true; // блокируем стандартное поведение
            }
        }

        private void mainTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                if (tb.Text.Length > 0)
                {
                    SendMessage.Content = "➤";
                }
                else
                {
                    SendMessage.Content = "🎤";
                }
                tb.UpdateLayout();
                Debug.WriteLine($"{MainGrid.RowDefinitions[2].Height.Value}");
                var formattedText = new FormattedText(
                    tb.Text + " ",
                    System.Globalization.CultureInfo.CurrentCulture,
                    tb.FlowDirection,
                    new Typeface(tb.FontFamily, tb.FontStyle, tb.FontWeight, tb.FontStretch),
                    tb.FontSize,
                    Brushes.White,
                    new NumberSubstitution(),
                    TextFormattingMode.Display);
                double desiredHeight = formattedText.Height + 20;

                if (tb.LineCount > _lastLineCount)
                {
                    Debug.WriteLine("🔥 Добавилась новая строка — текст переполнил поле!");
                    MainGrid.RowDefinitions[2].Height = new GridLength(MainGrid.RowDefinitions[2].Height.Value + 20);

                }
                else if (tb.LineCount > 0 && tb.LineCount < _lastLineCount)
                {
                    MainGrid.RowDefinitions[2].Height = new GridLength(MainGrid.RowDefinitions[2].Height.Value - 20);
                }

                else if (tb.LineCount == 1)
                {
                    MainGrid.RowDefinitions[2].Height = new GridLength(60);

                }
                else if (tb.LineCount == 0)
                {
                    MainGrid.RowDefinitions[2].Height = new GridLength(60);

                }
                if (string.IsNullOrEmpty(tb.Text))
                {
                    MainGrid.RowDefinitions[2].Height = new GridLength(60);
                }
                _lastLineCount = tb.LineCount;
                //tb.Height = Math.Min(Math.Max(40, desiredHeight), 200);
                //MainGrid.RowDefinitions[2].Height = new GridLength(Math.Min(Math.Max(50, desiredHeight), 250), GridUnitType.Star);



            }
        }

        private async void mainTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // SHIFT+ENTER — вставляем перевод строки (разрешаем)
            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.Shift)
            {
                // Позволяем вставку новой строки — ничего не делаем
                return;
            }

            // ENTER (без Shift) — отправляем и блокируем дальнейшую обработку
            if (e.Key == Key.Enter)
            {
                e.Handled = true; // предотвращаем вставку '\n'
                await SendMessageFuck(watermarkTextBox: mainTextBox); // или SendMessageFuck(), в зависимости от сигнатуры
            }
        }

        private async void chatScroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.VerticalOffset == 0 && FirstLoadedMessages == true && !_isLoadingMessages) // пользователь долистал вверх
            {
                await LoadMessages(SelectedToUserId, SelectedChatId, LoadMore: true);
            }
        }

        private async void AttachFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Выберите файл",
                Filter = "Все файлы|*.*",
                Multiselect = false
            };

            if (dialog.ShowDialog() == true)
            {
                FilePath = dialog.FileName;
                captionPopup = new CaptionPopup(FilePath);
                captionPopup.Topmost = false;
                captionPopup.Owner = this;
                DimLayer.Visibility = Visibility.Visible;
                var fadeIn = new DoubleAnimation(0, 0.3, TimeSpan.FromMilliseconds(200));
                DimLayer.BeginAnimation(OpacityProperty, fadeIn);
                captionPopup.Opacity = 0;
                captionPopup.Loaded += (s, args) =>
                {
                    var popupFade = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
                    captionPopup.BeginAnimation(Window.OpacityProperty, popupFade);
                };

                captionPopup.Closed += (s, args) =>
                {
                    var FadeOut = new DoubleAnimation(0.3, 0, TimeSpan.FromMilliseconds(200));
                    FadeOut.Completed += (s2, e2) => DimLayer.Visibility = Visibility.Collapsed;
                    DimLayer.BeginAnimation(OpacityProperty, FadeOut);
                    this.Activate();
                    captionPopup = null;
                };

                captionPopup.Show();

                if (captionPopup != null)
                {
                    this.LocationChanged += (s, e) =>
                    {
                        if (captionPopup != null)
                        {
                            captionPopup.Left = this.Left + (this.Width - captionPopup.Width) / 2;
                            captionPopup.Top = this.Top + (this.Height - captionPopup.Height) / 2;
                        }
                    };

                    this.SizeChanged += (s, e) =>
                    {
                        if (captionPopup != null)
                        {
                            captionPopup.Left = this.Left + (this.Width - captionPopup.Width) / 2;
                            captionPopup.Top = this.Top + (this.Height - captionPopup.Height) / 2;
                        }
                    };
                }

            }
        }

        // ↓ Добавить эти поля в начало MainWindow класса (где другие private поля)
        private bool isButtonHeld = false;
        private bool isRecording = false;
        private DateTime mouseDownTime;
        private const int HoldThresholdMs = 400;

        private async void SendMessage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Button button && button.Content?.ToString() == "🎤")
            {
                mouseDownTime = DateTime.Now;
                isButtonHeld = true;
                isRecording = false;

                // Асинхронная задержка перед стартом записи
                await Task.Delay(HoldThresholdMs);

                if (isButtonHeld) // если кнопку всё ещё держат — начинаем запись
                {
                    await StartRecordingAsync();
                }
            }
        }

        private async void SendMessage_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is Button button && button.Content?.ToString() == "🎤")
            {
                isButtonHeld = false;
                TimeSpan holdTime = DateTime.Now - mouseDownTime;

                if (isRecording)
                {
                    await StopRecordingAndUploadAsync();
                }
                else if (holdTime.TotalMilliseconds < HoldThresholdMs)
                {
                    MessageBox.Show("Короткое нажатие — можно вставить другое действие");
                }
            }
        }

        private async Task StartRecordingAsync()
        {
            try
            {
                isRecording = true;
                string tempDir = Path.GetTempPath();
                string fileName = $"voice_{DateTime.Now:yyyyMMdd_HHmmss}.wav";
                tempFilePath = Path.Combine(tempDir, fileName);

                waveSource = new WaveInEvent { WaveFormat = new WaveFormat(44100, 1) };
                waveFile = new WaveFileWriter(tempFilePath, waveSource.WaveFormat);

                waveSource.DataAvailable += (s, a) =>
                {
                    waveFile?.Write(a.Buffer, 0, a.BytesRecorded);
                };

                waveSource.RecordingStopped += (s, a) =>
                {
                    waveFile?.Dispose();
                    waveSource?.Dispose();
                };

                waveSource.StartRecording();

                Debug.WriteLine($"🎙 Начата запись: {tempFilePath}");
                // Можно добавить визуальный индикатор (например, красную подсветку)
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при старте записи: {ex.Message}");
            }

            await Task.CompletedTask;
        }

        private async Task StopRecordingAndUploadAsync()
        {
            try
            {
                if (!isRecording) return;
                isRecording = false;

                waveSource?.StopRecording();
                Debug.WriteLine($"⏹ Запись остановлена: {tempFilePath}");

                string ftpServer = "31.31.197.33";
                string username = "u3309507";
                string password = "kSKi8o2D3Yy19h3r";
                string remoteDir = "/var/www/u3309507/data/attachments/voices";
                string remotePath = $"{remoteDir}/{Path.GetFileName(tempFilePath)}";
                ISftpFile sftpFile = null;

                // Асинхронная отправка на SFTP
                await Task.Run(() =>
                {
                    using (var client = new SftpClient(ftpServer, username, password))
                    {
                        client.Connect();

                        if (!client.Exists(remoteDir))
                            client.CreateDirectory(remoteDir);

                        using (var fs = File.OpenRead(tempFilePath))
                        {
                            client.UploadFile(fs, remotePath, true);
                        }
                        sftpFile = client.Get(remotePath);
                        client.Disconnect();
                    }
                });

                Debug.WriteLine($"✅ Голосовое сообщение загружено: {remotePath}");
                List<SftpItem> d = new List<SftpItem> {
                        new SftpItem(remotePath, sftpFile)
                    };
                await SendMessageFuck(d, mainTextBox);
                MessageBox.Show("🎤 Голосовое сообщение отправлено!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отправке: {ex.Message}");
            }

            await Task.CompletedTask;
        }

        private async void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string query = SearchTextBox.Text.Trim();

            if (string.IsNullOrEmpty(query))
            {
                SearchPopup.IsOpen = false;
                return;
            }
            if (query.Length > 2)
            {
                // Пример поиска по списку пользователей
                await using (var context = new BratBaseContext())
                {
                    var results = await context.Users
                    .Where(u => (u.FirstName.ToLower() + " " + u.SecondName.ToLower()).Contains(query) && u.Id != Myid).Select(u => new {
                        u.Id,
                        u.FirstName,
                        u.SecondName,
                        Username = u.Username
                    })
                    .ToListAsync();

                    SearchResultsList.ItemsSource = results;
                    SearchPopup.IsOpen = results.Count > 0;
                }
                Debug.WriteLine("Пенисы");

            } 

        }

        private void SearchResultsList_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }
    }
}