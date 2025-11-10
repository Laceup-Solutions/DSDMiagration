using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocketMobile.Capture;

namespace LaceupMigration
{
    public class SocketMobileWorker
    {
        public event EventHandler DecodeData;
        public event EventHandler DecodeDataQR;
        public event EventHandler DeviceConnected;

        CaptureHelper capture;

        string appId = "android:com.laceupsolutions.LaceupDeliveryDSD";
        string developerId = "c3662daa-937b-eb11-a812-000d3a3bd33b";
        string appKey = "MC0CFQC+5yPXyBxNFwNidh9U/G5rsi5JngIUG0gTIGXsJD4yGY2skszdQau5Vtg=";

        public bool Isworking { get; set; }

        public SocketMobileWorker()
        {
            capture = new CaptureHelper();
            capture.DoNotUseWebSocket = true;

            capture.DeviceArrival += Capture_DeviceArrival;
            capture.DeviceRemoval += Capture_DeviceRemoval;
            capture.DecodedData += Capture_DecodedData;
            capture.Errors += Capture_Errors;
            capture.DevicePowerState += Capture_DevicePowerState;
            capture.Terminate += Capture_Terminate;


            OpenConnection();
        }

        async void OpenConnection()
        {
            try
            {
                long result = await capture.OpenAsync(appId, developerId, appKey);

                if (!SktErrors.SKTSUCCESS(result))
                {
                    Logger.CreateLog("Unable to connect to Socket Mobile Companion");
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void Capture_Terminate(object sender, CaptureHelper.TerminateArgs e)
        {

        }

        private void Capture_DevicePowerState(object sender, CaptureHelper.PowerStateArgs e)
        {

        }

        private void Capture_Errors(object sender, CaptureHelper.ErrorEventArgs e)
        {

        }

        private void Capture_DecodedData(object sender, CaptureHelper.DecodedDataArgs e)
        {
            if (!Isworking)
            {
                string s = new string(e.DecodedData.DataToUTF8String);

                // Updated symbology ID constants for new API
                const int kQRCode = 114;
                const int kCode128 = 8;
                const int kGs1Databar = 29;
                const int kGs1DatabarExpanded = 30;
                const int kGs1DatabarLimited = 31;
                
                // Check if the DecodedData has symbology information
                // In the new API, symbology info might be in a different property or method
                int symbologyId = 0;
                
                // Try to get symbology ID from available properties
                if (e.DecodedData.GetType().GetProperty("SymbologyId") != null)
                {
                    symbologyId = (int)e.DecodedData.GetType().GetProperty("SymbologyId").GetValue(e.DecodedData);
                }
                else if (e.DecodedData.GetType().GetProperty("Symbology") != null)
                {
                    var symbology = e.DecodedData.GetType().GetProperty("Symbology").GetValue(e.DecodedData);
                    if (symbology?.GetType().GetProperty("ID") != null)
                    {
                        symbologyId = (int)symbology.GetType().GetProperty("ID").GetValue(symbology);
                    }
                }
                
                // Fallback: use string-based detection if symbology ID is not available
                if (symbologyId == kQRCode ||
                    (symbologyId == kCode128 && s.Contains("/")) ||
                    symbologyId == kGs1Databar ||
                    symbologyId == kGs1DatabarExpanded ||
                    symbologyId == kGs1DatabarLimited ||
                    // Fallback string-based detection for QR codes
                    (symbologyId == 0 && (s.StartsWith("http") || s.Contains("BEGIN:VCARD") || s.Length > 50)))
                {
                    DecodeDataQR?.Invoke(BarcodeDecoder.CreateDecoder(s), null);
                }
                else
                {
                    DecodeData?.Invoke(s, null);
                }
            }
        }

        private void Capture_DeviceRemoval(object sender, CaptureHelper.DeviceArgs e)
        {

        }

        private void Capture_DeviceArrival(object sender, CaptureHelper.DeviceArgs e)
        {
            DeviceConnected?.Invoke(e.CaptureDevice.GetDeviceInfo().Name, null);
        }
    }
}