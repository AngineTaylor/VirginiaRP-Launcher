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
            // ⚙️ Настройки соединения с PostgreSQL
            const string host = "95.163.250.75";
            const string port = "5432";
            const string database = "PostgreSQL-9209";
            const string user = "user";
            const string password = "5bS7zN5v6-5b3c62o";

            // ✅ Используем пул соединений и таймауты
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
        }

        #endregion

        #region === Создание индексов ===

        private static void CreateIndexes(NpgsqlConnection conn)
        {
            using var cmd = conn.CreateCommand();

            // Индексы для ускорения выборок
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

            // Добавляем session_id в users, если его нет
            cmd.CommandText = @"
                DO $$ 
                BEGIN 
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                                  WHERE table_name = 'users' AND column_name = 'session_id') THEN
                        ALTER TABLE users ADD COLUMN session_id VARCHAR(256);
                    END IF;
                END $$;";
            cmd.ExecuteNonQuery();

            // Убедимся, что username может быть NULL в users
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

            // Добавляем уникальное ограничение для short_id в characters, если его нет
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
