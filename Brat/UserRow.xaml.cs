using Brat.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace Brat
{
    /// <summary>
    /// Логика взаимодействия для UserRow.xaml
    /// </summary>
    public partial class UserRow : UserControl
    {
        public MainWindow.UserClass ThisUser { get; set; }
        public UserRow(MainWindow.UserClass user)
        {
            InitializeComponent();
            nameOfUser.Text = user.FirstName + " " + user.SecondName + " " + user.LastMessageStatus;
            LastUserText.Text = user.LastText;
            gridFather.Tag = user.FromUserId;
            this.Tag = user.ChatId;
            TagToUserId.Tag = user.ToUserId;
            if (user.Status == "read")
            {
                HasRead.Fill = new SolidColorBrush(Color.FromRgb(81, 157, 255));
            }
            else
            {
                HasRead.Fill = new SolidColorBrush(Colors.LightGray);
            }
            this.ThisUser = user;
        }

        public void UpdateMessageText(string message)
        {
            LastUserText.Text = message;
            return;
        }
    }
}
