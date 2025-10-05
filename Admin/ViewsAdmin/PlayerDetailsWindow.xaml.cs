using Launcher.ServiceLib.Contracts;
using Launcher.ServiceLib.Data;
using System.Windows;

namespace Admin
{
    public partial class PlayerDetailsWindow : Window
    {
        public PlayerDetailsWindow(CharacterData character)
        {
            InitializeComponent();
            Title = $"Профиль: {character.Nickname} (ID: {character.Id})";
            // Позже можно добавить привязку к UI
        }
    }
}