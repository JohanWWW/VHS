using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using VHS.Backend.Apis.Interfaces;
using VHS.Backend.Apis.Responses;

namespace VHS.Backend.Apis
{
    public class UserAccountApi : IUserAccountClientApi
    {
        private readonly HttpClient _client;

        public UserAccountApi()
        {
            _client = new HttpClient
            {
                BaseAddress = new("https://kyhdev.hiqcloud.net/")
            };
        }

        public async Task<VehicleClientResponse> Register(Guid customerId, string vin, string accessToken)
        {
            Uri requestUri = new($"api/cds/v1.0/vehicle/owner/{HttpUtility.UrlEncode(vin)}/{HttpUtility.UrlEncode(customerId.ToString())}", UriKind.Relative);
            var request = new HttpRequestMessage
            {
                RequestUri = requestUri,
                Method = HttpMethod.Post,
                Headers =
                {
                    { "accept", "text/plain" },
                    { "kyh-auth", accessToken },
                }
            };

            HttpResponseMessage response = await _client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                return new VehicleClientResponse
                {
                    StatusCode = response.StatusCode,
                    StatusMessage = await response.Content.ReadAsStringAsync()
                };

            VehicleClientResponse pairResponse = JsonSerializer.Deserialize<VehicleClientResponse>(await response.Content.ReadAsStringAsync(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            pairResponse.StatusCode = response.StatusCode;
            pairResponse.StatusMessage = null;

            return pairResponse;
        }
    }
}
