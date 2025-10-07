using Launcher.ServiceLib;
using Launcher.ServiceLib.Contracts;
using Launcher.ServiceLib.Data;
using System;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace Launcher.Host
{
    internal class Program
    {
        private static ServiceHost _host;

        static void Main()
        {
            Console.Title = "Launcher.Host — сервис авторизации и базы";
            Console.WriteLine("=== 🚀 Инициализация LauncherService ===");

            if (!InitializeDatabase())
                return;

            LogDatabaseStatistics();

            if (!StartWcfService())
                return;

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("▶ Нажмите Enter для остановки сервиса...");
            Console.ResetColor();
            Console.ReadLine();

            StopWcfService();
        }

        // ============================
        // 🔹 ИНИЦИАЛИЗАЦИЯ БАЗЫ
        // ============================
        private static bool InitializeDatabase()
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("🔧 Строка подключения к PostgreSQL:");
                Console.ResetColor();
                Console.WriteLine(Database.ConnectionString);

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("⏳ Проверка доступности базы и создание таблиц...");
                Console.ResetColor();

                Database.EnsureDatabase();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✅ EnsureDatabase выполнен успешно (таблицы готовы).");
                Console.ResetColor();
                return true;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("❌ Ошибка инициализации базы данных:");
                Console.WriteLine(ex);
                Console.ResetColor();
                Console.ReadLine();
                return false;
            }
        }

        // ============================
        // 🔹 ВЫВОД СТАТИСТИКИ ИЗ БД
        // ============================
        private static void LogDatabaseStatistics()
        {
            try
            {
                var dbManager = new DbManager(Database.ConnectionString);

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("\n📊 Загрузка статистики из базы данных...");
                Console.ResetColor();

                int userCount = dbManager.GetTotalUsersCount();
                int charCount = dbManager.GetTotalCharactersCount();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"✅ Зарегистрировано пользователей: {userCount}");
                Console.WriteLine($"✅ Создано персонажей: {charCount}");
                Console.ResetColor();

                if (charCount > 0)
                    PrintLastCharacters();

                PrintAdmins(dbManager);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("❌ Ошибка при загрузке статистики:");
                Console.WriteLine(ex.Message);
                Console.ResetColor();
            }
        }

        private static void PrintLastCharacters()
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("\n🆕 Последние 3 персонажа:");
            Console.ResetColor();

            using var conn = Database.GetOpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT nickname, steam_id64, created_at 
                FROM characters 
                ORDER BY created_at DESC 
                LIMIT 3;";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string nick = reader.IsDBNull(reader.GetOrdinal("nickname")) ? "(без имени)" : reader.GetString(reader.GetOrdinal("nickname"));
                long steam = reader.GetInt64(reader.GetOrdinal("steam_id64"));
                DateTime created = reader.GetDateTime(reader.GetOrdinal("created_at"));
                Console.WriteLine($"  • {nick} | Steam: {steam} | {created:yyyy-MM-dd HH:mm}");
            }
        }

        // ============================
        // 🔹 ВЫВОД АДМИНИСТРАТОРОВ
        // ============================
        private static void PrintAdmins(DbManager dbManager)
        {
            try
            {
                var admins = dbManager.GetAllAdmins();

                if (admins.Count == 0)
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine("\n⚠️ Администраторы в базе не найдены.");
                    Console.ResetColor();
                    return;
                }

                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("\n🛡️ Список администраторов:");
                Console.ResetColor();

                foreach (var admin in admins)
                {
                    Console.WriteLine($"  • Id: {admin.Id} | Login: {admin.LoginAdmin} | Rang: {admin.Rang}");
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("❌ Ошибка при загрузке администраторов:");
                Console.WriteLine(ex.Message);
                Console.ResetColor();
            }
        }

        // ============================
        // 🔹 ЗАПУСК WCF-СЕРВИСА
        // ============================
        private static bool StartWcfService()
        {
            try
            {
                var baseAddress = new Uri("net.tcp://localhost:9000/LauncherService");
                _host = new ServiceHost(typeof(LauncherService), baseAddress);

                var binding = new NetTcpBinding(SecurityMode.None)
                {
                    MaxReceivedMessageSize = 65536,
                    TransferMode = TransferMode.Buffered,
                    OpenTimeout = TimeSpan.FromSeconds(10),
                    CloseTimeout = TimeSpan.FromSeconds(10),
                    SendTimeout = TimeSpan.FromSeconds(30),
                    ReceiveTimeout = TimeSpan.FromMinutes(10)
                };

                _host.AddServiceEndpoint(typeof(ILauncherService), binding, "");
                ConfigureServiceBehaviors(_host);

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\n🔧 Конфигурация сервиса:");
                Console.WriteLine($"  → Базовый адрес: {baseAddress}");
                Console.WriteLine($"  → Основной endpoint: {baseAddress}");
                Console.WriteLine($"  → MEX endpoint: {baseAddress}/mex");
                Console.ResetColor();

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("\n⏳ Открытие хоста...");
                Console.ResetColor();

                _host.Open();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✅ Хост успешно запущен.");
                Console.ResetColor();

                PrintServiceInfo();
                return true;
            }
            catch (AddressAlreadyInUseException)
            {
                HandlePortInUseError();
                return false;
            }
            catch (Exception ex)
            {
                HandleWcfError(ex);
                return false;
            }
        }

        private static void PrintServiceInfo()
        {
            Console.WriteLine($"\n[{DateTime.Now}] Сервис готов принимать запросы.");
            Console.WriteLine("==================================================");
            Console.WriteLine("ℹ Локальный доступ:   net.tcp://localhost:9000/LauncherService");
            Console.WriteLine("ℹ MEX endpoint:        net.tcp://localhost:9000/LauncherService/mex");
            Console.WriteLine("ℹ Для проверки используйте метод Ping()");
            Console.WriteLine("==================================================\n");
        }

        private static void HandlePortInUseError()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("❌ Порт 9000 уже занят!");
            Console.WriteLine("Закройте другие приложения, использующие порт 9000.");
            Console.WriteLine("Или измените порт в коде на другой (например, 9001, 9010)");
            Console.ResetColor();
            Console.ReadLine();
        }

        private static void HandleWcfError(Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("❌ Ошибка запуска WCF-сервиса:");
            Console.WriteLine(ex.Message);

            if (ex.InnerException != null)
            {
                Console.WriteLine("Внутренняя ошибка:");
                Console.WriteLine(ex.InnerException.Message);
            }
            Console.ResetColor();
            Console.ReadLine();
        }

        private static void StopWcfService()
        {
            try
            {
                _host?.Close();
                Console.WriteLine("✅ Сервис остановлен.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при остановке сервиса: {ex.Message}");
            }
        }

        // ============================
        // 🔹 НАСТРОЙКА ПОВЕДЕНИЙ WCF
        // ============================
        private static void ConfigureServiceBehaviors(ServiceHost host)
        {
            // ServiceMetadataBehavior
            var metadataBehavior = host.Description.Behaviors.Find<ServiceMetadataBehavior>();
            if (metadataBehavior == null)
            {
                metadataBehavior = new ServiceMetadataBehavior();
                host.Description.Behaviors.Add(metadataBehavior);
            }

            host.AddServiceEndpoint(
                typeof(IMetadataExchange),
                MetadataExchangeBindings.CreateMexTcpBinding(),
                "mex"
            );

            // ServiceDebugBehavior
            var debugBehavior = host.Description.Behaviors.Find<ServiceDebugBehavior>();
            if (debugBehavior == null)
            {
                debugBehavior = new ServiceDebugBehavior { IncludeExceptionDetailInFaults = true };
                host.Description.Behaviors.Add(debugBehavior);
            }
            else
            {
                debugBehavior.IncludeExceptionDetailInFaults = true;
            }

            // ServiceThrottlingBehavior
            var throttleBehavior = host.Description.Behaviors.Find<ServiceThrottlingBehavior>();
            if (throttleBehavior == null)
            {
                throttleBehavior = new ServiceThrottlingBehavior
                {
                    MaxConcurrentCalls = 100,
                    MaxConcurrentInstances = 100,
                    MaxConcurrentSessions = 100
                };
                host.Description.Behaviors.Add(throttleBehavior);
            }
        }
    }
}
