using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VHS.Backend.Apis.Responses
{
    public class VehicleClientResponse : ClientResponseBase
    {
        public string Vin { get; set; }
        public string RegNo { get; set; }
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public string Color { get; set; }
        public VehicleOwner Owner { get; set; }

        public class VehicleOwner
        {
            public Guid Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string City { get; set; }
            public string PhoneNumber { get; set; }
            public UserClientResponse User { get; set; }
            public int OwnerStatus { get; set; }
        }
    }
}
