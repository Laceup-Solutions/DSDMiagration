using Microsoft.Maui.Storage;

namespace LaceupMigration;

public partial class Config
{
    /*
     /base  (BasePath)
           /Images                           (ImageStorePath)
           /DataStatic                       (StaticDataPath) clients.csv & products.csv
           /LaceupData                       (CodeBase)
                      /Orders                (OrderPath)
                      /Data                  (DataPath)
                      /CurrentOrders         (CurrentOrdersPath
                      /SentPaymentPath       (SentPaymentPath
                      /PaymentsData          (PaymentPath
                      /consignmentData       (ConsignmenPath
                      /BatchData             (BatchPath
                      /Invoices             (BatchPath
                                 // ClientId
    */

    private static string orderpath = "Orders";
    private static string datapath = "Data";
    private static string tempOrderspath = "CurrentOrders";
    private static string zplPrintedOrders = "zplPrintedOrders";
    private static string paymentPath = "PaymentsData";
    private static string consignmentPath = "consignmentData";
    private static string batchPath = "BatchData";
    private static string invoicesPath = "InvoicesData";
    private static string sentPaymentPath = "SentPaymentPath";
    private static string sessionPath = "SessionPath";
    private static string clientPictures = "ClientPictures";
    private static string orderStatusPath = "OrdersStatus";
    private static string pdfsPath = "Pdfs";

    public static string LaceupStorage
    {
        get
        {
            var path = Path.Combine(BasePath, pdfsPath);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            return path;
        }
    }

    public static string SiteExPath => Path.Combine(BasePath, "siteEx.cvs");

    public static string ExpensesPath => Path.Combine(BasePath, "expensesPath.cvs");

    public static string InventorySiteExPath => Path.Combine(BasePath, "inventorySiteEx.cvs");

    public static string CatalogPDFPath => Path.Combine(BasePath, "catalogpdf.pdf");

    public static string TrucksStoreFile => Path.Combine(BasePath, "trucks.cvs");

    public static string ZPLOrdersPrintedPath => Path.Combine(CodeBase, zplPrintedOrders);

    public static string TemporalInvoicePayment => Path.Combine(BasePath, "tempInvoicePayment.cvs");

    public static string VehicleInformationPath => Path.Combine(BasePath, "vehicleInformation.cvs");

    public static string EODVehicleInformationPath => Path.Combine(BasePath, "EODvehicleInformation.cvs");

    public static string OrderDiscountTrackingPath => Path.Combine(BasePath, "OrderDiscountTracking.cvs");

    public static string SelfServiceCompany => Path.Combine(BasePath, "selfServiceCompany.cvs");

    public static string BankDepositPath => Path.Combine(BasePath, "bankDeposit.cvs");

    public static string RouteRelationStorePath => Path.Combine(BasePath, "routeRelation.cvs");

    public static string AccessCodePath => Path.Combine(BasePath, "accessCode.cvs");

    public static string LastSyncDate => Path.Combine(BasePath, "lastsyncdate.cvs");

    public static string EdittedRoutes
    {
        get
        {
            var dir = Path.Combine(CodeBase, "EdittedRoutes");

            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            return dir;
        }
    }

    public static string ClientPicturesPath
    {
        get
        {
            var dir = Path.Combine(CodeBase, clientPictures);

            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            return dir;
        }
    }

    // [MIGRATION]: Use MAUI FileSystem.AppDataDirectory instead of Environment.SpecialFolder.Personal
    // This ensures proper file path resolution in .NET MAUI
    public static string BasePath => FileSystem.AppDataDirectory;

    public static string CompanyLogosPath
    {
        get
        {
            if (!Directory.Exists(Path.Combine(BasePath, "Logos")))
                Directory.CreateDirectory(Path.Combine(BasePath, "Logos"));

            return Path.Combine(BasePath, "Logos");
        }
    }

    public static string CompanyLogosSavePath
    {
        get
        {
            if (!Directory.Exists(Path.Combine(BasePath, "SavedLogos")))
                Directory.CreateDirectory(Path.Combine(BasePath, "SavedLogos"));

            return Path.Combine(BasePath, "SavedLogos");
        }
    }

    public static string ImageStorePath => Path.Combine(BasePath, "Images");

