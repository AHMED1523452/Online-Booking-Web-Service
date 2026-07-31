using Application.Common.Models;
using Application.Common.Patterns;
using Application.Features.Auth.Commands.ChangePassword;
using Application.Features.Auth.Commands.ConfirmEmail;
using Application.Features.Auth.Commands.ForgotPassword;
using Application.Features.Auth.Commands.ResetPassword;
using Application.Features.Auth.Commands.RevokeTokenPassenger;
using Application.Features.Auth.Commands.RevokeTokenPassenger.UnRevokePassengerToken;
using Application.Features.Auth.Commands.Login;
using Application.Features.Auth.Commands.Register;
using Application.Features.Auth.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace OnlineTravelBooking.Controllers;

[Route("api/[controller]")]
[ApiController]
[EnableRateLimiting("auth-fixed-window")]
public sealed class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator) => _mediator = mediator;

    // OnlineTravelBooking\Controllers\AuthController.cs
    [HttpPost("register")]
    [ProducesResponseType(typeof(GenericResult<ForgotPasswordResponseDTO>), StatusCodes.Status200OK )]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GenericResult<ForgotPasswordResponseDTO>>> Register([FromBody] RegisterRequestDTO request, CancellationToken cancellationToken)
    {

        var result = await _mediator.Send(new RegisterCommand(
            request.Name,
            request.Email,
            request.Password,
            request.Phone,
            request.RoleId
        ), cancellationToken);

        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Authenticate a user and receive a JWT token.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDTO request, CancellationToken  cancellationToken)
    {
        var result = await _mediator.Send(new LoginCommand(
            request.Email,
            request.Password
        ),  cancellationToken);

        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }


    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(ApiResponse<RefreshTokenResponseDTO>), StatusCodes .Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize]
    public async Task<ActionResult<ApiResponse<RefreshTokenResponseDTO>>> RefreshToken([FromBody] RefreshTokenRequest request,CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new Application.Features.Auth.Commands.RefreshToken.RefreshTokenCommand(request.RefreshToken),cancellationToken);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }


    [HttpPost("forgot-password")]
    [ProducesResponseType(typeof(GenericResult<ForgotPasswordResponseDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GenericResult<ForgotPasswordResponseDTO>>> 
                    ForgotPasswordAsync([FromBody] ForgotPasswordRequestDTO requestDTO, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ForgotPasswordCommand(requestDTO), cancellationToken);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }


    [HttpPost("reset-password")]
    [ProducesResponseType(typeof(GenericResult<ForgotPasswordResponseDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GenericResult<ForgotPasswordResponseDTO>>> 
                    ResetPasswordAsync([FromBody] ResetPasswordRequestDTO requestDTO, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ResetPasswordCoammand(requestDTO), cancellationToken);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("confirm-email")]
    [ProducesResponseType(typeof(GenericResult<ForgotPasswordResponseDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GenericResult<ForgotPasswordResponseDTO>>> 
                    ConfirmEmailAsync([FromBody] ConfirmEmailRequestDTO requestDTO, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ConfirmEmailCommand(requestDTO), cancellationToken);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("change-password")]
    [ProducesResponseType(typeof(GenericResult<ForgotPasswordResponseDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize]
    public async Task<ActionResult<GenericResult<ForgotPasswordResponseDTO>>> 
                    ChangePasswordAsync([FromBody] ChangePasswordRequestDTO requestDTO, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ChangePassengerPasswordCommand(requestDTO), cancellationToken);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }



    [HttpPost("logout")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize]
    public async Task<ActionResult<ApiResponse<string>>> Logout([FromBody] LogoutRequestDTO request,CancellationToken cancelltionToken)
    {
        var result = await _mediator.Send(new Application.Features.Auth.Commands.Logout.LogoutCommand(request.RefreshToken), cancelltionToken);

        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPatch("revoke-token")]
    [ProducesResponseType(typeof(GenericResult<ForgotPasswordResponseDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<GenericResult<ForgotPasswordResponseDTO>>> 
                    RevokePassengerTokenAsync([FromBody] RevokeRefreshTokenRequestDTO requestDTO, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RevokeRefreshTokenCommand(requestDTO), cancellationToken);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpPatch("un-revoke-token")]
    [ProducesResponseType(typeof(GenericResult<ForgotPasswordResponseDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<GenericResult<ForgotPasswordResponseDTO>>> 
                    UnRevokePassengerTokenAsync([FromBody] RevokeRefreshTokenRequestDTO requestDTO,
                    CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new UnRevokePassengerCommand(requestDTO), cancellationToken);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }
}