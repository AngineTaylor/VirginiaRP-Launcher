using Admin.ServicesAdmin; // RelayCommand
using Launcher.ServiceLib.Contracts; // AuthAdmin, AdminData
using Launcher.WPF.Services; // RelayCommand
using System;
using System.Windows;
using System.Windows.Input;
using static Launcher.ServiceLib.Data.DbManager;

namespace Admin.ViewModelsAdmin
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly AuthAdmin _authAdmin;

        public LoginViewModel()
        {
            // Инициализация AuthAdmin с DbManager
            _authAdmin = new AuthAdmin(new Launcher.ServiceLib.Data.DbManager(
                Launcher.ServiceLib.Data.Database.ConnectionString));

            // Команда входа
            LoginCommand = new RelayCommand<object>(LoginExecute);
        }

        #region Свойства для биндинга

        private string _login;
        public string Login
        {
            get => _login;
            set { _login = value; OnPropertyChanged(); }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        #endregion

        public ICommand LoginCommand { get; }

        private void LoginExecute(object obj)
        {
            // Получаем пароль из параметра (PasswordBox)
            string password = obj as string;

            // Проверка на пустые поля
            if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(password))
            {
                ErrorMessage = "Введите логин и пароль.";
                return;
            }

            // Аутентификация администратора
            AdminData admin = _authAdmin.AuthenticateAdmin(Login, password);

            if (admin != null)
            {
                // Создаём окно в зависимости от ранга
                Window nextWindow = null;

                if (string.Equals(admin.Rang, "Lead", StringComparison.OrdinalIgnoreCase))
                {
                    nextWindow = new MainWindowLead();
                }
                else
                {
                    nextWindow = new MainWindow();
                }

                // Открываем окно на UI-потоке
                Application.Current.Dispatcher.Invoke(() =>
                {
                    nextWindow.Show();
                    CloseCurrentWindow();
                });
            }
            else
            {
                ErrorMessage = "Неверный логин или пароль.";
            }
        }

        private void CloseCurrentWindow()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.DataContext == this)
                {
                    window.Close();
                    break;
                }
            }
        }
    }
}
