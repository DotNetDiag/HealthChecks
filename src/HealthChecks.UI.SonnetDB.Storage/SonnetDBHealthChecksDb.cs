using HealthChecks.UI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Microsoft.Extensions.DependencyInjection;

internal sealed class SonnetDBHealthChecksDb : HealthChecksDb
{
    public SonnetDBHealthChecksDb(DbContextOptions<SonnetDBHealthChecksDb> options)
        : base(options)
    {
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        AssignIds();

        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        await AssignIdsAsync(cancellationToken).ConfigureAwait(false);

        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken).ConfigureAwait(false);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<HealthCheckConfiguration>().Property(entity => entity.Id).ValueGeneratedNever();
        modelBuilder.Entity<HealthCheckExecution>().Property(entity => entity.Id).ValueGeneratedNever();
        modelBuilder.Entity<HealthCheckExecutionEntry>().Property(entity => entity.Id).ValueGeneratedNever();
        modelBuilder.Entity<HealthCheckExecutionHistory>().Property(entity => entity.Id).ValueGeneratedNever();
        modelBuilder.Entity<HealthCheckFailureNotification>().Property(entity => entity.Id).ValueGeneratedNever();
    }

    private void AssignIds()
    {
        ChangeTracker.DetectChanges();

        AssignIds(GetAddedEntries<HealthCheckConfiguration>(), GetNextId<HealthCheckConfiguration>());
        AssignIds(GetAddedEntries<HealthCheckExecution>(), GetNextId<HealthCheckExecution>());
        AssignIds(GetAddedEntries<HealthCheckExecutionEntry>(), GetNextId<HealthCheckExecutionEntry>());
        AssignIds(GetAddedEntries<HealthCheckExecutionHistory>(), GetNextId<HealthCheckExecutionHistory>());
        AssignIds(GetAddedEntries<HealthCheckFailureNotification>(), GetNextId<HealthCheckFailureNotification>());

        ChangeTracker.DetectChanges();
    }

    private async Task AssignIdsAsync(CancellationToken cancellationToken)
    {
        ChangeTracker.DetectChanges();

        await AssignIdsAsync<HealthCheckConfiguration>(cancellationToken).ConfigureAwait(false);
        await AssignIdsAsync<HealthCheckExecution>(cancellationToken).ConfigureAwait(false);
        await AssignIdsAsync<HealthCheckExecutionEntry>(cancellationToken).ConfigureAwait(false);
        await AssignIdsAsync<HealthCheckExecutionHistory>(cancellationToken).ConfigureAwait(false);
        await AssignIdsAsync<HealthCheckFailureNotification>(cancellationToken).ConfigureAwait(false);

        ChangeTracker.DetectChanges();
    }

    private async Task AssignIdsAsync<TEntity>(CancellationToken cancellationToken)
        where TEntity : class
    {
        EntityEntry<TEntity>[] entries = GetAddedEntries<TEntity>();

        if (entries.Length == 0)
        {
            return;
        }

        int nextId = await GetNextIdAsync<TEntity>(cancellationToken).ConfigureAwait(false);
        AssignIds(entries, nextId);
    }

    private EntityEntry<TEntity>[] GetAddedEntries<TEntity>()
        where TEntity : class
    {
        return ChangeTracker.Entries<TEntity>()
            .Where(entry => entry.State == EntityState.Added && entry.Property<int>("Id").CurrentValue == 0)
            .ToArray();
    }

    private int GetNextId<TEntity>()
        where TEntity : class
    {
        return (Set<TEntity>().Max(entity => (int?)EF.Property<int>(entity, "Id")) ?? 0) + 1;
    }

    private async Task<int> GetNextIdAsync<TEntity>(CancellationToken cancellationToken)
        where TEntity : class
    {
        int? currentId = await Set<TEntity>()
            .MaxAsync(entity => (int?)EF.Property<int>(entity, "Id"), cancellationToken)
            .ConfigureAwait(false);

        return (currentId ?? 0) + 1;
    }

    private static void AssignIds<TEntity>(IReadOnlyCollection<EntityEntry<TEntity>> entries, int nextId)
        where TEntity : class
    {
        foreach (EntityEntry<TEntity> entry in entries)
        {
            entry.Property<int>("Id").CurrentValue = nextId++;
        }
    }
}
