
using Android.Media;


using System;
using System.Threading;
using Android.App;
using Android.OS;
using Android.Views;

namespace LaceupMigration
{
    public abstract class LaceupScannerActivity : Activity, Com.Zebra.Adc.Decoder.Barcode2DWithSoft.IScanCallback
    {
        protected static SocketMobileWorker socketScanner;

        protected static EMDKWorker emdkWorker;

        protected static HoneywellWorker honeywellWorker;

        protected static ChainwayWorker1D chainwayScanner1D;

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
            else if (Config.ScannerToUse == 6)
            {
                CreateChainway2D();
            }
            else if (Config.ScannerToUse == 5 && chainwayScanner1D == null)
            {
                chainwayScanner1D = new ChainwayWorker1D(this);
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
            else if (Config.ScannerToUse == 6)
            {
                InitializeChainway2D();
                HandleDataChainway += OnDecodeData;
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
            else if (Config.ScannerToUse == 6)
            {
                PauseChainway2D();
                HandleDataChainway -= OnDecodeData;
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
            else if (Config.ScannerToUse == 6)
                IsWorking = false;
            else if (Config.ScannerToUse == 5 && chainwayScanner1D != null)
                chainwayScanner1D.IsWorking = false;
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
            else if (Config.ScannerToUse == 6)
                IsWorking = true;
            else if (Config.ScannerToUse == 5 && chainwayScanner1D != null)
                chainwayScanner1D.IsWorking = true;
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
                else
                if (Config.ScannerToUse == 6)
                {
                    if (e.KeyCode.GetHashCode() == 139 || e.KeyCode.GetHashCode() == 280)
                    {
                        if (e.RepeatCount == 0)
                        {
                            if (isOpen)
                            {
                                barcode.Scan();
                            }
                        }
                        return true;
                    }
                }
                else if (Config.ScannerToUse == 5)
                {
                    if (e.KeyCode.GetHashCode() == 139 || e.KeyCode.GetHashCode() == 280)
                    {
                        if (e.RepeatCount == 0)
                        {
                            chainwayScanner1D.Scan();
                            return true;
                        }
                    }
                }
            }

            return base.OnKeyDown(keyCode, e);
        }

        #region Chainway2D

        Com.Zebra.Adc.Decoder.Barcode2DWithSoft barcode;
        bool isOpen = false;
        SoundPool soundPool;
        int soundPoolId;

        bool IsWorking = false;

        public event EventHandler HandleDataChainway;

        void CreateChainway2D()
        {
            try
            {
                barcode = Com.Zebra.Adc.Decoder.Barcode2DWithSoft.Instance;
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
            }
        }

        void InitializeChainway2D()
        {
            if (barcode != null && !isOpen)
            {
                if (barcode.Open(this))
                {
                    barcode.SetParameter(402, 1);

                    isOpen = true;
                    barcode.SetScanCallback(this);
                }
            }
        }

        void PauseChainway2D()
        {
            if (barcode != null)
                barcode.StopScan();
        }

        void SoundChainway2D()
        {
            soundPool.Play(soundPoolId, 1, 1, 0, 0, 1);
        }

        public void OnScanComplete(int symbology, int length, byte[] data)
        {
            string strData = "";
            if (length < 1)
            {
                if (length == -1)
                {
                    strData = "Scan canceled\r\n";
                }
                else if (length == 0)
                {
                    strData = "Scan timeout\r\n";
                }
                else
                {
                    strData = "Scan failure\r\n";
                }
            }
            else
            {
                if (!IsWorking)
                {
                    strData = System.Text.ASCIIEncoding.ASCII.GetString(data, 0, length);

                    if (!string.IsNullOrEmpty(strData))
                    {
                        SoundChainway2D();
                        HandleDataChainway?.Invoke(strData, null);
                    }
                }
            }

            barcode.StopScan();
        }

        #endregion
    }
}
