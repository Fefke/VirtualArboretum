namespace VirtualArboretum.Core.Application.DataTransferObjects.ModelDTOs;

public record PlantDto(
    String UniqueMarker,
    String PrimaryHyphae,
    IList<CellDto> Cells,
    IList<String> AssociatedHyphae
);