using Launcher.WPF.ViewModels;
using System.Windows;

namespace Launcher.WPF.Views
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            DataContext = new SettingsViewModel();
        }
    }
}