using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;

namespace LaceupMigration;

public partial class Config
{
    public static bool ShowBillOfLadingPdf { get; set; }
    public static bool ProductNameHasDifferentColor { get; set; }
    public static bool UseRetailPriceForSales { get; set; }
    public static int StartingPercentageBasedOnCost { get; set; }
    public static string GroupClientsByCat { get; set; }
    public static string ExtraInfoBottomPrint { get; set; }
    public static string ProductCategoryNameIdentifier { get; set; }
    public static string SupervisorEmailForRequests { get; set; }
    public static string SortClient { get; set; }
    public static string TrackTermsPaymentBotton { get; set; }
    public static bool ShowAllAvailableLoads { get; set; }
    public static bool RecalculateStops { get; set; }
    public static bool DefaultItemHasPrice { get; set; }

    public static bool OrderHistoryByClient { get; set; }
    public static bool PrintInvoiceAsReceipt { get; set; }
    public static bool HidePrintBatch { get; set; }
    public static bool ShowInvoicesCreditsInPayments { get; set; }
    public static bool CatalogQuickAdd { get; set; }
    public static bool TransferPasswordAtSaving { get; set; }
    public static bool OpenTemplateEmptyByDefault { get; set; }
    public static bool DeletePaymentsInTab { get; set; }
    public static bool AlwaysFreshCustomization { get; set; }
    public static bool MustAddImageToFinalizedCredit { get; set; }
    public static bool PaymentBankIsMandatory { get; set; }
    public static bool OrderCanBeChanged { get; set; }
    public static bool CreditCanBeChanged { get; set; }
    public static float OrderMinimumQty { get; set; }
    public static bool OrderMinQtyMinAmount { get; set; }
    public static bool ProductMinQty { get; set; }
    public static float MaxQtyInOrder { get; set; }
    public static bool ShowOHQtyInSelfService { get; set; }
    public static bool ShowProfitWhenChangingPrice { get; set; }
    public static bool ShowOrderStatus { get; set; }
    public static bool BringBranchInventories { get; set; }
    public static bool HideContactName { get; set; }
    public static bool PrintPaymentRegardless { get; set; }
    public static bool GeorgeHoweCustomization { get; set; }
    public static bool TransferScanningAddsProduct { get; set; }
    public static bool PrintRefusalReportByStore { get; set; }
    public static bool VoidPayments { get; set; }
    public static bool SpectrumFloralCustomization { get; set; }
    public static bool CanModifyQuotes { get; set; }
    public static bool ShowDiscountIfApplied { get; set; }
    public static bool ShowReports { get; set; }
    public static bool AllowToCollectInvoices { get; set; }
    public static bool PrintInveSettlementRegardless { get; set; }
    public static bool CanRestoreFromFile { get; set; }
    public static int MaxDiscountPerOrder { get; set; }
    public static int DaysToShowSentOrders { get; set; }
    public static int DaysToBringOrderStatus { get; set; }
    public static int DaysToRunReports { get; set; }
    public static int LowestPriceLevelId { get; set; }
    public static bool CanChangeUomInCredits { get; set; }
    public static bool RequestAuthPinForLogin { get; set; }
    public static int LoginTimeOut { get; set; }
    public static bool MustAddImageToFinalized { get; set; }
    public static float OrderMinimumTotalPrice { get; set; }
    public static bool CanChangeUoMInTransfer { get; set; }
    public static bool SensationalAssetTracking { get; set; }

    public static bool SalesReportTotalCreditsSubstracted { get; set; }

    public static string SurveyQuestions { get; private set; }
    public static string LastSignIn { get; set; }

    public static bool AddInventoryOnPo { get; set; }
    public static bool ViewAllComments { get; set; }
    public static string PrintLabelHeight { get; set; }
    public static bool AllowSelectPriceLevel { get; private set; }
    public static bool UpdateInventoryRegardless { get; private set; }
    public static int ParLevelHistoryNumVisit { get; private set; }
    public static bool ShowOnlyInvoiceForSalesman { get; private set; }
    public static bool HideInvoicesAndBalance { get; private set; }
    public static bool ImageInOrderMandatory { get; private set; }
    public static bool ImageInNoServiceMandatory { get; private set; }
    public static int PrintCopiesInFinalizeBatch { get; private set; }
    public static int WeeksOfSalesHistory { get; set; }
    public static int DaysOfProjectionInTemplate { get; set; }
    public static bool UseFastPrinter { get; private set; }
    public static bool CasaSanchezCustomization { get; set; }
    public static int BarcodeDecoder { get; private set; }
    public static bool TimeSheetCustomization { get; set; }
    public static bool GenerateValuesLoadOrder { get; set; }

