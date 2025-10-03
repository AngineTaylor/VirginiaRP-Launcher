using System.ServiceModel;
using System.Threading.Tasks;

namespace Launcher.ServiceLib.Contracts
{
    [ServiceContract]
    public interface IAuthService
    {
        [OperationContract]
        Task<UserDto> AuthenticateSteamAsync(string steamOpenIdResponse);

        [OperationContract]
        string GetSteamLoginUrl(string returnUrl);
    }
}