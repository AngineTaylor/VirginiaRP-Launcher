using Launcher.ServiceLib.Contracts;
using Launcher.ServiceLib.Data;
using Launcher.WPF.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;

namespace Launcher.ServiceLib
{
    public class LauncherService : ILauncherService
    {
        private readonly AuthService _authService = new();
        private readonly DbManager _dbManager = new(Database.ConnectionString);
        private readonly UserRepository _userRepo = new();
        private readonly NewsService _newsService = new();

        public string Ping() => "Сервис работает!";

        public UserDto AuthenticateSteam(string steamResponseUrl)
        {
            try
            {
                var userDto = _authService.AuthenticateSteamAsync(steamResponseUrl).GetAwaiter().GetResult();
                var userId = _userRepo.EnsureUserBySteamId(userDto.SteamId, userDto.UserName);
                _userRepo.UpdateSession(userId, userDto.SessionId);

                return userDto;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Service] Ошибка авторизации Steam: {ex.Message}");
                throw new FaultException($"Ошибка авторизации: {ex.Message}");
            }
        }

        public List<CharacterDto> GetCharacters(string sessionId)
        {
            try
            {
                var steamId = GetSteamIdFromSession(sessionId);
                if (string.IsNullOrEmpty(steamId) || !long.TryParse(steamId, out var steamId64))
                    return new List<CharacterDto>();

                var characters = _dbManager.GetCharacters(steamId64);
                return characters.Select(ToCharacterDto).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Service] Ошибка получения персонажей: {ex.Message}");
                return new List<CharacterDto>();
            }
        }

        public CharacterDto CreateCharacter(string sessionId, string name)
        {
            try
            {
                var steamId = GetSteamIdFromSession(sessionId);
                if (string.IsNullOrEmpty(steamId) || !long.TryParse(steamId, out var steamId64))
                    throw new InvalidOperationException("Неверная сессия");

                var characterData = new CharacterData
                {
                    Nickname = name?.Trim(),
                    Age = 18,
                    SteamId64 = steamId64,
                    RegIp = "127.0.0.1",
                    CreatedAt = DateTime.Now
                };

                var characterId = _dbManager.SaveCharacter(characterData);
                var characters = _dbManager.GetCharacters(steamId64);
                var created = characters.FirstOrDefault(c => c.Id == characterId);

                return created == null
                    ? throw new InvalidOperationException("Персонаж не создан")
                    : ToCharacterDto(created);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Service] Ошибка создания персонажа: {ex.Message}");
                throw new FaultException($"Ошибка создания персонажа: {ex.Message}");
            }
        }

        public bool UpdateCharacter(string sessionId, CharacterDto character)
        {
            try
            {
                var steamId = GetSteamIdFromSession(sessionId);
                if (string.IsNullOrEmpty(steamId)) return false;

                var characterData = new CharacterData
                {
                    Id = character.Id,
                    ShortId = character.ShortId,
                    Nickname = character.Nickname,
                    Age = character.Age,
                    Story = character.Story,
                    SteamId64 = character.SteamId64,
                    RegIp = character.RegIp
                };

                return _dbManager.UpdateCharacter(characterData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Service] Ошибка обновления: {ex.Message}");
                return false;
            }
        }

        public bool DeleteCharacter(string sessionId, int characterId)
        {
            try
            {
                var steamId = GetSteamIdFromSession(sessionId);
                return !string.IsNullOrEmpty(steamId) && _dbManager.DeleteCharacter(characterId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Service] Ошибка удаления: {ex.Message}");
                return false;
            }
        }

        public OnlineDto GetServerOnline()
        {
            try
            {
                var count = _dbManager.GetOnlineCount();
                return new OnlineDto { Count = count, Timestamp = DateTime.UtcNow };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Service] Ошибка получения онлайна: {ex.Message}");
                return new OnlineDto { Count = 0, Timestamp = DateTime.UtcNow };
            }
        }

        public NewsResponse GetNews()
        {
            try
            {
                return _newsService.GetNews();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Service] Ошибка получения новостей: {ex.Message}");
                return new NewsResponse();
            }
        }

        // Заглушки для нереализованных методов
        public List<FileInfoDto> GetFileList() => new List<FileInfoDto>();
        public byte[] DownloadFileChunk(string path, long offset, int length) => Array.Empty<byte>();
        public VerificationResult VerifyFiles(string sessionId) => new VerificationResult { IsSuccess = true, MissingFiles = new List<string>() };
        public bool ChangeInstallPath(string sessionId, string path) => true;
        public SettingsDto GetSettings(string sessionId) => new SettingsDto { InstallPath = "" };
        public bool SetSettings(string sessionId, SettingsDto settings) => true;

        #region Helper Methods

        private string GetSteamIdFromSession(string sessionId)
        {
            return _userRepo.GetSteamIdBySession(sessionId);
        }

        private static CharacterDto ToCharacterDto(CharacterData data)
        {
            return new CharacterDto
            {
                Id = data.Id,
                ShortId = data.ShortId,
                Nickname = data.Nickname,
                Age = data.Age,
                Story = data.Story,
                SteamId64 = data.SteamId64,
                CreatedAt = data.CreatedAt,
                RegIp = data.RegIp
            };
        }

        #endregion
    }
}