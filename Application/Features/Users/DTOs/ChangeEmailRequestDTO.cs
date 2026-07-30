using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Users.DTOs
{
    public sealed class ChangeEmailRequestDTO
    {
        public string NewEmail { get; set; }
        public string CurrentPassword { get; set; }
    }
}
