using Brat.Models;
using Microsoft.EntityFrameworkCore;
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
    /// Логика взаимодействия для EditSelfInfoUS.xaml
    /// </summary>
    public partial class EditSelfInfoUS : UserControl
    {
        MainWindow.UserClass CurrentUser;
        int UserId;
        public EditSelfInfoUS(MainWindow.UserClass user)
        {
            InitializeComponent();
            this.CurrentUser = user;
            if (CurrentUser != null)
            {
                PhoneNumberTextBox.Text = CurrentUser.PhoneNumber ?? "";
                NameTextBox.Text = CurrentUser.FirstName;
                SecondTextBox.Text = CurrentUser.SecondName;
                UsernameTextBox.Text = CurrentUser.Username;
                AboutSelfTextBox.Text = CurrentUser.AboutSelf;
                DoBTextBox.Text = CurrentUser.Birthday;
                UserId = CurrentUser.FromUserId;
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

        private async void Back_Click(object sender, RoutedEventArgs e)
        {
            await using (var context = new BratBaseContext())
            {
                User user = await context.Users.Where(x => x.Id == UserId).FirstOrDefaultAsync();
                var UN = await context.Users.Where(x => x.Username == UsernameTextBox.Text).FirstOrDefaultAsync();
                if (UN.Id != UserId)
                {
                    MessageBox.Show("Используйте другое имя пользователя!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (user != null)
                {
                    user.PhoneNumber = PhoneNumberTextBox.Text;
                    user.FirstName = NameTextBox.Text;
                    user.SecondName = SecondTextBox.Text;
                    user.Username = UsernameTextBox.Text;
                    user.AboutSelf = AboutSelfTextBox.Text;
                    user.Birthday = string.IsNullOrWhiteSpace(DoBTextBox.Text) ? null : DateOnly.Parse(DoBTextBox.Text);
                    await context.SaveChangesAsync();
                    CurrentUser.PhoneNumber = PhoneNumberTextBox.Text;
                    CurrentUser.FirstName = NameTextBox.Text;
                    CurrentUser.SecondName = SecondTextBox.Text;
                    CurrentUser.Username = UsernameTextBox.Text;
                    CurrentUser.AboutSelf = AboutSelfTextBox.Text;
                    CurrentUser.Birthday = string.IsNullOrWhiteSpace(DoBTextBox.Text) ? null : DoBTextBox.Text;
                }
            }
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow != null)
            {
                var contentHolder = parentWindow.FindName("ContentHolder") as ContentControl;
                if (contentHolder != null)
                {
                    // Можно менять контент
                    ShowPage(new PopupProfileUC(CurrentUser), contentHolder);
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

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);
            parentWindow?.Close();
        }
        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                FadeClosingAnimation();
            }
        }

        private void FadeClosingAnimation()
        {
            Window parentWindow = Window.GetWindow(this);
            var FadeOut = new DoubleAnimation(this.Opacity, 0, TimeSpan.FromMilliseconds(200));
            FadeOut.Completed += (s2, e2) => parentWindow?.Close();
            this.BeginAnimation(Window.OpacityProperty, FadeOut);
        }
    }
}
