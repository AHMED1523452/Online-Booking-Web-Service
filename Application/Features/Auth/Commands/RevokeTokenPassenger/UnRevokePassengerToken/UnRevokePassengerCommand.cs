using Application.Common.Patterns;
using Application.Features.Auth.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Auth.Commands.RevokeTokenPassenger.UnRevokePassengerToken
{
    public sealed record UnRevokePassengerCommand(RevokeRefreshTokenRequestDTO requestDTO) : IRequest<GenericResult<ForgotPasswordResponseDTO>>;
}
