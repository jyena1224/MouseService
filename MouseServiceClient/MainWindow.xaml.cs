using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using Grpc.Core;
using Grpc.Net.Client;
using MagicOnion.Client;
using MouseServiceContracts;

namespace MouseServiceClient
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private IMouseService _mouseService;

        private string _mouseCoordinates;
        public string MouseCoordinates
        {
            get => _mouseCoordinates;
            set
            {
                _mouseCoordinates = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this; // データバインディングの設定
        }
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private async void StartMonitoring_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // サーバーに接続
                var channel = GrpcChannel.ForAddress("http://localhost:5000");
                _mouseService = MagicOnionClient.Create<IMouseService>(channel);

                // 双方向ストリームを開始
                var streaming = await _mouseService.SubscribeMouseData();

                // サーバーからのデータを受信
                // サーバーからのデータを非同期で受信
                await foreach (var mouseData in streaming.ResponseStream.ReadAllAsync())
                {
                    MouseCoordinates = mouseData; // プロパティを更新
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"エラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
