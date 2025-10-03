using Launcher.WPF.Services;
using Launcher.WPF.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace Launcher.WPF.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            WcfClientService service;
            try
            {
                service = new WcfClientService("net.tcp://localhost:9000/LauncherService");

                // Проверяем соединение
                var pingResult = service.Ping();
                if (string.IsNullOrEmpty(pingResult))
                {
                    throw new System.ServiceModel.CommunicationException("Сервис недоступен");
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Не удалось подключиться к сервису: {ex.Message}\nПроверьте, запущен ли Launcher.Host",
                    "Ошибка подключения",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                // Пробуем альтернативный порт
                try
                {
                    service = new WcfClientService("net.tcp://localhost:9001/LauncherService");
                    var pingResult = service.Ping();
                    if (!string.IsNullOrEmpty(pingResult))
                    {
                        MessageBox.Show("Подключение установлено через порт 9001",
                            "Информация",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                }
                catch
                {
                    service = new WcfClientService();
                }
            }

            DataContext = new MainViewModel(service);
        }

        private void DragArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }
    }
}