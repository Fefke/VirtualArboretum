using VirtualArboretum.Core.Application.DataTransferObjects.InputDTOs;
using VirtualArboretum.Core.Application.DataTransferObjects.ModelDTOs;
using VirtualArboretum.Core.Application.Interfaces;
using VirtualArboretum.Core.Application.UseCases;
using VirtualArboretum.Core.Domain.AggregateRoots;
using VirtualArboretum.Core.Domain.Entities;
using VirtualArboretum.Core.Domain.ValueObjects;
using VirtualArboretum.Infrastructure.Repositories.InMemory;
using VirtualArboretumTests.Fakes;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace VirtualArboretumTests.UnitTests.UseCases;

[TestClass]
public class RetrievePlantsTest
{
    // Helper record to hold the initialized components for tests

    /// <summary>
    /// Sets up the necessary InMemory repositories and the RetrievePlants use case.
    /// Can be parameterized with initial data for plants and gardens.
    /// </summary>
    private RetrievePlants SetupRepositoriesAndUseCase(
        IList<Plant>? initialPlants = null,
        IList<Garden>? initialGardens = null)
    {
        initialPlants ??= new List<Plant>();
        initialGardens ??= new List<Garden>();

        // Initialize repositories with optional initial data
        var plantRepo = new InMemoryPlantRepository(initialPlants);
        var gardenRepo = new InMemoryGardenRepository(initialGardens);
        var arboretumRepo = new InMemoryArboretumRepository(initialGardens);

        // Create the use case instance
        var retrievePlantsUseCase = new RetrievePlants(arboretumRepo, plantRepo);

        return retrievePlantsUseCase;
    }

    // ## Tests for GetAllAsync

    [TestMethod]
    public async Task GetAllAsync_WhenNoGardensExist_ReturnsSuccessWithEmptyList()
    {
        // Arrange: Setup with no initial data
        var retrievePlantsUseCase = SetupRepositoriesAndUseCase();

        // Act: Call the method under test
        var result = await retrievePlantsUseCase.GetAllAsync();

        // Assert: Verify the outcome
        IsTrue(result.IsSuccess, "The operation should succeed even if no gardens exist.");
        IsNotNull(result.Value, "Success result value should not be null.");
        AreEqual(0, result.Value.MatchingGardens.Count, "Expected an empty list of gardens.");
    }

    [TestMethod]
    public async Task GetAllAsync_WhenGardensAndPlantsExist_ReturnsSuccessWithAllGardensAndPlants()
    {
        // Arrange: Setup with specific test data
        var plant1 = FakePlantFactory.CreateTestPlant("PlantA", "#HyphaA", 0, 0);
        var plant2 = FakePlantFactory.CreateTestPlant("PlantB", "#HyphaB", 0, 0);
        var garden1 = FakeGardenFactory.CreateRawTestGarden("Garden1");
        garden1.AddPlant(plant1); // Add plant to garden
        var garden2 = FakeGardenFactory.CreateRawTestGarden("Garden2");
        garden2.AddPlant(plant2); // Add plant to garden

        // Important: Repos need the final state of gardens/plants
        var retrievePlantsUseCase = SetupRepositoriesAndUseCase(
            initialPlants: [plant1, plant2],
            initialGardens: [garden1, garden2]);

        // Act
        var result = await retrievePlantsUseCase.GetAllAsync();

        // Assert
        IsTrue(result.IsSuccess);
        IsNotNull(result.Value);
        AreEqual(2, result.Value.MatchingGardens.Count, "Should return two gardens.");

        // Further assertions: Check specific garden/plant details in the DTOs
        var garden1Dto = result.Value.MatchingGardens
            .FirstOrDefault(g => g.PrimaryLocation == garden1.PrimaryLocation.ToString());
        IsNotNull(garden1Dto);
        AreEqual(1, garden1Dto.Plants.Count);
        AreEqual(plant1.Name.ToString(), garden1Dto.Plants[0].PrimaryHyphae);

        var garden2Dto = result.Value.MatchingGardens
            .FirstOrDefault(g => g.PrimaryLocation == garden2.PrimaryLocation.ToString());
        IsNotNull(garden2Dto);
        AreEqual(1, garden2Dto.Plants.Count);
        AreEqual(plant2.Name.ToString(), garden2Dto.Plants[0].PrimaryHyphae);
    }

