using Application.Common.Patterns;
using Application.Features.Auth.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Auth.Commands.ChangePassword
{
    public sealed record ChangePassengerPasswordCommand(ChangePasswordRequestDTO requestDTO) 
                : IRequest<GenericResult<ForgotPasswordResponseDTO>>;
}
