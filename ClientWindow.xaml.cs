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
    public partial class ClientWindow : Window
    {
        private Random r = new();
        private IPEndPoint? endPoint;

        public ClientWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            textBoxLogin.Text = "User" + r.Next(100);
            textBoxMessage.Text = "Daniil Soboliev!";
        }


        private void BtnSendMessage_Click(object sender, RoutedEventArgs e)
        {
            string[] address = textBoxHost.Text.Split(':');
            try
            {
                endPoint = new IPEndPoint(IPAddress.Parse(address[0]), Convert.ToInt32(address[1]));
                new Thread(SendMessage).Start(
                    new ClientRequest
                    {
                        Command = "Message",
                        Data = textBoxLogin.Text + ": " + textBoxMessage.Text
                    }
                );
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void SendMessage(object? arg)
        {
            var clientRequest = arg as ClientRequest;
            if (endPoint is null || clientRequest is null) return;

            // создаём сокет
            Socket clientSocket = new(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp
            );

            try
            {
                clientSocket.Connect(endPoint!);  // клиент "вызывает" (сервер слушает)
                clientSocket.Send(  // отправляем запрос в байтах
                    Encoding.UTF8.GetBytes(JsonSerializer.Serialize(clientRequest))  // преобразуем наш тип в json-формат
                );

                // получаем ответ сервера
                MemoryStream memoryStream = new();  // "ByteBuilder" - способ накопление байтов
                byte[] buffer = new byte[1024];
                do
                {
                    int n = clientSocket.Receive(buffer);
                    memoryStream.Write(buffer, 0, n);
                } while (clientSocket.Available > 0);
                string str = Encoding.UTF8.GetString(memoryStream.ToArray());

                // декадируем из JSON в ClientRequest
                ServerResponse? response = null;
                try { response = JsonSerializer.Deserialize<ServerResponse>(str); } catch { }
                if (response is null)
                {
                    str = "JSON Error in " + str;
                    CheckSendStatus(false);
                }
                else
                {
                    str = response.Status;
                    if (response.Status != "200 OK") CheckSendStatus(false);
                    else CheckSendStatus();
                }

                // выводим ответ на лог
                Dispatcher.Invoke(() => clientLog.Text += str + "\n");

                // уведомляем сервер про разрыв сокета
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();  // закрываем соединение
                //clientSocket.Dispose();  // освобождаем (то же самое что и Close())
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }


        private async void CheckSendStatus(bool status = true)
        {
            Dispatcher.Invoke(() => statusLabel.Visibility = Visibility.Visible);
            if (status)
            {
                Dispatcher.Invoke(() =>
                {
                    statusLabel.Background = Brushes.Green;
                    statusLabel.Content = "Отправлено";
                });
                
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    statusLabel.Background = Brushes.Pink;
                    statusLabel.Content = "Ошибка";
                });
            }
            await Task.Delay(3000);
            Dispatcher.Invoke(() => statusLabel.Visibility = Visibility.Hidden);
        }
    }
}
