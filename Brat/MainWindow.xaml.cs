using Brat;
using Brat.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Net;
using System.Net.WebSockets;
using System.Text;
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

namespace Brat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
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
        public MainWindow()
        {
            InitializeComponent();

            _wsClient = new WebSocketClient();
            _wsClient.MessageReceived += OnMessageReceived;
            _wsClient.StatusChanged += OnStatusChanged;

            _ = _wsClient.ConnectAsync("ws://172.20.10.2:6789");
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {

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
                LoadMessages();
            }

        }

        async private void SendMessage_Click(object sender, RoutedEventArgs e)
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
                    mainTextBox.Text = "";
                    LoadMessages(SelectedToUserId, SelectedChatId);
                }
                catch
                {
                    System.Windows.MessageBox.Show("Что-то слуичлось");
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
                var receiver = new Receiver(message);
                ChatField.Children.Add(receiver);

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
    }
}