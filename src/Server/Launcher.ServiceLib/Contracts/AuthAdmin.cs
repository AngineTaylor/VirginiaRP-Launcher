using Launcher.ServiceLib.Data;
using System;
using System.Linq;
using static Launcher.ServiceLib.Data.DbManager;

namespace Launcher.ServiceLib.Contracts
{
    public class AuthAdmin
    {
        private readonly DbManager _dbManager;

        public AuthAdmin(DbManager dbManager)
        {
            _dbManager = dbManager;
        }

        /// <summary>
        /// Аутентифицирует администратора. Возвращает данные админа при успешной аутентификации, иначе null.
        /// </summary>
        public AdminData AuthenticateAdmin(string login, string password)
        {
            var admins = _dbManager.GetAllAdmins();

            var admin = admins.FirstOrDefault(a =>
                string.Equals(a.LoginAdmin, login, StringComparison.OrdinalIgnoreCase));

            if (admin == null) return null;             // логин не найден
            if (admin.PasswordAdmin != password) return null;  // пароль неверный

            return admin; // успешная аутентификация
        }


    }
}
