using Application.Common.Patterns;
using Application.Features.Auth.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Auth.Commands.ResetPassword
{
    public sealed record ResetPasswordCoammand(ResetPasswordRequestDTO requestDTO) : IRequest<GenericResult<ForgotPasswordResponseDTO>>;
}
