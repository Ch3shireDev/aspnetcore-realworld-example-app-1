using Application.Exceptions;
using Application.Features.Articles.Commands;
using Application.Features.Articles.Queries;
using Application.Features.Profiles.Queries;
using Domain.Entities;
using FluentAssertions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace Application.IntegrationTests.Articles;

public class ArticleCreateTests : TestBase
{
    public ArticleCreateTests(Startup factory, ITestOutputHelper output) : base(factory, output) { }

    public static IEnumerable<object[]> Data => new List<object[]>
        {
            new [] { new NewArticleDTO {
                Title = "",
                Description = "Test Description",
                Body = "Test Body",
            } },
            new [] { new NewArticleDTO {
                Title = "Test Title",
                Description = "",
                Body = "Test Body",
            } },
            new [] { new NewArticleDTO {
                Title = "Test Title",
                Description = "Test Description",
                Body = "",
            } },
            new [] { new NewArticleDTO {
                Title = "Existing Title",
                Description = "Test Description",
                Body = "Test Body",
            } },
        };

    [Theory]
    [MemberData(nameof(Data))]
    public async Task Cannot_Create_Article_With_Invalid_Data(NewArticleDTO article)
    {
        await ActingAs(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
        });

        await Mediator.Send(new NewArticleRequest(
            new NewArticleDTO
            {
                Title = "Existing Title",
                Description = "Test Description",
                Body = "Test Body",
            }
        ));

        await this.Invoking(x => x.Act(new NewArticleRequest(article)))
            .Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Guest_Cannot_Create_Article()
    {
        await this.Invoking(x => x.Act(new NewArticleRequest(
            new NewArticleDTO()
        )))
            .Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Can_Create_Article()
    {
        await ActingAs(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
            Bio = "My Bio",
            Image = "https://i.pravatar.cc/300"
        });

        await Context.Tags.AddAsync(new Tag
        {
            Name = "Existing Tag"
        });
        await Context.SaveChangesAsync();

        var response = await Act(
            new NewArticleRequest(
                new NewArticleDTO
                {
                    Title = "Test Article",
                    Description = "Test Description",
                    Body = "Test Body",
                    TagList = new List<string> { "Test Tag 1", "Test Tag 2", "Existing Tag" }
                }
            )
        );

        response.Article.Should().BeEquivalentTo(new ArticleDTO
        {
            Title = "Test Article",
            Description = "Test Description",
            Body = "Test Body",
            Slug = "test-article",
            Author = new ProfileDTO
            {
                Username = "John Doe",
                Bio = "My Bio",
                Image = "https://i.pravatar.cc/300"
            },
            TagList = new List<string> { "Test Tag 1", "Test Tag 2", "Existing Tag" },
        }, options => options.Excluding(x => x.CreatedAt).Excluding(x => x.UpdatedAt));

        (await Context.Articles.AnyAsync()).Should().BeTrue();
        (await Context.Tags.CountAsync()).Should().Be(3);
    }
}
