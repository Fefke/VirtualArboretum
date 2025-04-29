using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Text;
using VirtualArboretum.Core.Application.DataTransferObjects.ModelDTOs;
using VirtualArboretum.Core.Application.Services;
using VirtualArboretum.Core.Domain.AggregateRoots;
using VirtualArboretum.Core.Domain.Entities;
using VirtualArboretum.Core.Domain.ValueObjects;

namespace VirtualArboretum.Core.Application.DataTransferObjects.MappedToDomain;

public class PlantMapper
{
    public static Plant IntoPlant(PlantDto plantTemplate)
    {
        StringBuilder errors = new();

        var plantUMaker = Fingerprint.TryCreate(plantTemplate.UniqueMarker);

        if (plantUMaker == null)
        {
            errors.AppendLine("Invalid plant fingerprint provided.");
            plantUMaker = new Fingerprint();
        }


        HyphaeStrain primaryHyphae;
        try
        {
            primaryHyphae = HyphaeSerializationService.Deserialize(plantTemplate.PrimaryHyphae).First();
        }
        catch (Exception e)
        {
            errors.AppendLine($"Your Primary Hypha is invalid: '{plantTemplate.PrimaryHyphae}'");
            throw new ArgumentException(errors.ToString());  // is fatal in this scope.
        }

        var cells = new List<Cell>(plantTemplate.AssociatedHyphae.Count);
        foreach (var cellTemplate in plantTemplate.Cells)
        {
            var newCellMarker = Fingerprint.TryCreate(cellTemplate.UniqueMarker);

            if (newCellMarker == null)
            {
                errors.AppendLine($"Your provided cell does have an invalid UniqueMarker: '{cellTemplate.UniqueMarker}' continuing..");
                continue;
            }

            var newCell = new Cell(
                cellTemplate.Organell,
                new CellType(cellTemplate.CellType),
                newCellMarker
            );

            cells.Add(newCell);
        }

        var associatedHyphae = new List<HyphaeStrain>(plantTemplate.AssociatedHyphae.Count);
        foreach (var associatedHypha in plantTemplate.AssociatedHyphae)
        {
            try
            {
                var newHyphaeStrain = HyphaeSerializationService.Deserialize(associatedHypha).First();
                associatedHyphae.Add(newHyphaeStrain);
            }
            catch (Exception e)
            {
                errors.AppendLine($"Your provided Hypha is invalid: '{associatedHypha}'");
            }
        }

        // Double down on possible aggregated errors.
        if (errors.Length > 0)
        {
            throw new ArgumentException(
                errors.ToString(),
                nameof(plantTemplate)
            );
        }

        var plant = new Plant(
            plantUMaker,
            primaryHyphae,
            cells,
            associatedHyphae
        );

        return plant;
    }

    public static ConcurrentDictionary<Fingerprint, Plant> IntoPlant(IList<PlantDto> plantTemplates)
    {
        var plants = new ConcurrentDictionary<Fingerprint, Plant>(
            capacity: plantTemplates.Count, concurrencyLevel: -1
        );


        foreach (var plantTemplate in plantTemplates)
        {
            var newPlant = PlantMapper.IntoPlant(plantTemplate);
            // throws Exception
            plants.AddOrUpdate(
                newPlant.UniqueMarker,
                newPlant,
                (_, presentPlant) => throw new ArgumentException(
                    "You cannot define multiple plants with same uniqueMarker" +
                    $" ({presentPlant.UniqueMarker})!"
                )
            );
        }

        return plants;
    }

    public static PlantDto IntoDto(Plant plant)
    {
        IList<string> serialHyphae = HyphaeSerializationService
            .SerializeEachListElement(plant.AssociatedHyphae);

        List<CellDto> serialCells = plant.Cells.Select(
            cell => new CellDto(
                cell.Value.UniqueMarker.ToString(),
                cell.Value.Type.ToString(),
                cell.Value.Organell
                )
            ).ToList();

        return new PlantDto(
            plant.UniqueMarker.ToString(),
            plant.Name.ToString(),
            serialCells,
            serialHyphae
                );
    }
}