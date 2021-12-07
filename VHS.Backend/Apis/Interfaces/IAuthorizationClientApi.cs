﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VHS.Backend.Apis.Responses;

namespace VHS.Backend.Apis.Interfaces
{
    public interface IAuthorizationClientApi
    {
        Task<UserClientResponse> Authorize(string username, string password);
    }
}