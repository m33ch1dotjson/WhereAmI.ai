namespace GeoSpy.ai.Models;

public class LocationResult
{
    public string? LocationName { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? Explanation { get; set; }
    public double? Confidence { get; set; }
    public List<string> Clues { get; set; } = new();
    public string? ErrorMessage { get; set; }
}
