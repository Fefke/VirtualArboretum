using VirtualArboretum.Core.Application.Interfaces;
using VirtualArboretum.Core.Domain.AggregateRoots;
using VirtualArboretum.Core.Domain.ValueObjects;

namespace VirtualArboretumTests.Fakes;

public class FakeArboretumRepository : IArboretumRepository
{
    public Arboretum Open()
    {
        throw new NotImplementedException();
    }

    public bool Close()
    {
        throw new NotImplementedException();
    }
}