using System.Collections.Concurrent;
using VirtualArboretum.Core.Application.Interfaces;
using VirtualArboretum.Core.Domain.AggregateRoots;
using VirtualArboretum.Core.Domain.ValueObjects;

namespace VirtualArboretum.Infrastructure.Repositories.InMemory;

public class InMemoryCellRepository : ICellRepository
{

    private readonly ConcurrentDictionary<Fingerprint, Cell> _cells;
    private readonly ConcurrentDictionary<HyphaeStrain, Fingerprint> _organellLocationAssociations;

    public InMemoryCellRepository(
        IEnumerable<Cell> cells
        )
    {
        _cells = new ConcurrentDictionary<Fingerprint, Cell>();
        _organellLocationAssociations = new ConcurrentDictionary<HyphaeStrain, Fingerprint>();

        var successAddedAllCells = cells.AsParallel()
            .All(cell => AddAsync(cell).IsCompletedSuccessfully);

        if (!successAddedAllCells)
        {
            throw new ArgumentException(
                "For an unknown reason, not all cells could be added to the repository."
                );
        }
    }

    public Task<Cell?> GetByFingerprintAsync(Fingerprint id)
    {
        _cells.TryGetValue(id, out var cell);
        return Task.FromResult(cell);
    }

    public Task<Cell?> GetByPrimaryHyphaeAsync(HyphaeStrain strain)
    {
        _organellLocationAssociations.TryGetValue(strain, out var uniqueMarker);
        if (uniqueMarker is null)
        {
            return Task.FromResult<Cell?>(null);
        }

        _cells.TryGetValue(uniqueMarker, out var cell);

        return Task.FromResult(cell);
    }

    public Task AddAsync(Cell candidate)
    {
        var isSuccess = _cells.TryAdd(candidate.UniqueMarker, candidate);

        if (!isSuccess)
        {
            return Task.FromResult(false);
        }

        isSuccess = _organellLocationAssociations
            .TryAdd(candidate.OrganellLocation, candidate.UniqueMarker);

        return Task.FromResult(isSuccess);
    }

    /// <summary>
    /// Like Updating OrganellLocation of Cell or its Type, by UniqueMarker!
    /// </summary>
    public Task UpdateAsync(Cell candidate)
    {
        var cellToUpdate = GetByFingerprintAsync(candidate.UniqueMarker);
        if (!cellToUpdate.IsCompletedSuccessfully || cellToUpdate.Result == null)
        {
            return Task.FromResult(false);
        }

        // Check organell Association, if it's still valid...
        var oldOrganellAssociation = cellToUpdate.Result.OrganellLocation;

        if (!Equals(oldOrganellAssociation, candidate.OrganellLocation))
        {
            _organellLocationAssociations.TryRemove(
                oldOrganellAssociation, out var _);
            _organellLocationAssociations.TryAdd(
                candidate.OrganellLocation, candidate.UniqueMarker);
        }

        // Update Cell
        return _cells.TryUpdate(candidate.UniqueMarker, candidate, cellToUpdate.Result)
            ? Task.CompletedTask : Task.FromResult(false);
    }
}