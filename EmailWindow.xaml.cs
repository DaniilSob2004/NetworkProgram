using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
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

namespace NetworkProgram
{
    public partial class EmailWindow : Window
    {
        public EmailWindow()
        {
            InitializeComponent();
        }

        private void BtnSendButton1_Click(object sender, RoutedEventArgs e)
        {
            string? host = App.GetConfiguration("smtp:host");
            if (host is null) { MessageBox.Show("Error getting host..."); return; }

            string? portString = App.GetConfiguration("smtp:port");
            if (portString is null) { MessageBox.Show("Error getting port..."); return; }
            int port;
            try { port = Convert.ToInt32(portString); }
            catch { MessageBox.Show("Error parsing port..."); return; }

            string? email = App.GetConfiguration("smtp:email");
            if (email is null) { MessageBox.Show("Error getting email..."); return; }

            string? password = App.GetConfiguration("smtp:password");
            if (password is null) { MessageBox.Show("Error getting password..."); return; }

            string? sslString = App.GetConfiguration("smtp:ssl");
            if (sslString is null) { MessageBox.Show("Error getting ssl..."); return; }
            bool ssl;
            try { ssl = Convert.ToBoolean(sslString); }
            catch { MessageBox.Show("Error parsing ssl..."); return; }


            if (!textBoxTo.Text.Contains("@")) { MessageBox.Show("Введите правильный email"); return; }


            using SmtpClient smtpClient = new(host, port)
            {
                EnableSsl = ssl,
                Credentials = new NetworkCredential(email, password)
            };
            smtpClient.Send(email, textBoxTo.Text, textBoxSubject.Text, textBoxContent.Text);
            MessageBox.Show("Отправлено!");
        }
    }
}