    static string sessionId = "";

    public static string SessionId
    {
        get { return sessionId; }
        set { sessionId = value; }
    }

    public static int MinimumOrderProductId { get; set; }
    public static bool Simone { get; set; }

    public static bool IgnoreDiscountInCredits { get; set; }

    public static bool CheckInventoryInPreSale { get; set; }

    public static bool AlwaysUpdateNewPar { get; set; }

    public static bool AskDEXUoM { get; set; }

    public static bool Wstco { get; set; }

    public static bool UseLSPByDefault { get; set; }

    public static bool CreditTemplate { get; set; }

    public static bool EnableSampleOrder { get; set; }
    public static bool HidePriceInSelfService { get; set; }
    public static string EnablePaymentsByTerms { get; private set; }
    public static bool RequireLotForDumps { get; private set; }
    public static bool ShowWarehouseInvInSummary { get; private set; }
    public static bool ConsignmentShow0 { get; set; }

    public static bool ConsignmentMustCountAll { get; set; }

    public static bool ConsignmentKeepPrice { get; set; }

    public static bool PrintInvoiceNumberDown { get; set; }

    public static bool PrintFromClientDetail { get; set; }

    public static bool ConcatUPCToName { get; set; }

    public static bool PrintUpcAsText { get; set; }

    public static bool CanLeaveBatch { get; set; }

    public static string InvoicePrefix { get; set; }

    public static string InvoicePresalePrefix { get; set; }

    public static string AutoEndInventoryPassword { get; set; }

    public static int DistanceToOrder { get; set; }

    public static long ProductFileCreationDate { get; set; }
    //DEPRECATED
    //public static bool AutoEndInventory { get; set; }

    public static bool MustBeOnlineAlways { get; set; }
    public static bool EnableLiveData { get; set; }
    public static bool BackGroundSync { get; set; }

    public static string DexVersion { get; set; }
    public static string CremiMexDepartments { get; set; }
    public static string DexDefaultUnit { get; set; }

    public static bool ShowInvoiceTotal { get; set; }

    public static int DefaultItem { get; set; }

    public static bool AddDefaultItemToCredit { get; set; }

    public static string TransferOffPassword { get; set; }

    public static bool LoadLotInTransfer { get; set; }

    public static bool PrintUPCInventory { get; set; }
    public static bool PrintUPCOpenInvoices { get; set; }

    public static bool AddQtyTotalRegardlessUoM { get; set; }
    public static bool ShowProductsWith0Inventory { get; set; }
    public static bool DisplayTaxOnCatalogAndPrint { get; set; }
    public static bool PrasekCustomization { get; set; }
    public static bool AlwaysShowDefaultUoM { get; set; }
    public static bool ButlerCustomization { get; set; }
    public static bool CaribCustomization { get; set; }
    public static bool ShowPrintPickTicket { get; set; }
    public static string ExtraTextForPrinter { get; private set; }
    public static bool MustEndOrders { get; set; }
    public static bool ProductInMultipleCategory { get; set; }
    public static bool AllowEditRelated { get; set; }
    public static bool AutoGenerateLoadOrder { get; set; }
    public static bool ApplyDiscountAfterTaxes { get; set; }

    public static bool ExtendedPaymentOptions { get; set; }

    public static bool GroupRelatedWhenPrinting { get; set; }

    public static bool BetaFeatures { get; set; }

    public static bool PrintLotPreOrder { get; set; }

    public static bool PrintLotOrder { get; set; }

    public static bool CanVoidFOrders { get; set; }

    public static bool HideTotalOrder { get; set; }

    public static bool HideTotalInPrintedLine { get; set; }

    public static bool PrintCopy { get; set; }

    public static bool Consignment { get; set; }

    public static bool MustEndOfDayDaily { get; set; }

    public static bool ForceEODWhenDateChanges { get; set; }

    public static bool UsesTerms { get; set; }

    public static bool UseReship { get; set; }

    public static bool UseClockInOut { get; set; }

    public static int OldPrinter { get; set; }

    public static bool UpdateWhenEndDay { get; set; }

    public static bool AutoAcceptLoad { get; set; }
    public static bool RemoveWarnings { get; set; }

    public static bool GroupLinesWhenPrinting { get; set; }

    public static bool ShortInventorySettlement { get; set; }

    public static bool PrintReportsRequired { get; set; }

    public static bool LoadOrderEmpty { get; set; }

    public static bool AcceptLoadPrintRequired { get; set; }

    public static bool PreSale { get; set; }

    public static bool ProductCatalog { get; set; }

