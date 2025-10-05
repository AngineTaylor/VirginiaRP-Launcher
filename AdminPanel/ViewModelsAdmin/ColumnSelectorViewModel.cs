using System.Windows;
using System.Windows.Input;
using Admin.ServicesAdmin;

namespace Admin.ViewModelsAdmin
{
    public class ColumnSelectorViewModel
    {
        // Галочки изначально сняты
        public bool ShowShortId { get; set; } = false;
        public bool ShowStory { get; set; } = false;
        public bool ShowCreatedAt { get; set; } = false;
        public bool ShowRegIp { get; set; } = false;

        public ICommand ApplyCommand { get; }
        public ICommand CancelCommand { get; }

        public ColumnSelectorViewModel()
        {
            ApplyCommand = new RelayCommand<Window>(Apply);
            CancelCommand = new RelayCommand<Window>(Cancel);
        }

        private void Apply(Window window)
        {
            window.DialogResult = true; // Сохраняем изменения
            window.Close();
        }

        private void Cancel(Window window)
        {
            window.DialogResult = false; // Отменяем изменения
            window.Close();
        }
    }
}
