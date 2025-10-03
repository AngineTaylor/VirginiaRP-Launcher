using Launcher.WPF.Services;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Launcher.WPF.ViewModels
{
    public class CharacterSelectionViewModel : INotifyPropertyChanged
    {
        public string SelectedCharacterKey { get; private set; }

        public RelayCommand SelectOligarchCommand { get; }
        public RelayCommand SelectOrphanCommand { get; }
        public RelayCommand SelectMafiosoCommand { get; }
        public RelayCommand CancelCommand { get; }

        public CharacterSelectionViewModel()
        {
            SelectOligarchCommand = new RelayCommand(() => SelectCharacter("oligarch"));
            SelectOrphanCommand = new RelayCommand(() => SelectCharacter("sirota"));
            SelectMafiosoCommand = new RelayCommand(() => SelectCharacter("mafioso"));
            CancelCommand = new RelayCommand(() => CloseWindow(false));
        }

        private void SelectCharacter(string key)
        {
            SelectedCharacterKey = key;
            CloseWindow(true); // Передаем true - пользователь сделал выбор
        }

        private void CloseWindow(bool dialogResult)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var window = Application.Current.Windows.OfType<Window>()
                    .FirstOrDefault(w => w.DataContext == this);

                if (window != null)
                {
                    window.DialogResult = dialogResult;
                    window.Close();
                }
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}