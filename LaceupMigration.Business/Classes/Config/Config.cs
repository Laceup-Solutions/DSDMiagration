using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Maui.Storage;

namespace LaceupMigration
{
    /// <summary>
    /// Keeps the configuration settings of the application
    /// </summary>`
    public partial class Config
    {
        public static IInterfaceHelper? helper;

        public static void Initialize()
        {
            //CurrentContext = referenceActivity;
            AllowFreeItems = true;
            try
            {
                AssignValues();

                // Move from the OLD format to the new one
                var codebase = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                var oldData = Path.Combine(codebase, "Data");
                if (Directory.Exists(oldData))
                {
                    if (Directory.Exists(Config.DataPath)) Directory.Delete(Config.DataPath);
                    Directory.Move(oldData, Config.DataPath);

                    // move back the data files
                    var file = Path.Combine(Config.DataPath, "clients.cvs");
                    if (File.Exists(file)) File.Move(file, Config.ClientStoreFile);
                    file = Path.Combine(Config.DataPath, "products.cvs");
                    if (File.Exists(file)) File.Move(file, Config.ProductStoreFile);
                }

                oldData = Path.Combine(codebase, "BatchData");
                if (Directory.Exists(oldData))
                {
                    if (Directory.Exists(Config.BatchPath)) Directory.Delete(Config.BatchPath);
                    Directory.Move(oldData, Config.BatchPath);
                }

                oldData = Path.Combine(codebase, "CurrentOrders");
                if (Directory.Exists(oldData))
                {
                    if (Directory.Exists(Config.CurrentOrdersPath)) Directory.Delete(Config.CurrentOrdersPath);
                    Directory.Move(oldData, Config.CurrentOrdersPath);
                }

                oldData = Path.Combine(codebase, "PaymentsData");
                if (Directory.Exists(oldData))
                {
                    if (Directory.Exists(Config.PaymentPath)) Directory.Delete(Config.PaymentPath);
                    Directory.Move(oldData, Config.PaymentPath);
                }

                oldData = Path.Combine(codebase, "Orders");
                if (Directory.Exists(oldData))
                {
                    if (Directory.Exists(Config.OrderPath)) Directory.Delete(Config.OrderPath);
                    Directory.Move(oldData, Config.OrderPath);
                }

                oldData = Path.Combine(codebase, "consignmentData");
                if (Directory.Exists(oldData))
                {
                    if (Directory.Exists(Config.ConsignmentPath)) Directory.Delete(Config.ConsignmentPath);
                    Directory.Move(oldData, Config.ConsignmentPath);
                }

                oldData = Path.Combine(codebase, "SentPaymentPath");
                if (Directory.Exists(oldData))
                {
                    if (Directory.Exists(Config.SentPaymentPath)) Directory.Delete(Config.SentPaymentPath);
                    Directory.Move(oldData, Config.SentPaymentPath);
                }

                GetDeviceID();
                LoadCurrentOrderId();
                LoadCurrentBatchId();
                LoadSessionId();
            }
            catch (Exception ex)
            {
                //Log the exception
                Logger.CreateLog(ex);
                //if (Xamarin.Insights.IsInitialized)
                //Xamarin.Insights.Report(ex);
            }
        }

        public static void GetDeviceID()
        {
            lock (FileOperationsLocker.lockFilesObject)
            {
                string fileName = Path.Combine(Config.BasePath, "instalation.id");
                string id;
                if (File.Exists(fileName))
                {
                    using (StreamReader reader = new StreamReader(fileName))
                    {
                        id = reader.ReadToEnd();
                    }
                }
                else
                {
                    id = Guid.NewGuid().ToString("N");

                    try
                    {
                        //FileOperationsLocker.InUse = true;

                        using (StreamWriter writer = new StreamWriter(fileName))
                        {
                            writer.Write(id);
                        }
                    }
                    finally
                    {
                        //FileOperationsLocker.InUse = false;
                    }
                }

                // var s = Settings.Secure.GetString(Android.App.Application.ApplicationContext.ContentResolver, Settings.Secure.AndroidId); 

                DeviceId = "android" + id;
            }
        }

        static void AssignValues()
        {
            UpdateSetting();
            Config.OrderDatePrintFormat = "MM/dd/yy h:mm tt";
            Config.InvoiceCopyDatePrintFormat = "MM/dd/yy";

            //See if the directory for the data exist
            if (!Directory.Exists(Config.ImageStorePath))
            {
                //Create the dir
                Directory.CreateDirectory(Config.ImageStorePath);
            }

            //See if the directory for the data exist
            if (!Directory.Exists(Config.StaticDataPath))
            {
                //Create the dir
                Directory.CreateDirectory(Config.StaticDataPath);
            }

            //See if the directory for the data exist
            if (!Directory.Exists(Config.CodeBase))
            {
                //Create the dir
                Directory.CreateDirectory(Config.CodeBase);
            }

            //See if the directory for the data exist
            if (!Directory.Exists(Config.DataPath))
            {
                //Create the dir
                Directory.CreateDirectory(Config.DataPath);
            }

            if (!Directory.Exists(Config.OrderPath))
            {
                //Create the dir
                Directory.CreateDirectory(Config.OrderPath);
            }

            if (!Directory.Exists(Config.CurrentOrdersPath))
            {
                //Create the dir
                Directory.CreateDirectory(Config.CurrentOrdersPath);
            }

            if (!Directory.Exists(Config.SentPaymentPath))
            {
                //Create the dir
                Directory.CreateDirectory(Config.SentPaymentPath);
            }

            if (!Directory.Exists(Config.PaymentPath))
            {
                //Create the dir
                Directory.CreateDirectory(Config.PaymentPath);
            }

            if (!Directory.Exists(Config.BatchPath))
            {
                //Create the dir
                Directory.CreateDirectory(Config.BatchPath);
            }

            if (!Directory.Exists(Config.InvoicesPath))
            {
                //Create the dir
                Directory.CreateDirectory(Config.InvoicesPath);
            }

            if (!Directory.Exists(Config.SessionPath))
            {
                //Create the dir
                Directory.CreateDirectory(Config.SessionPath);
            }

            if (!Directory.Exists(Config.ConsignmentPath))
            {
                //Create the dir
                Directory.CreateDirectory(Config.ConsignmentPath);
            }

            //See if the directory for the data exist
            if (!Directory.Exists(Config.OrderImageStorePath))
            {
                //Create the dir
                Directory.CreateDirectory(Config.OrderImageStorePath);
            }

            if (!Directory.Exists(Config.ZPLOrdersPrintedPath))
            {
                //Create the dir
                Directory.CreateDirectory(Config.ZPLOrdersPrintedPath);
            }

            if (Directory.Exists(Config.InvoicesOldPath))
            {
                Directory.Delete(Config.InvoicesOldPath, true);
            }

            LoadSystemSettings();

            LoadAppStatus();
        }

        static void UpdateSetting()
        {
            IgnoreDiscountInCredits = Preferences.Get(IgnoreDiscountInCreditsKey, true);
            CheckInventoryInPreSale = Preferences.Get(CheckInventoryInPreSaleKey, false);
            AlwaysUpdateNewPar = Preferences.Get(AlwaysUpdateNewParKey, false);
            ShipDateIsMandatory = Preferences.Get(ShipDateIsMandatoryKey, false);
            ShipDateIsMandatoryForLoad = Preferences.Get(ShipDateIsMandatoryForLoadKey, false);
            PresaleShipDate = Preferences.Get(PresaleShipDateKey, false);
            CoreAsCredit = Preferences.Get(CoreAsCreditKey, false);
            PrintClientOpenBalance = Preferences.Get(PrintClientOpenBalanceKey, false);
            CreditTemplate = Preferences.Get(CreditTemplateKey, false);
            AskDEXUoM = Preferences.Get(AskDEXUoMKey, false);
            Wstco = Preferences.Get(WstcoKey, false);
            UseLSPByDefault = Preferences.Get(UseLSPByDefaultKey, false);
            ConsignmentShow0 = Preferences.Get(ConsignmentShow0Key, true);
            ConsignmentMustCountAll = Preferences.Get(ConsignmentMustCountAllKey, false);
            ConsignmentKeepPrice = Preferences.Get(ConsignmentKeepPriceKey, false);
            PrintInvoiceNumberDown = Preferences.Get(PrintInvoiceNumberDownKey, false);
            PrintFromClientDetail = Preferences.Get(PrintFromClientDetailKey, true);
            AddQtyTotalRegardlessUoM = Preferences.Get(AddQtyTotalRegardlessUoMKey, false);
            ShowProductsWith0Inventory = Preferences.Get(ShowProductsWith0InventoryKey, false);
            DisplayTaxOnCatalogAndPrint = Preferences.Get(DisplayTaxOnCatalogAndPrintKey, false);
            PrasekCustomization = Preferences.Get(PrasekCustomizationKey, false);
            AlwaysShowDefaultUoM = Preferences.Get(AlwaysShowDefaultUoMKey, false);
            ButlerCustomization = Preferences.Get(ButlerCustomizationKey, false);
            ShowPrintPickTicket = Preferences.Get(ShowPrintPickTicketKey, false);
            CaribCustomization = Preferences.Get(CaribCustomizationKey, false);
            ConcatUPCToName = Preferences.Get(ConcatUPCToNameKey, false);
            PrintUPCInventory = Preferences.Get(PrintUPCInventoryKey, false);
            PrintUPCOpenInvoices = Preferences.Get(PrintUPCOpenInvoicesKey, false);
            PrintLotPreOrder = Preferences.Get(PrintLotPreOrderKey, true);
            PrintLotOrder = Preferences.Get(PrintLotOrderKey, true);
            CanVoidFOrders = Preferences.Get(CanVoidFOrdersKey, true);
            BetaFeatures = Preferences.Get(BetaFeaturesKey, false);
            MustBeOnlineAlways = Preferences.Get(MustBeOnlineAlwaysKey, false);
            EnableLiveData = Preferences.Get(EnableLiveDataKey, false);
            GroupRelatedWhenPrinting = Preferences.Get(GroupRelatedWhenPrintingKey, false);
            MustEndOrders = Preferences.Get(MustEndOrdersKey, true);
            ProductInMultipleCategory = Preferences.Get(ProductInMultipleCategoryKey, false);
            AllowEditRelated = Preferences.Get(AllowEditRelatedKey, false);
            ExtendedPaymentOptions = Preferences.Get(ExtendedPaymentOptionsKey, true);
            HideTotalOrder = Preferences.Get(HideTotalOrderKey, false);
            HideTotalInPrintedLine = Preferences.Get(HideTotalInPrintedLineKey, false);
            PrintCopy = Preferences.Get(PrintCopyKey, false);
            Consignment = Preferences.Get(ConsignmentKey, false);
            MustEndOfDayDaily = Preferences.Get(MustEndOfDayDailyKey, false);
            ForceEODWhenDateChanges = Preferences.Get(ForceEODWhenDateChangesKey, false);
            UseReship = Preferences.Get(UseReshipKey, true);
            UsesTerms = Preferences.Get(UsesTermsKey, false);
            UseClockInOut = Preferences.Get(UseClockInOutKey, false);
            TimeSheetCustomization = Preferences.Get(TimeSheetCustomizationKey, false);
            GenerateValuesLoadOrder = Preferences.Get(GenerateValuesLoadOrderKey, false);
            try
            {
                OldPrinter = Preferences.Get(OldPrinterKey, 1);
            }
            catch (Exception ee)
            {
                var oldBool = Preferences.Get(OldPrinterKey, false);
                if (oldBool)
                    OldPrinter = 1;
                else
                    OldPrinter = 0;
            }

            Config.DexVersion = Preferences.Get(DexVersionKey, "4010");
            Config.DexDefaultUnit = Preferences.Get(DexDefaultUnitKey, "");
            UpdateWhenEndDay = Preferences.Get(UpdateWhenEndDayKey, false);
            UseOrderId = Preferences.Get(UseOrderIdKey, false);
            try
            {
                LastPrintedId = Preferences.Get(LastOrderIdKey, (long)1);
            }
            catch
            {
                try
                {
                    LastPrintedId = Preferences.Get(LastOrderIdKey, 1);
                }
                catch
                {
                    LastPrintedId = 1;
                }
            }

            try
            {
                LastPresalePrintedId = Preferences.Get(LastPresaleOrderIdKey, (long)1);
            }
            catch
            {
                try
                {
                    LastPresalePrintedId = Preferences.Get(LastPresaleOrderIdKey, 1);
                }
                catch
                {
                    LastPresalePrintedId = 1;
                }
            }

            Config.AutoAcceptLoad = Preferences.Get(AutoAcceptLoadKey, false);
            Config.RemoveWarnings = Preferences.Get(RemoveWarningsKey, false);
            Config.GroupLinesWhenPrinting = Preferences.Get(GroupLinesWhenPrintingKey, true);
            Config.ShortInventorySettlement = Preferences.Get(ShortInventorySettlementKey, true);
            Config.PrintReportsRequired = Preferences.Get(PrintReportsRequiredKey, true);
            Config.LoadOrderEmpty = Preferences.Get(LoadOrderEmptyKey, true);
            Config.AcceptLoadPrintRequired = Preferences.Get(AcceptLoadPrintRequiredKey, true);
            Config.AddDefaultItemToCredit = Preferences.Get(AddDefaultItemToCreditKey, false);
            Config.DefaultItem = Preferences.Get(DefaultItemKey, 0);
            Config.DistanceToOrder = Preferences.Get(DistanceToOrderKey, 0);
            Config.ProductFileCreationDate = Preferences.Get(ProductFileCreationDateKey, 0);
            Config.ShowInvoiceTotal = Preferences.Get(ShowInvoiceTotalKey, true);
            Config.BackGroundSync = Preferences.Get(BackGroundSyncKey, true);
            Config.CanLeaveBatch = Preferences.Get(CanLeaveBatchKey, false);
            Config.PrintUpcAsText = Preferences.Get(PrintUpcAsTextKey, false);
            Config.ExtraTextForPrinter = Preferences.Get(ExtraTextForPrinterKey, string.Empty);
            Config.InvoicePrefix = Preferences.Get(InvoicePrefixKey, string.Empty);
            Config.SurveyQuestions = Preferences.Get(SurveyQuestionsKey, string.Empty);
            Config.LastSignIn = Preferences.Get(LastSignInKey, string.Empty);
            Config.InvoicePresalePrefix = Preferences.Get(InvoicePresalePrefixKey, string.Empty);
            Config.AutoEndInventoryPassword = Preferences.Get(AutoEndInventoryPasswordKey, string.Empty);
            Config.CremiMexDepartments = Preferences.Get(CremiMexDepartmentsKey, string.Empty);
            Config.MustPrintPreOrder = Preferences.Get(MustPrintPreOrderKey, false);
            Config.LoadLotInTransfer = Preferences.Get(LoadLotInTransferKey, false);
            Config.CanIncreasePrice = Preferences.Get(CanIncreasePriceKey, false);
            Config.LoadRequest = Preferences.Get(LoadRequestKey, false);
            Config.LoadRequired = Preferences.Get(LoadRequiredKey, false);
            Config.EmptyTruckAtEndOfDay = Preferences.Get(EmptyTruckAtEndOfDayKey, true);
            Config.LeftOrderTemplateEmpty = Preferences.Get(LeftOrderTemplateEmptyKey, false);
            Config.Delivery = Preferences.Get(DeliveryKey, false);
            Config.PickCompany = Preferences.Get(PickCompanyKey, true);
            Config.ProductCatalog = Preferences.Get(ProductCatalogKey, true);
            Config.PreSale = Preferences.Get(PreSaleKey, false);
            Config.SendLogByEmail = Preferences.Get(SendLogByEmailKey, false);
            Config.EmailOrder = Preferences.Get(EmailOrderKey, false);
            Config.FakePreOrder = Preferences.Get(FakePreOrderKey, false);
            Config.PrintTruncateNames = Preferences.Get(PrintTruncateNamesKey, false);
            Config.PopulateTemplateAuthProd = Preferences.Get(PopulateTemplateAuthProdKey, false);
            Config.OneDoc = Preferences.Get(OneDocKey, false);
            Config.CanAddClient = Preferences.Get(CanAddClientKey, false);
            Config.SetPO = Preferences.Get(SetPOKey, false);
            Config.InventoryRequestEmail = Preferences.Get(InvReqKey, string.Empty);
            Config.CanGoBelow0 = Preferences.Get(CanGoBelow0Key, false);
            Config.NoPriceChangeDeliveries = Preferences.Get(NoPriceChangeDeliveriesKey, false);
            Config.WarningDumpReturn = Preferences.Get(WarningDumpReturnKey, false);
            Config.Round = Preferences.Get(RoundKey, 2);
            Config.DexAvailable = Preferences.Get(DexKey, false);
            Config.PrintUPC = Preferences.Get(PrintUPCKey, true);
            Config.CanChangeSalesmanId = Preferences.Get(CanChangeSalesmanIdKey, true);
            Config.BottomOrderPrintText = Preferences.Get(BottomOrderPrintTextKey, string.Empty);
            Config.SingleScanStroke = Preferences.Get(SingleScanStrokeKey, true);
            Config.PrintInvoiceSort = Preferences.Get(PrintInvoiceSortKey, string.Empty);
            Config.PrintClientSort = Preferences.Get(PrintClientSortKey, string.Empty);
            Config.CompanyLogo = Preferences.Get(CompanyLogoKey, string.Empty);
            Config.CompanyLogoSize = Preferences.Get(CompanyLogoSizeKey, 0);
            Config.CompanyLogoWidth = Preferences.Get(CompanyLogoWidthKey, 0);
            Config.CompanyLogoHeight = Preferences.Get(CompanyLogoHeightKey, 0);
            Config.MustSendOrdersFirst = Preferences.Get(MustSendOrdersFirstKey, true);
            Config.AllowDiscount = Preferences.Get(AllowDiscountKey, false);
            Config.LocationIsMandatory = Preferences.Get(LocationIsMandatoryKey, false);
            Config.AlwaysAddItemsToOrder = Preferences.Get(AlwaysAddItemsToOrderKey, false);
            Config.FreeItemsNeedComments = Preferences.Get(FreeItemsNeedCommentsKey, false);
            Config.LotIsMandatory = Preferences.Get(LotIsMandatoryKey, false);
            Config.PaymentAvailable = Preferences.Get(PaymentAvailableKey, false);
            Config.DisablePaymentIfTermDaysMoreThan0 = Preferences.Get(DisablePaymentIfTermDaysMoreThan0Key, false);
            Config.PaymentRequired = Preferences.Get(PaymentRequiredKey, false);
            Config.PrinterToUse = Preferences.Get(PrinterToUseKey, "");
            Config.InvoiceIdProvider = Preferences.Get(InvoiceIdKey, "LaceUPMobileClassesIOS.DefaultInvoiceProvider");
            if (InvoiceIdProvider == "LaceUPMobileClassesIOS.SequentialInvoiceProvider")
            {
                Config.InvoiceIdProvider = "LaceUPMobileClassesIOS.PrefixedSequentialInvoiceProvider";
            }
            else
                Config.InvoiceIdProvider = InvoiceIdProvider;

            Config.SignatureRequired = Preferences.Get(SignatureRequiredKey, false);
            Config.UseLot = Preferences.Get(LotKey, false);
            Config.FakeUseLot = Preferences.Get(FakeUseLotKey, false);
            Config.DisplayPurchasePrice = Preferences.Get(DisplayPurchasePriceKey, false);
            Config.AnyPriceIsAcceptable = Preferences.Get(AnyPriceIsAcceptableKey, true);
            Config.UseLSP = Preferences.Get(UseLSPKey, false);
            Config.DaysToKeepOrder = Preferences.Get(DaysToKeepOrderKey, 90);
            Config.TransferPassword = Preferences.Get(TransferPasswordKey, string.Empty);
            Config.TransferOffPassword = Preferences.Get(TransferOffPasswordKey, string.Empty);
            Config.InventoryPassword = Preferences.Get(InventoryPasswordKey, string.Empty);
            Config.AddInventoryPassword = Preferences.Get(AddInventoryPasswordKey, string.Empty);
            Config.TrackInventory = Preferences.Get(TrackInventoryKey, false);
            Config.CanModifyInventory = Preferences.Get(CanModifyInventoryKey, false);
            Config.PrinterAvailable = Preferences.Get(PrinterAvailableKey, true);
            Config.PrintingRequired = Preferences.Get(PrintingRequiredKey, true);
            Config.AllowCreditOrders = Preferences.Get(AllowCreditOrdersKey, true);
            Config.SalesmanId = Preferences.Get(VendorIdKey, 1);
            Config.RouteName = Preferences.Get(RouteNameKey, "");
            Config.VendorName = Preferences.Get(VendorNameKey, "");
#if DEBUG
            Config.Port = Preferences.Get(PortKey, 947);
            Config.IPAddressGateway = Preferences.Get(IPAddressGatewayKey, "192.168.1.59");
#else
            Config.Port = Preferences.Get(PortKey, 9999);
            Config.IPAddressGateway = Preferences.Get(IPAddressGatewayKey, "app.laceupsolutions.com");
#endif
            Config.LanAddress = Preferences.Get(LanKey, IPAddressGateway);
            Config.SSID = Preferences.Get(SSIDKey, string.Empty);
            Config.MustUpdateDaily = Preferences.Get(MustUpdateDailyKey, true);
            //Config.CompanyName = Preferences.Get (CompanyNameKey, "Laceup Solutions, inc.");
            //Config.CompanyAddress1 = Preferences.Get (CompanyAddress1Key, "1040 falcon ave");
            //Config.CompanyAddress2 = Preferences.Get (CompanyAddress2Key, "Miami fl 33166");
            //Config.CompanyPhone = Preferences.Get (CompanyPhoneKey, " 305 3366750");
            Config.AllowFreeItems = Preferences.Get(AllowFreeItemsKey, true);
            Config.SendLoadOrder = Preferences.Get(SendLoadOrderKey, false);
            Config.UseLocation = Preferences.Get(UseLocationKey, true);
            Config.AllowOrderForClientOverCreditLimit = Preferences.Get(AllowOrderForClientOverCreditLimitKey, true);
            Config.UserCanChangePrices = Preferences.Get(UserCanChangePricesKey, false);
            string activityProvider = Preferences.Get(ViewProviderAndroidKey, string.Empty);
            if (!string.IsNullOrEmpty(activityProvider))
                DataAccess.ActivityProvider.DeSerializeFromString(activityProvider);

            if (string.IsNullOrEmpty(Config.LanAddress)) Config.LanAddress = iPAddressGateway;

            Config.ScannerToUse = Preferences.Get(ScannerToUseKey, 3);
            Config.UseUpcCheckDigit = Preferences.Get(UseUpcCheckDigitKey, true);
            Config.RouteManagement = Preferences.Get(RouteManagementKey, false);
            Config.SetParLevel = Preferences.Get(SetParLevelKey, false);
            Config.UseUpc128 = Preferences.Get(UseUpc128Key, false);
            Config.DeliveryScanning = Preferences.Get(DeliveryScanningKey, false);
            Config.UseBattery = Preferences.Get(UseBatteryKey, false);
            Config.NewConsPrinter = Preferences.Get(NewConsPrinterKey, false);
            Config.UseOldEmailFormat = Preferences.Get(UseOldEmailFormatKey, false);
            Config.PrintedIdLength = Preferences.Get(PrintedIdLengthKey, 0);
            Config.HideSetConsignment = Preferences.Get(HideSetConsignmentKey, false);
            Config.AutoGeneratePO = Preferences.Get(AutoGeneratePOKey, false);
            Config.ExtraSpaceForSignature = Preferences.Get(ExtraSpaceForSignatureKey, 0);
            Config.DefaultCreditDetType = Preferences.Get(DefaultCreditDetTypeKey, string.Empty);
            Config.CanSelectSalesman = Preferences.Get(CanSelectSalesmanKey, false);
            Config.UsePairLotQty = Preferences.Get(UsePairLotQtyKey, false);
            Config.UseSendByEmail = Preferences.Get(UseSendByEmailKey, false);
            Config.UsePrintProofDelivery = Preferences.Get(UsePrintProofDeliveryKey, false);
            Config.HideCompanyInfoPrint = Preferences.Get(HideCompanyInfoPrintKey, false);
            Config.PrintIsMandatory = Preferences.Get(PrintIsMandatoryKey, false);
            Config.SelectDriverFromPresale = Preferences.Get(SelectDriverFromPresaleKey, false);
            Config.MustCompleteRoute = Preferences.Get(MustCompleteRouteKey, false);
            Config.Discount100PercentPrintText = Preferences.Get(Discount100PercentPrintTextKey, string.Empty);
            Config.RemovePayBalFomInvoice = Preferences.Get(RemovePayBalFomInvoiceKey, false);
            Config.AddRelatedItemsInTotal = Preferences.Get(AddRelatedItemsInTotalKey, true);
            Config.SignatureNameRequired = Preferences.Get(SignatureNameRequiredKey, false);
            Config.PrintZeroesOnPickSheet = Preferences.Get(PrintZeroesOnPickSheetKey, false);
            Config.AddCoresInSalesItem = Preferences.Get(AddCoresInSalesItemKey, true);
            Config.UseAllowance = Preferences.Get(UseAllowanceKey, false);
            Config.IncludeDeliveriesInLoadOrder = Preferences.Get(IncludeDeliveriesInLoadOrderKey, false);
            Config.UseReturnInvoice = Preferences.Get(UseReturnInvoiceKey, false);
            Config.AddItemInDefaultUoM = Preferences.Get(AddItemInDefaultUoMKey, false);
            Config.UseTermsInLoadOrder = Preferences.Get(UseTermsInLoadOrderKey, false);
            Config.POIsMandatory = Preferences.Get(POIsMandatoryKey, false);
            Config.NewClientCanChangePrices = Preferences.Get(NewClientCanChangePricesKey, true);
            Config.PrintNetQty = Preferences.Get(PrintNetQtyKey, false);
            Config.AuthProdsInCredit = Preferences.Get(AuthProdsInCreditKey, false);
            Config.MagnoliaSetConsignment = Preferences.Get(MagnoliaSetConsignmentKey, false);
            Config.SyncLoadOnDemand = Preferences.Get(SyncLoadOnDemandKey, false);
            Config.MasterLoadOrder = Preferences.Get(MasterLoadOrderKey, false);
            Config.SalesRegReportWithTax = Preferences.Get(SalesRegReportWithTaxKey, false);
            Config.TransferComment = Preferences.Get(TransferCommentKey, false);
            Config.AddCoreBalance = Preferences.Get(AddCoreBalancekey, false);
            Config.HideItemComment = Preferences.Get(HideItemCommentKey, false);
            Config.SendByEmailInFinalize = Preferences.Get(SendByEmailInFinalizeKey, false);
            Config.HideInvoiceComment = Preferences.Get(HideInvoiceCommentKey, false);
            Config.IncludeBatteryInLoad = Preferences.Get(IncludeBatteryInLoadKey, false);
            Config.ClientDailyPL = Preferences.Get(ClientDailyPLKey, false);
            Config.BackgroundTime = Preferences.Get(BackgroundTimeKey, 30);
            Config.ShowAvgInCatalog = Preferences.Get(ShowAvgInCatalogKey, false);
            Config.DisableRouteReturn = Preferences.Get(DisableRouteReturnKey, false);
            Config.AllowAdjPastExpDate = Preferences.Get(AllowAdjPastExpDateKey, true);
            Config.ConsignmentContractText = Preferences.Get(ConsignmentContractTextKey, string.Empty);
            Config.SendBackgroundOrders = Preferences.Get(SendBackgroundOrdersKey, false);
            Config.HidePriceInPrintedLine = Preferences.Get(HidePriceInPrintedLineKey, false);
            Config.SendOrderIsMandatory = Preferences.Get(SendOrderIsMandatoryKey, false);
            Config.PaymentOrSignatureRequired = Preferences.Get(PaymentOrSignatureRequiredKey, false);
            Config.DoNotShrinkOrderImage = Preferences.Get(DoNotShrinkOrderImageKey, false);
            Config.MustEnterCaseInOut = Preferences.Get(MustEnterCaseInOutKey, false);
            Config.ParLevelHistoryDays = Preferences.Get(ParLevelHistoryDaysKey, 60);
            Config.AddSalesInConsignment = Preferences.Get(AddSalesInConsignmentKey, false);
            Config.CanChangeUoM = Preferences.Get(CanChangeUoMKey, true);
            Config.UseClientClassAsCompanyName = Preferences.Get(UseClientClassAsCompanyNameKey, false);
            Config.PdfProvider = Preferences.Get(PdfProviderKey, string.Empty);
            Config.NewClientEmailRequired = Preferences.Get(NewClientEmailRequiredKey, false);
            Config.NewClientExtraFields = Preferences.Get(NewClientExtraFieldsKey, string.Empty);
            Config.UseFullConsignment = Preferences.Get(UseFullConsignmentKey, false);
            Config.RTN = Preferences.Get(RTNKey, string.Empty);
            Config.BillNumRequired = Preferences.Get(BillNumRequiredKey, false);
            Config.DefaultTaxRate = Preferences.Get(DefaultTaxRateKey, 0);
            Config.UseLastUoM = Preferences.Get(UseLastUoMKey, false);
            Config.ShowAddrInClientList = Preferences.Get(ShowAddrInClientListKey, true);
            Config.SalesmanInCreditDel = Preferences.Get(SalesmanInCreditDelKey, false);
            Config.ClientNameMaxSize = Preferences.Get(ClientNameMaxSizeKey, 0);
            Config.MinShipDateDays = Preferences.Get(MinShipDateDaysKey, 0);
            Config.HidePrintedCommentLine = Preferences.Get(HidePrintedCommentLineKey, false);
            Config.UseDraggableTemplate = Preferences.Get(UseDraggableTemplateKey, false);
            Config.PrintBillShipDate = Preferences.Get(PrintBillShipDateKey, false);
            Config.DaysToKeepSignatures = Preferences.Get(DaysToKeepSignaturesKey, 7);
            Config.BlackStoneConsigCustom = Preferences.Get(BlackStoneConsigCustomKey, false);
            Config.HideProdOnHand = Preferences.Get(HideProdOnHandKey, false);

            Config.PrintInvSettReport = Preferences.Get(PrintInvSettReportKey, true);
            Config.PreSaleConsigment = Preferences.Get(PreSaleConsigmentKey, false);
            Config.UseConsignmentLot = Preferences.Get(UseConsignmentLotKey, false);
            Config.SendBackgroundBackup = Preferences.Get(SendBackgroundBackupKey, true);
            Config.MinimumWeight = Preferences.Get(MinimumWeightkey, 0);
            Config.MinimumAmount = Preferences.Get(Minimumamountkey, 0);
            Config.PresaleCommMandatory = Preferences.Get(PresaleCommMandatoryKey, false);
            Config.PrintTaxLabel = Preferences.Get(PrintTaxLabelKey, "SALES TAX:");
            Config.AllowDiscountPerLine = Preferences.Get(AllowDiscountPerLineKey, false);
            Config.HideVoidButton = Preferences.Get(HideVoidButtonKey, false);
            Config.ConsLotAsDate = Preferences.Get(ConsLotAsDateKey, false);
            Config.DisolCustomIdGenerator = Preferences.Get(DisolCustomIdGeneratorKey, false);
            Config.ParInConsignment = Preferences.Get(ParInConsignmentKey, false);
            Config.AddCreditInConsignment = Preferences.Get(AddCreditInConsignmentKey, true);
            Config.ShowShipVia = Preferences.Get(ShowShipViaKey, false);
            Config.ShipViaMandatory = Preferences.Get(ShipViaMandatoryKey, false);
            Config.GeneratePreorderNum = Preferences.Get(GeneratePreorderNumKey, true);
            Config.OnlyKitInCredit = Preferences.Get(OnlyKitInCreditKey, false);
            Config.DeliveryReasonInLine = Preferences.Get(DeliveryReasonInLineKey, false);
            AlwaysUpdateNewPar = Preferences.Get(AlwaysUpdateNewParKey, false);
            CanModifyConnectSett = Preferences.Get(CanModifyConnectSettKey, true);
            LspInAllLines = Preferences.Get(LspInAllLinesKey, false);
            UseAllDayParLevel = Preferences.Get(UseAllDayParLevelKey, false);
            CloseRouteInPresale = Preferences.Get(CloseRouteInPresaleKey, true);
            EditParInHistory = Preferences.Get(EditParInHistoryKey, false);
            AlwaysCountInPar = Preferences.Get(AlwaysCountInParKey, false);
            WarrantyPerClient = Preferences.Get(WarrantyPerClientKey, false);
            ChargeBatteryRotation = Preferences.Get(ChargeBatteryRotationKey, true);
            IncludeRotationInDelivery = Preferences.Get(IncludeRotationInDeliveryKey, false);
            ConsParFirstInPresale = Preferences.Get(ConsParFirstInPresaleKey, false);
            AverageSaleInParLevel = Preferences.Get(AverageSaleInParLevelKey, false);
            MultipleLoadOnDemand = Preferences.Get(MultipleLoadOnDemandKey, false);
            KeepPresaleOrders = Preferences.Get(KeepPresaleOrdersKey, false);
            ScanDeliveryChecking = Preferences.Get(ScanDeliveryCheckingKey, false);
            GeneratePresaleNumber = Preferences.Get(GeneratePresaleNumberKey, false);
            AllowMultParInvoices = Preferences.Get(AllowMultParInvoicesKey, false);
            SettReportInSalesUoM = Preferences.Get(SettReportInSalesUoMKey, false);
            IncludeCredInNewParCalc = Preferences.Get(IncludeCredInNewParCalcKey, false);
            DontAllowDecimalsInQty = Preferences.Get(DontAllowDecimalsInQtyKey, false);
            SendBackgroundPayments = Preferences.Get(SendBackgroundPaymentsKey, false);
            NewClientCanHaveDiscount = Preferences.Get(NewClientCanHaveDiscountKey, false);
            CaptureImages = Preferences.Get(CaptureImagesKey, true);
            ClientRtnNeededForQty = Preferences.Get(ClientRtnNeededForQtyKey, 0);
            SelectReshipDate = Preferences.Get(SelectReshipDateKey, false);
            RouteReturnPassword = Preferences.Get(RouteReturnPasswordKey, "");
            UseQuote = Preferences.Get(UseQuoteKey, false);
            MinimumAvailableNumbers = Preferences.Get(MinimumAvailableNumbersKey, 0);
            AdvanceSequencyNum = Preferences.Get(AdvanceSequencyNumKey, false);
            UserCanChangePricesSales = Preferences.Get(UserCanChangePricesSalesKey, false);
            UserCanChangePricesCredits = Preferences.Get(UserCanChangePricesCreditsKey, false);
            HideTransfers = Preferences.Get(HideTransfersKey, false);
            MustCompleteInDeliveryChecking = Preferences.Get(MustCompleteInDeliveryCheckingKey, true);
            ShowAllProductsInCredits = Preferences.Get(ShowAllProductsInCreditsKey, true);
            DeleteWeightItemsMenu = Preferences.Get(DeleteWeightItemsMenuKey, false);
            HidePresaleOptions = Preferences.Get(HidePresaleOptionsKey, false);
            ShowDiscountByPriceLevel = Preferences.Get(ShowDiscountByPriceLevelKey, false);
            UseFullTemplate = Preferences.Get(UseFullTemplateKey, false);
            AllowQtyConversionFactor = Preferences.Get(AllowQtyConversionFactorKey, true);
            DontDeleteEmptyDeliveries = Preferences.Get(DontDeleteEmptyDeliveriesKey, false);
            ViewGoals = Preferences.Get(ViewGoalsKey, false);
            EcoSkyWaterCustomEmail = Preferences.Get(EcoSkyWaterCustomEmailKey, false);
            KeepAppUpdated = Preferences.Get(KeepAppUpdatedKey, false);
            HideSalesOrders = Preferences.Get(HideSalesOrdersKey, false);

            AllowReset = Preferences.Get(AllowResetKey, false);
            HideClearData = Preferences.Get(HideClearDataKey, false);
            MustEnterPostedDate = Preferences.Get(MustEnterPostedDateKey, false);
            NeedAccessForConfiguration = Preferences.Get(NeedAccessForConfigurationKey, false);
            PreviewOfferPriceInAddItem = Preferences.Get(PreviewOfferPriceInAddItemKey, false);
            DeleteZeroItemsOnDelivery = Preferences.Get(DeleteZeroItemsOnDeliveryKey, true);
            SalesmanCanChangeSite = Preferences.Get(SalesmanCanChangeSiteKey, false);
            MustSetWeightInDelivery = Preferences.Get(MustSetWeightInDeliveryKey, false);
            PrintCreditReport = Preferences.Get(PrintCreditReportKey, false);
            CheckAvailableBeforeSending = Preferences.Get(CheckAvailableBeforeSendingKey, false);
            SAPOrderStatusReport = Preferences.Get(SAPOrderStatusReportKey, false);
            PresaleUseInventorySite = Preferences.Get(PresaleUseInventorySiteKey, false);
            CannotOrderWithUnpaidInvoices = Preferences.Get(CannotOrderWithUnpaidInvoicesKey, false);
            CanPayMoreThanOwned = Preferences.Get(CanPayMoreThanOwnedKey, false);
            ShowAllEmailsAsDestination = Preferences.Get(ShowAllEmailsAsDestinationKey, false);
            SelectPriceFromPrevInvoices = Preferences.Get(SelectPriceFromPrevInvoicesKey, false);
            UsePallets = Preferences.Get(UsePalletsKey, false);
            HideTaxesTotalPrint = Preferences.Get(HideTaxesTotalPrintKey, false);
            HideDiscountTotalPrint = Preferences.Get(HideDiscountTotalPrintKey, false);
            CanModifyWeightsOnDeliveries = Preferences.Get(CanModifyWeightsOnDeliveriesKey, false);
            ShowPricesInInventorySummary = Preferences.Get(ShowPricesInInventorySummaryKey, false);
            AlertOrderWasNotSent = Preferences.Get(AlertOrderWasNotSentKey, false);
            RequestVehicleInformation = Preferences.Get(RequestVehicleInformationKey, false);
            ShowCostInTemplate = Preferences.Get(ShowCostInTemplateKey, false);
            ShowListPriceInAdvancedCatalog = Preferences.Get(ShowListPriceInAdvancedCatalogKey, true);
            RecalculateOrdersAfterSync = Preferences.Get(RecalculateOrdersAfterSyncKey, false);
            ShowLowestPriceInTemplate = Preferences.Get(ShowLowestPriceInTemplateKey, false);
            HideSelectSitesFromMenu = Preferences.Get(HideSelectSitesFromMenuKey, false);
            ShowWeightOnInventorySummary = Preferences.Get(ShowWeightOnInventorySummaryKey, false);
            ShowLowestPriceLevel = Preferences.Get(ShowLowestPriceLevelKey, false);
            MustCreatePaymentDeposit = Preferences.Get(MustCreatePaymentDepositKey, false);
            PrintExternalInvoiceAsOrder = Preferences.Get(PrintExternalInvoiceAsOrderKey, true);
            UseProductionForPayments = Preferences.Get(UseProductionForPaymentsKey, false);
            UseCatalogWithFullTemplate = Preferences.Get(UseCatalogWithFullTemplateKey, false);
            MustSelectRouteToSync = Preferences.Get(MustSelectRouteToSyncKey, false);
            ShowLastThreeVisitsOnTemplate = Preferences.Get(ShowLastThreeVisitsOnTemplateKey, false);
            ForceSingleScan = Preferences.Get(ForceSingleScanKey, false);
            EnableUsernameandPassword = Preferences.Get(EnableUsernameandPasswordKey, false);
            SavePaymentsByInvoiceNumber = Preferences.Get(SavePaymentsByInvoiceNumberKey, false);
            SendPaymentsInEOD = Preferences.Get(SendPaymentsInEODKey, true);
            ShowExpensesInEOD = Preferences.Get(ShowExpensesInEODKey, false);
            DontCalculateOffersAfterPriceChanged = Preferences.Get(DontCalculateOffersAfterPriceChangedKey, false);
            RequireCodeForVoidInvoices = Preferences.Get(RequireCodeForVoidInvoicesKey, false);
            ShowPaymentSummary = Preferences.Get(ShowPaymentSummaryKey, false);
            CoolerCoCustomization = Preferences.Get(CoolerCoCustomizationKey, false);
            CanChangeRoutesOrder = Preferences.Get(CanChangeRoutesOrderKey, false);
            CalculateOffersAutomatically = Preferences.Get(CalculateOffersAutomaticallyKey, true);
            CalculateTaxPerLine = Preferences.Get(CalculateTaxPerLineKey, false);
            AddAllowanceToPriceDuringDEX = Preferences.Get(AddAllowanceToPriceDuringDEXKey, false);
            DontIncludePackageParameterDexUpc = Preferences.Get(DontIncludePackageParameterDexUpcKey, false);
            OnlyShowCostInProductDetails = Preferences.Get(OnlyShowCostInProductDetailsKey, false);
            DexUpcCharacterLimits = Preferences.Get(DexUpcCharacterLimitsKey, 14);
            DonNovoCustomization = Preferences.Get(DonNovoCustomizationKey, false);
            UseVisitsTemplateInSales = Preferences.Get(UseVisitsTemplateInSalesKey, false);
            AllowWorkOrder = Preferences.Get(AllowWorkOrderKey, false);
            NotifyNewerDataInOS = Preferences.Get(NotifyNewerDataInOSKey, false);
            AmericanEagleCustomization = Preferences.Get(AmericanEagleCustomizationKey, false);
            ShowSuggestedButton = Preferences.Get(ShowSuggestedButtonKey, false);
            DisableSendCatalogWithPrices = Preferences.Get(DisableSendCatalogWithPricesKey, false);
            RecalculateRoutesOnSyncData = Preferences.Get(RecalculateRoutesOnSyncDataKey, false);
            CanSelectTermsOnCreateClient = Preferences.Get(CanSelectTermsOnCreateClientKey, false);
            AlertPrintPaymentBeforeSaving = Preferences.Get(AlertPrintPaymentBeforeSavingKey, false);
            CatalogReturnsInDefaultUOM = Preferences.Get(CatalogReturnsInDefaultUOMKey, false);
            UseReturnOrder = Preferences.Get(UseReturnOrderKey, false);
            IncludeCreditInvoiceForPayments = Preferences.Get(IncludeCreditInvoiceForPaymentsKey, false);
            MarmiaCustomization = Preferences.Get(MarmiaCustomizationKey, false);
            NotificationsInSelfService = Preferences.Get(NotificationsInSelfServiceKey, true);
            CanDepositChecksWithDifDates = Preferences.Get(CanDepositChecksWithDifDatesKey, false);
            TemplateSearchByContains = Preferences.Get(TemplateSearchByContainsKey, false);
            AskOffersBeforeAdding = Preferences.Get(AskOffersBeforeAddingKey, false);
            UseLaceupDataInSalesReport = Preferences.Get(UseLaceupDataInSalesReportKey, false);
            ShowDescriptionInSelfServiceCatalog = Preferences.Get(ShowDescriptionInSelfServiceCatalogKey, false);
            CanChangeFinalizedInvoices = Preferences.Get(CanChangeFinalizedInvoicesKey, false);
            AllowNotifications = Preferences.Get(AllowNotificationsKey,
                DataAccess.CheckCommunicatorVersion(DataAccess.CommunicatorVersion, "52.2.0"));
            ShowLowestAcceptableOnWarning = Preferences.Get(ShowLowestAcceptableOnWarningKey, false);
            ShowPromoCheckbox = Preferences.Get(ShowPromoCheckboxKey, false);
            ShowImageInCatalog = Preferences.Get(ShowImageInCatalogKey, false);
            SoutoBottomEmailText = Preferences.Get(SoutoBottomEmailTextKey, string.Empty);
            BlockMultipleCollectPaymets = Preferences.Get(BlockMultipleCollectPaymetsKey, false);
            SelectWarehouseForSales = Preferences.Get(SelectWarehouseForSalesKey, false);
            CheckInventoryInLoad = Preferences.Get(CheckInventoryInLoadKey, false);
            CustomerInCreditHold = Preferences.Get(CustomerInCreditHoldKey, true);
            DeliveryMustScanProducts = Preferences.Get(DeliveryMustScanProductsKey, false);
            CanModifiyDeliveryWithScanning = Preferences.Get(CanModifiyDeliveryWithScanningKey, false);
            ShowSentTransactions = Preferences.Get(ShowSentTransactionsKey, true);
            MustSelectDepartment = Preferences.Get(MustSelectDepartmentKey, false);
            EnableAdvancedLogin = Preferences.Get(EnableAdvancedLoginKey, false);
            GenerateProjection = Preferences.Get(GenerateProjectionKey, false);
            PrintClientTotalOpenBalance = Preferences.Get(PrintClientTotalOpenBalanceKey, false);
            HidePONumber = Preferences.Get(HidePONumberKey, false);
            HidePriceInTransaction = Preferences.Get(HidePriceInTransactionKey, false);
            OnlyPresale = Preferences.Get(OnlyPresaleKey, false);
            LotIsMandatoryBeforeFinalize = Preferences.Get(LotIsMandatoryBeforeFinalizeKey, false);
            CheckDueInvoicesInCreateOrder = Preferences.Get(CheckDueInvoicesInCreateOrderKey, false);
            ConsignmentPresaleOnly = Preferences.Get(ConsignmentPresaleOnlyKey, false);
            TruckTemperatureReq = Preferences.Get(TruckTemperatureReqKey, false);
            MasterDevice = Preferences.Get(MasterDeviceKey, false);
            ConsignmentBeta = Preferences.Get(ConsignmentBetaKey, false);
            BetaConfigurationView = Preferences.Get(BetaConfigurationViewKey, false);
            SignedInSelfService = Preferences.Get(SignedInSelfServiceKey, true);
            SavedBanks = Preferences.Get(SavedBanksKey, string.Empty);
            DidCloseAlert = Preferences.Get(DidCloseAlertKey, false);
            HiddenItemCustomization = Preferences.Get(HiddenItemCustomizationKey, false);
            CheckDueInvoicesQtyInCreateOrder = Preferences.Get(CheckDueInvoicesQtyInCreateOrderKey, 0);
            AutomaticClockOutTime = Preferences.Get(AutomaticClockOutTimeKey, 0);
            MandatoryBreakDuration = Preferences.Get(MandatoryBreakDurationKey, 0);
            ForceBreakInMinutes = Preferences.Get(ForceBreakInMinutesKey, 0);
            OtherChargesType = Preferences.Get(OtherChargesTypeKey, 0);
            FreightType = Preferences.Get(FreightTypeKey, 0);
            OtherChargesComments = Preferences.Get(OtherChargesCommentsKey, string.Empty);
            FreightComments = Preferences.Get(FreightCommentsKey, string.Empty);
            OtherChargesVale = Preferences.Get(OtherChargesValeKey, 0);
            FreightVale = Preferences.Get(FreightValeKey, 0);
            UseBigFontForPrintDate = Preferences.Get(UseBigFontForPrintDateKey, false);
            Simone = Preferences.Get(SimoneKey, false);
            MinimumOrderProductId = Preferences.Get(MinimumOrderProductIdKey, 0);
            EnableLogin = Preferences.Get(EnableLoginKey, false);
            ZeroSoldInConsignment = Preferences.Get(ZeroSoldInConsignmentKey, false);
            SearchAllProductsInTemplate = Preferences.Get(SearchAllProductsInTemplateKey, false);
            AdvancedTemplateFocusSearch = Preferences.Get(AdvancedTemplateFocusSearchKey, false);
            MustSelectReasonForFreeItem = Preferences.Get(MustSelectReasonForFreeItemKey, false);
            CanEditCreditsInDelivery = Preferences.Get(CanEditCreditsInDeliveryKey, true);
            CanChangeSalesmanName = Preferences.Get(CanChangeSalesmanNameKey, false);
            PriceLevelComment = Preferences.Get(PriceLevelCommentKey, false);
            EnterWeightInCredits = Preferences.Get(EnterWeightInCreditsKey, false);
            ShowVisitsInfoInClients = Preferences.Get(ShowVisitsInfoInClientsKey, false);
            UpdateInventoryInPresale = Preferences.Get(UpdateInventoryInPresaleKey, false);
            SendTempPaymentsInBackground = Preferences.Get(SendTempPaymentsInBackgroundKey, false);
            DontGenerateLoadPrintedId = Preferences.Get(DontGenerateLoadPrintedIdKey, false);
            ShowBelow0InAdvancedTemplate = Preferences.Get(ShowBelow0InAdvancedTemplateKey, true);
            ShowRetailPriceForAddItem = Preferences.Get(ShowRetailPriceForAddItemKey, false);
            RoundTaxPerLine = Preferences.Get(RoundTaxPerLineKey, false);
            AllowExchange = Preferences.Get(AllowExchangeKey, false);
            UsePaymentDiscount = Preferences.Get(UsePaymentDiscountKey, false);
            OffersAddComment = Preferences.Get(OffersAddCommentKey, true);
            SendZplOrder = Preferences.Get(SendZplOrderKey, false);
            MultiplyConversionByCostInIracarAddItem =
                Preferences.Get(MultiplyConversionByCostInIracarAddItemKey, false);
            IncludeAvgWeightInCatalogPrice = Preferences.Get(IncludeAvgWeightInCatalogPriceKey, false);
            ShowOldReportsRegardless = Preferences.Get(ShowOldReportsRegardlessKey, false);
            CanLogout = Preferences.Get(CanLogoutKey, true);
            DontRoundInUI = Preferences.Get(DontRoundInUIKey, false);
            MustScanInTransfer = Preferences.Get(MustScanInTransferKey, false);
            TimeSheetAutomaticClockIn = Preferences.Get(TimeSheetAutomaticClockInKey, false);
            DontSortCompaniesByName = Preferences.Get(DontSortCompaniesByNameKey, false);
            PrintAllInventoriesInInvSummary = Preferences.Get(PrintAllInventoriesInInvSummaryKey, false);
            DicosaCustomization = Preferences.Get(DicosaCustomizationKey, false);
            CanModifyEnteredWeight = Preferences.Get(CanModifyEnteredWeightKey, false);
            ShowServiceReport = Preferences.Get(ShowServiceReportKey, false);
            RetailerAllowanceWins = Preferences.Get(RetailerAllowanceWinsKey, false);
            MilagroCustomization = Preferences.Get(MilagroCustomizationKey, false);
            AllowOtherCharges = Preferences.Get(AllowOtherChargesKey, false);
            CheckIfShipdateLocked = Preferences.Get(CheckIfShipdateLockedKey, false);
            GetUOMSOnCommand = Preferences.Get(GetUOMSOnCommandKey, true);
            SalesmanSeqValues = Preferences.Get(SalesmanSeqValuesKey, false);
            SalesmanSeqPrefix = Preferences.Get(SalesmanSeqPrefixKey, "");
            SalesmanSeqExpirationDate = new DateTime(Preferences.Get(SalesmanSeqExpirationDateKey, 0));
            SalesmanSeqFrom = Preferences.Get(SalesmanSeqFromKey, 0);
            SalesmanSeqTo = Preferences.Get(SalesmanSeqToKey, 0);
            SalesmanSelectedSite = Preferences.Get(SalesmanSelectedSiteKey, 0);

            ScanBasedTrading = Preferences.Get(ScanBasedTradingKey, false);
            SelfService = Preferences.Get(SelfServiceKey, false);
            BetaFragments = Preferences.Get(BetaFragmentsKey, false);

            salesPriceInSetting = Preferences.Get("salesPriceInSetting", false);
            creditsPriceInSetting = Preferences.Get("creditsPriceInSetting", false);

            SignedIn = Preferences.Get(SignedInKey, false);
            ProductCatalogViewType = Preferences.Get(ProductCatalogViewTypeKey, 0);
            ShouldGetPinBeforeSync = Preferences.Get(ShouldGetPinBeforeSyncKey, false);
            ButlerSignedIn = Preferences.Get(ButlerSignedInKey, false);
            SupervisorId = Preferences.Get(SupervisorIdKey, 0);
            DollyReminder = Preferences.Get(DollyReminderKey, false);
            GoToMain = Preferences.Get(GoToMainKey, false);
            UseFutureRouteEx = Preferences.Get(UseFutureRouteExKey, true);
            AutoCalculateRouteReturn = Preferences.Get(AutoCalculateRouteReturnKey, false);
            UseSurvey = Preferences.Get(UseSurveyKey, false);
            HideOpenInvoiceTotal = Preferences.Get(HideOpenInvoiceTotalKey, false);
            SalesHistoryForCredits = Preferences.Get(SalesHistoryForCreditsKey, false);
            UseCreditAccount = Preferences.Get(UseCreditAccountKey, false);
            HideTransactionsTotal = Preferences.Get(HideTransactionsTotalKey, false);
            HidePaymentsTotal = Preferences.Get(HidePaymentsTotalKey, false);
            HideSubTotalOrder = Preferences.Get(HideSubTotalOrderKey, false);
            SalesByDepartment = Preferences.Get(SalesByDepartmentKey, false);
            CreditReasonInLine = Preferences.Get(CreditReasonInLineKey, true);
            AcceptLoadEditable = Preferences.Get(AcceptLoadEditableKey, false);
            ShowProductsPerUoM = Preferences.Get(ShowProductsPerUoMKey, false);
            EnableCycleCount = Preferences.Get(EnableCycleCountKey, false);
            ViewPrintInvPassword = Preferences.Get(ViewPrintInvPasswordKey, string.Empty);
            CycleCountPassword = Preferences.Get(CycleCountPasswordKey, string.Empty);
            BranchSiteId = Preferences.Get(BranchSiteIdKey, 0);
            MustAddImageToFinalizedCredit = Preferences.Get(MustAddImageToFinalizedCreditKey, false);
            SiteId = Preferences.Get(SiteIdKey, 0);
            CalculateInvForEmptyTruck = Preferences.Get(CalculateInvForEmptyTruckKey, true);
            MustGenerateProjection = Preferences.Get(MustGenerateProjectionKey, true);
            LaceupVersion = Preferences.Get(LaceupVersionKey, 0);
            LoadByOrderHistory = Preferences.Get(LoadByOrderHistoryKey, false);
            EmptyEndingInventory = Preferences.Get(EmptyEndingInventoryKey, false);
            DefaultItemHasPrice = Preferences.Get(DefaultItemHasPriceKey, false);
            BonsuisseCustomization = Preferences.Get(BonsuisseCustomizationKey, false);
            NewSyncLoadOnDemand = Preferences.Get(NewSyncLoadOnDemandKey, false);
            UseBiggerUoMInLoadHistory = Preferences.Get(UseBiggerUoMInLoadHistoryKey, false);
            TotalsByUoMInPdf = Preferences.Get(TotalsByUoMInPdfKey, false);
            SplitDeliveryByDepartment = Preferences.Get(SplitDeliveryByDepartmentKey, false);
            RecalculateStops = Preferences.Get(RecalculateStopsKey, true);
            CatalogQuickAdd = Preferences.Get(CatalogQuickAddKey, false);
            TransferPasswordAtSaving = Preferences.Get(TransferPasswordAtSavingKey, false);
            OpenTemplateEmptyByDefault = Preferences.Get(OpenTemplateEmptyByDefaultKey, false);
            DeletePaymentsInTab = Preferences.Get(DeletePaymentsInTabKey, false);
            MustAddImageToFinalized = Preferences.Get(MustAddImageToFinalizedKey, false);
            AlwaysFreshCustomization = Preferences.Get(AlwaysFreshCustomizationKey, false);
            ShowOHQtyInSelfService = Preferences.Get(ShowOHQtyInSelfServiceKey, false);
            ShowProfitWhenChangingPrice = Preferences.Get(ShowProfitWhenChangingPriceKey, false);
            ShowOrderStatus = Preferences.Get(ShowOrderStatusKey, false);
            BringBranchInventories = Preferences.Get(BringBranchInventoriesKey, false);
            HideContactName = Preferences.Get(HideContactNameKey, false);
            PrintPaymentRegardless = Preferences.Get(PrintPaymentRegardlessKey, false);
            GeorgeHoweCustomization = Preferences.Get(GeorgeHoweCustomizationKey, false);
            PrintInveSettlementRegardless = Preferences.Get(PrintInveSettlementRegardlessKey, false);
            CanRestoreFromFile = Preferences.Get(CanRestoreFromFileKey, false);
            DaysToShowSentOrders = Preferences.Get(DaysToShowSentOrdersKey, 15);
            MaxDiscountPerOrder = Preferences.Get(MaxDiscountPerOrderKey, 0);
            DaysToBringOrderStatus = Preferences.Get(DaysToBringOrderStatusKey, 7);
            DaysToRunReports = Preferences.Get(DaysToRunReportsKey, 0);
            LowestPriceLevelId = Preferences.Get(LowestPriceLevelIdKey, 0);
            CanChangeUomInCredits = Preferences.Get(CanChangeUomInCreditsKey, true);
            RequestAuthPinForLogin = Preferences.Get(RequestAuthPinForLoginKey, false);
            LoginTimeOut = Preferences.Get(LoginTimeOutKey, 8);
            PaymentBankIsMandatory = Preferences.Get(PaymentBankIsMandatoryKey, false);
            OrderCanBeChanged = Preferences.Get(OrderCanBeChangedKey, true);
            CreditCanBeChanged = Preferences.Get(CreditCanBeChangedKey, true);
            OrderMinimumQty = Preferences.Get(OrderMinimumQtyKey, 0);
            OrderMinQtyMinAmount = Preferences.Get(OrderMinQtyMinAmountKey, true);
            ProductMinQty = Preferences.Get(ProductMinQtyKey, true);
            MaxQtyInOrder = Preferences.Get(MaxQtyInOrderKey, 0);
            OrderMinimumTotalPrice = Preferences.Get(OrderMinimumTotalPriceKey, 0);
            OrderHistoryByClient = Preferences.Get(OrderHistoryByClientKey, false);
            PrintInvoiceAsReceipt = Preferences.Get(PrintInvoiceAsReceiptKey, false);
            HidePrintBatch = Preferences.Get(HidePrintBatchKey, false);
            ShowInvoicesCreditsInPayments = Preferences.Get(ShowInvoicesCreditsInPaymentsKey, false);
            CanChangeUoMInTransfer = Preferences.Get(CanChangeUoMInTransferKey, true);
            TransferScanningAddsProduct = Preferences.Get(TransferScanningAddsProductKey, true);
            SensationalAssetTracking = Preferences.Get(SensationalAssetTrackingKey, false);
            PrintRefusalReportByStore = Preferences.Get(PrintRefusalReportByStoreKey, false);
            VoidPayments = Preferences.Get(VoidPaymentsKey, false);
            SpectrumFloralCustomization = Preferences.Get(SpectrumFloralCustomizationKey, false);
            CanModifyQuotes = Preferences.Get(CanModifyQuotesKey, false);
            ShowDiscountIfApplied = Preferences.Get(ShowDiscountIfAppliedKey, false);
            ShowReports = Preferences.Get(ShowReportsKey, true);
            AllowToCollectInvoices = Preferences.Get(AllowToCollectInvoicesKey, true);
            PrintNoServiceInSalesReports = Preferences.Get(PrintNoServiceInSalesReportsKey, true);
            XlsxProvider = Preferences.Get(XlsxProviderKey, string.Empty);
            SelfServiceInvitation = Preferences.Get(SelfServiceInvitationKey, 0);
            CarolinaCustomization = Preferences.Get(CarolinaCustomizationKey, false);
            IncludePresaleInSalesReport = Preferences.Get(IncludePresaleInSalesReportKey, false);
            SelectSalesRepForInvoice = Preferences.Get(SelectSalesRepForInvoiceKey, false);
            AssetTracking = Preferences.Get(AssetTrackingKey, false);
            DeliveryEditable = Preferences.Get(DeliveryEditableKey, true);
            DisablePrintEndOfDayReport = Preferences.Get(DisablePrintEndOfDayReportKey, false);
            HideWarehouseOHInLoad = Preferences.Get(HideWarehouseOHInLoadKey, false);
            PONumberMaxLength = Preferences.Get(PONumberMaxLengthKey, 0);
            UseLotExpiration = Preferences.Get(UseLotExpirationKey, false);
            DisolCrap = Preferences.Get(DisolCrapKey, false);
            ItemGroupedTemplate = Preferences.Get(ItemGroupedTemplateKey, false);
            WarehouseInventoryOnDemand = Preferences.Get(WarehouseInventoryOnDemandKey, false);
            AssetStaysMandatory = Preferences.Get(AssetStaysMandatoryKey, false);
            AddRelatedItemInCredit = Preferences.Get(AddRelatedItemInCreditKey, true);
            UseOffersInCredit = Preferences.Get(UseOffersInCreditKey, false);
            UseLaceupAdvancedCatalog = Preferences.Get(UseLaceupAdvancedCatalogKey, false);
            SelfServiceUser = Preferences.Get(SelfServiceUserKey, false);
            UseClientSort = Preferences.Get(UseClientSortKey, false);
            UseDisolSurvey = Preferences.Get(UseDisolSurveyKey, false);
            DisolSurveyProducts = Preferences.Get(DisolSurveyProductsKey, "1172,1158,1163,1164");
            ShowListPriceInAddItem = Preferences.Get(ShowListPriceInAddItemKey, false);
            CanChangeUomInLoad = Preferences.Get(CanChangeUomInLoadKey, true);
            IracarCustomization = Preferences.Get(IracarCustomizationKey, false);
            NewAddItemRandomWeight = Preferences.Get(NewAddItemRandomWeightKey, false);
            SetShipDate = Preferences.Get(SetShipDateKey, true);
            LockOrderAfterPrinted = Preferences.Get(LockOrderAfterPrintedKey, false);
            PackageInReturnPresale = Preferences.Get(PackageInReturnPresaleKey, false);
            EnableSampleOrder = Preferences.Get(EnableSampleOrderKey, false);
            EnablePaymentsByTerms = Preferences.Get(EnablePaymentsByTermsKey, "");
            RequireLotForDumps = Preferences.Get(RequireLotForDumpsKey, false);
            HidePriceInSelfService = Preferences.Get(HidePriceInSelfServiceKey, true);
            HideOHinSelfService = Preferences.Get(HideOHinSelfServiceKey, true);
            ShowWarehouseInvInSummary = Preferences.Get(ShowWarehouseInvInSummaryKey, false);
            RouteReturnIsMandatory = Preferences.Get(RouteReturnIsMandatoryKey, true);
            EndingInvIsMandatory = Preferences.Get(EndingInvIsMandatoryKey, true);
            CasaSanchezCustomization = Preferences.Get(CasaSanchezCustomizationKey, false);
            ViewAllComments = Preferences.Get(ViewAllCommentsKey, false);
            ShowAllAvailableLoads = Preferences.Get(ShowAllAvailableLoadsKey, false);
            AddInventoryOnPo = Preferences.Get(AddInventoryOnPoKey, false);
            UseRetailPriceForSales = Preferences.Get(UseRetailPriceForSalesKey, false);
            ProductNameHasDifferentColor = Preferences.Get(ProductNameHasDifferentColorKey, false);
            ShowBillOfLadingPdf = Preferences.Get(ShowBillOfLadingPdfKey, false);
            StartingPercentageBasedOnCost = Preferences.Get(StartingPercentageBasedOnCostKey, 0);
            GroupClientsByCat = Preferences.Get(GroupClientsByCatKey, string.Empty);
            ExtraInfoBottomPrint = Preferences.Get(ExtraInfoBottomPrintKey, string.Empty);
            ProductCategoryNameIdentifier = Preferences.Get(ProductCategoryNameIdentifierKey, "Suggested");
            SupervisorEmailForRequests = Preferences.Get(SupervisorEmailForRequestsKey, string.Empty);
            SortClient = Preferences.Get(SortClientKey, string.Empty);
            TrackTermsPaymentBotton = Preferences.Get(TrackTermsPaymentBottonKey, string.Empty);
            SalesReportTotalCreditsSubstracted = Preferences.Get(SalesReportTotalCreditsSubstractedKey, false);
            BarcodeDecoder = Preferences.Get(BarcodeDecoderKey, 1);
            PrintLabelHeight = Preferences.Get(PrintLabelHeightKey, "");
            AllowSelectPriceLevel = Preferences.Get(AllowSelectPriceLevelKey, false);
            UpdateInventoryRegardless = Preferences.Get(UpdateInventoryRegardlessKey, false);
            ShowOnlyInvoiceForSalesman = Preferences.Get(ShowOnlyInvoiceForSalesmanKey, false);
            HideInvoicesAndBalance = Preferences.Get(HideInvoicesAndBalanceKey, false);
            ImageInOrderMandatory = Preferences.Get(ImageInOrderMandatoryKey, false);
            ImageInNoServiceMandatory = Preferences.Get(ImageInNoServiceMandatoryKey, false);
            PrintCopiesInFinalizeBatch = Preferences.Get(PrintCopiesInFinalizeBatchKey, 2);
            WeeksOfSalesHistory = Preferences.Get(WeeksOfSalesHistoryKey, 2);
            DaysOfProjectionInTemplate = Preferences.Get(DaysOfProjectionInTemplateKey, 0);
            UseFastPrinter = Preferences.Get(UseFastPrinterKey, false);
            ParLevelHistoryNumVisit = Preferences.Get(ParLevelHistoryNumVisitKey, 0);
            AutoGenerateLoadOrder = Preferences.Get(AutoGenerateLoadOrderKey, false);
            ApplyDiscountAfterTaxes = Preferences.Get(ApplyDiscountAfterTaxesKey, false);

            var autoEndInventory = Preferences.Get(AutoEndInventoryKey, false);
            if (autoEndInventory)
            {
                RouteReturnIsMandatory = false;
                EndingInvIsMandatory = false;
            }
        }

        public static void ClearSettings()
        {
            Config.SaveSettings();
            Config.Initialize();
        }

        public static void SaveLastEndOfDay()
        {
            Preferences.Set("EndOfDayDate", DateTime.Now.Ticks);
        }

        public static DateTime GetLastEndOfDay()
        {
            long l = Preferences.Get("EndOfDayDate", 0L);
            if (l == 0) return DateTime.MinValue;
            return new DateTime(l);
        }

        public static void SaveAppStatus()
        {
            Preferences.Set("PendingLoadToAccept", DataAccess.PendingLoadToAccept ? 1 : 0);
            Preferences.Set("ReceivedData", DataAccess.ReceivedData ? 1 : 0);
            Preferences.Set("RouteOrdersCount", DataAccess.RouteOrdersCount);
            Preferences.Set("CommunicatorVersion", DataAccess.CommunicatorVersion);
            Preferences.Set("WaitBeforeStart", DataAccess.WaitBeforeStart);
            Preferences.Set("LastEndOfDay", DataAccess.LastEndOfDay.Ticks);
            Preferences.Set("AcceptInventoryReadOnly", DataAccess.AcceptInventoryReadOnly ? 1 : 0);
        }

        private static void LoadAppStatus()
        {
            DataAccess.PendingLoadToAccept = Preferences.Get("PendingLoadToAccept", 0) > 0;
            DataAccess.ReceivedData = Preferences.Get("ReceivedData", 0) > 0;
            DataAccess.RouteOrdersCount = Preferences.Get("RouteOrdersCount", 0);
            DataAccess.CommunicatorVersion = Preferences.Get("CommunicatorVersion", string.Empty);
            DataAccess.WaitBeforeStart = Preferences.Get("WaitBeforeStart", 5);
            DataAccess.LastEndOfDay = new DateTime(Preferences.Get("LastEndOfDay", DateTime.MinValue.Ticks));
            DataAccess.AcceptInventoryReadOnly = Preferences.Get("AcceptInventoryReadOnly", 0) > 0;
        }

        public static void SavePresaleLastOrderId()
        {
            Preferences.Set(LastPresaleOrderIdKey, LastPresalePrintedId);
        }

        public static void SaveLastOrderId()
        {
            Preferences.Set(LastOrderIdKey, LastPrintedId);
        }

        public static void SaveSettings()
        {
            //vendor name
            var salesman = Salesman.List.FirstOrDefault(x => x.Id == SalesmanId);
            if (salesman != null && !Config.CanChangeSalesmanName) VendorName = salesman.Name;

            Preferences.Set(IgnoreDiscountInCreditsKey, IgnoreDiscountInCredits);
            Preferences.Set(CheckInventoryInPreSaleKey, CheckInventoryInPreSale);
            Preferences.Set(AlwaysUpdateNewParKey, AlwaysUpdateNewPar);
            Preferences.Set(ShipDateIsMandatoryKey, ShipDateIsMandatory);
            Preferences.Set(ShipDateIsMandatoryForLoadKey, ShipDateIsMandatoryForLoad);
            Preferences.Set(PresaleShipDateKey, PresaleShipDate);
            Preferences.Set(CoreAsCreditKey, CoreAsCredit);
            Preferences.Set(PrintClientOpenBalanceKey, PrintClientOpenBalance);
            Preferences.Set(CreditTemplateKey, CreditTemplate);
            Preferences.Set(AskDEXUoMKey, AskDEXUoM);
            Preferences.Set(WstcoKey, Wstco);
            Preferences.Set(UseLSPByDefaultKey, UseLSPByDefault);
            Preferences.Set(TimeSheetCustomizationKey, TimeSheetCustomization);
            Preferences.Set(GenerateValuesLoadOrderKey, GenerateValuesLoadOrder);
            Preferences.Set(ConsignmentShow0Key, ConsignmentShow0);
            Preferences.Set(ConsignmentMustCountAllKey, ConsignmentMustCountAll);
            Preferences.Set(ConsignmentKeepPriceKey, ConsignmentKeepPrice);
            Preferences.Set(PrintInvoiceNumberDownKey, PrintInvoiceNumberDown);
            Preferences.Set(AddQtyTotalRegardlessUoMKey, AddQtyTotalRegardlessUoM);
            Preferences.Set(ShowProductsWith0InventoryKey, ShowProductsWith0Inventory);
            Preferences.Set(DisplayTaxOnCatalogAndPrintKey, DisplayTaxOnCatalogAndPrint);
            Preferences.Set(PrasekCustomizationKey, PrasekCustomization);
            Preferences.Set(AlwaysShowDefaultUoMKey, AlwaysShowDefaultUoM);
            Preferences.Set(ButlerCustomizationKey, ButlerCustomization);
            Preferences.Set(ShowPrintPickTicketKey, ShowPrintPickTicket);
            Preferences.Set(CaribCustomizationKey, CaribCustomization);
            Preferences.Set(PrintFromClientDetailKey, PrintFromClientDetail);
            Preferences.Set(ConcatUPCToNameKey, ConcatUPCToName);
            Preferences.Set(PrintUPCInventoryKey, PrintUPCInventory);
            Preferences.Set(PrintUPCOpenInvoicesKey, PrintUPCOpenInvoices);
            Preferences.Set(PrintLotOrderKey, PrintLotOrder);
            Preferences.Set(ExtraTextForPrinterKey, ExtraTextForPrinter);
            Preferences.Set(PrintLotPreOrderKey, PrintLotPreOrder);
            Preferences.Set(CanVoidFOrdersKey, CanVoidFOrders);
            Preferences.Set(BetaFeaturesKey, BetaFeatures);
            Preferences.Set(MustBeOnlineAlwaysKey, MustBeOnlineAlways);
            Preferences.Set(EnableLiveDataKey, EnableLiveData);
            Preferences.Set(GroupRelatedWhenPrintingKey, GroupRelatedWhenPrinting);
            Preferences.Set(MustEndOrdersKey, MustEndOrders);
            Preferences.Set(ProductInMultipleCategoryKey, ProductInMultipleCategory);
            Preferences.Set(AllowEditRelatedKey, AllowEditRelated);
            Preferences.Set(ExtendedPaymentOptionsKey, ExtendedPaymentOptions);
            Preferences.Set(HideTotalOrderKey, HideTotalOrder);
            Preferences.Set(HideTotalInPrintedLineKey, HideTotalInPrintedLine);
            Preferences.Set(PrintCopyKey, PrintCopy);
            Preferences.Set(ConsignmentKey, Consignment);
            Preferences.Set(MustEndOfDayDailyKey, MustEndOfDayDaily);
            Preferences.Set(ForceEODWhenDateChangesKey, ForceEODWhenDateChanges);
            Preferences.Set(UsesTermsKey, UsesTerms);
            Preferences.Set(UseReshipKey, UseReship);
            Preferences.Set(UseClockInOutKey, UseClockInOut);
            Preferences.Set(ShowBillOfLadingPdfKey, ShowBillOfLadingPdf);
            Preferences.Set(OldPrinterKey, OldPrinter);
            Preferences.Set(DexVersionKey, DexVersion);
            Preferences.Set(DexDefaultUnitKey, DexDefaultUnit);
            Preferences.Set(UpdateWhenEndDayKey, UpdateWhenEndDay);
            Preferences.Set(AutoAcceptLoadKey, AutoAcceptLoad);
            Preferences.Set(RemoveWarningsKey, RemoveWarnings);
            Preferences.Set(GroupLinesWhenPrintingKey, GroupLinesWhenPrinting);
            Preferences.Set(ShortInventorySettlementKey, ShortInventorySettlement);
            Preferences.Set(PrintReportsRequiredKey, PrintReportsRequired);
            Preferences.Set(LoadOrderEmptyKey, LoadOrderEmpty);
            Preferences.Set(CanLeaveBatchKey, CanLeaveBatch);
            Preferences.Set(PrintUpcAsTextKey, PrintUpcAsText);
            Preferences.Set(InvoicePrefixKey, InvoicePrefix);
            Preferences.Set(SurveyQuestionsKey, SurveyQuestions);
            Preferences.Set(LastSignInKey, LastSignIn);
            Preferences.Set(InvoicePresalePrefixKey, InvoicePresalePrefix);
            Preferences.Set(AutoEndInventoryPasswordKey, AutoEndInventoryPassword);
            Preferences.Set(CremiMexDepartmentsKey, CremiMexDepartments);
            Preferences.Set(BackGroundSyncKey, BackGroundSync);
            Preferences.Set(ShowInvoiceTotalKey, ShowInvoiceTotal);
            Preferences.Set(AddDefaultItemToCreditKey, AddDefaultItemToCredit);
            Preferences.Set(DefaultItemKey, DefaultItem);
            Preferences.Set(DistanceToOrderKey, DistanceToOrder);
            Preferences.Set(ProductFileCreationDateKey, ProductFileCreationDate);
            Preferences.Set(AcceptLoadPrintRequiredKey, AcceptLoadPrintRequired);
            Preferences.Set(MustPrintPreOrderKey, MustPrintPreOrder);
            Preferences.Set(LoadLotInTransferKey, LoadLotInTransfer);
            Preferences.Set(LoadRequestKey, LoadRequest);
            Preferences.Set(LoadRequiredKey, LoadRequired);
            Preferences.Set(EmptyTruckAtEndOfDayKey, EmptyTruckAtEndOfDay);
            Preferences.Set(LeftOrderTemplateEmptyKey, LeftOrderTemplateEmpty);
            Preferences.Set(CanIncreasePriceKey, CanIncreasePrice);
            Preferences.Set(DeliveryKey, Delivery);
            Preferences.Set(PickCompanyKey, PickCompany);
            Preferences.Set(ProductCatalogKey, ProductCatalog);
            Preferences.Set(PreSaleKey, PreSale);
            Preferences.Set(SendLogByEmailKey, SendLogByEmail);
            Preferences.Set(EmailOrderKey, EmailOrder);
            Preferences.Set(FakePreOrderKey, FakePreOrder);
            Preferences.Set(PrintTruncateNamesKey, PrintTruncateNames);
            Preferences.Set(PopulateTemplateAuthProdKey, PopulateTemplateAuthProd);
            Preferences.Set(OneDocKey, OneDoc);
            Preferences.Set(CanAddClientKey, CanAddClient);
            Preferences.Set(SetPOKey, SetPO);
            Preferences.Set(InvReqKey, InventoryRequestEmail);
            Preferences.Set(CanGoBelow0Key, CanGoBelow0);
            Preferences.Set(NoPriceChangeDeliveriesKey, NoPriceChangeDeliveries);
            Preferences.Set(WarningDumpReturnKey, WarningDumpReturn);
            Preferences.Set(LastOrderIdKey, LastPrintedId);
            Preferences.Set(UseOrderIdKey, UseOrderId);
            Preferences.Set(RoundKey, Round);
            Preferences.Set(DexKey, DexAvailable);
            Preferences.Set(PrintUPCKey, PrintUPC);
            Preferences.Set(CanChangeSalesmanIdKey, CanChangeSalesmanId);
            Preferences.Set(BottomOrderPrintTextKey, BottomOrderPrintText);
            Preferences.Set(SingleScanStrokeKey, SingleScanStroke);
            Preferences.Set(PrintInvoiceSortKey, PrintInvoiceSort);
            Preferences.Set(PrintClientSortKey, PrintClientSort);
            Preferences.Set(CompanyLogoKey, CompanyLogo ?? String.Empty);
            Preferences.Set(CompanyLogoHeightKey, CompanyLogoHeight);
            Preferences.Set(CompanyLogoWidthKey, CompanyLogoWidth);
            Preferences.Set(CompanyLogoSizeKey, CompanyLogoSize);
            Preferences.Set(MustSendOrdersFirstKey, MustSendOrdersFirst);
            Preferences.Set(AllowDiscountKey, AllowDiscount);
            Preferences.Set(LocationIsMandatoryKey, LocationIsMandatory);
            Preferences.Set(AlwaysAddItemsToOrderKey, AlwaysAddItemsToOrder);
            Preferences.Set(FreeItemsNeedCommentsKey, FreeItemsNeedComments);
            Preferences.Set(LotIsMandatoryKey, LotIsMandatory);
            Preferences.Set(PaymentAvailableKey, PaymentAvailable);
            Preferences.Set(DisablePaymentIfTermDaysMoreThan0Key, DisablePaymentIfTermDaysMoreThan0);
            Preferences.Set(PaymentRequiredKey, PaymentRequired);
            Preferences.Set(PrinterToUseKey, PrinterToUse);
            Preferences.Set(InvoiceIdKey, InvoiceIdProvider);
            Preferences.Set(SignatureRequiredKey, SignatureRequired);
            Preferences.Set(LotKey, UseLot);
            Preferences.Set(FakeUseLotKey, FakeUseLot);
            Preferences.Set(DisplayPurchasePriceKey, DisplayPurchasePrice);
            Preferences.Set(AnyPriceIsAcceptableKey, AnyPriceIsAcceptable);
            Preferences.Set(UseLSPKey, UseLSP);

            Preferences.Set(DaysToKeepOrderKey, DaysToKeepOrder);
            Preferences.Set(TransferPasswordKey, TransferPassword ?? string.Empty);
            Preferences.Set(TransferOffPasswordKey, TransferOffPassword ?? string.Empty);
            Preferences.Set(InventoryPasswordKey, InventoryPassword ?? string.Empty);
            Preferences.Set(AddInventoryPasswordKey, AddInventoryPassword ?? string.Empty);
            Preferences.Set(TrackInventoryKey, TrackInventory);
            Preferences.Set(CanModifyInventoryKey, CanModifyInventory);
            Preferences.Set(PrinterAvailableKey, PrinterAvailable);
            Preferences.Set(PrintingRequiredKey, PrintingRequired);
            Preferences.Set(AllowCreditOrdersKey, AllowCreditOrders);
            Preferences.Set(VendorIdKey, SalesmanId);
            Preferences.Set(RouteNameKey, RouteName);
            Preferences.Set(VendorNameKey, VendorName);
            Preferences.Set(PortKey, Port);
            Preferences.Set(IPAddressGatewayKey, IPAddressGateway);
            Preferences.Set(LanKey, LanAddress);
            Preferences.Set(SSIDKey, SSID);
            Preferences.Set(MustUpdateDailyKey, MustUpdateDaily);
            Preferences.Set(AllowFreeItemsKey, AllowFreeItems);
            Preferences.Set(SendLoadOrderKey, SendLoadOrder);
            Preferences.Set(UseLocationKey, UseLocation);
            Preferences.Set(AllowOrderForClientOverCreditLimitKey, AllowOrderForClientOverCreditLimit);
            Preferences.Set(UserCanChangePricesKey, UserCanChangePrices);
            Preferences.Set(ViewProviderAndroidKey, DataAccess.ActivityProvider.SerializeToString());
            Preferences.Set(ScannerToUseKey, ScannerToUse);
            Preferences.Set(UseUpcCheckDigitKey, UseUpcCheckDigit);
            Preferences.Set(RouteManagementKey, RouteManagement);
            Preferences.Set(SetParLevelKey, SetParLevel);
            Preferences.Set(UseUpc128Key, UseUpc128);
            Preferences.Set(DeliveryScanningKey, DeliveryScanning);
            Preferences.Set(UseBatteryKey, UseBattery);
            Preferences.Set(NewConsPrinterKey, NewConsPrinter);
            Preferences.Set(UseOldEmailFormatKey, UseOldEmailFormat);
            Preferences.Set(PrintedIdLengthKey, PrintedIdLength);
            Preferences.Set(HideSetConsignmentKey, HideSetConsignment);
            Preferences.Set(AutoGeneratePOKey, AutoGeneratePO);
            Preferences.Set(ExtraSpaceForSignatureKey, ExtraSpaceForSignature);
            Preferences.Set(DefaultCreditDetTypeKey, DefaultCreditDetType);
            Preferences.Set(CanSelectSalesmanKey, CanSelectSalesman);
            Preferences.Set(UsePairLotQtyKey, UsePairLotQty);
            Preferences.Set(UseSendByEmailKey, UseSendByEmail);
            Preferences.Set(UsePrintProofDeliveryKey, UsePrintProofDelivery);
            Preferences.Set(HideCompanyInfoPrintKey, HideCompanyInfoPrint);
            Preferences.Set(PrintIsMandatoryKey, PrintIsMandatory);
            Preferences.Set(SelectDriverFromPresaleKey, SelectDriverFromPresale);
            Preferences.Set(MustCompleteRouteKey, MustCompleteRoute);
            Preferences.Set(Discount100PercentPrintTextKey, Discount100PercentPrintText);
            Preferences.Set(RemovePayBalFomInvoiceKey, RemovePayBalFomInvoice);
            Preferences.Set(AddRelatedItemsInTotalKey, AddRelatedItemsInTotal);
            Preferences.Set(SignatureNameRequiredKey, SignatureNameRequired);
            Preferences.Set(PrintZeroesOnPickSheetKey, PrintZeroesOnPickSheet);
            Preferences.Set(AddCoresInSalesItemKey, AddCoresInSalesItem);
            Preferences.Set(UseAllowanceKey, UseAllowance);
            Preferences.Set(IncludeDeliveriesInLoadOrderKey, IncludeDeliveriesInLoadOrder);
            Preferences.Set(UseReturnInvoiceKey, UseReturnInvoice);
            Preferences.Set(AddItemInDefaultUoMKey, AddItemInDefaultUoM);
            Preferences.Set(UseTermsInLoadOrderKey, UseTermsInLoadOrder);
            Preferences.Set(POIsMandatoryKey, POIsMandatory);
            Preferences.Set(NewClientCanChangePricesKey, NewClientCanChangePrices);
            Preferences.Set(PrintNetQtyKey, PrintNetQty);
            Preferences.Set(AuthProdsInCreditKey, AuthProdsInCredit);
            Preferences.Set(MagnoliaSetConsignmentKey, MagnoliaSetConsignment);
            Preferences.Set(SyncLoadOnDemandKey, SyncLoadOnDemand);
            Preferences.Set(MasterLoadOrderKey, MasterLoadOrder);
            Preferences.Set(SalesRegReportWithTaxKey, SalesRegReportWithTax);
            Preferences.Set(TransferCommentKey, TransferComment);
            Preferences.Set(AddCoreBalancekey, AddCoreBalance);
            Preferences.Set(HideItemCommentKey, HideItemComment);
            Preferences.Set(SendByEmailInFinalizeKey, SendByEmailInFinalize);
            Preferences.Set(HideInvoiceCommentKey, HideInvoiceComment);
            Preferences.Set(IncludeBatteryInLoadKey, IncludeBatteryInLoad);
            Preferences.Set(ClientDailyPLKey, ClientDailyPL);
            Preferences.Set(BackgroundTimeKey, BackgroundTime);
            Preferences.Set(ShowAvgInCatalogKey, ShowAvgInCatalog);
            Preferences.Set(DisableRouteReturnKey, DisableRouteReturn);
            Preferences.Set(AllowAdjPastExpDateKey, AllowAdjPastExpDate);
            Preferences.Set(ConsignmentContractTextKey, ConsignmentContractText);
            Preferences.Set(SendBackgroundOrdersKey, SendBackgroundOrders);
            Preferences.Set(HidePriceInPrintedLineKey, HidePriceInPrintedLine);
            Preferences.Set(SendOrderIsMandatoryKey, SendOrderIsMandatory);
            Preferences.Set(PaymentOrSignatureRequiredKey, PaymentOrSignatureRequired);
            Preferences.Set(DoNotShrinkOrderImageKey, DoNotShrinkOrderImage);
            Preferences.Set(MustEnterCaseInOutKey, MustEnterCaseInOut);
            Preferences.Set(ParLevelHistoryDaysKey, ParLevelHistoryDays);
            Preferences.Set(AddSalesInConsignmentKey, AddSalesInConsignment);
            Preferences.Set(CanChangeUoMKey, CanChangeUoM);
            Preferences.Set(UseClientClassAsCompanyNameKey, UseClientClassAsCompanyName);
            Preferences.Set(PdfProviderKey, PdfProvider);
            Preferences.Set(NewClientEmailRequiredKey, NewClientEmailRequired);
            Preferences.Set(NewClientExtraFieldsKey, NewClientExtraFields);
            Preferences.Set(UseFullConsignmentKey, UseFullConsignment);
            Preferences.Set(RTNKey, RTN);
            Preferences.Set(BillNumRequiredKey, BillNumRequired);
            Preferences.Set(DefaultTaxRateKey, DefaultTaxRate);
            Preferences.Set(UseLastUoMKey, UseLastUoM);
            Preferences.Set(ShowAddrInClientListKey, ShowAddrInClientList);
            Preferences.Set(SalesmanInCreditDelKey, SalesmanInCreditDel);
            Preferences.Set(ClientNameMaxSizeKey, ClientNameMaxSize);
            Preferences.Set(MinShipDateDaysKey, MinShipDateDays);
            Preferences.Set(HidePrintedCommentLineKey, HidePrintedCommentLine);
            Preferences.Set(UseDraggableTemplateKey, UseDraggableTemplate);
            Preferences.Set(PrintBillShipDateKey, PrintBillShipDate);
            Preferences.Set(DaysToKeepSignaturesKey, DaysToKeepSignatures);
            Preferences.Set(BlackStoneConsigCustomKey, BlackStoneConsigCustom);
            Preferences.Set(HideProdOnHandKey, HideProdOnHand);
            Preferences.Set(PrintInvSettReportKey, PrintInvSettReport);
            Preferences.Set(PreSaleConsigmentKey, PreSaleConsigment);
            Preferences.Set(UseConsignmentLotKey, UseConsignmentLot);
            Preferences.Set(SendBackgroundBackupKey, SendBackgroundBackup);
            Preferences.Set(MinimumWeightkey, MinimumWeight);
            Preferences.Set(Minimumamountkey, MinimumAmount);
            Preferences.Set(PresaleCommMandatoryKey, PresaleCommMandatory);
            Preferences.Set(PrintTaxLabelKey, PrintTaxLabel);
            Preferences.Set(AllowDiscountPerLineKey, AllowDiscountPerLine);
            Preferences.Set(HideVoidButtonKey, HideVoidButton);
            Preferences.Set(ConsLotAsDateKey, ConsLotAsDate);
            Preferences.Set(DisolCustomIdGeneratorKey, DisolCustomIdGenerator);
            Preferences.Set(ParInConsignmentKey, ParInConsignment);
            Preferences.Set(AddCreditInConsignmentKey, AddCreditInConsignment);
            Preferences.Set(ShowShipViaKey, ShowShipVia);
            Preferences.Set(ShipViaMandatoryKey, ShipViaMandatory);
            Preferences.Set(GeneratePreorderNumKey, GeneratePreorderNum);
            Preferences.Set(OnlyKitInCreditKey, OnlyKitInCredit);
            Preferences.Set(DeliveryReasonInLineKey, DeliveryReasonInLine);
            Preferences.Set(AlwaysUpdateNewParKey, AlwaysUpdateNewPar);
            Preferences.Set(CanModifyConnectSettKey, CanModifyConnectSett);
            Preferences.Set(LspInAllLinesKey, LspInAllLines);
            Preferences.Set(UseAllDayParLevelKey, UseAllDayParLevel);
            Preferences.Set(CloseRouteInPresaleKey, CloseRouteInPresale);
            Preferences.Set(EditParInHistoryKey, EditParInHistory);
            Preferences.Set(AlwaysCountInParKey, AlwaysCountInPar);
            Preferences.Set(WarrantyPerClientKey, WarrantyPerClient);
            Preferences.Set(ChargeBatteryRotationKey, ChargeBatteryRotation);
            Preferences.Set(IncludeRotationInDeliveryKey, IncludeRotationInDelivery);
            Preferences.Set(ConsParFirstInPresaleKey, ConsParFirstInPresale);
            Preferences.Set(AverageSaleInParLevelKey, AverageSaleInParLevel);
            Preferences.Set(MultipleLoadOnDemandKey, MultipleLoadOnDemand);
            Preferences.Set(KeepPresaleOrdersKey, KeepPresaleOrders);
            Preferences.Set(ScanDeliveryCheckingKey, ScanDeliveryChecking);
            Preferences.Set(GeneratePresaleNumberKey, GeneratePresaleNumber);
            Preferences.Set(AllowMultParInvoicesKey, AllowMultParInvoices);
            Preferences.Set(SettReportInSalesUoMKey, SettReportInSalesUoM);
            Preferences.Set(IncludeCredInNewParCalcKey, IncludeCredInNewParCalc);
            Preferences.Set(DontAllowDecimalsInQtyKey, DontAllowDecimalsInQty);
            Preferences.Set(SendBackgroundPaymentsKey, SendBackgroundPayments);
            Preferences.Set(NewClientCanHaveDiscountKey, NewClientCanHaveDiscount);
            Preferences.Set(CaptureImagesKey, CaptureImages);
            Preferences.Set(ClientRtnNeededForQtyKey, ClientRtnNeededForQty);
            Preferences.Set(SelectReshipDateKey, SelectReshipDate);
            Preferences.Set(RouteReturnPasswordKey, RouteReturnPassword);
            Preferences.Set(UseQuoteKey, UseQuote);
            Preferences.Set(MinimumAvailableNumbersKey, MinimumAvailableNumbers);
            Preferences.Set(AdvanceSequencyNumKey, AdvanceSequencyNum);
            Preferences.Set(UserCanChangePricesSalesKey, UserCanChangePricesSales);
            Preferences.Set(UserCanChangePricesCreditsKey, UserCanChangePricesCredits);
            Preferences.Set(HideTransfersKey, HideTransfers);
            Preferences.Set(MustCompleteInDeliveryCheckingKey, MustCompleteInDeliveryChecking);
            Preferences.Set(ShowAllProductsInCreditsKey, ShowAllProductsInCredits);
            Preferences.Set(DeleteWeightItemsMenuKey, DeleteWeightItemsMenu);
            Preferences.Set(HidePresaleOptionsKey, HidePresaleOptions);
            Preferences.Set(ShowDiscountByPriceLevelKey, ShowDiscountByPriceLevel);
            Preferences.Set(UseFullTemplateKey, UseFullTemplate);
            Preferences.Set(AllowQtyConversionFactorKey, AllowQtyConversionFactor);
            Preferences.Set(DontDeleteEmptyDeliveriesKey, DontDeleteEmptyDeliveries);
            Preferences.Set(ViewGoalsKey, ViewGoals);
            Preferences.Set(EcoSkyWaterCustomEmailKey, EcoSkyWaterCustomEmail);
            Preferences.Set(KeepAppUpdatedKey, KeepAppUpdated);
            Preferences.Set(HideSalesOrdersKey, HideSalesOrders);
            Preferences.Set(AllowResetKey, AllowReset);
            Preferences.Set(HideClearDataKey, HideClearData);
            Preferences.Set(MustEnterPostedDateKey, MustEnterPostedDate);
            Preferences.Set(NeedAccessForConfigurationKey, NeedAccessForConfiguration);
            Preferences.Set(PreviewOfferPriceInAddItemKey, PreviewOfferPriceInAddItem);
            Preferences.Set(DeleteZeroItemsOnDeliveryKey, DeleteZeroItemsOnDelivery);
            Preferences.Set(SalesmanCanChangeSiteKey, SalesmanCanChangeSite);
            Preferences.Set(MustSetWeightInDeliveryKey, MustSetWeightInDelivery);
            Preferences.Set(PrintCreditReportKey, PrintCreditReport);
            Preferences.Set(CheckAvailableBeforeSendingKey, CheckAvailableBeforeSending);
            Preferences.Set(SAPOrderStatusReportKey, SAPOrderStatusReport);
            Preferences.Set(PresaleUseInventorySiteKey, PresaleUseInventorySite);
            Preferences.Set(CannotOrderWithUnpaidInvoicesKey, CannotOrderWithUnpaidInvoices);
            Preferences.Set(CanPayMoreThanOwnedKey, CanPayMoreThanOwned);
            Preferences.Set(ShowAllEmailsAsDestinationKey, ShowAllEmailsAsDestination);
            Preferences.Set(SelectPriceFromPrevInvoicesKey, SelectPriceFromPrevInvoices);
            Preferences.Set(UsePalletsKey, UsePallets);
            Preferences.Set(HideTaxesTotalPrintKey, HideTaxesTotalPrint);
            Preferences.Set(HideDiscountTotalPrintKey, HideDiscountTotalPrint);
            Preferences.Set(CanModifyWeightsOnDeliveriesKey, CanModifyWeightsOnDeliveries);
            Preferences.Set(ShowPricesInInventorySummaryKey, ShowPricesInInventorySummary);
            Preferences.Set(AlertOrderWasNotSentKey, AlertOrderWasNotSent);
            Preferences.Set(RequestVehicleInformationKey, RequestVehicleInformation);
            Preferences.Set(HideSelectSitesFromMenuKey, HideSelectSitesFromMenu);
            Preferences.Set(ShowCostInTemplateKey, ShowCostInTemplate);
            Preferences.Set(ShowListPriceInAdvancedCatalogKey, ShowListPriceInAdvancedCatalog);
            Preferences.Set(RecalculateOrdersAfterSyncKey, RecalculateOrdersAfterSync);
            Preferences.Set(ShowLowestPriceInTemplateKey, ShowLowestPriceInTemplate);
            Preferences.Set(PrintExternalInvoiceAsOrderKey, PrintExternalInvoiceAsOrder);
            Preferences.Set(UseProductionForPaymentsKey, UseProductionForPayments);
            Preferences.Set(UseCatalogWithFullTemplateKey, UseCatalogWithFullTemplate);
            Preferences.Set(ShowWeightOnInventorySummaryKey, ShowWeightOnInventorySummary);
            Preferences.Set(ShowLowestPriceLevelKey, ShowLowestPriceLevel);
            Preferences.Set(ForceSingleScanKey, ForceSingleScan);
            Preferences.Set(EnableUsernameandPasswordKey, EnableUsernameandPassword);
            Preferences.Set(SavePaymentsByInvoiceNumberKey, SavePaymentsByInvoiceNumber);
            Preferences.Set(SendPaymentsInEODKey, SendPaymentsInEOD);
            Preferences.Set(ShowExpensesInEODKey, ShowExpensesInEOD);
            Preferences.Set(DontCalculateOffersAfterPriceChangedKey, DontCalculateOffersAfterPriceChanged);
            Preferences.Set(RequireCodeForVoidInvoicesKey, RequireCodeForVoidInvoices);
            Preferences.Set(ShowPaymentSummaryKey, ShowPaymentSummary);
            Preferences.Set(CoolerCoCustomizationKey, CoolerCoCustomization);
            Preferences.Set(CanChangeRoutesOrderKey, CanChangeRoutesOrder);
            Preferences.Set(CalculateOffersAutomaticallyKey, CalculateOffersAutomatically);
            Preferences.Set(CalculateTaxPerLineKey, CalculateTaxPerLine);
            Preferences.Set(AddAllowanceToPriceDuringDEXKey, AddAllowanceToPriceDuringDEX);
            Preferences.Set(DontIncludePackageParameterDexUpcKey, DontIncludePackageParameterDexUpc);
            Preferences.Set(OnlyShowCostInProductDetailsKey, OnlyShowCostInProductDetails);
            Preferences.Set(DexUpcCharacterLimitsKey, DexUpcCharacterLimits);
            Preferences.Set(MustCreatePaymentDepositKey, MustCreatePaymentDeposit);
            Preferences.Set(MustSelectRouteToSyncKey, MustSelectRouteToSync);
            Preferences.Set(ShowLastThreeVisitsOnTemplateKey, ShowLastThreeVisitsOnTemplate);
            Preferences.Set(BlockMultipleCollectPaymetsKey, BlockMultipleCollectPaymets);
            Preferences.Set(SelectWarehouseForSalesKey, SelectWarehouseForSales);
            Preferences.Set(DonNovoCustomizationKey, DonNovoCustomization);
            Preferences.Set(UseVisitsTemplateInSalesKey, UseVisitsTemplateInSales);
            Preferences.Set(AllowWorkOrderKey, AllowWorkOrder);
            Preferences.Set(NotifyNewerDataInOSKey, NotifyNewerDataInOS);
            Preferences.Set(AmericanEagleCustomizationKey, AmericanEagleCustomization);
            Preferences.Set(ShowSuggestedButtonKey, ShowSuggestedButton);
            Preferences.Set(DisableSendCatalogWithPricesKey, DisableSendCatalogWithPrices);
            Preferences.Set(RecalculateRoutesOnSyncDataKey, RecalculateRoutesOnSyncData);
            Preferences.Set(CanSelectTermsOnCreateClientKey, CanSelectTermsOnCreateClient);
            Preferences.Set(AlertPrintPaymentBeforeSavingKey, AlertPrintPaymentBeforeSaving);
            Preferences.Set(CatalogReturnsInDefaultUOMKey, CatalogReturnsInDefaultUOM);
            Preferences.Set(UseReturnOrderKey, UseReturnOrder);
            Preferences.Set(IncludeCreditInvoiceForPaymentsKey, IncludeCreditInvoiceForPayments);
            Preferences.Set(MarmiaCustomizationKey, MarmiaCustomization);
            Preferences.Set(NotificationsInSelfServiceKey, NotificationsInSelfService);
            Preferences.Set(CanDepositChecksWithDifDatesKey, CanDepositChecksWithDifDates);
            Preferences.Set(TemplateSearchByContainsKey, TemplateSearchByContains);
            Preferences.Set(UseLaceupDataInSalesReportKey, UseLaceupDataInSalesReport);
            Preferences.Set(ShowDescriptionInSelfServiceCatalogKey, ShowDescriptionInSelfServiceCatalog);
            Preferences.Set(AskOffersBeforeAddingKey, AskOffersBeforeAdding);
            Preferences.Set(CanChangeFinalizedInvoicesKey, CanChangeFinalizedInvoices);
            Preferences.Set(AllowNotificationsKey, AllowNotifications);
            Preferences.Set(ShowLowestAcceptableOnWarningKey, ShowLowestAcceptableOnWarning);
            Preferences.Set(ShowPromoCheckboxKey, ShowPromoCheckbox);
            Preferences.Set(ShowImageInCatalogKey, ShowImageInCatalog);
            Preferences.Set(SoutoBottomEmailTextKey, SoutoBottomEmailText);
            Preferences.Set(CheckInventoryInLoadKey, CheckInventoryInLoad);
            Preferences.Set(CustomerInCreditHoldKey, CustomerInCreditHold);
            Preferences.Set(DeliveryMustScanProductsKey, DeliveryMustScanProducts);
            Preferences.Set(CanModifiyDeliveryWithScanningKey, CanModifiyDeliveryWithScanning);
            Preferences.Set(ShowSentTransactionsKey, ShowSentTransactions);
            Preferences.Set(MustSelectDepartmentKey, MustSelectDepartment);
            Preferences.Set(EnableAdvancedLoginKey, EnableAdvancedLogin);
            Preferences.Set(GenerateProjectionKey, GenerateProjection);
            Preferences.Set(PrintClientTotalOpenBalanceKey, PrintClientTotalOpenBalance);
            Preferences.Set(HidePONumberKey, HidePONumber);
            Preferences.Set(HidePriceInTransactionKey, HidePriceInTransaction);
            Preferences.Set(OnlyPresaleKey, OnlyPresale);
            Preferences.Set(LotIsMandatoryBeforeFinalizeKey, LotIsMandatoryBeforeFinalize);
            Preferences.Set(CheckDueInvoicesInCreateOrderKey, CheckDueInvoicesInCreateOrder);
            Preferences.Set(SignedInSelfServiceKey, SignedInSelfService);
            Preferences.Set(SavedBanksKey, SavedBanks);
            Preferences.Set(DidCloseAlertKey, DidCloseAlert);
            Preferences.Set(ConsignmentPresaleOnlyKey, ConsignmentPresaleOnly);
            Preferences.Set(TruckTemperatureReqKey, TruckTemperatureReq);
            Preferences.Set(MasterDeviceKey, MasterDevice);
            Preferences.Set(ConsignmentBetaKey, ConsignmentBeta);
            Preferences.Set(BetaConfigurationViewKey, BetaConfigurationView);
            Preferences.Set(HiddenItemCustomizationKey, HiddenItemCustomization);
            Preferences.Set(CheckDueInvoicesQtyInCreateOrderKey, CheckDueInvoicesQtyInCreateOrder);
            Preferences.Set(AutomaticClockOutTimeKey, AutomaticClockOutTime);
            Preferences.Set(MandatoryBreakDurationKey, MandatoryBreakDuration);
            Preferences.Set(ForceBreakInMinutesKey, ForceBreakInMinutes);
            Preferences.Set(OtherChargesTypeKey, OtherChargesType);
            Preferences.Set(FreightTypeKey, FreightType);
            Preferences.Set(OtherChargesCommentsKey, OtherChargesComments);
            Preferences.Set(FreightCommentsKey, FreightComments);
            Preferences.Set(OtherChargesValeKey, OtherChargesVale);
            Preferences.Set(FreightValeKey, FreightVale);
            Preferences.Set(UseBigFontForPrintDateKey, UseBigFontForPrintDate);
            Preferences.Set(SimoneKey, Simone);
            Preferences.Set(MinimumOrderProductIdKey, MinimumOrderProductId);
            Preferences.Set(EnableLoginKey, EnableLogin);
            Preferences.Set(ZeroSoldInConsignmentKey, ZeroSoldInConsignment);

            Preferences.Set(SalesmanSeqValuesKey, SalesmanSeqValues);
            Preferences.Set(SalesmanSeqPrefixKey, SalesmanSeqPrefix);
            Preferences.Set(SalesmanSeqExpirationDateKey, SalesmanSeqExpirationDate.Ticks);
            Preferences.Set(SalesmanSeqFromKey, SalesmanSeqFrom);
            Preferences.Set(SalesmanSeqToKey, SalesmanSeqTo);
            Preferences.Set(SalesmanSelectedSiteKey, SalesmanSelectedSite);

            Preferences.Set(ScanBasedTradingKey, ScanBasedTrading);
            Preferences.Set(SelfServiceKey, SelfService);
            Preferences.Set(BetaFragmentsKey, BetaFragments);

            Preferences.Set(SignedInKey, SignedIn);
            Preferences.Set(ProductCatalogViewTypeKey, ProductCatalogViewType);
            Preferences.Set(ShouldGetPinBeforeSyncKey, ShouldGetPinBeforeSync);
            Preferences.Set(ButlerSignedInKey, ButlerSignedIn);
            Preferences.Set(SupervisorIdKey, SupervisorId);
            Preferences.Set(DollyReminderKey, DollyReminder);
            Preferences.Set(GoToMainKey, GoToMain);
            Preferences.Set(UseFutureRouteExKey, UseFutureRouteEx);
            Preferences.Set(AutoCalculateRouteReturnKey, AutoCalculateRouteReturn);
            Preferences.Set(SearchAllProductsInTemplateKey, SearchAllProductsInTemplate);
            Preferences.Set(AdvancedTemplateFocusSearchKey, AdvancedTemplateFocusSearch);
            Preferences.Set(MustSelectReasonForFreeItemKey, MustSelectReasonForFreeItem);
            Preferences.Set(CanEditCreditsInDeliveryKey, CanEditCreditsInDelivery);
            Preferences.Set(CanChangeSalesmanNameKey, CanChangeSalesmanName);
            Preferences.Set(PriceLevelCommentKey, PriceLevelComment);
            Preferences.Set(GetUOMSOnCommandKey, GetUOMSOnCommand);
            Preferences.Set(EnterWeightInCreditsKey, EnterWeightInCredits);
            Preferences.Set(ShowVisitsInfoInClientsKey, ShowVisitsInfoInClients);
            Preferences.Set(UpdateInventoryInPresaleKey, UpdateInventoryInPresale);
            Preferences.Set(SendTempPaymentsInBackgroundKey, SendTempPaymentsInBackground);
            Preferences.Set(DontGenerateLoadPrintedIdKey, DontGenerateLoadPrintedId);
            Preferences.Set(ShowBelow0InAdvancedTemplateKey, ShowBelow0InAdvancedTemplate);
            Preferences.Set(ShowRetailPriceForAddItemKey, ShowRetailPriceForAddItem);
            Preferences.Set(RoundTaxPerLineKey, RoundTaxPerLine);
            Preferences.Set(AllowExchangeKey, AllowExchange);
            Preferences.Set(UsePaymentDiscountKey, UsePaymentDiscount);
            Preferences.Set(OffersAddCommentKey, OffersAddComment);
            Preferences.Set(SendZplOrderKey, SendZplOrder);
            Preferences.Set(MultiplyConversionByCostInIracarAddItemKey, MultiplyConversionByCostInIracarAddItem);
            Preferences.Set(IncludeAvgWeightInCatalogPriceKey, IncludeAvgWeightInCatalogPrice);
            Preferences.Set(ShowOldReportsRegardlessKey, ShowOldReportsRegardless);
            Preferences.Set(CanLogoutKey, CanLogout);
            Preferences.Set(DontRoundInUIKey, DontRoundInUI);
            Preferences.Set(MustScanInTransferKey, MustScanInTransfer);
            Preferences.Set(TimeSheetAutomaticClockInKey, TimeSheetAutomaticClockIn);
            Preferences.Set(DontSortCompaniesByNameKey, DontSortCompaniesByName);
            Preferences.Set(PrintAllInventoriesInInvSummaryKey, PrintAllInventoriesInInvSummary);
            Preferences.Set(DicosaCustomizationKey, DicosaCustomization);
            Preferences.Set(CanModifyEnteredWeightKey, CanModifyEnteredWeight);
            Preferences.Set(ShowServiceReportKey, ShowServiceReport);
            Preferences.Set(MilagroCustomizationKey, MilagroCustomization);
            Preferences.Set(AllowOtherChargesKey, AllowOtherCharges);
            Preferences.Set(CheckIfShipdateLockedKey, CheckIfShipdateLocked);
            Preferences.Set(UseSurveyKey, UseSurvey);
            Preferences.Set(HideOpenInvoiceTotalKey, HideOpenInvoiceTotal);
            Preferences.Set(SalesHistoryForCreditsKey, SalesHistoryForCredits);
            Preferences.Set(UseCreditAccountKey, UseCreditAccount);
            Preferences.Set(HideTransactionsTotalKey, HideTransactionsTotal);
            Preferences.Set(MustAddImageToFinalizedCreditKey, MustAddImageToFinalizedCredit);
            Preferences.Set(HidePaymentsTotalKey, HidePaymentsTotal);
            Preferences.Set(HideSubTotalOrderKey, HideSubTotalOrder);
            Preferences.Set(SalesByDepartmentKey, SalesByDepartment);
            Preferences.Set(CreditReasonInLineKey, CreditReasonInLine);
            Preferences.Set(AcceptLoadEditableKey, AcceptLoadEditable);
            Preferences.Set(ShowProductsPerUoMKey, ShowProductsPerUoM);
            Preferences.Set(EnableCycleCountKey, EnableCycleCount);
            Preferences.Set(DefaultItemHasPriceKey, DefaultItemHasPrice);
            Preferences.Set(ViewPrintInvPasswordKey, ViewPrintInvPassword);
            Preferences.Set(CycleCountPasswordKey, CycleCountPassword);
            Preferences.Set(BranchSiteIdKey, BranchSiteId);
            Preferences.Set(SiteIdKey, SiteId);
            Preferences.Set(CalculateInvForEmptyTruckKey, CalculateInvForEmptyTruck);
            Preferences.Set(MustGenerateProjectionKey, MustGenerateProjection);
            Preferences.Set(LaceupVersionKey, LaceupVersion);
            Preferences.Set(CatalogQuickAddKey, CatalogQuickAdd);
            Preferences.Set(TransferPasswordAtSavingKey, TransferPasswordAtSaving);
            Preferences.Set(OpenTemplateEmptyByDefaultKey, OpenTemplateEmptyByDefault);
            Preferences.Set(DeletePaymentsInTabKey, DeletePaymentsInTab);
            Preferences.Set(MustAddImageToFinalizedKey, MustAddImageToFinalized);
            Preferences.Set(AlwaysFreshCustomizationKey, AlwaysFreshCustomization);
            Preferences.Set(ShowOHQtyInSelfServiceKey, ShowOHQtyInSelfService);
            Preferences.Set(ShowProfitWhenChangingPriceKey, ShowProfitWhenChangingPrice);
            Preferences.Set(ShowOrderStatusKey, ShowOrderStatus);
            Preferences.Set(BringBranchInventoriesKey, BringBranchInventories);
            Preferences.Set(HideContactNameKey, HideContactName);
            Preferences.Set(PrintPaymentRegardlessKey, PrintPaymentRegardless);
            Preferences.Set(GeorgeHoweCustomizationKey, GeorgeHoweCustomization);
            Preferences.Set(PrintInveSettlementRegardlessKey, PrintInveSettlementRegardless);
            Preferences.Set(CanRestoreFromFileKey, CanRestoreFromFile);
            Preferences.Set(DaysToShowSentOrdersKey, DaysToShowSentOrders);
            Preferences.Set(MaxDiscountPerOrderKey, MaxDiscountPerOrder);
            Preferences.Set(DaysToBringOrderStatusKey, DaysToBringOrderStatus);
            Preferences.Set(DaysToRunReportsKey, DaysToRunReports);
            Preferences.Set(LowestPriceLevelIdKey, LowestPriceLevelId);
            Preferences.Set(CanChangeUomInCreditsKey, CanChangeUomInCredits);
            Preferences.Set(RequestAuthPinForLoginKey, RequestAuthPinForLogin);
            Preferences.Set(LoginTimeOutKey, LoginTimeOut);
            Preferences.Set(PaymentBankIsMandatoryKey, PaymentBankIsMandatory);
            Preferences.Set(OrderCanBeChangedKey, OrderCanBeChanged);
            Preferences.Set(CreditCanBeChangedKey, CreditCanBeChanged);
            Preferences.Set(OrderMinimumQtyKey, OrderMinimumQty);
            Preferences.Set(OrderMinQtyMinAmountKey, OrderMinQtyMinAmount);
            Preferences.Set(ProductMinQtyKey, ProductMinQty);
            Preferences.Set(MaxQtyInOrderKey, MaxQtyInOrder);
            Preferences.Set(OrderMinimumTotalPriceKey, OrderMinimumTotalPrice);
            Preferences.Set(LoadByOrderHistoryKey, LoadByOrderHistory);
            Preferences.Set(RecalculateStopsKey, RecalculateStops);
            Preferences.Set(OrderHistoryByClientKey, OrderHistoryByClient);
            Preferences.Set(PrintInvoiceAsReceiptKey, PrintInvoiceAsReceipt);
            Preferences.Set(HidePrintBatchKey, HidePrintBatch);
            Preferences.Set(ShowInvoicesCreditsInPaymentsKey, ShowInvoicesCreditsInPayments);
            Preferences.Set(CanChangeUoMInTransferKey, CanChangeUoMInTransfer);
            Preferences.Set(TransferScanningAddsProductKey, TransferScanningAddsProduct);
            Preferences.Set(SensationalAssetTrackingKey, SensationalAssetTracking);
            Preferences.Set(PrintRefusalReportByStoreKey, PrintRefusalReportByStore);
            Preferences.Set(VoidPaymentsKey, VoidPayments);
            Preferences.Set(SpectrumFloralCustomizationKey, SpectrumFloralCustomization);
            Preferences.Set(CanModifyQuotesKey, CanModifyQuotes);
            Preferences.Set(ShowDiscountIfAppliedKey, ShowDiscountIfApplied);
            Preferences.Set(ShowReportsKey, ShowReports);
            Preferences.Set(AllowToCollectInvoicesKey, AllowToCollectInvoices);
            Preferences.Set(EmptyEndingInventoryKey, EmptyEndingInventory);
            Preferences.Set(BonsuisseCustomizationKey, BonsuisseCustomization);
            Preferences.Set(NewSyncLoadOnDemandKey, NewSyncLoadOnDemand);
            Preferences.Set(UseBiggerUoMInLoadHistoryKey, UseBiggerUoMInLoadHistory);
            Preferences.Set(TotalsByUoMInPdfKey, TotalsByUoMInPdf);
            Preferences.Set(SplitDeliveryByDepartmentKey, SplitDeliveryByDepartment);
            Preferences.Set(PrintNoServiceInSalesReportsKey, PrintNoServiceInSalesReports);
            Preferences.Set(XlsxProviderKey, XlsxProvider);
            Preferences.Set(SelfServiceInvitationKey, SelfServiceInvitation);
            Preferences.Set(CarolinaCustomizationKey, CarolinaCustomization);
            Preferences.Set(IncludePresaleInSalesReportKey, IncludePresaleInSalesReport);
            Preferences.Set(SelectSalesRepForInvoiceKey, SelectSalesRepForInvoice);
            Preferences.Set(AssetTrackingKey, AssetTracking);
            Preferences.Set(DeliveryEditableKey, DeliveryEditable);
            Preferences.Set(DisablePrintEndOfDayReportKey, DisablePrintEndOfDayReport);
            Preferences.Set(HideWarehouseOHInLoadKey, HideWarehouseOHInLoad);
            Preferences.Set(PONumberMaxLengthKey, PONumberMaxLength);
            Preferences.Set(UseLotExpirationKey, UseLotExpiration);
            Preferences.Set(DisolCrapKey, DisolCrap);
            Preferences.Set(ItemGroupedTemplateKey, ItemGroupedTemplate);
            Preferences.Set(WarehouseInventoryOnDemandKey, WarehouseInventoryOnDemand);
            Preferences.Set(AssetStaysMandatoryKey, AssetStaysMandatory);
            Preferences.Set(AddRelatedItemInCreditKey, AddRelatedItemInCredit);
            Preferences.Set(UseOffersInCreditKey, UseOffersInCredit);
            Preferences.Set(UseLaceupAdvancedCatalogKey, UseLaceupAdvancedCatalog);
            Preferences.Set(SelfServiceUserKey, SelfServiceUser);
            Preferences.Set(UseClientSortKey, UseClientSort);
            Preferences.Set(UseDisolSurveyKey, UseDisolSurvey);
            Preferences.Set(DisolSurveyProductsKey, DisolSurveyProducts);
            Preferences.Set(ShowListPriceInAddItemKey, ShowListPriceInAddItem);
            Preferences.Set(CanChangeUomInLoadKey, CanChangeUomInLoad);
            Preferences.Set(IracarCustomizationKey, IracarCustomization);
            Preferences.Set(NewAddItemRandomWeightKey, NewAddItemRandomWeight);
            Preferences.Set(SetShipDateKey, SetShipDate);
            Preferences.Set(LockOrderAfterPrintedKey, LockOrderAfterPrinted);
            Preferences.Set(PackageInReturnPresaleKey, PackageInReturnPresale);
            Preferences.Set(EnableSampleOrderKey, EnableSampleOrder);
            Preferences.Set(EnablePaymentsByTermsKey, EnablePaymentsByTerms);
            Preferences.Set(RequireLotForDumpsKey, RequireLotForDumps);
            Preferences.Set(HidePriceInSelfServiceKey, HidePriceInSelfService);
            Preferences.Set(HideOHinSelfServiceKey, HideOHinSelfService);
            Preferences.Set(ShowWarehouseInvInSummaryKey, ShowWarehouseInvInSummary);
            Preferences.Set(RouteReturnIsMandatoryKey, RouteReturnIsMandatory);
            Preferences.Set(EndingInvIsMandatoryKey, EndingInvIsMandatory);
            Preferences.Set(CasaSanchezCustomizationKey, CasaSanchezCustomization);
            Preferences.Set(ViewAllCommentsKey, ViewAllComments);
            Preferences.Set(ShowAllAvailableLoadsKey, ShowAllAvailableLoads);
            Preferences.Set(AddInventoryOnPoKey, AddInventoryOnPo);
            Preferences.Set(UseRetailPriceForSalesKey, UseRetailPriceForSales);
            Preferences.Set(ProductNameHasDifferentColorKey, ProductNameHasDifferentColor);
            Preferences.Set(StartingPercentageBasedOnCostKey, StartingPercentageBasedOnCost);
            Preferences.Set(GroupClientsByCatKey, GroupClientsByCat);
            Preferences.Set(ExtraInfoBottomPrintKey, ExtraInfoBottomPrint);
            Preferences.Set(ProductCategoryNameIdentifierKey, ProductCategoryNameIdentifier);
            Preferences.Set(SupervisorEmailForRequestsKey, SupervisorEmailForRequests);
            Preferences.Set(SortClientKey, SortClient);
            Preferences.Set(TrackTermsPaymentBottonKey, TrackTermsPaymentBotton);
            Preferences.Set(SalesReportTotalCreditsSubstractedKey, SalesReportTotalCreditsSubstracted);
            Preferences.Set(BarcodeDecoderKey, BarcodeDecoder);
            Preferences.Set(PrintLabelHeight, PrintLabelHeightKey);
            Preferences.Set(AllowSelectPriceLevelKey, AllowSelectPriceLevel);
            Preferences.Set(UpdateInventoryRegardlessKey, UpdateInventoryRegardless);
            Preferences.Set(ShowOnlyInvoiceForSalesmanKey, ShowOnlyInvoiceForSalesman);
            Preferences.Set(HideInvoicesAndBalanceKey, HideInvoicesAndBalance);
            Preferences.Set(ImageInOrderMandatoryKey, ImageInOrderMandatory);
            Preferences.Set(ImageInNoServiceMandatoryKey, ImageInNoServiceMandatory);
            Preferences.Set(PrintCopiesInFinalizeBatchKey, PrintCopiesInFinalizeBatch);
            Preferences.Set(WeeksOfSalesHistoryKey, WeeksOfSalesHistory);
            Preferences.Set(DaysOfProjectionInTemplateKey, DaysOfProjectionInTemplate);
            Preferences.Set(UseFastPrinterKey, UseFastPrinter);
            Preferences.Set(AutoGenerateLoadOrderKey, AutoGenerateLoadOrder);
            Preferences.Set(ApplyDiscountAfterTaxesKey, ApplyDiscountAfterTaxes);
            Preferences.Set(ParLevelHistoryNumVisitKey, ParLevelHistoryNumVisit);

            Preferences.Set("salesPriceInSetting", salesPriceInSetting);
            Preferences.Set("creditsPriceInSetting", creditsPriceInSetting);

            CompanyInfo.Save();
        }

        public static void SetSalesmanSettings(string configSettings)
        {
            string[] parts = configSettings.Split(new char[] { '|' });
            long readedId = 0;

            foreach (string part in parts)
            {
                try
                {
                    string[] sections = part.Split(new char[] { '=' });

                    if (string.Compare(sections[0], "routeid", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.RouteName = sections[1];
                        continue;
                    }

                    if (string.Compare(sections[0], "lastorderid", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        try
                        {
                            readedId = Convert.ToInt64(sections[1], CultureInfo.InvariantCulture);
                            LastPrintedId = readedId;
                        }
                        catch (Exception ee)
                        {
                            Logger.CreateLog("Got an invalid order number from SetSalesmanSettings = " + sections[1]);
                            Logger.CreateLog(ee);
                        }

                        continue;
                    }

                    if (string.Compare(sections[0], "lastpresaleorderid", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        try
                        {
                            readedId = Convert.ToInt64(sections[1], CultureInfo.InvariantCulture);
                            LastPresalePrintedId = readedId;
                        }
                        catch (Exception ee)
                        {
                            Logger.CreateLog("Got an invalid order number from SetSalesmanSettings = " + sections[1]);
                            Logger.CreateLog(ee);
                        }

                        continue;
                    }

                    if (string.Compare(sections[0], "salesmanprefix", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.InvoicePrefix = sections[1];
                        continue;
                    }

                    if (string.Compare(sections[0], "salesmanpresaleprefix", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.InvoicePresalePrefix = sections[1];
                        continue;
                    }

                    if (string.Compare(sections[0], "salesmansequencevalues", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.SalesmanSeqValues = true;
                        continue;
                    }

                    if (string.Compare(sections[0], "salesmansequenceprefix", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.SalesmanSeqPrefix = sections[1];
                        continue;
                    }

                    if (string.Compare(sections[0], "salesmansequenceexpdate", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.SalesmanSeqExpirationDate = new DateTime(Convert.ToInt64(sections[1]));
                        continue;
                    }

                    if (string.Compare(sections[0], "salesmansequencefrom", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.SalesmanSeqFrom = Convert.ToInt32(sections[1]);
                        continue;
                    }

                    if (string.Compare(sections[0], "salesmansequenceto", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.SalesmanSeqTo = Convert.ToInt32(sections[1]);
                        continue;
                    }

                    if (string.Compare(sections[0], "branchsite", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.BranchSiteId = Convert.ToInt32(sections[1]);
                        continue;
                    }

                    if (string.Compare(sections[0], "salesmansiteid", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.SiteId = Convert.ToInt32(sections[1]);
                        continue;
                    }
                }
                catch (Exception ee1)
                {
                    Logger.CreateLog("got an incorrect setting from SetSalesmanSettings " + part);
                    Logger.CreateLog(ee1);
                }
            }
        }

        public static void SetConfigSettings(string configSettings, bool fromDownload = true)
        {
            #region Default Values

            MustAddImageToFinalizedCredit = false;
            ParLevelHistoryNumVisit = 0;
            UseFastPrinter = false;
            AutoGenerateLoadOrder = false;
            ApplyDiscountAfterTaxes = false;
            PrintCopiesInFinalizeBatch = 2;
            ImageInNoServiceMandatory = false;
            ImageInOrderMandatory = false;
            ShowOnlyInvoiceForSalesman = false;
            HideInvoicesAndBalance = false;
            UpdateInventoryRegardless = false;
            AllowSelectPriceLevel = false;
            PrintLabelHeight = "";
            BarcodeDecoder = 1;
            EndingInvIsMandatory = true;
            RouteReturnIsMandatory = true;
            CasaSanchezCustomization = false;
            ViewAllComments = false;
            CatalogQuickAdd = false;
            TransferPasswordAtSaving = false;
            OpenTemplateEmptyByDefault = false;
            DeletePaymentsInTab = false;
            AlwaysFreshCustomization = false;
            PaymentBankIsMandatory = false;
            MustAddImageToFinalized = false;
            OrderCanBeChanged = true;
            ShowOHQtyInSelfService = false;
            ShowProfitWhenChangingPrice = false;
            ShowOrderStatus = false;
            BringBranchInventories = false;
            HideContactName = false;
            PrintPaymentRegardless = false;
            GeorgeHoweCustomization = false;
            PrintInveSettlementRegardless = false;
            CanRestoreFromFile = false;
            DaysToShowSentOrders = 15;
            MaxDiscountPerOrder = 0;
            DaysToBringOrderStatus = 7;
            DaysToRunReports = 0;
            LowestPriceLevelId = 0;
            CanChangeUomInCredits = true;
            RequestAuthPinForLogin = false;
            LoginTimeOut = 8;
            CreditCanBeChanged = true;
            OrderMinimumQty = 0;
            OrderMinQtyMinAmount = false;
            ProductMinQty = false;
            MaxQtyInOrder = 0;
            OrderMinimumTotalPrice = 0;
            ShowAllAvailableLoads = false;
            AddInventoryOnPo = false;
            RecalculateStops = true;
            OrderHistoryByClient = false;
            PrintInvoiceAsReceipt = false;
            HidePrintBatch = false;
            ShowInvoicesCreditsInPayments = false;
            CanChangeUoMInTransfer = true;
            TransferScanningAddsProduct = true;
            UseRetailPriceForSales = false;
            ProductNameHasDifferentColor = false;
            StartingPercentageBasedOnCost = 0;
            ShowBillOfLadingPdf = false;
            DefaultItemHasPrice = false;
            GroupClientsByCat = string.Empty;
            ExtraInfoBottomPrint = string.Empty;
            ProductCategoryNameIdentifier = "Suggested";
            SupervisorEmailForRequests = string.Empty;
            SortClient = string.Empty;
            TrackTermsPaymentBotton = string.Empty;
            SalesReportTotalCreditsSubstracted = false;
            useCatalog = null;
            ShowWarehouseInvInSummary = false;
            HidePriceInSelfService = true;
            HideOHinSelfService = true;
            RequireLotForDumps = false;
            EnablePaymentsByTerms = "";
            PackageInReturnPresale = false;
            LockOrderAfterPrinted = false;
            SetShipDate = true;
            IracarCustomization = false;
            NewAddItemRandomWeight = false;
            CanChangeUomInLoad = true;
            ShowListPriceInAddItem = false;
            DisolSurveyProducts = "1172,1158,1163,1164";
            UseDisolSurvey = false;
            UseClientSort = false;
            SelfServiceUser = false;
            UseLaceupAdvancedCatalog = false;
            UseOffersInCredit = false;
            AddRelatedItemInCredit = true;
            AssetStaysMandatory = false;
            WarehouseInventoryOnDemand = false;
            DisolCrap = false;
            ItemGroupedTemplate = false;
            UseLotExpiration = false;
            PONumberMaxLength = 0;
            HideWarehouseOHInLoad = false;
            DisablePrintEndOfDayReport = false;
            DeliveryEditable = true;
            AssetTracking = false;
            SelectSalesRepForInvoice = false;
            IncludePresaleInSalesReport = false;
            CarolinaCustomization = false;
            SelfServiceInvitation = 0;
            XlsxProvider = string.Empty;
            PrintNoServiceInSalesReports = true;
            SplitDeliveryByDepartment = false;
            TotalsByUoMInPdf = false;
            UseBiggerUoMInLoadHistory = false;
            BonsuisseCustomization = false;
            EmptyEndingInventory = false;
            LoadByOrderHistory = false;
            HideSelectSitesFromMenu = false;
            MustGenerateProjection = true;
            CalculateInvForEmptyTruck = true;
            CycleCountPassword = string.Empty;
            ViewPrintInvPassword = string.Empty;
            EnableCycleCount = false;
            PrintExternalInvoiceAsOrder = true;
            UseProductionForPayments = false;
            UseCatalogWithFullTemplate = false;
            ShowProductsPerUoM = false;
            ForceSingleScan = false;
            EnableUsernameandPassword = false;
            SavePaymentsByInvoiceNumber = false;
            SendPaymentsInEOD = true;
            ShowExpensesInEOD = false;
            DontCalculateOffersAfterPriceChanged = false;
            RequireCodeForVoidInvoices = false;
            ShowPaymentSummary = false;
            CoolerCoCustomization = false;
            CanChangeRoutesOrder = false;
            CalculateOffersAutomatically = true;
            CalculateTaxPerLine = false;
            AddAllowanceToPriceDuringDEX = false;
            DontIncludePackageParameterDexUpc = false;
            OnlyShowCostInProductDetails = false;
            DexUpcCharacterLimits = 14;
            AcceptLoadEditable = true;
            CreditReasonInLine = true;
            SalesByDepartment = false;
            HideSubTotalOrder = false;
            HidePaymentsTotal = false;
            HideTransactionsTotal = false;
            UseCreditAccount = false;
            SalesHistoryForCredits = false;
            HideOpenInvoiceTotal = false;
            UseSurvey = false;
            AutoCalculateRouteReturn = false;
            UseFutureRouteEx = true;
            CustomerInCreditHold = true;
            DeliveryMustScanProducts = false;
            CanModifiyDeliveryWithScanning = false;
            ShowSentTransactions = true;
            MustSelectDepartment = false;
            GoToMain = false;
            DollyReminder = false;
            BetaFragments = DataAccess.CheckCommunicatorVersion(DataAccess.CommunicatorVersion, "26.1.0.0");
            SelfService = false;
            ScanBasedTrading = false;
            SalesmanSeqValues = false;
            ZeroSoldInConsignment = false;
            MinimumOrderProductId = 0;
            Simone = false;
            UseBigFontForPrintDate = false;
            CheckDueInvoicesQtyInCreateOrder = 0;
            AutomaticClockOutTime = 0;
            MandatoryBreakDuration = 0;
            ForceBreakInMinutes = 0;
            OtherChargesType = 0;
            FreightType = 0;
            OtherChargesComments = string.Empty;
            FreightComments = string.Empty;
            OtherChargesVale = 0;
            FreightVale = 0;
            HiddenItemCustomization = false;
            BetaConfigurationView = DataAccess.CheckCommunicatorVersion(DataAccess.CommunicatorVersion, "26.1.0.0");
            ConsignmentBeta = false;
            TruckTemperatureReq = false;
            ConsignmentPresaleOnly = false;
            CheckDueInvoicesInCreateOrder = false;
            LotIsMandatoryBeforeFinalize = false;
            OnlyPresale = false;
            HidePriceInTransaction = false;
            HidePONumber = false;
            PrintClientTotalOpenBalance = false;
            GenerateProjection = false;
            UseFullTemplate = false;
            HideTransfers = false;
            MustCompleteInDeliveryChecking = true;
            ShowAllProductsInCredits = true;
            DeleteWeightItemsMenu = false;
            HidePresaleOptions = false;
            ShowDiscountByPriceLevel = false;
            AllowQtyConversionFactor = true;
            DontDeleteEmptyDeliveries = false;
            ViewGoals = false;
            EcoSkyWaterCustomEmail = false;
            KeepAppUpdated = false;
            HideSalesOrders = false;
            AllowReset = false;
            HideClearData = false;
            MustEnterPostedDate = false;
            NeedAccessForConfiguration = false;
            PreviewOfferPriceInAddItem = false;
            DeleteZeroItemsOnDelivery = true;
            SalesmanCanChangeSite = false;
            MustSetWeightInDelivery = false;
            PrintCreditReport = false;
            CheckAvailableBeforeSending = false;
            SAPOrderStatusReport = false;
            PresaleUseInventorySite = false;
            CannotOrderWithUnpaidInvoices = false;
            CanPayMoreThanOwned = false;
            ShowAllEmailsAsDestination = false;
            SelectPriceFromPrevInvoices = false;
            UsePallets = false;
            HideTaxesTotalPrint = false;
            HideDiscountTotalPrint = false;
            CanModifyWeightsOnDeliveries = false;
            ShowPricesInInventorySummary = false;
            AlertOrderWasNotSent = false;
            RequestVehicleInformation = false;
            ShowCostInTemplate = false;
            ShowListPriceInAdvancedCatalog = true;
            RecalculateOrdersAfterSync = false;
            ShowLowestPriceInTemplate = false;
            ShowWeightOnInventorySummary = false;
            ShowLowestPriceLevel = false;
            EnableAdvancedLogin = false;
            UserCanChangePricesCredits = false;
            UserCanChangePricesSales = false;
            AdvanceSequencyNum = false;
            MinimumAvailableNumbers = 0;
            UseQuote = false;
            RouteReturnPassword = "";
            SelectReshipDate = false;
            ClientRtnNeededForQty = 0;
            CaptureImages = DataAccess.CheckCommunicatorVersion(DataAccess.CommunicatorVersion, "26.1.0.0");
            NewClientCanHaveDiscount = false;
            SendBackgroundPayments = false;
            DontAllowDecimalsInQty = false;
            IncludeCredInNewParCalc = false;
            SettReportInSalesUoM = false;
            AllowMultParInvoices = false;
            GeneratePresaleNumber = false;
            ScanDeliveryChecking = false;
            KeepPresaleOrders = false;
            MultipleLoadOnDemand = false;
            AverageSaleInParLevel = false;
            ConsParFirstInPresale = false;
            IncludeRotationInDelivery = false;
            ChargeBatteryRotation = true;
            WarrantyPerClient = false;
            AlwaysCountInPar = false;
            EditParInHistory = false;
            CloseRouteInPresale = true;
            UseAllDayParLevel = false;
            LspInAllLines = false;
            CheckInventoryInPreSale = false;
            IgnoreDiscountInCredits = true;
            CanModifyConnectSett = true;
            AlwaysUpdateNewPar = false;
            DeliveryReasonInLine = false;
            OnlyKitInCredit = false;
            GeneratePreorderNum = true;
            ShipViaMandatory = false;
            ShowShipVia = false;
            DisolCustomIdGenerator = false;
            ConsLotAsDate = false;
            AddCreditInConsignment = true;
            ParInConsignment = false;
            HideVoidButton = false;
            PrintTaxLabel = "SALES TAX:";
            AllowDiscountPerLine = false;
            PresaleCommMandatory = false;
            MinimumAmount = 0;
            MinimumWeight = 0;
            SendBackgroundBackup = true;
            UseConsignmentLot = false;
            PreSaleConsigment = false;
            PrintInvSettReport = true;
            HideProdOnHand = false;
            BlackStoneConsigCustom = false;
            DaysToKeepSignatures = 7;
            PrintBillShipDate = false;
            UseDraggableTemplate = false;
            HidePrintedCommentLine = false;
            MinShipDateDays = 0;
            ClientNameMaxSize = 0;
            SalesmanInCreditDel = false;
            ShowAddrInClientList = true;
            UseLastUoM = false;
            DefaultTaxRate = 0;
            BillNumRequired = false;
            RTN = string.Empty;
            UseFullConsignment = false;
            NewClientExtraFields = string.Empty;
            NewClientEmailRequired = false;
            PdfProvider = string.Empty;
            UseClientClassAsCompanyName = false;
            CanChangeUoM = true;
            AddSalesInConsignment = false;
            ParLevelHistoryDays = 60;
            MustEnterCaseInOut = false;
            PaymentOrSignatureRequired = false;
            DoNotShrinkOrderImage = false;
            SendOrderIsMandatory = false;
            HidePriceInPrintedLine = false;
            SendBackgroundOrders = false;
            ConsignmentContractText = string.Empty;
            AllowAdjPastExpDate = true;
            DisableRouteReturn = false;
            ShowAvgInCatalog = false;
            BackgroundTime = 30;
            ClientDailyPL = false;
            IncludeBatteryInLoad = false;
            HideInvoiceComment = false;
            SendByEmailInFinalize = false;
            HideItemComment = false;
            AddCoreBalance = false;
            TransferComment = false;
            SalesRegReportWithTax = false;
            MasterLoadOrder = false;
            SyncLoadOnDemand = false;
            MagnoliaSetConsignment = false;
            AuthProdsInCredit = false;
            PrintNetQty = false;
            NewClientCanChangePrices = true;
            POIsMandatory = false;
            UseTermsInLoadOrder = false;
            AddItemInDefaultUoM = false;
            UseReturnInvoice = false;
            IncludeDeliveriesInLoadOrder = false;
            UseAllowance = false;
            AddCoresInSalesItem = true;
            PrintZeroesOnPickSheet = false;
            SignatureNameRequired = false;
            ExtraTextForPrinter = string.Empty;
            AddRelatedItemsInTotal = true;
            RemovePayBalFomInvoice = false;
            Discount100PercentPrintText = string.Empty;
            MustCompleteRoute = false;
            SelectDriverFromPresale = false;
            PrintIsMandatory = false;
            HideCompanyInfoPrint = false;
            UseSendByEmail = false;
            UsePrintProofDelivery = false;
            UsePairLotQty = false;
            CanSelectSalesman = false;
            DefaultCreditDetType = string.Empty;
            ExtraSpaceForSignature = 0;
            AutoGeneratePO = false;
            HideSetConsignment = false;
            PrintedIdLength = 0;
            UseOldEmailFormat = false;
            ShipDateIsMandatory = false;
            ShipDateIsMandatoryForLoad = false;
            PresaleShipDate = false;
            CoreAsCredit = false;
            AskDEXUoM = false;
            UseLSPByDefault = false;
            PrintClientOpenBalance = false;
            CreditTemplate = false;
            ConsignmentShow0 = true;
            ConsignmentMustCountAll = false;
            ConsignmentKeepPrice = false;
            PrintInvoiceNumberDown = false;
            PrintFromClientDetail = true;
            ConcatUPCToName = false;
            PrintUpcAsText = false;
            InvoicePrefix = string.Empty;
            SurveyQuestions = string.Empty;
            InvoicePresalePrefix = string.Empty;
            CanLeaveBatch = false;
            AutoEndInventoryPassword = string.Empty;
            CremiMexDepartments = string.Empty;
            BackGroundSync = true;
            ShowInvoiceTotal = true;
            PrintUPCInventory = false;
            PrintUPCOpenInvoices = false;
            AddQtyTotalRegardlessUoM = false;
            ShowProductsWith0Inventory = false;
            DisplayTaxOnCatalogAndPrint = false;
            PrasekCustomization = false;
            AlwaysShowDefaultUoM = false;
            ButlerCustomization = false;
            ShowPrintPickTicket = false;
            CaribCustomization = false;
            AddDefaultItemToCredit = false;
            DefaultItem = 0;
            DistanceToOrder = 0;
            LoadLotInTransfer = false;
            MustEndOrders = true;
            ProductInMultipleCategory = false;
            AllowEditRelated = false;
            ExtendedPaymentOptions = true;
            GroupRelatedWhenPrinting = false;
            PrintLotPreOrder = true;
            PrintLotOrder = true;
            CanVoidFOrders = true;
            TransferOffPassword = string.Empty;
            BetaFeatures = false;
            MustBeOnlineAlways = false;
            EnableLiveData = false;
            HideTotalOrder = false;
            HideTotalInPrintedLine = false;
            PrintCopy = false;
            Consignment = false;
            MustEndOfDayDaily = false;
            ForceEODWhenDateChanges = false;
            UsesTerms = false;
            UseReship = true;
            UseClockInOut = false;
            DexVersion = "4010";
            DexDefaultUnit = string.Empty;
            OldPrinter = 1;
            UpdateWhenEndDay = false;
            AutoAcceptLoad = false;
            RemoveWarnings = false;
            GroupLinesWhenPrinting = true;
            ShortInventorySettlement = false;
            PrintReportsRequired = true;
            LoadOrderEmpty = false;
            AcceptLoadPrintRequired = false;
            PreSale = false;
            LeftOrderTemplateEmpty = false;
            EmptyTruckAtEndOfDay = false;
            LoadRequest = false;
            LoadRequired = false;
            PickCompany = true;
            ProductCatalog = true;
            Delivery = false;
            SendLogByEmail = false;
            EmailOrder = false;
            MustPrintPreOrder = false;
            CanIncreasePrice = false;
            FakePreOrder = false;
            PrintTruncateNames = false;
            PopulateTemplateAuthProd = false;
            OneDoc = false;
            CanAddClient = false;
            SetPO = false;
            InventoryRequestEmail = string.Empty;
            CanGoBelow0 = false;
            NoPriceChangeDeliveries = false;
            WarningDumpReturn = false;
            UseOrderId = false;
            Round = 2;
            DexAvailable = false;
            PrintUPC = true;
            CanChangeSalesmanId = !MasterDevice;
            BottomOrderPrintText = string.Empty;
            SingleScanStroke = true;
            PrintInvoiceSort = string.Empty;
            PrintClientSort = string.Empty;
            MustSendOrdersFirst = false;
            AllowDiscount = false;
            LocationIsMandatory = false;
            AlwaysAddItemsToOrder = false;
            FreeItemsNeedComments = false;
            LotIsMandatory = false;
            PaymentAvailable = true;
            DisablePaymentIfTermDaysMoreThan0 = false;
            PaymentRequired = false;
            PrinterToUse = "";
            InvoiceIdProvider = "LaceUPMobileClassesIOS.DefaultInvoiceProvider";
            SignatureRequired = false;
            UseLot = false;
            DisplayPurchasePrice = false;
            AnyPriceIsAcceptable = false;
            UseLSP = false;
            DaysToKeepOrder = 90;
            TransferPassword = string.Empty;
            InventoryPassword = string.Empty;
            AddInventoryPassword = string.Empty;
            TrackInventory = false;
            CanModifyInventory = false;
            PrinterAvailable = true;
            PrintingRequired = false;
            AllowCreditOrders = true;
            MustUpdateDaily = true;
            AllowFreeItems = true;
            SendLoadOrder = false;
            UseLocation = true;
            AllowOrderForClientOverCreditLimit = true;
            UserCanChangePrices = false;
            ScannerToUse = 3;
            UseUpcCheckDigit = true;
            RouteManagement = false;
            SetParLevel = false;
            UseUpc128 = false;
            DeliveryScanning = false;
            UseBattery = false;
            NewConsPrinter = false;
            TimeSheetCustomization = false;
            GenerateValuesLoadOrder = false;
            ShowReports = true;
            AllowToCollectInvoices = true;
            ShowDiscountIfApplied = false;
            CanModifyQuotes = false;
            SpectrumFloralCustomization = false;
            VoidPayments = false;
            PrintRefusalReportByStore = false;
            AmericanEagleCustomization = false;
            ShowSuggestedButton = false;
            DisableSendCatalogWithPrices = false;
            RecalculateRoutesOnSyncData = false;
            CanSelectTermsOnCreateClient = false;
            AlertPrintPaymentBeforeSaving = false;
            CatalogReturnsInDefaultUOM = false;
            UseReturnOrder = false;
            IncludeCreditInvoiceForPayments = false;
            MarmiaCustomization = false;
            NotificationsInSelfService = true;
            CanDepositChecksWithDifDates = false;
            TemplateSearchByContains = false;
            UseLaceupDataInSalesReport = false;
            ShowDescriptionInSelfServiceCatalog = false;
            CanChangeFinalizedInvoices = false;
            AskOffersBeforeAdding = false;
            SearchAllProductsInTemplate = false;
            AdvancedTemplateFocusSearch = false;
            MustSelectReasonForFreeItem = false;
            CanEditCreditsInDelivery = true;
            CanChangeSalesmanName = false;
            PriceLevelComment = false;
            GetUOMSOnCommand = true;
            EnterWeightInCredits = false;
            ShowVisitsInfoInClients = false;
            SendTempPaymentsInBackground = false;
            DontGenerateLoadPrintedId = false;
            ShowBelow0InAdvancedTemplate = true;
            ShowRetailPriceForAddItem = false;
            RoundTaxPerLine = false;
            AllowExchange = false;
            UsePaymentDiscount = false;
            OffersAddComment = true;
            SendZplOrder = false;
            MultiplyConversionByCostInIracarAddItem = false;
            IncludeAvgWeightInCatalogPrice = false;
            ShowOldReportsRegardless = false;
            CanLogout = true;
            DontRoundInUI = false;
            MustScanInTransfer = false;
            TimeSheetAutomaticClockIn = false;
            DontSortCompaniesByName = false;
            PrintAllInventoriesInInvSummary = false;
            DicosaCustomization = false;
            CanModifyEnteredWeight = false;
            ShowServiceReport = false;
            MilagroCustomization = false;
            AllowOtherCharges = false;
            CheckIfShipdateLocked = false;
            UpdateInventoryInPresale = false;
            AllowNotifications = DataAccess.CheckCommunicatorVersion(DataAccess.CommunicatorVersion, "52.2.0");
            ShowLowestAcceptableOnWarning = false;
            ShowPromoCheckbox = false;
            ShowImageInCatalog = false;
            SoutoBottomEmailText = string.Empty;
            NotifyNewerDataInOS = false;
            AllowWorkOrder = false;
            UseVisitsTemplateInSales = false;
            DonNovoCustomization = false;

            int companiesCount = 0;

            if (fromDownload)
                CompanyInfo.Clear();
            else
                CompanyInfo.Remove(false);

            companiesCount = CompanyInfo.Companies.Count(x => x.FromFile);

            if (companiesCount == 0) CompanyInfo.Companies.Add(CompanyInfo.CreateDefaultCompany());

            DataAccess.ActivityProvider.LoadActivitys();

            #endregion

            string[] parts = configSettings.Split(new char[] { '|' });

            foreach (string part in parts)
            {
                try
                {
                    string[] sections = part.Split(new char[] { '=' });

                    if (companiesCount == 0)
                    {
                        // CompanyName
                        if (string.Compare(sections[0], CompanyNameKey, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            CompanyInfo.Companies[0].CompanyName = sections[1];
                            continue;
                        }

                        // CompanyAddress1
                        if (string.Compare(sections[0], CompanyAddress1Key, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            CompanyInfo.Companies[0].CompanyAddress1 = sections[1];
                            continue;
                        }

                        // CompanyAddress2
                        if (string.Compare(sections[0], CompanyAddress2Key, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            CompanyInfo.Companies[0].CompanyAddress2 = sections[1];
                            continue;
                        }

                        // CompanyPhone
                        if (string.Compare(sections[0], CompanyPhoneKey, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            CompanyInfo.Companies[0].CompanyPhone = sections[1];
                            continue;
                        }

                        // CompanyFaxKey
                        if (string.Compare(sections[0], CompanyFaxKey, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            CompanyInfo.Companies[0].CompanyFax = sections[1];
                            continue;
                        }

                        // CompanyEmail
                        if (string.Compare(sections[0], CompanyEmailKey, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            CompanyInfo.Companies[0].CompanyEmail = sections[1];
                            continue;
                        }

                        // LocationKey
                        if (string.Compare(sections[0], LocationKey, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            CompanyInfo.Companies[0].Location = sections[1];
                            continue;
                        }

                        // CommIdKey
                        if (string.Compare(sections[0], CommIdKey, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            CompanyInfo.Companies[0].CommId = sections[1];
                            continue;
                        }

                        // DUNSKey
                        if (string.Compare(sections[0], DUNSKey, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            CompanyInfo.Companies[0].DUNS = sections[1];
                            continue;
                        }

                        //CompanyLicenseKey
                        if (string.Compare(sections[0], CompanyLicense, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            CompanyInfo.Companies[0].CompanyLicenses = sections[1];
                            continue;
                        }

                        //CompanyExtraFieldKey
                        if (string.Compare(sections[0], CompanyExtraField, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            CompanyInfo.Companies[0].ExtraFields = sections[1];
                            continue;
                        }
                    }

                    // extraCompanyInfo
                    if (string.Compare(sections[0], ExtraCompanyInfoKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        // a new company
                        var companyParts = sections[1].Split(new char[] { ';' });
                        if (companyParts.Length > 3)
                        {
                            var ci = new CompanyInfo()
                            {
                                CompanyName = companyParts[0],
                                CompanyAddress1 = companyParts[1],
                                CompanyAddress2 = companyParts[2],
                                CompanyPhone = companyParts[3]
                            };
                            if (companyParts.Length > 4) ci.DUNS = companyParts[4];
                            if (companyParts.Length > 5) ci.Location = companyParts[5];
                            if (companyParts.Length > 6) ci.CommId = companyParts[6];
                            if (companyParts.Length > 7) ci.CompanyFax = companyParts[7];
                            if (companyParts.Length > 8) ci.ExtraFields = companyParts[8];

                            CompanyInfo.Companies.Add(ci);
                        }

                        continue;
                    }

                    //MinimumOrderProductIdKey
                    if (string.Compare(sections[0], MinimumOrderProductIdKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.MinimumOrderProductId = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture);
                        continue;
                    }

                    //SimoneKey
                    if (string.Compare(sections[0], SimoneKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.Simone = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //DexVersionKey
                    if (string.Compare(sections[0], DexVersionKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.DexVersion = sections[1];
                        continue;
                    }

                    //DexDefaultUnitKey
                    if (string.Compare(sections[0], DexDefaultUnitKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.DexDefaultUnit = sections[1];
                        continue;
                    }

                    //CheckInventoryInPreSaleKey
                    if (string.Compare(sections[0], CheckInventoryInPreSaleKey, StringComparison.OrdinalIgnoreCase) ==
                        0)
                    {
                        Config.CheckInventoryInPreSale = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //IgnoreDiscountInCreditsKey
                    if (string.Compare(sections[0], IgnoreDiscountInCreditsKey, StringComparison.OrdinalIgnoreCase) ==
                        0)
                    {
                        Config.IgnoreDiscountInCredits = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //AlwaysUpdateInventoryKey
                    if (string.Compare(sections[0], AlwaysUpdateNewParKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.AlwaysUpdateNewPar = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //AskDEXUoMKey
                    if (string.Compare(sections[0], AskDEXUoMKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.AskDEXUoM = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], ExtraTextForPrinterKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.ExtraTextForPrinter = sections[1];
                        continue;
                    }

                    //WstcoKey
                    if (string.Compare(sections[0], WstcoKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.Wstco = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //UseLSPByDefaultKey
                    if (string.Compare(sections[0], UseLSPByDefaultKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.UseLSPByDefault = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], TimeSheetCustomizationKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.TimeSheetCustomization = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], GenerateValuesLoadOrderKey, StringComparison.OrdinalIgnoreCase) ==
                        0)
                    {
                        Config.GenerateValuesLoadOrder = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], SurveyQuestionsKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.SurveyQuestions = sections[1];
                        continue;
                    }

                    //PresaleShipDateKey
                    if (string.Compare(sections[0], PresaleShipDateKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.PresaleShipDate = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //ShipDateIsMandatoryKey
                    if (string.Compare(sections[0], ShipDateIsMandatoryKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.ShipDateIsMandatory = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //ShipDateIsMandatoryForLoadKey
                    if (string.Compare(sections[0], ShipDateIsMandatoryForLoadKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.ShipDateIsMandatoryForLoad =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //CoreAsCreditKey
                    if (string.Compare(sections[0], CoreAsCreditKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.CoreAsCredit = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //PrintConsCustOpenBalanceKey
                    if (string.Compare(sections[0], PrintClientOpenBalanceKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.PrintClientOpenBalance = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //ConsignmentMustCountAllKey
                    if (string.Compare(sections[0], CreditTemplateKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.CreditTemplate = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //ConsignmentMustCountAllKey
                    if (string.Compare(sections[0], ConsignmentShow0Key, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.ConsignmentShow0 = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //ConsignmentMustCountAllKey
                    if (string.Compare(sections[0], ConsignmentMustCountAllKey, StringComparison.OrdinalIgnoreCase) ==
                        0)
                    {
                        Config.ConsignmentMustCountAll = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //ConsignmentKeepPriceKey
                    if (string.Compare(sections[0], ConsignmentKeepPriceKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.ConsignmentKeepPrice = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //PrintInvoiceNumberDown
                    if (string.Compare(sections[0], PrintInvoiceNumberDownKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.PrintInvoiceNumberDown = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //PrintFromClientDetail
                    if (string.Compare(sections[0], PrintFromClientDetailKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.PrintFromClientDetail = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //ConcatUPCToNameKey
                    if (string.Compare(sections[0], ConcatUPCToNameKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.ConcatUPCToName = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //PrintChangedOnlyKey
                    if (string.Compare(sections[0], PrintUPCInventoryKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.PrintUPCInventory = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //PrintUPCOpenInvoicesKey
                    if (string.Compare(sections[0], PrintUPCOpenInvoicesKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.PrintUPCOpenInvoices = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //PrintUpcAsTextKey
                    if (string.Compare(sections[0], PrintUpcAsTextKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.PrintUpcAsText = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //CanLeaveBatchKey
                    if (string.Compare(sections[0], CanLeaveBatchKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.CanLeaveBatch = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //TransferOffPasswordKey
                    if (string.Compare(sections[0], TransferOffPasswordKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.TransferOffPassword = sections[1];
                        continue;
                    }

                    //AutoEndInventoryKey
                    if (string.Compare(sections[0], AutoEndInventoryKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        var autoendingInventory = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        if (autoendingInventory)
                        {
                            RouteReturnIsMandatory = false;
                            EndingInvIsMandatory = false;
                        }

                        continue;
                    }

                    //InvoicePrefixKey
                    if (string.Compare(sections[0], InvoicePrefixKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.InvoicePrefix = sections[1];
                        continue;
                    }

                    //InvoicePresalePrefixKey
                    if (string.Compare(sections[0], InvoicePresalePrefixKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.InvoicePresalePrefix = sections[1];
                        continue;
                    }

                    //AutoEndInventoryPasswordKey
                    if (string.Compare(sections[0], AutoEndInventoryPasswordKey, StringComparison.OrdinalIgnoreCase) ==
                        0)
                    {
                        Config.AutoEndInventoryPassword = sections[1];
                        continue;
                    }

                    //CremiMexDepartmentsKey
                    if (string.Compare(sections[0], CremiMexDepartmentsKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.CremiMexDepartments = sections[1];
                        continue;
                    }

                    //BackGroundSyncKey
                    if (string.Compare(sections[0], BackGroundSyncKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.BackGroundSync = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //ShowInvoiceTotalKey
                    if (string.Compare(sections[0], ShowInvoiceTotalKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.ShowInvoiceTotal = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //AddDefaultItemToCreditKey
                    if (string.Compare(sections[0], AddDefaultItemToCreditKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.AddDefaultItemToCredit = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //DefaultItemKey
                    if (string.Compare(sections[0], DefaultItemKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.DefaultItem = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture);
                        continue;
                    }

                    //DistanceToOrder
                    if (string.Compare(sections[0], DistanceToOrderKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.DistanceToOrder = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture);
                        continue;
                    }

                    //LoadLotInTransferKey
                    if (string.Compare(sections[0], LoadLotInTransferKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.LoadLotInTransfer = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //MustPrintPreOrderKey
                    if (string.Compare(sections[0], MustPrintPreOrderKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.MustPrintPreOrder = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //CanIncreasePriceKey
                    if (string.Compare(sections[0], CanIncreasePriceKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.CanIncreasePrice = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //ProductCatalogKey
                    if (string.Compare(sections[0], ProductCatalogKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.ProductCatalog = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //HidePriceAndTotalKey
                    if (string.Compare(sections[0], HideTotalInPrintedLineKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        HideTotalInPrintedLine = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //PrintLotOrderKey
                    if (string.Compare(sections[0], PrintLotOrderKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        PrintLotOrder = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //PrintLotPreOrderKey
                    if (string.Compare(sections[0], PrintLotPreOrderKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        PrintLotPreOrder = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //CanVoidFOrdersKey
                    if (string.Compare(sections[0], CanVoidFOrdersKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        CanVoidFOrders = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //GroupRelatedWhenPrintingKey
                    if (string.Compare(sections[0], GroupRelatedWhenPrintingKey, StringComparison.OrdinalIgnoreCase) ==
                        0)
                    {
                        GroupRelatedWhenPrinting = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //MustEndOrdersKey
                    if (string.Compare(sections[0], MustEndOrdersKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        MustEndOrders = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //ProductInMultipleCategoryKey
                    if (string.Compare(sections[0], ProductInMultipleCategoryKey, StringComparison.OrdinalIgnoreCase) ==
                        0)
                    {
                        ProductInMultipleCategory = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //DefaultItemHasPriceKey
                    if (string.Compare(sections[0], DefaultItemHasPriceKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        DefaultItemHasPrice = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //AllowEditRelated
                    if (string.Compare(sections[0], AllowEditRelatedKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        AllowEditRelated = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //AutoGenerateLoadOrderKey
                    if (string.Compare(sections[0], AutoGenerateLoadOrderKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        AutoGenerateLoadOrder = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //ApplyDiscountAfterTaxesKey
                    if (string.Compare(sections[0], ApplyDiscountAfterTaxesKey, StringComparison.OrdinalIgnoreCase) ==
                        0)
                    {
                        ApplyDiscountAfterTaxes = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //ExtendedPaymentOptionsKey
                    if (string.Compare(sections[0], ExtendedPaymentOptionsKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        ExtendedPaymentOptions = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], CasaSanchezCustomizationKey, StringComparison.OrdinalIgnoreCase) ==
                        0)
                    {
                        CasaSanchezCustomization = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], ViewAllCommentsKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        ViewAllComments = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], ShowAllAvailableLoadsKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        ShowAllAvailableLoads = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], RecalculateStopsKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        RecalculateStops = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], OrderHistoryByClientKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        OrderHistoryByClient = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], PrintInvoiceAsReceiptKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        PrintInvoiceAsReceipt = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], HidePrintBatchKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        HidePrintBatch = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], CatalogQuickAddKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        CatalogQuickAdd = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], TransferPasswordAtSavingKey, StringComparison.OrdinalIgnoreCase) ==
                        0)
                    {
                        TransferPasswordAtSaving = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], OpenTemplateEmptyByDefaultKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        OpenTemplateEmptyByDefault = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], DeletePaymentsInTabKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        DeletePaymentsInTab = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], AlwaysFreshCustomizationKey, StringComparison.OrdinalIgnoreCase) ==
                        0)
                    {
                        AlwaysFreshCustomization = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], MustAddImageToFinalizedCreditKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        MustAddImageToFinalizedCredit = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], PaymentBankIsMandatoryKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        PaymentBankIsMandatory = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], OrderCanBeChangedKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        OrderCanBeChanged = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], CreditCanBeChangedKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        CreditCanBeChanged = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], OrderMinimumQtyKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        OrderMinimumQty = (float)Convert.ToDouble(sections[1], CultureInfo.InvariantCulture);
                        continue;
                    }

                    if (string.Compare(sections[0], MaxQtyInOrderKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        MaxQtyInOrder = (float)Convert.ToDouble(sections[1], CultureInfo.InvariantCulture);
                        continue;
                    }

                    if (string.Compare(sections[0], MustAddImageToFinalizedKey, StringComparison.OrdinalIgnoreCase) ==
                        0)
                    {
                        MustAddImageToFinalized = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], OrderMinQtyMinAmountKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        OrderMinQtyMinAmount = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], ProductMinQtyKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        ProductMinQty = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], ShowOHQtyInSelfServiceKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        ShowOHQtyInSelfService = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], ShowProfitWhenChangingPriceKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        ShowProfitWhenChangingPrice = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], ShowOrderStatusKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        ShowOrderStatus = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], BringBranchInventoriesKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        BringBranchInventories = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], HideContactNameKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        HideContactName = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], PrintPaymentRegardlessKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        PrintPaymentRegardless = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], GeorgeHoweCustomizationKey, StringComparison.OrdinalIgnoreCase) ==
                        0)
                    {
                        GeorgeHoweCustomization = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], PrintInveSettlementRegardlessKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        PrintInveSettlementRegardless = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], CanRestoreFromFileKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        CanRestoreFromFile = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], DaysToShowSentOrdersKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        DaysToShowSentOrders = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture);
                        continue;
                    }

                    if (string.Compare(sections[0], MaxDiscountPerOrderKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        MaxDiscountPerOrder = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture);
                        continue;
                    }

                    if (string.Compare(sections[0], DaysToBringOrderStatusKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        DaysToBringOrderStatus = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture);
                        continue;
                    }

                    if (string.Compare(sections[0], DaysToRunReportsKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        DaysToRunReports = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture);
                        continue;
                    }

                    if (string.Compare(sections[0], LowestPriceLevelIdKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        LowestPriceLevelId = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture);
                        continue;
                    }

                    if (string.Compare(sections[0], CanChangeUomInCreditsKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        CanChangeUomInCredits = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], RequestAuthPinForLoginKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        RequestAuthPinForLogin = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], LoginTimeOutKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        LoginTimeOut = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture);
                        continue;
                    }

                    if (string.Compare(sections[0], OrderMinimumTotalPriceKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        OrderMinimumTotalPrice = (float)Convert.ToDouble(sections[1], CultureInfo.InvariantCulture);
                        continue;
                    }

                    if (string.Compare(sections[0], ShowInvoicesCreditsInPaymentsKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        ShowInvoicesCreditsInPayments = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], CanChangeUoMInTransferKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        CanChangeUoMInTransfer = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], TransferScanningAddsProductKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        TransferScanningAddsProduct = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], SensationalAssetTrackingKey, StringComparison.OrdinalIgnoreCase) ==
                        0)
                    {
                        SensationalAssetTracking = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], PrintRefusalReportByStoreKey, StringComparison.OrdinalIgnoreCase) ==
                        0)
                    {
                        PrintRefusalReportByStore = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], VoidPaymentsKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        VoidPayments = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], SpectrumFloralCustomizationKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        SpectrumFloralCustomization = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], CanModifyQuotesKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        CanModifyQuotes = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], ShowDiscountIfAppliedKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        ShowDiscountIfApplied = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], ShowReportsKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        ShowReports = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], AllowToCollectInvoicesKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        AllowToCollectInvoices = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], AddInventoryOnPoKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        AddInventoryOnPo = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], AddQtyTotalRegardlessUoMKey, StringComparison.OrdinalIgnoreCase) ==
                        0)
                    {
                        AddQtyTotalRegardlessUoM = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], ShowProductsWith0InventoryKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        ShowProductsWith0Inventory = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], DisplayTaxOnCatalogAndPrintKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        DisplayTaxOnCatalogAndPrint = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], PrasekCustomizationKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        PrasekCustomization = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], AlwaysShowDefaultUoMKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        AlwaysShowDefaultUoM = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], ButlerCustomizationKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        ButlerCustomization = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], ShowPrintPickTicketKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        ShowPrintPickTicket = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], CaribCustomizationKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        CaribCustomization = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], UseRetailPriceForSalesKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        UseRetailPriceForSales = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], ShowBillOfLadingPdfKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        ShowBillOfLadingPdf = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], StartingPercentageBasedOnCostKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        StartingPercentageBasedOnCost = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture);
                        continue;
                    }

                    if (string.Compare(sections[0], GroupClientsByCatKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        GroupClientsByCat = sections[1];
                        continue;
                    }

                    if (string.Compare(sections[0], ExtraInfoBottomPrintKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        ExtraInfoBottomPrint = sections[1];
                        continue;
                    }

                    if (string.Compare(sections[0], ProductCategoryNameIdentifierKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        ProductCategoryNameIdentifier = sections[1];
                        continue;
                    }

                    if (string.Compare(sections[0], SupervisorEmailForRequestsKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        SupervisorEmailForRequests = sections[1];
                        continue;
                    }

                    if (string.Compare(sections[0], SortClientKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        SortClient = sections[1];
                        continue;
                    }

                    if (string.Compare(sections[0], TrackTermsPaymentBottonKey, StringComparison.OrdinalIgnoreCase) ==
                        0)
                    {
                        TrackTermsPaymentBotton = sections[1];
                        continue;
                    }

                    if (string.Compare(sections[0], SalesReportTotalCreditsSubstractedKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        SalesReportTotalCreditsSubstracted =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //BetaFeaturesKey
                    if (string.Compare(sections[0], BetaFeaturesKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        BetaFeatures = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //MustBeOnlineAlwaysKey
                    if (string.Compare(sections[0], MustBeOnlineAlwaysKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        MustBeOnlineAlways = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //EnableLiveDataKey
                    if (string.Compare(sections[0], EnableLiveDataKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        EnableLiveData = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //HideTotalOrderKey
                    if (string.Compare(sections[0], HideTotalOrderKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        HideTotalOrder = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //PrintCopyKey
                    if (string.Compare(sections[0], PrintCopyKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        PrintCopy = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //ConsignmentKey
                    if (string.Compare(sections[0], ConsignmentKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Consignment = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //MustEndOfDayDailyKey
                    if (string.Compare(sections[0], MustEndOfDayDailyKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        MustEndOfDayDaily = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //ForceEODWhenDateChanges
                    if (string.Compare(sections[0], ForceEODWhenDateChangesKey, StringComparison.OrdinalIgnoreCase) ==
                        0)
                    {
                        ForceEODWhenDateChanges = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //UsesTermsKey
                    if (string.Compare(sections[0], UsesTermsKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        UsesTerms = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //UseClockInOutKey
                    if (string.Compare(sections[0], UseReshipKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        UseReship = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //UseClockInOutKey
                    if (string.Compare(sections[0], UseClockInOutKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        UseClockInOut = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //UpdateWhenEndDayKey
                    if (string.Compare(sections[0], OldPrinterKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        OldPrinter = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture);
                        continue;
                    }

                    //UpdateWhenEndDayKey
                    if (string.Compare(sections[0], UpdateWhenEndDayKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        UpdateWhenEndDay = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //AutoAcceptLoadKey
                    if (string.Compare(sections[0], AutoAcceptLoadKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        AutoAcceptLoad = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //RemoveWarnings
                    if (string.Compare(sections[0], RemoveWarningsKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        RemoveWarnings = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //ShortInventorySettlementKey
                    if (string.Compare(sections[0], GroupLinesWhenPrintingKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        GroupLinesWhenPrinting = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //ShortInventorySettlementKey
                    if (string.Compare(sections[0], ShortInventorySettlementKey, StringComparison.OrdinalIgnoreCase) ==
                        0)
                    {
                        ShortInventorySettlement = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //PrintReportsRequiredKey
                    if (string.Compare(sections[0], PrintReportsRequiredKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        PrintReportsRequired = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //LoadOrderEmptyKey
                    if (string.Compare(sections[0], LoadOrderEmptyKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        LoadOrderEmpty = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //AcceptLoadPrintRequiredKey
                    if (string.Compare(sections[0], AcceptLoadPrintRequiredKey, StringComparison.OrdinalIgnoreCase) ==
                        0)
                    {
                        AcceptLoadPrintRequired = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //PreSaleKey
                    if (string.Compare(sections[0], PreSaleKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        PreSale = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //DeliveryKey
                    if (string.Compare(sections[0], DeliveryKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.Delivery = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //LoadRequestKey
                    if (string.Compare(sections[0], LoadRequestKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.LoadRequest = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //LoadRequiredKey
                    if (string.Compare(sections[0], LoadRequiredKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.LoadRequired = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //EmptyTruckAtEndOfDayKey
                    if (string.Compare(sections[0], EmptyTruckAtEndOfDayKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.EmptyTruckAtEndOfDay = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //LeftOrderTemplateEmptyKey
                    if (string.Compare(sections[0], LeftOrderTemplateEmptyKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.LeftOrderTemplateEmpty = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //PickCompanyKey
                    if (string.Compare(sections[0], PickCompanyKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.PickCompany = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //SendLogByEmailKey
                    if (string.Compare(sections[0], SendLogByEmailKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.SendLogByEmail = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //EmailOrderKey
                    if (string.Compare(sections[0], EmailOrderKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.EmailOrder = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //FakePreOrderKey
                    if (string.Compare(sections[0], FakePreOrderKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.FakePreOrder = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //PrintTruncateNamesKey
                    if (string.Compare(sections[0], PrintTruncateNamesKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.PrintTruncateNames = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //PopulateTemplateAuthProd
                    if (string.Compare(sections[0], PopulateTemplateAuthProdKey, StringComparison.OrdinalIgnoreCase) ==
                        0)
                    {
                        Config.PopulateTemplateAuthProd =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //OneDocKey
                    if (string.Compare(sections[0], OneDocKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.OneDoc = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //CanAddClientKey
                    if (string.Compare(sections[0], CanAddClientKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.CanAddClient = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //SetPOKey
                    if (string.Compare(sections[0], SetPOKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.SetPO = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //InvReqKey
                    if (string.Compare(sections[0], InvReqKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.InventoryRequestEmail = sections[1];
                        continue;
                    }

                    //CanGoBelow0Key
                    if (string.Compare(sections[0], CanGoBelow0Key, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.CanGoBelow0 = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //NoPriceChangeDeliveries
                    if (string.Compare(sections[0], NoPriceChangeDeliveriesKey, StringComparison.OrdinalIgnoreCase) ==
                        0)
                    {
                        Config.NoPriceChangeDeliveries = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //WarningDumpReturnKey
                    if (string.Compare(sections[0], WarningDumpReturnKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.WarningDumpReturn = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //UseOrderIdKey
                    if (string.Compare(sections[0], UseOrderIdKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.UseOrderId = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //LastOrderIdKey
                    if (string.Compare(sections[0], LastOrderIdKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        var lastId = Convert.ToInt64(sections[1], CultureInfo.InvariantCulture);
                        if (lastId > LastPrintedId) LastPrintedId = lastId;
                        continue;
                    }

                    //LastPresaleOrderIdKey
                    if (string.Compare(sections[0], LastPresaleOrderIdKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        var lastPresaleId = Convert.ToInt64(sections[1], CultureInfo.InvariantCulture);
                        if (lastPresaleId > LastPresalePrintedId) LastPresalePrintedId = lastPresaleId;
                        continue;
                    }

                    // RoundKey
                    if (string.Compare(sections[0], RoundKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.Round = Convert.ToInt32(sections[1]);
                        continue;
                    }

                    // DexKey
                    if (string.Compare(sections[0], DexKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.DexAvailable = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // PrintUPCKey
                    if (string.Compare(sections[0], PrintUPCKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.PrintUPC = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // CanChangeSalesmanIdKey
                    if (string.Compare(sections[0], CanChangeSalesmanIdKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.CanChangeSalesmanId = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // SingleScanStrokeKey
                    if (string.Compare(sections[0], BottomOrderPrintTextKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.BottomOrderPrintText = sections[1];
                        continue;
                    }

                    // SingleScanStrokeKey
                    if (string.Compare(sections[0], SingleScanStrokeKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.SingleScanStroke = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // PrintInvoiceSortKey
                    if (string.Compare(sections[0], PrintInvoiceSortKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.PrintInvoiceSort = sections[1];
                        continue;
                    }

                    // PrintClientSortKey
                    if (string.Compare(sections[0], PrintClientSortKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.PrintClientSort = sections[1];
                        continue;
                    }

                    // MustSendOrdersFirstKey
                    if (string.Compare(sections[0], MustSendOrdersFirstKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.MustSendOrdersFirst = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // AllowDiscountKey
                    if (string.Compare(sections[0], AllowDiscountKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.AllowDiscount = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // LocationIsMandatoryKey
                    if (string.Compare(sections[0], LocationIsMandatoryKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.LocationIsMandatory = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // PaymentAvailableKey
                    if (string.Compare(sections[0], PaymentAvailableKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.PaymentAvailable = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // DisablePaymentIfTermDaysMoreThan0
                    if (string.Compare(sections[0], DisablePaymentIfTermDaysMoreThan0Key,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.DisablePaymentIfTermDaysMoreThan0 =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // PaymentRequiredKey
                    if (string.Compare(sections[0], PaymentRequiredKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.PaymentRequired = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // PrinterToUseKey
                    if (string.Compare(sections[0], PrinterToUseKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.PrinterToUse = sections[1];

                        if (!PrinterToUse.StartsWith("LaceUPMobileClassesIOS."))
                            PrinterToUse = "LaceUPMobileClassesIOS." + PrinterToUse;

                        continue;
                    }

                    // InvoiceIdKey
                    if (string.Compare(sections[0], InvoiceIdKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        if (sections[1] == "LaceUPMobileClassesIOS.SequentialInvoiceProvider")
                        {
                            Config.InvoiceIdProvider = "LaceUPMobileClassesIOS.PrefixedSequentialInvoiceProvider";
                        }
                        else
                            Config.InvoiceIdProvider = sections[1];

                        continue;
                    }

                    // SignatureRequiredKey
                    if (string.Compare(sections[0], SignatureRequiredKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.SignatureRequired = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // LotKey
                    if (string.Compare(sections[0], LotKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.UseLot = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // FakeUseLot
                    if (string.Compare(sections[0], FakeUseLotKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.FakeUseLot = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // LotIsMandatoryKey
                    if (string.Compare(sections[0], LotIsMandatoryKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.LotIsMandatory = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // AlwaysAddItemsToOrderKey
                    if (string.Compare(sections[0], AlwaysAddItemsToOrderKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.AlwaysAddItemsToOrder = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // PaymentInCashFullOnlyKey
                    if (string.Compare(sections[0], FreeItemsNeedCommentsKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.FreeItemsNeedComments = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // DisplayPurchasePriceKey
                    if (string.Compare(sections[0], DisplayPurchasePriceKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.DisplayPurchasePrice = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // DaysToKeepOrder
                    if (string.Compare(sections[0], DaysToKeepOrderKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.DaysToKeepOrder = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture);
                        continue;
                    }

                    // InventoryPasswordKey
                    if (string.Compare(sections[0], TransferPasswordKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.TransferPassword = sections[1];
                        continue;
                    }

                    // InventoryPasswordKey
                    if (string.Compare(sections[0], InventoryPasswordKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.InventoryPassword = sections[1];
                        continue;
                    }

                    // AddInventoryPasswordKey
                    if (string.Compare(sections[0], AddInventoryPasswordKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.AddInventoryPassword = sections[1];
                        continue;
                    }

                    // CanModifyInventory
                    if (string.Compare(sections[0], CanModifyInventoryKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.CanModifyInventory = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // TrackInventoryKey
                    if (string.Compare(sections[0], TrackInventoryKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.TrackInventory = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // PrinterAvailable
                    if (string.Compare(sections[0], PrinterAvailableKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.PrinterAvailable = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // PrintingRequiredKey
                    if (string.Compare(sections[0], PrintingRequiredKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.PrintingRequired = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // AllowCreditOrders
                    if (string.Compare(sections[0], AllowCreditOrdersKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.AllowCreditOrders = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // VendorNameKey
                    if (string.Compare(sections[0], VendorNameKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.VendorName = sections[1];
                        continue;
                    }

                    // RouteNameKey
                    if (string.Compare(sections[0], RouteNameKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.RouteName = sections[1];
                        continue;
                    }

                    // LanKey
                    if (string.Compare(sections[0], LanKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.LanAddress = sections[1];
                        continue;
                    }

                    // SSID
                    if (string.Compare(sections[0], SSIDKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.SSID = sections[1];
                        continue;
                    }

                    // MustUpdateDaily
                    if (string.Compare(sections[0], MustUpdateDailyKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.MustUpdateDaily = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // AllowFreeItems
                    if (string.Compare(sections[0], AllowFreeItemsKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.AllowFreeItems = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // ViewInvoiceTotal
                    if (string.Compare(sections[0], SendLoadOrderKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.SendLoadOrder = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // UseLocation
                    if (string.Compare(sections[0], UseLocationKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.UseLocation = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // AllowOrderForClientOverCreditLimit
                    if (string.Compare(sections[0], AllowOrderForClientOverCreditLimitKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.AllowOrderForClientOverCreditLimit =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // UserCanChangePrices
                    if (string.Compare(sections[0], UserCanChangePricesKey, StringComparison.OrdinalIgnoreCase) == 0 ||
                        string.Compare(sections[0], "UserCanChangePricesKey", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.UserCanChangePrices = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // AnyPriceIsAcceptableKey
                    if (string.Compare(sections[0], AnyPriceIsAcceptableKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.AnyPriceIsAcceptable = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // UseLSPKey
                    if (string.Compare(sections[0], UseLSPKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.UseLSP = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // ViewProviderKey
                    if (string.Compare(sections[0], ViewProviderKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        if (sections[1].Contains("iphone")) continue;
                        DataAccess.ActivityProvider.AddCustomActivitys(sections[1]);
                        continue;
                    }

                    if (string.Compare(sections[0], ViewProviderAndroidKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        if (sections[1].Contains("iphone")) continue;
                        DataAccess.ActivityProvider.AddCustomActivitys(sections[1]);
                        continue;
                    }

                    // ScannerToUseKey
                    if (string.Compare(sections[0], ScannerToUseKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.ScannerToUse = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture);
                        continue;
                    }

                    // UseUpcCheckDigitKey
                    if (string.Compare(sections[0], UseUpcCheckDigitKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.UseUpcCheckDigit = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // RouteManagement
                    if (string.Compare(sections[0], RouteManagementKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.RouteManagement = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // SetParLevelKey
                    if (string.Compare(sections[0], SetParLevelKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.SetParLevel = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // UseUpc128Key
                    if (string.Compare(sections[0], UseUpc128Key, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.UseUpc128 = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // DeliveryScanningKey
                    if (string.Compare(sections[0], DeliveryScanningKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.DeliveryScanning = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // UseBatteryKey
                    if (string.Compare(sections[0], UseBatteryKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.UseBattery = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // NewConsPrinterKey
                    if (string.Compare(sections[0], NewConsPrinterKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.NewConsPrinter = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // UseOldEmailFormatKey
                    if (string.Compare(sections[0], UseOldEmailFormatKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.UseOldEmailFormat = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // PrintedIdLengthKey
                    if (string.Compare(sections[0], PrintedIdLengthKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.PrintedIdLength = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture);
                        continue;
                    }

                    // HideSetConsignmentKey
                    if (string.Compare(sections[0], HideSetConsignmentKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.HideSetConsignment = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // AutoGeneratePOKey
                    if (string.Compare(sections[0], AutoGeneratePOKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.AutoGeneratePO = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // ExtraSpaceForSignatureKey
                    if (string.Compare(sections[0], ExtraSpaceForSignatureKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.ExtraSpaceForSignature = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture);
                        continue;
                    }

                    // DefaultCreditDetTypeKey
                    if (string.Compare(sections[0], DefaultCreditDetTypeKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.DefaultCreditDetType = sections[1];
                        continue;
                    }

                    // CanSelectSalesmanKey
                    if (string.Compare(sections[0], CanSelectSalesmanKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.CanSelectSalesman = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // UsePairLotQtyKey
                    if (string.Compare(sections[0], UsePairLotQtyKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.UsePairLotQty = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // UseSendByEmailKey
                    if (string.Compare(sections[0], UseSendByEmailKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.UseSendByEmail = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // UsePrintProofDelivery
                    if (string.Compare(sections[0], UsePrintProofDeliveryKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.UsePrintProofDelivery = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // HideCompanyInfoPrintKey
                    if (string.Compare(sections[0], HideCompanyInfoPrintKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.HideCompanyInfoPrint = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // PrintIsMandatoryKey
                    if (string.Compare(sections[0], PrintIsMandatoryKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.PrintIsMandatory = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // SelectDriverFromPresaleKey
                    if (string.Compare(sections[0], SelectDriverFromPresaleKey, StringComparison.OrdinalIgnoreCase) ==
                        0)
                    {
                        Config.SelectDriverFromPresale = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // MustCompleteRouteKey
                    if (string.Compare(sections[0], MustCompleteRouteKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.MustCompleteRoute = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // DiscountPrintTextKey
                    if (string.Compare(sections[0], Discount100PercentPrintTextKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.Discount100PercentPrintText = sections[1];
                        continue;
                    }

                    // RemovePayBalFomInvoiceKey
                    if (string.Compare(sections[0], RemovePayBalFomInvoiceKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.RemovePayBalFomInvoice = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // AddRelatedItemsInTotalKey
                    if (string.Compare(sections[0], AddRelatedItemsInTotalKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.AddRelatedItemsInTotal = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // SignatureNameRequiredKey
                    if (string.Compare(sections[0], SignatureNameRequiredKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.SignatureNameRequired = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // PrintZeroesOnPickSheetKey
                    if (string.Compare(sections[0], PrintZeroesOnPickSheetKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.PrintZeroesOnPickSheet = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // AddCoresInSalesItemKey
                    if (string.Compare(sections[0], AddCoresInSalesItemKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.AddCoresInSalesItem = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // UseAllowanceKey
                    if (string.Compare(sections[0], UseAllowanceKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.UseAllowance = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // IncludeDeliveriesInLoadOderKey
                    if (string.Compare(sections[0], IncludeDeliveriesInLoadOrderKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.IncludeDeliveriesInLoadOrder =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // UseReturnInvoiceKey
                    if (string.Compare(sections[0], UseReturnInvoiceKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.UseReturnInvoice = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // AddItemInDefaultUoMKey
                    if (string.Compare(sections[0], AddItemInDefaultUoMKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.AddItemInDefaultUoM = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // UseTermsInLoadOrderKey
                    if (string.Compare(sections[0], UseTermsInLoadOrderKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.UseTermsInLoadOrder = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // POIsMandatoryKey
                    if (string.Compare(sections[0], POIsMandatoryKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.POIsMandatory = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // NewClientCanChangePricesKey
                    if (string.Compare(sections[0], NewClientCanChangePricesKey, StringComparison.OrdinalIgnoreCase) ==
                        0)
                    {
                        Config.NewClientCanChangePrices =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // PrintNetQtyKey
                    if (string.Compare(sections[0], PrintNetQtyKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.PrintNetQty = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // AuthProdsInCreditKey
                    if (string.Compare(sections[0], AuthProdsInCreditKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.AuthProdsInCredit = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // MagnoliaSetConsignmentKey
                    if (string.Compare(sections[0], MagnoliaSetConsignmentKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.MagnoliaSetConsignment = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // SyncLoadOnDemandKey
                    if (string.Compare(sections[0], SyncLoadOnDemandKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.SyncLoadOnDemand = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // MasterLoadOrderKey
                    if (string.Compare(sections[0], MasterLoadOrderKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.MasterLoadOrder = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // SalesRegReportWithTaxKey
                    if (string.Compare(sections[0], SalesRegReportWithTaxKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.SalesRegReportWithTax = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // TransferCommentKey
                    if (string.Compare(sections[0], TransferCommentKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.TransferComment = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // AddCoreBalancekey
                    if (string.Compare(sections[0], AddCoreBalancekey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.AddCoreBalance = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // HideItemCommentKey
                    if (string.Compare(sections[0], HideItemCommentKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.HideItemComment = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // SendByEmailInFinalizeKey
                    if (string.Compare(sections[0], SendByEmailInFinalizeKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.SendByEmailInFinalize = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // HideInvoiceCommentKey
                    if (string.Compare(sections[0], HideInvoiceCommentKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.HideInvoiceComment = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // IncludeBatteryInLoadKey
                    if (string.Compare(sections[0], IncludeBatteryInLoadKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.IncludeBatteryInLoad = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // ClientDailyPLKey
                    if (string.Compare(sections[0], ClientDailyPLKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.ClientDailyPL = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // BackgroundTimeKey
                    if (string.Compare(sections[0], BackgroundTimeKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.BackgroundTime = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture);
                        continue;
                    }

                    // ShowAvgInCatalogKey
                    if (string.Compare(sections[0], ShowAvgInCatalogKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.ShowAvgInCatalog = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // DisableRouteReturnKey
                    if (string.Compare(sections[0], DisableRouteReturnKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.DisableRouteReturn = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // AllowAdjPastExpDateKey
                    if (string.Compare(sections[0], AllowAdjPastExpDateKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.AllowAdjPastExpDate = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // ConsignmentContractTextKey
                    if (string.Compare(sections[0], ConsignmentContractTextKey, StringComparison.OrdinalIgnoreCase) ==
                        0)
                    {
                        Config.ConsignmentContractText = sections[1];
                        continue;
                    }

                    // SendBackgroundOrdersKey
                    if (string.Compare(sections[0], SendBackgroundOrdersKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.SendBackgroundOrders = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // HidePriceInPrintedLineKey
                    if (string.Compare(sections[0], HidePriceInPrintedLineKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.HidePriceInPrintedLine = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // SendOrderIsMandatoryKey
                    if (string.Compare(sections[0], SendOrderIsMandatoryKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.SendOrderIsMandatory = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // PaymentOrSignatureRequiredKey
                    if (string.Compare(sections[0], PaymentOrSignatureRequiredKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.PaymentOrSignatureRequired =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // DoNotShrinkOrderImageKey
                    if (string.Compare(sections[0], DoNotShrinkOrderImageKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.DoNotShrinkOrderImage = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // MustEnterCaseInOutKey
                    if (string.Compare(sections[0], MustEnterCaseInOutKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.MustEnterCaseInOut = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // ParLevelHistoryDaysKey
                    if (string.Compare(sections[0], ParLevelHistoryDaysKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.ParLevelHistoryDays = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture);
                        continue;
                    }

                    // AddSalesInConsignmentKey
                    if (string.Compare(sections[0], AddSalesInConsignmentKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.AddSalesInConsignment = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // CanChangeUoMKey
                    if (string.Compare(sections[0], CanChangeUoMKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.CanChangeUoM = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // UseClientClassAsCompanyNameKey
                    if (string.Compare(sections[0], UseClientClassAsCompanyNameKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.UseClientClassAsCompanyName =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // PdfProviderKey
                    if (string.Compare(sections[0], PdfProviderKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.PdfProvider = sections[1];
                        continue;
                    }

                    // NewClientEmailRequiredKey
                    if (string.Compare(sections[0], NewClientEmailRequiredKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.NewClientEmailRequired = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // NewClientExtraFieldsKey
                    if (string.Compare(sections[0], NewClientExtraFieldsKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.NewClientExtraFields = sections[1];
                        continue;
                    }

                    // UseFullConsignmentKey
                    if (string.Compare(sections[0], UseFullConsignmentKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.UseFullConsignment = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // RTNKey
                    if (string.Compare(sections[0], RTNKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.RTN = sections[1];
                        continue;
                    }

                    // BillNumRequiredKey
                    if (string.Compare(sections[0], BillNumRequiredKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.BillNumRequired = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // DefaultTaxRateKey
                    if (string.Compare(sections[0], DefaultTaxRateKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.DefaultTaxRate = Convert.ToSingle(sections[1], CultureInfo.InvariantCulture);
                        continue;
                    }

                    // UseLastUoMKey
                    if (string.Compare(sections[0], UseLastUoMKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.UseLastUoM = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // ShowAddrInClientListKey
                    if (string.Compare(sections[0], ShowAddrInClientListKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.ShowAddrInClientList = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // SalesmanInCreditDelKey
                    if (string.Compare(sections[0], SalesmanInCreditDelKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.SalesmanInCreditDel = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // ClientNameMaxSizeKey
                    if (string.Compare(sections[0], ClientNameMaxSizeKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.ClientNameMaxSize = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture);
                        continue;
                    }

                    // MinShipDateDaysKey
                    if (string.Compare(sections[0], MinShipDateDaysKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.MinShipDateDays = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture);
                        continue;
                    }

                    // HidePrintedCommentLineKey
                    if (string.Compare(sections[0], HidePrintedCommentLineKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.HidePrintedCommentLine = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // UseDraggableTemplateKey
                    if (string.Compare(sections[0], UseDraggableTemplateKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.UseDraggableTemplate = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // PrintBillShipDateKey
                    if (string.Compare(sections[0], PrintBillShipDateKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.PrintBillShipDate = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // DaysToKeepSignaturesKey
                    if (string.Compare(sections[0], DaysToKeepSignaturesKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.DaysToKeepSignatures = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture);
                        continue;
                    }

                    // BlackStoneConsigCustomKey
                    if (string.Compare(sections[0], BlackStoneConsigCustomKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.BlackStoneConsigCustom = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // HideProdOnHandKey
                    if (string.Compare(sections[0], HideProdOnHandKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.HideProdOnHand = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // PrintInvSettReportKey
                    if (string.Compare(sections[0], PrintInvSettReportKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.PrintInvSettReport = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // PreSaleConsigmentKey
                    if (string.Compare(sections[0], PreSaleConsigmentKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.PreSaleConsigment = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // UseConsignmentLotKey
                    if (string.Compare(sections[0], UseConsignmentLotKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.UseConsignmentLot = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // SendBackgroundBackupKey
                    if (string.Compare(sections[0], SendBackgroundBackupKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.SendBackgroundBackup = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // MinimumWeightkey
                    if (string.Compare(sections[0], MinimumWeightkey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.MinimumWeight = Convert.ToSingle(sections[1], CultureInfo.InvariantCulture);
                        continue;
                    }

                    // Minimumamountkey
                    if (string.Compare(sections[0], Minimumamountkey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.MinimumAmount = Convert.ToSingle(sections[1], CultureInfo.InvariantCulture);
                        continue;
                    }

                    // PresaleCommMandatoryKey
                    if (string.Compare(sections[0], PresaleCommMandatoryKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.PresaleCommMandatory = Convert.ToSingle(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // PrintTaxLabelKey
                    if (string.Compare(sections[0], PrintTaxLabelKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.PrintTaxLabel = sections[1];
                        continue;
                    }

                    // AllowDiscountPerLineKey
                    if (string.Compare(sections[0], AllowDiscountPerLineKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.AllowDiscountPerLine = Convert.ToSingle(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // HideVoidButtonKey
                    if (string.Compare(sections[0], HideVoidButtonKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.HideVoidButton = Convert.ToSingle(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // ConsLotAsDateKey
                    if (string.Compare(sections[0], ConsLotAsDateKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.ConsLotAsDate = Convert.ToSingle(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // DisolCustomIdGeneratorKey
                    if (string.Compare(sections[0], DisolCustomIdGeneratorKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.DisolCustomIdGenerator = Convert.ToSingle(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // ParInConsignmentKey
                    if (string.Compare(sections[0], ParInConsignmentKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.ParInConsignment = Convert.ToSingle(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // AddCreditInConsignmentKey
                    if (string.Compare(sections[0], AddCreditInConsignmentKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.AddCreditInConsignment = Convert.ToSingle(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // ShowShipViaKey
                    if (string.Compare(sections[0], ShowShipViaKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.ShowShipVia = Convert.ToSingle(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // ShipViaMandatoryKey
                    if (string.Compare(sections[0], ShipViaMandatoryKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.ShipViaMandatory = Convert.ToSingle(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // GeneratePreorderNumKey
                    if (string.Compare(sections[0], GeneratePreorderNumKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.GeneratePreorderNum = Convert.ToSingle(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // OnlyKitInCreditKey
                    if (string.Compare(sections[0], OnlyKitInCreditKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.OnlyKitInCredit = Convert.ToSingle(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // DeliveryReasonInLineKey
                    if (string.Compare(sections[0], DeliveryReasonInLineKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.DeliveryReasonInLine = Convert.ToSingle(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //AlwaysUpdateInventoryKey
                    if (string.Compare(sections[0], AlwaysUpdateNewParKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.AlwaysUpdateNewPar = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //CanModifyConnectSettKey
                    if (string.Compare(sections[0], CanModifyConnectSettKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.CanModifyConnectSett = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //LspInAllLinesKey
                    if (string.Compare(sections[0], LspInAllLinesKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.LspInAllLines = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //UseAllDayParLevelKey
                    if (string.Compare(sections[0], UseAllDayParLevelKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.UseAllDayParLevel = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //CloseRouteInPresaleKey
                    if (string.Compare(sections[0], CloseRouteInPresaleKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.CloseRouteInPresale = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //ScanParToHistoryKey
                    if (string.Compare(sections[0], EditParInHistoryKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.EditParInHistory = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //AlwaysCountInParKey
                    if (string.Compare(sections[0], AlwaysCountInParKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.AlwaysCountInPar = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //WarrantyPerClientKey
                    if (string.Compare(sections[0], WarrantyPerClientKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.WarrantyPerClient = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //ChargeBatteryRotationKey
                    if (string.Compare(sections[0], ChargeBatteryRotationKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.ChargeBatteryRotation = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //IncludeRotationInDeliveryKey
                    if (string.Compare(sections[0], IncludeRotationInDeliveryKey, StringComparison.OrdinalIgnoreCase) ==
                        0)
                    {
                        Config.IncludeRotationInDelivery =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //ConsParFirstInPresaleKey
                    if (string.Compare(sections[0], ConsParFirstInPresaleKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.ConsParFirstInPresale = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //AverageSaleInParLevelKey
                    if (string.Compare(sections[0], AverageSaleInParLevelKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.AverageSaleInParLevel = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //MultipleLoadOnDemandKey
                    if (string.Compare(sections[0], MultipleLoadOnDemandKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.MultipleLoadOnDemand = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //KeepPresaleOrdersKey
                    if (string.Compare(sections[0], KeepPresaleOrdersKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.KeepPresaleOrders = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //ScanDeliveryCheckingKey
                    if (string.Compare(sections[0], ScanDeliveryCheckingKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.ScanDeliveryChecking = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //GeneratePresaleNumberKey
                    if (string.Compare(sections[0], GeneratePresaleNumberKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.GeneratePresaleNumber = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //AllowMultParInvoicesKey
                    if (string.Compare(sections[0], AllowMultParInvoicesKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.AllowMultParInvoices = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //SettReportInSalesUoMKey
                    if (string.Compare(sections[0], SettReportInSalesUoMKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.SettReportInSalesUoM = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //IncludeCredInNewParCalcKey
                    if (string.Compare(sections[0], IncludeCredInNewParCalcKey, StringComparison.OrdinalIgnoreCase) ==
                        0)
                    {
                        Config.IncludeCredInNewParCalc = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //DontAllowDecimalsInQtyKey
                    if (string.Compare(sections[0], DontAllowDecimalsInQtyKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.DontAllowDecimalsInQty = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //SendBackgroundPaymentsKey
                    if (string.Compare(sections[0], SendBackgroundPaymentsKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.SendBackgroundPayments = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //NewClientCanHaveDiscountKey
                    if (string.Compare(sections[0], NewClientCanHaveDiscountKey, StringComparison.OrdinalIgnoreCase) ==
                        0)
                    {
                        Config.NewClientCanHaveDiscount =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //CaptureImagesKey
                    if (string.Compare(sections[0], CaptureImagesKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.CaptureImages = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //ClientRtnNeededForQtyKey
                    if (string.Compare(sections[0], ClientRtnNeededForQtyKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.ClientRtnNeededForQty = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture);
                        continue;
                    }

                    //SelectReshipDateKey
                    if (string.Compare(sections[0], SelectReshipDateKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.SelectReshipDate = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //RouteReturnPasswordKey
                    if (string.Compare(sections[0], RouteReturnPasswordKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.RouteReturnPassword = sections[1];
                        continue;
                    }

                    //UseQuoteKey
                    if (string.Compare(sections[0], UseQuoteKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.UseQuote = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //MinimumAvailableNumbersKey
                    if (string.Compare(sections[0], MinimumAvailableNumbersKey, StringComparison.OrdinalIgnoreCase) ==
                        0)
                    {
                        Config.MinimumAvailableNumbers = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture);
                        continue;
                    }

                    //AdvanceSequencyNumKey
                    if (string.Compare(sections[0], AdvanceSequencyNumKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.AdvanceSequencyNum = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //UserCanChangePricesSalesKey
                    if (string.Compare(sections[0], UserCanChangePricesSalesKey, StringComparison.OrdinalIgnoreCase) ==
                        0)
                    {
                        Config.UserCanChangePricesSales =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        salesPriceInSetting = true;
                        continue;
                    }

                    //UserCanChangePricesCreditsKey
                    if (string.Compare(sections[0], UserCanChangePricesCreditsKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.UserCanChangePricesCredits =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        creditsPriceInSetting = true;
                        continue;
                    }

                    //HideTransfersKey
                    if (string.Compare(sections[0], HideTransfersKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.HideTransfers = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //MustCompleteInDeliveryCheckingKey
                    if (string.Compare(sections[0], MustCompleteInDeliveryCheckingKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.MustCompleteInDeliveryChecking =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //ShowAllProductsInCredits
                    if (string.Compare(sections[0], ShowAllProductsInCreditsKey, StringComparison.OrdinalIgnoreCase) ==
                        0)
                    {
                        Config.ShowAllProductsInCredits =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //DeleteWeightItemsMenu
                    if (string.Compare(sections[0], DeleteWeightItemsMenuKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.DeleteWeightItemsMenu = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //HidePresaleOptionsKey
                    if (string.Compare(sections[0], HidePresaleOptionsKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.HidePresaleOptions = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // ShowDiscountByPriceLevel
                    if (string.Compare(sections[0], ShowDiscountByPriceLevelKey, StringComparison.OrdinalIgnoreCase) ==
                        0)
                    {
                        Config.ShowDiscountByPriceLevel =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //UseFullTemplateKey
                    if (string.Compare(sections[0], UseFullTemplateKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.UseFullTemplate = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;

                        if (UseFullTemplate)
                        {
                            DataAccess.ActivityProvider.AddCustomActivitys(
                                "orderdetailsactivityphone,LaceupAndroidApp.FullTemplateActivity,orderdetailsactivitypad,LaceupAndroidApp.FullTemplateActivity");
                            DataAccess.ActivityProvider.AddCustomActivitys(
                                "ordercreditactivityphone,LaceupAndroidApp.FullTemplateActivity,ordercreditactivitypad,LaceupAndroidApp.FullTemplateActivity");
                        }

                        continue;
                    }

                    // AllowQtyConversionFactor
                    if (string.Compare(sections[0], AllowQtyConversionFactorKey, StringComparison.OrdinalIgnoreCase) ==
                        0)
                    {
                        Config.AllowQtyConversionFactor =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // DeleteEmptyDeliveriesKey
                    if (string.Compare(sections[0], DontDeleteEmptyDeliveriesKey, StringComparison.OrdinalIgnoreCase) ==
                        0)
                    {
                        Config.DontDeleteEmptyDeliveries =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // DeleteEmptyDeliveriesKey
                    if (string.Compare(sections[0], ViewGoalsKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.ViewGoals = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // EcoSkyWaterCustomEmailKey
                    if (string.Compare(sections[0], EcoSkyWaterCustomEmailKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.EcoSkyWaterCustomEmail = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // KeepAppUpdatedKey
                    if (string.Compare(sections[0], KeepAppUpdatedKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.KeepAppUpdated = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // HideSalesOrdersKey
                    if (string.Compare(sections[0], HideSalesOrdersKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.HideSalesOrders = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // AllowResetKey
                    if (string.Compare(sections[0], AllowResetKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.AllowReset = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // HideClearDataKey
                    if (string.Compare(sections[0], HideClearDataKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.HideClearData = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // MustEnterPostedDate
                    if (string.Compare(sections[0], MustEnterPostedDateKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.MustEnterPostedDate = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // NeedAccessForConfiguration
                    if (string.Compare(sections[0], NeedAccessForConfigurationKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.NeedAccessForConfiguration =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // PreviewOfferPriceInAddItemKey
                    if (string.Compare(sections[0], PreviewOfferPriceInAddItemKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.PreviewOfferPriceInAddItem =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // DeleteZeroItemsOnDelivery
                    if (string.Compare(sections[0], DeleteZeroItemsOnDeliveryKey, StringComparison.OrdinalIgnoreCase) ==
                        0)
                    {
                        Config.DeleteZeroItemsOnDelivery =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // SalesmanCanChangeSite
                    if (string.Compare(sections[0], SalesmanCanChangeSiteKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.SalesmanCanChangeSite = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // MustSetWeightInDeliveryKey
                    if (string.Compare(sections[0], MustSetWeightInDeliveryKey, StringComparison.OrdinalIgnoreCase) ==
                        0)
                    {
                        Config.MustSetWeightInDelivery = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // PrintCreditReportKey
                    if (string.Compare(sections[0], PrintCreditReportKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.PrintCreditReport = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // CheckAvailableBeforeSendingKey
                    if (string.Compare(sections[0], CheckAvailableBeforeSendingKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.CheckAvailableBeforeSending =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // SAPOrderStatusReportKey
                    if (string.Compare(sections[0], SAPOrderStatusReportKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.SAPOrderStatusReport = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // PresaleUseInventorySite
                    if (string.Compare(sections[0], PresaleUseInventorySiteKey, StringComparison.OrdinalIgnoreCase) ==
                        0)
                    {
                        Config.PresaleUseInventorySite = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // CannotOrderWithUnpaidInvoices
                    if (string.Compare(sections[0], CannotOrderWithUnpaidInvoicesKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.CannotOrderWithUnpaidInvoices =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // CanPayMoreThanOwnedKey
                    if (string.Compare(sections[0], CanPayMoreThanOwnedKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.CanPayMoreThanOwned = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // ShowAllEmailsAsDestination
                    if (string.Compare(sections[0], ShowAllEmailsAsDestinationKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.ShowAllEmailsAsDestination =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // SelectPriceFromPrevInvoicesKey
                    if (string.Compare(sections[0], SelectPriceFromPrevInvoicesKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.SelectPriceFromPrevInvoices =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // UsePallets
                    if (string.Compare(sections[0], UsePalletsKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.UsePallets = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // HideTaxesTotalPrint
                    if (string.Compare(sections[0], HideTaxesTotalPrintKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.HideTaxesTotalPrint = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // HideDiscountTotalPrint
                    if (string.Compare(sections[0], HideDiscountTotalPrintKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.HideDiscountTotalPrint = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // HideDiscountTotalPrint
                    if (string.Compare(sections[0], CanModifyWeightsOnDeliveriesKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.CanModifyWeightsOnDeliveries =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // ShowPricesInInventorySummary
                    if (string.Compare(sections[0], ShowPricesInInventorySummaryKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.ShowPricesInInventorySummary =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // AlertOrderWasNotSent
                    if (string.Compare(sections[0], AlertOrderWasNotSentKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.AlertOrderWasNotSent = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // RequestVehicleInformation
                    if (string.Compare(sections[0], RequestVehicleInformationKey, StringComparison.OrdinalIgnoreCase) ==
                        0)
                    {
                        Config.RequestVehicleInformation =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // ShowCostInTemplate
                    if (string.Compare(sections[0], ShowCostInTemplateKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.ShowCostInTemplate = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // ShowListPriceInAdvancedCatalog
                    if (string.Compare(sections[0], ShowListPriceInAdvancedCatalogKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.ShowListPriceInAdvancedCatalog =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // RecalculateOrdersAfterSync
                    if (string.Compare(sections[0], RecalculateOrdersAfterSyncKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.RecalculateOrdersAfterSync =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // ShowLowestPriceInTemplateKey
                    if (string.Compare(sections[0], ShowLowestPriceInTemplateKey, StringComparison.OrdinalIgnoreCase) ==
                        0)
                    {
                        Config.ShowLowestPriceInTemplate =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // ShowWeightOnInventorySummary
                    if (string.Compare(sections[0], ShowWeightOnInventorySummaryKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.ShowWeightOnInventorySummary =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // ShowLowestPriceLevel
                    if (string.Compare(sections[0], ShowLowestPriceLevelKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.ShowLowestPriceLevel = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // MustCreatePaymentDeposit
                    if (string.Compare(sections[0], MustCreatePaymentDepositKey, StringComparison.OrdinalIgnoreCase) ==
                        0)
                    {
                        Config.MustCreatePaymentDeposit =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // MustSelectRouteToSync
                    if (string.Compare(sections[0], MustSelectRouteToSyncKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.MustSelectRouteToSync = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // ShowLastThreeVisitsOnTemplate
                    if (string.Compare(sections[0], ShowLastThreeVisitsOnTemplateKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.ShowLastThreeVisitsOnTemplate =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // MustSelectSingleInvoicePerPayment
                    if (string.Compare(sections[0], BlockMultipleCollectPaymetsKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.BlockMultipleCollectPaymets =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // MustSelectSingleInvoicePerPayment
                    if (string.Compare(sections[0], SelectWarehouseForSalesKey, StringComparison.OrdinalIgnoreCase) ==
                        0)
                    {
                        Config.SelectWarehouseForSales = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // DonNovoCustomization
                    if (string.Compare(sections[0], DonNovoCustomizationKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.DonNovoCustomization = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // UseVisitsTemplateInSales
                    if (string.Compare(sections[0], UseVisitsTemplateInSalesKey, StringComparison.OrdinalIgnoreCase) ==
                        0)
                    {
                        Config.UseVisitsTemplateInSales =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // AllowWorkOrder
                    if (string.Compare(sections[0], AllowWorkOrderKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.AllowWorkOrder = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // NotifyNewerDataInOS
                    if (string.Compare(sections[0], NotifyNewerDataInOSKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.NotifyNewerDataInOS = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // AmericanEagleCustomization
                    if (string.Compare(sections[0], AmericanEagleCustomizationKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.AmericanEagleCustomization =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // ShowSuggestedButton
                    if (string.Compare(sections[0], ShowSuggestedButtonKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.ShowSuggestedButton = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // DisableSendCatalogWithPrices
                    if (string.Compare(sections[0], DisableSendCatalogWithPricesKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.DisableSendCatalogWithPrices =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // RecalculateRoutesOnSyncData
                    if (string.Compare(sections[0], RecalculateRoutesOnSyncDataKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.RecalculateRoutesOnSyncData =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // CanSelectTermsOnCreateClient
                    if (string.Compare(sections[0], CanSelectTermsOnCreateClientKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.CanSelectTermsOnCreateClient =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // AlertPrintPaymentBeforeSaving
                    if (string.Compare(sections[0], AlertPrintPaymentBeforeSavingKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.AlertPrintPaymentBeforeSaving =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // CatalogReturnsInDefaultUOM
                    if (string.Compare(sections[0], CatalogReturnsInDefaultUOMKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.CatalogReturnsInDefaultUOM =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // UseReturnOrder
                    if (string.Compare(sections[0], UseReturnOrderKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.UseReturnOrder = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // IncludeCreditInvoiceForPayments
                    if (string.Compare(sections[0], IncludeCreditInvoiceForPaymentsKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.IncludeCreditInvoiceForPayments =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // MarmiaCustomization
                    if (string.Compare(sections[0], MarmiaCustomizationKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.MarmiaCustomization = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // NotificationsInSelfService
                    if (string.Compare(sections[0], NotificationsInSelfServiceKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.NotificationsInSelfService =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // CanDepositChecksWithDifDates
                    if (string.Compare(sections[0], CanDepositChecksWithDifDatesKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.CanDepositChecksWithDifDates =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // TemplateSearchByContains
                    if (string.Compare(sections[0], TemplateSearchByContainsKey, StringComparison.OrdinalIgnoreCase) ==
                        0)
                    {
                        Config.TemplateSearchByContains =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //  UseLaceupDataInSalesReport
                    if (string.Compare(sections[0], UseLaceupDataInSalesReportKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.UseLaceupDataInSalesReport =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // AskOffersBeforeAdding
                    if (string.Compare(sections[0], AskOffersBeforeAddingKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.AskOffersBeforeAdding = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // SearchAllProductsInTemplate
                    if (string.Compare(sections[0], SearchAllProductsInTemplateKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.SearchAllProductsInTemplate =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // AdvancedTemplateFocusSearch
                    if (string.Compare(sections[0], AdvancedTemplateFocusSearchKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.AdvancedTemplateFocusSearch =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // MustSelectReasonForFreeItem
                    if (string.Compare(sections[0], MustSelectReasonForFreeItemKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.MustSelectReasonForFreeItem =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // CanEditCreditsInDelivery
                    if (string.Compare(sections[0], CanEditCreditsInDeliveryKey, StringComparison.OrdinalIgnoreCase) ==
                        0)
                    {
                        Config.CanEditCreditsInDelivery =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //  CanChangeSalesmanName
                    if (string.Compare(sections[0], CanChangeSalesmanNameKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.CanChangeSalesmanName = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //  PriceLevelComment
                    if (string.Compare(sections[0], PriceLevelCommentKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.PriceLevelComment = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //  GetUOMSOnCommand
                    if (string.Compare(sections[0], GetUOMSOnCommandKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.GetUOMSOnCommand = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //  EnterWeightInCredits
                    if (string.Compare(sections[0], EnterWeightInCreditsKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.EnterWeightInCredits = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //  ShowVisitsInfoInClients
                    if (string.Compare(sections[0], ShowVisitsInfoInClientsKey, StringComparison.OrdinalIgnoreCase) ==
                        0)
                    {
                        Config.ShowVisitsInfoInClients = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //  UpdateInventoryInPresale
                    if (string.Compare(sections[0], UpdateInventoryInPresaleKey, StringComparison.OrdinalIgnoreCase) ==
                        0)
                    {
                        Config.UpdateInventoryInPresale =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //  SendTempPaymentsInBackground
                    if (string.Compare(sections[0], SendTempPaymentsInBackgroundKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.SendTempPaymentsInBackground =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //  DontGenerateLoadPrintedId
                    if (string.Compare(sections[0], DontGenerateLoadPrintedIdKey, StringComparison.OrdinalIgnoreCase) ==
                        0)
                    {
                        Config.DontGenerateLoadPrintedId =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //  ShowBelow0InAdvancedTemplate
                    if (string.Compare(sections[0], ShowBelow0InAdvancedTemplateKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.ShowBelow0InAdvancedTemplate =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //  ShowRetailPriceForAddItem
                    if (string.Compare(sections[0], ShowRetailPriceForAddItemKey, StringComparison.OrdinalIgnoreCase) ==
                        0)
                    {
                        Config.ShowRetailPriceForAddItem =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //  RoundTaxPerLine
                    if (string.Compare(sections[0], RoundTaxPerLineKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.RoundTaxPerLine = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //  AllowExchange
                    if (string.Compare(sections[0], AllowExchangeKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.AllowExchange = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //  UseBrotherPrinter
                    if (string.Compare(sections[0], UsePaymentDiscountKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.UsePaymentDiscount = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //  OffersAddCommentKey
                    if (string.Compare(sections[0], OffersAddCommentKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.OffersAddComment = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //  SendZplOrder
                    if (string.Compare(sections[0], SendZplOrderKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.SendZplOrder = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //  MultiplyConversionForCostInIracarAddItem
                    if (string.Compare(sections[0], MultiplyConversionByCostInIracarAddItemKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.MultiplyConversionByCostInIracarAddItem =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //  IncludeAvgWeightInCatalogPriceKey
                    if (string.Compare(sections[0], IncludeAvgWeightInCatalogPriceKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.IncludeAvgWeightInCatalogPrice =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //  ShowOldReportsRegardlessKey
                    if (string.Compare(sections[0], ShowOldReportsRegardlessKey, StringComparison.OrdinalIgnoreCase) ==
                        0)
                    {
                        Config.ShowOldReportsRegardless =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //  CanLogoutKey
                    if (string.Compare(sections[0], CanLogoutKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.CanLogout = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //  DontRoundInUI
                    if (string.Compare(sections[0], DontRoundInUIKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.DontRoundInUI = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //  MustScanInTransfer
                    if (string.Compare(sections[0], MustScanInTransferKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.MustScanInTransfer = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //  TimeSheetAutomaticClockIn
                    if (string.Compare(sections[0], TimeSheetAutomaticClockInKey, StringComparison.OrdinalIgnoreCase) ==
                        0)
                    {
                        Config.TimeSheetAutomaticClockIn =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //  DontSortCompaniesByName
                    if (string.Compare(sections[0], DontSortCompaniesByNameKey, StringComparison.OrdinalIgnoreCase) ==
                        0)
                    {
                        Config.DontSortCompaniesByName = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //  PrintAllInventoriesInInvSummary
                    if (string.Compare(sections[0], PrintAllInventoriesInInvSummaryKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.PrintAllInventoriesInInvSummary =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //  DicosaCustomization
                    if (string.Compare(sections[0], DicosaCustomizationKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.DicosaCustomization = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //  CanModifyScannedWeight
                    if (string.Compare(sections[0], CanModifyEnteredWeightKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.CanModifyEnteredWeight = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //  ShowServiceReport
                    if (string.Compare(sections[0], ShowServiceReportKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.ShowServiceReport = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //  MilagroCustomization
                    if (string.Compare(sections[0], MilagroCustomizationKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.MilagroCustomization = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //  AllowOtherCharges
                    if (string.Compare(sections[0], AllowOtherChargesKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.AllowOtherCharges = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //  CheckIfShipdateLocked
                    if (string.Compare(sections[0], CheckIfShipdateLockedKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.CheckIfShipdateLocked = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //  ShowDescriptionInSelfServiceCatalog
                    if (string.Compare(sections[0], ShowDescriptionInSelfServiceCatalogKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.ShowDescriptionInSelfServiceCatalog =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //  CanChangeFinalizedInvoices
                    if (string.Compare(sections[0], CanChangeFinalizedInvoicesKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.CanChangeFinalizedInvoices =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // AllowNotifications
                    if (string.Compare(sections[0], AllowNotificationsKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.AllowNotifications = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // ShowLowestAcceptableOnWarning
                    if (string.Compare(sections[0], ShowLowestAcceptableOnWarningKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.ShowLowestAcceptableOnWarning =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // ShowPromoCheckbox
                    if (string.Compare(sections[0], ShowPromoCheckboxKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.ShowPromoCheckbox = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // ShowImageInCatalog
                    if (string.Compare(sections[0], ShowImageInCatalogKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.ShowImageInCatalog = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    // SoutoBottomEmailText
                    if (string.Compare(sections[0], SoutoBottomEmailTextKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.SoutoBottomEmailText = sections[1];
                        continue;
                    }

                    //  HideSelectSitesFromMenu
                    if (string.Compare(sections[0], HideSelectSitesFromMenuKey, StringComparison.OrdinalIgnoreCase) ==
                        0)
                    {
                        Config.HideSelectSitesFromMenu = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //  CheckInventoryInLoad
                    if (string.Compare(sections[0], CheckInventoryInLoadKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.CheckInventoryInLoad = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //  CustomerInCreditHold
                    if (string.Compare(sections[0], CustomerInCreditHoldKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.CustomerInCreditHold = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //  DeliveryMustScanProducts
                    if (string.Compare(sections[0], DeliveryMustScanProductsKey, StringComparison.OrdinalIgnoreCase) ==
                        0)
                    {
                        Config.DeliveryMustScanProducts =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //  PrintExternalInvoiceAsOrder
                    if (string.Compare(sections[0], PrintExternalInvoiceAsOrderKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.PrintExternalInvoiceAsOrder =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //  UseProductionForPayments
                    if (string.Compare(sections[0], UseProductionForPaymentsKey, StringComparison.OrdinalIgnoreCase) ==
                        0)
                    {
                        Config.UseProductionForPayments =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //  UseCatalogWithFullTemplate
                    if (string.Compare(sections[0], UseCatalogWithFullTemplateKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.UseCatalogWithFullTemplate =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //  CanModifiyDeliveryWithScanning
                    if (string.Compare(sections[0], CanModifiyDeliveryWithScanningKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.CanModifiyDeliveryWithScanning =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //  ShowSentTransactions
                    if (string.Compare(sections[0], ShowSentTransactionsKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.ShowSentTransactions = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //  ForceSingleScan
                    if (string.Compare(sections[0], ForceSingleScanKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.ForceSingleScan = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //    EnableUsernameandPassword
                    if (string.Compare(sections[0], EnableUsernameandPasswordKey, StringComparison.OrdinalIgnoreCase) ==
                        0)
                    {
                        Config.EnableUsernameandPassword =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //    SavePaymentsByInvoiceNumber
                    if (string.Compare(sections[0], SavePaymentsByInvoiceNumberKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.SavePaymentsByInvoiceNumber =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //    SendPaymentsInEOD
                    if (string.Compare(sections[0], SendPaymentsInEODKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.SendPaymentsInEOD = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //    ShowExpensesInEOD
                    if (string.Compare(sections[0], ShowExpensesInEODKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.ShowExpensesInEOD = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //    DontCalculateOffersAfterPriceChanged
                    if (string.Compare(sections[0], DontCalculateOffersAfterPriceChangedKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.DontCalculateOffersAfterPriceChanged =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //    RequireCodeForVoidInvoices
                    if (string.Compare(sections[0], RequireCodeForVoidInvoicesKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.RequireCodeForVoidInvoices =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //    ShowPaymentSummary
                    if (string.Compare(sections[0], ShowPaymentSummaryKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.ShowPaymentSummary = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //    CoolerCoCustomization
                    if (string.Compare(sections[0], CoolerCoCustomizationKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.CoolerCoCustomization = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //    CanChangeRoutesOrder
                    if (string.Compare(sections[0], CanChangeRoutesOrderKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.CanChangeRoutesOrder = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //    CalculateOffersAutomatically
                    if (string.Compare(sections[0], CalculateOffersAutomaticallyKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.CalculateOffersAutomatically =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //    CalculateTaxPerLine
                    if (string.Compare(sections[0], CalculateTaxPerLineKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.CalculateTaxPerLine = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //    AddAllowanceToPriceDuringDEX
                    if (string.Compare(sections[0], AddAllowanceToPriceDuringDEXKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.AddAllowanceToPriceDuringDEX =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //    DontIncludePackageParameterDexUpc
                    if (string.Compare(sections[0], DontIncludePackageParameterDexUpcKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.DontIncludePackageParameterDexUpc =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //    OnlyShowCostInProductDetails
                    if (string.Compare(sections[0], OnlyShowCostInProductDetailsKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.OnlyShowCostInProductDetails =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //    DexUpcCharacterLimits
                    if (string.Compare(sections[0], DexUpcCharacterLimitsKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.DexUpcCharacterLimits = Convert.ToInt32(sections[1]);
                        continue;
                    }

                    //  MustSelectDepartment
                    if (string.Compare(sections[0], MustSelectDepartmentKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.MustSelectDepartment = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //GenerateProjectionKey
                    if (string.Compare(sections[0], GenerateProjectionKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.GenerateProjection = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //PrintClientTotalOpenBalanceKey
                    if (string.Compare(sections[0], PrintClientTotalOpenBalanceKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.PrintClientTotalOpenBalance =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //HidePONumberKey
                    if (string.Compare(sections[0], HidePONumberKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.HidePONumber = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //HidePriceInTransactionKey
                    if (string.Compare(sections[0], HidePriceInTransactionKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.HidePriceInTransaction = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //OnlyPresaleKey
                    if (string.Compare(sections[0], OnlyPresaleKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.OnlyPresale = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //LotIsMandatoryBeforeFinalizeKey
                    if (string.Compare(sections[0], LotIsMandatoryBeforeFinalizeKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.LotIsMandatoryBeforeFinalize =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //CheckDueInvoicesInCreateOrderKey
                    if (string.Compare(sections[0], CheckDueInvoicesInCreateOrderKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.CheckDueInvoicesInCreateOrder =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //ConsignmentPresaleOnlyKey
                    if (string.Compare(sections[0], ConsignmentPresaleOnlyKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.ConsignmentPresaleOnly = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //TruckTemperatureReqKey
                    if (string.Compare(sections[0], TruckTemperatureReqKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.TruckTemperatureReq = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //MasterDeviceKey
                    if (!MasterDevice &&
                        string.Compare(sections[0], MasterDeviceKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.MasterDevice = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //ConsignmentBetaKey
                    if (string.Compare(sections[0], ConsignmentBetaKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.ConsignmentBeta = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //BetaCofigurationViewKey
                    if (string.Compare(sections[0], BetaConfigurationViewKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.BetaConfigurationView = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //HidenItemCustomizationKey
                    if (string.Compare(sections[0], HiddenItemCustomizationKey, StringComparison.OrdinalIgnoreCase) ==
                        0)
                    {
                        Config.HiddenItemCustomization = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //CheckDueInvoicesQtyInCreateOrderKey
                    if (string.Compare(sections[0], CheckDueInvoicesQtyInCreateOrderKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.CheckDueInvoicesQtyInCreateOrder =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture);
                        continue;
                    }

                    //AutomaticClockOutTime
                    if (string.Compare(sections[0], AutomaticClockOutTimeKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.AutomaticClockOutTime = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture);
                        continue;
                    }

                    //MandatoryBreakDuration
                    if (string.Compare(sections[0], MandatoryBreakDurationKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.MandatoryBreakDuration = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture);
                        continue;
                    }

                    //ForceBreakInMinutes
                    if (string.Compare(sections[0], ForceBreakInMinutesKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.ForceBreakInMinutes = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture);
                        continue;
                    }

                    //ForceBreakInMinutes
                    if (string.Compare(sections[0], OtherChargesTypeKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.OtherChargesType = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture);
                        continue;
                    }

                    //FreightType
                    if (string.Compare(sections[0], FreightTypeKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.FreightType = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture);
                        continue;
                    }

                    //OtherChargesComments
                    if (string.Compare(sections[0], OtherChargesCommentsKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.OtherChargesComments = sections[1];
                        continue;
                    }

                    //FreightComments
                    if (string.Compare(sections[0], FreightCommentsKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.FreightComments = sections[1];
                        continue;
                    }

                    //OtherChargesVale
                    if (string.Compare(sections[0], OtherChargesValeKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.OtherChargesVale = Convert.ToSingle(sections[1], CultureInfo.InvariantCulture);
                        continue;
                    }

                    //FreightVale
                    if (string.Compare(sections[0], FreightValeKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.FreightVale = Convert.ToSingle(sections[1], CultureInfo.InvariantCulture);
                        continue;
                    }

                    //UseBigFontForPrintDateKey
                    if (string.Compare(sections[0], UseBigFontForPrintDateKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.UseBigFontForPrintDate = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //ZeroSoldInConsignmentKey
                    if (string.Compare(sections[0], ZeroSoldInConsignmentKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.ZeroSoldInConsignment = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //ScanBaseTradingKey
                    if (string.Compare(sections[0], ScanBasedTradingKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.ScanBasedTrading = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //BetaFragmentsKey
                    if (string.Compare(sections[0], BetaFragmentsKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.BetaFragments = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //SelfServiceKey
                    if (string.Compare(sections[0], SelfServiceKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.SelfService = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //DollyReminderKey
                    if (string.Compare(sections[0], DollyReminderKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.DollyReminder = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //GoToMainKey
                    if (string.Compare(sections[0], GoToMainKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.GoToMain = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //UseFutureRouteExKey
                    if (string.Compare(sections[0], UseFutureRouteExKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.UseFutureRouteEx = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //AutoCalculateRouteReturnKey
                    if (string.Compare(sections[0], AutoCalculateRouteReturnKey, StringComparison.OrdinalIgnoreCase) ==
                        0)
                    {
                        Config.AutoCalculateRouteReturn =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //UseSurveyKey
                    if (string.Compare(sections[0], UseSurveyKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.UseSurvey = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //HideOpenInvoiceTotalKey
                    if (string.Compare(sections[0], HideOpenInvoiceTotalKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.HideOpenInvoiceTotal = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //SalesHistoryForCreditsKey
                    if (string.Compare(sections[0], SalesHistoryForCreditsKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.SalesHistoryForCredits = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //UseCreditAccountKey
                    if (string.Compare(sections[0], UseCreditAccountKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.UseCreditAccount = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //HideTransactionsTotalKey
                    if (string.Compare(sections[0], HideTransactionsTotalKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.HideTransactionsTotal = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //HidePaymentsTotalKey
                    if (string.Compare(sections[0], HidePaymentsTotalKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.HidePaymentsTotal = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //HideSubTotalOrderKey
                    if (string.Compare(sections[0], HideSubTotalOrderKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.HideSubTotalOrder = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //SalesByDepartmentKey
                    if (string.Compare(sections[0], SalesByDepartmentKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.SalesByDepartment = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //CreditReasonInLineKey
                    if (string.Compare(sections[0], CreditReasonInLineKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.CreditReasonInLine = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //AcceptLoadEditableKey
                    if (string.Compare(sections[0], AcceptLoadEditableKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.AcceptLoadEditable = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //ShowProductsPerUoMKey
                    if (string.Compare(sections[0], ShowProductsPerUoMKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.ShowProductsPerUoM = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;

                        if (ShowProductsPerUoM)
                            DataAccess.ActivityProvider.AddCustomActivitys(
                                "productlistactivityphone,LaceupAndroidApp.ProductCatalogActivity,productlistactivitypad,LaceupAndroidApp.ProductCatalogActivity");

                        continue;
                    }

                    //CycleCountAtEoDKey
                    if (string.Compare(sections[0], EnableCycleCountKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.EnableCycleCount = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //ViewPrintInvPasswordKey
                    if (string.Compare(sections[0], ViewPrintInvPasswordKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.ViewPrintInvPassword = sections[1];
                        continue;
                    }

                    //CycleCountPasswordKey
                    if (string.Compare(sections[0], CycleCountPasswordKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.CycleCountPassword = sections[1];
                        continue;
                    }

                    //CalculateInvForEmptyTruckKey
                    if (string.Compare(sections[0], CalculateInvForEmptyTruckKey, StringComparison.OrdinalIgnoreCase) ==
                        0)
                    {
                        Config.CalculateInvForEmptyTruck =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //MustGenerateProjectionKey
                    if (string.Compare(sections[0], MustGenerateProjectionKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.MustGenerateProjection = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //LoadByOrderHistoryKey
                    if (string.Compare(sections[0], LoadByOrderHistoryKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.LoadByOrderHistory = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //EmptyEndingInventoryKey
                    if (string.Compare(sections[0], EmptyEndingInventoryKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.EmptyEndingInventory = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //BonsuisseCustomizationKey
                    if (string.Compare(sections[0], BonsuisseCustomizationKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.BonsuisseCustomization = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //UseBiggerUoMInLoadHistoryKey
                    if (string.Compare(sections[0], UseBiggerUoMInLoadHistoryKey, StringComparison.OrdinalIgnoreCase) ==
                        0)
                    {
                        Config.UseBiggerUoMInLoadHistory =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //TotalsByUoMInPdfKey
                    if (string.Compare(sections[0], TotalsByUoMInPdfKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.TotalsByUoMInPdf = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //SplitDeliveryByDepartmentKey
                    if (string.Compare(sections[0], SplitDeliveryByDepartmentKey, StringComparison.OrdinalIgnoreCase) ==
                        0)
                    {
                        Config.SplitDeliveryByDepartment =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //PrintNoServiceInSalesReportsKey
                    if (string.Compare(sections[0], PrintNoServiceInSalesReportsKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.PrintNoServiceInSalesReports =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //XlsmProviderKey
                    if (string.Compare(sections[0], XlsxProviderKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.XlsxProvider = sections[1];
                        continue;
                    }

                    //UseCatalogKey
                    if (string.Compare(sections[0], UseCatalogKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        UseCatalog = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //SelfServiceInvitationKey
                    if (string.Compare(sections[0], SelfServiceInvitationKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.SelfServiceInvitation = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture);
                        continue;
                    }

                    //CarolinaCustomizationKey
                    if (string.Compare(sections[0], CarolinaCustomizationKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.CarolinaCustomization = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //IncludePresaleInSalesReportKey
                    if (string.Compare(sections[0], IncludePresaleInSalesReportKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.IncludePresaleInSalesReport =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //SelectSalesRepForInvoiceKey
                    if (string.Compare(sections[0], SelectSalesRepForInvoiceKey, StringComparison.OrdinalIgnoreCase) ==
                        0)
                    {
                        Config.SelectSalesRepForInvoice =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //AssetTrackingKey
                    if (string.Compare(sections[0], AssetTrackingKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.AssetTracking = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //DeliveryEditableKey
                    if (string.Compare(sections[0], DeliveryEditableKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.DeliveryEditable = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //DisablePrintEndOfDayReportKey
                    if (string.Compare(sections[0], DisablePrintEndOfDayReportKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.DisablePrintEndOfDayReport =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //HideWarehouseOHInLoadKey
                    if (string.Compare(sections[0], HideWarehouseOHInLoadKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.HideWarehouseOHInLoad = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //PONumberMaxLengthKey
                    if (string.Compare(sections[0], PONumberMaxLengthKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.PONumberMaxLength = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture);
                        continue;
                    }

                    //UseLotExpirationKey
                    if (string.Compare(sections[0], UseLotExpirationKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.UseLotExpiration = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //ItemGroupedTemplateKey
                    if (string.Compare(sections[0], ItemGroupedTemplateKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.ItemGroupedTemplate = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //DisolCustomization
                    if (string.Compare(sections[0], DisolCrapKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.DisolCrap = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //WarehouseInventoryOnDemandKey
                    if (string.Compare(sections[0], WarehouseInventoryOnDemandKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.WarehouseInventoryOnDemand =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //AssetStaysMandatoryKey
                    if (string.Compare(sections[0], AssetStaysMandatoryKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.AssetStaysMandatory = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //AddRelatedItemInCreditKey
                    if (string.Compare(sections[0], AddRelatedItemInCreditKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.AddRelatedItemInCredit = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //UseOffersInCreditKey
                    if (string.Compare(sections[0], UseOffersInCreditKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.UseOffersInCredit = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //UseLaceupAdvancedCatalogKey
                    if (string.Compare(sections[0], UseLaceupAdvancedCatalogKey, StringComparison.OrdinalIgnoreCase) ==
                        0)
                    {
                        Config.UseLaceupAdvancedCatalog =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //SelfServiceUserKey
                    if (string.Compare(sections[0], SelfServiceUserKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.SelfServiceUser = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //UseClientSortKey
                    if (string.Compare(sections[0], UseClientSortKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.UseClientSort = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //UseDisolSurveyKey
                    if (string.Compare(sections[0], UseDisolSurveyKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.UseDisolSurvey = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //UseDisolSurveyKey
                    if (string.Compare(sections[0], DisolSurveyProductsKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.DisolSurveyProducts = sections[1].Trim();
                        continue;
                    }

                    //ShowListPriceInAddItemKey
                    if (string.Compare(sections[0], ShowListPriceInAddItemKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.ShowListPriceInAddItem = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //CanChangeUomInLoadKey
                    if (string.Compare(sections[0], CanChangeUomInLoadKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.CanChangeUomInLoad = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //IracarCustomizationKey
                    if (string.Compare(sections[0], IracarCustomizationKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.IracarCustomization = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //NewAddItemRandomWeightKey
                    if (string.Compare(sections[0], NewAddItemRandomWeightKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.NewAddItemRandomWeight = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //SetShipDateKey
                    if (string.Compare(sections[0], SetShipDateKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.SetShipDate = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //LockOrderAfterPrintedKey
                    if (string.Compare(sections[0], LockOrderAfterPrintedKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.LockOrderAfterPrinted = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    //PackageInReturnPresaleKey
                    if (string.Compare(sections[0], PackageInReturnPresaleKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.PackageInReturnPresale = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], EnableSampleOrderKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.EnableSampleOrder = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], EnablePaymentsByTermsKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        if (sections[1] == "1")
                            Config.EnablePaymentsByTerms = "Net 30 Days";
                        else
                            Config.EnablePaymentsByTerms = sections[1];
                        continue;
                    }

                    if (string.Compare(sections[0], RequireLotForDumpsKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.RequireLotForDumps = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], HidePriceInSelfServiceKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.HidePriceInSelfService = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], HideOHinSelfServiceKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.HideOHinSelfService = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], ShowWarehouseInvInSummaryKey, StringComparison.OrdinalIgnoreCase) ==
                        0)
                    {
                        Config.ShowWarehouseInvInSummary =
                            Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], BarcodeDecoderKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        BarcodeDecoder = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture);
                        continue;
                    }

                    if (string.Compare(sections[0], PrintLabelHeightKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Config.PrintLabelHeight = sections[1];
                        continue;
                    }

                    if (string.Compare(sections[0], AllowSelectPriceLevelKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        AllowSelectPriceLevel = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], UpdateInventoryRegardlessKey, StringComparison.OrdinalIgnoreCase) ==
                        0)
                    {
                        UpdateInventoryRegardless = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], ShowOnlyInvoiceForSalesmanKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        ShowOnlyInvoiceForSalesman = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], HideInvoicesAndBalanceKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        HideInvoicesAndBalance = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], ImageInOrderMandatoryKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        ImageInOrderMandatory = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], ImageInNoServiceMandatoryKey, StringComparison.OrdinalIgnoreCase) ==
                        0)
                    {
                        ImageInNoServiceMandatory = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], PrintCopiesInFinalizeBatchKey,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        PrintCopiesInFinalizeBatch = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture);
                        continue;
                    }

                    if (string.Compare(sections[0], UseFastPrinterKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        UseFastPrinter = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture) > 0;
                        continue;
                    }

                    if (string.Compare(sections[0], ParLevelHistoryNumVisitKey, StringComparison.OrdinalIgnoreCase) ==
                        0)
                    {
                        ParLevelHistoryNumVisit = Convert.ToInt32(sections[1], CultureInfo.InvariantCulture);
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    Logger.CreateLog(ex);
                }
            }

            if (AddItemInDefaultUoM)
            {
                useCatalog = false;
                UseLaceupAdvancedCatalog = false;
            }

            if (UseFullTemplate && !Config.UseCatalogWithFullTemplate)
            {
                UseCatalog = false;
                DataAccess.ActivityProvider.AddCustomActivitys(
                    "productlistactivityphone,LaceupAndroidApp.ProductListActivity,productlistactivitypad,LaceupAndroidApp.ProductListActivity");
            }

            if (Config.UseCatalogWithFullTemplate && Config.UseFullTemplate) UseCatalog = true;

            if (UseVisitsTemplateInSales) UseLaceupAdvancedCatalog = true;

            if (UseDraggableTemplate) Config.UseClientSort = true;

            if (Config.UseLotExpiration)
            {
                Config.UseLot = true;
                Config.LotIsMandatory = true;
            }

            if (Config.IracarCustomization)
            {
                DataAccess.ActivityProvider.AddCustomActivitys(
                    "additemactivityphone,LaceupAndroidApp.IracarAddItemActivity,additemactivitypad,LaceupAndroidApp.IracarAddItemActivity");
                UseCatalog = false;
                DataAccess.ActivityProvider.AddCustomActivitys(
                    "productlistactivityphone,LaceupAndroidApp.ProductListActivity,productlistactivitypad,LaceupAndroidApp.ProductListActivity");
            }

            if (Config.NewAddItemRandomWeight)
                DataAccess.ActivityProvider.AddCustomActivitys(
                    "additemactivityphone,LaceupAndroidApp.AddItemRandomWeightActivity,additemactivitypad,LaceupAndroidApp.IracarAddItemActivity");

            if (Config.UseCatalog)
                DataAccess.ActivityProvider.AddCustomActivitys(
                    "productlistactivityphone,LaceupAndroidApp.ProductCatalogActivity,productlistactivitypad,LaceupAndroidApp.ProductCatalogActivity");

            if (Config.UseLaceupAdvancedCatalog)
            {
                DataAccess.ActivityProvider.AddCustomActivitys(
                    "orderdetailsactivityphone,LaceupAndroidApp.AdvancedTemplateActivity,orderdetailsactivitypad,LaceupAndroidApp.AdvancedTemplateActivity");
                DataAccess.ActivityProvider.AddCustomActivitys(
                    "ordercreditactivityphone,LaceupAndroidApp.AdvancedTemplateActivity,ordercreditactivitypad,LaceupAndroidApp.AdvancedTemplateActivity");
            }

            if (Config.ItemGroupedTemplate)
            {
                DataAccess.ActivityProvider.AddCustomActivitys(
                    "orderdetailsactivityphone,LaceupAndroidApp.NewOrderTemplateActivity,orderdetailsactivitypad,LaceupAndroidApp.NewOrderTemplateActivity");
                DataAccess.ActivityProvider.AddCustomActivitys(
                    "ordercreditactivityphone,LaceupAndroidApp.NewCreditTemplateActivity,ordercreditactivitypad,LaceupAndroidApp.NewCreditTemplateActivity");
                DataAccess.ActivityProvider.AddCustomActivitys(
                    "productlistactivityphone,LaceupAndroidApp.ProductListActivity,productlistactivitypad,LaceupAndroidApp.ProductListActivity");
            }

            if (Config.ParInConsignment) Config.PrinterToUse = "LaceUPMobileClassesIOS.AdvanceBatteryPrinter";

            if (MasterDevice) CanChangeSalesmanId = false;

            if (Config.HidePriceInTransaction)
            {
                Config.HidePriceInPrintedLine = true;
                Config.HideOpenInvoiceTotal = true;
                Config.HidePaymentsTotal = true;
                Config.HidePriceInPrintedLine = true;
                Config.HideTotalInPrintedLine = true;
                Config.HideTotalOrder = true;
                Config.HideSubTotalOrder = true;
            }

            if (DonNovoCustomization) GeneratePresaleNumber = true;

            //EnableAdvancedLogin = DataAccess.CheckCommunicatorVersion(DataAccess.CommunicatorVersion, "60.0.0");

            if (EnableUsernameandPassword) EnableAdvancedLogin = true;

            SaveSettings();

            //if (BackGroundSync && !BackgroundDataSync.Running)
            //    BackgroundDataSync.StartThreadh();
        }

        private static string OSVersion()
        {
            var s = helper?.GetOsVersion();
            return s ?? "";
        }

        public static string SerializeConfig()
        {
            var sb = new StringBuilder();

            sb.Append("Version");
            sb.Append("=");
            sb.Append(Version);
            sb.Append(Environment.NewLine);

            sb.Append("DeviceId");
            sb.Append("=");
            sb.Append(DeviceId);
            sb.Append(Environment.NewLine);

            sb.Append(Config.VendorIdKey);
            sb.Append("=");
            sb.Append(SalesmanId);
            sb.Append(Environment.NewLine);

            sb.Append(RouteNameKey);
            sb.Append("=");
            sb.Append(RouteName);
            sb.Append(Environment.NewLine);

            sb.Append(VendorNameKey);
            sb.Append("=");
            sb.Append(VendorName);
            sb.Append(Environment.NewLine);

            sb.Append(PortKey);
            sb.Append("=");
            sb.Append(Port);
            sb.Append(Environment.NewLine);

            sb.Append(IPAddressGatewayKey);
            sb.Append("=");
            sb.Append(IPAddressGateway);
            sb.Append(Environment.NewLine);

            sb.Append(LanKey);
            sb.Append("=");
            sb.Append(LanAddress);
            sb.Append(Environment.NewLine);

            sb.Append(SSIDKey);
            sb.Append("=");
            sb.Append(SSID);
            sb.Append(Environment.NewLine);

            sb.Append("Communicator Version");
            sb.Append("=");
            sb.Append(DataAccess.CommunicatorVersion);
            sb.Append(Environment.NewLine);

            sb.Append("Companies");

            foreach (var ci in CompanyInfo.Companies)
            {
                sb.Append("  Company name : " + ci.CompanyName);
                sb.Append(Environment.NewLine);
                sb.Append("  Company CompanyAddress1 : " + ci.CompanyAddress1);
                sb.Append(Environment.NewLine);
                sb.Append("  Company CompanyAddress2 : " + ci.CompanyAddress2);
                sb.Append(Environment.NewLine);
                sb.Append("  Company CompanyPhone : " + ci.CompanyPhone);
                sb.Append(Environment.NewLine);
                sb.Append("  Company DUNS : " + ci.DUNS);
                sb.Append(Environment.NewLine);
                sb.Append("  Company Location : " + ci.Location);
                sb.Append(Environment.NewLine);
                sb.Append("  Company CommId : " + ci.CommId);
                sb.Append(Environment.NewLine);
                sb.Append("  Company Fax : " + ci.CompanyFax);
                sb.Append(Environment.NewLine);
                sb.Append("  Company Licenses : " + ci.CompanyLicenses);
                sb.Append(Environment.NewLine);
            }

            sb.Append(ParLevelHistoryNumVisitKey);
            sb.Append("=");
            sb.Append(ParLevelHistoryNumVisit);
            sb.Append(Environment.NewLine);

            sb.Append(UseFastPrinterKey);
            sb.Append("=");
            sb.Append(UseFastPrinter);
            sb.Append(Environment.NewLine);

            sb.Append(WeeksOfSalesHistoryKey);
            sb.Append("=");
            sb.Append(WeeksOfSalesHistory);
            sb.Append(Environment.NewLine);

            sb.Append(DaysOfProjectionInTemplateKey);
            sb.Append("=");
            sb.Append(DaysOfProjectionInTemplate);
            sb.Append(Environment.NewLine);

            sb.Append(PrintCopiesInFinalizeBatchKey);
            sb.Append("=");
            sb.Append(PrintCopiesInFinalizeBatch);
            sb.Append(Environment.NewLine);

            sb.Append(ImageInNoServiceMandatoryKey);
            sb.Append("=");
            sb.Append(ImageInNoServiceMandatory);
            sb.Append(Environment.NewLine);

            sb.Append(ImageInOrderMandatoryKey);
            sb.Append("=");
            sb.Append(ImageInOrderMandatory);
            sb.Append(Environment.NewLine);

            sb.Append(ShowOnlyInvoiceForSalesmanKey);
            sb.Append("=");
            sb.Append(ShowOnlyInvoiceForSalesman);
            sb.Append(Environment.NewLine);

            sb.Append(HideInvoicesAndBalanceKey);
            sb.Append("=");
            sb.Append(HideInvoicesAndBalance);
            sb.Append(Environment.NewLine);

            sb.Append(UpdateInventoryRegardlessKey);
            sb.Append("=");
            sb.Append(UpdateInventoryRegardless);
            sb.Append(Environment.NewLine);

            sb.Append(AllowSelectPriceLevelKey);
            sb.Append("=");
            sb.Append(AllowSelectPriceLevel);
            sb.Append(Environment.NewLine);

            sb.Append(PrintLabelHeightKey);
            sb.Append("=");
            sb.Append(PrintLabelHeight);
            sb.Append(Environment.NewLine);

            sb.Append(BarcodeDecoderKey);
            sb.Append("=");
            sb.Append(BarcodeDecoder);
            sb.Append(Environment.NewLine);

            sb.Append(CasaSanchezCustomizationKey);
            sb.Append("=");
            sb.Append(CasaSanchezCustomization);
            sb.Append(Environment.NewLine);

            sb.Append(ShowWarehouseInvInSummaryKey);
            sb.Append("=");
            sb.Append(ShowWarehouseInvInSummary);
            sb.Append(Environment.NewLine);

            sb.Append(HidePriceInSelfServiceKey);
            sb.Append("=");
            sb.Append(HidePriceInSelfService);
            sb.Append(Environment.NewLine);

            sb.Append(HideOHinSelfServiceKey);
            sb.Append("=");
            sb.Append(HideOHinSelfService);
            sb.Append(Environment.NewLine);

            sb.Append(RequireLotForDumpsKey);
            sb.Append("=");
            sb.Append(RequireLotForDumps);
            sb.Append(Environment.NewLine);

            sb.Append(EnablePaymentsByTermsKey);
            sb.Append("=");
            sb.Append(EnablePaymentsByTerms);
            sb.Append(Environment.NewLine);

            sb.Append(PackageInReturnPresaleKey);
            sb.Append("=");
            sb.Append(PackageInReturnPresale);
            sb.Append(Environment.NewLine);

            sb.Append(EnableSampleOrderKey);
            sb.Append("=");
            sb.Append(EnableSampleOrder);
            sb.Append(Environment.NewLine);

            sb.Append(LockOrderAfterPrintedKey);
            sb.Append("=");
            sb.Append(LockOrderAfterPrinted);
            sb.Append(Environment.NewLine);

            sb.Append(SetShipDateKey);
            sb.Append("=");
            sb.Append(SetShipDate);
            sb.Append(Environment.NewLine);

            sb.Append(IracarCustomizationKey);
            sb.Append("=");
            sb.Append(IracarCustomization);
            sb.Append(Environment.NewLine);

            sb.Append(NewAddItemRandomWeightKey);
            sb.Append("=");
            sb.Append(NewAddItemRandomWeight);
            sb.Append(Environment.NewLine);

            sb.Append(CanChangeUomInLoadKey);
            sb.Append("=");
            sb.Append(CanChangeUomInLoad);
            sb.Append(Environment.NewLine);

            sb.Append(ShowListPriceInAddItemKey);
            sb.Append("=");
            sb.Append(ShowListPriceInAddItem);
            sb.Append(Environment.NewLine);

            sb.Append(DisolSurveyProductsKey);
            sb.Append("=");
            sb.Append(DisolSurveyProducts);
            sb.Append(Environment.NewLine);

            sb.Append(UseDisolSurveyKey);
            sb.Append("=");
            sb.Append(UseDisolSurvey);
            sb.Append(Environment.NewLine);

            sb.Append(UseClientSortKey);
            sb.Append("=");
            sb.Append(UseClientSort);
            sb.Append(Environment.NewLine);

            sb.Append(SelfServiceUserKey);
            sb.Append("=");
            sb.Append(SelfServiceUser);
            sb.Append(Environment.NewLine);

            sb.Append(UseLaceupAdvancedCatalogKey);
            sb.Append("=");
            sb.Append(UseLaceupAdvancedCatalog);
            sb.Append(Environment.NewLine);

            sb.Append(UseOffersInCreditKey);
            sb.Append("=");
            sb.Append(UseOffersInCredit);
            sb.Append(Environment.NewLine);

            sb.Append(AddRelatedItemInCreditKey);
            sb.Append("=");
            sb.Append(AddRelatedItemInCredit);
            sb.Append(Environment.NewLine);

            sb.Append(AssetStaysMandatoryKey);
            sb.Append("=");
            sb.Append(AssetStaysMandatory);
            sb.Append(Environment.NewLine);

            sb.Append(WarehouseInventoryOnDemandKey);
            sb.Append("=");
            sb.Append(WarehouseInventoryOnDemand);
            sb.Append(Environment.NewLine);

            sb.Append(ItemGroupedTemplateKey);
            sb.Append("=");
            sb.Append(ItemGroupedTemplate);
            sb.Append(Environment.NewLine);

            sb.Append(DisolCrapKey);
            sb.Append("=");
            sb.Append(DisolCrap);
            sb.Append(Environment.NewLine);

            sb.Append(UseLotExpirationKey);
            sb.Append("=");
            sb.Append(UseLotExpiration);
            sb.Append(Environment.NewLine);

            sb.Append(PONumberMaxLengthKey);
            sb.Append("=");
            sb.Append(PONumberMaxLength);
            sb.Append(Environment.NewLine);

            sb.Append(HideWarehouseOHInLoadKey);
            sb.Append("=");
            sb.Append(HideWarehouseOHInLoad);
            sb.Append(Environment.NewLine);

            sb.Append(DisablePrintEndOfDayReportKey);
            sb.Append("=");
            sb.Append(DisablePrintEndOfDayReport);
            sb.Append(Environment.NewLine);

            sb.Append(DeliveryEditableKey);
            sb.Append("=");
            sb.Append(DeliveryEditable);
            sb.Append(Environment.NewLine);

            sb.Append(AssetTrackingKey);
            sb.Append("=");
            sb.Append(AssetTracking);
            sb.Append(Environment.NewLine);

            sb.Append(SelectSalesRepForInvoiceKey);
            sb.Append("=");
            sb.Append(SelectSalesRepForInvoice);
            sb.Append(Environment.NewLine);

            sb.Append(IncludePresaleInSalesReportKey);
            sb.Append("=");
            sb.Append(IncludePresaleInSalesReport);
            sb.Append(Environment.NewLine);

            sb.Append(CarolinaCustomizationKey);
            sb.Append("=");
            sb.Append(CarolinaCustomization);
            sb.Append(Environment.NewLine);

            sb.Append(SelfServiceInvitationKey);
            sb.Append("=");
            sb.Append(SelfServiceInvitation);
            sb.Append(Environment.NewLine);

            sb.Append(UseCatalogKey);
            sb.Append("=");
            sb.Append(UseCatalog);
            sb.Append(Environment.NewLine);

            sb.Append(XlsxProviderKey);
            sb.Append("=");
            sb.Append(XlsxProvider);
            sb.Append(Environment.NewLine);

            sb.Append(PrintNoServiceInSalesReportsKey);
            sb.Append("=");
            sb.Append(PrintNoServiceInSalesReports);
            sb.Append(Environment.NewLine);

            sb.Append(SplitDeliveryByDepartmentKey);
            sb.Append("=");
            sb.Append(SplitDeliveryByDepartment);
            sb.Append(Environment.NewLine);

            sb.Append(TotalsByUoMInPdfKey);
            sb.Append("=");
            sb.Append(TotalsByUoMInPdf);
            sb.Append(Environment.NewLine);

            sb.Append(UseBiggerUoMInLoadHistoryKey);
            sb.Append("=");
            sb.Append(UseBiggerUoMInLoadHistory);
            sb.Append(Environment.NewLine);

            sb.Append(NewSyncLoadOnDemandKey);
            sb.Append("=");
            sb.Append(NewSyncLoadOnDemand);
            sb.Append(Environment.NewLine);

            sb.Append(BonsuisseCustomizationKey);
            sb.Append("=");
            sb.Append(BonsuisseCustomization);
            sb.Append(Environment.NewLine);

            sb.Append(EmptyEndingInventoryKey);
            sb.Append("=");
            sb.Append(EmptyEndingInventory);
            sb.Append(Environment.NewLine);

            sb.Append(LoadByOrderHistoryKey);
            sb.Append("=");
            sb.Append(LoadByOrderHistory);
            sb.Append(Environment.NewLine);

            sb.Append(MustGenerateProjectionKey);
            sb.Append("=");
            sb.Append(MustGenerateProjection);
            sb.Append(Environment.NewLine);

            sb.Append(CalculateInvForEmptyTruckKey);
            sb.Append("=");
            sb.Append(CalculateInvForEmptyTruck);
            sb.Append(Environment.NewLine);

            sb.Append(BranchSiteIdKey);
            sb.Append("=");
            sb.Append(BranchSiteId);
            sb.Append(Environment.NewLine);

            sb.Append(SiteIdKey);
            sb.Append("=");
            sb.Append(SiteId);
            sb.Append(Environment.NewLine);

            sb.Append(CycleCountPasswordKey);
            sb.Append("=");
            sb.Append(CycleCountPassword);
            sb.Append(Environment.NewLine);

            sb.Append(ViewPrintInvPasswordKey);
            sb.Append("=");
            sb.Append(ViewPrintInvPassword);
            sb.Append(Environment.NewLine);

            sb.Append(EnableCycleCountKey);
            sb.Append("=");
            sb.Append(EnableCycleCount);
            sb.Append(Environment.NewLine);

            sb.Append(ShowProductsPerUoMKey);
            sb.Append("=");
            sb.Append(ShowProductsPerUoM);
            sb.Append(Environment.NewLine);

            sb.Append(AcceptLoadEditableKey);
            sb.Append("=");
            sb.Append(AcceptLoadEditable);
            sb.Append(Environment.NewLine);

            sb.Append(CreditReasonInLineKey);
            sb.Append("=");
            sb.Append(CreditReasonInLine);
            sb.Append(Environment.NewLine);

            sb.Append(SalesByDepartmentKey);
            sb.Append("=");
            sb.Append(SalesByDepartment);
            sb.Append(Environment.NewLine);

            sb.Append(HideSubTotalOrderKey);
            sb.Append("=");
            sb.Append(HideSubTotalOrder);
            sb.Append(Environment.NewLine);

            sb.Append(HidePaymentsTotalKey);
            sb.Append("=");
            sb.Append(HidePaymentsTotal);
            sb.Append(Environment.NewLine);

            sb.Append(HideTransactionsTotalKey);
            sb.Append("=");
            sb.Append(HideTransactionsTotal);
            sb.Append(Environment.NewLine);

            sb.Append(UseCreditAccountKey);
            sb.Append("=");
            sb.Append(UseCreditAccount);
            sb.Append(Environment.NewLine);

            sb.Append(SalesHistoryForCreditsKey);
            sb.Append("=");
            sb.Append(SalesHistoryForCredits);
            sb.Append(Environment.NewLine);

            sb.Append(SignedInSelfServiceKey);
            sb.Append("=");
            sb.Append(SignedInSelfService);
            sb.Append(Environment.NewLine);

            sb.Append(SavedBanksKey);
            sb.Append("=");
            sb.Append(SavedBanks);
            sb.Append(Environment.NewLine);

            sb.Append(HideOpenInvoiceTotalKey);
            sb.Append("=");
            sb.Append(HideOpenInvoiceTotal);
            sb.Append(Environment.NewLine);

            sb.Append(UseSurveyKey);
            sb.Append("=");
            sb.Append(UseSurvey);
            sb.Append(Environment.NewLine);

            sb.Append(AutoCalculateRouteReturnKey);
            sb.Append("=");
            sb.Append(AutoCalculateRouteReturn);
            sb.Append(Environment.NewLine);

            sb.Append(UseFutureRouteExKey);
            sb.Append("=");
            sb.Append(UseFutureRouteEx);
            sb.Append(Environment.NewLine);

            sb.Append(GoToMainKey);
            sb.Append("=");
            sb.Append(GoToMain);
            sb.Append(Environment.NewLine);

            sb.Append(DollyReminderKey);
            sb.Append("=");
            sb.Append(DollyReminder);
            sb.Append(Environment.NewLine);

            sb.Append(SupervisorIdKey);
            sb.Append("=");
            sb.Append(SupervisorId);
            sb.Append(Environment.NewLine);

            sb.Append(SignedInKey);
            sb.Append("=");
            sb.Append(SignedIn);
            sb.Append(Environment.NewLine);

            sb.Append(ShouldGetPinBeforeSyncKey);
            sb.Append("=");
            sb.Append(ShouldGetPinBeforeSync);
            sb.Append(Environment.NewLine);

            sb.Append(BetaFragmentsKey);
            sb.Append("=");
            sb.Append(BetaFragments);
            sb.Append(Environment.NewLine);

            sb.Append(SelfServiceKey);
            sb.Append("=");
            sb.Append(SelfService);
            sb.Append(Environment.NewLine);

            sb.Append(ScanBasedTradingKey);
            sb.Append("=");
            sb.Append(ScanBasedTrading);
            sb.Append(Environment.NewLine);

            sb.Append(SalesmanSeqToKey);
            sb.Append("=");
            sb.Append(SalesmanSeqTo);
            sb.Append(Environment.NewLine);

            sb.Append(SalesmanSelectedSiteKey);
            sb.Append("=");
            sb.Append(SalesmanSelectedSite);
            sb.Append(Environment.NewLine);

            sb.Append(SalesmanSeqFromKey);
            sb.Append("=");
            sb.Append(SalesmanSeqFrom);
            sb.Append(Environment.NewLine);

            sb.Append(SalesmanSeqExpirationDateKey);
            sb.Append("=");
            sb.Append(SalesmanSeqExpirationDate.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(SalesmanSeqPrefixKey);
            sb.Append("=");
            sb.Append(SalesmanSeqPrefix);
            sb.Append(Environment.NewLine);

            sb.Append(SalesmanSeqValuesKey);
            sb.Append("=");
            sb.Append(SalesmanSeqValues);
            sb.Append(Environment.NewLine);

            sb.Append(ZeroSoldInConsignmentKey);
            sb.Append("=");
            sb.Append(ZeroSoldInConsignment);
            sb.Append(Environment.NewLine);

            sb.Append(EnableLoginKey);
            sb.Append("=");
            sb.Append(EnableLogin);
            sb.Append(Environment.NewLine);

            sb.Append(MinimumOrderProductIdKey);
            sb.Append("=");
            sb.Append(MinimumOrderProductId);
            sb.Append(Environment.NewLine);

            sb.Append(SimoneKey);
            sb.Append("=");
            sb.Append(Simone);
            sb.Append(Environment.NewLine);

            sb.Append(UseBigFontForPrintDateKey);
            sb.Append("=");
            sb.Append(UseBigFontForPrintDate);
            sb.Append(Environment.NewLine);

            sb.Append(CheckDueInvoicesQtyInCreateOrderKey);
            sb.Append("=");
            sb.Append(CheckDueInvoicesQtyInCreateOrder);
            sb.Append(Environment.NewLine);

            sb.Append(AutomaticClockOutTimeKey);
            sb.Append("=");
            sb.Append(AutomaticClockOutTime);
            sb.Append(Environment.NewLine);

            sb.Append(MandatoryBreakDurationKey);
            sb.Append("=");
            sb.Append(MandatoryBreakDuration);
            sb.Append(Environment.NewLine);

            sb.Append(ForceBreakInMinutesKey);
            sb.Append("=");
            sb.Append(ForceBreakInMinutes);
            sb.Append(Environment.NewLine);

            sb.Append(OtherChargesTypeKey);
            sb.Append("=");
            sb.Append(OtherChargesType);
            sb.Append(Environment.NewLine);

            sb.Append(FreightTypeKey);
            sb.Append("=");
            sb.Append(FreightType);
            sb.Append(Environment.NewLine);

            sb.Append(OtherChargesCommentsKey);
            sb.Append("=");
            sb.Append(OtherChargesComments);
            sb.Append(Environment.NewLine);

            sb.Append(FreightCommentsKey);
            sb.Append("=");
            sb.Append(FreightComments);
            sb.Append(Environment.NewLine);

            sb.Append(OtherChargesValeKey);
            sb.Append("=");
            sb.Append(OtherChargesVale);
            sb.Append(Environment.NewLine);

            sb.Append(FreightValeKey);
            sb.Append("=");
            sb.Append(FreightVale);
            sb.Append(Environment.NewLine);

            sb.Append(HiddenItemCustomizationKey);
            sb.Append("=");
            sb.Append(HiddenItemCustomization);
            sb.Append(Environment.NewLine);

            sb.Append(BetaConfigurationViewKey);
            sb.Append("=");
            sb.Append(BetaConfigurationView);
            sb.Append(Environment.NewLine);

            sb.Append(ConsignmentBetaKey);
            sb.Append("=");
            sb.Append(ConsignmentBeta);
            sb.Append(Environment.NewLine);

            sb.Append(MasterDeviceKey);
            sb.Append("=");
            sb.Append(MasterDevice);
            sb.Append(Environment.NewLine);

            sb.Append(TruckTemperatureReqKey);
            sb.Append("=");
            sb.Append(TruckTemperatureReq);
            sb.Append(Environment.NewLine);

            sb.Append(ConsignmentPresaleOnlyKey);
            sb.Append("=");
            sb.Append(ConsignmentPresaleOnly);
            sb.Append(Environment.NewLine);

            sb.Append(CheckDueInvoicesInCreateOrderKey);
            sb.Append("=");
            sb.Append(CheckDueInvoicesInCreateOrder);
            sb.Append(Environment.NewLine);

            sb.Append(LotIsMandatoryBeforeFinalizeKey);
            sb.Append("=");
            sb.Append(LotIsMandatoryBeforeFinalize);
            sb.Append(Environment.NewLine);

            sb.Append(OnlyPresaleKey);
            sb.Append("=");
            sb.Append(OnlyPresale);
            sb.Append(Environment.NewLine);

            sb.Append(HidePriceInTransactionKey);
            sb.Append("=");
            sb.Append(HidePriceInTransaction);
            sb.Append(Environment.NewLine);

            sb.Append(HidePONumberKey);
            sb.Append("=");
            sb.Append(HidePONumber);
            sb.Append(Environment.NewLine);

            sb.Append(PrintClientTotalOpenBalanceKey);
            sb.Append("=");
            sb.Append(PrintClientTotalOpenBalance);
            sb.Append(Environment.NewLine);

            sb.Append(GenerateProjectionKey);
            sb.Append("=");
            sb.Append(GenerateProjection);
            sb.Append(Environment.NewLine);

            sb.Append(UseFullTemplateKey);
            sb.Append("=");
            sb.Append(UseFullTemplate);
            sb.Append(Environment.NewLine);

            sb.Append(HideTransfersKey);
            sb.Append("=");
            sb.Append(HideTransfers);
            sb.Append(Environment.NewLine);

            sb.Append(MustCompleteInDeliveryCheckingKey);
            sb.Append("=");
            sb.Append(MustCompleteInDeliveryChecking);
            sb.Append(Environment.NewLine);

            sb.Append(ShowAllProductsInCreditsKey);
            sb.Append("=");
            sb.Append(ShowAllProductsInCredits);
            sb.Append(Environment.NewLine);

            sb.Append(DeleteWeightItemsMenuKey);
            sb.Append("=");
            sb.Append(DeleteWeightItemsMenu);
            sb.Append(Environment.NewLine);

            sb.Append(HidePresaleOptionsKey);
            sb.Append("=");
            sb.Append(HidePresaleOptions);
            sb.Append(Environment.NewLine);

            sb.Append(ShowDiscountByPriceLevelKey);
            sb.Append("=");
            sb.Append(ShowDiscountByPriceLevel);
            sb.Append(Environment.NewLine);

            sb.Append(UserCanChangePricesCreditsKey);
            sb.Append("=");
            sb.Append(UserCanChangePricesCredits);
            sb.Append(Environment.NewLine);

            sb.Append(UserCanChangePricesSalesKey);
            sb.Append("=");
            sb.Append(UserCanChangePricesSales);
            sb.Append(Environment.NewLine);

            sb.Append(AllowQtyConversionFactorKey);
            sb.Append("=");
            sb.Append(AllowQtyConversionFactor);
            sb.Append(Environment.NewLine);

            sb.Append(DontDeleteEmptyDeliveriesKey);
            sb.Append("=");
            sb.Append(DontDeleteEmptyDeliveries);
            sb.Append(Environment.NewLine);

            sb.Append(ViewGoalsKey);
            sb.Append("=");
            sb.Append(ViewGoals);
            sb.Append(Environment.NewLine);

            sb.Append(EcoSkyWaterCustomEmailKey);
            sb.Append("=");
            sb.Append(EcoSkyWaterCustomEmail);
            sb.Append(Environment.NewLine);

            sb.Append(KeepAppUpdatedKey);
            sb.Append("=");
            sb.Append(KeepAppUpdated);
            sb.Append(Environment.NewLine);

            sb.Append(HideSalesOrdersKey);
            sb.Append("=");
            sb.Append(HideSalesOrders);
            sb.Append(Environment.NewLine);

            sb.Append(AllowResetKey);
            sb.Append("=");
            sb.Append(AllowReset);
            sb.Append(Environment.NewLine);

            sb.Append(HideClearDataKey);
            sb.Append("=");
            sb.Append(HideClearData);
            sb.Append(Environment.NewLine);

            sb.Append(MustEnterPostedDateKey);
            sb.Append("=");
            sb.Append(MustEnterPostedDate);
            sb.Append(Environment.NewLine);

            sb.Append(NeedAccessForConfigurationKey);
            sb.Append("=");
            sb.Append(NeedAccessForConfiguration);
            sb.Append(Environment.NewLine);

            sb.Append(PreviewOfferPriceInAddItemKey);
            sb.Append("=");
            sb.Append(PreviewOfferPriceInAddItem);
            sb.Append(Environment.NewLine);

            sb.Append(DeleteZeroItemsOnDeliveryKey);
            sb.Append("=");
            sb.Append(DeleteZeroItemsOnDelivery);
            sb.Append(Environment.NewLine);

            sb.Append(SalesmanCanChangeSiteKey);
            sb.Append("=");
            sb.Append(SalesmanCanChangeSite);
            sb.Append(Environment.NewLine);

            sb.Append(MustSetWeightInDeliveryKey);
            sb.Append("=");
            sb.Append(MustSetWeightInDelivery);
            sb.Append(Environment.NewLine);

            sb.Append(PrintCreditReportKey);
            sb.Append("=");
            sb.Append(PrintCreditReport);
            sb.Append(Environment.NewLine);

            sb.Append(CheckAvailableBeforeSendingKey);
            sb.Append("=");
            sb.Append(CheckAvailableBeforeSending);
            sb.Append(Environment.NewLine);

            sb.Append(SAPOrderStatusReportKey);
            sb.Append("=");
            sb.Append(SAPOrderStatusReport);
            sb.Append(Environment.NewLine);

            sb.Append(PresaleUseInventorySiteKey);
            sb.Append("=");
            sb.Append(PresaleUseInventorySite);
            sb.Append(Environment.NewLine);

            sb.Append(CannotOrderWithUnpaidInvoicesKey);
            sb.Append("=");
            sb.Append(CannotOrderWithUnpaidInvoices);
            sb.Append(Environment.NewLine);

            sb.Append(CanPayMoreThanOwnedKey);
            sb.Append("=");
            sb.Append(CanPayMoreThanOwned);
            sb.Append(Environment.NewLine);

            sb.Append(ShowAllEmailsAsDestinationKey);
            sb.Append("=");
            sb.Append(ShowAllEmailsAsDestination);
            sb.Append(Environment.NewLine);

            sb.Append(SelectPriceFromPrevInvoicesKey);
            sb.Append("=");
            sb.Append(SelectPriceFromPrevInvoices);
            sb.Append(Environment.NewLine);

            sb.Append(UsePalletsKey);
            sb.Append("=");
            sb.Append(UsePallets);
            sb.Append(Environment.NewLine);

            sb.Append(HideTaxesTotalPrintKey);
            sb.Append("=");
            sb.Append(HideTaxesTotalPrint);
            sb.Append(Environment.NewLine);

            sb.Append(HideDiscountTotalPrintKey);
            sb.Append("=");
            sb.Append(HideDiscountTotalPrint);
            sb.Append(Environment.NewLine);

            sb.Append(CanModifyWeightsOnDeliveriesKey);
            sb.Append("=");
            sb.Append(CanModifyWeightsOnDeliveries);
            sb.Append(Environment.NewLine);

            sb.Append(ShowPricesInInventorySummaryKey);
            sb.Append("=");
            sb.Append(ShowPricesInInventorySummary);
            sb.Append(Environment.NewLine);

            sb.Append(AlertOrderWasNotSentKey);
            sb.Append("=");
            sb.Append(AlertOrderWasNotSent);
            sb.Append(Environment.NewLine);

            sb.Append(RequestVehicleInformationKey);
            sb.Append("=");
            sb.Append(RequestVehicleInformation);
            sb.Append(Environment.NewLine);

            sb.Append(ShowCostInTemplateKey);
            sb.Append("=");
            sb.Append(ShowCostInTemplate);
            sb.Append(Environment.NewLine);

            sb.Append(ShowListPriceInAdvancedCatalogKey);
            sb.Append("=");
            sb.Append(ShowListPriceInAdvancedCatalog);
            sb.Append(Environment.NewLine);

            sb.Append(RecalculateOrdersAfterSyncKey);
            sb.Append("=");
            sb.Append(RecalculateOrdersAfterSync);
            sb.Append(Environment.NewLine);

            sb.Append(ShowLowestPriceInTemplateKey);
            sb.Append("=");
            sb.Append(ShowLowestPriceInTemplate);
            sb.Append(Environment.NewLine);

            sb.Append(ShowWeightOnInventorySummaryKey);
            sb.Append("=");
            sb.Append(ShowWeightOnInventorySummary);
            sb.Append(Environment.NewLine);

            sb.Append(EnableAdvancedLoginKey);
            sb.Append("=");
            sb.Append(EnableAdvancedLogin);
            sb.Append(Environment.NewLine);

            sb.Append(ShowLowestPriceLevelKey);
            sb.Append("=");
            sb.Append(ShowLowestPriceLevel);
            sb.Append(Environment.NewLine);

            sb.Append(MustCreatePaymentDepositKey);
            sb.Append("=");
            sb.Append(MustCreatePaymentDeposit);
            sb.Append(Environment.NewLine);

            sb.Append(MustSelectRouteToSyncKey);
            sb.Append("=");
            sb.Append(MustSelectRouteToSync);
            sb.Append(Environment.NewLine);

            sb.Append(ShowLastThreeVisitsOnTemplateKey);
            sb.Append("=");
            sb.Append(ShowLastThreeVisitsOnTemplate);
            sb.Append(Environment.NewLine);

            sb.Append(BlockMultipleCollectPaymetsKey);
            sb.Append("=");
            sb.Append(BlockMultipleCollectPaymets);
            sb.Append(Environment.NewLine);

            sb.Append(SelectWarehouseForSalesKey);
            sb.Append("=");
            sb.Append(SelectWarehouseForSales);
            sb.Append(Environment.NewLine);

            sb.Append(DonNovoCustomizationKey);
            sb.Append("=");
            sb.Append(DonNovoCustomization);
            sb.Append(Environment.NewLine);

            sb.Append(UseVisitsTemplateInSalesKey);
            sb.Append("=");
            sb.Append(UseVisitsTemplateInSales);
            sb.Append(Environment.NewLine);

            sb.Append(AllowWorkOrderKey);
            sb.Append("=");
            sb.Append(AllowWorkOrder);
            sb.Append(Environment.NewLine);

            sb.Append(NotifyNewerDataInOSKey);
            sb.Append("=");
            sb.Append(NotifyNewerDataInOS);
            sb.Append(Environment.NewLine);

            sb.Append(AmericanEagleCustomizationKey);
            sb.Append("=");
            sb.Append(AmericanEagleCustomization);
            sb.Append(Environment.NewLine);

            sb.Append(ShowSuggestedButtonKey);
            sb.Append("=");
            sb.Append(ShowSuggestedButton);
            sb.Append(Environment.NewLine);

            sb.Append(DisableSendCatalogWithPricesKey);
            sb.Append("=");
            sb.Append(DisableSendCatalogWithPrices);
            sb.Append(Environment.NewLine);

            sb.Append(RecalculateRoutesOnSyncDataKey);
            sb.Append("=");
            sb.Append(RecalculateRoutesOnSyncData);
            sb.Append(Environment.NewLine);

            sb.Append(CheckInventoryInLoadKey);
            sb.Append("=");
            sb.Append(CheckInventoryInLoad);
            sb.Append(Environment.NewLine);

            sb.Append(CustomerInCreditHoldKey);
            sb.Append("=");
            sb.Append(CustomerInCreditHold);
            sb.Append(Environment.NewLine);

            sb.Append(AdvanceSequencyNumKey);
            sb.Append("=");
            sb.Append(AdvanceSequencyNum);
            sb.Append(Environment.NewLine);

            sb.Append(MinimumAvailableNumbersKey);
            sb.Append("=");
            sb.Append(MinimumAvailableNumbers);
            sb.Append(Environment.NewLine);

            sb.Append(UseQuoteKey);
            sb.Append("=");
            sb.Append(UseQuote);
            sb.Append(Environment.NewLine);

            sb.Append(RouteReturnPasswordKey);
            sb.Append("=");
            sb.Append(RouteReturnPassword);
            sb.Append(Environment.NewLine);

            sb.Append(SelectReshipDateKey);
            sb.Append("=");
            sb.Append(SelectReshipDate);
            sb.Append(Environment.NewLine);

            sb.Append(ClientRtnNeededForQtyKey);
            sb.Append("=");
            sb.Append(ClientRtnNeededForQty);
            sb.Append(Environment.NewLine);

            sb.Append(CaptureImagesKey);
            sb.Append("=");
            sb.Append(CaptureImages);
            sb.Append(Environment.NewLine);

            sb.Append(NewClientCanHaveDiscountKey);
            sb.Append("=");
            sb.Append(NewClientCanHaveDiscount);
            sb.Append(Environment.NewLine);

            sb.Append(SendBackgroundPaymentsKey);
            sb.Append("=");
            sb.Append(SendBackgroundPayments);
            sb.Append(Environment.NewLine);

            sb.Append(DontAllowDecimalsInQtyKey);
            sb.Append("=");
            sb.Append(DontAllowDecimalsInQty);
            sb.Append(Environment.NewLine);

            sb.Append(IncludeCredInNewParCalcKey);
            sb.Append("=");
            sb.Append(IncludeCredInNewParCalc);
            sb.Append(Environment.NewLine);

            sb.Append(SettReportInSalesUoMKey);
            sb.Append("=");
            sb.Append(SettReportInSalesUoM);
            sb.Append(Environment.NewLine);

            sb.Append(AllowMultParInvoicesKey);
            sb.Append("=");
            sb.Append(AllowMultParInvoices);
            sb.Append(Environment.NewLine);

            sb.Append(GeneratePresaleNumberKey);
            sb.Append("=");
            sb.Append(GeneratePresaleNumber);
            sb.Append(Environment.NewLine);

            sb.Append(ScanDeliveryCheckingKey);
            sb.Append("=");
            sb.Append(ScanDeliveryChecking);
            sb.Append(Environment.NewLine);

            sb.Append(KeepPresaleOrdersKey);
            sb.Append("=");
            sb.Append(KeepPresaleOrders);
            sb.Append(Environment.NewLine);

            sb.Append(MultipleLoadOnDemandKey);
            sb.Append("=");
            sb.Append(MultipleLoadOnDemand);
            sb.Append(Environment.NewLine);

            sb.Append(AverageSaleInParLevelKey);
            sb.Append("=");
            sb.Append(AverageSaleInParLevel);
            sb.Append(Environment.NewLine);

            sb.Append(ConsParFirstInPresaleKey);
            sb.Append("=");
            sb.Append(ConsParFirstInPresale);
            sb.Append(Environment.NewLine);

            sb.Append(IncludeRotationInDeliveryKey);
            sb.Append("=");
            sb.Append(IncludeRotationInDelivery);
            sb.Append(Environment.NewLine);

            sb.Append(ChargeBatteryRotationKey);
            sb.Append("=");
            sb.Append(ChargeBatteryRotation);
            sb.Append(Environment.NewLine);

            sb.Append(WarrantyPerClientKey);
            sb.Append("=");
            sb.Append(WarrantyPerClient);
            sb.Append(Environment.NewLine);

            sb.Append(AlwaysCountInParKey);
            sb.Append("=");
            sb.Append(AlwaysCountInPar);
            sb.Append(Environment.NewLine);

            sb.Append(EditParInHistoryKey);
            sb.Append("=");
            sb.Append(EditParInHistory);
            sb.Append(Environment.NewLine);

            sb.Append(CloseRouteInPresaleKey);
            sb.Append("=");
            sb.Append(CloseRouteInPresale);
            sb.Append(Environment.NewLine);

            sb.Append(UseAllDayParLevelKey);
            sb.Append("=");
            sb.Append(UseAllDayParLevel);
            sb.Append(Environment.NewLine);

            sb.Append(LspInAllLinesKey);
            sb.Append("=");
            sb.Append(LspInAllLines);
            sb.Append(Environment.NewLine);

            sb.Append(CheckInventoryInPreSaleKey);
            sb.Append("=");
            sb.Append(CheckInventoryInPreSale);
            sb.Append(Environment.NewLine);

            sb.Append(IgnoreDiscountInCreditsKey);
            sb.Append("=");
            sb.Append(IgnoreDiscountInCredits);
            sb.Append(Environment.NewLine);

            sb.Append(CanModifyConnectSettKey);
            sb.Append("=");
            sb.Append(CanModifyConnectSett);
            sb.Append(Environment.NewLine);

            sb.Append(AlwaysUpdateNewParKey);
            sb.Append("=");
            sb.Append(AlwaysUpdateNewPar);
            sb.Append(Environment.NewLine);

            sb.Append(DeliveryReasonInLineKey);
            sb.Append("=");
            sb.Append(DeliveryReasonInLine);
            sb.Append(Environment.NewLine);

            sb.Append(OnlyKitInCreditKey);
            sb.Append("=");
            sb.Append(OnlyKitInCredit);
            sb.Append(Environment.NewLine);

            sb.Append(GeneratePreorderNumKey);
            sb.Append("=");
            sb.Append(GeneratePreorderNum);
            sb.Append(Environment.NewLine);

            sb.Append(ShipViaMandatoryKey);
            sb.Append("=");
            sb.Append(ShipViaMandatory);
            sb.Append(Environment.NewLine);

            sb.Append(ShowShipViaKey);
            sb.Append("=");
            sb.Append(ShowShipVia);
            sb.Append(Environment.NewLine);

            sb.Append(DisolCustomIdGeneratorKey);
            sb.Append("=");
            sb.Append(DisolCustomIdGenerator);
            sb.Append(Environment.NewLine);

            sb.Append(ConsLotAsDateKey);
            sb.Append("=");
            sb.Append(ConsLotAsDate);
            sb.Append(Environment.NewLine);

            sb.Append(AddCreditInConsignmentKey);
            sb.Append("=");
            sb.Append(AddCreditInConsignment);
            sb.Append(Environment.NewLine);

            sb.Append(ParInConsignmentKey);
            sb.Append("=");
            sb.Append(ParInConsignment);
            sb.Append(Environment.NewLine);

            sb.Append(HideVoidButtonKey);
            sb.Append("=");
            sb.Append(HideVoidButton);
            sb.Append(Environment.NewLine);

            sb.Append(PrintTaxLabelKey);
            sb.Append("=");
            sb.Append(PrintTaxLabel);
            sb.Append(Environment.NewLine);

            sb.Append(AllowDiscountPerLineKey);
            sb.Append("=");
            sb.Append(AllowDiscountPerLine);
            sb.Append(Environment.NewLine);

            sb.Append(PresaleCommMandatoryKey);
            sb.Append("=");
            sb.Append(PresaleCommMandatory);
            sb.Append(Environment.NewLine);

            sb.Append(Minimumamountkey);
            sb.Append("=");
            sb.Append(MinimumAmount);
            sb.Append(Environment.NewLine);

            sb.Append(MinimumWeightkey);
            sb.Append("=");
            sb.Append(MinimumWeight);
            sb.Append(Environment.NewLine);

            sb.Append(SendBackgroundBackupKey);
            sb.Append("=");
            sb.Append(SendBackgroundBackup);
            sb.Append(Environment.NewLine);

            sb.Append(UseConsignmentLotKey);
            sb.Append("=");
            sb.Append(UseConsignmentLot);
            sb.Append(Environment.NewLine);

            sb.Append(PreSaleConsigmentKey);
            sb.Append("=");
            sb.Append(PreSaleConsigment);
            sb.Append(Environment.NewLine);

            sb.Append(PrintInvSettReportKey);
            sb.Append("=");
            sb.Append(PrintInvSettReport);
            sb.Append(Environment.NewLine);

            sb.Append(HideProdOnHandKey);
            sb.Append("=");
            sb.Append(HideProdOnHand);
            sb.Append(Environment.NewLine);

            sb.Append(HideContactNameKey);
            sb.Append("=");
            sb.Append(HideContactName);
            sb.Append(Environment.NewLine);

            sb.Append(PrintPaymentRegardlessKey);
            sb.Append("=");
            sb.Append(PrintPaymentRegardless);
            sb.Append(Environment.NewLine);

            sb.Append(GeorgeHoweCustomizationKey);
            sb.Append("=");
            sb.Append(GeorgeHoweCustomization);
            sb.Append(Environment.NewLine);

            sb.Append(CanRestoreFromFileKey);
            sb.Append("=");
            sb.Append(CanRestoreFromFile);
            sb.Append(Environment.NewLine);

            sb.Append(BlackStoneConsigCustomKey);
            sb.Append("=");
            sb.Append(BlackStoneConsigCustom);
            sb.Append(Environment.NewLine);

            sb.Append(DaysToKeepSignaturesKey);
            sb.Append("=");
            sb.Append(DaysToKeepSignatures);
            sb.Append(Environment.NewLine);

            sb.Append(PrintBillShipDateKey);
            sb.Append("=");
            sb.Append(PrintBillShipDate);
            sb.Append(Environment.NewLine);
            sb.Append(UseDraggableTemplateKey);
            sb.Append("=");
            sb.Append(UseDraggableTemplate);
            sb.Append(Environment.NewLine);

            sb.Append(HidePrintedCommentLineKey);
            sb.Append("=");
            sb.Append(HidePrintedCommentLine);
            sb.Append(Environment.NewLine);

            sb.Append(MinShipDateDaysKey);
            sb.Append("=");
            sb.Append(MinShipDateDays);
            sb.Append(Environment.NewLine);

            sb.Append(ClientNameMaxSizeKey);
            sb.Append("=");
            sb.Append(ClientNameMaxSize);
            sb.Append(Environment.NewLine);

            sb.Append(SalesmanInCreditDelKey);
            sb.Append("=");
            sb.Append(SalesmanInCreditDel);
            sb.Append(Environment.NewLine);

            sb.Append(ShowAddrInClientListKey);
            sb.Append("=");
            sb.Append(ShowAddrInClientList);
            sb.Append(Environment.NewLine);

            sb.Append(UseLastUoMKey);
            sb.Append("=");
            sb.Append(UseLastUoM);
            sb.Append(Environment.NewLine);

            sb.Append(DefaultTaxRateKey);
            sb.Append("=");
            sb.Append(DefaultTaxRate);
            sb.Append(Environment.NewLine);

            sb.Append(BillNumRequiredKey);
            sb.Append("=");
            sb.Append(BillNumRequired);
            sb.Append(Environment.NewLine);

            sb.Append(RTNKey);
            sb.Append("=");
            sb.Append(RTN);
            sb.Append(Environment.NewLine);

            sb.Append(UseFullConsignmentKey);
            sb.Append("=");
            sb.Append(UseFullConsignment);
            sb.Append(Environment.NewLine);

            sb.Append(NewClientExtraFieldsKey);
            sb.Append("=");
            sb.Append(NewClientExtraFields);
            sb.Append(Environment.NewLine);

            sb.Append(NewClientEmailRequiredKey);
            sb.Append("=");
            sb.Append(NewClientEmailRequired);
            sb.Append(Environment.NewLine);

            sb.Append(PdfProviderKey);
            sb.Append("=");
            sb.Append(PdfProvider);
            sb.Append(Environment.NewLine);

            sb.Append(UseClientClassAsCompanyNameKey);
            sb.Append("=");
            sb.Append(UseClientClassAsCompanyName);
            sb.Append(Environment.NewLine);

            sb.Append(CanChangeUoMKey);
            sb.Append("=");
            sb.Append(CanChangeUoM);
            sb.Append(Environment.NewLine);

            sb.Append(AddSalesInConsignmentKey);
            sb.Append("=");
            sb.Append(AddSalesInConsignment);
            sb.Append(Environment.NewLine);

            sb.Append(ParLevelHistoryDaysKey);
            sb.Append("=");
            sb.Append(ParLevelHistoryDays);
            sb.Append(Environment.NewLine);

            sb.Append(MustEnterCaseInOutKey);
            sb.Append("=");
            sb.Append(MustEnterCaseInOut);
            sb.Append(Environment.NewLine);

            sb.Append(PaymentOrSignatureRequiredKey);
            sb.Append("=");
            sb.Append(PaymentOrSignatureRequired);
            sb.Append(Environment.NewLine);

            sb.Append(DoNotShrinkOrderImageKey);
            sb.Append("=");
            sb.Append(DoNotShrinkOrderImage);
            sb.Append(Environment.NewLine);

            sb.Append(SendOrderIsMandatoryKey);
            sb.Append("=");
            sb.Append(SendOrderIsMandatory);
            sb.Append(Environment.NewLine);

            sb.Append(HidePriceInPrintedLineKey);
            sb.Append("=");
            sb.Append(HidePriceInPrintedLine);
            sb.Append(Environment.NewLine);

            sb.Append(SendBackgroundOrdersKey);
            sb.Append("=");
            sb.Append(SendBackgroundOrders);
            sb.Append(Environment.NewLine);

            sb.Append(ConsignmentContractTextKey);
            sb.Append("=");
            sb.Append(ConsignmentContractText);
            sb.Append(Environment.NewLine);

            sb.Append(AllowAdjPastExpDateKey);
            sb.Append("=");
            sb.Append(AllowAdjPastExpDate);
            sb.Append(Environment.NewLine);

            sb.Append(DisableRouteReturnKey);
            sb.Append("=");
            sb.Append(DisableRouteReturn);
            sb.Append(Environment.NewLine);

            sb.Append(ShowAvgInCatalogKey);
            sb.Append("=");
            sb.Append(ShowAvgInCatalog);
            sb.Append(Environment.NewLine);

            sb.Append(BackgroundTimeKey);
            sb.Append("=");
            sb.Append(BackgroundTime);
            sb.Append(Environment.NewLine);

            sb.Append(ClientDailyPLKey);
            sb.Append("=");
            sb.Append(ClientDailyPL);
            sb.Append(Environment.NewLine);

            sb.Append(IncludeBatteryInLoadKey);
            sb.Append("=");
            sb.Append(IncludeBatteryInLoad);
            sb.Append(Environment.NewLine);

            sb.Append(HideInvoiceCommentKey);
            sb.Append("=");
            sb.Append(HideInvoiceComment);
            sb.Append(Environment.NewLine);

            sb.Append(SendByEmailInFinalizeKey);
            sb.Append("=");
            sb.Append(SendByEmailInFinalize);
            sb.Append(Environment.NewLine);

            sb.Append(HideItemCommentKey);
            sb.Append("=");
            sb.Append(HideItemComment);
            sb.Append(Environment.NewLine);

            sb.Append(AddCoreBalancekey);
            sb.Append("=");
            sb.Append(AddCoreBalance);
            sb.Append(Environment.NewLine);

            sb.Append(TransferCommentKey);
            sb.Append("=");
            sb.Append(TransferComment);
            sb.Append(Environment.NewLine);

            sb.Append(SalesRegReportWithTaxKey);
            sb.Append("=");
            sb.Append(SalesRegReportWithTax);
            sb.Append(Environment.NewLine);

            sb.Append(MasterLoadOrderKey);
            sb.Append("=");
            sb.Append(MasterLoadOrder);
            sb.Append(Environment.NewLine);

            sb.Append(SyncLoadOnDemandKey);
            sb.Append("=");
            sb.Append(SyncLoadOnDemand);
            sb.Append(Environment.NewLine);

            sb.Append(MagnoliaSetConsignmentKey);
            sb.Append("=");
            sb.Append(MagnoliaSetConsignment);
            sb.Append(Environment.NewLine);

            sb.Append(AuthProdsInCreditKey);
            sb.Append("=");
            sb.Append(AuthProdsInCredit);
            sb.Append(Environment.NewLine);

            sb.Append(PrintNetQtyKey);
            sb.Append("=");
            sb.Append(PrintNetQty);
            sb.Append(Environment.NewLine);

            sb.Append(NewClientCanChangePricesKey);
            sb.Append("=");
            sb.Append(NewClientCanChangePrices);
            sb.Append(Environment.NewLine);

            sb.Append(POIsMandatoryKey);
            sb.Append("=");
            sb.Append(POIsMandatory);
            sb.Append(Environment.NewLine);

            sb.Append(UseTermsInLoadOrderKey);
            sb.Append("=");
            sb.Append(UseTermsInLoadOrder);
            sb.Append(Environment.NewLine);

            sb.Append(AddItemInDefaultUoMKey);
            sb.Append("=");
            sb.Append(AddItemInDefaultUoM);
            sb.Append(Environment.NewLine);

            sb.Append(UseReturnInvoiceKey);
            sb.Append("=");
            sb.Append(UseReturnInvoice);
            sb.Append(Environment.NewLine);

            sb.Append(IncludeDeliveriesInLoadOrderKey);
            sb.Append("=");
            sb.Append(IncludeDeliveriesInLoadOrder);
            sb.Append(Environment.NewLine);

            sb.Append(UseAllowanceKey);
            sb.Append("=");
            sb.Append(UseAllowance);
            sb.Append(Environment.NewLine);

            sb.Append(AddCoresInSalesItemKey);
            sb.Append("=");
            sb.Append(AddCoresInSalesItem);
            sb.Append(Environment.NewLine);

            sb.Append(PrintZeroesOnPickSheetKey);
            sb.Append("=");
            sb.Append(PrintZeroesOnPickSheet);
            sb.Append(Environment.NewLine);

            sb.Append(SignatureNameRequiredKey);
            sb.Append("=");
            sb.Append(SignatureNameRequired);
            sb.Append(Environment.NewLine);

            sb.Append(AddRelatedItemsInTotalKey);
            sb.Append("=");
            sb.Append(AddRelatedItemsInTotal);
            sb.Append(Environment.NewLine);

            sb.Append(RemovePayBalFomInvoiceKey);
            sb.Append("=");
            sb.Append(RemovePayBalFomInvoice);
            sb.Append(Environment.NewLine);

            sb.Append(Discount100PercentPrintTextKey);
            sb.Append("=");
            sb.Append(Discount100PercentPrintText);
            sb.Append(Environment.NewLine);

            sb.Append(MustCompleteRouteKey);
            sb.Append("=");
            sb.Append(MustCompleteRoute);
            sb.Append(Environment.NewLine);

            sb.Append(SelectDriverFromPresaleKey);
            sb.Append("=");
            sb.Append(SelectDriverFromPresale);
            sb.Append(Environment.NewLine);

            sb.Append(PrintIsMandatoryKey);
            sb.Append("=");
            sb.Append(PrintIsMandatory);
            sb.Append(Environment.NewLine);

            sb.Append(HideCompanyInfoPrintKey);
            sb.Append("=");
            sb.Append(HideCompanyInfoPrint);
            sb.Append(Environment.NewLine);

            sb.Append(UseSendByEmailKey);
            sb.Append("=");
            sb.Append(UseSendByEmail);
            sb.Append(Environment.NewLine);

            sb.Append(UseReturnOrderKey);
            sb.Append("=");
            sb.Append(UseReturnOrder);
            sb.Append(Environment.NewLine);

            sb.Append(IncludeCreditInvoiceForPaymentsKey);
            sb.Append("=");
            sb.Append(IncludeCreditInvoiceForPayments);
            sb.Append(Environment.NewLine);

            sb.Append(MarmiaCustomizationKey);
            sb.Append("=");
            sb.Append(MarmiaCustomization);
            sb.Append(Environment.NewLine);

            sb.Append(NotificationsInSelfServiceKey);
            sb.Append("=");
            sb.Append(NotificationsInSelfService);
            sb.Append(Environment.NewLine);

            sb.Append(CanDepositChecksWithDifDatesKey);
            sb.Append("=");
            sb.Append(CanDepositChecksWithDifDates);
            sb.Append(Environment.NewLine);

            sb.Append(TemplateSearchByContainsKey);
            sb.Append("=");
            sb.Append(TemplateSearchByContains);
            sb.Append(Environment.NewLine);

            sb.Append(UseLaceupDataInSalesReportKey);
            sb.Append("=");
            sb.Append(UseLaceupDataInSalesReport);
            sb.Append(Environment.NewLine);

            sb.Append(ShowDescriptionInSelfServiceCatalogKey);
            sb.Append("=");
            sb.Append(ShowDescriptionInSelfServiceCatalog);
            sb.Append(Environment.NewLine);

            sb.Append(AskOffersBeforeAddingKey);
            sb.Append("=");
            sb.Append(AskOffersBeforeAdding);
            sb.Append(Environment.NewLine);

            sb.Append(SearchAllProductsInTemplateKey);
            sb.Append("=");
            sb.Append(SearchAllProductsInTemplate);
            sb.Append(Environment.NewLine);

            sb.Append(AdvancedTemplateFocusSearchKey);
            sb.Append("=");
            sb.Append(AdvancedTemplateFocusSearch);
            sb.Append(Environment.NewLine);

            sb.Append(MustSelectReasonForFreeItemKey);
            sb.Append("=");
            sb.Append(MustSelectReasonForFreeItem);
            sb.Append(Environment.NewLine);

            sb.Append(CanEditCreditsInDeliveryKey);
            sb.Append("=");
            sb.Append(CanEditCreditsInDelivery);
            sb.Append(Environment.NewLine);

            sb.Append(CanChangeSalesmanNameKey);
            sb.Append("=");
            sb.Append(CanChangeSalesmanName);
            sb.Append(Environment.NewLine);

            sb.Append(PriceLevelCommentKey);
            sb.Append("=");
            sb.Append(PriceLevelComment);
            sb.Append(Environment.NewLine);

            sb.Append(GetUOMSOnCommandKey);
            sb.Append("=");
            sb.Append(GetUOMSOnCommand);
            sb.Append(Environment.NewLine);

            sb.Append(CanChangeFinalizedInvoicesKey);
            sb.Append("=");
            sb.Append(CanChangeFinalizedInvoices);
            sb.Append(Environment.NewLine);

            sb.Append(AllowNotificationsKey);
            sb.Append("=");
            sb.Append(AllowNotifications);
            sb.Append(Environment.NewLine);

            sb.Append(ShowLowestAcceptableOnWarningKey);
            sb.Append("=");
            sb.Append(ShowLowestAcceptableOnWarning);
            sb.Append(Environment.NewLine);

            sb.Append(SoutoBottomEmailTextKey);
            sb.Append("=");
            sb.Append(SoutoBottomEmailText);
            sb.Append(Environment.NewLine);

            sb.Append(UsePrintProofDeliveryKey);
            sb.Append("=");
            sb.Append(UsePrintProofDelivery);
            sb.Append(Environment.NewLine);

            sb.Append(UsePairLotQtyKey);
            sb.Append("=");
            sb.Append(UsePairLotQty);
            sb.Append(Environment.NewLine);

            sb.Append(CanSelectSalesmanKey);
            sb.Append("=");
            sb.Append(CanSelectSalesman);
            sb.Append(Environment.NewLine);

            sb.Append(DefaultCreditDetTypeKey);
            sb.Append("=");
            sb.Append(DefaultCreditDetType);
            sb.Append(Environment.NewLine);

            sb.Append(ExtraSpaceForSignatureKey);
            sb.Append("=");
            sb.Append(ExtraSpaceForSignature);
            sb.Append(Environment.NewLine);

            sb.Append(AutoGeneratePOKey);
            sb.Append("=");
            sb.Append(AutoGeneratePO);
            sb.Append(Environment.NewLine);

            sb.Append(HideSetConsignmentKey);
            sb.Append("=");
            sb.Append(HideSetConsignment);
            sb.Append(Environment.NewLine);

            sb.Append(PrintedIdLengthKey);
            sb.Append("=");
            sb.Append(PrintedIdLength.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(UseOldEmailFormatKey);
            sb.Append("=");
            sb.Append(UseOldEmailFormat);
            sb.Append(Environment.NewLine);

            sb.Append(NewConsPrinterKey);
            sb.Append("=");
            sb.Append(NewConsPrinter);
            sb.Append(Environment.NewLine);

            sb.Append(UseBatteryKey);
            sb.Append("=");
            sb.Append(UseBattery);
            sb.Append(Environment.NewLine);

            sb.Append(DeliveryScanningKey);
            sb.Append("=");
            sb.Append(DeliveryScanning);
            sb.Append(Environment.NewLine);

            sb.Append(UseUpc128Key);
            sb.Append("=");
            sb.Append(UseUpc128);
            sb.Append(Environment.NewLine);

            sb.Append(SetParLevelKey);
            sb.Append("=");
            sb.Append(SetParLevel);
            sb.Append(Environment.NewLine);

            sb.Append(UseUpcCheckDigitKey);
            sb.Append("=");
            sb.Append(UseUpcCheckDigit);
            sb.Append(Environment.NewLine);

            sb.Append(RouteManagementKey);
            sb.Append("=");
            sb.Append(RouteManagement);
            sb.Append(Environment.NewLine);

            sb.Append(ScannerToUseKey);
            sb.Append("=");
            sb.Append(ScannerToUse);
            sb.Append(Environment.NewLine);

            sb.Append(ShipDateIsMandatoryKey);
            sb.Append("=");
            sb.Append(ShipDateIsMandatory);
            sb.Append(Environment.NewLine);

            sb.Append(ShipDateIsMandatoryForLoadKey);
            sb.Append("=");
            sb.Append(ShipDateIsMandatoryForLoad);
            sb.Append(Environment.NewLine);

            sb.Append(AskDEXUoMKey);
            sb.Append("=");
            sb.Append(AskDEXUoM);
            sb.Append(Environment.NewLine);

            sb.Append(WstcoKey);
            sb.Append("=");
            sb.Append(Wstco);
            sb.Append(Environment.NewLine);

            sb.Append(PresaleShipDateKey);
            sb.Append("=");
            sb.Append(PresaleShipDate);
            sb.Append(Environment.NewLine);

            sb.Append(CoreAsCreditKey);
            sb.Append("=");
            sb.Append(CoreAsCredit);
            sb.Append(Environment.NewLine);

            sb.Append(UseLSPByDefaultKey);
            sb.Append("=");
            sb.Append(UseLSPByDefault.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(TimeSheetCustomizationKey);
            sb.Append("=");
            sb.Append(TimeSheetCustomization.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(ExtraTextForPrinterKey);
            sb.Append("=");
            sb.Append(ExtraTextForPrinter.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(GenerateValuesLoadOrderKey);
            sb.Append("=");
            sb.Append(GenerateValuesLoadOrder.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(PrintClientOpenBalanceKey);
            sb.Append("=");
            sb.Append(PrintClientOpenBalance.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(CreditTemplateKey);
            sb.Append("=");
            sb.Append(CreditTemplate.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(ConsignmentShow0Key);
            sb.Append("=");
            sb.Append(ConsignmentShow0.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(ConsignmentMustCountAllKey);
            sb.Append("=");
            sb.Append(ConsignmentMustCountAll.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(ConsignmentKeepPriceKey);
            sb.Append("=");
            sb.Append(ConsignmentKeepPrice.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(PrintInvoiceNumberDownKey);
            sb.Append("=");
            sb.Append(PrintInvoiceNumberDown.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(PrintFromClientDetailKey);
            sb.Append("=");
            sb.Append(PrintFromClientDetail.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(ConcatUPCToNameKey);
            sb.Append("=");
            sb.Append(ConcatUPCToName.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(PrintUpcAsTextKey);
            sb.Append("=");
            sb.Append(PrintUpcAsText.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(InvoicePrefixKey);
            sb.Append("=");
            sb.Append(InvoicePrefix.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(SurveyQuestionsKey);
            sb.Append("=");
            sb.Append(SurveyQuestions.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(CanLeaveBatchKey);
            sb.Append("=");
            sb.Append(CanLeaveBatch.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(UpdateInventoryInPresaleKey);
            sb.Append("=");
            sb.Append(UpdateInventoryInPresale.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(SendTempPaymentsInBackgroundKey);
            sb.Append("=");
            sb.Append(SendTempPaymentsInBackground.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(DontGenerateLoadPrintedIdKey);
            sb.Append("=");
            sb.Append(DontGenerateLoadPrintedId.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(ShowBelow0InAdvancedTemplateKey);
            sb.Append("=");
            sb.Append(ShowBelow0InAdvancedTemplate.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(ShowRetailPriceForAddItemKey);
            sb.Append("=");
            sb.Append(ShowRetailPriceForAddItem.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(RoundTaxPerLineKey);
            sb.Append("=");
            sb.Append(RoundTaxPerLine.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(AllowExchangeKey);
            sb.Append("=");
            sb.Append(AllowExchange.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(UsePaymentDiscountKey);
            sb.Append("=");
            sb.Append(UsePaymentDiscount.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(OffersAddCommentKey);
            sb.Append("=");
            sb.Append(OffersAddComment.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(SendZplOrderKey);
            sb.Append("=");
            sb.Append(SendZplOrder.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(MultiplyConversionByCostInIracarAddItemKey);
            sb.Append("=");
            sb.Append(MultiplyConversionByCostInIracarAddItem.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(IncludeAvgWeightInCatalogPriceKey);
            sb.Append("=");
            sb.Append(IncludeAvgWeightInCatalogPrice.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(ShowOldReportsRegardlessKey);
            sb.Append("=");
            sb.Append(ShowOldReportsRegardless.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(CanLogoutKey);
            sb.Append("=");
            sb.Append(CanLogout.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(DontRoundInUIKey);
            sb.Append("=");
            sb.Append(DontRoundInUI.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(MustScanInTransferKey);
            sb.Append("=");
            sb.Append(MustScanInTransfer.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(TimeSheetAutomaticClockInKey);
            sb.Append("=");
            sb.Append(TimeSheetAutomaticClockIn.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(DontSortCompaniesByNameKey);
            sb.Append("=");
            sb.Append(DontSortCompaniesByName.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(PrintAllInventoriesInInvSummaryKey);
            sb.Append("=");
            sb.Append(PrintAllInventoriesInInvSummary.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(DicosaCustomizationKey);
            sb.Append("=");
            sb.Append(DicosaCustomization.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(CanModifyEnteredWeightKey);
            sb.Append("=");
            sb.Append(CanModifyEnteredWeight.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(ShowServiceReportKey);
            sb.Append("=");
            sb.Append(ShowServiceReport.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(MilagroCustomizationKey);
            sb.Append("=");
            sb.Append(MilagroCustomization.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(AllowOtherChargesKey);
            sb.Append("=");
            sb.Append(AllowOtherCharges.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(AutoEndInventoryPasswordKey);
            sb.Append("=");
            sb.Append(AutoEndInventoryPassword.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(CremiMexDepartmentsKey);
            sb.Append("=");
            sb.Append(CremiMexDepartments.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(BackGroundSyncKey);
            sb.Append("=");
            sb.Append(BackGroundSync.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(ShowInvoiceTotalKey);
            sb.Append("=");
            sb.Append(ShowInvoiceTotal.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(PrintUPCInventoryKey);
            sb.Append("=");
            sb.Append(PrintUPCInventory.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(PrintUPCOpenInvoicesKey);
            sb.Append("=");
            sb.Append(PrintUPCOpenInvoices.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(AddDefaultItemToCreditKey);
            sb.Append("=");
            sb.Append(AddDefaultItemToCredit.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(DefaultItemKey);
            sb.Append("=");
            sb.Append(DefaultItem.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(DistanceToOrderKey);
            sb.Append("=");
            sb.Append(DistanceToOrder.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(ProductFileCreationDateKey);
            sb.Append("=");
            sb.Append(ProductFileCreationDate.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(LoadLotInTransferKey);
            sb.Append("=");
            sb.Append(LoadLotInTransfer.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(MustEndOrdersKey);
            sb.Append("=");
            sb.Append(MustEndOrders.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(ProductInMultipleCategoryKey);
            sb.Append("=");
            sb.Append(ProductInMultipleCategory.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(AllowEditRelatedKey);
            sb.Append("=");
            sb.Append(AllowEditRelated.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(AddQtyTotalRegardlessUoMKey);
            sb.Append("=");
            sb.Append(AddQtyTotalRegardlessUoM.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(AllowEditRelatedKey);
            sb.Append("=");
            sb.Append(AllowEditRelated.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(AutoGenerateLoadOrderKey);
            sb.Append("=");
            sb.Append(AutoGenerateLoadOrder.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(ApplyDiscountAfterTaxesKey);
            sb.Append("=");
            sb.Append(ApplyDiscountAfterTaxes.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(ExtendedPaymentOptionsKey);
            sb.Append("=");
            sb.Append(ExtendedPaymentOptions.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(GroupRelatedWhenPrintingKey);
            sb.Append("=");
            sb.Append(GroupRelatedWhenPrinting.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(PrintLotPreOrderKey);
            sb.Append("=");
            sb.Append(PrintLotPreOrder.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(PrintLotOrderKey);
            sb.Append("=");
            sb.Append(PrintLotOrder.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(CanVoidFOrdersKey);
            sb.Append("=");
            sb.Append(CanVoidFOrders.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(TransferOffPasswordKey);
            sb.Append("=");
            sb.Append(TransferOffPassword);
            sb.Append(Environment.NewLine);

            sb.Append(BetaFeaturesKey);
            sb.Append("=");
            sb.Append(BetaFeatures.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(TrackTermsPaymentBottonKey);
            sb.Append("=");
            sb.Append(TrackTermsPaymentBotton.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(MustBeOnlineAlwaysKey);
            sb.Append("=");
            sb.Append(MustBeOnlineAlways.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(EnableLiveDataKey);
            sb.Append("=");
            sb.Append(EnableLiveData.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(CatalogQuickAddKey);
            sb.Append("=");
            sb.Append(CatalogQuickAdd.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(MustAddImageToFinalizedCreditKey);
            sb.Append("=");
            sb.Append(MustAddImageToFinalizedCredit.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(TransferPasswordAtSavingKey);
            sb.Append("=");
            sb.Append(TransferPasswordAtSaving.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(OpenTemplateEmptyByDefaultKey);
            sb.Append("=");
            sb.Append(OpenTemplateEmptyByDefault.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(DeletePaymentsInTabKey);
            sb.Append("=");
            sb.Append(DeletePaymentsInTab.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(AlwaysFreshCustomizationKey);
            sb.Append("=");
            sb.Append(AlwaysFreshCustomization.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(PaymentBankIsMandatoryKey);
            sb.Append("=");
            sb.Append(PaymentBankIsMandatory.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(OrderCanBeChangedKey);
            sb.Append("=");
            sb.Append(OrderCanBeChanged.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(CreditCanBeChangedKey);
            sb.Append("=");
            sb.Append(CreditCanBeChanged.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(MustAddImageToFinalizedKey);
            sb.Append("=");
            sb.Append(MustAddImageToFinalized.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(ShowOHQtyInSelfServiceKey);
            sb.Append("=");
            sb.Append(ShowOHQtyInSelfService.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(ShowProfitWhenChangingPriceKey);
            sb.Append("=");
            sb.Append(ShowProfitWhenChangingPrice.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(ShowOrderStatusKey);
            sb.Append("=");
            sb.Append(ShowOrderStatus.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(DaysToShowSentOrdersKey);
            sb.Append("=");
            sb.Append(DaysToShowSentOrders.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(MaxDiscountPerOrderKey);
            sb.Append("=");
            sb.Append(MaxDiscountPerOrder.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(DaysToBringOrderStatusKey);
            sb.Append("=");
            sb.Append(DaysToBringOrderStatus.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(DaysToRunReportsKey);
            sb.Append("=");
            sb.Append(DaysToRunReports.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(LowestPriceLevelIdKey);
            sb.Append("=");
            sb.Append(LowestPriceLevelId.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(CanChangeUomInCreditsKey);
            sb.Append("=");
            sb.Append(CanChangeUomInCredits.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(RequestAuthPinForLoginKey);
            sb.Append("=");
            sb.Append(RequestAuthPinForLogin.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(ShowImageInCatalogKey);
            sb.Append("=");
            sb.Append(ShowImageInCatalog.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(ShowPromoCheckboxKey);
            sb.Append("=");
            sb.Append(ShowPromoCheckbox.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(OrderMinimumQtyKey);
            sb.Append("=");
            sb.Append(OrderMinimumQty.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(OrderMinQtyMinAmountKey);
            sb.Append("=");
            sb.Append(OrderMinQtyMinAmount.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(ProductMinQtyKey);
            sb.Append("=");
            sb.Append(ProductMinQty.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(MaxQtyInOrderKey);
            sb.Append("=");
            sb.Append(MaxQtyInOrder.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(OrderMinimumTotalPriceKey);
            sb.Append("=");
            sb.Append(OrderMinimumTotalPrice.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(HideTotalOrderKey);
            sb.Append("=");
            sb.Append(HideTotalOrder.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(DisplayTaxOnCatalogAndPrintKey);
            sb.Append("=");
            sb.Append(DisplayTaxOnCatalogAndPrint.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(PrintInvoiceAsReceiptKey);
            sb.Append("=");
            sb.Append(PrintInvoiceAsReceipt.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(HidePrintBatchKey);
            sb.Append("=");
            sb.Append(HidePrintBatch.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(ShowInvoicesCreditsInPaymentsKey);
            sb.Append("=");
            sb.Append(ShowInvoicesCreditsInPayments.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(CanChangeUoMInTransferKey);
            sb.Append("=");
            sb.Append(CanChangeUoMInTransfer.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(SupervisorEmailForRequestsKey);
            sb.Append("=");
            sb.Append(SupervisorEmailForRequests.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(SortClientKey);
            sb.Append("=");
            sb.Append(SortClient.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(TransferScanningAddsProductKey);
            sb.Append("=");
            sb.Append(TransferScanningAddsProduct.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(SensationalAssetTrackingKey);
            sb.Append("=");
            sb.Append(SensationalAssetTracking.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(PrintRefusalReportByStoreKey);
            sb.Append("=");
            sb.Append(PrintRefusalReportByStore.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(VoidPaymentsKey);
            sb.Append("=");
            sb.Append(VoidPayments.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(SpectrumFloralCustomizationKey);
            sb.Append("=");
            sb.Append(SpectrumFloralCustomization.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(CanModifyQuotesKey);
            sb.Append("=");
            sb.Append(CanModifyQuotes.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(ShowDiscountIfAppliedKey);
            sb.Append("=");
            sb.Append(ShowDiscountIfApplied.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(ShowReportsKey);
            sb.Append("=");
            sb.Append(ShowReports.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(AllowToCollectInvoicesKey);
            sb.Append("=");
            sb.Append(AllowToCollectInvoices.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(PrasekCustomizationKey);
            sb.Append("=");
            sb.Append(PrasekCustomization.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(AlwaysShowDefaultUoMKey);
            sb.Append("=");
            sb.Append(AlwaysShowDefaultUoM.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(ButlerCustomizationKey);
            sb.Append("=");
            sb.Append(ButlerCustomization.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(ShowPrintPickTicketKey);
            sb.Append("=");
            sb.Append(ShowPrintPickTicket.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(CaribCustomizationKey);
            sb.Append("=");
            sb.Append(CaribCustomization.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(HideTotalInPrintedLineKey);
            sb.Append("=");
            sb.Append(HideTotalInPrintedLine.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(PrintCopyKey);
            sb.Append("=");
            sb.Append(PrintCopy.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(ConsignmentKey);
            sb.Append("=");
            sb.Append(Consignment.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(MustEndOfDayDailyKey);
            sb.Append("=");
            sb.Append(MustEndOfDayDaily.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(ForceEODWhenDateChangesKey);
            sb.Append("=");
            sb.Append(ForceEODWhenDateChanges.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(UsesTermsKey);
            sb.Append("=");
            sb.Append(UsesTerms.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(UseReshipKey);
            sb.Append("=");
            sb.Append(UseReship.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(UseClockInOutKey);
            sb.Append("=");
            sb.Append(UseClockInOut.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(DexVersionKey);
            sb.Append("=");
            sb.Append(DexVersion);
            sb.Append(Environment.NewLine);

            sb.Append(DexDefaultUnitKey);
            sb.Append("=");
            sb.Append(DexDefaultUnit.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(OldPrinterKey);
            sb.Append("=");
            sb.Append(OldPrinter);
            sb.Append(Environment.NewLine);

            sb.Append(UpdateWhenEndDayKey);
            sb.Append("=");
            sb.Append(UpdateWhenEndDay.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(AutoAcceptLoadKey);
            sb.Append("=");
            sb.Append(AutoAcceptLoad.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(RemoveWarningsKey);
            sb.Append("=");
            sb.Append(RemoveWarnings.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(GroupLinesWhenPrintingKey);
            sb.Append("=");
            sb.Append(GroupLinesWhenPrinting.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(ShortInventorySettlementKey);
            sb.Append("=");
            sb.Append(ShortInventorySettlement.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(PrintReportsRequiredKey);
            sb.Append("=");
            sb.Append(PrintReportsRequired.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(LoadOrderEmptyKey);
            sb.Append("=");
            sb.Append(LoadOrderEmpty.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(AcceptLoadPrintRequiredKey);
            sb.Append("=");
            sb.Append(AcceptLoadPrintRequired.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(PreSaleKey);
            sb.Append("=");
            sb.Append(PreSale.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(LeftOrderTemplateEmptyKey);
            sb.Append("=");
            sb.Append(LeftOrderTemplateEmpty.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(EmptyTruckAtEndOfDayKey);
            sb.Append("=");
            sb.Append(EmptyTruckAtEndOfDay.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(LoadRequestKey);
            sb.Append("=");
            sb.Append(LoadRequest.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(LoadRequiredKey);
            sb.Append("=");
            sb.Append(LoadRequired.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(PickCompanyKey);
            sb.Append("=");
            sb.Append(PickCompany.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(ProductCatalogKey);
            sb.Append("=");
            sb.Append(ProductCatalog.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(DeliveryKey);
            sb.Append("=");
            sb.Append(Delivery.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(SendLogByEmailKey);
            sb.Append("=");
            sb.Append(SendLogByEmail.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(EmailOrderKey);
            sb.Append("=");
            sb.Append(EmailOrder.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(MustPrintPreOrderKey);
            sb.Append("=");
            sb.Append(MustPrintPreOrder.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(CanIncreasePriceKey);
            sb.Append("=");
            sb.Append(CanIncreasePrice.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(FakePreOrderKey);
            sb.Append("=");
            sb.Append(FakePreOrder.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(PrintTruncateNamesKey);
            sb.Append("=");
            sb.Append(PrintTruncateNames.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(PopulateTemplateAuthProdKey);
            sb.Append("=");
            sb.Append(PopulateTemplateAuthProd.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(OneDocKey);
            sb.Append("=");
            sb.Append(OneDoc.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(CanAddClientKey);
            sb.Append("=");
            sb.Append(CanAddClient.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(SetPOKey);
            sb.Append("=");
            sb.Append(SetPO.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(InvReqKey);
            sb.Append("=");
            sb.Append(InventoryRequestEmail);
            sb.Append(Environment.NewLine);

            sb.Append(CanGoBelow0Key);
            sb.Append("=");
            sb.Append(CanGoBelow0.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(NoPriceChangeDeliveriesKey);
            sb.Append("=");
            sb.Append(NoPriceChangeDeliveries.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(WarningDumpReturnKey);
            sb.Append("=");
            sb.Append(WarningDumpReturn.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(UseOrderIdKey);
            sb.Append("=");
            sb.Append(UseOrderId.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(LastOrderIdKey);
            sb.Append("=");
            sb.Append(LastPrintedId.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(LastPresaleOrderIdKey);
            sb.Append("=");
            sb.Append(LastPresalePrintedId.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(RoundKey);
            sb.Append("=");
            sb.Append(Round.ToString());
            sb.Append(Environment.NewLine);

            sb.Append(DexKey);
            sb.Append("=");
            sb.Append(DexAvailable);
            sb.Append(Environment.NewLine);

            sb.Append(PrintUPCKey);
            sb.Append("=");
            sb.Append(PrintUPC);
            sb.Append(Environment.NewLine);

            sb.Append(CanChangeSalesmanIdKey);
            sb.Append("=");
            sb.Append(CanChangeSalesmanId);
            sb.Append(Environment.NewLine);

            sb.Append(BottomOrderPrintTextKey);
            sb.Append("=");
            sb.Append(BottomOrderPrintText);
            sb.Append(Environment.NewLine);

            sb.Append(SingleScanStrokeKey);
            sb.Append("=");
            sb.Append(SingleScanStroke);
            sb.Append(Environment.NewLine);

            sb.Append(PrintInvoiceSortKey);
            sb.Append("=");
            sb.Append(PrintInvoiceSort);
            sb.Append(Environment.NewLine);

            sb.Append(PrintClientSortKey);
            sb.Append("=");
            sb.Append(PrintClientSort);
            sb.Append(Environment.NewLine);

            sb.Append(MustSendOrdersFirstKey);
            sb.Append("=");
            sb.Append(MustSendOrdersFirst);
            sb.Append(Environment.NewLine);

            sb.Append(AllowDiscountKey);
            sb.Append("=");
            sb.Append(AllowDiscount);
            sb.Append(Environment.NewLine);

            sb.Append(LocationIsMandatoryKey);
            sb.Append("=");
            sb.Append(LocationIsMandatory);
            sb.Append(Environment.NewLine);

            sb.Append(AlwaysAddItemsToOrderKey);
            sb.Append("=");
            sb.Append(AlwaysAddItemsToOrder);
            sb.Append(Environment.NewLine);

            sb.Append(FreeItemsNeedCommentsKey);
            sb.Append("=");
            sb.Append(FreeItemsNeedComments);
            sb.Append(Environment.NewLine);

            sb.Append(LotIsMandatoryKey);
            sb.Append("=");
            sb.Append(LotIsMandatory);
            sb.Append(Environment.NewLine);

            sb.Append(PaymentAvailableKey);
            sb.Append("=");
            sb.Append(PaymentAvailable);
            sb.Append(Environment.NewLine);

            sb.Append(DisablePaymentIfTermDaysMoreThan0Key);
            sb.Append("=");
            sb.Append(DisablePaymentIfTermDaysMoreThan0);
            sb.Append(Environment.NewLine);

            sb.Append(PaymentRequiredKey);
            sb.Append("=");
            sb.Append(PaymentRequired);
            sb.Append(Environment.NewLine);

            sb.Append(PrinterToUseKey);
            sb.Append("=");
            sb.Append(PrinterToUse);
            sb.Append(Environment.NewLine);

            sb.Append(InvoiceIdKey);
            sb.Append("=");
            sb.Append(InvoiceIdProvider);
            sb.Append(Environment.NewLine);

            sb.Append(SignatureRequiredKey);
            sb.Append("=");
            sb.Append(SignatureRequired);
            sb.Append(Environment.NewLine);

            sb.Append(LotKey);
            sb.Append("=");
            sb.Append(UseLot);
            sb.Append(Environment.NewLine);

            sb.Append(FakeUseLotKey);
            sb.Append("=");
            sb.Append(FakeUseLot);
            sb.Append(Environment.NewLine);

            sb.Append(DisplayPurchasePriceKey);
            sb.Append("=");
            sb.Append(DisplayPurchasePrice);
            sb.Append(Environment.NewLine);

            sb.Append(AnyPriceIsAcceptableKey);
            sb.Append("=");
            sb.Append(AnyPriceIsAcceptable);
            sb.Append(Environment.NewLine);

            sb.Append(UseLSPKey);
            sb.Append("=");
            sb.Append(UseLSP);
            sb.Append(Environment.NewLine);

            sb.Append(DaysToKeepOrderKey);
            sb.Append("=");
            sb.Append(DaysToKeepOrder);
            sb.Append(Environment.NewLine);

            sb.Append(TransferPasswordKey);
            sb.Append("=");
            sb.Append(TransferPassword);
            sb.Append(Environment.NewLine);

            sb.Append(AddInventoryPasswordKey);
            sb.Append("=");
            sb.Append(AddInventoryPassword);
            sb.Append(Environment.NewLine);

            sb.Append(TrackInventoryKey);
            sb.Append("=");
            sb.Append(TrackInventory);
            sb.Append(Environment.NewLine);

            sb.Append(CanModifyInventoryKey);
            sb.Append("=");
            sb.Append(CanModifyInventory);
            sb.Append(Environment.NewLine);

            sb.Append(HideSelectSitesFromMenuKey);
            sb.Append("=");
            sb.Append(HideSelectSitesFromMenu);
            sb.Append(Environment.NewLine);

            sb.Append(PrinterAvailableKey);
            sb.Append("=");
            sb.Append(PrinterAvailable);
            sb.Append(Environment.NewLine);

            sb.Append(PrintingRequiredKey);
            sb.Append("=");
            sb.Append(PrintingRequired);
            sb.Append(Environment.NewLine);

            sb.Append(AllowCreditOrdersKey);
            sb.Append("=");
            sb.Append(AllowCreditOrders);
            sb.Append(Environment.NewLine);

            sb.Append(MustUpdateDailyKey);
            sb.Append("=");
            sb.Append(MustUpdateDaily);
            sb.Append(Environment.NewLine);

            sb.Append(AllowFreeItemsKey);
            sb.Append("=");
            sb.Append(AllowFreeItems);
            sb.Append(Environment.NewLine);

            sb.Append(SendLoadOrderKey);
            sb.Append("=");
            sb.Append(SendLoadOrder);
            sb.Append(Environment.NewLine);

            sb.Append(UseLocationKey);
            sb.Append("=");
            sb.Append(UseLocation);
            sb.Append(Environment.NewLine);

            sb.Append(AllowOrderForClientOverCreditLimitKey);
            sb.Append("=");
            sb.Append(AllowOrderForClientOverCreditLimit);
            sb.Append(Environment.NewLine);

            sb.Append(UserCanChangePricesKey);
            sb.Append("=");
            sb.Append(UserCanChangePrices);
            sb.Append(Environment.NewLine);

            sb.Append(SalesReportTotalCreditsSubstractedKey);
            sb.Append("=");
            sb.Append(SalesReportTotalCreditsSubstracted);
            sb.Append(Environment.NewLine);

            sb.Append(ShowSentTransactionsKey);
            sb.Append("=");
            sb.Append(ShowSentTransactions);
            sb.Append(Environment.NewLine);

            sb.Append(MustSelectDepartmentKey);
            sb.Append("=");
            sb.Append(MustSelectDepartment);
            sb.Append(Environment.NewLine);

            sb.Append(PrintExternalInvoiceAsOrderKey);
            sb.Append("=");
            sb.Append(PrintExternalInvoiceAsOrder);
            sb.Append(Environment.NewLine);

            sb.Append(UseProductionForPaymentsKey);
            sb.Append("=");
            sb.Append(UseProductionForPayments);
            sb.Append(Environment.NewLine);

            sb.Append(UseCatalogWithFullTemplateKey);
            sb.Append("=");
            sb.Append(UseCatalogWithFullTemplate);
            sb.Append(Environment.NewLine);

            sb.Append(StartingPercentageBasedOnCostKey);
            sb.Append("=");
            sb.Append(StartingPercentageBasedOnCost);
            sb.Append(Environment.NewLine);

            sb.Append(ShowBillOfLadingPdfKey);
            sb.Append("=");
            sb.Append(ShowBillOfLadingPdf);
            sb.Append(Environment.NewLine);

            return sb.ToString();
        }

        public static string GetSSID()
        {
            return helper != null ? helper.GetSSID() : string.Empty;
        }

        static string WhichAddressToUse()
        {
            string currentSSID = GetSSID().Replace("\"", "").Trim().ToUpperInvariant();
            Logger.CreateLog("Connected to: [" + (currentSSID ?? "not connected") + "]");
            var ssid = Config.SSID.Replace("\"", "").Trim().ToUpperInvariant();
            if (!string.IsNullOrEmpty(SSID) &&
                string.Compare(currentSSID, ssid, StringComparison.OrdinalIgnoreCase) == 0)
                return LanAddress;
            return IPAddressGateway;
        }

        static public void SaveCurrentOrderId()
        {
            lock (FileOperationsLocker.lockFilesObject)
            {
                try
                {
                    //FileOperationsLocker.InUse = true;

                    using (StreamWriter writer = new StreamWriter(Config.CurrentOrderFile, false))
                    {
                        writer.Write(Config.CurrentOrderId.ToString());
                        writer.Write(",");
                        writer.Write(DateTime.Today.Ticks);
                    }
                }
                finally
                {
                    //FileOperationsLocker.InUse = false;
                }
            }
        }

        static public void SaveCurrentBatchId()
        {
            lock (FileOperationsLocker.lockFilesObject)
            {
                try
                {
                    //FileOperationsLocker.InUse = true;

                    using (StreamWriter writer = new StreamWriter(Config.CurrentBatchFile, false))
                    {
                        writer.Write(Config.CurrentBatchId.ToString());
                        writer.Write(",");
                        writer.Write(DateTime.Today.Ticks);
                    }
                }
                finally
                {
                    //FileOperationsLocker.InUse = false;
                }
            }
        }

        private static void LoadCurrentOrderId()
        {
            Config.CurrentOrderId = 0;
            Config.CurrentOrderDate = DateTime.Today;
            if (File.Exists(Config.CurrentOrderFile))
                using (StreamReader reader = new StreamReader(Config.CurrentOrderFile))
                {
                    string[] parts = reader.ReadLine().Split(new char[] { ',' });
                    DateTime savedDate = new DateTime(Convert.ToInt64(parts[1]));
                    if (savedDate.Date == DateTime.Today)
                    {
                        Config.CurrentOrderId = Convert.ToInt32(parts[0]);
                    }
                }
        }

        private static void LoadCurrentBatchId()
        {
            Config.CurrentBatchId = 0;
            Config.CurrentBatchDate = DateTime.Today;
            if (File.Exists(Config.CurrentBatchFile))
                using (StreamReader reader = new StreamReader(Config.CurrentBatchFile))
                {
                    string[] parts = reader.ReadLine().Split(new char[] { ',' });
                    DateTime savedDate = new DateTime(Convert.ToInt64(parts[1]));
                    if (savedDate.Date == DateTime.Today)
                    {
                        Config.CurrentBatchId = Convert.ToInt32(parts[0]);
                    }
                }
        }

        public static void SaveLastPosition()
        {
            Preferences.Set("LastLatitudeKey", Convert.ToSingle(DataAccess.LastLatitude));
            Preferences.Set("LastLongitudeKey", Convert.ToSingle(DataAccess.LastLongitude));
        }

        public static void GetLastPosition()
        {
            DataAccess.LastLatitude = Preferences.Get("LastLatitudeKey", 0L);
            DataAccess.LastLongitude = Preferences.Get("LastLongitudeKey", 0L);
        }

        public static bool UseAllowanceForOrder(Order order, bool damaged = false)
        {
            return Config.UseAllowance && !order.AsPresale &&
                   (order.OrderType == OrderType.Order || order.OrderType == OrderType.Credit);
        }

        #region Naura

        public static void SaveSystemSettings()
        {
            if (string.IsNullOrEmpty(iPAddressGateway) || Port == 0 || SalesmanId == 0) return;

            if (File.Exists(LacupConfigFile)) File.Delete(LacupConfigFile);

            using (var writer = new StreamWriter(LacupConfigFile))
            {
                writer.WriteLine(IPAddressGateway);
                writer.WriteLine(LanAddress);
                writer.WriteLine(Port);
                writer.WriteLine(SSID);
                writer.WriteLine(VendorName);
                writer.WriteLine(SalesmanId);
            }
        }

        public static void LoadSystemSettings()
        {
            if (!File.Exists(LacupConfigFile)) return;

            using (var reader = new StreamReader(LacupConfigFile))
            {
                string line = reader.ReadLine();
                IPAddressGateway = line;

                line = reader.ReadLine();
                LanAddress = line;

                line = reader.ReadLine();
                Port = Convert.ToInt32(line);

                line = reader.ReadLine();
                SSID = line;

                line = reader.ReadLine();
                VendorName = line;

                line = reader.ReadLine();
                SalesmanId = Convert.ToInt32(line);
            }

            CanChangeSalesmanId = !MasterDevice;
        }

        static bool? useCatalog = null;

        public static bool UseCatalog
        {
            get
            {
                if (!useCatalog.HasValue)
                {
                    var activity = DataAccess.ActivityProvider.GetActivityType(ActivityNames.ProductListActivity);

                    return activity.FullName == "LaceupAndroidApp.ProductCatalogActivity";
                }

                return useCatalog.Value;
            }
            set { useCatalog = value; }
        }

        #endregion

        public static void SaveSessionId()
        {
            if (File.Exists(SessionIdFile)) File.Delete(SessionIdFile);

            using (StreamWriter writer = new StreamWriter(Config.SessionIdFile, false))
            {
                writer.Write(Config.SessionId);
                writer.Close();
            }
        }

        public static void LoadSessionId()
        {
            Config.SessionId = string.Empty;
            if (!File.Exists(Config.SessionIdFile)) return;

            using (StreamReader reader = new StreamReader(Config.SessionIdFile))
            {
                Config.SessionId = reader.ReadLine();
                reader.Close();
            }
        }

        public static void ClearSessionId()
        {
            Config.SessionId = string.Empty;
            if (File.Exists(Config.SessionIdFile)) File.Delete(Config.SessionIdFile);
        }

        public static bool CheckIfCanIncreasePrice(Order order, Product product)
        {
            if (!string.IsNullOrEmpty(order.Client.NonvisibleExtraPropertiesAsString))
            {
                var value = DataAccess.GetSingleUDF("productscanchangeprice",
                    order.Client.NonvisibleExtraPropertiesAsString);

                if (!string.IsNullOrEmpty(value))
                {
                    var parts = value.Split(",");

                    foreach (var p in parts)
                    {
                        int pid = 0;
                        Int32.TryParse(p, out pid);

                        if (product.ProductId == pid) return false;
                    }
                }
            }

            return Config.CanIncreasePrice;
        }

        public static bool CanChangePrice(Order order, Product product, bool isCredit)
        {
            if (isCredit && creditsPriceInSetting) return Config.UserCanChangePricesCredits;

            if (!isCredit && salesPriceInSetting) return Config.UserCanChangePricesSales;

            bool canChangePrice = false;

            if (product != null)
            {
                var _prodExtField = product.NonVisibleExtraFields != null
                    ? product.NonVisibleExtraFields.FirstOrDefault(x => x.Item1.ToLower() == "pricechangeable")
                    : null;
                canChangePrice = _prodExtField != null && (_prodExtField.Item2.ToLower() == "yes" ||
                                                           _prodExtField.Item2.ToLower() == "y" ||
                                                           _prodExtField.Item2.ToLower() == "1");
            }

            if (!canChangePrice)
            {
                var _clientExtField = order.Client.NonVisibleExtraProperties != null
                    ? order.Client.NonVisibleExtraProperties.FirstOrDefault(x => x.Item1.ToLower() == "pricechangeable")
                    : null;
                canChangePrice = _clientExtField != null && (_clientExtField.Item2.ToLower() == "yes" ||
                                                             _clientExtField.Item2.ToLower() == "y" ||
                                                             _clientExtField.Item2.ToLower() == "1");
            }

            var sm = Salesman.FullList.FirstOrDefault(x => x.Id == Config.SalesmanId);
            if (sm != null)
                if (!string.IsNullOrEmpty(sm.ExtraProperties) &&
                    sm.ExtraProperties.ToLower().Contains("canchangepricebasedonproductorclient=0"))
                    return false;
            return canChangePrice || Config.UserCanChangePrices || Config.CanIncreasePrice;
        }

        public static bool ExistPendingTransfer
        {
            get
            {
                var tempFile = Path.Combine(Config.DataPath, "On_temp_LoadOrderPath.xml");
                if (!File.Exists(tempFile))
                {
                    tempFile = Path.Combine(Config.DataPath, "Off_temp_LoadOrderPath.xml");
                    return File.Exists(tempFile);
                }

                return true;
            }
        }

        public static string GetDeviceInfo()
        {
            var dexText = string.Empty;
            var dex_version = Config.VersatileDEXVersion;

            if (!string.IsNullOrEmpty(dex_version)) dexText = " DEX:" + dex_version;

            return string.Format("{0} Android {1}", Version, (helper.GetDeviceModel() + dexText));
        }

        public static string GetAuthString()
        {
            var deviceId = Config.DeviceId;
            if (Config.SelfService)
                deviceId = "SelfService_" + deviceId;
            else if (Config.SelfServiceUser) deviceId = "UserSelfService_" + deviceId;

            return deviceId + "=" + Config.SalesmanId.ToString(CultureInfo.InvariantCulture);
        }

        public static bool UseFullTemplateForClient(Client client)
        {
            var useFullTemplate = DataAccess.GetSingleUDF("fullTemplate", client.NonvisibleExtraPropertiesAsString);
            if (!string.IsNullOrEmpty(useFullTemplate) && useFullTemplate == "0") return false;
            return Config.UseFullTemplate;
        }

        public static bool EndingInventoryCounted
        {
            get => Preferences.Get("EndingInventoryCountedKey", false);
            set => Preferences.Set("EndingInventoryCountedKey", value);
        }

        public static bool EnableSelfServiceModule => (Config.SelfService || Config.SelfServiceUser);

        public static bool RouteReturnIsMandatory { get; private set; }
        public static bool EndingInvIsMandatory { get; private set; }

        public static Dictionary<string, string> Icons_Sets = new Dictionary<string, string>()
        {
            ["10.1.10.222_1113"] = "Test1",
            ["3_30416"] = "DalCampo",
            ["7_47080"] = "Bemka",
            ["8_1064"] = "Prida",
            ["9_64190"] = "Inca"
        };
    }
}