using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace NetworkProgram
{
    public partial class ServerWindow : Window
    {
        private bool isStartServer = false;
        private Socket? listenSocket;  // "слушающий" сокет - ожидает запросы
        private IPEndPoint? endPoint;  // точка(endPoint), которую "слушает" сокет, на эту точку приходят запросы
        private LinkedList<ChatMessage> messages;  // коллекция сообщений всех пользователей

        public ServerWindow()
        {
            InitializeComponent();
            messages = new();
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
                    // ожидание запроса, эта инструкция блокирует поток до прихода запроса
                    Socket socket = listenSocket.Accept();

                    // этот код выполняется когда сервер получил запрос
                    MemoryStream memoryStream = new();  // "ByteBuilder" - способ накопление байтов
                    do
                    {
                        int n = socket.Receive(buffer);
                        memoryStream.Write(buffer, 0, n);
                    } while (socket.Available > 0);
                    string str = Encoding.UTF8.GetString(memoryStream.ToArray());

                    // декадируем из JSON в ClientRequest
                    ServerResponse serverResponse = new();
                    ClientRequest? clientRequest = null;
                    try { clientRequest = JsonSerializer.Deserialize<ClientRequest>(str); } catch { }

                    bool needLog = true;  // нужно ли логировать данные на экран
                    if (clientRequest is null)
                    {
                        str = "Error decoding JSON: " + str;
                        serverResponse.Status = "400 Bad request";
                    }
                    else  // узнаём команду запроса
                    {
                        if (clientRequest.Command.Equals("Message"))
                        {
                            // время устанавливаем которое на сервере
                            clientRequest.ChatMessage.Moment = DateTime.Now;

                            // добавляем в коллекцию
                            messages.AddLast(clientRequest.ChatMessage);

                            // логируем
                            str = clientRequest.ChatMessage.ToString();
                            serverResponse.Status = "200 OK";
                        }
                        else if (clientRequest.Command.Equals("Check")) // синхронизация
                        {
                            // узнаём момент последней синхронизации и отправляем в ответ все уведомления, раньше этого момента
                            serverResponse.Status = "200 OK";
                            serverResponse.Messages = messages;  //.Where(m => m.Moment > clientRequest.ChatMessage.Moment);
                            needLog = false;
                        }
                    }
                    if (needLog)  // только Message будет выводиться, Check не будет
                    {
                        Dispatcher.Invoke(() => serverLog.Text += $"({clientRequest!.ChatMessage.Moment}) {str}\n");
                    }

                    // сервер готовит ответ и отправляет клиенту
                    socket.Send(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(serverResponse)));
                    socket.Close();
                }
            }
            catch (Exception)
            {
                // скорее всего сервер остановился кнопкой из UI, но в любом случае работу прекращаем
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
