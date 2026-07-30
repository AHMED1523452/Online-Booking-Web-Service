using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Users.DTOs
{
    public class UserSummaryDTO
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public string Role { get; set; }

        public string Status { get; set; }

        public bool? IsEmailVerified { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
