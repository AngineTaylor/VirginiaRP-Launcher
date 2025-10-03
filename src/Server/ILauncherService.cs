using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Runtime.Serialization;

namespace Launcher.Server
{
    [ServiceContract]
    public interface ILauncherService
    {
        [OperationContract]
        UserDto AuthenticateSteam(string steamId);

        [OperationContract]
        List<CharacterDto> GetCharacters(string sessionId);

        [OperationContract]
        CharacterDto CreateCharacter(string sessionId, string name, int avatarId);

        [OperationContract]
        bool UpdateCharacter(string sessionId, CharacterDto character);

        [OperationContract]
        bool DeleteCharacter(string sessionId, int characterId);

        [OperationContract]
        OnlineDto GetServerOnline();

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

    [DataContract]
    public class UserDto
    {
        [DataMember] public string SteamId { get; set; }
        [DataMember] public string NickName { get; set; }
        [DataMember] public string SessionId { get; set; }
        [DataMember] public bool FirstLaunch { get; set; }
        [DataMember] public int Version { get; set; } = 1;
    }

    [DataContract]
    public class CharacterDto
    {
        [DataMember] public int Id { get; set; }
        [DataMember] public string Name { get; set; }
        [DataMember] public string InternalName { get; set; }
        [DataMember] public int AvatarId { get; set; }
        [DataMember] public int UserId { get; set; }
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
        [DataMember] public List<string> MissingFiles { get; set; }
        [DataMember] public int Version { get; set; } = 1;
    }

    [DataContract]
    public class SettingsDto
    {
        [DataMember] public string InstallPath { get; set; }
        [DataMember] public int Version { get; set; } = 1;
    }
}
