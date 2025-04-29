using System.Runtime.CompilerServices;
using VirtualArboretum.Core.Application.Interfaces;
using VirtualArboretum.Core.Domain.AggregateRoots;
using VirtualArboretum.Core.Domain.Entities;
using VirtualArboretum.Core.Domain.ValueObjects;

namespace VirtualArboretumTests.Fakes;

public class ArboretumRepositorySpy : IArboretumRepository
{
    public int OpenCallCount { get; private set; } = 0;
    public int MycorrhizateCallCount { get; private set; } = 0;
    public List<Plant> MycorrhizatedPlants { get; } = new();


    public Arboretum Open()
    {
        OpenCallCount++;
        return new SpyArboretum();
    }

    public Arboretum Open(IEnumerable<Garden> gardens)
    {
        OpenCallCount++;
        return new SpyArboretum(gardens);
    }


    public bool Close()
    {
        // Will save state in real repo. in-memory stays in-memory.
        return true;
    }

    // You would need to intercept the Mycorrhizate call in your test version of Arboretum
    public class SpyArboretum : Arboretum
    {
        private readonly ArboretumRepositorySpy _spy;

        public SpyArboretum(IEnumerable<Garden> gardens)
            : base(gardens)
        {
            _spy = new ArboretumRepositorySpy();
        }

        public SpyArboretum() : this(new List<Garden>())
        { }

        public new void Mycorrhizate(Plant plant)
        {
            _spy.MycorrhizateCallCount++;
            _spy.MycorrhizatedPlants.Add(plant);
            Mycelium.Mycorrhizate(plant);
        }
    }
}