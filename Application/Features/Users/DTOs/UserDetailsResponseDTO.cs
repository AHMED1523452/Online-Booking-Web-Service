using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Application.Features.Users.DTOs
{
    public sealed class UserDetailsResponseDTO
    {
        public long id { get; set; }
        public string name { get; set; }

        public string email { get; set; }

        public string phone { get; set; }

        public bool? is_email_verified { get; set; }

        public string status { get; set; }

        public string role { get; set; }

        public DateTime created_at { get; set; }

        public DateTime? updated_at { get; set; }

    }
}
