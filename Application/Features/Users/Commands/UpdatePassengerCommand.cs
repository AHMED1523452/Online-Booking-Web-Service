using Application.Common.Patterns;
using Application.Features.Users.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Users.Commands
{
    public sealed record UpdateUserCommand(UpdateUserRequestDTO requestDTO) : IRequest<GenericResult<UpdateUserResponseDTO>>;
}
