using Application.Extensions;
using Application.Features.Auth.Queries;
using Application.Features.Profiles.Queries;
using Application.Interfaces;
using Application.Interfaces.Mediator;

namespace Application.Features.Profiles.Commands;

public record ProfileFollowRequest(string Username, bool Follow) : ICommand<ProfileResponse>;

public class ProfileGetHandler : ICommandHandler<ProfileFollowRequest, ProfileResponse>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUser _currentUser;

    public ProfileGetHandler(IAppDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ProfileResponse> Handle(ProfileFollowRequest request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FindAsync(x => x.Name == request.Username, cancellationToken);

        if (request.Follow)
        {
            user.Follow(_currentUser.User!);
        }
        else
        {
            user.Unfollow(_currentUser.User!);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new ProfileResponse(user.MapToProfile(_currentUser.User));
    }
}