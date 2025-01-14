using Application.Extensions;
using Application.Features.Auth.Queries;
using Application.Interfaces;
using Application.Interfaces.Mediator;

namespace Application.Features.Profiles.Queries;

public class ProfileDTO
{
    public string Username { get; set; } = default!;

    public string? Bio { get; set; }

    public string? Image { get; set; }

    public bool Following { get; set; }
}

public record ProfileResponse(ProfileDTO Profile);

public record ProfileGetQuery(string Username) : IQuery<ProfileResponse>;

public class ProfileGetHandler : IQueryHandler<ProfileGetQuery, ProfileResponse>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUser _currentUser;

    public ProfileGetHandler(IAppDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ProfileResponse> Handle(ProfileGetQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FindAsync(x => x.Name == request.Username, cancellationToken);

        return new ProfileResponse(user.MapToProfile(_currentUser.User));
    }
}