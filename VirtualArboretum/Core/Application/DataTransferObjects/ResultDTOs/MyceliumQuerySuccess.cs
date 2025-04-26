using System.Collections.Immutable;
using VirtualArboretum.Core.Application.DataTransferObjects.ModelDTOs;

namespace VirtualArboretum.Core.Application.DataTransferObjects.ResultDTOs;

/// <summary>
/// As long as at least one garden is found, the query is successful
/// and will return this DTO, with Gardens containing all their associated plants
/// and each element containing the list of associated hyphae.
/// </summary>
public record MyceliumQuerySuccess(
    ImmutableList<GardenWithPlantsDto> Gardens
    );