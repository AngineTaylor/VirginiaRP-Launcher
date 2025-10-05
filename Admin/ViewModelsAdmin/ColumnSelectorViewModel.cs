using System;
using System.Windows.Input;
using Admin.ServicesAdmin;

namespace Admin.ViewModelsAdmin
{
    public class ColumnSelectorViewModel
    {
        public bool ShowShortId { get; set; } = false;
        public bool ShowStory { get; set; } = false;
        public bool ShowCreatedAt { get; set; } = false;
        public bool ShowRegIp { get; set; } = false;

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