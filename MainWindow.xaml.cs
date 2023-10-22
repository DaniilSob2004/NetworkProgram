using System.Windows;

namespace NetworkProgram
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ServerBtn_Click(object sender, RoutedEventArgs e)
        {
            new ServerWindow().Show();
        }

        private void ClientBtn_Click(object sender, RoutedEventArgs e)
        {
            new ClientWindow().Show();
        }

        private void GmailBtn_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            new EmailWindow().ShowDialog();
            Show();
        }

        private void GmailConfirmBtn_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            new AuthWindow().ShowDialog();
            Show();
        }

        private void HttpBtn_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            new HttpWindow().ShowDialog();
            Show();
        }

        private void CryptoBtn_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            new CryptoWindow().ShowDialog();
            Show();
        }
    }
}
