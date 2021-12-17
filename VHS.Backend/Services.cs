using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VHS.Backend.Storage;

namespace VHS.Backend
{
    public class Services
    {
        private InMemoryStorage _inMemoryStorage;

        public InMemoryStorage InMemoryStorage => _inMemoryStorage ??= new InMemoryStorage();
    }
}
