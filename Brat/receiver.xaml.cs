using Brat.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Pkcs;
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
    /// Логика взаимодействия для receiver.xaml
    /// </summary>
    public partial class Receiver : UserControl
    {

        public Receiver()
        {
            InitializeComponent();
            this.HorizontalAlignment = HorizontalAlignment.Left;

        }

        public Receiver(string text, string dateTime) : this()
        {
            messageText.Text = text;
            MessageDate.Text = dateTime.ToString();
        }
    }
}
