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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Brat
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
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
    }
}