    public static string InvoiceImagesTempStorePath => Path.Combine(BasePath, "InvoiceImages");

    public static string OrderImageStorePath => Path.Combine(BasePath, "OrdersImages");

    public static string PaymentImagesPath
    {
        get
        {
            var path = Path.Combine(BasePath, "PaymentImages");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            return path;
        }
    }

    public static string DepositImagesPath
    {
        get
        {
            var path = Path.Combine(BasePath, "DepositImages");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            return path;
        }
    }

    public static string LogoStorePath => Path.Combine(BasePath, "logo.jpg");

    public static string HistoryModificationPath => Path.Combine(BasePath, "HistoryModifications");

    public static string StaticDataPath => Path.Combine(BasePath, "DataStatic");

    public static string CodeBase
    {
        get
        {
            var codeBase = Path.Combine(BasePath, "LaceupData");
            // Ensure the directory exists to prevent path errors
            if (!Directory.Exists(codeBase))
            {
                try
                {
                    Directory.CreateDirectory(codeBase);
                }
                catch
                {
                    // If creation fails, still return the path
                }
            }
            return codeBase;
        }
    }

    public static string OrderPath => Path.Combine(CodeBase, orderpath);

    public static string OrderStatusPath => Path.Combine(BasePath, "orderStatus.cvs");

    public static string TermsPath => Path.Combine(BasePath, "terms.cvs");

    public static string SapStatusPath => Path.Combine(BasePath, "sapStatuses.cvs");

    public static string DataPath
    {
        get
        {
            var codeBase = CodeBase;
            var dataPath = Path.Combine(codeBase, datapath);
            // Ensure the directory exists to prevent path errors
            if (!Directory.Exists(dataPath))
            {
                try
                {
                    Directory.CreateDirectory(dataPath);
                }
                catch
                {
                    // If creation fails, still return the path
                }
            }
            return dataPath;
        }
    }

    public static string CurrentOrdersPath => Path.Combine(CodeBase, tempOrderspath);

    public static string QuotesPath => Path.Combine(BasePath, "quotesPath.cvs");

    public static string GoalsPath => Path.Combine(BasePath, "goalsPath.cvs");

    public static string GoalProgressPath => Path.Combine(BasePath, "goalsProgressPath.cvs");

    public static string PaymentPath => Path.Combine(CodeBase, paymentPath);

    public static string ConsignmentPath => Path.Combine(CodeBase, consignmentPath);

    public static string BatchPath => Path.Combine(CodeBase, batchPath);

    public static string InvoicesOldPath => Path.Combine(CodeBase, invoicesPath);

    public static string InvoicesPath => Path.Combine(StaticDataPath, invoicesPath);

    public static string SessionPath => Path.Combine(StaticDataPath, sessionPath);

    public static string SentPaymentPath => Path.Combine(CodeBase, sentPaymentPath);

    public static string ExportImportPath => Path.Combine(BasePath, "LaceUpData.zip");

    public static string ProductImageMappingFile => Path.Combine(ImageStorePath, "map.txt");

    public static string ProductInventoriesFile => Path.Combine(DataPath, "productInventories.xml");

    public static string AssetTrackingHistoriesFile => Path.Combine(DataPath, "assetTrackingHistory.xml");

    public static string AssetTrackingFile => Path.Combine(DataPath, "assetTracking.xml");

    public static string LoadOrderFile => Path.Combine(DataPath, "loadorder.xml");

    public static string CycleCountFile => Path.Combine(DataPath, "cycleCount.xml");

    public static string TransferOnFile => Path.Combine(DataPath, "transferOn.xml");

    public static string TransferOffFile => Path.Combine(DataPath, "transferOff.xml");

    public static string ButlerTransfersOff => Path.Combine(DataPath, "ButlerTransfersOff");

    public static string ButlerTransfersOn => Path.Combine(DataPath, "ButlerTransfersOn");

    public static string ButlerInventorySiteInventories => Path.Combine(DataPath, "InventorySiteInventory.xml");

    public static string ParLevelFile => Path.Combine(DataPath, "parlevel.xml");

    public static string DailyParLevelFile => Path.Combine(DataPath, "dailyparlevel.xml");

    public static string SavedDailyParLevelFile => Path.Combine(DataPath, "savedDailyparlevel.xml");

