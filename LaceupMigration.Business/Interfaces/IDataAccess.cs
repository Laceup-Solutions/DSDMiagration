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

        #endregion

        #region Data Loading

        void GetGoalProgress();
        void GetGoalProgressDetail();

        #endregion

        #region Data Sending/Upload

        void SendTheOrders(string fileName);
        void SendTheOrders(IEnumerable<Batch> source, List<string> ordersId = null, bool deleteOrders = true, bool sendPayment = false);
        void SendTheSignatures(string file);
        void SendAll();
        bool SendClientLocation(Client client);
        void SendSelfServiceInvitation(int clientId, string name, string email, string phone);
        void UpdateClientNote(Client client);

        #endregion

        #region Inventory Operations

        void SaveInventory();
        void UpdateInventoryBySite();
        void UpdateInventoryBySite(int siteId);
        void GetInventoryInBackground(bool isPresale = false);
        List<InventorySettlementRow> ExtendedSendTheLeftOverInventory(bool fromSend = false, bool fromInventorySummary = false);

        #endregion

        #region Validation and Checks

        bool CheckIfShipdateIsValid(List<DateTime> shipDates, ref List<DateTime> lockedDates);

        #endregion

        #region Reports and Catalog

        bool GetCatalogPdf(int priceLevelId, bool printPrice, bool printUpc, bool printUom, List<int> categories);

        #endregion

        #region Notifications

        string GetTopic();
        void Unsubscribe();

        #endregion

    }
}

