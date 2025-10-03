using Launcher.ServiceLib.Contracts;
using Launcher.WPF.Services;
using Launcher.WPF.Views;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Launcher.WPF.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly WcfClientService _service;
        private UserSessionData _currentUser;
        private NewsViewModel _newsViewModel;

        // Константа лимита персонажей
        public const int MAX_CHARACTERS = 3;
        public ICommand OpenShopCommand { get; }
        public ICommand OpenForumCommand { get; }

        public UserSessionData CurrentUser
        {
            get => _currentUser;
            set
            {
                _currentUser = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsAuthenticated));
                OnPropertyChanged(nameof(UserName));
                OnPropertyChanged(nameof(Characters));
                OnPropertyChanged(nameof(SelectedCharacter));
                OnPropertyChanged(nameof(CanCreateCharacter));
                OnPropertyChanged(nameof(CharacterLimitText));
                OnPropertyChanged(nameof(CreateCharacterToolTip));
                (CreateCharacterCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (PlayCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public NewsViewModel NewsViewModel
        {
            get => _newsViewModel;
            set { _newsViewModel = value; OnPropertyChanged(); }
        }

        public bool IsAuthenticated => CurrentUser?.IsAuthenticated ?? false;
        public string UserName => CurrentUser?.UserName ?? "Гражданин";

        private string _onlineText = "Онлайн: --";
        public string OnlineText
        {
            get => _onlineText;
            set { _onlineText = value; OnPropertyChanged(); }
        }

        public ObservableCollection<CharacterDto> Characters => CurrentUser?.Characters ?? new ObservableCollection<CharacterDto>();

        public CharacterDto SelectedCharacter
        {
            get => CurrentUser?.SelectedCharacter;
            set
            {
                if (CurrentUser != null)
                {
                    CurrentUser.SelectedCharacter = value;
                    OnPropertyChanged();
                    (PlayCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        // Свойства для лимита персонажей
        public bool CanCreateCharacter => IsAuthenticated && (Characters?.Count < MAX_CHARACTERS);

        public string CharacterLimitText => $"Слотов: {Characters?.Count ?? 0}/{MAX_CHARACTERS}";

        public string CreateCharacterToolTip => CanCreateCharacter
            ? "Создать нового персонажа"
            : Characters?.Count >= MAX_CHARACTERS
                ? $"Достигнут лимит персонажей ({MAX_CHARACTERS})"
                : "Требуется авторизация";

        public ICommand LoginCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand CreateCharacterCommand { get; }
        public ICommand PlayCommand { get; }
        public ICommand SettingsCommand { get; }
        public ICommand MinimizeCommand { get; }
        public ICommand CloseCommand { get; }
        public ICommand OpenSiteCommand { get; }
        public ICommand DeleteCharacterCommand { get; }

        public MainViewModel(WcfClientService service)
        {
            _service = service;
            CurrentUser = new UserSessionData();
            NewsViewModel = new NewsViewModel(service);

            LoginCommand = new RelayCommand(OnLogin);
            LogoutCommand = new RelayCommand(OnLogout);
            CreateCharacterCommand = new RelayCommand(OnCreateCharacter, () => CanCreateCharacter);
            PlayCommand = new RelayCommand(OnPlay, () => SelectedCharacter != null && IsAuthenticated);
            SettingsCommand = new RelayCommand(OnSettings);
            MinimizeCommand = new RelayCommand(() => Application.Current.MainWindow.WindowState = WindowState.Minimized);
            CloseCommand = new RelayCommand(() => Application.Current.Shutdown());
            OpenSiteCommand = new RelayCommand(() => OpenUrl("https://vk.com/unturnedrp"));
            DeleteCharacterCommand = new RelayCommand<CharacterDto>(OnDeleteCharacter);
            OpenShopCommand = new RelayCommand(() => OpenUrl("https://ваш-магазин.com"));
            OpenForumCommand = new RelayCommand(() => OpenUrl("https://ваш-форум.com"));
        }

        private async void OnLogin()
        {
            var loginWindow = new SteamLoginWindow { Owner = Application.Current.MainWindow };
            if (loginWindow.ShowDialog() != true || string.IsNullOrEmpty(loginWindow.SteamResponseUrl)) return;

            try
            {
                var user = await Task.Run(() => _service.AuthenticateSteam(loginWindow.SteamResponseUrl));
                if (user == null) return;

                CurrentUser = new UserSessionData
                {
                    UserName = user.UserName,
                    SessionId = user.SessionId,
                    AvatarUrl = user.AvatarUrl,
                    IsAuthenticated = true
                };

                await RefreshAllAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка авторизации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnLogout()
        {
            CurrentUser = new UserSessionData();
            OnlineText = "Онлайн: --";
        }

        private void OnCreateCharacter()
        {
            if (!CanCreateCharacter) return;

            var selectionWindow = new CharacterSelectionWindow { Owner = Application.Current.MainWindow };

            // Ждем, пока пользователь выберет тип персонажа
            if (selectionWindow.ShowDialog() == true)
            {
                // Получаем выбранный тип персонажа
                var selectedCharacterType = selectionWindow.SelectedCharacterKey;

                if (!string.IsNullOrEmpty(selectedCharacterType))
                {
                    // Создаем окно ввода имени с передачей выбранного типа персонажа
                    var nicknameWindow = new NicknameWindow
                    {
                        Owner = Application.Current.MainWindow,
                        DataContext = new NicknameViewModel(_service, CurrentUser.SessionId, selectedCharacterType)
                    };

                    // Показываем окно ввода имени и ждем результат
                    if (nicknameWindow.ShowDialog() == true)
                    {
                        // Обновляем список персонажей после успешного создания
                        _ = RefreshAllAsync();
                    }
                }
            }
        }

        private async void OnDeleteCharacter(CharacterDto character)
        {
            if (character == null || !IsAuthenticated) return;

            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить персонажа '{character.Nickname}'?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await Task.Run(() => _service.DeleteCharacter(CurrentUser.SessionId, character.Id));
                    await RefreshAllAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления персонажа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void OnPlay()
        {
            if (SelectedCharacter == null)
            {
                MessageBox.Show("Сначала создайте персонажа!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            MessageBox.Show($"Вы начали игру за {SelectedCharacter.Nickname} (ID: {SelectedCharacter.ShortId})", "Игра запущена", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OnSettings()
        {
            var w = new SettingsWindow { Owner = Application.Current.MainWindow };
            w.ShowDialog();
        }

        private async Task RefreshAllAsync()
        {
            if (!IsAuthenticated || string.IsNullOrEmpty(CurrentUser.SessionId)) return;

            try
            {
                var online = await Task.Run(() => _service.GetServerOnline());
                OnlineText = $"Онлайн: {online?.Count ?? 0}";
            }
            catch
            {
                OnlineText = "Онлайн: --";
            }

            try
            {
                var chars = await Task.Run(() => _service.GetCharacters(CurrentUser.SessionId));
                CurrentUser.Characters.Clear();
                if (chars != null)
                {
                    foreach (var c in chars)
                        CurrentUser.Characters.Add(c);
                }
                CurrentUser.SelectedCharacter = CurrentUser.Characters.Count > 0 ? CurrentUser.Characters[0] : null;
                OnPropertyChanged(nameof(SelectedCharacter));
                OnPropertyChanged(nameof(CanCreateCharacter));
                OnPropertyChanged(nameof(CharacterLimitText));
                OnPropertyChanged(nameof(CreateCharacterToolTip));
                (CreateCharacterCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки персонажей: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenUrl(string url)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch { }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}