    public static bool LeftOrderTemplateEmpty { get; set; }

    public static bool EmptyTruckAtEndOfDay { get; set; }

    public static bool LoadRequest { get; set; }

    public static bool LoadRequired { get; set; }

    public static bool PickCompany { get; set; }

    public static bool Delivery { get; set; }

    public static bool SendLogByEmail { get; set; }

    public static long LastPrintedId { get; set; }

    public static long LastPresalePrintedId { get; set; }

    public static bool EmailOrder { get; set; }

    public static bool MustPrintPreOrder { get; set; }

    public static bool CanIncreasePrice { get; set; }

    public static bool FakePreOrder { get; set; }

    public static bool PrintTruncateNames { get; set; }

    public static bool PopulateTemplateAuthProd { get; set; }

    public static bool OneDoc { get; set; }

    public static bool CanAddClient { get; set; }

    public static bool SetPO { get; set; }

    public static string InventoryRequestEmail { get; set; }

    public static bool CanGoBelow0 { get; set; }

    public static bool NoPriceChangeDeliveries { get; set; }

    public static bool WarningDumpReturn { get; set; }

    public static bool UseOrderId { get; set; }

    public static int Round { get; set; }

    public static bool DexAvailable { get; set; }

    public static bool PrintUPC { get; set; }

    public static bool CanChangeSalesmanId { get; set; }

    public static string BottomOrderPrintText { get; set; }

    public static bool SingleScanStroke { get; set; }

    public static string PrintInvoiceSort { get; set; }

    public static string PrintClientSort { get; set; }

    public static bool MustSendOrdersFirst { get; set; }

    public static bool AllowDiscount { get; set; }

    public static bool LocationIsMandatory { get; set; }

    public static bool AlwaysAddItemsToOrder { get; set; }

    public static bool FreeItemsNeedComments { get; set; }

    public static bool LotIsMandatory { get; set; }

    public static bool PaymentAvailable { get; set; }
    public static bool DisablePaymentIfTermDaysMoreThan0 { get; set; }

    public static bool PaymentRequired { get; set; }

    public static string PrinterToUse { get; set; }

    public static string InvoiceIdProvider { get; set; }

    public static bool SignatureRequired { get; set; }

    public static bool UseLot { get; set; }

    public static bool FakeUseLot { get; set; }

    public static bool DisplayPurchasePrice { get; set; }

    public static bool AnyPriceIsAcceptable { get; set; }

    public static bool UseLSP { get; set; }

    public static int DaysToKeepOrder { get; set; }

    public static string TransferPassword { get; set; }

    public static string InventoryPassword { get; set; }

    public static string AddInventoryPassword { get; set; }

    public static bool TrackInventory { get; set; }

    public static bool CanModifyInventory { get; set; }

    public static bool PrinterAvailable { get; set; }

    public static bool PrintingRequired { get; set; }

    public static bool AllowCreditOrders { get; set; }

    public static int SalesmanId { get; set; }

    public static int BranchSiteId { get; set; }
    public static int SiteId { get; set; }
    public static bool CalculateInvForEmptyTruck { get; private set; }
    public static bool MustGenerateProjection { get; private set; }
    public static int LaceupVersion { get; set; }
    public static bool LoadByOrderHistory { get; set; }
    public static bool EmptyEndingInventory { get; private set; }
    public static bool BonsuisseCustomization { get; private set; }
    public static bool NewSyncLoadOnDemand { get; set; }
    public static bool UseBiggerUoMInLoadHistory { get; private set; }
    public static bool TotalsByUoMInPdf { get; private set; }
    public static bool SplitDeliveryByDepartment { get; private set; }
    public static bool PrintNoServiceInSalesReports { get; private set; }
    public static string XlsxProvider { get; set; }
    public static int SelfServiceInvitation { get; set; }
    public static bool CarolinaCustomization { get; private set; }
    public static bool IncludePresaleInSalesReport { get; private set; }
    public static bool SelectSalesRepForInvoice { get; private set; }
    public static bool AssetTracking { get; private set; }
    public static bool DeliveryEditable { get; private set; }
    public static bool DisablePrintEndOfDayReport { get; private set; }
    public static bool HideWarehouseOHInLoad { get; private set; }
    public static int PONumberMaxLength { get; private set; }
    public static bool UseLotExpiration { get; private set; }
    public static bool ItemGroupedTemplate { get; private set; }
    public static bool WarehouseInventoryOnDemand { get; private set; }
    public static bool AssetStaysMandatory { get; private set; }
    public static bool AddRelatedItemInCredit { get; private set; }
    public static bool UseOffersInCredit { get; private set; }
    public static bool UseLaceupAdvancedCatalog { get; private set; }
    public static bool SelfServiceUser { get; private set; }
    public static bool UseClientSort { get; set; }
    public static bool UseDisolSurvey { get; private set; }
    public static string DisolSurveyProducts { get; private set; }
    public static bool ShowListPriceInAddItem { get; private set; }
    public static bool CanChangeUomInLoad { get; private set; }
    public static bool IracarCustomization { get; private set; }
    public static bool NewAddItemRandomWeight { get; private set; }
    public static bool SetShipDate { get; private set; }
    public static bool LockOrderAfterPrinted { get; private set; }
    public static bool PackageInReturnPresale { get; private set; }
    public static string RouteName { get; set; }
    public static bool DisolCrap { get; set; }