    // ## Tests for GetPlantByFingerprintAsync

    [TestMethod]
    public async Task GetPlantByFingerprintAsync_ExistingPlant_ReturnsSuccessWithPlantInGarden()
    {
        // Arrange
        var plant1 = FakePlantFactory.CreateTestPlant("PlantA", "#HyphaA", 0, 0);
        var garden1 = FakeGardenFactory.CreateRawTestGarden("Garden1");
        garden1.AddPlant(plant1);

        var retrievePlantsUseCase = SetupRepositoriesAndUseCase(
             initialPlants: [plant1],
             initialGardens: [garden1]);

        var input = new PlantIdentifierInput(plant1.UniqueMarker.ToString());

        // Act
        var result = await retrievePlantsUseCase.GetPlantByFingerprintAsync(input);

        // Assert
        IsTrue(result.IsSuccess);
        IsNotNull(result.Value);
        AreEqual(1, result.Value.MatchingGardens.Count, "Should find the plant in one garden.");
        var gardenDto = result.Value.MatchingGardens[0];
        AreEqual(garden1.UniqueMarker.ToString(), gardenDto.UniqueMarker);
        AreEqual(1, gardenDto.Plants.Count);
        AreEqual(plant1.UniqueMarker.ToString(), gardenDto.Plants[0].UniqueMarker);
    }

    [TestMethod]
    // TODO: Debug this!
    public async Task GetPlantByFingerprintAsync_NonExistingPlant_ReturnsFailurePlantNotFound()
    {
        // Arrange
        var retrievePlantsUseCase = SetupRepositoriesAndUseCase(); // Empty repos
        var nonExistentFingerprint = new Fingerprint().ToString();
        var input = new PlantIdentifierInput(nonExistentFingerprint);

        // Act
        var result = await retrievePlantsUseCase.GetPlantByFingerprintAsync(input);

        // Assert
        IsFalse(result.IsSuccess, "Result should indicate failure.");
        AreEqual(
            RetrievePlantsError.RepositoryError,
            result.Error.Code,
            "Error code should indicate PlantNotFound.");
    }

    [TestMethod]
    public async Task GetPlantByFingerprintAsync_InvalidFingerprintFormat_ReturnsFailureInvalidInput()
    {
        // Arrange
        var retrievePlantsUseCase = SetupRepositoriesAndUseCase();
        var input = new PlantIdentifierInput("this-is-not-a-guid");

        // Act
        var result = await retrievePlantsUseCase.GetPlantByFingerprintAsync(input);

        // Assert
        IsFalse(result.IsSuccess);
        AreEqual(RetrievePlantsError.InvalidInput, result.Error.Code);
    }

    // ## Tests for ByPrimaryHyphaeAsync

    [TestMethod]
    public async Task ByPrimaryHyphaeAsync_ExistingPlant_ReturnsSuccessWithPlantInGarden()
    {
        // Arrange
        var plant1 = FakePlantFactory.CreateTestPlant("PlantA", "#HyphaA", 0, 0); // Plant name is #PlantA
        var garden1 = FakeGardenFactory.CreateRawTestGarden("Garden1");
        garden1.AddPlant(plant1);

        var retrievePlantsUseCase = SetupRepositoriesAndUseCase(
             initialPlants: [plant1],
             initialGardens: [garden1]);

        var input = new HyphaeStrainDto(plant1.Name.ToString()); // Search by #PlantA

        // Act
        var result = await retrievePlantsUseCase.ByPrimaryHyphaeAsync(input);

        // Assert
        IsTrue(result.IsSuccess);
        IsNotNull(result.Value);
        AreEqual(1, result.Value.MatchingGardens.Count);
        AreEqual(plant1.UniqueMarker.ToString(), result.Value.MatchingGardens[0].Plants[0].UniqueMarker);
    }

