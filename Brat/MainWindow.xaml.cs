using Brat;
using Brat.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Net;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
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
        public static int Myid = 1;
        private int SelectedToUserId;
        private int SelectedFromUserId;
        private int SelectedChatId;
        private WebSocketClient _wsClient;
        public class fullStack()
        {
            public string FirstName;
            public string SecondName;
            public int ChatId;
            public int FromUserId;
            public int ToUserId;
            public string LastText;
            public string LastMessageStatus;
            public string Status;
        }

        public void UpdateLastText(string text, int fromUserId)
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
                        userItem.UpdateMessageText(text);
                        break;
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

#pragma warning disable CS8601 // Возможно, назначение-ссылка, допускающее значение NULL.
                var Result = context.Chats
                    .Where(c => c.UserId1 == Myid || c.UserId2 == Myid)
                    .Select(c => new
                    {
                        Chat = c,
                        User = c.UserId1 == Myid
                            ? c.UserId2Navigation  
                            : c.UserId1Navigation
                    })
                    .Select(x => new fullStack
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

                        Status = context.Messages
                            .Where(m => m.ChatId == x.Chat.ChatId)
                            .OrderByDescending(m => m.MessageId)
                            .Select(m => m.Status)
                            .FirstOrDefault().ToString(),
                    })
                    .ToList();

#pragma warning disable CS8601 // Возможно, назначение-ссылка, допускающее значение NULL.


                foreach (fullStack user in Result)
                {
                    var useraaaaaa = new UserRow(user.ToUserId, user.FirstName.ToString(), user.SecondName.ToString(), user.FromUserId, user.ChatId, user.LastText, user.LastMessageStatus, user.Status);
                    UsersList.Items.Add(useraaaaaa);
                }
            }




        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void LoadMessages(int user_id = -1, int chatId = -1)
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
                                var receiver = new Receiver(chat.MessageText.ToString());
                                ChatField.Children.Add(receiver);
                            }
                            else
                            {
                                var sender = new Sender(chat.MessageText.ToString(), chat.Status.ToString());
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
                    Text= "Выберите, кому вы хотите написать...",
                    Foreground = new SolidColorBrush(Colors.White),
                });
                var path = ButtonHeader.Template.FindName("HeaderChatText", ButtonHeader) as System.Windows.Controls.TextBlock;
                if (path != null)
                {
                    path.Text = $"";
                }
                TopRow.Visibility = Visibility.Collapsed;
            }
        }


        private void UsersList_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ListBoxItem a  = ItemsControl.ContainerFromElement(UsersList, e.OriginalSource as DependencyObject) as ListBoxItem;;
            if (a != null && a.Content is UserRow userrow)
            {
                SelectedToUserId = (int)userrow.gridFather.Tag;
                SelectedChatId = (int)userrow.Tag;
                SelectedFromUserId = (int)userrow.TagToUserId.Tag;
                LoadMessages(SelectedToUserId, SelectedChatId);
                userrow.gridFather.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF37587E"));

            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
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
                SelectedToUserId= -1;
                LoadMessages();
            }

        }

        async private void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            List<char> ForbiddenChars = new List<char>
            {
                '\u0000','\u0001','\u0002','\u0003','\u0004','\u0005','\u0006','\u0007',
                '\u0008','\u0009','\u000A','\u000B','\u000C','\u000D','\u000E','\u000F',
                '\u0010','\u0011','\u0012','\u0013','\u0014','\u0015','\u0016','\u0017',
                '\u0018','\u0019','\u001A','\u001B','\u001C','\u001D','\u001E','\u001F',
                '\u007F', ' ',       // DEL
                '\u200B','\u200C','\u200D','\uFEFF' // zero-width
            };
            if (mainTextBox.Text.Any(c => ForbiddenChars.Contains(c)) || String.IsNullOrEmpty(mainTextBox.Text))
            {
                mainTextBox.Text = null;
                return;
            }
                
            else
            {
                using (var context = new BratBaseContext())
                {
                    try
                    {
                        //System.Windows.MessageBox.Show($"ChatID: {SelectedChatId}\n\nFromUserId: {SelectedFromUserId}\n\n ToUserId{SelectedToUserId}");
                        var result = context.Messages.Add(new Message
                        {
                            ChatId = SelectedChatId,
                            FromUserId = SelectedFromUserId,
                            UserId = SelectedToUserId,
                            MessageText = mainTextBox.Text,
                            Status = "notread"
                        });
                        await context.SaveChangesAsync();
                        LoadMessages(SelectedToUserId, SelectedChatId);
                        UpdateLastText(mainTextBox.Text, SelectedToUserId);
                        mainTextBox.Text = "";
                    }
                    catch
                    {
                        System.Windows.MessageBox.Show("Что-то случилось");
                    }
                }
            }
        }

        private void mainTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LoadMessages(SelectedToUserId, SelectedChatId);
            }
        }

        private void OnMessageReceived(string message)
        {
            Dispatcher.Invoke(() =>
            {
                using var doc = JsonDocument.Parse(message);
                int fromUserId = doc.RootElement.GetProperty("to_user_id").GetInt32();
                string text = doc.RootElement.GetProperty("message_text").GetString();
                Debug.WriteLine($"Сообщение от {fromUserId}: {text}");
                UpdateLastText(text, fromUserId);
                if (SelectedToUserId == fromUserId)
                {
                    var receiver = new Receiver(text);
                    ChatField.Children.Add(receiver);
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
            Window1 popup = new Window1();
            popup.Owner = this;
            popup.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            popup.Show();

            this.LocationChanged += (s, e) =>
            {
                popup.Left = this.Left + (this.Width - popup.Width) / 2;
                popup.Top = this.Top + (this.Height - popup.Height) / 2;
            };

            this.SizeChanged += (s, e) =>
            {
                popup.Left = this.Left + (this.Width - popup.Width) / 2;
                popup.Top = this.Top + (this.Height - popup.Height) / 2;
            };
        }
    }
}