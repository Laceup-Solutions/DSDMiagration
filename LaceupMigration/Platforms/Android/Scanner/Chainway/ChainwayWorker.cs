using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Android.App;
using Android.Media;




using Com.Barcode;
using Com.Rscja.Deviceapi;

namespace LaceupMigration
{
    public class ChainwayWorker1D
    {
        Activity context;

        Barcode1D barcode1D;
        BarcodeUtility barcodeUtility;

        Thread thScan = null;
        AutoResetEvent thEvent = new AutoResetEvent(false);

        bool isClose = false;

        public event EventHandler HandleData;

        public bool IsWorking { get; set; }

        public ChainwayWorker1D(Activity context)
        {
            this.context = context;

            try
            {
                barcode1D = Barcode1D.Instance;
                barcodeUtility = BarcodeUtility.Instance;
            }
            catch
            {

            }
        }

        public void Initialize()
        {
            if (barcode1D.Open())
            {
                barcodeUtility.Open(context, BarcodeUtility.ModuleType.Barcode1d);

                barcodeUtility.EnablePlaySuccessSound(context, false);
                barcodeUtility.EnablePlayFailureSound(context, false);

                barcode1D.SetTimeOut(1);

                StartScanThread();
            }
        }

        public void Pause()
        {
            if (barcode1D != null)
                barcode1D.Close();

            if (barcodeUtility != null)
                barcodeUtility.Close(context, BarcodeUtility.ModuleType.Barcode1d);
        }

        public void Stop()
        {
            StopScanThread();
        }

        void StartScanThread()
        {
            if (thScan == null && barcode1D != null)
            {
                isClose = false;
                thScan = new Thread(BarcodeScan);
                thScan.IsBackground = true;
                thScan.Start();
            }
        }

        void StopScanThread()
        {
            if (thScan != null)
            {
                isClose = true;
                thEvent.Set();
                Thread.Sleep(100);
                thScan = null;
            }
        }

        void BarcodeScan()
        {
            while (!isClose)
            {
                thEvent.WaitOne(-1, false);

                if (isClose)
                    return;

                if (!IsWorking)
                {
                    string strData = barcode1D.Scan();

                    if (!string.IsNullOrEmpty(strData))
                    {
                        Sound();
                        HandleData?.Invoke(strData, null);
                    }
                }
            }
        }

        public void Scan()
        {
            thEvent.Set();
        }

        void Sound()
        {
        }
    }
}