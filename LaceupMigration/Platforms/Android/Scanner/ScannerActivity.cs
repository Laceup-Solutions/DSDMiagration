
using Android.Media;


using System;
using System.Threading;
using Android.App;
using Android.OS;
using Android.Views;

namespace LaceupMigration
{
    public abstract class LaceupScannerActivity : Activity
    {
        protected static SocketMobileWorker socketScanner;

        protected static EMDKWorker emdkWorker;

        protected static HoneywellWorker honeywellWorker;
        
        protected CipherlabScanner cipherlabScanner;

        protected bool errorScanner = false;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (Config.ScannerToUse == 9)
            {
                cipherlabScanner = new CipherlabScanner(this);
            }
            else if (Config.ScannerToUse == 7)
            {
                honeywellWorker = new HoneywellWorker();
            }
            else if (Config.ScannerToUse == 2 && emdkWorker == null)
            {
                try
                {
                    emdkWorker = new EMDKWorker(Application.ApplicationContext);
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

        public abstract void OnDecodeData(object sender, EventArgs e);

        public virtual void OnDecodeDataQR(object sender, EventArgs e)
        {

        }

        protected override void OnStart()
        {
            base.OnStart();

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

        protected override void OnPause()
        {
            base.OnPause();

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

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        public void ReleaseScanner()
        {
            if (Config.ScannerToUse == 9 && cipherlabScanner != null)
                cipherlabScanner.IsWorking = false;
            else if (Config.ScannerToUse == 7 && honeywellWorker != null)
                honeywellWorker.IsWorking = false;
            else if (Config.ScannerToUse == 3 && socketScanner != null)
                socketScanner.Isworking = false;
            else if (Config.ScannerToUse == 2 && emdkWorker != null)
                emdkWorker.IsWorking = false;
        }

        public void SetScannerWorking()
        {
            if (Config.ScannerToUse == 9 && cipherlabScanner != null)
                cipherlabScanner.IsWorking = true;
            else if (Config.ScannerToUse == 7 && honeywellWorker != null)
                honeywellWorker.IsWorking = true;
            else if (Config.ScannerToUse == 3 && socketScanner != null)
                socketScanner.Isworking = true;
            else if (Config.ScannerToUse == 2 && emdkWorker != null)
                emdkWorker.IsWorking = true;
        }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            if (e != null)
            {
                if (Config.ScannerToUse == 9)
                {
                    if (keyCode == (Keycode.D | Keycode.TvContentsMenu))
                    {
                        if (e.RepeatCount == 0)
                            cipherlabScanner.Triger();
                        return true;
                    }
                }
            }

            return base.OnKeyDown(keyCode, e);
        }
    }
}
