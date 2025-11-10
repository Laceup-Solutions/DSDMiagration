using Android.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaceupMigration;

[assembly: Dependency(typeof(IScannerService))]
public class ScannerService : IScannerService
{
    public event EventHandler<string> OnDataScanned;
    public event EventHandler<BarcodeDecoder> OnDataScannedQR;
    protected static SocketMobileWorker socketScanner;

    protected static EMDKWorker emdkWorker;

    protected static HoneywellWorker honeywellWorker;

    protected static ChainwayWorker1D chainwayScanner1D;

    protected CipherlabScanner cipherlabScanner;

    protected bool errorScanner = false;

    public void InitScanner()
    {
        if (Config.ScannerToUse == 9)
        {
            cipherlabScanner = new CipherlabScanner();
        }
        else if (Config.ScannerToUse == 7)
        {
            honeywellWorker = new HoneywellWorker();
        }
        else if (Config.ScannerToUse == 5 && chainwayScanner1D == null)
        {
            chainwayScanner1D = new ChainwayWorker1D(Platform.CurrentActivity);
        }
        else if (Config.ScannerToUse == 2 && emdkWorker == null)
        {
            try
            {
                emdkWorker = new EMDKWorker(Android.App.Application.Context);
            }
            catch (Exception ee)
            {
                Logger.CreateLog(ee);
                errorScanner = true;
            }
        }

        if (errorScanner || (Config.ScannerToUse == 3 && socketScanner == null))
            socketScanner = new SocketMobileWorker();
    }

    public void StartScanning()
    {
        if (Config.ScannerToUse == 9 && cipherlabScanner != null)
        {
            cipherlabScanner.InitScanner();
            cipherlabScanner.HandleData += OnDecodeData;
            cipherlabScanner.HandleDataQR += OnDecodeDataQR;
        }

        if (Config.ScannerToUse == 7 && honeywellWorker != null)
        {
            honeywellWorker.OpenBarcodeReader();
            honeywellWorker.HandleData += OnDecodeData;
            honeywellWorker.HandleDataQR += OnDecodeDataQR;
        }
        else if (Config.ScannerToUse == 5 && chainwayScanner1D != null)
        {
            chainwayScanner1D.Initialize();
            chainwayScanner1D.HandleData += OnDecodeData;
        }
        else if (!errorScanner && Config.ScannerToUse == 2 && emdkWorker != null)
        {
            emdkWorker.InitScanner();
            emdkWorker.HandleData += OnDecodeData;
            emdkWorker.HandleDataQR += OnDecodeDataQR;
        }
        else if (Config.ScannerToUse == 3 && socketScanner != null)
        {
            socketScanner.DecodeData += OnDecodeData;
            socketScanner.DecodeDataQR += OnDecodeDataQR;
        }
    }

    public void StopScanning()
    {
        if (Config.ScannerToUse == 9 && cipherlabScanner != null)
        {
            cipherlabScanner.DeinitScanner();
            cipherlabScanner.HandleData -= OnDecodeData;
            cipherlabScanner.HandleDataQR -= OnDecodeDataQR;
        }
        else if (Config.ScannerToUse == 7 && honeywellWorker != null)
        {
            honeywellWorker.CloseBarcodeReader();
            honeywellWorker.HandleData -= OnDecodeData;
            honeywellWorker.HandleDataQR -= OnDecodeDataQR;
        }
        else if (Config.ScannerToUse == 5 && chainwayScanner1D != null)
        {
            chainwayScanner1D.Pause();
            chainwayScanner1D.HandleData -= OnDecodeData;
        }
        else if (Config.ScannerToUse == 2 && emdkWorker != null)
        {
            emdkWorker.DeinitScanner();
            emdkWorker.HandleData -= OnDecodeData;
            emdkWorker.HandleDataQR -= OnDecodeDataQR;
        }
        else if ((errorScanner || Config.ScannerToUse == 3) && socketScanner != null)
        {
            socketScanner.DecodeData -= OnDecodeData;
            socketScanner.DecodeDataQR -= OnDecodeDataQR;
        }
    }

    public void ReleaseScanner()
    {
        if (Config.ScannerToUse == 9 && cipherlabScanner != null)
            cipherlabScanner.IsWorking = false;
        else if (Config.ScannerToUse == 7 && honeywellWorker != null)
            honeywellWorker.IsWorking = false;
        else if (Config.ScannerToUse == 5 && chainwayScanner1D != null)
            chainwayScanner1D.IsWorking = false;
        else if (Config.ScannerToUse == 3 && socketScanner != null)
            socketScanner.Isworking = false;
        else if (Config.ScannerToUse == 2 && emdkWorker != null) emdkWorker.IsWorking = false;
    }

    public void SetScannerWorking()
    {
        if (Config.ScannerToUse == 9 && cipherlabScanner != null)
            cipherlabScanner.IsWorking = true;
        else if (Config.ScannerToUse == 7 && honeywellWorker != null)
            honeywellWorker.IsWorking = true;
        else if (Config.ScannerToUse == 5 && chainwayScanner1D != null)
            chainwayScanner1D.IsWorking = true;
        else if (Config.ScannerToUse == 3 && socketScanner != null)
            socketScanner.Isworking = true;
        else if (Config.ScannerToUse == 2 && emdkWorker != null) emdkWorker.IsWorking = true;
    }

    public void OnDecodeData(object sender, EventArgs e)
    {
        var data = (string)sender; // Modify this based on how you get data
        OnDataScanned?.Invoke(this, data);
    }

    public virtual void OnDecodeDataQR(object sender, EventArgs e)
    {
        var data = (BarcodeDecoder)sender; // Modify this based on how you get data
        OnDataScannedQR?.Invoke(this, data);
    }
}