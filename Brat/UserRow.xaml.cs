using Brat.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
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
            nameOfUser.Text = user.FirstName + " " + user.SecondName;
            LastUserText.Text = user.LastText;
            gridFather.Tag = user.FromUserId;
            this.Tag = user.ChatId;
            TagToUserId.Tag = user.ToUserId;
            if (DateTime.TryParse(user.LastMessageTime, out DateTime time))
            {
                if (time.Day == DateTime.Now.Day && time.Month == DateTime.Now.Month &&  time.Year == DateTime.Now.Year)
                {
                    TimeTextBox.Text = time.ToString("HH:mm");
                }
                else
                {
                    TimeTextBox.Text = time.ToString("ddd");
                }
            }
/*            if (user.Status == "read")
            {
                FirstArrow.Stroke = (SolidColorBrush)this.TryFindResource("TickReadColor");
                SecondArrow.Stroke = (SolidColorBrush)this.TryFindResource("TickReadColor");
            }
            else
            {
                FirstArrow.Stroke = (SolidColorBrush)this.TryFindResource("TickBackColor");
                SecondArrow.Stroke = (SolidColorBrush)this.TryFindResource("TickFrontColor");
            }
            if (CurrentId != user.FromUserId)
            {
                CheckArrow.Visibility = Visibility.Hidden;
            }*/
            this.ThisUser = user;
        }

        public void UpdateUserRow(string message=null, string Status=null)
        {
            if (message != null)
            {
                LastUserText.Text = message;
            }
/*            if (Status == "read")
            {
                FirstArrow.Stroke = (SolidColorBrush)this.TryFindResource("TickReadColor");
                SecondArrow.Stroke = (SolidColorBrush)this.TryFindResource("TickReadColor");
            }
            else
            {
                FirstArrow.Stroke = (SolidColorBrush)this.TryFindResource("TickBackColor");
                SecondArrow.Stroke = (SolidColorBrush)this.TryFindResource("TickFrontColor");
            }*/

        }
    }
}
