namespace GeoSpy.ai.Models;

public class UploadViewModel
{
    public IFormFile? Photo { get; set; }
    public PhotoMetadata? Metadata { get; set; }
    public LocationResult? LocationResult { get; set; }
    public string? ErrorMessage { get; set; }
}
