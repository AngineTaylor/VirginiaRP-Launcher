using Dapper;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Launcher.ServiceLib.Data
{
    public class DbManager
    {
        private readonly UserRepository _userRepository;
        private readonly string _connectionString;

        public DbManager(string connectionString)
        {
            _connectionString = connectionString;
            _userRepository = new UserRepository();
        }

        #region User Methods

        public int EnsureUser(string steamId, string userName, string regIp = null)
        {
            return _userRepository.EnsureUserBySteamId(steamId, userName, regIp);
        }

        public void UpdateUserSession(int userId, string sessionId)
        {
            _userRepository.UpdateSession(userId, sessionId);
        }

        public string GetSteamIdBySession(string sessionId)
        {
            return _userRepository.GetSteamIdBySession(sessionId);
        }

        public UserData GetUserBySteamId(long steamId64)
        {
            using var db = Database.GetOpenConnection();
            return db.QueryFirstOrDefault<UserData>(@"
                SELECT id, steam_id64, username, session_id, reg_ip, created_at
                FROM users 
                WHERE steam_id64 = @steamId64;", new { steamId64 });
        }

        public UserData GetUserById(int userId)
        {
            using var db = Database.GetOpenConnection();
            return db.QueryFirstOrDefault<UserData>(@"
                SELECT id, steam_id64, username, session_id, reg_ip, created_at
                FROM users 
                WHERE id = @userId;", new { userId });
        }

        public bool DeleteUser(int userId)
        {
            using var db = Database.GetOpenConnection();
            var rows = db.Execute("DELETE FROM users WHERE id = @userId;", new { userId });
            return rows > 0;
        }

        #endregion

        #region Character Methods

        private int GenerateUniqueShortId()
        {
            using var db = Database.GetOpenConnection();
            var rand = new Random();

            for (int i = 0; i < 500; i++)
            {
                int candidate = rand.Next(100, 1000);
                var exists = db.QueryFirstOrDefault<int?>("SELECT 1 FROM characters WHERE short_id = @sid", new { sid = candidate });
                if (!exists.HasValue) return candidate;
            }

            int max = db.QueryFirstOrDefault<int>("SELECT COALESCE(MAX(short_id),99) FROM characters");
            return Math.Max(100, (max + 1) % 900 + 100);
        }

        public int SaveCharacter(CharacterData c)
        {
            if (c == null) return 0;

            _userRepository.EnsureUserBySteamId(c.SteamId64.ToString(), c.Nickname, c.RegIp);

            using var db = Database.GetOpenConnection();

            if (c.Id > 0)
            {
                db.Execute(@"
                    UPDATE characters SET
                        short_id = @ShortId,
                        nickname = @Nickname,
                        age = @Age,
                        story = @Story,
                        steam_id64 = @SteamId64,
                        reg_ip = @RegIp
                    WHERE id = @Id", c);
                return c.Id;
            }
            else
            {
                if (c.ShortId <= 0)
                    c.ShortId = GenerateUniqueShortId();

                return db.ExecuteScalar<int>(@"
                    INSERT INTO characters (short_id, nickname, age, story, steam_id64, created_at, reg_ip)
                    VALUES (@ShortId, @Nickname, @Age, @Story, @SteamId64, NOW(), @RegIp)
                    RETURNING id;", c);
            }
        }

        public List<CharacterData> GetCharacters(long steamId64)
        {
            using var db = Database.GetOpenConnection();
            return db.Query<CharacterData>(@"
                SELECT id, short_id, nickname, age, story, steam_id64, created_at, reg_ip
                FROM characters 
                WHERE steam_id64 = @steamId64
                ORDER BY created_at DESC;",
                new { steamId64 }).ToList();
        }

        

        public CharacterData GetCharacterById(int characterId)
        {
            using var db = Database.GetOpenConnection();
            return db.QueryFirstOrDefault<CharacterData>(@"
                SELECT id, short_id, nickname, age, story, steam_id64, created_at, reg_ip
                FROM characters 
                WHERE id = @characterId;", new { characterId });
        }

        public CharacterData GetCharacterByShortId(int shortId)
        {
            using var db = Database.GetOpenConnection();
            return db.QueryFirstOrDefault<CharacterData>(@"
                SELECT id, short_id, nickname, age, story, steam_id64, created_at, reg_ip
                FROM characters 
                WHERE short_id = @shortId;", new { shortId });
        }

        public bool UpdateCharacter(CharacterData c)
        {
            using var db = Database.GetOpenConnection();
            var rows = db.Execute(@"
                UPDATE characters SET
                    short_id = @ShortId,
                    nickname = @Nickname,
                    age = @Age,
                    story = @Story,
                    steam_id64 = @SteamId64,
                    reg_ip = @RegIp
                WHERE id = @Id;", c);
            return rows > 0;
        }

        public bool DeleteCharacter(int characterId)
        {
            using var db = Database.GetOpenConnection();
            var rows = db.Execute("DELETE FROM characters WHERE id = @characterId;", new { characterId });
            return rows > 0;
        }

        public bool DeleteAllUserCharacters(long steamId64)
        {
            using var db = Database.GetOpenConnection();
            var rows = db.Execute("DELETE FROM characters WHERE steam_id64 = @steamId64;", new { steamId64 });
            return rows > 0;
        }

        #endregion

        #region Statistics Methods

        public int GetOnlineCount()
        {
            try
            {
                using var db = Database.GetOpenConnection();
                return db.QueryFirstOrDefault<int>("SELECT COUNT(DISTINCT steam_id64) FROM characters;");
            }
            catch
            {
                return 0;
            }
        }

        public int GetTotalUsersCount()
        {
            try
            {
                using var db = Database.GetOpenConnection();
                return db.QueryFirstOrDefault<int>("SELECT COUNT(*) FROM users;");
            }
            catch
            {
                return 0;
            }
        }

        public int GetTotalCharactersCount()
        {
            try
            {
                using var db = Database.GetOpenConnection();
                return db.QueryFirstOrDefault<int>("SELECT COUNT(*) FROM characters;");
            }
            catch
            {
                return 0;
            }
        }

        public UserStats GetUserStats(long steamId64)
        {
            using var db = Database.GetOpenConnection();
            return db.QueryFirstOrDefault<UserStats>(@"
                SELECT 
                    u.steam_id64 as SteamId64,
                    u.username as Username,
                    u.created_at as RegisteredAt,
                    COUNT(c.id) as CharactersCount,
                    MAX(c.created_at) as LastCharacterCreated
                FROM users u
                LEFT JOIN characters c ON u.steam_id64 = c.steam_id64
                WHERE u.steam_id64 = @steamId64
                GROUP BY u.steam_id64, u.username, u.created_at;",
                new { steamId64 });
        }

        #endregion
    }

    public class UserStats
    {
        public long SteamId64 { get; set; }
        public string Username { get; set; }
        public DateTime RegisteredAt { get; set; }
        public int CharactersCount { get; set; }
        public DateTime? LastCharacterCreated { get; set; }
    }

    public class UserData
    {
        public int Id { get; set; }
        public long SteamId64 { get; set; }
        public string Username { get; set; }
        public string SessionId { get; set; }
        public string RegIp { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
