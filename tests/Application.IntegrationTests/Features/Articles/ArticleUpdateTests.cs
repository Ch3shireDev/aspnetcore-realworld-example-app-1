using System.Net;
using Application.Features.Articles.Commands;
using Application.Features.Articles.Queries;
using Application.Features.Profiles.Queries;
using Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace Application.IntegrationTests.Features.Articles;

public class ArticleUpdateTests : TestBase
{
    public ArticleUpdateTests(ConduitApiFactory factory, ITestOutputHelper output) : base(factory, output) { }

    public static IEnumerable<object[]> InvalidArticles()
    {
        yield return new object[]
        {
            new UpdateArticleDTO
            {
                Title = "Test Title",
                Description = "Test Description",
                Body = "",
            }
        };
    }

    [Theory, MemberData(nameof(InvalidArticles))]
    public async Task Cannot_Update_Article_With_Invalid_Data(UpdateArticleDTO article)
    {
        await ActingAs(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
        });

        var response = await Act(HttpMethod.Put, "/articles/test-title", new UpdateArticleBody(article));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Cannot_Update_Non_Existent_Article()
    {
        await ActingAs(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
        });

        var response = await Act(HttpMethod.Put, "/articles/slug-article", new UpdateArticleBody(
            new UpdateArticleDTO
            {
                Title = "New Title",
                Description = "New Description",
                Body = "New Body",
            }
        ));
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Guest_Cannot_Update_Article()
    {
        var response = await Act(HttpMethod.Put, "/articles/slug-article", new UpdateArticleBody(
            new UpdateArticleDTO()
        ));
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Cannot_Update_Article_Of_Other_Author()
    {
        await ActingAs(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
        });

        await _mediator.Send(new NewArticleRequest(
            new NewArticleDTO
            {
                Title = "Test Title",
                Description = "Test Description",
                Body = "Test Body",
            }
        ));

        await ActingAs(new User
        {
            Name = "Jane Doe",
            Email = "jane.doe@example.com",
        });

        var response = await Act(
            HttpMethod.Put, "/articles/test-title",
            new UpdateArticleBody(
                new UpdateArticleDTO
                {
                    Title = "New Title",
                    Description = "New Description",
                    Body = "New Body",
                }
            ));
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Can_Update_Own_Article()
    {
        await ActingAs(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
        });

        await _mediator.Send(new NewArticleRequest(
            new NewArticleDTO
            {
                Title = "Test Title",
                Description = "Test Description",
                Body = "Test Body",
            }
        ));

        var response = await Act<SingleArticleResponse>(HttpMethod.Put, "/articles/test-title",
            new UpdateArticleBody(
                new UpdateArticleDTO
                {
                    Title = "New Title",
                    Description = "New Description",
                }
            )
        );

        response.Article.Should().BeEquivalentTo(new ArticleDTO
        {
            Title = "New Title",
            Description = "New Description",
            Body = "Test Body",
            Slug = "test-title",
            Author = new ProfileDTO
            {
                Username = "John Doe",
            },
            TagList = new List<string>(),
        }, options => options.Excluding(x => x.CreatedAt).Excluding(x => x.UpdatedAt));

        (await _context.Articles.AnyAsync(x => x.Title == "New Title")).Should().BeTrue();
    }
}