    public static string VendorName { get; set; }

    public static int Port { get; set; }

    public static string LanAddress { get; set; }

    public static bool HideOHinSelfService { get; set; }

    public static string SSID { get; set; }

    public static bool MustUpdateDaily { get; set; }

    public static bool AllowFreeItems { get; set; }

    public static bool SendLoadOrder { get; set; }

    public static bool UseLocation { get; set; }

    public static bool AllowOrderForClientOverCreditLimit { get; set; }

    public static bool UserCanChangePrices { get; set; }

    public static int ScannerToUse { get; set; }

    public static bool UseUpcCheckDigit { get; set; }

    public static bool RouteManagement { get; private set; }

    public static bool SetParLevel { get; set; }

    public static string VersatileDEXVersion { get; set; }

    public static bool dataDownloaded = false;

    public static string ConnectionAddress
    {
        get { return WhichAddressToUse(); }
    }

    static string iPAddressGateway;

    public static string IPAddressGateway
    {
        get { return iPAddressGateway; }
        set
        {
            iPAddressGateway = value;
            IsTest = false; // string.Compare (IPAddressGateway, "laceup.dyndns.org", StringComparison.OrdinalIgnoreCase) == 0;
        }
    }

    public static string Version
    {
        get { return VersionTracking.CurrentVersion; }
    }

    static public string OrderDatePrintFormat { get; set; }
    static public string InvoiceCopyDatePrintFormat { get; set; }

    static public DateTime CurrentOrderDate { get; set; }
    static public DateTime CurrentBatchDate { get; set; }

    static public int CurrentOrderId { get; set; }
    static public int CurrentBatchId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating if the invoices has invoice details.
    /// </summary>
    /// <value><c>true</c> if has open invoice details; otherwise, <c>false</c>.</value>
    public static bool HasOpenInvoiceDetails { get; set; }

    /// <summary>
    /// If the remote address is laceup test
    /// </summary>
    /// <value><c>true</c> if is test; otherwise, <c>false</c>.</value>
    public static bool IsTest { get; set; }

    public static string CompanyLogo { get; set; }
    public static int CompanyLogoWidth { get; set; }
    public static int CompanyLogoHeight { get; set; }
    public static int CompanyLogoSize { get; set; }

