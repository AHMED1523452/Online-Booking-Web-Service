using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Auth.DTOs
{
    public sealed record RefreshTokenResponseDTO(string refreshToken, string userName, string Email, string Role);
}
