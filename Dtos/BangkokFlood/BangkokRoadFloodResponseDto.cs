using System.Text.Json.Serialization;

namespace PolicyClaimHub.Dtos.BangkokFlood;

public sealed class BangkokRoadFloodResponseDto
{
    public DateTimeOffset? LastAttemptAt { get; set; }
    public DateTimeOffset? LastFetchedAt { get; set; }
    public bool FetchFailed { get; set; }
    public int Total { get; set; }
    public List<BangkokRoadFloodSensorDto> Sensors { get; set; } = [];
}

public sealed class BangkokRoadFloodSensorDto
{
    public string Id { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Road { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    [JsonPropertyName("lat")]
    public double Latitude { get; set; }

    [JsonPropertyName("lon")]
    public double Longitude { get; set; }
    public string Status { get; set; } = string.Empty;
    public double? ValueCm { get; set; }
    public DateTimeOffset? ObservedAt { get; set; }
}
