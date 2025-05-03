using VirtualArboretum.Core.Application.DataTransferObjects;
using VirtualArboretum.Core.Application.DataTransferObjects.InputDTOs;
using VirtualArboretum.Core.Application.DataTransferObjects.MappedToDomain;
using VirtualArboretum.Core.Application.DataTransferObjects.ModelDTOs;
using VirtualArboretum.Core.Application.DataTransferObjects.ResultDTOs;
using VirtualArboretum.Core.Application.Interfaces;
using VirtualArboretum.Core.Application.Services;
using VirtualArboretum.Core.Domain.AggregateRoots;
using VirtualArboretum.Core.Domain.Entities;
using VirtualArboretum.Core.Domain.ValueObjects;

using static VirtualArboretum.Core.Application.DataTransferObjects.ResultFactory;

namespace VirtualArboretum.Core.Application.UseCases;

public enum PlacePlantErrors
{
    PlantAlreadyExists,
    GardenNotFound,
    AssociationWithMyceliumFailed,
    PlantCannotBeConstructed,
}

public class PlacePlant
{
    // You require a Garden to place a plant, which has to be part of an Arboretum.

    private readonly IArboretumRepository _arboretumRepo;
    private readonly IGardenRepository _gardenRepo;
    private readonly IPlantRepository _plantRepo; // TODO: Store new plant in _plantRepo!

    public PlacePlant(IArboretumRepository arboretumRepo, IGardenRepository gardenRepo, IPlantRepository plantRepo)
    {
        this._arboretumRepo = arboretumRepo;
        this._gardenRepo = gardenRepo;
        this._plantRepo = plantRepo;
    }

    /// <summary>
    /// Does store a plant into a garden, while micorrhizating it with the mycelium.
    /// </summary>
    public async Task<Result<PlacePlantSuccess, PlacePlantErrors>> IntoGarden(
         PlantDto plant, GardenIdentifierInput gardenIdentifier)
    {

        var plantPlacement = await IntoGardenWithoutAdditionalMycorrhization(plant, gardenIdentifier);

        if (!plantPlacement.IsSuccess)
        {
            return plantPlacement;  // => fail.
        }

        // Plant exists in the garden - associate the with mycelium.
        var newPlantFingerprint = Fingerprint.TryCreate(plantPlacement.Value.PlantFingerprint);

        Plant? newPlant;
        try
        {
            var activeArboretum = _arboretumRepo.Open();
            newPlant = await _plantRepo.GetByFingerprintAsync(newPlantFingerprint!);

            if (newPlant == null)
            {
                return Fail<PlacePlantSuccess, PlacePlantErrors>(
                    PlacePlantErrors.PlantCannotBeConstructed,
                    "Internal Error. Plant not found in plant repository."
                    );
            }

            activeArboretum.Mycelium.Mycorrhizate(newPlant);
        }
        catch (Exception e)
        {
            return Fail<PlacePlantSuccess, PlacePlantErrors>(PlacePlantErrors.AssociationWithMyceliumFailed, e.Message);
            // might leak internals, due to e.Message!
        }

        // Update in PlantRepo, as associations need to be stored.
        await _plantRepo.UpdateAsync(newPlant);

        var placePlantSuccess = new PlacePlantSuccess(
            PlantFingerprint: newPlantFingerprint!.ToString(),
            NewGardenFingerprint: plantPlacement.Value.NewGardenFingerprint,
            PrimaryPlantHyphae: newPlant.Name.ToString(),
            HyphaeStrains: HyphaeSerializationService.SerializeEachListElement(newPlant.AssociatedHyphae)
            );

        return Ok<PlacePlantSuccess, PlacePlantErrors>(placePlantSuccess);
    }

    /// <summary>
    /// Meaning, without associating the plant with the mycelium (besides Garden association).
    /// </summary>
    public async Task<Result<PlacePlantSuccess, PlacePlantErrors>> IntoGardenWithoutAdditionalMycorrhization(
        PlantDto plantTemplate, GardenIdentifierInput gardenIdentifier)
    {

        Garden garden;
        try
        {
            garden = await FindGarden(gardenIdentifier);
        }
        catch (Exception e)
        {
            return Fail<PlacePlantSuccess, PlacePlantErrors>(PlacePlantErrors.GardenNotFound, e.Message);
        }

        // construct plant 
        Plant plantModel;
        try
        {
            plantModel = PlantMapper.IntoPlant(plantTemplate);
        }
        catch (Exception e)
        {
            return Fail<PlacePlantSuccess, PlacePlantErrors>(
                PlacePlantErrors.PlantCannotBeConstructed, e.Message
            // might leak internals, most probably won't.
            );
        }


        // Check if plant already exists in the garden
        if (garden.ContainsPlant(plantModel.UniqueMarker))
        {
            return Fail<PlacePlantSuccess, PlacePlantErrors>(
                PlacePlantErrors.PlantAlreadyExists,
                "Exact same Plant already exists in the garden"
            );
        }

        // AddPlant should associate the plant with the gardens mycelium.
        garden.AddPlant(plantModel);

        var serialAssociatedHyphae = HyphaeSerializationService
            .SerializeEachListElement(plantModel.AssociatedHyphae);

        // Store Updates in _plantRepo and _gardenRepo.
        var plantTask = _plantRepo.AddAsync(plantModel);
        var gardenTask = _gardenRepo.UpdateAsync(garden);
        // Parallel execution as the operations are independent.
        await Task.WhenAll(plantTask, gardenTask);

        return Ok<PlacePlantSuccess, PlacePlantErrors>(new(
            PlantFingerprint: plantModel.UniqueMarker.ToString(),
            NewGardenFingerprint: garden.UniqueMarker.ToString(),
            PrimaryPlantHyphae: plantModel.Name.ToString(),
            HyphaeStrains: serialAssociatedHyphae
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

}