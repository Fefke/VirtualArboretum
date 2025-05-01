using System.Collections.Concurrent;
using System.Collections.Immutable;
using VirtualArboretum.Core.Application.DataTransferObjects.InputDTOs;
using VirtualArboretum.Core.Application.DataTransferObjects.ResultDTOs;
using VirtualArboretum.Core.Application.DataTransferObjects;
using VirtualArboretum.Core.Application.DataTransferObjects.MappedToDomain;
using VirtualArboretum.Core.Application.DataTransferObjects.ModelDTOs;
using VirtualArboretum.Core.Application.Interfaces;
using VirtualArboretum.Core.Domain.AggregateRoots;
using VirtualArboretum.Core.Domain.Entities;
using VirtualArboretum.Core.Domain.ValueObjects;
using static VirtualArboretum.Core.Application.DataTransferObjects.ResultFactory;

namespace VirtualArboretum.Core.Application.Services;

public enum MyceliumQueryError
{
    NoHyphaeQueryProvided,
    InvalidHyphaeQuery,
    MyceliumAccessFailed,
    GardenFilterNotFound,
    PlantLoadingFailed,
    RepositoryError,
    MappingFailed,
    InconsistentData,
    UnknownError
}

/// <summary>
/// Service to query the Mycelium with queries build with hyphae.
/// </summary>
public class MyceliumQueryService  // <ImmutableList<GardenDto>, MyceliumQueryError>
{
    private readonly IArboretumRepository _arboretumRepo;
    private readonly IPlantRepository _plantRepo;
    private readonly IGardenRepository _gardenRepo;

    private static char notMarker = '!';
    private static char orMarker = '|';
    //private static char andMarker = ' ';  // not required, as and is just a sequence of hyphae.

    // Konstruktor mit Dependency Injection
    public MyceliumQueryService(
        IArboretumRepository arboretumRepo,
        IPlantRepository plantRepo,
        IGardenRepository gardenRepo)
    {
        _arboretumRepo = arboretumRepo ?? throw new ArgumentNullException(nameof(arboretumRepo));
        _plantRepo = plantRepo ?? throw new ArgumentNullException(nameof(plantRepo));
        _gardenRepo = gardenRepo ?? throw new ArgumentNullException(nameof(gardenRepo));
    }

