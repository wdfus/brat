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
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;

namespace Brat
{
    /// <summary>
    /// Логика взаимодействия для MessageCloud.xaml
    /// </summary>
    public partial class MessageCloud : UserControl
    {
        public MessageCloud()
        {
            InitializeComponent();

        }
        public MessageCloud(string dateTime, string text, string MessageType, int MessageId, int CurrentId, string StatusRead = "") : this()
        {
            this.MessageId.Tag = MessageId;
            messageText.Text = text;
            if (DateTime.TryParse(dateTime, out DateTime time))
            {
                MessageDate.Text = time.ToString("HH:mm");
            }
            if (MessageType == "sender")
            {
                messageText.Text += $" {StatusRead}";
                Bubble.Style = (Style)this.FindResource("BubbleSender");
                messageText.Style = (Style)this.FindResource("TextSender");
            }
            else if (MessageType == "reciever")
            {
                Bubble.Style = (Style)this.FindResource("BubbleReciever");
                messageText.Style = (Style)this.FindResource("TextReciever");
                this.Tag = StatusRead;

            }
        }
    }
}
