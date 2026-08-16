using HealthChecks.UI.Data;
using Microsoft.EntityFrameworkCore;
using SonnetDB.EntityFrameworkCore.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class HealthChecksUIBuilderExtensions
{
    public static HealthChecksUIBuilder AddSonnetDBStorage(
        this HealthChecksUIBuilder builder,
        string connectionString,
        Action<DbContextOptionsBuilder>? configureOptions = null)
    {
        builder.Services.AddDbContext<HealthChecksDb, SonnetDBHealthChecksDb>(optionsBuilder =>
        {
            configureOptions?.Invoke(optionsBuilder);
            optionsBuilder.UseSonnetDB(connectionString, sonnetDbOptionsBuilder =>
                sonnetDbOptionsBuilder.MigrationsAssembly("HealthChecks.UI.SonnetDB.Storage"));
        });

        return builder;
    }
}
