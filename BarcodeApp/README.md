# Secure Barcode Generator & Printer

A modern, secure desktop application for generating and printing barcodes, built with WPF, Material Design, and the MVVM pattern.

## Features

- **Multiple Barcode Formats**: Supports Code 128, EAN-13, Code 39, QR Code, and Data Matrix
- **Live Preview**: Real-time barcode preview with customizable size and appearance
- **Batch Processing**: Load CSV/text files for bulk barcode generation
- **Secure Licensing**: Server-based license validation with DPAPI-encrypted local storage
- **Offline Grace Period**: 7-day grace period for offline operation
- **Material Design UI**: Modern, responsive interface with light/dark theme support
- **Printing**: Support for single and batch printing with customizable label layouts

## Prerequisites

- .NET 8.0 SDK or later
- Windows 10/11
- Visual Studio 2022 or VS Code (recommended)

## Building the Application

1. Clone or download the repository
2. Open a terminal in the `BarcodeApp` directory
3. Restore NuGet packages:
   ```bash
   dotnet restore
   ```
4. Build the application:
   ```bash
   dotnet build
   ```
5. Run the application:
   ```bash
   dotnet run
   ```

## Configuration

### License Server Endpoint

The application expects a license validation server at the following endpoint:

**URL**: `https://your-server.com/api/validateKey`

**Method**: POST

**Request Body** (JSON):
```json
{
  "Key": "string",
  "MachineId": "string",
  "AppVersion": "string"
}
```

**Response Body** (JSON):
```json
{
  "IsValid": true,
  "ExpiryDate": "2024-12-31T23:59:59Z",
  "StatusMessage": "License activated successfully"
}
```

### Changing the License Server URL

To change the license server endpoint, edit the `LicenseServerUrl` constant in `Services/LicensingService.cs`:

```csharp
private const string LicenseServerUrl = "https://v0-license-server-setup.vercel.app/api/validateKey";
```

## Architecture Overview

The application follows the **MVVM (Model-View-ViewModel)** pattern:

### Models
- `BarcodeItem`: Represents a barcode with data, symbology, and quantity
- `BarcodeSymbology`: Enumeration of supported barcode types
- `LabelLayout`: Defines paper/label size configurations
- `LicenseToken`: Encrypted license information

### ViewModels
- `MainViewModel`: Main application logic, barcode generation, printing coordination
- `LicenseViewModel`: License activation and validation UI logic
- `BaseViewModel`: Base class with INotifyPropertyChanged implementation

### Views
- `MainWindow.xaml`: Main application window with barcode input, preview, and printing controls
- `LicenseDialog.xaml`: License activation dialog (non-dismissible when required)

### Services
- `BarcodeService`: Generates barcode images using ZXing.Net
- `BarcodeValidation`: Validates input data against symbology requirements
- `PrintingService`: Handles printer enumeration and print job execution
- `LicensingService`: Manages license validation, DPAPI encryption, and offline grace period
- `ThemeService`: Manages light/dark theme switching with persistence
- `MachineIdProvider`: Generates SHA-256 hash of machine identifier

### Key Components

1. **Barcode Generation**: Uses ZXing.Net library to generate barcode images in various formats
2. **License Security**: 
   - Machine ID based on primary MAC address (SHA-256 hashed)
   - DPAPI encryption for local token storage
   - HTTPS server validation with exponential backoff retry logic
   - 7-day offline grace period
3. **Theme Management**: Material Design theme switching with Properties.Settings persistence
4. **Printing**: System.Drawing.Printing integration for Windows printer support

## Project Structure

```
BarcodeApp/
├── Models/              # Data models
├── ViewModels/          # MVVM ViewModels
├── Views/               # XAML views
├── Services/            # Business logic services
├── Converters/          # WPF value converters
├── Resources/           # Resource dictionaries (Material Design)
├── Properties/          # Application settings
└── App.xaml.cs          # Application startup logic
```

## License Validation Flow

1. On startup, the application checks for a valid local license token
2. If no token exists or token is invalid/expired:
   - Non-dismissible license activation dialog is shown
   - User must enter a valid license key
3. License key and machine ID are sent to the server via HTTPS POST
4. On successful validation:
   - License token is encrypted using DPAPI and saved locally
   - Application proceeds to main window
5. Offline operation:
   - If server check fails but a valid token exists, 7-day grace period applies
   - Application functions normally during grace period

## Security Features

- **DPAPI Encryption**: License tokens are encrypted using Windows Data Protection API (CurrentUser scope)
- **Machine Binding**: License is bound to machine ID (SHA-256 hash of MAC address)
- **HTTPS Communication**: All license validation requests use secure HTTPS
- **Exponential Backoff**: Network retry logic prevents server overload

## Usage

1. **Generate Single Barcode**:
   - Enter data in the input field
   - Select symbology type
   - Adjust module width and height using sliders
   - Toggle human-readable text visibility
   - Click "Print Now" (requires valid license)

2. **Batch Processing**:
   - Click "Load Batch File"
   - Select a CSV or text file (one barcode per line)
   - Batch items appear in the list
   - Click "Print Now" to print all items

3. **Theme Switching**:
   - Use the "Dark Theme" toggle in the top-right corner
   - Preference is saved and restored on next launch

## Troubleshooting

- **Build Errors**: Ensure all NuGet packages are restored (`dotnet restore`)
- **License Issues**: Verify the license server endpoint is accessible and returns the expected JSON format
- **Printing Errors**: Check that a printer is installed and selected in the printer dropdown
- **Barcode Not Displaying**: Verify input data matches the selected symbology requirements (see validation messages)

## Dependencies

- MaterialDesignThemes (5.3.0)
- MaterialDesignColors (5.3.0)
- ZXing.Net (0.16.11)
- System.Drawing.Common (8.0.0)

## License

This application requires a valid license key for printing functionality. Preview and generation features are available without activation, but printing is restricted until a valid license is activated.

