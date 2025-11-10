namespace LaceupMigration;

public interface IScannerService
{
    void InitScanner();
    void StartScanning();
    void StopScanning();
    void ReleaseScanner();
    void SetScannerWorking();

    event EventHandler<string> OnDataScanned;
    event EventHandler<BarcodeDecoder> OnDataScannedQR;
    
}

public class CurrentScanner
{
    public static IScannerService? scanner { get; set; }
}