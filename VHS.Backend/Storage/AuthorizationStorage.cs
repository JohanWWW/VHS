using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VHS.Backend.Storage.Interfaces;

namespace VHS.Backend.Storage
{
    public class AuthorizationStorage : IAuthorizationStorage
    {
        private readonly IDictionary<string, Guid> _tokens;

        public AuthorizationStorage()
        {
            _tokens = new Dictionary<string, Guid>();
        }

        public void AddToken(string token, Guid userId)
        {
            lock (_tokens)
            {
                if (_tokens.ContainsKey(token))
                {
                    _tokens[token] = userId;
                }
                else
                {
                    _tokens.Add(token, userId);
                }
            }
        }

        public bool TryGetUserId(string token, out Guid userId)
        {
            userId = Guid.Empty;
            var result = false;
            lock (_tokens)
            {
                result = _tokens.TryGetValue(token, out userId);
            }
            return result;
        }
    }
}
