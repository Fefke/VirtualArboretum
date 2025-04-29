using System.Collections.Generic;
using VirtualArboretum.Core.Application.Services;
using VirtualArboretum.Core.Domain.AggregateRoots;
using VirtualArboretum.Core.Domain.Entities;
using VirtualArboretum.Core.Domain.ValueObjects;

namespace VirtualArboretumTests.Fakes;

public class FakePlantFactory
{
    public static Plant CreateTestPlant(string name, List<HyphaeStrain> hyphaeStrains, uint? randomCells, uint? cellByteSize)
    {

        randomCells ??= 3;
        cellByteSize ??= 64;

        var hypha = new HyphaApex(name);
        var primaryStrain = new HyphaeStrain(hypha);

        // construct cells
        var cells = new List<Cell>((int)randomCells);

        for (var i = 0; i < randomCells; i++)
        {
            byte[] organell = new byte[(int)cellByteSize];
            Random random = new Random();
            random.NextBytes(organell);
            cells.Add(new Cell(organell));
        }

        // of you go...
        return new Plant(
            new Fingerprint(),
            primaryStrain,
            cells,
            hyphaeStrains
        );
    }

    public static Plant CreateTestPlant(string name, string serialHyphaeStrains, uint? randomCells, uint? cellByteSize)
    {
        var hyphaeStrains = HyphaeSerializationService
            .Deserialize(serialHyphaeStrains);

        return CreateTestPlant(
            name, hyphaeStrains.ToList(), randomCells, cellByteSize
            );
    }

}