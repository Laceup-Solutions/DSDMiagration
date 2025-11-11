using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Com.Cipherlab.Barcode;
using Com.Cipherlab.Barcode.Decoder;
using Com.Cipherlab.Barcode.Decoderparams;

namespace LaceupMigration
{
    [BroadcastReceiver(Enabled = true, Exported = false)]
    public class CipherlabScanner : BroadcastReceiver, IDisposable
    {
        Activity context;
        ReaderManager readerManager;
        IntentFilter filter;

        public event EventHandler HandleData;
        public event EventHandler HandleDataQR;

        public bool IsWorking { get; set; }

        public CipherlabScanner()
        {

        }

        public CipherlabScanner(Activity context)
        {
            this.context = context;

            readerManager = ReaderManager.InitInstance(context);
            
            filter = new IntentFilter();
            filter.AddAction(GeneralString.IntentSOFTTRIGGERDATA);
            filter.AddAction(GeneralString.IntentPASSTOAPP);
            filter.AddAction(GeneralString.IntentREADERSERVICECONNECTED);

            context.RegisterReceiver(this, filter);
        }

        public void InitScanner()
        {
            
        }

        public void DeinitScanner()
        {
            
        }

        public void Triger()
        {
            if (readerManager != null)
                readerManager.SoftScanTrigger();
        }

        public override void OnReceive(Context context, Intent intent)
        {
            if (intent.Action.Equals(GeneralString.IntentSOFTTRIGGERDATA))
            {
                if (IsWorking)
                    return;

                string sDataStr = intent.GetStringExtra(GeneralString.BcReaderData);

                if (sDataStr.Contains('\n'))
                    sDataStr = sDataStr.Remove('\n');

                if (sDataStr.Contains('|'))
                    HandleDataQR?.Invoke(sDataStr, null);
                else
                    HandleData?.Invoke(sDataStr, null);
            }
            else if (intent.Action.Equals(GeneralString.IntentREADERSERVICECONNECTED))
            {
                try
                {
                    if (readerManager != null)
                    {
                        ReaderOutputConfiguration settings = new ReaderOutputConfiguration();

                        readerManager.Set_ReaderOutputConfiguration(settings);

                        readerManager.Get_ReaderOutputConfiguration(settings);

                        settings.EnableKeyboardEmulation = KeyboardEmulationType.None;

                        var result = readerManager.Set_ReaderOutputConfiguration(settings);
                        if (result == ClResult.SErr)
                        {

                        }
                        else if (result == ClResult.ErrInvalidParameter)
                        {

                        }
                        else if (result == ClResult.ErrNotSupport)
                        {

                        }
                        else if (result == ClResult.SOk)
                        {

                        }
                    }
                }
                catch(Exception ex)
                {

                }
            }
        }
    }
}