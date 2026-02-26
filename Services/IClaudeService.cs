using GeoSpy.ai.Models;

namespace GeoSpy.ai.Services;

public interface IClaudeService
{
    Task<LocationResult> AnalyzeLocationAsync(Stream imageStream, PhotoMetadata metadata);
}
