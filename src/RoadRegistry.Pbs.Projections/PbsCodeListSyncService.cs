namespace RoadRegistry.Pbs.Projections;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RoadRegistry.RoadSegment.ValueObjects;
using Schema;
using Schema.Records;

// One-time sync of the enum-based code lists (the "xxxCodelijstxxx" tables) from the V2 domain into SQL Server.
// These carry no events, so we diff the known code values against the table and apply smart CRUD (insert new,
// update changed labels/definitions, delete removed) rather than truncate-and-reload. The Wegbeheerder code list is
// NOT handled here - it is event-driven (see OrganizationPbsProjection).
public sealed class PbsCodeListSyncService : IHostedService
{
    private const int MaxAttempts = 20;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(3);

    private readonly IDbContextFactory<PbsContext> _dbContextFactory;
    private readonly ILogger<PbsCodeListSyncService> _logger;

    public PbsCodeListSyncService(IDbContextFactory<PbsContext> dbContextFactory, ILogger<PbsCodeListSyncService> logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // The PbsContext tables are created by the EF migrator at host startup; tolerate running before that finished.
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await SyncAsync(cancellationToken);
                _logger.LogInformation("PBS code lists synced.");
                return;
            }
            catch (Exception ex) when (attempt < MaxAttempts && !cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning(ex, "PBS code list sync attempt {Attempt}/{Max} failed; retrying in {Delay}s.", attempt, MaxAttempts, RetryDelay.TotalSeconds);
                await Task.Delay(RetryDelay, cancellationToken);
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task SyncAsync(CancellationToken ct)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync(ct);

        SyncList(context.RoadNodeTypeCodeList,
            RoadNodeTypeV2.All.Select(x => new RoadNodeTypeCodeListRecord { TYPE = x.Translation.Identifier, LBLTYPE = x.Translation.Name, DEFTYPE = x.Translation.Description }).ToList(),
            r => r.TYPE, (a, b) => a.LBLTYPE == b.LBLTYPE && a.DEFTYPE == b.DEFTYPE, (src, dst) => { dst.LBLTYPE = src.LBLTYPE; dst.DEFTYPE = src.DEFTYPE; });

        SyncList(context.GradeSeparatedJunctionTypeCodeList,
            GradeSeparatedJunctionTypeV2.All.Select(x => new GradeSeparatedJunctionTypeCodeListRecord { TYPE = x.Translation.Identifier, LBLTYPE = x.Translation.Name, DEFTYPE = x.Translation.Description }).ToList(),
            r => r.TYPE, (a, b) => a.LBLTYPE == b.LBLTYPE && a.DEFTYPE == b.DEFTYPE, (src, dst) => { dst.LBLTYPE = src.LBLTYPE; dst.DEFTYPE = src.DEFTYPE; });

        SyncList(context.RoadSegmentStatusCodeList,
            RoadSegmentStatusV2.All.Select(x => new RoadSegmentStatusCodeListRecord { STATUS = x.Translation.Identifier, LBLSTATUS = x.Translation.Name, DEFSTATUS = x.Translation.Description }).ToList(),
            r => r.STATUS, (a, b) => a.LBLSTATUS == b.LBLSTATUS && a.DEFSTATUS == b.DEFSTATUS, (src, dst) => { dst.LBLSTATUS = src.LBLSTATUS; dst.DEFSTATUS = src.DEFSTATUS; });

        SyncList(context.RoadSegmentMethodCodeList,
            RoadSegmentGeometryDrawMethodV2.All.Select(x => new RoadSegmentMethodCodeListRecord { METHODE = x.Translation.Identifier, LBLMETHODE = x.Translation.Name, DEFMETHODE = x.Translation.Description }).ToList(),
            r => r.METHODE, (a, b) => a.LBLMETHODE == b.LBLMETHODE && a.DEFMETHODE == b.DEFMETHODE, (src, dst) => { dst.LBLMETHODE = src.LBLMETHODE; dst.DEFMETHODE = src.DEFMETHODE; });

        SyncList(context.RoadSegmentMorphologyCodeList,
            RoadSegmentMorphologyV2.All.Select(x => new RoadSegmentMorphologyCodeListRecord { MORF = x.Translation.Identifier, LBLMORF = x.Translation.Name, DEFMORF = x.Translation.Description }).ToList(),
            r => r.MORF, (a, b) => a.LBLMORF == b.LBLMORF && a.DEFMORF == b.DEFMORF, (src, dst) => { dst.LBLMORF = src.LBLMORF; dst.DEFMORF = src.DEFMORF; });

        SyncList(context.RoadSegmentAccessRestrictionCodeList,
            RoadSegmentAccessRestrictionV2.All.Select(x => new RoadSegmentAccessRestrictionCodeListRecord { TOEGANG = x.Translation.Identifier, LBLTOEGANG = x.Translation.Name, DEFTOEGANG = x.Translation.Description }).ToList(),
            r => r.TOEGANG, (a, b) => a.LBLTOEGANG == b.LBLTOEGANG && a.DEFTOEGANG == b.DEFTOEGANG, (src, dst) => { dst.LBLTOEGANG = src.LBLTOEGANG; dst.DEFTOEGANG = src.DEFTOEGANG; });

        SyncList(context.RoadSegmentSurfaceTypeCodeList,
            RoadSegmentSurfaceTypeV2.All.Select(x => new RoadSegmentSurfaceTypeCodeListRecord { VERHARDING = x.Translation.Identifier, LBLVERHARD = x.Translation.Name, DEFVERHARD = x.Translation.Description }).ToList(),
            r => r.VERHARDING, (a, b) => a.LBLVERHARD == b.LBLVERHARD && a.DEFVERHARD == b.DEFVERHARD, (src, dst) => { dst.LBLVERHARD = src.LBLVERHARD; dst.DEFVERHARD = src.DEFVERHARD; });

        SyncList(context.RoadSegmentCategoryCodeList,
            RoadSegmentCategoryV2.All.Select(x => new RoadSegmentCategoryCodeListRecord { WEGCAT = x.Translation.Identifier, LBLWEGCAT = x.Translation.Name, DEFWEGCAT = x.Translation.Description }).ToList(),
            r => r.WEGCAT, (a, b) => a.LBLWEGCAT == b.LBLWEGCAT && a.DEFWEGCAT == b.DEFWEGCAT, (src, dst) => { dst.LBLWEGCAT = src.LBLWEGCAT; dst.DEFWEGCAT = src.DEFWEGCAT; });

        SyncList(context.RoadSegmentDirectionCodeList,
            RoadSegmentTrafficDirection.All.Select(x => new RoadSegmentDirectionCodeListRecord { RICHTING = x.Translation.Identifier, LBLRICHT = x.Translation.Name, DEFRICHT = x.Translation.Description }).ToList(),
            r => r.RICHTING, (a, b) => a.LBLRICHT == b.LBLRICHT, (src, dst) => { dst.LBLRICHT = src.LBLRICHT; });

        SyncList(context.RoadSegmentSideCodeList,
            RoadSegmentAttributeSide.All.Select(x => new RoadSegmentSideCodeListRecord { KANT = x.Translation.Identifier, LBLKANT = x.Translation.Name, DEFKANT = x.Translation.Description }).ToList(),
            r => r.KANT, (a, b) => a.LBLKANT == b.LBLKANT, (src, dst) => { dst.LBLKANT = src.LBLKANT; });

        await context.SaveChangesAsync(ct);
    }

    // Smart CRUD: insert new, update changed, delete removed - keyed by keyOf.
    private static void SyncList<TRecord, TKey>(
        DbSet<TRecord> set,
        List<TRecord> desired,
        Func<TRecord, TKey> keyOf,
        Func<TRecord, TRecord, bool> fieldsEqual,
        Action<TRecord, TRecord> copyFields)
        where TRecord : class
    {
        var existing = set.AsNoTracking().ToList();
        var existingByKey = existing.ToDictionary(keyOf);
        var desiredKeys = desired.Select(keyOf).ToHashSet();

        foreach (var d in desired)
        {
            if (existingByKey.TryGetValue(keyOf(d), out var current))
            {
                if (!fieldsEqual(current, d))
                {
                    copyFields(d, current);
                    set.Update(current);
                }
            }
            else
            {
                set.Add(d);
            }
        }

        foreach (var current in existing.Where(x => !desiredKeys.Contains(keyOf(x))))
        {
            set.Remove(current);
        }
    }
}
