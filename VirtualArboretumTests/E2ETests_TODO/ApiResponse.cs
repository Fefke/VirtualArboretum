using System.Text.Json;
using System.Text.Json.Nodes;

namespace VirtualArboretumTests;


public class ApiResponse
{
    public System.Net.HttpStatusCode StatusCode { get; init; }
    public bool IsSuccess { get; set; }
    public Dictionary<string, string> Headers { get; init; } = new Dictionary<string, string>();
    public string? RawContent { get; init; }
    public JsonNode? JsonContent { get; init; }
    public double ResponseTimeMs { get; init; }
    public string? ErrorMessage { get; init; }

    public override bool Equals(object? obj)
    {
        return obj is ApiResponse other
               && this.Equals(other);
    }

    public bool Equals(ApiResponse other)
    {
        return this.IsSuccess == other.IsSuccess &&
               this.StatusCode == other.StatusCode &&
               this.RawContent == other.RawContent &&
               //this.ResponseTimeMs == other.ResponseTimeMs &&  // trivial
               //this.ErrorMessage == other.ErrorMessage && // trivial, bcs. IsSuccess indicates
               this.Headers.SequenceEqual(other.Headers) &&
               JsonContent?.ToJsonString() == other.JsonContent?.ToJsonString();
    }

}
