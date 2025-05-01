using System.Collections.Immutable;
using VirtualArboretum.Core.Application.DataTransferObjects.ModelDTOs;

namespace VirtualArboretum.Core.Application.DataTransferObjects.ResultDTOs;

/// <summary>
/// MatchingGardens is a list of gardens, whose plants match the query.<br/>
/// Each garden itself does not need to match the query, but at least one of its plants does.
/// </summary>
public record RetrievePlantsSuccess(
    ImmutableList<GardenDto> MatchingGardens
    );