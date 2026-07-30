using Application.Common.Patterns;
using Application.Features.Users.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Users.Queries
{
    public sealed record GetUsersDetailsQuery(GetUsersRequestDTO requestDTO) 
                                    : IRequest<PaginatedResult<UserSummaryDTO>>;
}
