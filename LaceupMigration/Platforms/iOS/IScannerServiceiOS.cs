namespace LaceupMigration;

[assembly: Dependency(typeof(IScannerService))]
public class ScannerService : IScannerService
{
    public event EventHandler<string> OnDataScanned;
    public event EventHandler<BarcodeDecoder> OnDataScannedQR;

    protected bool errorScanner = false;

    public void InitScanner()
    {
    }

    public void StartScanning()
    {
    }

    public void StopScanning()
    {
    }

    public void ReleaseScanner()
    {
    }

    public void OnDecodeData(object sender, EventArgs e)
    {
    }

    public void SetScannerWorking()
    {
    }
}