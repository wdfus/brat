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
        public UserRow(int ToUserId, string NameOf, string SecondName, int SenderId, int ChatId, string LastText, string LastStatus, string Status)
        {
            InitializeComponent();
            nameOfUser.Text = NameOf + " " + SecondName + " " + LastStatus;
            LastUserText.Text = LastText;
            gridFather.Tag = SenderId;
            this.Tag = ChatId;
            TagToUserId.Tag = ToUserId;
            if (Status == "read")
            {
                HasRead.Fill = new SolidColorBrush(Color.FromRgb(81, 157, 255));
            }
            else
            {
                HasRead.Fill = new SolidColorBrush(Colors.LightGray);
            }

        }
    }
}
