namespace VirtualArboretum.Core.Application.DataTransferObjects.ModelDTOs;

public record CellDto(
    String CellType,
    String UniqueMarker,
    HyphaeStrainDto OrganellLocation
    );