namespace VirtualArboretum.Core.Application.DataTransferObjects.ModelDTOs;

public record GardenDto(
    String PrimaryLocation,
    String UniqueMarker,
    IList<PlantDto> Plants
);