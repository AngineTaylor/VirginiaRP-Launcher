using Admin.ServicesAdmin; // RelayCommand
using Launcher.ServiceLib.Contracts; // AdminData
using Launcher.ServiceLib.Data;
using Launcher.WPF.Services;
using System;
using System.Windows;
using System.Windows.Input;
using static Launcher.ServiceLib.Data.DbManager;

namespace Admin.ViewModelsAdmin
{
    public class AdminLoginViewModel : BaseViewModel
    {
        private readonly AuthAdmin _authAdmin;

        public AdminLoginViewModel()
        {
            // Инициализация AuthAdmin с DbManager
            _authAdmin = new AuthAdmin(new DbManager(Database.ConnectionString));

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

        private string _password;
        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        #endregion

        public ICommand LoginCommand { get; }

        /// <summary>
        /// Метод выполнения команды входа
        /// </summary>
        /// <param name="obj">Пароль, переданный через PasswordBox</param>
        private void LoginExecute(object obj)
        {
            string password = obj as string;

            // Логируем ввод пользователя
            Console.WriteLine($"[DEBUG] Login='{Login}', Password='{password}'");

            // Проверка пустых полей
            if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(password))
            {
                ErrorMessage = "Введите логин и пароль.";
                Console.WriteLine("[DEBUG] Поля пустые!");
                return;
            }

            // Аутентификация администратора
            AdminData admin = _authAdmin.AuthenticateAdmin(Login.Trim(), password.Trim());

            if (admin != null)
            {
                // Очистка сообщения об ошибке
                ErrorMessage = "";
                Console.WriteLine($"[DEBUG] Аутентификация успешна: {admin.LoginAdmin}, Rang={admin.Rang}");

                // Определяем окно в зависимости от ранга
                Window nextWindow;
                if (string.Equals(admin.Rang, "Lead", StringComparison.OrdinalIgnoreCase))
                    nextWindow = new MainWindowLead();
                else
                    nextWindow = new MainWindow();

                // Открываем новое окно на UI-потоке
                Application.Current.Dispatcher.Invoke(() =>
                {
                    nextWindow.Show();
                    CloseCurrentWindow();
                });
            }
            else
            {
                ErrorMessage = "Неверный логин или пароль.";
                Console.WriteLine("[DEBUG] Аутентификация неудачна!");
            }
        }

        /// <summary>
        /// Закрытие текущего окна
        /// </summary>
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
