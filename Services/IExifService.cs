using GeoSpy.ai.Models;

namespace GeoSpy.ai.Services;

public interface IExifService
{
    Task<PhotoMetadata> ExtractMetadataAsync(Stream imageStream);
}
