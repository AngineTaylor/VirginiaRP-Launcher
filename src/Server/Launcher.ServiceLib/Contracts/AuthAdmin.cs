using Launcher.ServiceLib.Contracts;
using Launcher.ServiceLib.Data;
using System.Linq;
using static Launcher.ServiceLib.Data.DbManager;

namespace Admin.ServicesAdmin
{
    public class AuthAdmin
    {
        private readonly DbManager _dbManager;

        public AuthAdmin(DbManager dbManager)
        {
            _dbManager = dbManager;
        }

        /// <summary>
        /// Аутентификация администратора по логину и паролю.
        /// </summary>
        /// <param name="login">Логин администратора</param>
        /// <param name="password">Пароль администратора</param>
        /// <returns>AdminData при успешной аутентификации, иначе null</returns>
        public AdminData AuthenticateAdmin(string login, string password)
        {
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
                return null;

            // Получаем всех админов
            var admins = _dbManager.GetAllAdmins();

            // Находим совпадение по логину и паролю
            var admin = admins.FirstOrDefault(a =>
                a.LoginAdmin.Trim() == login.Trim() &&
                a.PasswordAdmin.Trim() == password.Trim());

            return admin;
        }
    }
}
