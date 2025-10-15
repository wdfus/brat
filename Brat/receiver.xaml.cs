using Brat.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public Receiver(string text, string dateTime, string StatusRead, int MessageId, int CurrentId) : this()
        {
            this.Tag = StatusRead;
            messageText.Text = text;
            this.MessageId.Tag = MessageId;
            if (DateTime.TryParse(dateTime, out DateTime time))
            {
                MessageDate.Text = time.ToString("HH:mm");
            }
            if (this.Tag.ToString() == "notread")
                this.IsVisibleChanged += (s, e) =>
                {
                    if (this.IsVisible && this.Tag.ToString() == "notread")
                        using(var context = new BratBaseContext())
                        {
                            var message = context.Messages.Find(MessageId);
                            if (message != null && message.UserId == CurrentId)
                            {
                                message.Status = "read";
                                context.SaveChanges();
                                this.Tag = "read";
                                
                                Debug.WriteLine("Элемент виден и помечен как прочитанный.");
                            }
                            else
                            {
                                Debug.WriteLine("Блять не прозодить");
                            }
                        }
                    else
                        Debug.WriteLine("Элемент скрыт.");
                };
        }

    }
}
