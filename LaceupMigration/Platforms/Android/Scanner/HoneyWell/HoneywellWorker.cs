using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Honeywell.AIDC.CrossPlatform;

namespace LaceupMigration
{
    public class HoneywellWorker
    {
        private const string DEFAULT_READER_KEY = "default";
        private bool mOpenReader = true;
        private BarcodeReader mSelectedReader = null;
        private bool mSoftContinuousScanStarted = false;
        private bool mSoftOneShotScanStarted = false;

        public event EventHandler HandleData;
        public event EventHandler HandleDataQR;

        public bool IsWorking { get; set; }

        public BarcodeReader GetBarcodeReader(string readerName)
        {
            BarcodeReader reader = null;

            if (readerName == DEFAULT_READER_KEY)
            { // This name was added to the Open Reader picker list if the
              // query for connected barcode readers failed. It is not a
              // valid reader name. Set the readerName to null to default
              // to internal scanner.
                readerName = null;
            }

            if (null == reader)
            {
                // Create a new instance of BarcodeReader object.
                reader = new BarcodeReader(readerName);



                // Add an event handler to receive barcode data.
                // Even though we may have multiple reader sessions, we only
                // have one event handler. In this app, no matter which reader
                // the data come frome it will update the same UI controls.
                reader.BarcodeDataReady += MBarcodeReader_BarcodeDataReady;
            }

            return reader;
        }

        private void MBarcodeReader_BarcodeDataReady(object sender, BarcodeDataArgs e)
        {
            if (!IsWorking)
            {
                string s = e.Data;

                if(e.SymbologyType == BarcodeSymbologies.Qr ||
                    (e.SymbologyType == BarcodeSymbologies.Code128 &&s.Contains('/')) ||
                    e.SymbologyType == BarcodeSymbologies.Gs1128||
                    e.SymbologyType == BarcodeSymbologies.Gs1DataBarExpanded ||
                    e.SymbologyType == BarcodeSymbologies.Gs1DataBarLimited ||
                    e.SymbologyType == BarcodeSymbologies.Gs1DataBarOmniDir)
                    HandleDataQR?.Invoke(BarcodeDecoder.CreateDecoder(s), null);
                else
                    HandleData?.Invoke(e.Data, null);
            }
        }

        public async void OpenBarcodeReader()
        {
            if (mOpenReader) // Open Reader switch is in the On state.
            {
                mSelectedReader = GetBarcodeReader(DEFAULT_READER_KEY);
                if (!mSelectedReader.IsReaderOpened)
                {
                    BarcodeReader.Result result = await mSelectedReader.OpenAsync();

                    if (result.Code == BarcodeReader.Result.Codes.SUCCESS ||
                        result.Code == BarcodeReader.Result.Codes.READER_ALREADY_OPENED)
                    {
                        SetScannerAndSymbologySettings();
                    }
                    else
                    {
                        Logger.CreateLog("Error OpenAsync failed, Code:" + result.Code + " Message:" + result.Message);
                    }
                }
            } //endif (mOpenReader)
        }

        public async void CloseBarcodeReader()
        {
            if (mSelectedReader != null && mSelectedReader.IsReaderOpened)
            {
                if (mSoftOneShotScanStarted || mSoftContinuousScanStarted)
                {
                    // Turn off the software trigger.
                    await mSelectedReader.SoftwareTriggerAsync(false);
                    mSoftOneShotScanStarted = false;
                    mSoftContinuousScanStarted = false;
                }

                BarcodeReader.Result result = await mSelectedReader.CloseAsync();
                if (result.Code != BarcodeReader.Result.Codes.SUCCESS)
                {
                    Logger.CreateLog("Error CloseAcync failed, Code:" + result.Code + " Message:" + result.Message);
                }
            }
        }

        private async void SetScannerAndSymbologySettings()
        {
            try
            {
                if (mSelectedReader.IsReaderOpened)
                {
                    Dictionary<string, object> settings = new Dictionary<string, object>()
                    {
                        {mSelectedReader.SettingKeys.TriggerScanMode, mSelectedReader.SettingValues.TriggerScanMode_OneShot },
                        {mSelectedReader.SettingKeys.QrCodeEnabled, true },
                        {mSelectedReader.SettingKeys.UpcAEnable, true },
                        {mSelectedReader.SettingKeys.UpcACheckDigitTransmitEnabled, true },
                        {mSelectedReader.SettingKeys.Code128Enabled, true },
                        {mSelectedReader.SettingKeys.Code39Enabled, true },
                        {mSelectedReader.SettingKeys.Ean8Enabled, true },
                        {mSelectedReader.SettingKeys.Ean8CheckDigitTransmitEnabled, true },
                        {mSelectedReader.SettingKeys.Ean13Enabled, true },
                        {mSelectedReader.SettingKeys.Ean13CheckDigitTransmitEnabled, true },
                        {mSelectedReader.SettingKeys.Interleaved25Enabled, true },
                        {mSelectedReader.SettingKeys.Interleaved25MaximumLength, 100 },
                        {mSelectedReader.SettingKeys.Postal2DMode, mSelectedReader.SettingValues.Postal2DMode_Usps }
                    };

                    BarcodeReader.Result result = await mSelectedReader.SetAsync(settings);
                    if (result.Code != BarcodeReader.Result.Codes.SUCCESS)
                    {
                        Logger.CreateLog("Error Symbology settings faied, Code:" + result.Code + " Message:" + result.Message);
                    }
                }
            }
            catch (Exception exp)
            {
                Logger.CreateLog("Error Symbology settings faied, Exception:" + exp.Message);
            }
        }
    }
}