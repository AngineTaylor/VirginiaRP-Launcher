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

        private void LoginExecute(object obj)
        {
            string password = obj as string; // пароль приходит из CommandParameter
            if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(password))
            {
                ErrorMessage = "Введите логин и пароль.";
                return;
            }

            AdminData admin = _authAdmin.AuthenticateAdmin(Login.Trim(), password.Trim());

            if (admin != null)
            {
                ErrorMessage = "";
                Window nextWindow = string.Equals(admin.Rang, "Lead", StringComparison.OrdinalIgnoreCase)
                    ? (Window)new MainWindowLead()
                    : (Window)new MainWindow();

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
