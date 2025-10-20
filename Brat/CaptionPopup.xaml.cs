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
    /// Логика взаимодействия для CaptionPopup.xaml
    /// </summary>
    public partial class CaptionPopup : Window
    {
        string FilePath;
        public CaptionPopup(string FilePath)
        {
            this.FilePath = FilePath;
            InitializeComponent();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {

        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            var myWindow = Application.Current.Windows
            .OfType<MainWindow>()
            .FirstOrDefault();

            if (myWindow != null)
            {
                Debug.WriteLine("cklsmcdsklc");
                await myWindow.SendMessageFuck(this.FilePath, CaptionTextBox);
            }
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(FilePath))
                CapturePlace.Source = new BitmapImage(new Uri(this.FilePath));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
