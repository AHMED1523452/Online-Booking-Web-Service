using Application.Common.Patterns;
using Application.Features.Auth.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Auth.Commands.ConfirmEmail
{
    public sealed record ConfirmEmailCommand(ConfirmEmailRequestDTO requestDTO) : IRequest<GenericResult<ForgotPasswordResponseDTO>>;
}
