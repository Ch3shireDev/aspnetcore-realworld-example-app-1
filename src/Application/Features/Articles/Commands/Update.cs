using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Extensions;
using Application.Features.Articles.Queries;
using Application.Interfaces;
using AutoMapper;
using FluentValidation;

namespace Application.Features.Articles.Commands
{
    public class ArticleUpdateDTO
    {
        public string Body { get; set; }
    }

    public record ArticleUpdateCommand(string Slug, ArticleUpdateDTO Article) : IAuthorizationRequest<ArticleEnvelope>;

    public class ArticleUpdateValidator : AbstractValidator<ArticleUpdateCommand>
    {
        public ArticleUpdateValidator()
        {
            RuleFor(x => x.Article.Body).NotNull().NotEmpty();
        }
    }

    public class ArticleUpdateHandler : IAuthorizationRequestHandler<ArticleUpdateCommand, ArticleEnvelope>
    {
        private readonly IAppDbContext _context;
        private readonly IMapper _mapper;

        public ArticleUpdateHandler(IAppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ArticleEnvelope> Handle(ArticleUpdateCommand request, CancellationToken cancellationToken)
        {
            var article = await _context.Articles.FindAsync(x => x.Slug == request.Slug, cancellationToken);

            article.Body = request.Article.Body;

            _context.Articles.Update(article);
            await _context.SaveChangesAsync(cancellationToken);

            return new ArticleEnvelope(_mapper.Map<ArticleDTO>(article));
        }
    }
}