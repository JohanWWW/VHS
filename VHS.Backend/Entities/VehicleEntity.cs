using System;

namespace VHS.Backend.Entities
{
    public class VehicleEntity
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
            public UserEntity User { get; set; }
            public int OwnerStatus { get; set; }
        }
    }
}