    public static bool UseUpc128 { get; set; }
    public static bool DeliveryScanning { get; set; }
    public static bool UseBattery { get; set; }
    public static bool NewConsPrinter { get; set; }
    public static bool PrintClientOpenBalance { get; set; }
    public static bool CoreAsCredit { get; set; }
    public static bool PresaleShipDate { get; set; }
    public static bool ShipDateIsMandatory { get; set; }
    public static bool ShipDateIsMandatoryForLoad { get; set; }
    public static bool UseOldEmailFormat { get; set; }
    public static int PrintedIdLength { get; set; }
    public static bool HideSetConsignment { get; set; }
    public static bool AutoGeneratePO { get; set; }
    public static int ExtraSpaceForSignature { get; set; }
    public static string DefaultCreditDetType { get; set; }
    public static bool CanSelectSalesman { get; set; }
    public static bool UsePairLotQty { get; set; }
    public static bool UseSendByEmail { get; set; }
    public static bool UsePrintProofDelivery { get; set; }
    public static bool HideCompanyInfoPrint { get; set; }
    public static bool PrintIsMandatory { get; set; }
    public static bool SelectDriverFromPresale { get; set; }
    public static bool MustCompleteRoute { get; set; }
    public static string Discount100PercentPrintText { get; set; }
    public static bool RemovePayBalFomInvoice { get; set; }
    public static bool AddRelatedItemsInTotal { get; set; }
    public static bool SignatureNameRequired { get; set; }
    public static bool PrintZeroesOnPickSheet { get; set; }
    public static bool AddCoresInSalesItem { get; set; }
    public static bool UseAllowance { get; set; }
    public static bool IncludeDeliveriesInLoadOrder { get; set; }
    public static bool UseReturnInvoice { get; set; }
    public static bool AddItemInDefaultUoM { get; set; }
    public static bool UseTermsInLoadOrder { get; set; }
    public static bool POIsMandatory { get; set; }
    public static bool NewClientCanChangePrices { get; set; }
    public static bool PrintNetQty { get; set; }
    public static bool AuthProdsInCredit { get; set; }
    public static bool MagnoliaSetConsignment { get; set; }
    public static bool SyncLoadOnDemand { get; set; }
    public static bool MasterLoadOrder { get; set; }
    public static bool SalesRegReportWithTax { get; set; }
    public static bool TransferComment { get; set; }
    public static bool AddCoreBalance { get; set; }
    public static bool HideItemComment { get; set; }
    public static bool SendByEmailInFinalize { get; set; }
    public static bool HideInvoiceComment { get; set; }
    public static bool IncludeBatteryInLoad { get; set; }
    public static bool ClientDailyPL { get; set; }
    public static int BackgroundTime { get; set; }
    public static bool ShowAvgInCatalog { get; set; }
    public static bool DisableRouteReturn { get; set; }
    public static bool AllowAdjPastExpDate { get; set; }
    public static string ConsignmentContractText { get; set; }
    public static bool SendBackgroundOrders { get; set; }
    public static bool HidePriceInPrintedLine { get; set; }
    public static bool SendOrderIsMandatory { get; set; }
    public static bool PaymentOrSignatureRequired { get; set; }
    public static bool DoNotShrinkOrderImage { get; set; }
    public static bool MustEnterCaseInOut { get; set; }
    public static int ParLevelHistoryDays { get; set; }
    public static bool AddSalesInConsignment { get; set; }
    public static bool CanChangeUoM { get; set; }
    public static bool UseClientClassAsCompanyName { get; set; }
    public static string PdfProvider { get; set; }
    public static bool NewClientEmailRequired { get; set; }
    public static bool UseFullConsignment { get; private set; }
    public static string NewClientExtraFields { get; set; }
    public static string RTN { get; set; }
    public static bool BillNumRequired { get; set; }
    public static float DefaultTaxRate { get; set; }
    public static bool UseLastUoM { get; set; }
    public static bool ShowAddrInClientList { get; set; }
    public static bool SalesmanInCreditDel { get; set; }
    public static int ClientNameMaxSize { get; set; }
    public static int MinShipDateDays { get; set; }
    public static bool HidePrintedCommentLine { get; set; }
    public static bool UseDraggableTemplate { get; set; }
    public static bool PrintBillShipDate { get; set; }
    public static int DaysToKeepSignatures { get; set; }
    public static bool BlackStoneConsigCustom { get; set; }
    public static bool HideProdOnHand { get; set; }
    public static bool PrintInvSettReport { get; set; }
    public static bool PreSaleConsigment { get; set; }
    public static bool UseConsignmentLot { get; set; }
    public static bool SendBackgroundBackup { get; set; }
    public static float MinimumWeight { get; set; }
    public static float MinimumAmount { get; set; }
    public static bool PresaleCommMandatory { get; set; }
    public static bool AllowDiscountPerLine { get; set; }
    public static string PrintTaxLabel { get; set; }
    public static bool HideVoidButton { get; set; }
    public static bool ConsLotAsDate { get; set; }
    public static bool DisolCustomIdGenerator { get; set; }
    public static bool ParInConsignment { get; set; }
    public static bool AddCreditInConsignment { get; set; }
    public static bool ShowShipVia { get; set; }
    public static bool ShipViaMandatory { get; set; }
    public static bool GeneratePreorderNum { get; set; }
    public static bool OnlyKitInCredit { get; set; }
    public static bool DeliveryReasonInLine { get; set; }
    public static bool CanModifyConnectSett { get; set; }
    public static bool LspInAllLines { get; set; }
    public static bool UseAllDayParLevel { get; set; }
    public static bool CloseRouteInPresale { get; set; }
    public static bool EditParInHistory { get; set; }
    public static bool AlwaysCountInPar { get; set; }
    public static bool WarrantyPerClient { get; set; }
    public static bool ChargeBatteryRotation { get; set; }
    public static bool IncludeRotationInDelivery { get; set; }
    public static bool SignedInSelfService { get; set; }
    public static string SavedBanks { get; set; }
    public static bool DidCloseAlert { get; set; }
    public static bool ConsParFirstInPresale { get; set; }
    public static bool AverageSaleInParLevel { get; set; }
    public static bool MultipleLoadOnDemand { get; set; }
    public static bool KeepPresaleOrders { get; set; }
    public static bool ScanDeliveryChecking { get; set; }
    public static bool GeneratePresaleNumber { get; set; }
    public static bool AllowMultParInvoices { get; set; }
    public static bool SettReportInSalesUoM { get; set; }
    public static bool IncludeCredInNewParCalc { get; set; }
    public static bool DontAllowDecimalsInQty { get; set; }
    public static bool SendBackgroundPayments { get; set; }
    public static bool NewClientCanHaveDiscount { get; set; }
    public static bool CaptureImages { get; set; }
    public static int ClientRtnNeededForQty { get; set; }
    public static bool SelectReshipDate { get; set; }
    public static string RouteReturnPassword { get; set; }
    public static bool UseQuote { get; set; }
    public static int MinimumAvailableNumbers { get; set; }
    public static bool AdvanceSequencyNum { get; set; }

