using System;
using System.Collections.Generic;

namespace LaceupMigration
{
    /// <summary>
    /// Interface for data access operations used by ViewModels
    /// </summary>
    public interface IDataAccess
    {
        #region Initialization and Setup

        void Initialize();
        void GetUserSettingLine();
        void GetSalesmanSettings(bool fromDownload = true);
        void GetSalesmanList();
        string GetFieldForLogin();
        void ClearData();

        #endregion

        #region Authorization and Validation

        void CheckAuthorization();
        bool CheckSyncAuthInfo();
        bool MustEndOfDay();
        bool CanUseApplication();

        #endregion

        #region Data Download/Sync

        string DownloadData(bool getDeliveries, bool updateInventory);
        string DownloadStaticData();
        string DownloadProducts();
        bool GetPendingLoadOrders(DateTime date, bool getAll = false);
        string GetClientImages(int clientId);
        string GetExternalInvoiceImages(string invoiceNumber);
        void GetExternalInvoiceSignature(Invoice invoice);
        string GetInvoiceDetails(int invoiceId, int clientId);
        /// <summary>Fetch inactive/missing products from backend so invoice details can resolve product names.</summary>
        void GetInactiveProducts(List<int> productIds);
        void AddDeliveryClient(Client client);
        DriverRoute GetRouteForDriverShipDate(int driverId, DateTime date);
        #endregion

        #region Data Loading

        void GetGoalProgress();
        void GetGoalProgressDetail();
        void EnsureInvoicesAreLoadedForClient(Client client);
        void GetExcelFile(string source, string destination);
        void LoadDeliveriesInSite(string file);
        void DeletePengingLoads();
        #endregion

        #region Data Sending/Upload

        void SendLoadOrder();
        void SendTheOrders(string fileName);
        void SendTheOrders(IEnumerable<Batch> source, List<string> ordersId = null, bool deleteOrders = true, bool sendPayment = false);
        void SendTheSignatures(string file);
        /// <summary>Resend a payment package file (zipped) to server via InvoicesAR.</summary>
        void SendThePayments(string fileName);
        void SendAll();
        bool SendClientLocation(Client client);
        void SendSelfServiceInvitation(int clientId, string name, string email, string phone);
        void UpdateClientNote(Client client);
        bool SendCurrentSession(string file);
        void SendInvoicePaymentsBySource(List<InvoicePayment> source, bool delete = false, bool inBackground = false);
        void UpdateProductImagesMap();
        void SendClientPictures(string tempPathFile, int clientId);
        void AcceptLoadOrders(Order order, string valuesChanged);
        void AcceptLoadOrders(List<int> ids, string valuesChanged);
        void SendClientDepartments(bool delete = true);
        void SendScannerToUse();
        void ExportData(string subject = "");
        void SendEmailSequenceNotification(string text);
        void SaveRoute(string filename);
        void SendTransfer(string transferFile);
        void SendDeposit();
        /// <summary>EOD step: send invoice payments (for resumable End of Day).</summary>
        void SendInvoicePayments(Dictionary<string, double> ordersTotals);
        /// <summary>EOD step: send left-over inventory (for resumable End of Day).</summary>
        void SendTheLeftOverInventory(List<InventorySettlementRow> extendedMap);
        /// <summary>EOD step: send day report (for resumable End of Day).</summary>
        void SendDayReport(string sessionId);
        /// <summary>EOD step: send par level (for resumable End of Day).</summary>
        void SendParLevel();
        /// <summary>EOD step: send transfers (for resumable End of Day).</summary>
        void SendTransfers();
        /// <summary>EOD step: send build-to-qty (for resumable End of Day).</summary>
        void SendBuildToQty();
        /// <summary>EOD step: send daily par level (for resumable End of Day).</summary>
        void SendDailyParLevel();
        /// <summary>EOD step: send Butler transfers (for resumable End of Day).</summary>
        void SendButlerTransfers();
        /// <summary>EOD step: send client product sort (for resumable End of Day).</summary>
        void SendClientProdSort();
        void DeleteTransferFiles();
        #endregion

        #region Inventory Operations

        void UpdateInventoryBySite();
        void UpdateInventoryBySite(int siteId);
        void GetInventoryInBackground(bool isPresale = false);
        /// <summary>Runs inventory fetch synchronously (for presale update flow). Call from background thread.</summary>
        void RunInventorySync(bool forSite, bool isPresale);
        List<InventorySettlementRow> ExtendedSendTheLeftOverInventory(bool fromSend = false, bool fromInventorySummary = false);
        void UpdateInventory();
        void UpdateInventory(bool isPresale = false);
        #endregion

        #region Validation and Checks

        bool CheckIfShipdateIsValid(List<DateTime> shipDates, ref List<DateTime> lockedDates);

        string CheckOrderChangesBeforeSaveRoute(List<DriverRouteOrder> orders);

        string GetQtyProdSalesReport(string command);
        string GetSalesmenCommReport(string command);
        string GetSalesProdCatReport(string command);
        string GetSalesReportWithDetails(string command);
        string GetSalesReport(string command);
        string GetSAPReport(string command);
        string GetTransmissionReport(string command);
        string GetPaymentsReport(string command);
        #endregion

        #region Reports and Catalog

        bool GetCatalogPdf(int priceLevelId, bool printPrice, bool printUpc, bool printUom, List<int> categories);

        string GetCommissionReport(string command);

        string GetLoadOrderReport(string command);

        #endregion

        #region Notifications

        string GetTopic();
        void Unsubscribe();

        #endregion

    }
}

