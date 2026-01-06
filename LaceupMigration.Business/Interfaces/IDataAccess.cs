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

        void SendTheOrders(string fileName);
        void SendTheOrders(IEnumerable<Batch> source, List<string> ordersId = null, bool deleteOrders = true, bool sendPayment = false);
        void SendTheSignatures(string file);
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
        #endregion

        #region Inventory Operations

        void UpdateInventoryBySite();
        void UpdateInventoryBySite(int siteId);
        void GetInventoryInBackground(bool isPresale = false);
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

