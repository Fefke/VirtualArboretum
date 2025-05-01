namespace VirtualArboretum.Core.Application.DataTransferObjects.InputDTOs;

/// <summary>
/// Does return matching results for PrimaryHyphae.
/// <i>Actual implementation determines context of the associated hyphae.</i>
/// </summary>
public record HyphaeStrainInput(
    String HyphaeStrain
        );