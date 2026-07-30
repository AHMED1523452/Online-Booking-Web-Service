using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Auth.DTOs
{
    public class RevokeRefreshTokenRequestDTO
    {
        public string RefreshToken { get; set;  }
    }
}
