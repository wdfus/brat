using Brat.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
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
        int Myid = 1;
        public class fullStack()
        {
            public string firstName;
            public string secondName;
            public int chatId;
            public int Id;
            public string LastText;
            public string LastMessageStatus;
        }
        public MainWindow()
        {
            InitializeComponent();
            using (var context = new BratBaseContext())
            {

#pragma warning disable CS8601 // Возможно, назначение-ссылка, допускающее значение NULL.
                var result = context.Chats
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
                        chatId = x.Chat.ChatId,
                        Id = x.User.Id,
                        firstName = x.User.FirstName,
                        secondName = x.User.SecondName,

                        LastText = context.Messages
                            .Where(m => m.ChatId == x.Chat.ChatId)
                            .OrderByDescending(m => m.MessageId)
                            .Select(m => m.MessageText)
                            .FirstOrDefault(),

                        LastMessageStatus = context.Messages
                            .Where(m => m.ChatId == x.Chat.ChatId)
                            .OrderByDescending(m => m.MessageId)
                            .Select(m => m.Status)
                            .FirstOrDefault()
                    })
                    .ToList();

#pragma warning disable CS8601 // Возможно, назначение-ссылка, допускающее значение NULL.


                foreach (fullStack user in result)
                {
                    var useraaaaaa = new UserRow(user.firstName.ToString(), user.secondName.ToString(), user.Id, user.chatId, user.LastText, user.LastMessageStatus);
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
                chatField.Children.Clear();
                borderEnterField.Visibility = Visibility.Visible;
                chatField.HorizontalAlignment = HorizontalAlignment.Left;
                chatScroll.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
                using (var context = new BratBaseContext())
                {
                    var jujun = context.Messages.ToList().Where(x => x.ChatId == chatId);
                    try
                    {
                        foreach (Message chat in jujun)
                        {
                            if (user_id == chat.FromUserId)
                            {
                                var receiver = new Receiver(chat.MessageText.ToString());
                                chatField.Children.Add(receiver);
                            }
                            else
                            {
                                var sender = new Sender(chat.MessageText.ToString(), chat.Status.ToString());
                                chatField.Children.Add(sender);
                            }
                        }
                    }
                    catch { }

                }
            }
            else
            {
                chatField.Children.Clear();
                chatField.HorizontalAlignment = HorizontalAlignment.Center;
                chatField.VerticalAlignment = VerticalAlignment.Center;
                chatField.Children.Add(new TextBlock
                {
                    FontSize = 20,
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
                LoadMessages((int)userrow.gridFather.Tag, (int)userrow.Tag);
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

    }
}