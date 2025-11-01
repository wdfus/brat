using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Brat
{
    /// <summary>
    /// Логика взаимодействия для EditSelfInfoUS.xaml
    /// </summary>
    public partial class EditSelfInfoUS : UserControl
    {
        MainWindow.UserClass CurrentUser;
        public EditSelfInfoUS(MainWindow.UserClass user)
        {
            InitializeComponent();
            this.CurrentUser = user;
        }

        private void Button_MouseMove(object sender, MouseEventArgs e)
        {
            if (sender is Button button)
            {
                var path = FindVisualChild<Path>(button);
                if (path != null)
                {
                    AnimatePathColor(path, (Color)ColorConverter.ConvertFromString("#FF8E8E8E"));
                }
            }
        }

        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Button button)
            {
                var path = FindVisualChild<Path>(button);
                if (path != null)
                {
                    AnimatePathColor(path, Colors.Black);
                }
            }
        }

        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T tChild)
                    return tChild;

                var result = FindVisualChild<T>(child);
                if (result != null)
                    return result;
            }

            return null;
        }

        private void AnimatePathColor(Path path, Color toColor)
        {
            // Проверяем, есть ли кисть, и не заморожена ли она
            if (path.Fill is not SolidColorBrush brush || brush.IsFrozen)
            {
                // Создаём новую незамороженную копию
                brush = new SolidColorBrush(((SolidColorBrush?)path.Fill)?.Color ?? Colors.Black);
                path.Fill = brush;
                path.Stroke = brush;
            }

            var animation = new ColorAnimation
            {
                To = toColor,
                Duration = TimeSpan.FromMilliseconds(150)
            };

            brush.BeginAnimation(SolidColorBrush.ColorProperty, animation);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);
            parentWindow?.Close();
        }
    }
}
