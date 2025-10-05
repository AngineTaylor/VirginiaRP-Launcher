using Npgsql;
using System;

namespace Launcher.ServiceLib.Data
{
    /// <summary>
    /// –епозиторий дл€ работы с пользовател€ми в базе данных.
    /// </summary>
    public class UserRepository
    {
        /// <summary>
        /// —оздаЄт пользовател€ по Steam ID, если он не существует, или обновл€ет его.
        /// </summary>
        /// <param name="steamId">Steam ID пользовател€ (в формате строки).</param>
        /// <param name="userName">»м€ пользовател€.</param>
        /// <param name="regIp">IP-адрес пользовател€ (опционально).</param>
        /// <returns>¬озвращает ID пользовател€.</returns>
        public int EnsureUserBySteamId(string steamId, string userName, string regIp = null)
        {
            if (!long.TryParse(steamId, out var steamId64))
                throw new ArgumentException("Invalid SteamId");

            using var conn = Database.GetOpenConnection(); // »спользуем глобальное соединение
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO users (steam_id64, username, reg_ip, created_at)
                VALUES (@steam_id64, @username, @reg_ip, NOW())
                ON CONFLICT (steam_id64) DO UPDATE
                    SET username = EXCLUDED.username
                RETURNING id;";
            cmd.Parameters.AddWithValue("steam_id64", steamId64);
            cmd.Parameters.AddWithValue("username", userName ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("reg_ip", regIp ?? (object)DBNull.Value);

            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        /// <summary>
        /// ќбновл€ет сессию пользовател€ по ID.
        /// </summary>
        /// <param name="userId">ID пользовател€.</param>
        /// <param name="sessionId">ID сессии пользовател€.</param>
        public void UpdateSession(int userId, string sessionId)
        {
            using var conn = Database.GetOpenConnection(); // »спользуем глобальное соединение
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE users SET session_id = @sid WHERE id = @id;";
            cmd.Parameters.AddWithValue("sid", sessionId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("id", userId);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// ѕолучает Steam ID пользовател€ по session ID.
        /// </summary>
        /// <param name="sessionId">ID сессии пользовател€.</param>
        /// <returns>¬озвращает Steam ID пользовател€.</returns>
        public string GetSteamIdBySession(string sessionId)
        {
            using var conn = Database.GetOpenConnection(); // »спользуем глобальное соединение
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT steam_id64 FROM users WHERE session_id = @sid;";
            cmd.Parameters.AddWithValue("sid", sessionId ?? (object)DBNull.Value);
            var result = cmd.ExecuteScalar();
            return result?.ToString();
        }
    }
}
