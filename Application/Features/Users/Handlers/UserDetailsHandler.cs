using Application.Common.Interfaces;
using Application.Common.Patterns;
using Application.Features.Users.DTOs;
using Application.Features.Users.Queries;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Stripe.Treasury;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Users.Handlers
{
    public sealed class UserDetailsHandler : IRequestHandler<UserDetailsQuery, GenericResult<UserDetailsResponseDTO>>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ICurrentIUserService currentIUser;
        private readonly IMapper mapper;
        private readonly ICachService<UserDetailsResponseDTO> cachService;

        public UserDetailsHandler(IUnitOfWork unitOfWork, 
                                  ICurrentIUserService currentIUser,
                                  IMapper mapper,
                                  ICachService<UserDetailsResponseDTO> cachService)
        {
            this.unitOfWork = unitOfWork;
            this.currentIUser = currentIUser;
            this.mapper = mapper;
            this.cachService = cachService;
        }
        public async Task<GenericResult<UserDetailsResponseDTO>> Handle(UserDetailsQuery request, CancellationToken cancellationToken)
        {
            //. User must be authenticated , don't forget
            var user_instance = unitOfWork.Repository<passenger>();
            if (user_instance is null) throw new ArgumentNullException(nameof(user_instance));

            var cache_result = await cachService.GetAsync($"User details with id {currentIUser.UserId}", cancellationToken);
            if(cache_result is not null)
                return await Result.SuccessAsync<UserDetailsResponseDTO>(cache_result);

            var exitingUser = await user_instance
                            .GetByIdAsync(predicate: op => op.id == currentIUser.UserId &&
                                                           op.IsDeleted == false,
                                           cancellationToken,
                                           op => op.role); //. to load the data into the server
            if (exitingUser is null) return await Result.FailureAsync<UserDetailsResponseDTO>("User not found. ");
            UserDetailsResponseDTO user_mapped = mapper.Map<UserDetailsResponseDTO>(exitingUser);
            user_mapped.role = exitingUser.role.name;

            //. in any time the details or the data will be changed
            await cachService.SetUserIdScopedAsync($"User details with id {currentIUser.UserId}",currentIUser.UserId ,user_mapped, cancellationToken);

            return await Result.SuccessAsync<UserDetailsResponseDTO>(user_mapped);
        }
    }
}
