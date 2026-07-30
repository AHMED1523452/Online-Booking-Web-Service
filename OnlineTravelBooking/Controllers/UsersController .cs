using Amazon.Runtime;
using Amazon.S3.Model.Internal.MarshallTransformations;
using Application.Common.Patterns;
using Application.Features.Users.Commands;
using Application.Features.Users.DTOs;
using Application.Features.Users.Handlers;
using Application.Features.Users.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Reflection;

namespace OnlineTravelBooking.Controllers
{
    [ApiController]
    [Route("api/users")]
    [EnableRateLimiting("auth-fixed-window")]
    public class UsersController : ControllerBase
    {
        private readonly IMediator mediator;

        public UsersController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PaginatedResult<UserSummaryDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PaginatedResult<UserSummaryDTO>>>
                        UsersDeatilsAsync([FromQuery] GetUsersRequestDTO requestDTO, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new GetUsersDetailsQuery(requestDTO), cancellationToken);
            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }

        [HttpGet("me")]
        [ProducesResponseType(typeof(PaginatedResult<UserSummaryDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize]
        public async Task<ActionResult<PaginatedResult<UserSummaryDTO>>>
                        UsersDeatilsAsync(CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new UserDetailsQuery(), cancellationToken);
            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }

        [HttpPatch("me")]
        [ProducesResponseType(typeof(GenericResult<UpdateUserResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize]
        public async Task<ActionResult<GenericResult<UpdateUserResponseDTO>>> UpdateUserDataAsync([FromBody] UpdateUserRequestDTO requestDTO, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new UpdateUserCommand(requestDTO), cancellationToken);
            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }

        [HttpPatch("change-email")]
        [ProducesResponseType(typeof(GenericResult<ChangeEmailResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize]
        public async Task<ActionResult<GenericResult<ChangeEmailResponseDTO>>> ChangeEmailAsync([FromBody] ChangeEmailRequestDTO requestDTO,
                                                                                                  CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new ChangeUserEmailCommand(requestDTO), cancellationToken);
            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }
    }
}
 