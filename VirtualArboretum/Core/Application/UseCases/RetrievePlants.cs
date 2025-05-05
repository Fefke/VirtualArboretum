using System.Collections.Immutable;
using System.Linq;
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

public enum RetrievePlantsError
{
    PlantNotFound,
    GardenNotFound,
    InvalidInput,
    RepositoryError,
    UnknownError
}


public class RetrievePlants
{
    private readonly IArboretumRepository _arboretumRepo;
    private readonly IPlantRepository _plantRepo;
    private readonly MyceliumQueryService _myceliumQueryService;

    public RetrievePlants(
        IArboretumRepository arboretumRepo,
        //IGardenRepository gardenRepo,
        IPlantRepository plantRepo

)
    {
        _arboretumRepo = arboretumRepo ?? throw new ArgumentNullException(nameof(arboretumRepo));
        _plantRepo = plantRepo ?? throw new ArgumentNullException(nameof(plantRepo));
        //var gardenRepo1 = gardenRepo ?? throw new ArgumentNullException(nameof(gardenRepo));

        _myceliumQueryService = new MyceliumQueryService(
            _arboretumRepo,
            _plantRepo
        );
    }


    /// <summary>
    /// Will resolve your Mycelium Query
    /// </summary>
    /// <returns>All matching plants in their represented gardens (duplicates possible, as plant can be placed into multiple gardens)</returns>
    public async Task<Result<RetrievePlantsSuccess, RetrievePlantsError>> ByMyceliumQuery(QueryMyceliumInput query)
    {
        var queryResult = await _myceliumQueryService.FindAllByHyphaeQueryAsync(query);

        if (!queryResult.IsSuccess)
        {
            return Fail<RetrievePlantsSuccess, RetrievePlantsError>(
                    MapServiceErrorToUseCaseError(queryResult.Error),
                    queryResult.Error.Message,
                    queryResult.Error.Target);
        }

        var result = new RetrievePlantsSuccess(
            MatchingGardens: queryResult.Value.Gardens
        );

        return Ok<RetrievePlantsSuccess, RetrievePlantsError>(result);

    }


    /// <summary>
    /// Does return a List of GardenDto containing either none or at least one Garden, found plant is part of.<br/>
    /// Each garden will contain just the found plant.
    /// </summary>
    public async Task<Result<RetrievePlantsSuccess, RetrievePlantsError>> GetPlantByFingerprintAsync(PlantIdentifierInput input)
    {
        if (string.IsNullOrWhiteSpace(input.PlantFingerprint))
        {
            return Fail<RetrievePlantsSuccess, RetrievePlantsError>(
                RetrievePlantsError.InvalidInput, "Cannot find Plant with empty fingerprint.");
        }

        var fingerprint = Fingerprint.TryCreate(input.PlantFingerprint);
        if (fingerprint == null)
        {
            return Fail<RetrievePlantsSuccess, RetrievePlantsError>(
                RetrievePlantsError.InvalidInput, $"Invalid plant fingerprint format: '{input.PlantFingerprint}'!");
        }

        try
        {
            var plantInGardens = await _myceliumQueryService
                .GroupPlantsIntoGardens([fingerprint]);

            var result = new RetrievePlantsSuccess(
                plantInGardens.Value.Gardens
            );

            return Ok<RetrievePlantsSuccess, RetrievePlantsError>(result);

        }
        catch (Exception ex)
        {
            return Fail<RetrievePlantsSuccess, RetrievePlantsError>(
                RetrievePlantsError.RepositoryError,
                $"An error occurred while retrieving plant or its garden: {ex.Message}");
        }
    }


