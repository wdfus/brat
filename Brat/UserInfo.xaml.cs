using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Brat
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class UserInfo : Window
    {
        public UserInfo(MainWindow.UserClass user=null)
        {
            InitializeComponent();
            if (user != null)
            {
                ContentHolder.Content = new PopupProfileUC(user);
            }

        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            FadeClosingAnimation();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                FadeClosingAnimation();
            }
        }

        private void FadeClosingAnimation()
        {
            var FadeOut = new DoubleAnimation(this.Opacity, 0, TimeSpan.FromMilliseconds(200));
            FadeOut.Completed += (s2, e2) => this.Close();
            this.BeginAnimation(Window.OpacityProperty, FadeOut);
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {

        }
    }
}