    static bool salesPriceInSetting = false;
    static bool creditsPriceInSetting = false;

    public static int ProductCatalogViewType { get; set; }
    public static bool SignedIn { get; set; }
    public static bool ShouldGetPinBeforeSync { get; set; }
    public static bool ButlerSignedIn { get; set; }
    public static int SupervisorId { get; set; }
    public static bool DollyReminder { get; set; }
    public static bool GoToMain { get; set; }
    public static bool UseFutureRouteEx { get; set; }
    public static bool AutoCalculateRouteReturn { get; set; }
    public static bool UseSurvey { get; set; }
    public static bool HideOpenInvoiceTotal { get; private set; }
    public static bool SalesHistoryForCredits { get; set; }
    public static bool UseCreditAccount { get; private set; }
    public static bool HideTransactionsTotal { get; private set; }
    public static bool HidePaymentsTotal { get; private set; }
    public static bool HideSubTotalOrder { get; private set; }
    public static bool SalesByDepartment { get; private set; }
    public static bool CreditReasonInLine { get; private set; }
    public static bool AcceptLoadEditable { get; private set; }
    public static bool ShowProductsPerUoM { get; private set; }
    public static bool EnableCycleCount { get; private set; }
    public static string ViewPrintInvPassword { get; private set; }
    public static string CycleCountPassword { get; private set; }
    public static bool UserCanChangePricesSales { get; set; }
    public static bool UserCanChangePricesCredits { get; set; }
    public static bool HideTransfers { get; set; }
    public static bool MustCompleteInDeliveryChecking { get; set; }
    public static bool ShowAllProductsInCredits { get; set; }
    public static bool DeleteWeightItemsMenu { get; set; }
    public static bool HidePresaleOptions { get; set; }
    public static bool ShowDiscountByPriceLevel { get; set; }
    public static bool AllowQtyConversionFactor { get; set; }
    public static bool DontDeleteEmptyDeliveries { get; set; }
    public static bool ViewGoals { get; set; }
    public static bool EcoSkyWaterCustomEmail { get; set; }
    public static bool KeepAppUpdated { get; set; }
    public static bool HideSalesOrders { get; set; }
    public static bool AllowReset { get; set; }
    public static bool HideClearData { get; set; }
    public static bool MustEnterPostedDate { get; set; }
    public static bool NeedAccessForConfiguration { get; set; }
    public static bool PreviewOfferPriceInAddItem { get; set; }
    public static bool DeleteZeroItemsOnDelivery { get; set; }
    public static bool SalesmanCanChangeSite { get; set; }
    public static bool MustSetWeightInDelivery { get; set; }
    public static bool PrintCreditReport { get; set; }
    public static bool CheckAvailableBeforeSending { get; set; }
    public static bool SAPOrderStatusReport { get; set; }
    public static bool PresaleUseInventorySite { get; set; }
    public static bool CannotOrderWithUnpaidInvoices { get; set; }
    public static bool CanPayMoreThanOwned { get; set; }
    public static bool ShowAllEmailsAsDestination { get; set; }
    public static bool SelectPriceFromPrevInvoices { get; set; }
    public static bool UsePallets { get; set; }
    public static bool HideTaxesTotalPrint { get; set; }
    public static bool HideDiscountTotalPrint { get; set; }
    public static bool CanModifyWeightsOnDeliveries { get; set; }
    public static bool ShowPricesInInventorySummary { get; set; }
    public static bool AlertOrderWasNotSent { get; set; }
    public static bool RequestVehicleInformation { get; set; }
    public static bool ShowListPriceInAdvancedCatalog { get; set; }
    public static bool ShowCostInTemplate { get; set; }
    public static bool RecalculateOrdersAfterSync { get; set; }
    public static bool ShowLowestPriceInTemplate { get; set; }
    public static bool ShowWeightOnInventorySummary { get; set; }
    public static bool ShowLowestPriceLevel { get; set; }
    public static bool MustCreatePaymentDeposit { get; set; }
    public static bool HideSelectSitesFromMenu { get; set; }
    public static bool MustSelectRouteToSync { get; set; }
    public static bool PrintExternalInvoiceAsOrder { get; set; }
    public static bool UseProductionForPayments { get; set; }
    public static bool UseCatalogWithFullTemplate { get; set; }
    public static bool ShowLastThreeVisitsOnTemplate { get; set; }
    public static bool BlockMultipleCollectPaymets { get; set; }
    public static bool ForceSingleScan { get; set; }
    public static bool EnableUsernameandPassword { get; set; }
    public static bool SavePaymentsByInvoiceNumber { get; set; }
    public static bool SendPaymentsInEOD { get; set; }
    public static bool ShowExpensesInEOD { get; set; }
    public static bool DontCalculateOffersAfterPriceChanged { get; set; }
    public static bool RequireCodeForVoidInvoices { get; set; }
    public static bool ShowPaymentSummary { get; set; }
    public static bool CoolerCoCustomization { get; set; }
    public static bool CanChangeRoutesOrder { get; set; }
    public static bool CalculateOffersAutomatically { get; set; }
    public static bool CalculateTaxPerLine { get; set; }
    public static bool AddAllowanceToPriceDuringDEX { get; set; }
    public static bool DontIncludePackageParameterDexUpc { get; set; }
    public static bool OnlyShowCostInProductDetails { get; set; }
    public static int DexUpcCharacterLimits { get; set; }
    public static bool DonNovoCustomization { get; set; }
    public static bool UseVisitsTemplateInSales { get; set; }
    public static bool AllowWorkOrder { get; set; }
    public static bool NotifyNewerDataInOS { get; set; }
    public static bool AmericanEagleCustomization { get; set; }
    public static bool ShowSuggestedButton { get; set; }
    public static bool DisableSendCatalogWithPrices { get; set; }
    public static bool RecalculateRoutesOnSyncData { get; set; }
    public static bool CanSelectTermsOnCreateClient { get; set; }
    public static bool AlertPrintPaymentBeforeSaving { get; set; }
    public static bool CatalogReturnsInDefaultUOM { get; set; }
    public static bool UseReturnOrder { get; set; }
    public static bool IncludeCreditInvoiceForPayments { get; set; }
    public static bool MarmiaCustomization { get; set; }
    public static bool NotificationsInSelfService { get; set; }
    public static bool CanDepositChecksWithDifDates { get; set; }
    public static bool TemplateSearchByContains { get; set; }
    public static bool UseLaceupDataInSalesReport { get; set; }
    public static bool ShowDescriptionInSelfServiceCatalog { get; set; }
    public static bool CanChangeFinalizedInvoices { get; set; }
    public static bool AllowNotifications { get; set; }
    public static bool ShowLowestAcceptableOnWarning { get; set; }

