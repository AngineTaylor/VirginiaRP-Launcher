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

                // ��������� ����������
                var pingResult = service.Ping();
                if (string.IsNullOrEmpty(pingResult))
                {
                    throw new System.ServiceModel.CommunicationException("������ ����������");
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"�� ������� ������������ � �������: {ex.Message}\n���������, ������� �� Launcher.Host",
                    "������ �����������",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                // ������� �������������� ����
                try
                {
                    service = new WcfClientService("net.tcp://localhost:9001/LauncherService");
                    var pingResult = service.Ping();
                    if (!string.IsNullOrEmpty(pingResult))
                    {
                        MessageBox.Show("����������� ����������� ����� ���� 9001",
                            "����������",
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