    /// <summary>
    /// Will parse your HyphaeStrainDto to a single one or returns appropriate Failure if not possible.
    /// </summary>
    /// <returns>All Gardens containing Plants matching primary hyphae.</returns>
    private Task<Result<HyphaeStrain, RetrievePlantsError>> ParsePrimaryHyphaeAsync(HyphaeStrainDto input)
    {
        if (string.IsNullOrWhiteSpace(input.SingleHyphaeStrain))
        {
            return Task.FromResult(Fail<HyphaeStrain, RetrievePlantsError>(
                RetrievePlantsError.InvalidInput,
                "Primary hyphae input is missing."));
        }

        ImmutableList<HyphaeStrain> strains;
        try
        {
            strains = HyphaeSerializationService.Deserialize(input.SingleHyphaeStrain);
            if (strains.Count != 1)
            {
                return Task.FromResult(Fail<HyphaeStrain, RetrievePlantsError>(
                    RetrievePlantsError.InvalidInput,
                    $"Expected exactly one primary hyphae strain, but got {strains.Count} from '{input.SingleHyphaeStrain}'."));
            }
        }
        catch (Exception ex)
        {
            return Task.FromResult(Fail<HyphaeStrain, RetrievePlantsError>(
                RetrievePlantsError.InvalidInput,
                $"Error deserializing primary hyphae '{input.SingleHyphaeStrain}'.\nDetails: {ex.Message}"));
            //      => will only contain details about hyphae parsing error.
        }

        return Task.FromResult(Ok<HyphaeStrain, RetrievePlantsError>(strains.First()));
    }

    /// <summary>
    /// Will parse your HyphaeStrainDto to a single one or returns appropriate Failure if not possible.
    /// </summary>
    /// <returns>All Gardens containing Plants matching primary hyphae.</returns>
    public async Task<Result<RetrievePlantsSuccess, RetrievePlantsError>> ByPrimaryHyphaeAsync(HyphaeStrainDto input)
    {
        try
        {
            var primaryStrainTask = await ParsePrimaryHyphaeAsync(input);

            if (!primaryStrainTask.IsSuccess)
            {
                return Fail<RetrievePlantsSuccess, RetrievePlantsError>(primaryStrainTask.Error);
            }

            var primaryStrain = primaryStrainTask.Value;

            var plant = await _plantRepo.GetByPrimaryHyphaeAsync(primaryStrain);
            if (plant == null)
            {
                return Fail<RetrievePlantsSuccess, RetrievePlantsError>(
                    RetrievePlantsError.PlantNotFound,
                    $"Plant with primary hyphae '{primaryStrain}' not found.");
            }

            // find all gardens plant is part of...
            var gardensContainingPlant = await _myceliumQueryService
                .GroupPlantsIntoGardens([plant.UniqueMarker]);

            if (gardensContainingPlant.IsSuccess)
            {
                var result = new RetrievePlantsSuccess(gardensContainingPlant.Value.Gardens);
                return Ok<RetrievePlantsSuccess, RetrievePlantsError>(result);
            }

            return Fail<RetrievePlantsSuccess, RetrievePlantsError>(new(
                RetrievePlantsError.GardenNotFound,
                gardensContainingPlant.Error.Message,
                gardensContainingPlant.Error.Target));
        }
        catch (Exception ex)
        {
            return Fail<RetrievePlantsSuccess, RetrievePlantsError>(
                RetrievePlantsError.RepositoryError,
                $"An error occurred retrieving plant or its garden: {ex.Message}");
        }
    }


    public async Task<Result<RetrievePlantsSuccess, RetrievePlantsError>> GetAllAsync()
    {
        var arboretum = _arboretumRepo.Open();
        var allGardensLocations = arboretum.GetAllGardensPrimaryLocation();

        var gardens = allGardensLocations
            .Select(arboretum.ViewGarden)
            .OfType<Garden>(); // Does remove nullable annotation.

        var gardenDtos = gardens.AsParallel().Select(
            garden => new GardenDto(
                garden.PrimaryLocation.ToString(),
                garden.UniqueMarker.ToString(),
                garden.GetPlants()
                    .Select(PlantMapper.IntoDto)
                    .ToImmutableList()
            )
        ).ToImmutableList();

        var result = new RetrievePlantsSuccess(
            MatchingGardens: gardenDtos);
        return Ok<RetrievePlantsSuccess, RetrievePlantsError>(result);
    }


    private RetrievePlantsError MapServiceErrorToUseCaseError(ErrorResult<MyceliumQueryError> serviceError)
    => serviceError.Code switch
    {
        MyceliumQueryError.NoHyphaeQueryProvided => RetrievePlantsError.InvalidInput,
        MyceliumQueryError.InvalidHyphaeQuery => RetrievePlantsError.InvalidInput,
        MyceliumQueryError.PlantLoadingFailed => RetrievePlantsError.PlantNotFound,
        _ => RetrievePlantsError.UnknownError
    };

}