namespace GeoSpy.ai.Models;

public class PhotoMetadata
{
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? CameraMake { get; set; }
    public string? CameraModel { get; set; }
    public DateTime? DateTaken { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public string? Orientation { get; set; }
    public Dictionary<string, string> AdditionalMetadata { get; set; } = new();
}
