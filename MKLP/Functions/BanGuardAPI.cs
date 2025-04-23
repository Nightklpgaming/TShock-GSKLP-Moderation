using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using TShockAPI;

namespace MKLP.Functions
{
    public class BanGuardAPI
    {
        private static readonly HttpClient _client = new HttpClient();
        private static string _apiKey => MKLP.Config.BanGuard.APIKey;
        public static bool _isApiKeyValid { get; private set; } = false;
        private static readonly string _rootURL = "https://banguard.uk/api/";
        private static HttpRequestMessage _generateMessage => new HttpRequestMessage(HttpMethod.Get, _rootURL + "generate-connection-code");
        private static HttpRequestMessage _newConnectionMessage => new HttpRequestMessage(HttpMethod.Post, _rootURL + "new-connection-code");
        private static HttpRequestMessage _checkMessage => new HttpRequestMessage(HttpMethod.Get, _rootURL + "check-player-ban");
        private static HttpRequestMessage _tokenMessage => new HttpRequestMessage(HttpMethod.Get, _rootURL + "check-token");
        private static HttpRequestMessage _banMessage => new HttpRequestMessage(HttpMethod.Post, _rootURL + "ban-player");
        private static HttpRequestMessage _discordCheckMessage => new HttpRequestMessage(HttpMethod.Get, _rootURL + "check-player-connection");

        private static async Task<JObject?> SendApiRequest(HttpRequestMessage message, Dictionary<string, string>? data = null, bool checkToken = true)
        {
            if (checkToken && !_isApiKeyValid)
            {
                TShock.Log.ConsoleError("BanGuard API key is not valid. Please check your configuration.");
                return null;
            }

            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("Authorization", _apiKey);

            if (data != null)
            {
                var content = new FormUrlEncodedContent(data);
                message.Content = content;
            }

            HttpResponseMessage response = await _client.SendAsync(message);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var jsonResponse = JsonConvert.DeserializeObject<JObject>(responseBody);

            return jsonResponse;
        }

        public static void Initialize()
        {
            Task.Run(async () =>
            {
                _isApiKeyValid = await CheckToken();

                if (!_isApiKeyValid)
                {
                    TShock.Log.ConsoleError($"Error validating BanGuard API key. Please check your configuration.");
                }
            });
        }

        public static async Task<bool> CheckToken()
        {
            try
            {
                JObject? response = await SendApiRequest(_tokenMessage, checkToken: false);
                return response!["valid"]!.ToObject<bool>();
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool?> CheckPlayerBan(string uuid, string playerName, string playerIP)
        {
            var requestData = new Dictionary<string, string>
            {
                { "player_uuid", uuid },
                { "player_name", playerName },
                { "player_ip", playerIP}
            };

            try
            {
                JObject? response = await SendApiRequest(_checkMessage, requestData);
                return response!["banned"]!.ToObject<bool>();
            }
            catch (Exception ex)
            {
                TShock.Log.ConsoleError($"Error checking player ban: {ex.Message}");
                return null;
            }
        }

        public static async Task<int?> GenerateNewConnection(string uuid, string playerName)
        {
            try
            {
                var requestData = new Dictionary<string, string>
            {
                { "uuid", uuid },
                { "username", playerName }
            };

                JObject? response = await SendApiRequest(_newConnectionMessage, requestData);
                var code = int.Parse(response!["code"]!.ToString()!);

                return code;
            }
            catch (Exception ex)
            {
                TShock.Log.ConsoleError($"Error generating connection code: {ex.Message}");
                return null;
            }
        }

        public static async Task<bool> BanPlayer(string uuid, string category, string ip)
        {
            var requestData = new Dictionary<string, string>
            {
                { "player", uuid },
                { "category", category },
                { "player_ip", ip }
            };

            try
            {
                JObject? response = await SendApiRequest(_banMessage, requestData);
                return response != null;
            }
            catch (Exception ex)
            {
                TShock.Log.ConsoleError($"Error banning player: {ex.Message}");
                return false;
            }
        }

        /*
        public static async Task<DCAccount?> TryGetDiscordAccount(string uuid)
        {
            var requestData = new Dictionary<string, string>
            {
                { "player_uuid", uuid }
            };

            try
            {
                JObject? response = await SendApiRequest(_discordCheckMessage, requestData);
                return DCAccount.FromJson(response!["connection_data"]!.ToObject<JObject>()!);
            }
            catch
            {
                Console.WriteLine("Error getting Discord account.");
                return null;
            }
        }
        */

        public static string GetCategoryFromReason(string reason)
        {
            switch (reason.ToLower())
            {
                case "dupe":
                case "duping":
                case "duplicating":
                case "duper":
                case "splitdupe":
                case "split dupe":
                case "splitduping":
                case "split duping":
                case "splitduplicating":
                case "split duplicating":
                case "splitduper":
                case "split duper":
                    {
                        return "Duping";
                    }
                case "hack":
                case "hacks":
                case "hacker":
                case "hacking":
                case "cheating":
                case "godmode":
                case "godmodding":
                    {
                        return "hacks";
                    }
                case "nsfw":
                case "inappropriate":
                case "inappropriatecontent":
                case "inappropriate content":
                case "childsafety":
                case "child-safety":
                case "child safety":
                    {
                        return "child-safety";
                    }
                case "grief":
                case "griefing":
                case "destroying":
                case "flooding":
                    {
                        return "griefing";
                    }
                case "tunneling":
                case "hole maker":
                case "tunnel":
                    {
                        return "tunneling";
                    }
            }

            return "N/A";
        }

        public static bool IsCategory(string category)
        {
            return category is "duping" or "hacks" or "griefing" or "tunneling" or "child-safety";
        }

        public static void ChangeTokenFromPlugin()
        {
            MKLP.Config.BanGuard.APIKey = BanGuard.BanGuard.Config.APIKey;
            MKLP.Config.Changeall();
        }
    }
}