    [TestMethod]
    public async Task ByPrimaryHyphaeAsync_NonExistingPlant_ReturnsFailurePlantNotFound()
    {
        // Arrange
        var retrievePlantsUseCase = SetupRepositoriesAndUseCase();
        var input = new HyphaeStrainDto("#NonExistentPlant");

        // Act
        var result = await retrievePlantsUseCase.ByPrimaryHyphaeAsync(input);

        // Assert
        IsFalse(result.IsSuccess);
        AreEqual(RetrievePlantsError.PlantNotFound, result.Error.Code);
    }

    [TestMethod]
    // TODO: Debug this!
    public async Task ByPrimaryHyphaeAsync_InvalidHyphaeFormat_ReturnsFailureSerializationError()
    {
        // Arrange
        var retrievePlantsUseCase = SetupRepositoriesAndUseCase();
        var input = new HyphaeStrainDto("#Invalid-Format#With#Reserved-Char");
        //  => Contains multiple HyphaeStrains, which is not allowed for identifying HyphaeStrain

        // Act
        var result = await retrievePlantsUseCase.ByPrimaryHyphaeAsync(input);

        // Assert
        IsFalse(result.IsSuccess);
        AreEqual(RetrievePlantsError.InvalidInput, result.Error.Code);
    }

    [TestMethod]
    public async Task ByPrimaryHyphaeAsync_MultipleHyphaeProvided_ReturnsFailureInvalidInput()
    {
        // Arrange
        var retrievePlantsUseCase = SetupRepositoriesAndUseCase();
        // Represents more than one strain
        var input = new HyphaeStrainDto("#StrainOne#StrainTwo");

        // Act
        var result = await retrievePlantsUseCase.ByPrimaryHyphaeAsync(input);

        // Assert
        IsFalse(result.IsSuccess);
        AreEqual(RetrievePlantsError.InvalidInput, result.Error.Code, "Expected InvalidInput due to multiple strains.");
    }

    // # Tests for ByMyceliumQuery

    private static RetrievePlants SetupForMyceliumQueryTests()
    {
        // ! Do not change this method, unless you also change associated parameter dataset!
        //  => As we allow negation of all via `!#hyphae-strains`.

        var plantA = FakePlantFactory.CreateTestPlant("PlantA", "#HyphaA #color-red", 0, 0);   // In G1
        var plantB = FakePlantFactory.CreateTestPlant("PlantB", "#HyphaB #color-blue", 0, 0);  // In G1 & G2
        var plantC = FakePlantFactory.CreateTestPlant("PlantC", "#HyphaA #color-blue", 0, 0); // In G2
        var plantD = FakePlantFactory.CreateTestPlant("PlantD", "#HyphaC #color-red", 0, 0);  // In G2

        var garden1 = FakeGardenFactory.CreateRawTestGarden("Garden1");
        garden1.AddPlant(plantA);
        garden1.AddPlant(plantB);

        var garden2 = FakeGardenFactory.CreateRawTestGarden("Garden2");
        garden2.AddPlant(plantB); // PlantB is in both gardens
        garden2.AddPlant(plantC);
        garden2.AddPlant(plantD);

        // Return the setup
        return new RetrievePlantsTest().SetupRepositoriesAndUseCase(
           initialPlants: [plantA, plantB, plantC, plantD],
           initialGardens: [garden1, garden2]);
    }

    // ## Data-Driven Test Template
    public record MyceliumQueryTestCase(
        string HyphaeQuery,
        bool ExpectedSuccess,
        RetrievePlantsError? ExpectedErrorCode,
        int ExpectedGardenCount,
        int ExpectedDistinctPlantCount,  // ! Distinct over all occurrences in many gardens.
        string DisplayName)
    {
        // Optional validation logic
        public override string ToString() => DisplayName;
    }

