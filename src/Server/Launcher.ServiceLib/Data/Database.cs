using Npgsql;
using System;

namespace Launcher.ServiceLib.Data
{
    /// <summary>
    /// Класс для управления подключением и структурой базы данных PostgreSQL.
    /// </summary>
    public static class Database
    {
        /// <summary>
        /// Строка подключения к базе данных PostgreSQL.
        /// </summary>
        public static readonly string ConnectionString;

        static Database()
        {
            const string host = "95.163.250.75";
            const string port = "5432";
            const string database = "PostgreSQL-9209";
            const string user = "user";
            const string password = "5bS7zN5v6-5b3c62o";

            ConnectionString = $"Host={host};Port={port};Database={database};Username={user};Password={password};Pooling=true;Timeout=30;CommandTimeout=30";
        }

        /// <summary>
        /// Создаёт и открывает новое соединение с БД.
        /// </summary>
        public static NpgsqlConnection GetOpenConnection()
        {
            var conn = new NpgsqlConnection(ConnectionString);
            conn.Open();
            return conn;
        }

        /// <summary>
        /// Проверяет и создаёт необходимые таблицы и индексы.
        /// </summary>
        public static void EnsureDatabase()
        {
            using var conn = GetOpenConnection();

            CreateTables(conn);
            CreateIndexes(conn);
            AddMissingColumns(conn);
        }

        #region === Создание таблиц ===

        private static void CreateTables(NpgsqlConnection conn)
        {
            using var cmd = conn.CreateCommand();

            // 🛡 Таблица администраторов без created_at
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS admins (
                    id SERIAL PRIMARY KEY,
                    login VARCHAR(64) NOT NULL UNIQUE,
                    password VARCHAR(256) NOT NULL,
                    rang VARCHAR(32) NOT NULL
                );";
            int result = cmd.ExecuteNonQuery();
            Console.WriteLine(result == 0
                ? "ℹ Таблица 'admins' уже существует."
                : "✅ Таблица 'admins' создана.");

            // 🧩 Таблица пользователей
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS users (
                    id SERIAL PRIMARY KEY,
                    steam_id64 BIGINT NOT NULL UNIQUE,
                    username VARCHAR(64),
                    session_id VARCHAR(256),
                    reg_ip VARCHAR(64),
                    created_at TIMESTAMP NOT NULL DEFAULT NOW()
                );";
            cmd.ExecuteNonQuery();

            // 🧩 Таблица персонажей
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS characters (
                    id SERIAL PRIMARY KEY,
                    short_id INT NOT NULL,
                    nickname VARCHAR(64) NOT NULL,
                    age INT NOT NULL,
                    story TEXT,
                    steam_id64 BIGINT NOT NULL REFERENCES users(steam_id64),
                    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
                    reg_ip VARCHAR(64),
                    CONSTRAINT uk_characters_short_id UNIQUE (short_id)
                );";
            cmd.ExecuteNonQuery();

            // 🔹 Лог администраторов
            try
            {
                using var logCmd = conn.CreateCommand();
                logCmd.CommandText = @"SELECT id, login, rang FROM admins ORDER BY id;";
                using var reader = logCmd.ExecuteReader();

                if (!reader.HasRows)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("⚠️ В таблице 'admins' пока нет администраторов.");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine("🛡 Список администраторов:");
                    Console.ResetColor();

                    while (reader.Read())
                    {
                        int id = reader.GetInt32(reader.GetOrdinal("id"));
                        string login = reader.GetString(reader.GetOrdinal("login"));
                        string rang = reader.GetString(reader.GetOrdinal("rang"));

                        Console.WriteLine($"  • Id: {id} | Login: {login} | Rang: {rang}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("❌ Ошибка при чтении администраторов:");
                Console.WriteLine(ex.Message);
                Console.ResetColor();
            }
        }

        #endregion

        #region === Создание индексов ===

        private static void CreateIndexes(NpgsqlConnection conn)
        {
            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"
                CREATE INDEX IF NOT EXISTS idx_characters_steam_id64 ON characters(steam_id64);
                CREATE INDEX IF NOT EXISTS idx_users_steam_id64 ON users(steam_id64);
                CREATE INDEX IF NOT EXISTS idx_users_session_id ON users(session_id);";
            cmd.ExecuteNonQuery();
        }

        #endregion

        #region === Добавление недостающих колонок и ограничений ===

        private static void AddMissingColumns(NpgsqlConnection conn)
        {
            using var cmd = conn.CreateCommand();

            // session_id в users
            cmd.CommandText = @"
                DO $$ 
                BEGIN 
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                                  WHERE table_name = 'users' AND column_name = 'session_id') THEN
                        ALTER TABLE users ADD COLUMN session_id VARCHAR(256);
                    END IF;
                END $$;";
            cmd.ExecuteNonQuery();

            // username nullable
            cmd.CommandText = @"
                DO $$ 
                BEGIN 
                    IF EXISTS (SELECT 1 FROM information_schema.columns 
                              WHERE table_name = 'users' AND column_name = 'username' 
                              AND is_nullable = 'NO') THEN
                        ALTER TABLE users ALTER COLUMN username DROP NOT NULL;
                    END IF;
                END $$;";
            cmd.ExecuteNonQuery();

            // уникальность short_id
            cmd.CommandText = @"
                DO $$ 
                BEGIN 
                    IF NOT EXISTS (SELECT 1 FROM information_schema.table_constraints 
                                  WHERE table_name = 'characters' AND constraint_name = 'uk_characters_short_id') THEN
                        ALTER TABLE characters ADD CONSTRAINT uk_characters_short_id UNIQUE (short_id);
                    END IF;
                END $$;";
            cmd.ExecuteNonQuery();
        }

        #endregion
    }
}
