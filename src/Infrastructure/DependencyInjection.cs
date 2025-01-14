
using Application;
using Application.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Scrutor;
using Slugify;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddApplication()
            .AddHttpContextAccessor()
            .AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>())
            .AddDbContextFactory<AppDbContext>((options) =>
            {
                options
                    .UseLazyLoadingProxies()
                    .UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
            })
            .Scan(
                selector => selector
                    .FromAssemblies(
                        typeof(DependencyInjection).Assembly,
                        typeof(SlugHelper).Assembly
                    )
                    .AddClasses()
                    .UsingRegistrationStrategy(RegistrationStrategy.Skip)
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
            );
    }
}