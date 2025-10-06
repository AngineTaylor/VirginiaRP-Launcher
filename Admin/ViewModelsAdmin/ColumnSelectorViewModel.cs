using System;
using System.Windows.Input;
using Admin.ServicesAdmin;

namespace Admin.ViewModelsAdmin
{
    public class ColumnSelectorViewModel
    {
        // Существующие колонки
        public bool ShowShortId { get; set; } = false;
        public bool ShowStory { get; set; } = false;
        public bool ShowCreatedAt { get; set; } = false;
        public bool ShowRegIp { get; set; } = false;

        // Новые колонки
        public bool ShowAge { get; set; } = false;
        public bool ShowSteamId64 { get; set; } = false;

        // Команды для кнопок
        public ICommand ApplyCommand { get; }
        public ICommand CancelCommand { get; }

        public event Action ApplyRequested = delegate { };
        public event Action CancelRequested = delegate { };

        public ColumnSelectorViewModel()
        {
            ApplyCommand = new RelayCommand<object>(_ => ApplyRequested());
            CancelCommand = new RelayCommand<object>(_ => CancelRequested());
        }
    }
}
