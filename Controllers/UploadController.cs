using GeoSpy.ai.Models;
using GeoSpy.ai.Services;
using Microsoft.AspNetCore.Mvc;

namespace GeoSpy.ai.Controllers;

public class UploadController : Controller
{
    private readonly IExifService _exifService;
    private readonly IClaudeService _claudeService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<UploadController> _logger;

    public UploadController(
        IExifService exifService,
        IClaudeService claudeService,
        IConfiguration configuration,
        ILogger<UploadController> logger)
    {
        _exifService = exifService;
        _claudeService = claudeService;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Analyze(IFormFile photo)
    {
        if (photo == null || photo.Length == 0)
        {
            return Json(new { error = "Geen foto geüpload" });
        }

        var maxSize = _configuration.GetValue<long>("UploadSettings:MaxFileSize", 10485760);
        if (photo.Length > maxSize)
        {
            return Json(new { error = $"Bestand is te groot. Maximum: {maxSize / 1024 / 1024}MB" });
        }

        var allowedExtensions = _configuration.GetSection("UploadSettings:AllowedExtensions").Get<string[]>() 
            ?? new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var extension = Path.GetExtension(photo.FileName).ToLowerInvariant();
        
        if (!allowedExtensions.Contains(extension))
        {
            return Json(new { error = $"Bestandstype niet ondersteund. Toegestaan: {string.Join(", ", allowedExtensions)}" });
        }

        try
        {
            using var imageStream = new MemoryStream();
            await photo.CopyToAsync(imageStream);
            imageStream.Position = 0;

            // Extract EXIF metadata
            var metadata = await _exifService.ExtractMetadataAsync(imageStream);
            
            // Reset stream for Claude API
            imageStream.Position = 0;

            // Analyze with Claude
            var locationResult = await _claudeService.AnalyzeLocationAsync(imageStream, metadata);

            // If EXIF already has GPS coordinates, use those if Claude didn't find any
            if (!locationResult.Latitude.HasValue && metadata.Latitude.HasValue)
            {
                locationResult.Latitude = metadata.Latitude;
                locationResult.Longitude = metadata.Longitude;
            }

            return Json(new
            {
                success = true,
                metadata = new
                {
                    latitude = metadata.Latitude,
                    longitude = metadata.Longitude,
                    cameraMake = metadata.CameraMake,
                    cameraModel = metadata.CameraModel,
                    dateTaken = metadata.DateTaken
                },
                location = locationResult
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing photo");
            return Json(new { error = $"Fout bij verwerken: {ex.Message}" });
        }
    }
}
