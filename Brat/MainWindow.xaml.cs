using Brat;
using Brat.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;
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
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Xceed.Wpf.Toolkit;
using static System.Net.Mime.MediaTypeNames;

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
        public static int Myid = 2;
        private int SelectedToUserId;
        private int SelectedFromUserId;
        private int SelectedChatId;
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

        private async Task LoadMessages(int userId = -1, int chatId = -1)
        {
            ChatField.Children.Clear();

            if (userId == -1 || chatId == -1)
            {
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

                // ✅ Берём сообщения сразу с фильтром, без ToList() до фильтрации
                var messages = await context.Messages
                    .Where(m => m.ChatId == chatId)
                    .OrderBy(m => m.SentTime)
 // ⚡ Ограничение по количеству (для производительности)
                    .ToListAsync();

                foreach (var chat in messages)
                {
                    if (userId == chat.FromUserId)
                    {
                        ChatField.Children.Add(new MessageCloud(chat.SentTime.ToString(), chat.MessageText, "reciever", chat.MessageId, Myid, chat.Status));
                    }
                    else
                    {
                        ChatField.Children.Add(new MessageCloud(chat.SentTime.ToString(), chat.MessageText, "sender", chat.MessageId, Myid));
                    }
                }

                // Обновляем заголовок чата
                var headerName = await context.Users
                    .Where(u => u.Id == SelectedToUserId)
                    .Select(u => new { u.FirstName, u.SecondName })
                    .FirstOrDefaultAsync();

                if (headerName != null)
                    SetHeaderText($"{headerName.FirstName} {headerName.SecondName}");

                TopRow.Visibility = Visibility.Visible;
                chatScroll.ScrollToEnd();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LoadMessagesAsync] Ошибка: {ex.Message}");
            }
        }

        private void SetHeaderText(string text)
        {
            if (ButtonHeader.Template.FindName("HeaderChatText", ButtonHeader) is TextBlock path)
                path.Text = text;
        }


        private void UsersList_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ListBoxItem a = ItemsControl.ContainerFromElement(UsersList, e.OriginalSource as DependencyObject) as ListBoxItem;
            if (a != null && a.Content is UserRow userrow)
            {
                SelectedUserRow = userrow;
                SelectedToUserId = (int)userrow.gridFather.Tag;
                SelectedChatId = (int)userrow.Tag;
                SelectedFromUserId = (int)userrow.TagToUserId.Tag;
                LoadMessages(SelectedToUserId, SelectedChatId);
                userrow.gridFather.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF37587E"));

            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            chatScroll.ScrollToEnd();
        }

        // В code-behind
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                borderEnterField.Visibility = Visibility.Hidden;
                SelectedChatId = -1;
                SelectedFromUserId = -1;
                SelectedToUserId = -1;
                LoadMessages();
                if (SelectedUserRow != null && UsersList.Items.Contains(SelectedUserRow))
                {
                    SelectedUserRow.gridFather.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00FFFFFF"));
                    chatScroll.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                }
            }
        }

        async private void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            await SendMessageFuck();
        }


        private async Task SendMessageFuck()
        {
            List<char> ForbiddenChars = new List<char>
                {
                    '\u0000','\u0001','\u0002','\u0003','\u0004','\u0005','\u0006','\u0007',
                    '\u0008','\u0009','\u000A','\u000B','\u000C','\u000D','\u000E','\u000F',
                    '\u0010','\u0011','\u0012','\u0013','\u0014','\u0015','\u0016','\u0017',
                    '\u0018','\u0019','\u001A','\u001B','\u001C','\u001D','\u001E','\u001F',
                    '\u007F'
                };

            if (string.IsNullOrWhiteSpace(mainTextBox.Text))
            {
                mainTextBox.Text = string.Empty;
                return;
            }

            await using (var context = new BratBaseContext())
            {
                try
                {
                    var newMessage = new Message
                    {
                        ChatId = SelectedChatId,
                        FromUserId = SelectedFromUserId,
                        UserId = SelectedToUserId,
                        MessageText = mainTextBox.Text,
                        Status = "notread",
                        SentTime = DateTime.Now,
                    };

                    await context.Messages.AddAsync(newMessage);
                    await context.SaveChangesAsync();

                    // после сохранения можно получить ID нового сообщения
                    var RecentlySentMessage = await context.Messages
                        .Where(x => x.ChatId == SelectedChatId)
                        .OrderByDescending(x => x.MessageId)
                        .FirstOrDefaultAsync();

                    if (RecentlySentMessage != null)
                    {
                        if (SelectedFromUserId == RecentlySentMessage.FromUserId)
                        {
                            var sender = new MessageCloud(RecentlySentMessage.SentTime.ToString(), RecentlySentMessage.MessageText, "sender", RecentlySentMessage.MessageId, Myid, "notread");
                            ChatField.Children.Add(sender);
                            UpdateLastText(mainTextBox.Text, SelectedToUserId);
                            mainTextBox.Text = string.Empty;
                            chatScroll.ScrollToEnd();
                        }
                    }
                }
                catch
                {
                    System.Windows.MessageBox.Show("Что-то случилось");
                }
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
                SendMessageFuck(); // отправляем сообщение
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
                        var receiver = new MessageCloud(timeString.ToString(), text, "sender", MessageId, Myid, Status); ;
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
                tb.Height = Double.NaN; // авто
                tb.UpdateLayout();

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
                tb.Height = Math.Min(Math.Max(40, desiredHeight), 200);
                MainGrid.RowDefinitions[2].Height = new GridLength(Math.Min(Math.Max(50, desiredHeight), 250), GridUnitType.Star);
                Debug.WriteLine($"{MainGrid.RowDefinitions[2].Height.Value}");
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
                await SendMessageFuck(); // или SendMessageFuck(), в зависимости от сигнатуры
            }
        }

        private void chatScroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {

        }
    }
}