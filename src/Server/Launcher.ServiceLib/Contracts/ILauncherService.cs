using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Launcher.ServiceLib.Contracts
{
    [ServiceContract]
    public interface ILauncherService
    {
        [OperationContract]
        string Ping();

        [OperationContract]
        UserDto AuthenticateSteam(string steamOpenIdResponse);

        [OperationContract]
        List<CharacterDto> GetCharacters(string sessionId);

        [OperationContract]
        CharacterDto CreateCharacter(string sessionId, string nickname);

        [OperationContract]
        bool UpdateCharacter(string sessionId, CharacterDto character);

        [OperationContract]
        bool DeleteCharacter(string sessionId, int characterId);

        [OperationContract]
        OnlineDto GetServerOnline();

        [OperationContract]
        NewsResponse GetNews();

        // --- Файловые операции ---
        [OperationContract]
        List<FileInfoDto> GetFileList();

        [OperationContract]
        byte[] DownloadFileChunk(string relativePath, long offset, int length);

        [OperationContract]
        VerificationResult VerifyFiles(string sessionId);

        [OperationContract]
        bool ChangeInstallPath(string sessionId, string newPath);

        [OperationContract]
        SettingsDto GetSettings(string sessionId);

        [OperationContract]
        bool SetSettings(string sessionId, SettingsDto settings);
    }


// =============== DTO ===============

[DataContract]
    public class UserDto
    {
        [DataMember] public string SteamId { get; set; }
        [DataMember] public string UserName { get; set; }
        [DataMember] public string SessionId { get; set; }
        [DataMember] public string AvatarUrl { get; set; }
        [DataMember] public int Version { get; set; } = 1;
    }

    [DataContract]
    public class CharacterDto
    {
        [DataMember] public int Id { get; set; }
        [DataMember] public int ShortId { get; set; }
        [DataMember] public string Nickname { get; set; }
        [DataMember] public int Age { get; set; }
        [DataMember] public string Story { get; set; }
        [DataMember] public long SteamId64 { get; set; }
        [DataMember] public DateTime CreatedAt { get; set; }
        [DataMember] public string RegIp { get; set; }
        [DataMember] public int Version { get; set; } = 1;
    }

    [DataContract]
    public class FileInfoDto
    {
        [DataMember] public string RelativePath { get; set; }
        [DataMember] public long Size { get; set; }
        [DataMember] public string SHA256 { get; set; }
        [DataMember] public int Version { get; set; } = 1;
    }

    [DataContract]
    public class OnlineDto
    {
        [DataMember] public int Count { get; set; }
        [DataMember] public DateTime Timestamp { get; set; }
        [DataMember] public int Version { get; set; } = 1;
    }

    [DataContract]
    public class VerificationResult
    {
        [DataMember] public bool IsSuccess { get; set; }
        [DataMember] public List<string> MissingFiles { get; set; } = [];
        [DataMember] public int Version { get; set; } = 1;
    }

    [DataContract]
    public class SettingsDto
    {
        [DataMember] public string InstallPath { get; set; } = string.Empty;
        [DataMember] public int Version { get; set; } = 1;
    }
}