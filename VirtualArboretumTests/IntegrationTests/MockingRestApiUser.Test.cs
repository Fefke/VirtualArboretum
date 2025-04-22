using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VirtualArboretum.Presentation.Services;

namespace VirtualArboretumTests;

[TestClass]
public class MockingRestApiUser
{
 
    [TestMethod]
    public async Task TestMethod1()
    {
        // TODO: Apply REST API Layout & finish test
        string plantJson = @"{
            ""name"": ""Aloe Vera"",
            ""type"": ""Succulent"",
            ""growthStage"": ""Mature"",
            ""description"": ""A medicinal plant with thick, spiky leaves""
        }";

        string hyphaJson = @"{
            ""name"": ""MedicinalConnection"",
            ""description"": ""Connects medicinal plants"",
            ""connects"": [""Aloe Vera"", ""Lavender""]
        }";

        string tagJson = @"{
            ""name"": ""medicinal"",
            ""description"": ""Plants with medicinal properties"",
            ""associatedPlants"": [""Aloe Vera"", ""Lavender"", ""Echinacea""]
        }";

        // Erwartete Antwortvorlagen
        var expectedPlantResponse = new
        {
            StatusCode = System.Net.HttpStatusCode.Created,
            Body = new
            {
                name = "Aloe Vera",
                type = "Succulent",
                id = "", // wird dynamisch zugewiesen
                links = new[] { new { rel = "self", href = "/plants/Aloe%20Vera" } }
            }
        };

        var expectedHyphaResponse = new
        {
            StatusCode = System.Net.HttpStatusCode.Created,
            Body = new
            {
                name = "MedicinalConnection",
                connects = new[] { "Aloe Vera", "Lavender" }
            }
        };

        Console.WriteLine("1. Create plant");
        var plantResponse = await RestClient.SendRequestAsync(HttpMethod.Post, "/plants/Aloe%20Vera", plantJson);

        Console.WriteLine("2. Apply Hypha to plant");
        var hyphaResponse = await RestClient.SendRequestAsync(HttpMethod.Post, "/hyphae/MedicinalConnection", hyphaJson);

        Console.WriteLine("4. Fetch plant with its information");
        var queryParams = new Dictionary<string, string> { { "hyphae", "true" } };
        var getPlantResponse = await RestClient.SendRequestAsync(HttpMethod.Get, "/plants/Aloe%20Vera", null, queryParams);

        // Assertions - TODO: Assert target form.

        // Plant creation
        Assert.IsTrue(plantResponse.IsSuccess, $"Did not create Plant: {plantResponse.ErrorMessage}");
        Assert.AreEqual(System.Net.HttpStatusCode.Created, plantResponse.StatusCode);

        // Hypha creation
        Assert.IsTrue(hyphaResponse.IsSuccess, $"Did not create Hypha: {hyphaResponse.ErrorMessage}");
        Assert.AreEqual(System.Net.HttpStatusCode.Created, hyphaResponse.StatusCode);

        // Get plant with hyphae
        Assert.IsTrue(getPlantResponse.IsSuccess, $"Not able to fetch just created plant: {getPlantResponse.ErrorMessage}");
        Assert.AreEqual(System.Net.HttpStatusCode.OK, getPlantResponse.StatusCode);
    }
}
