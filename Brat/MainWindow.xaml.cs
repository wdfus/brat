using Brat;
using Brat.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
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
using static System.Net.Mime.MediaTypeNames;
using Path = System.IO.Path;

namespace Brat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        private PopupProfile popup;
        private UserRow SelectedUserRow;
        DateTime? LastDate = null;
        DateTime? FirstDate = null;
        private bool FirstLoadedMessages = false;
        private int _lastLineCount = 1;
        public static int Myid = 2;
        private int SelectedToUserId;
        private int SelectedFromUserId;
        private int LoadedMessagesCount = 0;
        private int SelectedChatId;
        private string FilePath = string.Empty;
        private WebSocketClient _wsClient;
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

            _ = _wsClient.ConnectAsync($"ws://{WebSocketClient.GetLocalIPv4()}:6789");
            using (var context = new BratBaseContext())
            {
                var Result = context.Chats
                    .Where(c => c.UserId1 == Myid || c.UserId2 == Myid)
                    .Select(c => new
                    {
                        Chat = c,
                        User = c.UserId1 == Myid
                            ? c.UserId2Navigation
                            : c.UserId1Navigation
                    })
                    .Select(x => new UserClass
                    {
                        ChatId = x.Chat.ChatId,
                        FromUserId = x.User.Id,
                        ToUserId = context.Chats
                        .Where(c => c.ChatId == x.Chat.ChatId && (c.UserId1 == Myid || c.UserId2 == Myid))
                        .AsEnumerable() // дальше вычисляется в памяти
                        .Select(c => c.UserId1 == x.User.Id ? c.UserId2 : c.UserId1)
                        .FirstOrDefault(),
                        FirstName = x.User.FirstName,
                        SecondName = x.User.SecondName,

                        LastText = context.Messages
                            .Where(m => m.ChatId == x.Chat.ChatId)
                            .OrderByDescending(m => m.MessageId)
                            .Select(m => m.MessageText)
                            .FirstOrDefault(),

                        LastMessageStatus = context.Messages
                            .Where(m => m.ChatId == x.Chat.ChatId)
                            .OrderByDescending(m => m.MessageId)
                            .Select(m => m.Status)
                            .FirstOrDefault(),

                        LastMessageTime = context.Messages
                            .Where(m => m.ChatId == x.Chat.ChatId)
                            .OrderByDescending(m => m.MessageId)
                            .Select(m => m.SentTime)
                            .FirstOrDefault().ToString(),

                        Status = context.Messages
                            .Where(m => m.ChatId == x.Chat.ChatId)
                            .OrderByDescending(m => m.MessageId)
                            .Select(m => m.Status)
                            .FirstOrDefault().ToString(),
                        AboutSelf = x.User.AboutSelf,
                        Birthday = x.User.Birthday.ToString(),
                        PhoneNumber = x.User.PhoneNumber.ToString(),
                        Username = x.User.Username,
                    })
                    .ToList();


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

        private async Task LoadMessages(int userId = -1, int chatId = -1, bool LoadMore = false)
        {
            int SkipCount;
            if ((userId == -1 || chatId == -1) && !LoadMore)
            {
                ChatField.Children.Clear();
                // Заглушка, если чат не выбран
                ChatField.HorizontalAlignment = HorizontalAlignment.Center;
                ChatField.VerticalAlignment = VerticalAlignment.Center;
                ChatField.Children.Add(new TextBlock
                {
                    FontSize = 24,
                    Text = "Выберите, кому вы хотите написать...",
                    Foreground = Brushes.White,
                });

                TopRow.Visibility = Visibility.Collapsed;
                SetHeaderText("");
                return;
            }

            borderEnterField.Visibility = Visibility.Visible;
            ChatField.HorizontalAlignment = HorizontalAlignment.Left;
            ChatField.VerticalAlignment = VerticalAlignment.Bottom;
            chatScroll.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;

            try
            {
                using var context = new BratBaseContext();
                {
                    SkipCount = LoadMore ? LoadedMessagesCount : 0;

                    if (!LoadMore)
                    {
                        ChatField.Children.Clear();
                        // Для первой загрузки

                        var messages = await context.Messages
                            .Where(x => x.ChatId == chatId)
                            .OrderByDescending(x => x.MessageId)
                            .Skip(SkipCount)
                            .Take(20)
                            .Include(m => m.MessageFiles)      // связи с файлами
                                .ThenInclude(ma => ma.File)     // подключаем сам файл
                            .ToListAsync();

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
                            FirstLoadedMessages = true;
                            bubble.DeleteRequested += Message_DeleteRequested;
                            ChatField.Children.Add(bubble);

                            if (chat == messages.First())
                                FirstDate = chat.SentTime.Value.Date;
                        }

                    }
                }



                if (LoadMore)
                {
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

                    foreach (var chat in messages.AsEnumerable().Reverse())
                    {
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

                            ChatField.Children.Insert(0, dateLabel);
                            ChatField.Children.Insert(1, bubble);

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
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LoadMessagesAsync] Ошибка: {ex.Message}");
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
                SelectedUserRow = userrow;
                SelectedToUserId = (int)userrow.gridFather.Tag;
                SelectedChatId = (int)userrow.Tag;
                SelectedFromUserId = (int)userrow.TagToUserId.Tag;
                await LoadMessages(SelectedToUserId, SelectedChatId, false);
                userrow.gridFather.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF37587E"));

            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            chatScroll.ScrollToEnd();
        }

        // В code-behind
        private async void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                borderEnterField.Visibility = Visibility.Hidden;
                SelectedChatId = -1;
                SelectedFromUserId = -1;
                SelectedToUserId = -1;
                await LoadMessages();
                FirstLoadedMessages = false;
                if (SelectedUserRow != null && UsersList.Items.Contains(SelectedUserRow))
                {
                    SelectedUserRow.gridFather.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00FFFFFF"));
                    chatScroll.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                }
            }
        }

        async private void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            await SendMessageFuck(watermarkTextBox: mainTextBox);
        }


        public async Task SendMessageFuck(string FilePath = "", WatermarkTextBox watermarkTextBox = null)
        {
            List<char> ForbiddenChars = new()
    {
        '\u0000','\u0001','\u0002','\u0003','\u0004','\u0005','\u0006','\u0007',
        '\u0008','\u0009','\u000A','\u000B','\u000C','\u000D','\u000E','\u000F',
        '\u0010','\u0011','\u0012','\u0013','\u0014','\u0015','\u0016','\u0017',
        '\u0018','\u0019','\u001A','\u001B','\u001C','\u001D','\u001E','\u001F',
        '\u007F'
    };

            // Если поле пустое и нет файла — ничего не отправляем
            if (string.IsNullOrWhiteSpace(FilePath) &&
                (watermarkTextBox == null || string.IsNullOrWhiteSpace(watermarkTextBox.Text)))
                return;

            await using var context = new BratBaseContext();

            DateTime sentTime = DateTime.Now;
            try
            {
                // создаём сообщение
                var newMessage = new Message
                {
                    ChatId = SelectedChatId,
                    FromUserId = SelectedFromUserId,
                    UserId = SelectedToUserId,
                    MessageText = watermarkTextBox?.Text ?? string.Empty,
                    Status = "notread",
                    SentTime = sentTime,
                };

                await context.Messages.AddAsync(newMessage);
                await context.SaveChangesAsync(); // сохраняем, чтобы получить MessageId

                // если есть файл — создаём FileAsset и MessageAttachment
                if (!string.IsNullOrEmpty(FilePath))
                {
                    string basePath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\.."));
                    string relativePath = Path.GetRelativePath(basePath, FilePath);

                    var fileInfo = new FileInfo(FilePath);
                    var fileAsset = new FileAsset
                    {
                        File = relativePath.Replace("\\", "/"), // относительный путь
                        Kind = "photo",
                        Mime = "image/png",
                        Size = (ulong)fileInfo.Length,
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
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при отправке: {ex.Message}");
            }
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
                int fromUserId = doc.RootElement.GetProperty("to_user_id").GetInt32();
                int MessageId = doc.RootElement.GetProperty("message_id").GetInt32();
                string text = doc.RootElement.GetProperty("message_text").GetString();
                string timeString = doc.RootElement.GetProperty("SentTime").GetString();
                string Status = doc.RootElement.GetProperty("status").GetString();

                if (DateTime.TryParse(timeString, out DateTime time))
                {
                    Console.WriteLine(time.ToString("HH:mm"));

                    Debug.WriteLine($"Сообщение от {fromUserId}: {text}");
                    UpdateLastText(text, fromUserId);
                    if (SelectedToUserId == fromUserId)
                    {
                        var receiver = new MessageCloud(timeString.ToString(), text, "sender", MessageId, Myid, fromUserId, Status); ;
                        ChatField.Children.Add(receiver);
                    }
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _wsClient.CloseWebSocketAsync(_wsClient._client);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
        }

        private void TextBlock_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedUserRow != null && SelectedUserRow is UserRow userrow)
            {
                popup = new PopupProfile(userrow.ThisUser);
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
            if (e.VerticalOffset == 0 && FirstLoadedMessages == true) // пользователь долистал вверх
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
                CaptionPopup captionPopup = new CaptionPopup(FilePath);
                captionPopup.Show();

            }
        }
    }
}