    public static bool AskOffersBeforeAdding { get; set; }

    public static bool ShowPromoCheckbox { get; set; }
    public static bool ShowImageInCatalog { get; set; }
    public static string SoutoBottomEmailText { get; set; }
    public static bool SelectWarehouseForSales { get; set; }
    public static bool CheckInventoryInLoad { get; set; }
    public static bool CustomerInCreditHold { get; set; }
    public static bool DeliveryMustScanProducts { get; set; }
    public static bool CanModifiyDeliveryWithScanning { get; set; }
    public static bool ShowSentTransactions { get; set; }
    public static bool MustSelectDepartment { get; set; }
    public static bool EnableAdvancedLogin { get; set; }
    public static bool UseFullTemplate { get; set; }
    public static bool GenerateProjection { get; set; }
    public static bool PrintClientTotalOpenBalance { get; set; }
    public static bool HidePONumber { get; set; }
    public static bool HidePriceInTransaction { get; set; }
    public static bool OnlyPresale { get; set; }
    public static bool LotIsMandatoryBeforeFinalize { get; set; }
    public static bool CheckDueInvoicesInCreateOrder { get; set; }
    public static bool ConsignmentPresaleOnly { get; set; }
    public static bool TruckTemperatureReq { get; set; }
    public static bool MasterDevice { get; set; }
    public static bool ConsignmentBeta { get; set; }
    public static bool SearchAllProductsInTemplate { get; set; }
    public static bool AdvancedTemplateFocusSearch { get; set; }
    public static bool MustSelectReasonForFreeItem { get; set; }
    public static bool CanEditCreditsInDelivery { get; set; }
    public static bool CanChangeSalesmanName { get; set; }
    public static bool PriceLevelComment { get; set; }
    public static bool GetUOMSOnCommand { get; set; }
    public static bool EnterWeightInCredits { get; set; }
    public static bool ShowVisitsInfoInClients { get; set; }
    public static bool UpdateInventoryInPresale { get; set; }
    public static bool SendTempPaymentsInBackground { get; set; }

