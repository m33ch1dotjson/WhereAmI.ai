using System.Text;
using System.Text.Json;
using GeoSpy.ai.Models;

namespace GeoSpy.ai.Services;

public class ClaudeService : IClaudeService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly ILogger<ClaudeService> _logger;

    public ClaudeService(IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<ClaudeService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        
        var apiKey = _configuration["ClaudeApi:ApiKey"];
        if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_CLAUDE_API_KEY_HERE")
        {
            _logger.LogWarning("Claude API key not configured. Please set ClaudeApi:ApiKey in appsettings.json");
        }
        
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
    }

    public async Task<LocationResult> AnalyzeLocationAsync(Stream imageStream, PhotoMetadata metadata)
    {
        var apiKey = _configuration["ClaudeApi:ApiKey"];
        if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_CLAUDE_API_KEY_HERE")
        {
            return new LocationResult
            {
                ErrorMessage = "Claude API key niet geconfigureerd. Voeg je API key toe aan appsettings.json"
            };
        }

        try
        {
            // Convert image to base64
            imageStream.Position = 0;
            var imageBytes = new byte[imageStream.Length];
            await imageStream.ReadAsync(imageBytes, 0, (int)imageStream.Length);
            var base64Image = Convert.ToBase64String(imageBytes);
            
            // Determine image format
            var imageFormat = "image/jpeg";
            if (imageBytes.Length > 4)
            {
                if (imageBytes[0] == 0x89 && imageBytes[1] == 0x50 && imageBytes[2] == 0x4E && imageBytes[3] == 0x47)
                    imageFormat = "image/png";
                else if (imageBytes[0] == 0x52 && imageBytes[1] == 0x49 && imageBytes[2] == 0x46 && imageBytes[3] == 0x46)
                    imageFormat = "image/webp";
            }

            // Build metadata context
            var metadataContext = new StringBuilder();
            if (metadata.Latitude.HasValue && metadata.Longitude.HasValue)
            {
                metadataContext.AppendLine($"GPS Coördinaten: {metadata.Latitude}, {metadata.Longitude}");
            }
            if (!string.IsNullOrEmpty(metadata.CameraMake))
            {
                metadataContext.AppendLine($"Camera: {metadata.CameraMake} {metadata.CameraModel}");
            }
            if (metadata.DateTaken.HasValue)
            {
                metadataContext.AppendLine($"Datum: {metadata.DateTaken:yyyy-MM-dd HH:mm:ss}");
            }

            var prompt = $@"Analyseer deze foto en bepaal waar ter wereld deze is genomen. 

{(metadataContext.Length > 0 ? $"Metadata van de foto:\n{metadataContext}\n" : "")}

Geef een gedetailleerde analyse met:
1. De geschatte locatie (stad, land)
2. GPS coördinaten (latitude, longitude) als je deze kunt bepalen
3. Een uitleg waarom je denkt dat het daar is (gebouwen, landschap, tekens, vegetatie, architectuur, etc.)
4. Een lijst van visuele aanwijzingen die je hebt gevonden
5. Een betrouwbaarheidsscore (0-100%)

Antwoord in JSON formaat:
{{
  ""locationName"": ""Naam van de locatie"",
  ""latitude"": 52.1234,
  ""longitude"": 4.5678,
  ""country"": ""Nederland"",
  ""city"": ""Amsterdam"",
  ""explanation"": ""Gedetailleerde uitleg..."",
  ""confidence"": 85.5,
  ""clues"": [""aanwijzing 1"", ""aanwijzing 2""]
}}";

            var requestBody = new
            {
                model = _configuration["ClaudeApi:Model"] ?? "claude-3-5-sonnet-20241022",
                max_tokens = 4096,
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = new object[]
                        {
                            new
                            {
                                type = "image",
                                source = new
                                {
                                    type = "base64",
                                    media_type = imageFormat,
                                    data = base64Image
                                }
                            },
                            new
                            {
                                type = "text",
                                text = prompt
                            }
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var apiUrl = _configuration["ClaudeApi:ApiUrl"] ?? "https://api.anthropic.com/v1/messages";
            
            var request = new HttpRequestMessage(HttpMethod.Post, apiUrl)
            {
                Content = content
            };
            request.Headers.Add("x-api-key", apiKey);
            
            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Claude API error: {StatusCode} - {Content}", response.StatusCode, responseContent);
                return new LocationResult
                {
                    ErrorMessage = $"API fout: {response.StatusCode}. Controleer je API key en configuratie."
                };
            }

            // Parse response
            var responseJson = JsonDocument.Parse(responseContent);
            var textContent = responseJson.RootElement
                .GetProperty("content")
                .EnumerateArray()
                .FirstOrDefault(c => c.GetProperty("type").GetString() == "text")
                .GetProperty("text")
                .GetString();

            if (string.IsNullOrEmpty(textContent))
            {
                return new LocationResult
                {
                    ErrorMessage = "Geen tekst respons ontvangen van Claude API"
                };
            }

            // Try to extract JSON from the response
            var jsonStart = textContent.IndexOf('{');
            var jsonEnd = textContent.LastIndexOf('}');
            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var jsonText = textContent.Substring(jsonStart, jsonEnd - jsonStart + 1);
                var result = JsonSerializer.Deserialize<LocationResult>(jsonText, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                if (result != null)
                {
                    return result;
                }
            }

            // Fallback: return explanation as text
            return new LocationResult
            {
                Explanation = textContent,
                Confidence = 50.0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing location with Claude");
            return new LocationResult
            {
                ErrorMessage = $"Fout bij analyse: {ex.Message}"
            };
        }
    }
}
