using Launcher.WPF.ViewModels;
using System.Windows;

namespace Launcher.WPF.Views
{
    public partial class CharacterSelectionWindow : Window
    {
        public string SelectedCharacterKey => (DataContext as CharacterSelectionViewModel)?.SelectedCharacterKey;

        public CharacterSelectionWindow()
        {
            InitializeComponent();
            DataContext = new CharacterSelectionViewModel();
        }
    }
}