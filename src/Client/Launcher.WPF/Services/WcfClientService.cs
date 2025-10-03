using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Launcher.ServiceLib.Contracts;

namespace Launcher.WPF.Services
{
    public class WcfClientService : ClientBase<ILauncherService>, ILauncherService, IDisposable
    {
        private const string DefaultEndpointName = "LauncherTcpEndpoint";
        private bool _disposed = false;

        public WcfClientService(string address)
            : base(CreateBinding(), new EndpointAddress(address))
        {
            ConfigureTimeouts();
        }

        public WcfClientService()
            : base(DefaultEndpointName)
        {
            ConfigureTimeouts();
        }

        private static NetTcpBinding CreateBinding()
        {
            return new NetTcpBinding(SecurityMode.None)
            {
                MaxReceivedMessageSize = 50 * 1024 * 1024,
                MaxBufferSize = 50 * 1024 * 1024,
                OpenTimeout = TimeSpan.FromSeconds(10),
                CloseTimeout = TimeSpan.FromSeconds(10),
                SendTimeout = TimeSpan.FromSeconds(30),
                ReceiveTimeout = TimeSpan.FromMinutes(10),
                ReaderQuotas =
                {
                    MaxArrayLength = int.MaxValue,
                    MaxStringContentLength = int.MaxValue
                }
            };
        }

        private void ConfigureTimeouts()
        {
            // Дополнительная конфигурация таймаутов если нужно
        }

        #region ILauncherService Implementation

        public UserDto AuthenticateSteam(string steamOpenIdResponse)
            => SafeExecute(() => Channel.AuthenticateSteam(steamOpenIdResponse));

        public List<CharacterDto> GetCharacters(string sessionId)
            => SafeExecute(() => Channel.GetCharacters(sessionId));

        public CharacterDto CreateCharacter(string sessionId, string nickname)
            => SafeExecute(() => Channel.CreateCharacter(sessionId, nickname));

        public bool UpdateCharacter(string sessionId, CharacterDto character)
            => SafeExecute(() => Channel.UpdateCharacter(sessionId, character));

        public bool DeleteCharacter(string sessionId, int characterId)
            => SafeExecute(() => Channel.DeleteCharacter(sessionId, characterId));

        public OnlineDto GetServerOnline()
            => SafeExecute(() => Channel.GetServerOnline());

        public NewsResponse GetNews()
            => SafeExecute(() => Channel.GetNews());

        public List<FileInfoDto> GetFileList()
            => SafeExecute(() => Channel.GetFileList());

        public byte[] DownloadFileChunk(string relativePath, long offset, int length)
            => SafeExecute(() => Channel.DownloadFileChunk(relativePath, offset, length));

        public VerificationResult VerifyFiles(string sessionId)
            => SafeExecute(() => Channel.VerifyFiles(sessionId));

        public bool ChangeInstallPath(string sessionId, string newPath)
            => SafeExecute(() => Channel.ChangeInstallPath(sessionId, newPath));

        public SettingsDto GetSettings(string sessionId)
            => SafeExecute(() => Channel.GetSettings(sessionId));

        public bool SetSettings(string sessionId, SettingsDto settings)
            => SafeExecute(() => Channel.SetSettings(sessionId, settings));

        public string Ping()
            => SafeExecute(() => Channel.Ping());

        #endregion

        #region Safe Execution and Error Handling

        private T SafeExecute<T>(Func<T> action)
        {
            try
            {
                return action();
            }
            catch (TimeoutException ex)
            {
                throw new ServiceException("Превышено время ожидания сервиса", ex);
            }
            catch (CommunicationException ex)
            {
                throw new ServiceException("Ошибка связи с сервисом", ex);
            }
            catch (Exception ex)
            {
                throw new ServiceException("Ошибка при вызове сервиса", ex);
            }
        }

        #endregion

        #region IDisposable Implementation

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                try
                {
                    if (State == CommunicationState.Opened)
                        Close();
                    else
                        Abort();
                }
                catch
                {
                    Abort();
                }
                _disposed = true;
            }
        }

        ~WcfClientService()
        {
            Dispose(false);
        }

        #endregion
    }

    public class ServiceException : Exception
    {
        public ServiceException(string message) : base(message) { }
        public ServiceException(string message, Exception innerException) : base(message, innerException) { }
    }
}