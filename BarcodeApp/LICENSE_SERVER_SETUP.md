# License Server Setup Guide

This guide explains how to set up a license validation server for the Barcode App.

## API Specification

### Endpoint
- **URL**: `POST /api/validateKey`
- **Content-Type**: `application/json`
- **Timeout**: 30 seconds

### Request Format

```json
{
  "Key": "user-provided-license-key",
  "MachineId": "sha256-hash-of-machine-identifier",
  "AppVersion": "1.0"
}
```

### Response Format

**Success (200 OK):**
```json
{
  "IsValid": true,
  "ExpiryDate": "2024-12-31T23:59:59Z",
  "StatusMessage": "License activated successfully"
}
```

**Invalid License (200 OK):**
```json
{
  "IsValid": false,
  "ExpiryDate": "0001-01-01T00:00:00Z",
  "StatusMessage": "Invalid license key"
}
```

**Error (500 Internal Server Error):**
The server should return a standard HTTP error response.

## Example Implementations

### Option 1: Node.js/Express Server

```javascript
const express = require('express');
const app = express();

app.use(express.json());

// In-memory license store (use a database in production)
const licenses = {
  'TEST-LICENSE-001': {
    isValid: true,
    expiryDate: new Date('2025-12-31'),
    maxMachines: 1,
    machines: []
  }
};

app.post('/api/validateKey', (req, res) => {
  const { Key, MachineId, AppVersion } = req.body;
  
  // Validate input
  if (!Key || !MachineId) {
    return res.status(400).json({
      IsValid: false,
      ExpiryDate: '0001-01-01T00:00:00Z',
      StatusMessage: 'Missing required fields'
    });
  }
  
  const license = licenses[Key];
  
  if (!license) {
    return res.json({
      IsValid: false,
      ExpiryDate: '0001-01-01T00:00:00Z',
      StatusMessage: 'Invalid license key'
    });
  }
  
  // Check if expired
  if (new Date() > license.expiryDate) {
    return res.json({
      IsValid: false,
      ExpiryDate: license.expiryDate.toISOString(),
      StatusMessage: 'License has expired'
    });
  }
  
  // Check machine binding
  if (!license.machines.includes(MachineId)) {
    if (license.machines.length >= license.maxMachines) {
      return res.json({
        IsValid: false,
        ExpiryDate: license.expiryDate.toISOString(),
        StatusMessage: 'License already activated on maximum number of machines'
      });
    }
    license.machines.push(MachineId);
  }
  
  res.json({
    IsValid: true,
    ExpiryDate: license.expiryDate.toISOString(),
    StatusMessage: 'License activated successfully'
  });
});

const PORT = process.env.PORT || 3000;
app.listen(PORT, () => {
  console.log(`License server running on port ${PORT}`);
});
```

### Option 2: Python/Flask Server

```python
from flask import Flask, request, jsonify
from datetime import datetime, timedelta
from typing import Dict, List

app = Flask(__name__)

# In-memory license store (use a database in production)
licenses: Dict[str, dict] = {
    'TEST-LICENSE-001': {
        'is_valid': True,
        'expiry_date': datetime(2025, 12, 31),
        'max_machines': 1,
        'machines': []
    }
}

@app.route('/api/validateKey', methods=['POST'])
def validate_key():
    data = request.json
    
    if not data or 'Key' not in data or 'MachineId' not in data:
        return jsonify({
            'IsValid': False,
            'ExpiryDate': '0001-01-01T00:00:00Z',
            'StatusMessage': 'Missing required fields'
        }), 400
    
    key = data['Key']
    machine_id = data['MachineId']
    
    if key not in licenses:
        return jsonify({
            'IsValid': False,
            'ExpiryDate': '0001-01-01T00:00:00Z',
            'StatusMessage': 'Invalid license key'
        })
    
    license_info = licenses[key]
    
    # Check if expired
    if datetime.utcnow() > license_info['expiry_date']:
        return jsonify({
            'IsValid': False,
            'ExpiryDate': license_info['expiry_date'].isoformat() + 'Z',
            'StatusMessage': 'License has expired'
        })
    
    # Check machine binding
    if machine_id not in license_info['machines']:
        if len(license_info['machines']) >= license_info['max_machines']:
            return jsonify({
                'IsValid': False,
                'ExpiryDate': license_info['expiry_date'].isoformat() + 'Z',
                'StatusMessage': 'License already activated on maximum number of machines'
            })
        license_info['machines'].append(machine_id)
    
    return jsonify({
        'IsValid': True,
        'ExpiryDate': license_info['expiry_date'].isoformat() + 'Z',
        'StatusMessage': 'License activated successfully'
    })

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=3000, ssl_context='adhoc')  # Use proper SSL in production
```

### Option 3: C# ASP.NET Core Server

