using System.Collections.Concurrent;
using System.Collections.Immutable;
using VirtualArboretum.Core.Application.DataTransferObjects.InputDTOs;
using VirtualArboretum.Core.Application.DataTransferObjects.ResultDTOs;
using VirtualArboretum.Core.Application.DataTransferObjects;
using VirtualArboretum.Core.Application.DataTransferObjects.MappedToDomain;
using VirtualArboretum.Core.Application.DataTransferObjects.ModelDTOs;
using VirtualArboretum.Core.Application.Interfaces;
using VirtualArboretum.Core.Domain.Entities;
using VirtualArboretum.Core.Domain.ValueObjects;
using static VirtualArboretum.Core.Application.DataTransferObjects.ResultFactory;
using HyphaeStrain = VirtualArboretum.Core.Domain.AggregateRoots.HyphaeStrain;
using VirtualArboretum.Core.Domain.AggregateRoots;
using System.Linq;
using VirtualArboretum.Core.Application.Services.QueryParser.Expressions;
using VirtualArboretum.Core.Application.Services.QueryParser;

namespace VirtualArboretum.Core.Application.Services;

public enum MyceliumQueryError
{
    NoHyphaeQueryProvided,
    InvalidHyphaeQuery,
    PlantLoadingFailed,
    PlantIntoGardenGroupingError,
    UnknownError
}

/// <summary>
/// Service to query the Mycelium with queries build with hyphae.
/// </summary>
public class MyceliumQueryService  // <ImmutableList<GardenDto>, MyceliumQueryError>
{
    private readonly IArboretumRepository _arboretumRepo;
    private readonly IPlantRepository _plantRepo;

    public MyceliumQueryService(
        IArboretumRepository arboretumRepo,
        IPlantRepository plantRepo)
    {
        _arboretumRepo = arboretumRepo;
        _plantRepo = plantRepo;
    }

    public async Task<Result<MyceliumQuerySuccess, MyceliumQueryError>> FindAllByHyphaeQueryAsync(
        QueryMyceliumInput hyphaeQueryInput)
    {
        if (string.IsNullOrWhiteSpace(hyphaeQueryInput.HyphaeQuery))
        {
            return Fail<MyceliumQuerySuccess, MyceliumQueryError>(
                MyceliumQueryError.NoHyphaeQueryProvided,
                "Please provide a hyphae query to be resolved to plants in gardens."
            );
        }

        var parseResult = HyphaeQueryParser.Parse(hyphaeQueryInput.HyphaeQuery);
        if (!parseResult.IsSuccess)
        {
            return Fail<MyceliumQuerySuccess, MyceliumQueryError>(
                MapParserErrorToServiceError(parseResult.Error.Code),
                parseResult.Error.Message,
                parseResult.Error.Target // Pass target if available
            );
        }

        var mycelium = _arboretumRepo.Open().Mycelium;
        var astRoot = parseResult.Value;
        var context = new MyceliumContext(mycelium);
        ImmutableList<Fingerprint> resultingPlantFingerprints;

        try
        {
            resultingPlantFingerprints = astRoot.Interpret(context);
        }
        catch (Exception ex)
        {
            return Fail<MyceliumQuerySuccess, MyceliumQueryError>(
                MyceliumQueryError.UnknownError,
                $"Error during query interpretation: {ex.Message}"
            );
        }

        if (resultingPlantFingerprints.IsEmpty)
        {
            return Ok<MyceliumQuerySuccess, MyceliumQueryError>(
               new MyceliumQuerySuccess(ImmutableList<GardenDto>.Empty)
           );
        }

        return await GroupPlantsIntoGardens(resultingPlantFingerprints);
    }

