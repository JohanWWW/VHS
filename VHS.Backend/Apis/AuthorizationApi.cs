using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using VHS.Backend.Apis.Interfaces;
using VHS.Backend.Apis.Responses;

namespace VHS.Backend.Apis
{
    public class AuthorizationApi : IAuthorizationClientApi
    {
        private readonly HttpClient _client;

        ~AuthorizationApi()
        {
            // Dispose managed resources
            _client.Dispose();
        }

        public AuthorizationApi()
        {
            _client = new HttpClient()
            {
                BaseAddress = new("https://kyhdev.hiqcloud.net/")
            };
        }

        public async Task<UserClientResponse> Authorize(string username, string password)
        {
            Uri requestUri = new($"api/cds/v1.0/user/authenticate?userName={HttpUtility.UrlEncode(username)}&pwd={HttpUtility.UrlEncode(password)}", UriKind.Relative);
            HttpResponseMessage response = await _client.GetAsync(requestUri);

            if (!response.IsSuccessStatusCode)
                return new UserClientResponse
                {
                    StatusCode = response.StatusCode,
                    StatusMessage = await response.Content.ReadAsStringAsync()
                };

            UserClientResponse clientResponse = JsonSerializer.Deserialize<UserClientResponse>(await response.Content.ReadAsStringAsync(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            clientResponse.StatusCode = response.StatusCode;
            clientResponse.StatusMessage = null;

            return clientResponse;
        }
    }
}
