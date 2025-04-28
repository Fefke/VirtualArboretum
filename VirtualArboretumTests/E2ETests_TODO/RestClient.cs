using System.Net.Http.Json;
using System.Text.Json.Nodes;
using System.Text;
using VirtualArboretum.Presentation.Services;

namespace VirtualArboretumTests;

public class RestClient
{
    protected static readonly HttpClient client = new HttpClient();
    protected static string BaseUrl = "http://localhost:" + ServerService.Port;

    public static async Task<ApiResponse> SendRequestAsync(
        HttpMethod method,
        string endpoint,
        string jsonContent,  // parsing may fail!
        Dictionary<string, string>? queryParams = null
    )
    {
        var uriBuilder = new UriBuilder($"{BaseUrl}{endpoint}");
        if (null != queryParams)
        {
            var query = new StringBuilder();
            foreach (var param in queryParams)
            {
                if (query.Length > 0)
                    query.Append('&');
                query.Append($"{Uri.EscapeDataString(param.Key)}={Uri.EscapeDataString(param.Value)}");
            }
            uriBuilder.Query = query.ToString();
        }

        var request = new HttpRequestMessage(method, uriBuilder.Uri);

        request.Content = JsonContent.Create(
            JsonNode.Parse(jsonContent) // May fail!
            );

        // Send request, await response..
        try
        {
            var startTime = DateTime.Now;
            var response = await client.SendAsync(request);
            var endTime = DateTime.Now;
            var responseTime = (endTime - startTime).TotalMilliseconds;
            Console.WriteLine($@"  [{request.Method}] Test-Request took {responseTime}ms");

            string responseContent = "";
            if (method != HttpMethod.Head)
            {
                responseContent = await response.Content.ReadAsStringAsync();
            }

            JsonNode? deserializedContent = null;
            if (!string.IsNullOrEmpty(responseContent) &&
                response.Content?.Headers?.ContentType?.MediaType == "application/json")
            {
                deserializedContent = JsonNode.Parse(responseContent);
            }

            return new ApiResponse
            {
                StatusCode = response.StatusCode,
                IsSuccess = response.IsSuccessStatusCode,
                Headers = response.Headers.ToDictionary(h => h.Key, h => string.Join(", ", h.Value)),
                RawContent = responseContent,
                JsonContent = deserializedContent,
                ResponseTimeMs = responseTime
            };
        }
        catch (HttpRequestException ex)
        {
            return new ApiResponse
            {
                IsSuccess = false,
                ErrorMessage = ex.Message
            };
        }
    }
}