    private static MyceliumQueryError MapParserErrorToServiceError(MyceliumQueryParserError parserError)
        => parserError switch
        {
            MyceliumQueryParserError.EmptyQuery => MyceliumQueryError.NoHyphaeQueryProvided,
            MyceliumQueryParserError.MalformedQuery => MyceliumQueryError.InvalidHyphaeQuery,
            MyceliumQueryParserError.InvalidTermFormat => MyceliumQueryError.InvalidHyphaeQuery,
            MyceliumQueryParserError.SerializationError => MyceliumQueryError.InvalidHyphaeQuery,
            MyceliumQueryParserError.EmptyOrGroup => MyceliumQueryError.InvalidHyphaeQuery,
            MyceliumQueryParserError.EmptyAndGroup => MyceliumQueryError.InvalidHyphaeQuery,
            _ => MyceliumQueryError.UnknownError,
        };

    // GroupPlantsIntoGardens remains the same as it's a post-processing step.
    // Ensure it's defined within this class or accessible.
    public async Task<Result<MyceliumQuerySuccess, MyceliumQueryError>> GroupPlantsIntoGardens(
        IList<Fingerprint> plantIdentifyingFingerprints
        )
    {
        var arboretum = _arboretumRepo.Open();
        var allGardenLocations = arboretum.GetAllGardensPrimaryLocation().ToImmutableHashSet();
        var selectedGardens = new ConcurrentDictionary<HyphaeStrain, GardenDto>();

        var plantTasks = plantIdentifyingFingerprints.Select(async fingerprint =>
        {
            var plant = await _plantRepo.GetByFingerprintAsync(fingerprint);
            if (plant == null)
            {
                throw new KeyNotFoundException(
                    $"Plant with fingerprint '{fingerprint}' not found in repository during grouping.");
            }

            var plantsGardens = plant.AssociatedHyphae
                .Where(allGardenLocations.Contains)
                .Select(arboretum.ViewGarden)
                .OfType<Garden>()
                .Where(garden => garden.ContainsPlant(plant.UniqueMarker))
                .ToList(); // ToList to avoid multiple enumerations if needed below

            if (!plantsGardens.Any())
            {
                // Plant exists but is not in any of the known/queried gardens.
                // This might be acceptable depending on requirements. For now, we just won't add it to any garden DTO.
                throw new InvalidOperationException(
                    $"Roque Plant '{plant.Name}' is not associated with any known gardens!");
            }

            var thisPlantDto = PlantMapper.IntoDto(plant);

            foreach (var garden in plantsGardens)
            {
                selectedGardens.AddOrUpdate(
                    garden.PrimaryLocation,
                    // Add new GardenDto
                    _ => new GardenDto(
                        PrimaryLocation: garden.PrimaryLocation.ToString(),
                        UniqueMarker: garden.UniqueMarker.ToString(),
                        Plants: new List<PlantDto> { thisPlantDto } // Initialize with the current plant
                    ),
                    // Update existing GardenDto
                    (_, existingGardenDto) =>
                    {
                        // Ensure plant is not added multiple times if multiple fingerprints resolve to the same plant
                        // and that plant is in the same garden (though fingerprint should be unique per plant).
                        // The primary concern is if this grouping logic is called with duplicate fingerprints for the *same* plant.
                        var plantAlreadyInGarden = existingGardenDto.Plants
                            .Any(p => p.UniqueMarker == thisPlantDto.UniqueMarker);

                        if (!plantAlreadyInGarden)
                        {
                            existingGardenDto.Plants.Add(thisPlantDto);
                        }
                        return existingGardenDto;
                    }
                );
            }
        });

        try
        {
            await Task.WhenAll(plantTasks);
        }
        catch (Exception ex) // More general catch for issues during plant/garden processing
        {
            return Fail<MyceliumQuerySuccess, MyceliumQueryError>(
                MyceliumQueryError.PlantIntoGardenGroupingError,
                $"An error occurred while grouping plants into gardens: {ex.Message}",
                "GroupPlantsIntoGardens"
            );
        }

        return Ok<MyceliumQuerySuccess, MyceliumQueryError>(
            new MyceliumQuerySuccess(selectedGardens.Values.ToImmutableList())
        );
    }
}