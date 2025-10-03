using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace Launcher.Server
{
    public class LauncherService : ILauncherService
    {
        private static readonly string FileRepo = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FileRepo");

        public UserDto AuthenticateSteam(string steamId)
        {
            // Minimal stub - в реальном проекте валидировать через Steam Web API
            return new UserDto
            {
                SteamId = steamId,
                NickName = $"User_{steamId}",
                SessionId = Guid.NewGuid().ToString(),
                FirstLaunch = true
            };
        }

        public List<CharacterDto> GetCharacters(string sessionId)
        {
            return new List<CharacterDto>();
        }

        public CharacterDto CreateCharacter(string sessionId, string name, int avatarId)
        {
            var id = new Random().Next(1000, 9999);
            return new CharacterDto
            {
                Id = id,
                Name = name,
                AvatarId = avatarId,
                InternalName = $"{name}_{id}"
            };
        }

        public bool UpdateCharacter(string sessionId, CharacterDto character)
        {
            return true;
        }

        public bool DeleteCharacter(string sessionId, int characterId)
        {
            return true;
        }

        public OnlineDto GetServerOnline()
        {
            return new OnlineDto { Count = 10 + new Random().Next(5,58), Timestamp = DateTime.UtcNow };
        }

        public List<FileInfoDto> GetFileList()
        {
            var manifestPath = Path.Combine(FileRepo, "manifest.json");
            if (!File.Exists(manifestPath))
                return new List<FileInfoDto>();
            var json = File.ReadAllText(manifestPath, Encoding.UTF8);
            try
            {
                var items = System.Text.Json.JsonSerializer.Deserialize<List<FileInfoDto>>(json);
                return items ?? new List<FileInfoDto>();
            }
            catch
            {
                return new List<FileInfoDto>();
            }
        }

        public byte[] DownloadFileChunk(string relativePath, long offset, int length)
        {
            var full = Path.Combine(FileRepo, relativePath);
            if (!File.Exists(full)) return new byte[0];
            using (var fs = new FileStream(full, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                fs.Seek(offset, SeekOrigin.Begin);
                var buffer = new byte[Math.Min(length, (int)Math.Max(0, fs.Length - offset))];
                var read = fs.Read(buffer, 0, buffer.Length);
                if (read != buffer.Length)
                {
                    Array.Resize(ref buffer, read);
                }
                return buffer;
            }
        }

        public VerificationResult VerifyFiles(string sessionId)
        {
            return new VerificationResult { IsSuccess = false, MissingFiles = new List<string>() };
        }

        public bool ChangeInstallPath(string sessionId, string newPath)
        {
            return true;
        }

        public SettingsDto GetSettings(string sessionId)
        {
            return new SettingsDto { InstallPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GameLauncher", "Unturned") };
        }

        public bool SetSettings(string sessionId, SettingsDto settings)
        {
            return true;
        }
    }
}
