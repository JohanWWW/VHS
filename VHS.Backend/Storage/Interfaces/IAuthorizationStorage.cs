using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VHS.Backend.Storage.Interfaces
{
    public interface IAuthorizationStorage
    {
        void AddToken(string token, Guid userId);
        bool TryGetUserId(string token, out Guid userId);
    }
}
