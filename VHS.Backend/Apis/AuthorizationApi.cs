using RestSharp;
using System;
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
            var response = _restClient.Execute<UserClientResponse>(request);

            if (!response.IsSuccessful)
            {
                return new UserClientResponse
                {
                    StatusCode = response.StatusCode,
                    StatusMessage = response.Content
                };
            }

            UserClientResponse data = response.Data;
            data.StatusCode = response.StatusCode;
            data.StatusMessage = null;

            return data;
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
