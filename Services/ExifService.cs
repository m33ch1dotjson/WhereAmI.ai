using GeoSpy.ai.Models;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;

namespace GeoSpy.ai.Services;

public class ExifService : IExifService
{
    public async Task<PhotoMetadata> ExtractMetadataAsync(Stream imageStream)
    {
        return await Task.Run(() =>
        {
            var metadata = new PhotoMetadata();
            
            try
            {
                imageStream.Position = 0;
                var directories = ImageMetadataReader.ReadMetadata(imageStream);
                
                foreach (var directory in directories)
                {
                    foreach (var tag in directory.Tags)
                    {
                        var tagName = tag.Name;
                        var tagValue = tag.Description ?? tag.ToString();
                        
                        // GPS Coordinates
                        if (tagName.Contains("GPS Latitude"))
                        {
                            var latDir = directories
                                .SelectMany(d => d.Tags)
                                .FirstOrDefault(t => t.Name.Contains("GPS Latitude Ref"));
                            var latValue = ParseCoordinate(tagValue, latDir?.Description);
                            if (latValue.HasValue)
                                metadata.Latitude = latValue;
                        }
                        else if (tagName.Contains("GPS Longitude"))
                        {
                            var lonDir = directories
                                .SelectMany(d => d.Tags)
                                .FirstOrDefault(t => t.Name.Contains("GPS Longitude Ref"));
                            var lonValue = ParseCoordinate(tagValue, lonDir?.Description);
                            if (lonValue.HasValue)
                                metadata.Longitude = lonValue;
                        }
                        // Camera info
                        else if (tagName == "Make")
                        {
                            metadata.CameraMake = tagValue;
                        }
                        else if (tagName == "Model")
                        {
                            metadata.CameraModel = tagValue;
                        }
                        // Date taken
                        else if (tagName.Contains("Date/Time") || tagName.Contains("DateTime"))
                        {
                            if (DateTime.TryParse(tagValue, out var dateTime))
                            {
                                metadata.DateTaken = dateTime;
                            }
                        }
                        // Image dimensions
                        else if (tagName == "Image Width")
                        {
                            if (int.TryParse(tagValue, out var width))
                                metadata.Width = width;
                        }
                        else if (tagName == "Image Height")
                        {
                            if (int.TryParse(tagValue, out var height))
                                metadata.Height = height;
                        }
                        else if (tagName == "Orientation")
                        {
                            metadata.Orientation = tagValue;
                        }
                        
                        // Store all metadata for reference
                        metadata.AdditionalMetadata[tagName] = tagValue;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but continue - metadata extraction is optional
                metadata.AdditionalMetadata["Error"] = ex.Message;
            }
            
            return metadata;
        });
    }
    
    private double? ParseCoordinate(string coordinate, string? direction)
    {
        try
        {
            // Format: "52° 5' 0.00\" N" or "52 5 0.00"
            var parts = coordinate.Split(new[] { '°', '\'', '"', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 3)
            {
                var degrees = double.Parse(parts[0]);
                var minutes = double.Parse(parts[1]);
                var seconds = parts.Length > 2 ? double.Parse(parts[2]) : 0;
                
                var decimalDegrees = degrees + (minutes / 60.0) + (seconds / 3600.0);
                
                // Apply direction (S or W = negative)
                if (direction == "S" || direction == "W")
                    decimalDegrees = -decimalDegrees;
                
                return decimalDegrees;
            }
        }
        catch
        {
            // If parsing fails, try direct parse
            if (double.TryParse(coordinate, out var directValue))
                return directValue;
        }
        
        return null;
    }
}
