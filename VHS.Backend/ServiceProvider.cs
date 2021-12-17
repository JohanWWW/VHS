using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VHS.Backend
{
    public class ServiceProvider
    {
        private static Services _services;

        public static Services Current => _services ??= new Services();
    }
}
