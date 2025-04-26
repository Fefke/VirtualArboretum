namespace VirtualArboretum.Core.Application.DataTransferObjects.InputDTOs;

/// <summary>
/// Please note, this defaults to OR logic/ intersection quantity, <br/>
/// meaning at least one hypha must be present to yield the target.
/// </summary>
public record QueryHyphaeIntersection(
    IList<String> AssociatedHyphae
    );