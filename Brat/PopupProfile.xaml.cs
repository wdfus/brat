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
    public partial class PopupProfile : Window
    {
        public PopupProfile(MainWindow.UserClass user)
        {
            InitializeComponent();
            PhoneNumber.Text = user.PhoneNumber;
            MainName.Text = $"{user.FirstName} {user.SecondName}";
            AboutSelf.Text = user.AboutSelf;
            DateOfBirthday.Text = user.Birthday;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            FadeClosingAnimation();
        }

        private void Button_MouseMove(object sender, MouseEventArgs e)
        {
            var path = ButtonX.Template.FindName("XPath", ButtonX) as System.Windows.Shapes.Path;
            if (path != null)
            {
                path.Stroke = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF8E8E8E"));
            }
        }

        private void ButtonX_MouseLeave(object sender, MouseEventArgs e)
        {
            var path = ButtonX.Template.FindName("XPath", ButtonX) as System.Windows.Shapes.Path;
            if (path != null)
            {
                path.Stroke = Brushes.Black; // пример: меняем цвет
            }
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

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                e.Handled = true; // блокируем стандартное поведение
            }
        }

    }
}
