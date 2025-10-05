using Launcher.ServiceLib.Data;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Admin.ViewModelsAdmin
{
    public class AdminCharacterViewModel : INotifyPropertyChanged
    {
        public CharacterData Original { get; }

        public AdminCharacterViewModel(CharacterData data)
        {
            Original = data;
        }

        public int Id => Original.Id;
        public string Nickname => Original.Nickname ?? "(Без имени)";
        public int ShortId => Original.ShortId;
        public string Story => Original.Story ?? "(Нет истории)";
        public string RegIp => Original.RegIp ?? "(Нет IP)";
        public DateTime CreatedAt => Original.CreatedAt;

        // Заглушка для статуса онлайн
        public bool IsOnline
        {
            get
            {
                // Временная логика - можно заменить на реальную проверку онлайна
                var random = new Random(Id);
                return random.Next(0, 2) == 1;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}