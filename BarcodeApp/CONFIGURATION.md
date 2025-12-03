# Configuration Guide

## License Server Configuration

The license validation server endpoint is configured in `Services/LicensingService.cs`:

```csharp
private const string LicenseServerUrl = "https://v0-license-server-setup.vercel.app/api/validateKey";
```

To change the endpoint, modify this constant and rebuild the application.

## Server Response Format

The license server must return a JSON response with the following structure:

```json
{
  "IsValid": true,
  "ExpiryDate": "2024-12-31T23:59:59Z",
  "StatusMessage": "Optional status message"
}
```

### Response Fields

- `IsValid` (boolean, required): Indicates if the license key is valid
- `ExpiryDate` (ISO 8601 datetime string, required): License expiration date/time
- `StatusMessage` (string, optional): Human-readable status message

### Request Format

The application sends a POST request with the following JSON body:

```json
{
  "Key": "user-provided-license-key",
  "MachineId": "sha256-hash-of-machine-identifier",
  "AppVersion": "1.0"
}
```

## Application Settings

User preferences are stored in `Properties/Settings.settings`:

- **Theme**: Light or Dark theme preference (defaults to Dark)

Settings are automatically saved when changed and restored on application startup.

## License Storage

License tokens are stored in:
```
%LocalAppData%\BarcodeApp\license.dat
```

The file is encrypted using Windows DPAPI (Data Protection API) with CurrentUser scope, ensuring that only the user who activated the license can decrypt it.

## Offline Grace Period

The application allows 7 days of offline operation after the last successful server validation. This period is configurable via the `GracePeriodDays` constant in `LicensingService.cs`.