    private static IEnumerable<object[]> GetMyceliumQueryTestCases()
    {
        var testCases = new List<MyceliumQueryTestCase>
        {
            // Basic Matches
            new("#HyphaA", true, null, 2, 2, "Query: Simple HyphaA match"),
            new("#color-blue", true, null, 2, 2, "Query: Simple color-blue match"),
            new("#HyphaC", true, null, 1, 1, "Query: Simple HyphaC match"),
            new("#non-existent", true, null, 0, 0, "Query: No match"),
            new("#--color------blue--", true, null, 2, 2, "Query: Simple color-blue match"),
            // => is recoverable input error, as we don't care about hyphae closeness (yet).

            // AND Logic
            new("#HyphaA #color-red", true, null, 1, 1, "Query: AND HyphaA AND color-red"),
            new("#HyphaA #color-blue", true, null, 1, 1, "Query: AND HyphaA AND color-blue"),
            
            // OR Logic
            new("#HyphaB | #HyphaC", true, null, 2, 2, "Query: OR HyphaB OR HyphaC"),
            new("#color-red | #color-blue ", true, null, 2, 4, "Query: OR color-red OR color-blue"),
            new("#color-red | #color-blue | #color-red ", true, null, 2, 4, "Query: OR color-red OR color-blue"),
            // => please note, this query is not optimized yet for deduplication of selection,
            //    which does mean #color-red will be selected twice, but should produce same outcome
            //    as previous test, as GardenDto should be deduplicated with their plants.
            
            // NOT Logic
            new("#HyphaA !#color-red", true, null, 1, 1, "Query: NOT HyphaA AND NOT color-red"),
            new("!#HyphaA", true, null, 2, 2, "Query: NOT HyphaA, does match any plant not having HyphaA"),

            // Combined Logic
            new("#HyphaA !#color-red | #HyphaC", true, null, 1, 2, "Query: Combined OR/NOT"),
            new("#HyphaA|!#HyphaA", true, null, 2, 4, "Query is Self-Excluding, does match *all* plants"),


            // Invalid Cases
            new("", false, RetrievePlantsError.InvalidInput, 0, 0, "Query: Empty"),
            new("--#", false, RetrievePlantsError.InvalidInput, 0, 0, "Query: Invalid Syntax"),
            new("#--", false, RetrievePlantsError.InvalidInput, 0, 0, "Query: Invalid Syntax, one shall not connect voids."),
            new("i-want-to-be-a-hyphae-strain", false, RetrievePlantsError.InvalidInput, 0, 0,
                "Query: Invalid Syntax, as Hyphae input not explicitly stated."),
            new("###", false, RetrievePlantsError.InvalidInput, 0, 0, "Query: Invalid Syntax, no Hyphae to select provided."),
            new("!#", false, RetrievePlantsError.InvalidInput, 0, 0, "Query: Invalid Syntax, no Hyphae to negate provided."),
        };

        return testCases.Select(tc => new object[] { tc });
    }

    [DataTestMethod]
    [DynamicData(nameof(GetMyceliumQueryTestCases), DynamicDataSourceType.Method)]
    public async Task ByMyceliumQuery_VariousScenarios_ReturnsExpectedResult(MyceliumQueryTestCase testCase)
    {
        // Arrange
        var retrievePlantsUseCase = SetupForMyceliumQueryTests();
        var input = new QueryMyceliumInput(testCase.HyphaeQuery);

        // Act
        var result = await retrievePlantsUseCase.ByMyceliumQuery(input);

        // Assert
        AreEqual(
            testCase.ExpectedSuccess, result.IsSuccess,
            $"Query '{testCase.HyphaeQuery}' - Success state mismatch.");

        if (testCase.ExpectedSuccess)
        {
            IsNotNull(result.Value, $"Query '{testCase.HyphaeQuery}' - Value should not be null on success.");
            AreEqual(testCase.ExpectedGardenCount, result.Value.MatchingGardens.Count,
                $"Query '{testCase.HyphaeQuery}' - Garden count mismatch.");

            var totalDistinctPlants = result.Value.MatchingGardens
                .SelectMany(garden => garden.Plants)
                .DistinctBy(plant => plant.UniqueMarker)
                .ToList();

            AreEqual(testCase.ExpectedDistinctPlantCount, totalDistinctPlants.Count,
                $"Query '{testCase.HyphaeQuery}' - Total plant count mismatch.");

        }
        else
        {
            IsNotNull(result.Error,
                $"Query '{testCase.HyphaeQuery}' - Error object should not be null on failure.");
            AreEqual(testCase.ExpectedErrorCode, result.Error.Code,
                $"Query '{testCase.HyphaeQuery}' - Error code mismatch.");
        }
    }

}
