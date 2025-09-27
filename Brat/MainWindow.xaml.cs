using Brat.Models;
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
        int id = 1;
        public MainWindow()
        {

            InitializeComponent();
            using (var context = new BratBaseContext())
            {
                var jujun = context.Messages.ToList();
                foreach (Message chat in jujun)
                {
                    if (id == chat.FromUserId)
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
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}