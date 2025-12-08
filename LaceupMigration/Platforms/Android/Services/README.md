# Camera Barcode Scanner Service

## Overview

This service provides camera-based barcode scanning using **ZXing.Net.Maui**, a cross-platform barcode scanning library for .NET MAUI.

## Implementation

### Service Interface
- `ICameraBarcodeScannerService` - Defined in `LaceupMigration.Business/Interfaces/`

### Android Implementation
- `CameraBarcodeScannerService.Android.cs` - Android implementation using ZXing.Net.Maui
- Uses ZXing's `CameraBarcodeReaderView` for camera preview and barcode detection
- Handles permissions automatically
- Displays modal scanner page

### iOS Implementation
- `CameraBarcodeScannerService.iOS.cs` - Placeholder (TODO: implement with ZXing.Net.Maui)

## NuGet Packages Required

- `ZXing.Net.Maui` (0.4.0)
- `ZXing.Net.Maui.Controls` (0.4.0)

## Registration

The service is registered in `MauiProgram.cs`:
- ZXing is initialized with `.UseBarcodeReader()`
- Android: `Platforms.Android.Services.CameraBarcodeScannerService`
- iOS: `Platforms.iOS.Services.CameraBarcodeScannerService` (placeholder)

## Usage

```csharp
// Inject in ViewModel constructor
public MyViewModel(ICameraBarcodeScannerService cameraBarcodeScanner)
{
    _cameraBarcodeScanner = cameraBarcodeScanner;
}

// Use in command
[RelayCommand]
private async Task ScanAsync()
{
    var barcode = await _cameraBarcodeScanner.ScanBarcodeAsync();
    if (!string.IsNullOrEmpty(barcode))
    {
        // Use barcode value
    }
}
```

## Permissions

Camera permission is already declared in `AndroidManifest.xml`. The service automatically:
1. Checks if permission is granted
2. Requests permission if needed
3. Returns null if permission is denied

## Features

✅ Cross-platform barcode scanning (ZXing.Net.Maui)
✅ Supports all barcode formats
✅ Automatic permission handling
✅ Modal UI with cancel option
✅ Clean service interface
✅ Ready for dependency injection
