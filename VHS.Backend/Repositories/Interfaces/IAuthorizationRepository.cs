using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VHS.Backend.Entities;

namespace VHS.Backend.Repositories.Interfaces
{
    public interface IAuthorizationRepository
    {
        Task<AuthorizationResponseEntity> Authorize(string username, string password);
    }
}
