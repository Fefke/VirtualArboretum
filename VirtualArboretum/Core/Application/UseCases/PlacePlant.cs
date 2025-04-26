using System.Collections.Immutable;
using System.Text;
using VirtualArboretum.Core.Application.DataTransferObjects;
using VirtualArboretum.Core.Application.DataTransferObjects.InputDTOs;
using VirtualArboretum.Core.Application.DataTransferObjects.ModelDTOs;
using VirtualArboretum.Core.Application.DataTransferObjects.ResultDTOs;
using VirtualArboretum.Core.Application.Interfaces;
using VirtualArboretum.Core.Application.Services;
using VirtualArboretum.Core.Domain.AggregateRoots;
using VirtualArboretum.Core.Domain.Entities;
using VirtualArboretum.Core.Domain.ValueObjects;

namespace VirtualArboretum.Core.Application.UseCases;

public enum PlacePlantErrors
{
    PlantAlreadyExists,
    GardenNotFound,
    AssociationWithMyceliumFailed,
    PlantCannotBeConstructed,
}

public class PlacePlant : AbstractUseCase<PlacePlantSuccess, PlacePlantErrors>
{
    // You require a Garden to place a plant, which has to be part of an Arboretum.

    private readonly IArboretumRepository _arboretumRepo;
    private readonly IGardenRepository _gardenRepo;

    public PlacePlant(IArboretumRepository arboretumRepo, IGardenRepository gardenRepo)
    {
        this._arboretumRepo = arboretumRepo;
        this._gardenRepo = gardenRepo;
    }

    public async Task<Result<PlacePlantSuccess, PlacePlantErrors>> IntoGarden(
         PlantDto plant, GardenIdentifierInput gardenIdentifier)
    {

        var plantPlacement = await IntoGardenWithoutAdditionalMycorrhization(plant, gardenIdentifier);

        if (!plantPlacement.IsSuccess)
        {
            return plantPlacement;
        }

        // Plant exists in the garden - associate the with mycelium.
        var newPlantFingerprint = plantPlacement.Value.PlantFingerprint;
        try
        {
            var activeArboretum = _arboretumRepo.Open();
            //TODO: activeArboretum.
            //activeArboretum.Mycorrhizate();
        }
        catch (Exception e)
        {
            return Fail(PlacePlantErrors.AssociationWithMyceliumFailed, "");
        }
        //plant.AssociatedHyphae

        //return Ok(new PlacePlantSuccess(plant, garden));
        throw new NotImplementedException();
    }

    /// <summary>
    /// Meaning, without associating the plant with the mycelium (besides Garden association).
    /// </summary>
    public async Task<Result<PlacePlantSuccess, PlacePlantErrors>> IntoGardenWithoutAdditionalMycorrhization(
        PlantDto plant, GardenIdentifierInput gardenIdentifier)
    {

        Garden garden;
        try
        {
            garden = await FindGarden(gardenIdentifier);
        }
        catch (Exception e)
        {
            return Fail(PlacePlantErrors.GardenNotFound, e.Message);
        }

        // construct plant 
        Plant plantModel;
        try
        {
            plantModel = ConstructPlantModel(plant);
        }
        catch (Exception e)
        {
            return Fail(
                PlacePlantErrors.PlantCannotBeConstructed, e.Message
            // might leak internals, most probably won't.
            );
        }


        // Check if plant already exists in the garden
        if (garden.ContainsPlant(plantModel.UniqueMarker))
        {
            return Fail(
                PlacePlantErrors.PlantAlreadyExists,
                "Exact same Plant already exists in the garden"
            );
        }

        // AddPlant should associate the plant with the gardens mycelium.
        garden.AddPlant(plantModel);

        var serialAssociatedHyphae = HyphaeSerializationService
            .SerializeEachListElement(plantModel.AssociatedHyphae);

        return Ok(new(
            plantModel.UniqueMarker.ToString(),
            garden.PrimaryLocation.ToString(),
            plantModel.Name.ToString(),
            serialAssociatedHyphae
            ));
    }

    private async Task<Garden> FindGarden(GardenIdentifierInput gardenIdentifier)
    {
        var gardenFingerprint = Fingerprint.TryCreate(gardenIdentifier.GardenFingerprint);

        if (gardenFingerprint == null)
        {
            throw new ArgumentException("Invalid garden fingerprint provided.");
        }

        return await _gardenRepo.GetByFingerprintAsync(gardenFingerprint)
               ?? throw new ArgumentException(
                   $"Garden with valid provided fingerprint: '{gardenFingerprint}' not found."
                   );
    }

    private Plant ConstructPlantModel(PlantDto plantTemplate)
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
}