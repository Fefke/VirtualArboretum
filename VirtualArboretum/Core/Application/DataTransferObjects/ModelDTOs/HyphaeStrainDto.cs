namespace VirtualArboretum.Core.Application.DataTransferObjects.ModelDTOs;

/// <summary>
/// Does return matching results for PrimaryHyphae.
/// <i>Actual implementation determines context of the associated hyphae.</i>
/// </summary>
public record HyphaeStrainDto(
    string SingleHyphaeStrain
        );