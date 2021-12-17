using RestSharp;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using VHS.Backend.Apis.Interfaces;
using VHS.Backend.Apis.Responses;

namespace VHS.Backend.Apis
{
    public class AuthorizationApi : IAuthorizationClientApi
    {
        private readonly IRestClient _restClient;

        ~AuthorizationApi()
        {
        }

        public AuthorizationApi()
        {
            _restClient = new RestClient("https://kyhdev.hiqcloud.net/");
        }

        public UserClientResponse Authorize(string username, string password)
        {
            var request = new RestRequest($"api/cds/v1.0/user/authenticate?userName={HttpUtility.UrlEncode(username)}&pwd={HttpUtility.UrlEncode(password)}", Method.GET);
            var response = _restClient.Execute(request);

            if (!response.IsSuccessful)
            {
                return new UserClientResponse
                {
                    StatusCode = response.StatusCode,
                    StatusMessage = response.Content
                };
            }

            UserClientResponse clientResponse = JsonSerializer.Deserialize<UserClientResponse>(response.Content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            clientResponse.StatusCode = response.StatusCode;
            clientResponse.StatusMessage = null;

            return clientResponse;
        }

        public bool ValidateToken(Guid userId, string accessToken)
        {
            var request = new RestRequest($"/api/cds/v1.0/user/{userId}/validate?token={HttpUtility.UrlEncode(accessToken)}", Method.GET);
            var response = _restClient.Execute(request);
            if (response.IsSuccessful)
            {
                return response.Content.Equals("true", StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }
    }
}
