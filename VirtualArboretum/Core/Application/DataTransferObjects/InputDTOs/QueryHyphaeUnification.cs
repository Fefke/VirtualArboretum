namespace VirtualArboretum.Core.Application.DataTransferObjects.InputDTOs;

/// <summary>
/// Please note, this defaults to AND logic/ unification quantity, <br/>
/// meaning all hyphae must be present to yield the target.
/// </summary>
public record QueryHyphaeUnification(
    IList<String> AssociatedHyphae
    );