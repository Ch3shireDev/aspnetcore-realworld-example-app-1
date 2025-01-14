using Application.Features.Auth.Commands;
using Application.Features.Auth.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Controllers;

[Route("[controller]")]
[ApiExplorerSettings(GroupName = "User and Authentication")]
public class UsersController
{
    private readonly ISender _sender;

    public UsersController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    /// <remarks>Register a new user</remarks>
    /// <param name="command">Details of the new user to register</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost(Name = "CreateUser")]
    [ProducesResponseType(200)]
    [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
    public Task<UserResponse> Register([FromBody] NewUserRequest command, CancellationToken cancellationToken)
    {
        return _sender.Send(command, cancellationToken);
    }

    /// <summary>
    /// Existing user login
    /// </summary>
    /// <remarks>Login for existing user</remarks>
    /// <param name="command">Credentials to use</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("login", Name = "Login")]
    [ProducesResponseType(200)]
    [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
    public Task<UserResponse> Login([FromBody] LoginUserRequest command, CancellationToken cancellationToken)
    {
        return _sender.Send(command, cancellationToken);
    }
}