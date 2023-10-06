using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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
    public partial class ServerWindow : Window
    {
        private bool isStartServer = false;
        private Socket? listenSocket;  // "слушающий" сокет - ожидает запросы
        private IPEndPoint? endPoint;  // точка(endPoint), которую "слушает" сокет, на эту точку приходят запросы

        public ServerWindow()
        {
            InitializeComponent();
            CheckUIStatusState();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            listenSocket?.Close();  // останавливаем сервер при закрытии окна
        }


        private void SwitchServerBtn_Click(object sender, RoutedEventArgs e)
        {
            if (listenSocket is null)  // если сервер выкл.
            {
                try
                {
                    // парсим хост - получаем номер-адрес узла из текстового вида
                    IPAddress ip = IPAddress.Parse(textBoxHost.Text);

                    // получаем порт
                    int port = Convert.ToInt32(textBoxPort.Text);

                    // собираем хост + порт в endpoint
                    endPoint = new IPEndPoint(ip, port);
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }

                listenSocket = new Socket(
                    AddressFamily.InterNetwork,  // IPv4
                    SocketType.Stream,  // двухсторонний (читает и пишет)
                    ProtocolType.Tcp  // протокол - TCP
                );

                // стартуем сервер, поскольку процесс слушания долгий, запускаем в другом потоке
                new Thread(StartServer).Start();
            }
            else  // если сервер вкл.
            {
                // сервер остановить, если он в ожидании, очень сложно
                listenSocket.Close();  // создаёи конфликт, закрываем рабочий сокет
                // это произвидёт к exception в потоке сервера
            }

            isStartServer = !isStartServer;
            CheckUIStatusState();
        }

        private void StartServer()
        {
            if (listenSocket is null || endPoint is null)
            {
                MessageBox.Show("Попытка запуска без инициализации данных!");
                return;
            }
            try
            {
                listenSocket.Bind(endPoint);  // связываем сокет к endpointer, если endpoint(порт) занят, то будет исключение
                listenSocket.Listen(10);  // 10 запросов - максимальная очередь
                Dispatcher.Invoke(() => serverLog.Text += "Сервер запущен\n");

                byte[] buffer = new byte[1024];  // буффер приёма данных
                while (true)  // бесконечный процесс слушания - постоянная работа сервера
                {
                    // ожидание запроса, эта инструкция блокирует поток до приёхода запроса
                    Socket socket = listenSocket.Accept();

                    // этот код выполняется когда сервер получил запрос
                    StringBuilder stringBuilder = new();
                    do
                    {
                        int n = socket.Receive(buffer);  // получаем пакет, сохраняем в буффере
                        // n - кол-во реально полученных байт
                        // декадируем полученные байты в строку и добавляем в string
                        stringBuilder.Append(Encoding.UTF8.GetString(buffer, 0, n));  // TODO: узнать кодирование в настройках
                    
                    } while (socket.Available > 0);  // пока у сокета есть данные
                    
                    string str = stringBuilder.ToString();
                    Dispatcher.Invoke(() => serverLog.Text += $"{DateTime.Now}: {str}\n");
                }
            }
            catch (Exception)
            {
                // скорее всего сервер остановился кнопкой из UI
                // в любом случае работу прекращаем
                listenSocket = null;
                Dispatcher.Invoke(() => serverLog.Text += "Сервер остановлен\n");
            }
        }


        private void CheckUIStatusState()
        {
            CheckStateServerButton();
            CheckStateStatusLabel();
        }

        private void CheckStateServerButton()
        {
            if (isStartServer)
            {
                btnSwitchServer.Content = "Выключить";
                btnSwitchServer.Background = Brushes.Pink;
            }
            else
            {
                btnSwitchServer.Content = "Включить";
                btnSwitchServer.Background = Brushes.Green;
            }
        }

        private void CheckStateStatusLabel()
        {
            if (isStartServer)
            {
                statusLabel.Content = "Включено";
                statusLabel.Background = Brushes.Green;
            }
            else
            {
                statusLabel.Content = "Выключено";
                statusLabel.Background = Brushes.Pink;
            }
        }
    }
}
