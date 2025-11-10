
using System;
using System.Collections.Generic;
using Symbol.XamarinEMDK;
using Symbol.XamarinEMDK.Barcode;
using static Symbol.XamarinEMDK.Barcode.ScanDataCollection;
using System.Threading;
using Android.App;

namespace LaceupMigration
{
    public class EMDKWorker : Java.Lang.Object, EMDKManager.IEMDKListener
    {
        EMDKManager emdkManager;
        BarcodeManager barcodeManager;

        Scanner emdkScanner;

        public event EventHandler HandleData;
        public event EventHandler HandleDataQR;
        //public event EventHandler HandleStatus;

        public bool IsWorking { get; set; }

        public EMDKWorker(Activity context)
        {
            EMDKResults results = EMDKManager.GetEMDKManager(context, this);
            if (results.StatusCode != EMDKResults.STATUS_CODE.Success)
                displayStatus("Error: EMDKManager object creation failed.");
        }

        public void OnOpened(EMDKManager p0)
        {
            emdkManager = p0;

            InitScanner();
        }

        public void InitScanner()
        {
            if (emdkManager != null)
            {
                if (barcodeManager == null)
                {
                    try
                    {
                        //Get the feature object such as BarcodeManager object for accessing the feature.
                        barcodeManager = (BarcodeManager)emdkManager.GetInstance(EMDKManager.FEATURE_TYPE.Barcode);

                        emdkScanner = barcodeManager.GetDevice(BarcodeManager.DeviceIdentifier.Default);

                        if (emdkScanner != null)
                        {

                            //Attahch the Data Event handler to get the data callbacks.
                            emdkScanner.Data -= ScannerData;
                            emdkScanner.Data += ScannerData;

                            //Attach Scanner Status Event to get the status callbacks.
                            emdkScanner.Status -= ScannerStatus;
                            emdkScanner.Status += ScannerStatus;

                            ThreadPool.QueueUserWorkItem(delegate (object state)
                            {
                                emdkScanner.Enable();
                            });
                        }
                        else
                        {
                            displayStatus("Failed to enable scanner.\n");
                        }
                    }
                    catch (ScannerException e)
                    {
                        displayStatus("Error: " + e.Message);
                    }
                    catch (Exception ex)
                    {
                        displayStatus("Error: " + ex.Message);
                    }
                }
            }

        }

        void ScannerData(object sender, Scanner.DataEventArgs e)
        {
            ScanDataCollection scanDataCollection = e.P0;

            if (scanDataCollection != null && scanDataCollection.Result == ScannerResults.Success)
            {
                IList<ScanDataCollection.ScanData> scanData = scanDataCollection.GetScanData();

                foreach (ScanDataCollection.ScanData data in scanData)
                {
                    displaydata(data.Data, data.LabelType);
                }
            }
        }

        void ScannerStatus(object sender, Scanner.StatusEventArgs e)
        {
            try
            {
                String statusStr = "";

                //EMDK: The status will be returned on multiple cases. Check the state and take the action.
                StatusData.ScannerStates state = e.P0.State;

                if (state == StatusData.ScannerStates.Idle)
                {
                    statusStr = "Scanner is idle and ready to submit read.";
                    try
                    {
                        if (emdkScanner.IsEnabled && !emdkScanner.IsReadPending)
                        {
                            try
                            {
                                //EMDK: Configure the scanner settings
                                ScannerConfig config = emdkScanner.GetConfig();
                                config.SkipOnUnsupported = ScannerConfig.SkipOnUnSupported.None;
                                config.ScanParams.DecodeLEDFeedback = true;
                                config.ReaderParams.ReaderSpecific.ImagerSpecific.PickList = ScannerConfig.PickList.Enabled;
                                config.ReaderParams.ReaderSpecific.CameraSpecific.IlluminationMode = ScannerConfig.IlluminationMode.On;
                                config.DecoderParams.Code39.Enabled = true;
                                config.DecoderParams.Code128.Enabled = true;

                                config.DecoderParams.Upca.Enabled = true;
                                config.DecoderParams.Upce0.Enabled = true;

                                config.DecoderParams.UpcEanParams.Supplemental2 = true;
                                config.DecoderParams.UpcEanParams.Supplemental5 = true;
                                config.DecoderParams.UpcEanParams.SupplementalMode = ScannerConfig.SupplementalMode.Auto;

                                config.DecoderParams.I2of5.Enabled = true;

                                emdkScanner.SetConfig(config);
                            }
                            catch
                            {

                            }

                            emdkScanner.Read();
                        }
                    }
                    catch (ScannerException e1)
                    {
                        statusStr = e1.Message;
                    }
                }
                if (state == StatusData.ScannerStates.Waiting)
                {
                    statusStr = "Waiting for Trigger Press to scan";
                }
                if (state == StatusData.ScannerStates.Scanning)
                {
                    statusStr = "Scanning in progress...";
                }
                if (state == StatusData.ScannerStates.Disabled)
                {
                    statusStr = "Scanner disabled";
                }
                if (state == StatusData.ScannerStates.Error)
                {
                    statusStr = "Error occurred during scanning";

                }
                displayStatus(statusStr);
            }
            catch (Exception ee)
            {
                Logger.CreateLog(ee);
            }
        }

        void displayStatus(string status)
        {
            //if (HandleStatus != null)
            //    HandleStatus(status, null);
        }

        void displaydata(string data, LabelType labelType)
        {
            if (!IsWorking)
            {
                if (labelType == LabelType.Qrcode ||
                    (labelType == LabelType.Code128 && data.Contains('/')) ||
                    labelType == LabelType.Gs1Databar ||
                    labelType == LabelType.Gs1DatabarExp ||
                    labelType == LabelType.Gs1DatabarLim ||
                    labelType == LabelType.Ean128
                    )
                    HandleDataQR?.Invoke(BarcodeDecoder.CreateDecoder(data), null);
                else
                    HandleData?.Invoke(data, null);
            }
        }

        public void OnClosed()
        {
            DestroyManager();
        }

        public void DeinitScanner()
        {
            if (emdkManager != null)
            {
                if (emdkScanner != null)
                {
                    try
                    {
                        emdkScanner.Data -= ScannerData;
                        emdkScanner.Status -= ScannerStatus;

                        emdkScanner.Disable();
                    }
                    catch (ScannerException e)
                    {
                        displayStatus("Exception:" + e.Result.Description);
                    }
                }

                if (barcodeManager != null)
                {
                    emdkManager.Release(EMDKManager.FEATURE_TYPE.Barcode);
                }
                barcodeManager = null;
                emdkScanner = null;
            }
        }

        public void DestroyManager()
        {
            //Clean up the emdkManager
            if (emdkManager != null)
            {
                //EMDK: Release the EMDK manager object
                emdkManager.Release();
                emdkManager = null;
            }
        }
    }
}