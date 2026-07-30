using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Auth.DTOs
{
    public sealed record RegisterRequestDTO(string Name,
    string Email,
    string Password,
    string? Phone = null,
    int RoleId = 1);
}
