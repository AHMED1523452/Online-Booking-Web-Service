using Application.Common.Caching;
using Application.Common.Interfaces;
using Application.Common.Patterns;
using Application.Features.Users.DTOs;
using Application.Features.Users.Queries;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using Stripe;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Users.Handlers
{
    public sealed class GetUsersDetailsHandler : IRequestHandler<GetUsersDetailsQuery, PaginatedResult<UserSummaryDTO>>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ICachService<PaginatedResult<UserSummaryDTO>> cachService;
        private readonly ILogger<GetUsersDetailsHandler> logger;

        public GetUsersDetailsHandler(IUnitOfWork unitOfWork,
                                      ICachService<PaginatedResult<UserSummaryDTO>> cacheService,
                                      ILogger<GetUsersDetailsHandler> logger)
        {
            this.unitOfWork = unitOfWork;
            this.cachService = cacheService;
            this.logger = logger;
        }
        public async Task<PaginatedResult<UserSummaryDTO>> Handle(GetUsersDetailsQuery request, CancellationToken cancellationToken)
        {
            var user_instance = unitOfWork.Repository<passenger>();
            if (user_instance is null) throw new ArgumentNullException(nameof(user_instance));

            var cach_result = await cachService.GetAsync($"get-users-" +
                                                         $"{request.requestDTO.PageNumber}-" +
                                                         $"{request.requestDTO.PageSize}-" +
                                                         $"{request.requestDTO.Status}-" +
                                                         $"{request.requestDTO.EmailVerified}-", cancellationToken);
            if (cach_result is not null)
                return cach_result ;

            try
            {
                var users_result = await user_instance
                       .GetPaginationAsync<UserSummaryDTO>(predicate: u =>
                                                            !u.IsDeleted &&
                                                            (!request.requestDTO.RoleId.HasValue || u.role_id == request.requestDTO.RoleId.Value) &&
                                                            (string.IsNullOrWhiteSpace(request.requestDTO.Status) || u.status == request.requestDTO.Status) &&
                                                            (!request.requestDTO.EmailVerified.HasValue || u.is_email_verified == request.requestDTO.EmailVerified.Value) &&
                                                            (!request.requestDTO.IsRevoked.HasValue || u.is_revoked == request.requestDTO.IsRevoked.Value),
                                                           selector: op => new UserSummaryDTO
                                                           {
                                                               CreatedAt = op.created_at,
                                                               Email = op.email,
                                                               Id = op.id,
                                                               Name = op.name,
                                                               IsEmailVerified = op.is_email_verified,
                                                               Phone = op.phone,
                                                               Role = op.role.name,
                                                               Status = op.status
                                                           },
                                                           page: request.requestDTO.PageNumber, pageSize: request.requestDTO.PageSize,
                                                           cancellationToken: cancellationToken, includes: op => op.role);

                await cachService.SetAsync($"get-users-" +
                                                         $"{request.requestDTO.PageNumber}-" +
                                                         $"{request.requestDTO.PageSize}-" +
                                                         $"{request.requestDTO.Status}-" +
                                                         $"{request.requestDTO.EmailVerified}-", users_result, cancellationToken);
                return users_result;
            }catch (Exception ex)
            {
                logger.LogError("Something invalid occurred with message {message}", ex.Message);
                throw new Exception(ex.Message);
            }
        }
    }
}
