# GeoSpy.ai

Een webapplicatie voor het bepalen van de locatie van foto's met behulp van AI (Claude Visual) en EXIF metadata.

## Features

- 📸 Foto upload met drag & drop
- 📍 EXIF metadata extractie (GPS coördinaten, camera info, etc.)
- 🤖 AI-gebaseerde locatie analyse via Claude Visual API
- 🗺️ Kaartweergave van gevonden locaties
- ⚡ Moderne, responsive UI

## Vereisten

- .NET 8.0 SDK
- Claude API key van Anthropic

## Installatie

1. Clone of download dit project
2. Open een terminal in de project directory
3. Restore dependencies:
   ```bash
   dotnet restore
   ```

4. Configureer je Claude API key in `appsettings.json`:
   ```json
   "ClaudeApi": {
     "ApiKey": "jouw-api-key-hier"
   }
   ```

## Gebruik

1. Start de applicatie:
   ```bash
   dotnet run
   ```

2. Open je browser en ga naar `https://localhost:5001` (of de poort die wordt getoond)

3. Upload een foto via drag & drop of klik op de upload area

4. Wacht op de AI analyse (dit kan enkele seconden duren)

5. Bekijk het resultaat met de geschatte locatie, uitleg en kaart link

## Project Structuur

```
GeoSpy.ai/
├── Controllers/          # MVC controllers
│   ├── HomeController.cs
│   └── UploadController.cs
├── Models/              # Data models
│   ├── LocationResult.cs
│   ├── PhotoMetadata.cs
│   └── UploadViewModel.cs
├── Services/            # Business logic
│   ├── ClaudeService.cs    # Claude API integratie
│   └── ExifService.cs      # EXIF metadata extractie
├── Views/               # Razor views
│   ├── Home/
│   │   └── Index.cshtml
│   └── Shared/
│       └── _Layout.cshtml
├── wwwroot/            # Static files
│   └── css/
│       └── site.css
├── appsettings.json    # Configuratie
└── Program.cs          # Application entry point
```

## Hoe het werkt

1. **Foto Upload**: Gebruiker upload een foto via de web interface
2. **EXIF Extractie**: De `ExifService` extraheert metadata zoals GPS coördinaten, camera info, etc.
3. **AI Analyse**: De `ClaudeService` stuurt de foto + metadata naar Claude Visual API
4. **Resultaat**: De AI analyseert visuele aanwijzingen (gebouwen, landschap, tekens, etc.) en geeft een geschatte locatie terug
5. **Weergave**: Het resultaat wordt getoond met coördinaten, uitleg en een link naar de kaart

## Configuratie

### Upload Instellingen

In `appsettings.json` kun je aanpassen:
- `MaxFileSize`: Maximum bestandsgrootte in bytes (standaard: 10MB)
- `AllowedExtensions`: Toegestane bestandstypen

### Claude API

- Model: Standaard `claude-3-5-sonnet-20241022`
- API URL: `https://api.anthropic.com/v1/messages`

## Technologieën

- **ASP.NET Core MVC 8.0**: Web framework
- **MetadataExtractor**: EXIF metadata extractie
- **Claude API**: AI visuele analyse
- **Vanilla JavaScript**: Frontend interactiviteit

## Licentie

Dit project is gemaakt voor educatieve doeleinden.
