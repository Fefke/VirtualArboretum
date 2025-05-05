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

namespace VirtualArboretum.Core.Application.Services;

public enum MyceliumQueryError
{
    NoHyphaeQueryProvided,
    InvalidHyphaeQuery,
    PlantLoadingFailed,
    UnknownError
}

/// <summary>
/// Service to query the Mycelium with queries build with hyphae.
/// </summary>
public class MyceliumQueryService  // <ImmutableList<GardenDto>, MyceliumQueryError>
{
    private readonly IArboretumRepository _arboretumRepo;
    private readonly IPlantRepository _plantRepo;
    private readonly Mycelium mycelium;

    private static char notMarker = '!';
    private static char orMarker = '|';
    //private static char andMarker = ' ';  // not required, as AND is just a sequence of hyphae.

    public MyceliumQueryService(
        IArboretumRepository arboretumRepo,
        IPlantRepository plantRepo)
    {
        _arboretumRepo = arboretumRepo ?? throw new ArgumentNullException(nameof(arboretumRepo));
        _plantRepo = plantRepo ?? throw new ArgumentNullException(nameof(plantRepo));
        mycelium = _arboretumRepo.Open().Mycelium;
        //_gardenRepo = gardenRepo ?? throw new ArgumentNullException(nameof(gardenRepo));
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
                MyceliumQueryError.NoHyphaeQueryProvided,
                "Please provide a hyphae query to be resolved to plants in gardens."
            );
        }

        var orBuckets = hyphaeQuery.HyphaeQuery
            .Split(orMarker, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .ToList();

        var resultList = new List<MyceliumQuerySuccess>(orBuckets.Count);

        try
        {
            var tasks = orBuckets.Select(FindAllInSingleOrBucket).ToList();

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

        // Combine gardens from multiple OR selections
        var combinedGardens = new Dictionary<string, GardenDto>();

        // Process all gardens from all results
        foreach (var garden in resultList.SelectMany(result => result.Gardens))
        {
            if (combinedGardens.TryGetValue(garden.UniqueMarker, out var existingGarden))
            {
                // Garden already exists, merge plants ensuring uniqueness
                var existingPlantIds = existingGarden.Plants
                    .Select(p => p.UniqueMarker)
                    .ToHashSet();

                // Add only plants that don't already exist in the garden
                var newPlants = garden.Plants
                    .Where(p => !existingPlantIds.Contains(p.UniqueMarker));

                foreach (var plant in newPlants)
                {
                    existingGarden.Plants.Add(plant);
                }
            }
            else
            {
                combinedGardens[garden.UniqueMarker] = new GardenDto(
                    PrimaryLocation: garden.PrimaryLocation,
                    UniqueMarker: garden.UniqueMarker,
                    Plants: new List<PlantDto>(garden.Plants)
                );
            }
        }
        
        // Return successful result with aggregated data.
        return Ok<MyceliumQuerySuccess, MyceliumQueryError>(
            new MyceliumQuerySuccess(
                Gardens: combinedGardens.Values.ToImmutableList()
            )
        );
    }

    /// <summary>
    /// All Exceptions should contain externalisable messages.
    /// Will throw Exception, if Query not parsable!
    /// </summary>
    private async Task<Result<MyceliumQuerySuccess, MyceliumQueryError>> FindAllInSingleOrBucket(
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

        if (orBucket[0] != HyphaKey.StartMarker && orBucket[0] != notMarker)
        {
            return Fail<MyceliumQuerySuccess, MyceliumQueryError>(
                MyceliumQueryError.InvalidHyphaeQuery,
                $"Your first Hyphae is missing start marker ({HyphaKey.StartMarker}) " +
                $"or not marker ({notMarker}), meaning its invalid."
                );
        }

        // parse orBucket containing hyphae unions and negations.
        var possibleNegationBuckets = orBucket
            .Split(
                $"{notMarker}{HyphaKey.StartMarker}",
                StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .ToList();
        // every element not starting with a StartMarker is to be negated now...

        var negatedHyphaeStrains = new List<HyphaeStrain>(possibleNegationBuckets.Count);
        var plainHyphaeStrains = new List<HyphaeStrain>(); // size not determined atm.

        foreach (var bucket in possibleNegationBuckets)
        {
            try
            {
                if (possibleNegationBuckets.Count > 0 && bucket[0] != HyphaKey.StartMarker)
                {
                    // does mean, element is correctly negated...
                    var correctedStrain = $"{HyphaKey.StartMarker}{bucket}";
                    var hyphaeStrains = HyphaeSerializationService.Deserialize(correctedStrain);

                    if (hyphaeStrains.IsEmpty) continue;
                    negatedHyphaeStrains.Add(hyphaeStrains.First()); // first is negated
                    plainHyphaeStrains.AddRange(hyphaeStrains.Skip(1)); // tail is "normal"
                }
                else
                {
                    // it's the first element, meaning first couple Hyhpae-Strains are plain...
                    var hyphaeStrains = HyphaeSerializationService.Deserialize(bucket);
                    plainHyphaeStrains.AddRange(hyphaeStrains);
                }
            }
            catch (Exception e)
            {
                return Fail<MyceliumQuerySuccess, MyceliumQueryError>(
                    MyceliumQueryError.InvalidHyphaeQuery,
                    e.Message // is user friendly.
                    );
            }
        }

        return await QueryMyceliumIntersection(plainHyphaeStrains, negatedHyphaeStrains);
    }


    /// <summary>
    /// Queries Mycelium for all plants matching the provided hyphae strains.
    /// </summary>
    private async Task<Result<MyceliumQuerySuccess, MyceliumQueryError>> QueryMyceliumIntersection(
        List<HyphaeStrain> plainHyphaeStrains, List<HyphaeStrain> negatedHyphaeStrains)
    {
        return (plainHyphaeStrains.Count, negatedHyphaeStrains.Count) switch
        {
            (0, 0) => Ok<MyceliumQuerySuccess, MyceliumQueryError>(
                new MyceliumQuerySuccess(
                    Gardens: new List<GardenDto>().ToImmutableList()
                    )),
            (0, _) => await QueryMyceliumNegatedAll(negatedHyphaeStrains),
            _ => await QueryMyceliumMixedIntersection(plainHyphaeStrains, negatedHyphaeStrains)
        };
    }


    /// <returns>All Plants, not being associated with provided HyphaeStrains in their Gardens.</returns>
    private async Task<Result<MyceliumQuerySuccess, MyceliumQueryError>> QueryMyceliumNegatedAll(
        IList<HyphaeStrain> negatedHyphaeStrains)
    {
        var lookupTable = new HashSet<HyphaeStrain>(negatedHyphaeStrains);
        var allMycorrhizations = mycelium.GetAllMycorrhizations();

        var resultFingerprints = allMycorrhizations
            .SelectMany(associations => associations.Value)
            .ToHashSet();

        var filterSuccess = negatedHyphaeStrains.All(
            strain =>
            {
                allMycorrhizations.TryGetValue(strain, out var filteredFingerprints);

                if (filteredFingerprints == null) return false;

                return filteredFingerprints.All(
                    resultFingerprints.Remove
                );
            }
            );

        if (!filterSuccess)
        {
            return Fail<MyceliumQuerySuccess, MyceliumQueryError>(
                MyceliumQueryError.UnknownError,
                "For unknown reason its not possible to negate all your Mycorrhizations!"
                );
        }

        return await GroupPlantsIntoGardens(resultFingerprints.ToList());
    }

    private async Task<Result<MyceliumQuerySuccess, MyceliumQueryError>> QueryMyceliumMixedIntersection(
        IList<HyphaeStrain> plainHyphaeStrains, IList<HyphaeStrain> negatedHyphaeStrains)
    {
        var plainFingerprintMappings = mycelium.GetMycorrhizations(plainHyphaeStrains);

        // Optimization: For one association you don't have to Aggregate Fingerprints
        var fingerprintOccurrences = new ConcurrentDictionary<Fingerprint, int>(
            capacity: plainHyphaeStrains.Count, concurrencyLevel: -1);

        // now get all possible plants, to kick those out, containing one of negatedHyphaeStrains.
        plainFingerprintMappings.AsParallel()
            .Where(association => !association.Value.IsEmpty)
            .SelectMany(association => association.Value)
            .ForAll(fingerprint =>
            {
                fingerprintOccurrences.AddOrUpdate(
                    fingerprint,
                    1,
                    (key, oldValue) => oldValue + 1
                );
            });

        var plainFingerprints = fingerprintOccurrences.AsParallel()
            .Where(association => association.Value == plainHyphaeStrains.Count)
            // => does mean, plant is associated with all provided plainHyphaeStrains.
            .Select(association => association.Key)
            .ToList();

        if (plainFingerprints.Count == 0)
        {
            // no plants found to process, fast-return empty result.
            return Ok<MyceliumQuerySuccess, MyceliumQueryError>(
                new MyceliumQuerySuccess(
                    Gardens: new List<GardenDto>().ToImmutableList()
            )
            );
        }

        // negation behaviour is different from plain behaviour,
        // as one match does 
        var negatedFingerprintMappings = mycelium.GetMycorrhizations(negatedHyphaeStrains);

        var negatedFingerprints = negatedFingerprintMappings.AsParallel()
            .Where(association => !association.Value.IsEmpty)
            .SelectMany(association => association.Value)
            .ToHashSet(); // HashSet for quicker lookups.

        // remove all plants, which are associated with negated hyphae.
        var resultingFingerprints = plainFingerprints
            .Where(fingerprint => !negatedFingerprints.Contains(fingerprint)) // is O(1)
            .ToList();

        return await GroupPlantsIntoGardens(resultingFingerprints);
    }


    public async Task<Result<MyceliumQuerySuccess, MyceliumQueryError>> GroupPlantsIntoGardens(IList<Fingerprint> plantIdentifyingFingerprints)
    {
        var arboretum = _arboretumRepo.Open();

        var allGardenLocations = arboretum.GetAllGardensPrimaryLocation().ToImmutableHashSet();
        var selectedGardens = new ConcurrentDictionary<HyphaeStrain, GardenDto>();

        // Parallelize the plant retrieval and their garden association per plant...
        var plantTasks = plantIdentifyingFingerprints.Select(async fingerprint =>
        {
            // 1. Fetch Plant
            var plant = await _plantRepo.GetByFingerprintAsync(fingerprint);
            if (plant == null)
            {
                throw new Exception(
                    $"Plant with fingerprint {fingerprint} not retrievable!"
                    );
            }

            // 2. find all gardens this plant is planted in...
            var plantsGardens = plant.AssociatedHyphae
                .Where(allGardenLocations.Contains)
                .Select(arboretum.ViewGarden)
                .OfType<Garden>() // Does remove nullable annotation.
                .Where(garden => garden.ContainsPlant(plant.UniqueMarker))
                .ToArray();

            if (plantsGardens.Length == 0)
            {
                throw new Exception(
                    $"Plant with fingerprint {fingerprint} is roque and not part of any garden!"
                );  // will be caught below.
            }

            // 3. Formulate as DataTransferObject
            var thisPlantDto = PlantMapper.IntoDto(plant);

            foreach (var garden in plantsGardens)
            {
                if (garden == null)
                {
                    throw new Exception(
                        $"Cannot access Gardens of plant with fingerprint {plant.UniqueMarker}!"
                    );
                }

                // 4. Add this Plant to the selectedGardens dictionary
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
            }
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
                errorMessage, "Group Plants From Strains Into Gardens"
            );
        }

        // Return the result as a success with the valid plants
        return Ok<MyceliumQuerySuccess, MyceliumQueryError>(
            new MyceliumQuerySuccess(
                Gardens: selectedGardens.Values.ToImmutableList()
            )
        );
    }

    public async Task<Result<MyceliumQuerySuccess, MyceliumQueryError>> GroupPlantsIntoGardens(IList<Plant> plants)
    {
        var plantsFingerprints = plants.Select(plant => plant.UniqueMarker).ToList();
        return await GroupPlantsIntoGardens(plantsFingerprints);
    }
}