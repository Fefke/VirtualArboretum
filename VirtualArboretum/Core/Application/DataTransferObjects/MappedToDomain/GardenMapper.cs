using System.CommandLine;
using System.Text;
using VirtualArboretum.Core.Application.DataTransferObjects.ModelDTOs;
using VirtualArboretum.Core.Application.Services;
using VirtualArboretum.Core.Domain.AggregateRoots;
using VirtualArboretum.Core.Domain.Entities;
using VirtualArboretum.Core.Domain.ValueObjects;

namespace VirtualArboretum.Core.Application.DataTransferObjects.MappedToDomain;

public class GardenMapper
{
    /// <summary>
    /// Turns your garden Template into a real one.
    /// Please note, fatal failures throw ArgumentExceptions.
    /// Some Failures, like false plantTemplates, are aggregated and collectively thrown.
    /// </summary>
    public static Garden IntoGarden(GardenDto gardenTemplate)
    {
        var initErrors = new StringBuilder();

        // 1. Unique Marker
        var uniqueMarker = Fingerprint.TryCreate(gardenTemplate.UniqueMarker);

        if (uniqueMarker == null)
        {
            throw new ArgumentException(
                $"Invalid garden fingerprint provided: {gardenTemplate.UniqueMarker}"
                );
        }

        // 2. primary Location

        var possibleLocations = HyphaeSerializationService.Deserialize(
            gardenTemplate.PrimaryLocation
            );

        if (possibleLocations.Count > 1)
        {
            var falseLocations = possibleLocations.Select(strain => strain.ToString());
            throw new ArgumentException(
                "You cannot have multiple primary locations for your Garden! " +
                $"(Forbidden: {falseLocations})"
                );
        }

        var primaryLocation = possibleLocations.First();


        // 3. Plants
        if (gardenTemplate.Plants.Count == 0)
        {
            return new Garden(
                primaryLocation,
                new List<Plant>(),
                uniqueMarker
            );
        }

        var plants = new List<Plant>(gardenTemplate.Plants.Count);
        foreach (var plantTemplate in gardenTemplate.Plants)
        {
            Plant plant;
            try
            {
                plant = PlantMapper.IntoPlant(plantTemplate);

            }
            catch (Exception e)
            {
                initErrors.AppendLine(
                    $"Your plant template ({plantTemplate.UniqueMarker}) is invalid:\n{e.Message}"
                    );
                continue;
            }

            plants.Add(
            // might throw an exception.
                plant
            );
        }

        if (initErrors.Length > 0)
        {
            throw new ArgumentException(initErrors.ToString());
        }

        return new Garden(
            primaryLocation,
            plants,
            uniqueMarker
            );
    }

    public static GardenDto IntoDto(Garden garden)
    {
        // Serialize all plants..
        var serializedPlants = new List<PlantDto>(garden.AmountOfPlants());

        var serializationErrors = new StringBuilder();

        foreach (var plant in garden.GetPlants())
        {
            PlantDto serialPlant;
            try
            {
                serialPlant = PlantMapper.IntoDto(plant);
                
            }
            catch (Exception e)
            {
                serializationErrors.AppendLine(
                    $" - ({plant.UniqueMarker}), due to:\n {e.Message}"
                    );
                continue;
            }
            serializedPlants.Add(serialPlant);
        }

        if (serializationErrors.Length > 0)
        {
            throw new ArgumentException(
                $"Your garden ({garden.UniqueMarker}) somehow has invalid plants in Domain-Model:\n" +
                $"{serializationErrors}"
                );
        }


        return new GardenDto(
            garden.PrimaryLocation.ToString(),
            garden.UniqueMarker.ToString(),
            serializedPlants
            );
    }
}