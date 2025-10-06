using Launcher.ServiceLib.Data;
using System;
using System.ComponentModel;

namespace Admin.ViewModelsAdmin
{
    public class AdminCharacterViewModel : INotifyPropertyChanged
    {
        private readonly CharacterData _character;

        public AdminCharacterViewModel(CharacterData character)
        {
            _character = character ?? throw new ArgumentNullException(nameof(character));
        }

        public int Id => _character.Id;
        public int ShortId => _character.ShortId;
        public string Nickname => _character.Nickname;
        public int Age => _character.Age;
        public string Story => _character.Story;
        public long SteamId64 => _character.SteamId64;
        public DateTime CreatedAt => _character.CreatedAt;
        public string RegIp => _character.RegIp;
        public bool IsOnline { get; set; } // Заполняется отдельно при загрузке онлайн-статуса

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
