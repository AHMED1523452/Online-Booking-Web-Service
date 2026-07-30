using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Users.DTOs
{
    public sealed class UpdateUserRequestDTO
    {
        public string Name { get; set; }

        public string Phone { get; set; }
    }
}
