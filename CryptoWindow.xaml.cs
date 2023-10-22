using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
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
    public partial class CryptoWindow : Window
    {
        private readonly HttpClient _httpClient;
        public ObservableCollection<CoinData> CoinsData { get; set; }
        private ListViewItem? previousSelectedItem;  // предыдущий выделенный элемент

        public CryptoWindow()
        {
            InitializeComponent();
            CoinsData = new();
            DataContext = this;
            _httpClient = new() { BaseAddress = new Uri("https://api.coincap.io/") };
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Run(() => LoadAssetsAsync());
        }


        private async Task LoadAssetsAsync()
        {
            var response = JsonSerializer.Deserialize<CoinCapResponse>(
                                await _httpClient.GetStringAsync("/v2/assets?limit=10")
                            );
            if (response is null)
            {
                MessageBox.Show("Deserialization error");
                return;
            }

            Dispatcher.Invoke(() => CoinsData.Clear());
            foreach (var coinData in response.data)
            {
                Dispatcher.Invoke(() => CoinsData.Add(coinData));
            }
        }

        private void CoinData_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListViewItem item)
            {
                if (previousSelectedItem is not null)  // если элемент выделенный
                {
                    previousSelectedItem.Background = Brushes.White;  // возвращаем обычный цвет
                }
                item.Background = Brushes.Aqua;
                previousSelectedItem = item;
                if (item.Content is CoinData coinData)
                {
                    MessageBox.Show(coinData.id);
                }
            }
        }
    }

    // ORM
    public class CoinCapResponse
    {
        public List<CoinData> data { get; set; } = null!;
        public long timestamp { get; set; }
    }

    public class CoinData
    {
        public string id { get; set; } = null!;
        public string rank { get; set; } = null!;
        public string symbol { get; set; } = null!;
        public string name { get; set; } = null!;
        public string supply { get; set; } = null!;
        public string maxSupply { get; set; } = null!;
        public string marketCapUsd { get; set; } = null!;
        public string volumeUsd24Hr { get; set; } = null!;
        public string priceUsd { get; set; } = null!;
        public string changePercent24Hr { get; set; } = null!;
        public string vwap24Hr { get; set; } = null!;
        public string explorer { get; set; } = null!;
    }
}
