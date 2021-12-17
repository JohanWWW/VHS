using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VHS.Backend.Filters;

namespace VHS.Backend.Attributes
{
    public class VHSAuthorizationAttribute : TypeFilterAttribute
    {
        public VHSAuthorizationAttribute() : base(typeof(ClaimFilter))
        {
        }
    }
}
