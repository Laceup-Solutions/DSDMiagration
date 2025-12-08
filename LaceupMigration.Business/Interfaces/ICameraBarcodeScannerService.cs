namespace LaceupMigration.Business.Interfaces;

/// <summary>
/// Service interface for camera-based barcode scanning
/// </summary>
public interface ICameraBarcodeScannerService
{
    /// <summary>
    /// Starts the camera barcode scanner and displays it in a modal overlay
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to cancel scanning</param>
    /// <returns>The scanned barcode value, or null if cancelled or failed</returns>
    Task<string?> ScanBarcodeAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks if camera permission is granted
    /// </summary>
    /// <returns>True if permission is granted, false otherwise</returns>
    Task<bool> CheckCameraPermissionAsync();
    
    /// <summary>
    /// Requests camera permission from the user
    /// </summary>
    /// <returns>True if permission was granted, false otherwise</returns>
    Task<bool> RequestCameraPermissionAsync();
}

