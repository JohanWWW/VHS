using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using VHS.Backend.Entities;
using VHS.Backend.Exceptions.Http;
using VHS.Backend.Repositories.Interfaces;

namespace VHS.Backend.Repositories
{
    public class AuthorizationApi : IAuthorizationRepository
    {
        private readonly HttpClient _client;

        public AuthorizationApi()
        {
            _client = new()
            {
                BaseAddress = new("https://kyhdev.hiqcloud.net/")
            };
        }

        public async Task<AuthorizationResponseEntity> Authorize(string username, string password)
        {
            Uri requestUri = new($"api/cds/v1.0/user/authenticate?userName={HttpUtility.UrlEncode(username)}&pwd={HttpUtility.UrlEncode(password)}", UriKind.Relative);
            HttpResponseMessage response = await _client.GetAsync(requestUri);
            return response.StatusCode switch
            {
                System.Net.HttpStatusCode.OK => JsonSerializer.Deserialize<AuthorizationResponseEntity>(await response.Content.ReadAsStringAsync(), new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }),
                _ => throw new HttpResponseException(response.StatusCode),
            };
        }
    }
}
