using VirtualArboretum.Core.Application.DataTransferObjects.InputDTOs;
using VirtualArboretum.Core.Application.DataTransferObjects.MappedToDomain;
using VirtualArboretum.Core.Application.DataTransferObjects.ModelDTOs;
using VirtualArboretum.Core.Application.UseCases;
using VirtualArboretum.Core.Domain.AggregateRoots;
using VirtualArboretum.Core.Domain.ValueObjects;
using VirtualArboretum.Infrastructure.Repositories.InMemory;
using VirtualArboretumTests.Fakes;

namespace VirtualArboretumTests.UnitTests.UseCases;

[TestClass]
public class PlacePlantTest
{


    [TestMethod]
    public async Task IntoGardenWithoutAdditionalMycorrhization_ValidPlant_ReturnsSuccess()
    {
        // # Arrange
        var testGarden = FakeGardenFactory.CreateRawTestGarden("TestGarden");

        // Set up the repositories
        var gardenRepo = new InMemoryGardenRepository([testGarden]);
        var arboretumRepo = new InMemoryArboretumRepository([testGarden]);
        var plantRepo = new InMemoryPlantRepository();

        // Create Test Plant
        var testPlant = FakePlantFactory.CreateTestPlant(
            "TestPlant",
            "#this-strain-should-not-be-associated#neither-this-one", null, null);

        var testPlantDto = PlantMapper.IntoDto(testPlant);

        var gardenId = new GardenIdentifierInput(testGarden.UniqueMarker.ToString());

        // Create the use case with our in-memory repositories
        var placePlantUseCae = new PlacePlant(arboretumRepo, gardenRepo, plantRepo);

        // # Act
        var result = await placePlantUseCae
            .IntoGardenWithoutAdditionalMycorrhization(testPlantDto, gardenId);

        // # Assert
        Assert.IsTrue(result.IsSuccess);
        var resultPlant = result.Value;
        Assert.IsNotNull(resultPlant);

        Assert.AreEqual("TestPlant", resultPlant.PrimaryPlantHyphae);
        Assert.AreEqual(gardenId.GardenFingerprint, resultPlant.NewGardenFingerprint);

        // Nooow, the plant should still hold its hyphae (due to information preservation)
        //  => but, the mycelium should not be aware of the plant, just the garden.
        var arboretum = arboretumRepo.Open();

        // Plant should not be present by itself
        Assert.IsFalse(arboretum.Mycelium.ContainsMycorrhization(
            testPlant.Name, testPlant.UniqueMarker
            ));

        // Plants hyphae strains should not associate with plant.
        Assert.IsFalse(arboretum.Mycelium.ContainsMycorrhizations(
            testPlant.AssociatedHyphae, testPlant.UniqueMarker
            ));

    }

    [TestMethod]
    public async Task IntoGardenWithoutAdditionalMycorrhization_GardenNotFound_ReturnsFailure()
    {
        // Arrange
        var gardenRepo = new InMemoryGardenRepository(
            new List<GardenDto>() // is empty.
            );
        var arboretumRepo = new InMemoryArboretumRepository(
            new List<Garden>() // is empty.
            );
        var plantRepo = new InMemoryPlantRepository();

        // Create Test Plant
        var testPlant = FakePlantFactory.CreateTestPlant(
            "TestPlant",
            "#this-strain-should-not-be-associated#neither-this-one", null, null);

        var testPlantDto = PlantMapper.IntoDto(testPlant);

        var nonExistentGardenId = new GardenIdentifierInput(
            new Fingerprint().ToString() // random, non-existing GardenIdentifier.
            );

        // Create the use case with our in-memory repositories
        var placePlantUseCae = new PlacePlant(arboretumRepo, gardenRepo, plantRepo);


        // Act
        var result = await placePlantUseCae
            .IntoGardenWithoutAdditionalMycorrhization(testPlantDto, nonExistentGardenId);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(PlacePlantErrors.GardenNotFound, result.Error.Code);
    }

    [TestMethod]
    public async Task IntoGarden_PlantAlreadyExists_ReturnsFailure()
    {
        // Arrange
        var testGarden = FakeGardenFactory.CreateRawTestGarden("TestGarden");

        // Set up the repositories
        var gardenRepo = new InMemoryGardenRepository([testGarden]);
        var arboretumRepo = new InMemoryArboretumRepository([testGarden]);
        var plantRepo = new InMemoryPlantRepository();

        // Create Test Plant
        var testPlant = FakePlantFactory.CreateTestPlant(
            "TestPlant",
            "#this-strain-should-not-be-associated#neither-this-one", null, null);

        var testPlantDto = PlantMapper.IntoDto(testPlant);

        var gardenId = new GardenIdentifierInput(testGarden.UniqueMarker.ToString());

        // Create the use case with our in-memory repositories
        var placePlantUseCae = new PlacePlant(arboretumRepo, gardenRepo, plantRepo);

        // Act
        var firstResult = await placePlantUseCae
            .IntoGarden(testPlantDto, gardenId);

        var secondResult = await placePlantUseCae
            .IntoGarden(testPlantDto, gardenId);

        // Assert
        Assert.IsTrue(firstResult.IsSuccess);
        Assert.IsFalse(secondResult.IsSuccess);
        Assert.AreEqual(PlacePlantErrors.PlantAlreadyExists, secondResult.Error.Code);
    }

    [TestMethod]
    public async Task IntoGarden_ValidPlant_AssociatesWithMycelium()
    {
        // # Arrange
        var testGarden = FakeGardenFactory.CreateRawTestGarden("TestGarden");

        // Set up the repositories
        var gardenRepo = new InMemoryGardenRepository([testGarden]);
        var arboretumRepo = new InMemoryArboretumRepository([testGarden]);
        var plantRepo = new InMemoryPlantRepository();

        // Create Test Plant
        var testPlant = FakePlantFactory.CreateTestPlant(
            "TestPlant",
            "#this-strain-should-absolutely-be-associated" +
            "#this-strain-should-be-associated-as-well", null, null);
        var testPlantDto = PlantMapper.IntoDto(testPlant);

        var gardenId = new GardenIdentifierInput(testGarden.UniqueMarker.ToString());

        // Create the use case with our in-memory repositories
        var placePlantUseCae = new PlacePlant(arboretumRepo, gardenRepo, plantRepo);

        // # Act
        var result = await placePlantUseCae.IntoGarden(testPlantDto, gardenId);

        // # Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Value);
        Assert.AreEqual("TestPlant", result.Value.PrimaryPlantHyphae);

        // Verify mycorrhization via mycelium in arboretum 
        var arboretum = arboretumRepo.Open();

        // Plant be present by itself
        Assert.IsTrue(arboretum.Mycelium.ContainsMycorrhization(
            testPlant.Name, testPlant.UniqueMarker
            ));

        // Plants hyphae strains should be associated with plant.
        Assert.IsTrue(arboretum.Mycelium.ContainsMycorrhizations(
            testPlant.AssociatedHyphae, testPlant.UniqueMarker
            ));

    }
}
