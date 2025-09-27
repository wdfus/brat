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
        }
        public MainWindow()
        {

            InitializeComponent();
            using (var context = new BratBaseContext())
            {
                var result = context.Users
                    .Join(context.Chats,
                          u => u.Id,
                          c => c.UserId2,
                          (u, c) => new { User = u, Chat = c })
                    .Where(uc => context.Chats
                                 .Where(c => c.UserId1 == 1)
                                 .Select(c => c.UserId2)
                                 .Contains(uc.User.Id))
                    .Select(uc =>  new fullStack
                    {
                    firstName = uc.User.FirstName,
                    secondName = uc.User.SecondName,
                    chatId = uc.Chat.ChatId,
                    Id = uc.User.Id
                    })
                    .ToList();

                foreach (fullStack user in result)
                {
                    var useraaaaaa = new UserRow(user.firstName.ToString(), user.secondName.ToString(), user.Id, user.chatId);
                    UsersList.Items.Add(useraaaaaa);
                }
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (UsersList.SelectedItem is UserRow selectedUser)
            {
                MessageBox.Show($"Нажали на: {selectedUser}");
            }
        }

        private void LoadMessages(int user_id, int chatId)
        {
            chatField.Children.Clear();
            chatField.HorizontalAlignment = HorizontalAlignment.Left;
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
                            var sender = new Sender(chat.MessageText.ToString());
                            chatField.Children.Add(sender);
                        }
                    }
                }
                catch { }

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
    }
}