```csharp
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LicenseServer.Controllers
{
    [ApiController]
    [Route("api")]
    public class LicenseController : ControllerBase
    {
        // In-memory license store (use a database in production)
        private static readonly Dictionary<string, LicenseInfo> Licenses = new()
        {
            ["TEST-LICENSE-001"] = new LicenseInfo
            {
                IsValid = true,
                ExpiryDate = new DateTime(2025, 12, 31),
                MaxMachines = 1,
                Machines = new List<string>()
            }
        };

        [HttpPost("validateKey")]
        public IActionResult ValidateKey([FromBody] LicenseRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Key) || string.IsNullOrEmpty(request.MachineId))
            {
                return BadRequest(new LicenseResponse
                {
                    IsValid = false,
                    ExpiryDate = DateTime.MinValue,
                    StatusMessage = "Missing required fields"
                });
            }

            if (!Licenses.TryGetValue(request.Key, out var license))
            {
                return Ok(new LicenseResponse
                {
                    IsValid = false,
                    ExpiryDate = DateTime.MinValue,
                    StatusMessage = "Invalid license key"
                });
            }

            // Check if expired
            if (DateTime.UtcNow > license.ExpiryDate)
            {
                return Ok(new LicenseResponse
                {
                    IsValid = false,
                    ExpiryDate = license.ExpiryDate,
                    StatusMessage = "License has expired"
                });
            }

            // Check machine binding
            if (!license.Machines.Contains(request.MachineId))
            {
                if (license.Machines.Count >= license.MaxMachines)
                {
                    return Ok(new LicenseResponse
                    {
                        IsValid = false,
                        ExpiryDate = license.ExpiryDate,
                        StatusMessage = "License already activated on maximum number of machines"
                    });
                }
                license.Machines.Add(request.MachineId);
            }

            return Ok(new LicenseResponse
            {
                IsValid = true,
                ExpiryDate = license.ExpiryDate,
                StatusMessage = "License activated successfully"
            });
        }
    }

    public class LicenseRequest
    {
        public string Key { get; set; } = string.Empty;
        public string MachineId { get; set; } = string.Empty;
        public string AppVersion { get; set; } = string.Empty;
    }

    public class LicenseResponse
    {
        public bool IsValid { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string? StatusMessage { get; set; }
    }

    public class LicenseInfo
    {
        public bool IsValid { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int MaxMachines { get; set; }
        public List<string> Machines { get; set; } = new();
    }
}
```

## Database Schema (Recommended)

For production, use a proper database:

```sql
CREATE TABLE licenses (
    id INT PRIMARY KEY AUTO_INCREMENT,
    license_key VARCHAR(255) UNIQUE NOT NULL,
    expiry_date DATETIME NOT NULL,
    max_machines INT DEFAULT 1,
    is_active BOOLEAN DEFAULT TRUE,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE license_activations (
    id INT PRIMARY KEY AUTO_INCREMENT,
    license_id INT NOT NULL,
    machine_id VARCHAR(255) NOT NULL,
    activated_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    last_validated_at DATETIME,
    FOREIGN KEY (license_id) REFERENCES licenses(id),
    UNIQUE KEY unique_activation (license_id, machine_id)
);
```

## Security Considerations

1. **HTTPS Only**: Always use HTTPS in production. The client expects secure connections.

2. **Rate Limiting**: Implement rate limiting to prevent brute force attacks:
   ```javascript
   // Example with express-rate-limit
   const rateLimit = require('express-rate-limit');
   const limiter = rateLimit({
     windowMs: 15 * 60 * 1000, // 15 minutes
     max: 5 // limit each IP to 5 requests per windowMs
   });
   app.use('/api/validateKey', limiter);
   ```

3. **Input Validation**: Validate and sanitize all inputs.

4. **License Key Format**: Consider using cryptographically secure license keys (e.g., UUIDs or hashed keys).

5. **Logging**: Log all validation attempts for security auditing.

6. **CORS**: Configure CORS appropriately if serving from a different domain.

## Testing the Server

### Using cURL

```bash
curl -X POST https://your-server.com/api/validateKey \
  -H "Content-Type: application/json" \
  -d '{
    "Key": "TEST-LICENSE-001",
    "MachineId": "test-machine-id-123",
    "AppVersion": "1.0"
  }'
```

### Using PowerShell

```powershell
$body = @{
    Key = "TEST-LICENSE-001"
    MachineId = "test-machine-id-123"
    AppVersion = "1.0"
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://your-server.com/api/validateKey" `
    -Method Post `
    -ContentType "application/json" `
    -Body $body
```

## Updating the Client

After setting up your server, update the client application:

1. Open `BarcodeApp/Services/LicensingService.cs`
2. Update the `LicenseServerUrl` constant:
   ```csharp
   private const string LicenseServerUrl = "https://v0-license-server-setup.vercel.app/api/validateKey";
   ```
3. Rebuild the application

## Deployment Options

### Cloud Platforms
- **Azure**: Azure App Service, Azure Functions
- **AWS**: AWS Lambda, EC2 with API Gateway
- **Google Cloud**: Cloud Functions, App Engine
- **Heroku**: Simple Node.js/Python deployment
- **DigitalOcean**: App Platform or Droplets

### Self-Hosted
- Use a reverse proxy (nginx, Apache) with SSL certificate (Let's Encrypt)
- Deploy behind a firewall with proper security rules
- Use Docker for easy deployment

## Monitoring and Maintenance

1. **Health Checks**: Implement a health check endpoint
2. **Monitoring**: Set up monitoring (e.g., Application Insights, DataDog)
3. **Backups**: Regular database backups
4. **License Management**: Create an admin interface for managing licenses
5. **Expiry Notifications**: Set up alerts for expiring licenses

## Example Test License Keys

For development/testing, you can use these patterns:
- `TEST-LICENSE-001` - Valid until 2025-12-31
- `TEST-LICENSE-002` - Valid until 2026-12-31
- `EXPIRED-LICENSE` - Already expired
- `INVALID-KEY` - Invalid key for error testing

