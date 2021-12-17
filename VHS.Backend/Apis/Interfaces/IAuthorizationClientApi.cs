using System;
using System.Threading.Tasks;
using VHS.Backend.Apis.Responses;

namespace VHS.Backend.Apis.Interfaces
{
    public interface IAuthorizationClientApi
    {
        UserClientResponse Authorize(string username, string password);
        bool ValidateToken(Guid userId, string accessToken);
    }
}
