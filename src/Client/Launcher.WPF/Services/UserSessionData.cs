using Launcher.ServiceLib.Contracts;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Launcher.WPF.Services
{
    public class UserSessionData : INotifyPropertyChanged
    {
        private string _userName = "Гражданин";
        private string _sessionId;
        private string _avatarUrl;
        private bool _isAuthenticated;
        private CharacterDto _selectedCharacter;

        public string UserName
        {
            get => _userName;
            set { _userName = value; OnPropertyChanged(); }
        }

        public string SessionId
        {
            get => _sessionId;
            set { _sessionId = value; OnPropertyChanged(); }
        }

        public string AvatarUrl
        {
            get => _avatarUrl;
            set { _avatarUrl = value; OnPropertyChanged(); }
        }

        public bool IsAuthenticated
        {
            get => _isAuthenticated;
            set { _isAuthenticated = value; OnPropertyChanged(); }
        }

        public ObservableCollection<CharacterDto> Characters { get; } = new ObservableCollection<CharacterDto>();

        public CharacterDto SelectedCharacter
        {
            get => _selectedCharacter;
            set
            {
                if (_selectedCharacter != value)
                {
                    _selectedCharacter = value;
                    OnPropertyChanged();
                }
            }
        }

        public void Clear()
        {
            UserName = "Гражданин";
            SessionId = null;
            AvatarUrl = null;
            IsAuthenticated = false;
            Characters.Clear();
            SelectedCharacter = null;
        }

        public void UpdateFromUserDto(UserDto user)
        {
            if (user != null)
            {
                UserName = user.UserName ?? "Гражданин";
                SessionId = user.SessionId;
                AvatarUrl = user.AvatarUrl;
                IsAuthenticated = true;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}