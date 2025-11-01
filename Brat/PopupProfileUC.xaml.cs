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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Brat
{
    /// <summary>
    /// Логика взаимодействия для PopupProfileUC.xaml
    /// </summary>
    public partial class PopupProfileUC : UserControl
    {
        MainWindow.UserClass CurrentUser;
        public PopupProfileUC(MainWindow.UserClass user)
        {
            this.CurrentUser = user;
            InitializeComponent();
            PhoneNumber.Text = user.PhoneNumber;
            MainName.Text = $"{user.FirstName} {user.SecondName}";
            AboutSelf.Text = user.AboutSelf;
            DateOfBirthday.Text = user.Birthday;
            Username.Text = $"@{user.Username}";
            if (user.ChatId == -1)
            {
                ButtonEdit.Visibility = Visibility.Visible;
            }
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

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                e.Handled = true; // блокируем стандартное поведение
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);
            parentWindow?.Close();
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow != null)
            {
                var contentHolder = parentWindow.FindName("ContentHolder") as ContentControl;
                if (contentHolder != null)
                {
                    // Можно менять контент
                    ShowPage(new EditSelfInfoUS(CurrentUser), contentHolder);
                }
            }

        }

        private void ShowPage(UserControl newPage, ContentControl ContentHolder)
        {
            if (ContentHolder.Content != null)
            {
                // Анимация скрытия старого контрола
                var oldContent = ContentHolder.Content as UIElement;
                var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(200));
                fadeOut.Completed += (s, e) =>
                {
                    // После анимации скрытия заменяем контент
                    ContentHolder.Content = newPage;

                    // Анимация появления нового
                    var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
                    newPage.BeginAnimation(UIElement.OpacityProperty, fadeIn);
                };
                oldContent.BeginAnimation(UIElement.OpacityProperty, fadeOut);
            }
            else
            {
                // Если контейнер пустой, просто вставляем с анимацией
                ContentHolder.Content = newPage;
                newPage.Opacity = 0;
                var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
                newPage.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            }
        }

    }
}
