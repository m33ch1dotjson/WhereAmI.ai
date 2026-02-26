# Architectuur & Data Opslag

## Huidige Implementatie (Stateless)

### Hoe foto's worden verwerkt:

1. **Upload**: Gebruiker upload foto via web interface
2. **In-Memory Processing**: 
   - Foto wordt geladen in een `MemoryStream` (tijdelijk in RAM)
   - EXIF metadata wordt geëxtraheerd
   - Foto wordt naar Claude API gestuurd voor analyse
3. **Resultaat**: JSON response met locatie informatie
4. **Cleanup**: Na de response wordt de `MemoryStream` automatisch vrijgegeven (garbage collected)

### Geen Persistente Opslag:
- ❌ Foto's worden **niet** opgeslagen op de server
- ❌ Resultaten worden **niet** opgeslagen
- ❌ Geen database nodig
- ✅ Elke request is volledig onafhankelijk
- ✅ Privacy-vriendelijk (geen data blijft achter)

## Wanneer Database Toevoegen?

### Scenario 1: Geschiedenis Functionaliteit
Als gebruikers hun eerdere analyses willen bekijken:
- **Database nodig**: ✅ Ja
- **Foto opslag**: Optioneel (kan alleen metadata opslaan)
- **Voordelen**: Gebruikers kunnen terugkijken, statistieken
- **Nadelen**: Privacy concerns, storage kosten

### Scenario 2: Caching
Om API kosten te besparen door resultaten te cachen:
- **Database nodig**: ✅ Ja (of in-memory cache zoals Redis)
- **Foto opslag**: Nee (alleen hash/fingerprint)
- **Voordelen**: Sneller, goedkoper
- **Nadelen**: Extra complexiteit

### Scenario 3: Gebruikersaccounts
Als je gebruikers wilt laten inloggen:
- **Database nodig**: ✅ Ja
- **Foto opslag**: Optioneel
- **Voordelen**: Persoonlijke geschiedenis, statistieken
- **Nadelen**: Authenticatie nodig, meer complexiteit

## Aanbevelingen

### Voor MVP/Basisversie:
✅ **Huidige implementatie is prima** - geen database nodig
- Simpel en snel
- Privacy-vriendelijk
- Geen setup vereist
- Werkt direct out-of-the-box

### Voor Productie met Geschiedenis:
✅ **Database toevoegen** met:
- Entity Framework Core
- SQLite (lokaal) of SQL Server (productie)
- Alleen resultaten opslaan (niet de foto's zelf)
- Optioneel: foto thumbnails voor preview

### Voor Schaalbaarheid:
✅ **Hybrid approach**:
- In-memory cache voor recente resultaten
- Database voor geschiedenis
- Optioneel: Blob storage (Azure Blob, AWS S3) voor foto's als nodig

## Data Flow Diagram

```
[Gebruiker] 
    ↓
[Upload Foto] → [MemoryStream] → [EXIF Extractie]
    ↓
[Claude API] → [AI Analyse]
    ↓
[JSON Response] → [Frontend Display]
    ↓
[MemoryStream Garbage Collected] ← Geen opslag!
```

## Privacy & Security

### Huidige Implementatie:
- ✅ Foto's worden niet opgeslagen
- ✅ Geen persistent data
- ✅ Elke request is stateless
- ✅ Privacy-vriendelijk

### Met Database:
- ⚠️ Foto's kunnen worden opgeslagen (optioneel)
- ⚠️ Resultaten worden opgeslagen
- ⚠️ GDPR compliance nodig
- ⚠️ Data retention policies nodig