    /// <summary>
    /// Finds plants based on associated hyphae.
    /// optionally filtered by a specific garden.
    /// Returns a list of GardenWithPlants DTOs on success.
    /// </summary>
    public async Task<Result<MyceliumQuerySuccess, MyceliumQueryError>> FindAllByHyphaeQueryAsync(
        QueryMyceliumInput hyphaeQuery
    )
    {
        if (string.IsNullOrWhiteSpace(hyphaeQuery.HyphaeQuery))
        {
            return Fail<MyceliumQuerySuccess, MyceliumQueryError>(
                MyceliumQueryError.NoHyphaeQueryProvided
            );
        }

        var orBuckets = hyphaeQuery.HyphaeQuery
            .Split(orMarker, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .ToList();

        var resultList = new List<MyceliumQuerySuccess>();

        try
        {
            var tasks = orBuckets.Select(FindAllInOrBucket).ToList();

            var results = await Task.WhenAll(tasks);

            foreach (var result in results)
            {
                if (!result.IsSuccess)
                {
                    return result; // Return the first failure encountered.
                }

                resultList.Add(result.Value);
            }
        }
        catch (Exception e)
        {
            return Fail<MyceliumQuerySuccess, MyceliumQueryError>(
                MyceliumQueryError.UnknownError,
                e.Message
            );
        }

        // Return successful result with aggregated data.
        return Ok<MyceliumQuerySuccess, MyceliumQueryError>(
            new MyceliumQuerySuccess(
                Gardens: resultList.SelectMany(r => r.Gardens).ToImmutableList()
            )
        );
    }

    /// <summary>
    /// All Exceptions should contain externalisable messages.
    /// Will throw Exception, if Query not parsable!
    /// </summary>
    private async Task<Result<MyceliumQuerySuccess, MyceliumQueryError>> FindAllInOrBucket(
        string orBucket
    )
    {
        if (string.IsNullOrWhiteSpace(orBucket))
        {
            // no HyphaeStrains to mach means, no result is valid.
            return Ok<MyceliumQuerySuccess, MyceliumQueryError>(
                new MyceliumQuerySuccess(
                    Gardens: new List<GardenDto>().ToImmutableList()
                )
            );
        }

        // parse orBucket containing hyphae unions and negations.
        var negationBuckets = orBucket
            .Split(
                $"{notMarker}{HyphaKey.StartMarker}",
                StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(negatedHyphae => $"{HyphaKey.StartMarker}{negatedHyphae}")
            // => reapply start marker for deserialization.
            .ToList();

        var negatedHyphaeStrains = new List<HyphaeStrain>(negationBuckets.Count);
        var plainHyphaeStrains = new List<HyphaeStrain>(); // size not determined atm.

        foreach (var negationBucket in negationBuckets)
        {

            try
            {
                var hyphaeStrains = HyphaeSerializationService.Deserialize(negationBucket);
                // won't fail on empty hyphae - which is right, bcs. of ignoring whitespace.

                if (hyphaeStrains.IsEmpty) continue;

                negatedHyphaeStrains.Add(hyphaeStrains.First()); // first is negated.
                plainHyphaeStrains.AddRange(hyphaeStrains.Skip(1)); // tail is "normal"
            }
            catch (Exception e)
            {
                return Fail<MyceliumQuerySuccess, MyceliumQueryError>(
                    MyceliumQueryError.InvalidHyphaeQuery,
                    e.Message // is user friendly.
                    );
            }
        }

        return await QueryMycelium(plainHyphaeStrains, negatedHyphaeStrains);
    }


    /// <summary>
    /// Queries Mycelium for all plants matching the provided hyphae strains.
    /// </summary>
    private async Task<Result<MyceliumQuerySuccess, MyceliumQueryError>> QueryMycelium(
        List<HyphaeStrain> plainHyphaeStrains, List<HyphaeStrain> negatedHyphaeStrains)
    {
        var arboretum = this._arboretumRepo.Open();
        var mycelium = arboretum.Mycelium;

        var plainFingerprintMappings = mycelium.GetMycorrhizations(plainHyphaeStrains);

        // now get all possible plants, to kick those out, containing one of negatedHyphaeStrains.
        var plainFingerprints = plainFingerprintMappings.AsParallel()
            .Where(kvp => !kvp.Value.IsEmpty)
            .SelectMany(kvp => kvp.Value)
            .Distinct()  // as multiple hyphae can be associated with the same plant.
            .ToList();

        if (plainFingerprints.Count == 0)
        {
            // no plants found, fast-return empty result.
            return Ok<MyceliumQuerySuccess, MyceliumQueryError>(
                new MyceliumQuerySuccess(
                    Gardens: new List<GardenDto>().ToImmutableList()
                )
            );
        }

        if (negatedHyphaeStrains.Count > 0)
        {
            var negatedFingerprintMappings = mycelium.GetMycorrhizations(negatedHyphaeStrains);

            var negatedFingerprints = negatedFingerprintMappings.AsParallel()
                .Where(kvp => !kvp.Value.IsEmpty)
                .SelectMany(kvp => kvp.Value)
                .Distinct()
                .ToHashSet(); // HashSet for quicker lookups.

            // remove all plants, which are associated with negated hyphae.
            plainFingerprints = plainFingerprints
                .Where(fingerprint => !negatedFingerprints.Contains(fingerprint)) // is O(1)
                .ToList();
        }


        var allGardenLocations = arboretum.GetAllGardensPrimaryLocation().ToImmutableHashSet();
        var selectedGardens = new ConcurrentDictionary<HyphaeStrain, GardenDto>();

        // Parallelize the plant retrieval and their garden association.
        var plantTasks = plainFingerprints.AsParallel().Select(async fingerprint =>
        {
            var plant = await _plantRepo.GetByFingerprintAsync(fingerprint);
            if (plant == null)
            {
                throw new Exception(
                    $"Plant with fingerprint {fingerprint} not retrievable!"
                    );
            }

            // check for gardens plant is planted in...
            var plantIsInGardens = plant.AssociatedHyphae
                .Where(allGardenLocations.Contains)
                .Select(arboretum.ViewGarden)
                .Where(garden => garden != null
                                 && garden.ContainsPlant(plant.UniqueMarker))
                .ToArray();
            // as plant might have identifying hyphae, but is not in the garden.

            if (plantIsInGardens.Length == 0)
            {
                throw new Exception(
                    $"Plant with fingerprint {fingerprint} is roque and not part of any garden!"
                );
            }

            var thisPlantDto = PlantMapper.IntoDto(plant);

            plantIsInGardens.AsParallel().ForAll(garden =>
            {
                if (garden == null)
                {
                    throw new Exception(
                        $"Cannot access Gardens of plant with fingerprint {plant.UniqueMarker}!"
                    );
                }
                selectedGardens.AddOrUpdate(
                    garden.PrimaryLocation,
                    new GardenDto(
                        PrimaryLocation: garden.PrimaryLocation.ToString(),
                        UniqueMarker: garden.UniqueMarker.ToString(),
                        Plants: [thisPlantDto]
                    ),
                    (key, existingGarden) =>
                    {
                        existingGarden.Plants.Add(thisPlantDto);
                        return existingGarden;
                    }
                );

            });

            return plant;
        });

        try
        {
            await Task.WhenAll(plantTasks);
        }
        catch (AggregateException ex)
        {
            var errorMessages = ex.InnerExceptions.Select(e => e.Message);
            var errorMessage = string.Join("\n", errorMessages);

            return Fail<MyceliumQuerySuccess, MyceliumQueryError>(
                MyceliumQueryError.PlantLoadingFailed,
                errorMessage, "Plant-Repository"
            );
        }


        // Return the result as a success with the valid plants
        return Ok<MyceliumQuerySuccess, MyceliumQueryError>(
            new MyceliumQuerySuccess(
                Gardens: selectedGardens.Values.ToImmutableList()
            )
        );
    }

}