    public static string ConsignmentParFile => Path.Combine(DataPath, "consignmentpar.xml");

    public static string ProductLotFile => Path.Combine(DataPath, "productLot.xml");

    public static string BuildToQtyFile => Path.Combine(DataPath, "BuildToQtyPath.xml");

    public static string LotsFile => Path.Combine(DataPath, "lots.xml");

    public static string ActivitiesStateFile => Path.Combine(DataPath, "activitystates.xml");

    public static string CurrentOrderFile => Path.Combine(DataPath, "currentordernumber.xml");

    public static string CurrentBatchFile => Path.Combine(DataPath, "currentbatchnumber.xml");

    public static string ProductStoreFile => Path.Combine(StaticDataPath, "products.cvs");

    public static string CurrentInventoryUpdateFile => Path.Combine(DataPath, "inventoryupdate.cvs");

    public static string AcceptInventoryFile => Path.Combine(DataPath, "acceptinventory.cvs");

    public static string CurrentInventoryAddFile => Path.Combine(DataPath, "inventoryadd.cvs");

    public static string CurrentCheckInventoryFile => Path.Combine(DataPath, "CurrentCheckInventoryFile.cvs");

    public static string CompanyInfoStoreFile
    {
        get
        {
            // Ensure DataPath directory exists before returning the file path
            // This prevents "Could not find a part of the path" errors
            var dataPath = DataPath;
            if (!Directory.Exists(dataPath))
            {
                try
                {
                    Directory.CreateDirectory(dataPath);
                }
                catch
                {
                    // If creation fails, still return the path - the caller will handle the error
                }
            }
            return Path.Combine(dataPath, "companies.cvs");
        }
    }

    public static string InventoryStoreFile => Path.Combine(DataPath, "inventory.cvs");

    public static string InventoryOnDemandStoreFile => Path.Combine(DataPath, "inventoryOnDemand.cvs");

    public static string LogFile => Path.Combine(DataPath, "log.txt");

    public static string LogFilePrevious => Path.Combine(DataPath, "log1.txt");

    public static string OpenInvoicesFile => Path.Combine(DataPath, "openinvoices.cvs");

    public static string RouteExFile => Path.Combine(DataPath, "routeex.cvs");

    public static string DeliveryFile => Path.Combine(DataPath, "delivery.cvs");

    public static string ProductLabelFormatPath => Path.Combine(DataPath, "productlabelformatpath.cvs");

    public static string FutureRoutesFile => Path.Combine(DataPath, "futureroute.cvs");

    public static string BanksAccountFile => Path.Combine(DataPath, "banks.cvs");

    public static string ClientStoreFile => Path.Combine(StaticDataPath, "clients.cvs");

    public static string NewClientsStoreFile => Path.Combine(DataPath, "newclients.cvs");

    public static string ClientNotesStoreFile => Path.Combine(DataPath, "clientnotes.cvs");

    public static string OrderStorageFile => Path.Combine(DataPath, "Orders.xml");

    public static string VersionFile => Path.Combine(CodeBase, "versionPath");

    public static string ReasonsFile => Path.Combine(DataPath, "reasons.xml");

    public static string ParLevelHistoryFile => Path.Combine(DataPath, "parLevelHistory.xml");

    public static string LacupConfigFile => Path.Combine(DataPath, "laceup.config");

    public static string SalesmanSessionsFile => Path.Combine(DataPath, "salesmanSessions.cvs");

    public static string ClientProdSortFile => Path.Combine(DataPath, "sortperclient.xml");

    public static string TmpDeliveryClientsFile => Path.Combine(DataPath, "tmpDeliveryClients.xml");

    public static string DeliveryNewClientsFile => Path.Combine(DataPath, "deliveryNewClients.xml");

    public static string SessionIdFile => Path.Combine(DataPath, "sessionId.xml");

    public static string OrderHistoryFile => Path.Combine(DataPath, "orderHistory.xml");

    public static string ProjectionFile => Path.Combine(DataPath, "projectionValues.xml");

    public static string ClientSalesProjectionFile => Path.Combine(DataPath, "clientSalesProjection.xml");

    public static string ClientDepartmentsFile => Path.Combine(DataPath, "clientDepartments.xml");

    public static string UnitOfMeasuresFile => Path.Combine(DataPath, "unitOfMeasures.xml");
}