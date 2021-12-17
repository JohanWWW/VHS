using RestSharp;
using System;
using System.Threading.Tasks;
using System.Web;
using VHS.Backend.Apis.Interfaces;
using VHS.Backend.Apis.Responses;

namespace VHS.Backend.Apis
{
    public class UserAccountApi : IUserAccountClientApi
    {
        private readonly IRestClient _restClient;

        public UserAccountApi()
        {
            _restClient = new RestClient("https://kyhdev.hiqcloud.net/");
        }

        public async Task<VehicleClientResponse> Register(Guid customerId, string vin, string accessToken)
        {
            var request = new RestRequest($"api/cds/v1.0/vehicle/owner/{HttpUtility.UrlEncode(vin)}/{HttpUtility.UrlEncode(customerId.ToString())}");
            request.Method = Method.POST;
            request.AddHeader("accept", "text/plain");
            request.AddHeader("kyh-auth", accessToken);

            var response = await _restClient.ExecuteAsync<VehicleClientResponse>(request);

            if (!response.IsSuccessful)
            {
                return new VehicleClientResponse
                {
                    StatusCode = response.StatusCode,
                    StatusMessage = response.ErrorMessage
                };
            }

            VehicleClientResponse data = response.Data;
            data.StatusCode = response.StatusCode;
            data.StatusMessage = null;

            return data;
        }
    }
}
