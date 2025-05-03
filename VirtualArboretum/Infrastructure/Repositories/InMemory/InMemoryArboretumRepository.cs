using VirtualArboretum.Core.Application.Interfaces;
using VirtualArboretum.Core.Domain.AggregateRoots;

namespace VirtualArboretum.Infrastructure.Repositories.InMemory;

public class InMemoryArboretumRepository : IArboretumRepository
{
    private readonly Arboretum _arboretum;

    public InMemoryArboretumRepository(Arboretum arboretum)
    {
        _arboretum = arboretum;
    }

    public InMemoryArboretumRepository(IList<Garden> initialGardens)
        : this(new Arboretum(initialGardens)) { }

    public InMemoryArboretumRepository()
        : this(new List<Garden>()) { }


    public Arboretum Open()
    {
        return this._arboretum;
    }

    public bool Close()
    {
        return true;
        // as in-memory is not saved and does live until the process ends.
    }
}