    public static bool DontGenerateLoadPrintedId { get; set; }
    public static bool ShowBelow0InAdvancedTemplate { get; set; }
    public static bool RoundTaxPerLine { get; set; }

    public static bool AllowExchange { get; set; }
    public static bool UsePaymentDiscount { get; set; }
    public static bool OffersAddComment { get; set; }
    public static bool SendZplOrder { get; set; }
    public static bool MultiplyConversionByCostInIracarAddItem { get; set; }
    public static bool IncludeAvgWeightInCatalogPrice { get; set; }
    public static bool ShowOldReportsRegardless { get; set; }
    public static bool CanLogout { get; set; }
    public static bool DontRoundInUI { get; set; }
    public static bool MustScanInTransfer { get; set; }
    public static bool TimeSheetAutomaticClockIn { get; set; }
    public static bool DontSortCompaniesByName { get; set; }
    public static bool PrintAllInventoriesInInvSummary { get; set; }
    public static bool DicosaCustomization { get; set; }
    public static bool CanModifyEnteredWeight { get; set; }
    public static bool ShowServiceReport { get; set; }
    public static bool RetailerAllowanceWins { get; set; }
    public static bool MilagroCustomization { get; set; }
    public static bool AllowOtherCharges { get; set; }
    public static int AutomaticClockOutTime { get; set; }
    public static int ForceBreakInMinutes { get; set; }
    public static int OtherChargesType { get; set; }
    public static int FreightType { get; set; }
    public static string OtherChargesComments { get; set; }
    public static string FreightComments { get; set; }
    public static float OtherChargesVale { get; set; }
    public static float FreightVale { get; set; }
    public static int MandatoryBreakDuration { get; set; }
    public static bool ShowRetailPriceForAddItem { get; set; }
    public static bool CheckIfShipdateLocked { get; set; }
    public static bool BetaConfigurationView { get; set; }
    public static bool HiddenItemCustomization { get; set; }
    public static int CheckDueInvoicesQtyInCreateOrder { get; set; }
    public static bool UseBigFontForPrintDate { get; set; }
    public static bool EnableLogin { get; set; }
    public static bool ZeroSoldInConsignment { get; set; }

    public static bool SalesmanSeqValues { get; set; }
    public static string SalesmanSeqPrefix { get; set; }
    public static DateTime SalesmanSeqExpirationDate { get; set; }
    public static int SalesmanSeqFrom { get; set; }
    public static int SalesmanSeqTo { get; set; }
    public static int SalesmanSelectedSite { get; set; }
    public static bool ScanBasedTrading { get; set; }
    public static bool SelfService { get; set; }
    public static bool BetaFragments { get; set; }
    public static bool UseCatalog { get; set; }

    public static string DeviceId { get; set; }

    public static bool AuthorizationFailed
    {
        get { return Preferences.Get("AuthorizationFailedKey", false); }
        set { Preferences.Set("AuthorizationFailedKey", value); }
    }

    public static bool ApplicationIsInDemoMode
    {
        get { return Config.Port == 9999 && Config.IPAddressGateway.ToLowerInvariant() == "app.laceupsolutions.com"; }
    }

    public static bool AcceptedTermsAndConditions
    {
        get { return Preferences.Get("AcceptedTermsAndConditionsKey", false); }
        set { Preferences.Set("AcceptedTermsAndConditionsKey", value); }
    }

    public static DateTime DayClockIn
    {
        get { return DateTime.FromBinary(Preferences.Get("DayClockInKey", 0)); }
        set { Preferences.Set("DayClockInKey", value.Ticks); }
    }

    public static DateTime FirstDayClockIn
    {
        get { return DateTime.FromBinary(Preferences.Get("FirstDayClockInKey", 0)); }
        set { Preferences.Set("FirstDayClockInKey", value.Ticks); }
    }

    public static DateTime DayClockOut
    {
        get { return DateTime.FromBinary(Preferences.Get("DayClockOutKey", 0)); }
        set { Preferences.Set("DayClockOutKey", value.Ticks); }
    }
}