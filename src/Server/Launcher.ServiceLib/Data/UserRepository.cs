using Npgsql;
using System;

namespace Launcher.ServiceLib.Data
{
    public class UserRepository
    {
        public int EnsureUserBySteamId(string steamId, string userName, string regIp = null)
        {
            if (!long.TryParse(steamId, out var steamId64))
                throw new ArgumentException("Invalid SteamId");

            using var conn = Database.GetOpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
        INSERT INTO users (steam_id64, username, reg_ip, created_at)
        VALUES (@steam_id64, @username, @reg_ip, NOW())
        ON CONFLICT (steam_id64) DO UPDATE
            SET username = EXCLUDED.username
        RETURNING id;";
            cmd.Parameters.AddWithValue("steam_id64", steamId64); // Используем long, а не string
            cmd.Parameters.AddWithValue("username", userName ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("reg_ip", regIp ?? (object)DBNull.Value);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public void UpdateSession(int userId, string sessionId)
        {
            using var conn = Database.GetOpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE users SET session_id = @sid WHERE id = @id;";
            cmd.Parameters.AddWithValue("sid", sessionId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("id", userId);
            cmd.ExecuteNonQuery();
        }

        public string GetSteamIdBySession(string sessionId)
        {
            using var conn = Database.GetOpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT steam_id64 FROM users WHERE session_id = @sid;";
            cmd.Parameters.AddWithValue("sid", sessionId);
            var result = cmd.ExecuteScalar();
            return result?.ToString(); // Конвертируем bigint в string
        }
    }
}