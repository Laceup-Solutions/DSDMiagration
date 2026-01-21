using Android.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LaceupMigration;
using Microsoft.Maui.ApplicationModel;

[assembly: Dependency(typeof(IScannerService))]

namespace LaceupMigration;

public class ScannerService : IScannerService
{
    public event EventHandler<string> OnDataScanned;
    public event EventHandler<BarcodeDecoder> OnDataScannedQR;
    protected static SocketMobileWorker socketScanner;

    protected static EMDKWorker emdkWorker;

    protected static HoneywellWorker honeywellWorker;
    
    protected CipherlabScanner cipherlabScanner;
    private EventHandler _cipherlabQRHandler; // Store handler for proper unsubscribe

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
            // CipherlabScanner passes string for QR codes, create wrapper handler
            _cipherlabQRHandler = (sender, e) => 
            {
                if (sender is string qrData)
                {
                    var decoder = BarcodeDecoder.CreateDecoder(qrData);
                    // Dispatch to main thread to avoid threading issues with UI updates
                    var capturedDecoder = decoder; // Capture for closure
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        OnDataScannedQR?.Invoke(this, capturedDecoder);
                    });
                }
            };
            cipherlabScanner.HandleDataQR += _cipherlabQRHandler;
        }

        if (Config.ScannerToUse == 7 && honeywellWorker != null)
        {
            honeywellWorker.OpenBarcodeReader();
            honeywellWorker.HandleData += OnDecodeData;
            honeywellWorker.HandleDataQR += OnDecodeDataQR;
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
            if (_cipherlabQRHandler != null)
            {
                cipherlabScanner.HandleDataQR -= _cipherlabQRHandler;
                _cipherlabQRHandler = null;
            }
        }
        else if (Config.ScannerToUse == 7 && honeywellWorker != null)
        {
            honeywellWorker.CloseBarcodeReader();
            honeywellWorker.HandleData -= OnDecodeData;
            honeywellWorker.HandleDataQR -= OnDecodeDataQR;
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
        else if (Config.ScannerToUse == 3 && socketScanner != null)
            socketScanner.Isworking = true;
        else if (Config.ScannerToUse == 2 && emdkWorker != null) emdkWorker.IsWorking = true;
    }

    public void OnDecodeData(object sender, EventArgs e)
    {
        // CipherlabScanner passes string directly as sender
        // Other scanners may pass string or use EventArgs
        string data = null;
        if (sender is string str)
        {
            data = str;
        }
        else if (e != null && e.GetType().GetProperty("Data") != null)
        {
            data = e.GetType().GetProperty("Data").GetValue(e)?.ToString();
        }
        
        if (!string.IsNullOrEmpty(data))
        {
            // Dispatch to main thread to avoid threading issues with UI updates
            var capturedData = data; // Capture for closure
            MainThread.BeginInvokeOnMainThread(() =>
            {
                OnDataScanned?.Invoke(this, capturedData);
            });
        }
    }

    public virtual void OnDecodeDataQR(object sender, EventArgs e)
    {
        // CipherlabScanner passes string directly as sender, need to create decoder
        // Other scanners pass BarcodeDecoder directly
        BarcodeDecoder decoder = null;
        if (sender is string str)
        {
            decoder = BarcodeDecoder.CreateDecoder(str);
        }
        else if (sender is BarcodeDecoder bd)
        {
            decoder = bd;
        }
        
        if (decoder != null)
        {
            // Dispatch to main thread to avoid threading issues with UI updates
            var capturedDecoder = decoder; // Capture for closure
            MainThread.BeginInvokeOnMainThread(() =>
            {
                OnDataScannedQR?.Invoke(this, capturedDecoder);
            });
        }
    }
}