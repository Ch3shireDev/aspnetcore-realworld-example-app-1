using System.Net;
using Application.Features.Profiles.Queries;
using Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace Application.IntegrationTests.Features.Profiles;

public class ProfileFollowTests : TestBase
{
    public ProfileFollowTests(ConduitApiFactory factory, ITestOutputHelper output) : base(factory, output) { }

    [Fact]
    public async Task Guest_Cannot_Follow_Profile()
    {
        var response = await Act(HttpMethod.Post, "/profiles/celeb_john/follow");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Can_Follow_Profile()
    {
        await ActingAs(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
        });

        await _context.Users.AddRangeAsync(
            new User
            {
                Name = "Jane Doe",
                Email = "jane.doe@example.com",
            },
            new User
            {
                Name = "Alice",
                Email = "alice@example.com",
            }
        );
        await _context.SaveChangesAsync();

        var response = await Act<ProfileResponse>(HttpMethod.Post, "/profiles/celeb_Jane Doe/follow");

        response.Profile.Should().BeEquivalentTo(new ProfileDTO
        {
            Username = "Jane Doe",
            Following = true
        });

        (await _context.Set<FollowerUser>().CountAsync()).Should().Be(1);
        (await _context.Set<FollowerUser>().AnyAsync(x => x.Following.Name == "Jane Doe"))
            .Should().BeTrue();
    }

    [Fact]
    public async Task Can_Unfollow_Profile()
    {
        var user = new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
        };

        user.AddFollowing(
            new User
            {
                Name = "Jane Doe",
                Email = "jane.doe@example.com",
            },
            new User
            {
                Name = "Alice",
                Email = "alice@example.com",
            }
        );

        await ActingAs(user);

        var response = await Act<ProfileResponse>(HttpMethod.Delete, "/profiles/celeb_Jane Doe/follow");

        response.Profile.Should().BeEquivalentTo(new ProfileDTO
        {
            Username = "Jane Doe",
            Following = false
        });

        (await _context.Set<FollowerUser>().CountAsync()).Should().Be(1);
        (await _context.Set<FollowerUser>().AnyAsync(x => x.Following.Name == "Alice"))
            .Should().BeTrue();
    }
}