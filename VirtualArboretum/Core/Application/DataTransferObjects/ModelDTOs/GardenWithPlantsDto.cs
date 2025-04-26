namespace VirtualArboretum.Core.Application.DataTransferObjects.ModelDTOs;

public record GardenWithPlantsDto(
    GardenDto Garden,
    IList<PlantDto> PlantsInGarden
    );