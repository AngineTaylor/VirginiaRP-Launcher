ServiceContract: ILauncherService
Методы:
- AuthenticateSteam(string steamId) -> UserDto
- GetCharacters(string sessionId) -> List<CharacterDto>
- CreateCharacter(string sessionId, string name, int avatarId) -> CharacterDto
- UpdateCharacter(string sessionId, CharacterDto character) -> bool
- DeleteCharacter(string sessionId, int characterId) -> bool
- GetServerOnline() -> OnlineDto
- GetFileList() -> List<FileInfoDto>
- DownloadFileChunk(string relativePath, long offset, int length) -> byte[]
- VerifyFiles(string sessionId) -> VerificationResult
- ChangeInstallPath(string sessionId, string newPath) -> bool
- GetSettings(string sessionId) -> SettingsDto
- SetSettings(string sessionId, SettingsDto settings) -> bool

DTOs (DataContract):
- UserDto { string SteamId; string NickName; string SessionId; bool FirstLaunch; int Version; }
- CharacterDto { int Id; string Name; string InternalName; int AvatarId; int UserId; int Version; }
- FileInfoDto { string RelativePath; long Size; string SHA256; int Version; }
- OnlineDto { int Count; DateTime Timestamp; int Version; }
- VerificationResult { bool IsSuccess; List<string> MissingFiles; int Version; }
- SettingsDto { string InstallPath; int Version; }
