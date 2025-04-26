namespace VirtualArboretum.Core.Application.DataTransferObjects.ResultDTOs;
using System.Collections.Immutable;


/// <summary>
/// Represents the successful result of planting a plant in a garden.
/// </summary>
public record PlacePlantSuccess(
    string PlantFingerprint,
    string NewGardenFingerprint,
    string PrimaryPlantHyphae,
    ImmutableList<string> HyphaeStrains
    //DateTimeOffset PlantedAt
);
