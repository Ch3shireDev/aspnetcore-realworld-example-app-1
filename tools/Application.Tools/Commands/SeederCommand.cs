using Application.Tools.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Respawn;
using Respawn.Graph;

namespace Tools.Commands;


[Command("db")]
public class SeederCommand : ConsoleAppBase, IAsyncDisposable
{
    private readonly AppDbContext _context;
    private readonly IEnumerable<ISeeder> _seeders;

    public SeederCommand(AppDbContext context, IEnumerable<ISeeder> seeders)
    {
        _context = context;
        _seeders = seeders;
    }

    [Command("migrate", "Migrate database")]
    public async Task Migrate()
    {
        await _context.Database.MigrateAsync();
    }

    [Command("fresh", "Wipe data")]
    public async Task FreshData()
    {
        await Migrate();

        using var conn = new NpgsqlConnection(_context.Database.GetConnectionString());

        await conn.OpenAsync();

        var respawner = await Respawner.CreateAsync(conn, new RespawnerOptions
        {
            TablesToIgnore = new Table[] { "__EFMigrationsHistory" },
            DbAdapter = DbAdapter.Postgres
        });

        await respawner.ResetAsync(conn);
    }

    [Command("seed", "Fake data")]
    public async Task SeedData()
    {
        await Migrate();
        await FreshData();

        var token = new CancellationToken();

        foreach (var seeder in _seeders)
        {
            await seeder.Run(token);
        }
    }

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return _context.DisposeAsync();
    }
}