using System;

namespace VHS.Backend.Entities
{
    public class UserEntity
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; }
        public string AccessToken { get; set; }
        public Guid CustomerId { get; set; }
    }
}
