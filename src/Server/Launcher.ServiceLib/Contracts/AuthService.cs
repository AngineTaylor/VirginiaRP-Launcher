using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Launcher.ServiceLib.Contracts
{
    public class AuthService : IAuthService
    {
        private readonly string _steamApiKey = "23C4B86C15DFDE7D2BA70332943D82CA";
        private readonly string _steamLoginUrl = "https://steamcommunity.com/openid/login";
        private readonly HttpClient _httpClient;

        public AuthService()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<UserDto> AuthenticateSteamAsync(string steamOpenIdResponse)
        {
            if (string.IsNullOrWhiteSpace(steamOpenIdResponse))
                throw new ArgumentException("steamOpenIdResponse не может быть пустым");

            var isValid = await ValidateSteamOpenIdResponseAsync(steamOpenIdResponse);
            if (!isValid)
                throw new UnauthorizedAccessException("Невалидный OpenID ответ от Steam");

            var steamId = ExtractSteamIdFromOpenIdResponse(steamOpenIdResponse);
            if (string.IsNullOrWhiteSpace(steamId))
                throw new UnauthorizedAccessException("Не удалось извлечь SteamID");

            var (userName, avatarUrl) = await GetSteamUserInfoAsync(steamId);
            if (string.IsNullOrWhiteSpace(userName))
                throw new UnauthorizedAccessException("Не удалось получить информацию о пользователе");

            return new UserDto
            {
                SteamId = steamId,
                UserName = userName,
                AvatarUrl = avatarUrl,
                SessionId = Guid.NewGuid().ToString()
            };
        }

        private async Task<bool> ValidateSteamOpenIdResponseAsync(string openIdResponse)
        {
            try
            {
                var parameters = ParseQueryString(openIdResponse);

                if (parameters["openid.mode"] != "id_res" ||
                    string.IsNullOrEmpty(parameters["openid.claimed_id"]) ||
                    string.IsNullOrEmpty(parameters["openid.identity"]) ||
                    string.IsNullOrEmpty(parameters["openid.sig"]))
                {
                    Console.WriteLine("Отсутствуют обязательные параметры OpenID");
                    return false;
                }

                var claimedId = parameters["openid.claimed_id"];
                var identity = parameters["openid.identity"];

                if (!claimedId.Contains("steamcommunity.com/openid/id/") ||
                    !identity.Contains("steamcommunity.com/openid/id/"))
                {
                    Console.WriteLine("Некорректный формат SteamID в параметрах");
                    return false;
                }

                var steamId = ExtractSteamIdFromOpenIdResponse(openIdResponse);
                if (string.IsNullOrWhiteSpace(steamId) || steamId.Length != 17 || !steamId.StartsWith("7656"))
                {
                    Console.WriteLine($"Некорректный SteamID: {steamId}");
                    return false;
                }

                Console.WriteLine($"OpenID валидация пройдена для SteamID: {steamId}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка валидации OpenID: {ex.Message}");
                return false;
            }
        }

        private string ExtractSteamIdFromOpenIdResponse(string openIdResponse)
        {
            try
            {
                var parameters = ParseQueryString(openIdResponse);
                var claimedId = parameters["openid.claimed_id"];

                if (string.IsNullOrEmpty(claimedId))
                    return null;

                var steamId = claimedId.Split('/').Last();
                return steamId.All(char.IsDigit) ? steamId : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка извлечения SteamID: {ex.Message}");
                return null;
            }
        }

        private async Task<(string userName, string avatarUrl)> GetSteamUserInfoAsync(string steamId)
        {
            if (string.IsNullOrWhiteSpace(steamId))
                return (null, null);

            try
            {
                var url = $"https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v2/?key={_steamApiKey}&steamids={steamId}";
                var response = await _httpClient.GetStringAsync(url).ConfigureAwait(false);
                var json = JObject.Parse(response);
                var player = json["response"]?["players"]?.FirstOrDefault();

                var userName = player?["personaname"]?.ToString();
                var avatarUrl = player?["avatarfull"]?.ToString();

                return (userName, avatarUrl);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка получения информации о пользователе: {ex.Message}");
                return (null, null);
            }
        }

        public string GetSteamLoginUrl(string returnUrl)
        {
            var baseUri = new Uri(returnUrl);
            var realm = baseUri.GetLeftPart(UriPartial.Authority);

            var parameters = new[]
            {
                $"openid.ns={Uri.EscapeDataString("http://specs.openid.net/auth/2.0")}",
                $"openid.mode=checkid_setup",
                $"openid.return_to={Uri.EscapeDataString(returnUrl)}",
                $"openid.realm={Uri.EscapeDataString(realm)}",
                $"openid.identity={Uri.EscapeDataString("http://specs.openid.net/auth/2.0/identifier_select")}",
                $"openid.claimed_id={Uri.EscapeDataString("http://specs.openid.net/auth/2.0/identifier_select")}"
            };

            var queryString = string.Join("&", parameters);
            return $"{_steamLoginUrl}?{queryString}";
        }

        private Dictionary<string, string> ParseQueryString(string query)
        {
            var parameters = new Dictionary<string, string>();

            if (string.IsNullOrEmpty(query))
                return parameters;

            if (query.StartsWith("?"))
                query = query.Substring(1);

            var pairs = query.Split('&');
            foreach (var pair in pairs)
            {
                var parts = pair.Split('=');
                if (parts.Length == 2)
                {
                    var key = Uri.UnescapeDataString(parts[0]);
                    var value = Uri.UnescapeDataString(parts[1]);
                    parameters[key] = value;
                }
            }

            return parameters;
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}