using Application.Common.Patterns;
using Application.Features.Auth.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Auth.Commands.RevokeTokenPassenger
{
    public sealed record RevokeRefreshTokenCommand(RevokeRefreshTokenRequestDTO requestDTO) : IRequest<GenericResult<ForgotPasswordResponseDTO>>;
}
