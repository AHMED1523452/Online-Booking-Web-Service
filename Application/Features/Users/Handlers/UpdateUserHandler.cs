using Application.Common.Interfaces;
using Application.Common.Patterns;
using Application.Features.Users.Commands;
using Application.Features.Users.DTOs;
using Domain.Entities;
using MediatR;

namespace Application.Features.Users.Handlers
{
    public sealed class UpdateUserHandler : IRequestHandler<UpdateUserCommand, GenericResult<UpdateUserResponseDTO>>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ICurrentIUserService currentIUser;
        private readonly ICachService<UpdateUserResponseDTO> cachService;

        public UpdateUserHandler(IUnitOfWork unitOfWork, 
                                 ICurrentIUserService currentIUser,
                                 ICachService<UpdateUserResponseDTO> cachService)
        {
            this.unitOfWork = unitOfWork;
            this.currentIUser = currentIUser;
            this.cachService = cachService;
        }
        public async Task<GenericResult<UpdateUserResponseDTO>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var user_instance = unitOfWork.Repository<passenger>();
            if (user_instance is null) throw new ArgumentNullException(nameof(user_instance));

            passenger existing_user = await user_instance.GetByIdAsync(predicate: op => op.id == currentIUser.UserId &&
                                                                                  op.IsDeleted == false &&
                                                                                  op.is_email_verified == true &&
                                                                                  op.status == "verified", cancellationToken);
            if (existing_user is null) return await Result.FailureAsync<UpdateUserResponseDTO>("User not found. ");

            existing_user.phone = request.requestDTO.Phone;
            existing_user.name = request.requestDTO.Name;
            existing_user.updated_at = DateTime.UtcNow;
            existing_user.UpdatedBy = currentIUser.UserId;

            await unitOfWork.SaveChangesAsync(cancellationToken);

            await cachService.RemoveAsync($"User details with id {currentIUser.UserId}", cancellationToken);

            return await Result.SuccessAsync<UpdateUserResponseDTO>(new UpdateUserResponseDTO
            {
                Message = "User data updated successfully. "
            });
        }
    }
}
