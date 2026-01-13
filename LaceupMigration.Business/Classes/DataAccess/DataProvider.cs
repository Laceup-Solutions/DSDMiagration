using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaceupMigration
{
    public static class DataProvider
    {
        static IDataAccess dataAccessInstance = null;

        #region Initialization and Setup

        public static void Initialize()
        {
            if (dataAccessInstance == null)
            {
                var version = NetAccess.GetCommunicatorVersion();

                dataAccessInstance = new DataAccessLegacy();
                dataAccessInstance.Initialize();

                //if (version != null && version > new Version("80.0.0.0"))
                //{
                //    dataAccessInstance = new DataAccess();
                //}
                //else
                //    dataAccessInstance = new DataAccess();
            }
        }

        public static void GetUserSettingLine()
        {
            dataAccessInstance.GetUserSettingLine();
        }

        public static void GetSalesmanSettings(bool fromDownload = true)
        {
            dataAccessInstance.GetSalesmanSettings(fromDownload);
        }

        public static void GetSalesmanList()
        {
            dataAccessInstance.GetSalesmanList();
        }

        public static string GetFieldForLogin()
        {
            return dataAccessInstance.GetFieldForLogin();
        }

        public static void ClearData()
        {
            dataAccessInstance.ClearData();
        }

        #endregion

        #region Authorization and Validation

        public static void CheckAuthorization()
        {
            dataAccessInstance.CheckAuthorization();
        }

        public static bool CheckSyncAuthInfo()
        {
            return dataAccessInstance.CheckSyncAuthInfo();
        }

        public static bool MustEndOfDay()
        {
            return dataAccessInstance.MustEndOfDay();
        }

        public static bool CanUseApplication()
        {
            return dataAccessInstance.CanUseApplication();
        }

        #endregion

        #region Data Download/Sync

        public static string DownloadData(bool getDeliveries, bool updateInventory)
        {
            return dataAccessInstance.DownloadData(getDeliveries, updateInventory);
        }

        public static string DownloadStaticData()
        {
            return dataAccessInstance.DownloadStaticData();
        }

        public static string DownloadProducts()
        {
            return dataAccessInstance.DownloadProducts();
        }

        public static bool GetPendingLoadOrders(DateTime date, bool getAll = false)
        {
            return dataAccessInstance.GetPendingLoadOrders(date, getAll);
        }

        public static string GetClientImages(int clientId)
        {
            return dataAccessInstance.GetClientImages(clientId);
        }

        public static string GetExternalInvoiceImages(string invoiceNumber)
        {
            return dataAccessInstance.GetExternalInvoiceImages(invoiceNumber);
        }

        public static void AddDeliveryClient(Client client)
        {
            dataAccessInstance.AddDeliveryClient(client);
        }

        public static DriverRoute GetRouteForDriverShipDate(int driverId, DateTime date)
        {
            return dataAccessInstance.GetRouteForDriverShipDate(driverId, date);
        }
        #endregion

        #region Data Loading

        public static void GetGoalProgress()
        {
            dataAccessInstance.GetGoalProgress();
        }

        public static void GetGoalProgressDetail()
        {
            dataAccessInstance.GetGoalProgressDetail();
        }

        public static void EnsureInvoicesAreLoadedForClient(Client client)
        {
            dataAccessInstance.EnsureInvoicesAreLoadedForClient(client);
        }

        public static void GetExcelFile(string source, string destination)
        {
            dataAccessInstance.GetExcelFile(source, destination);
        }

        public static void LoadDeliveriesInSite(string file)
        {
            dataAccessInstance.LoadDeliveriesInSite(file);
        }

        public static void DeletePengingLoads()
        {
            dataAccessInstance.DeletePengingLoads();
        }

        #endregion

        #region Data Sending/Upload

        public static void SendTheOrders(string fileName)
        {
            dataAccessInstance.SendTheOrders(fileName);
        }

        public static void SendTheOrders(IEnumerable<Batch> source, List<string> ordersId = null, bool deleteOrders = true, bool sendPayment = false)
        {
            dataAccessInstance.SendTheOrders(source, ordersId, deleteOrders, sendPayment);
        }

        public static void SendLoadOrder()
        {
            dataAccessInstance.SendLoadOrder();
        }

        public static void SendTheSignatures(string file)
        {
            dataAccessInstance.SendTheSignatures(file);
        }

        public static void SendAll()
        {
            dataAccessInstance.SendAll();
        }

        public static bool SendClientLocation(Client client)
        {
            return dataAccessInstance.SendClientLocation(client);
        }

        public static void SendSelfServiceInvitation(int clientId, string name, string email, string phone)
        {
            dataAccessInstance.SendSelfServiceInvitation(clientId, name, email, phone);
        }

        public static void UpdateClientNote(Client client)
        {
            dataAccessInstance.UpdateClientNote(client);
        }

        public static bool SendCurrentSession(string file)
        {
            return dataAccessInstance.SendCurrentSession(file);
        }

        public static void SendInvoicePaymentsBySource(List<InvoicePayment> source, bool delete = false, bool inBackground = false)
        {
            dataAccessInstance.SendInvoicePaymentsBySource(source, delete, inBackground);
        }

        public static void UpdateProductImagesMap()
        {
            dataAccessInstance.UpdateProductImagesMap();
        }

        public static void SendClientPictures(string tempPathFile, int clientId)
        {
            dataAccessInstance.SendClientPictures(tempPathFile, clientId);
        }

        public static void AcceptLoadOrders(Order order, string valuesChanged)
        {
            dataAccessInstance.AcceptLoadOrders(order, valuesChanged);
        }

        public static void AcceptLoadOrders(List<int> ids, string valuesChanged)
        {
            dataAccessInstance.AcceptLoadOrders(ids, valuesChanged);
        }

        public static void SendClientDepartments(bool delete = true)
        {
            dataAccessInstance.SendClientDepartments(delete);
        }

        public static void SendScannerToUse()
        {
            dataAccessInstance.SendScannerToUse();
        }

        public static void ExportData(string subject = "")
        {
            dataAccessInstance.ExportData(subject);
        }

        public static void SendEmailSequenceNotification(string text)
        {
            dataAccessInstance.SendEmailSequenceNotification(text);
        }
        public static void SaveRoute(string filename)
        {
            dataAccessInstance.SaveRoute(filename);
        }
        public static void SendTransfer(string transferFile)
        {
            dataAccessInstance.SendTransfer(transferFile);
        }
        #endregion

        #region Inventory Operations

        public static void UpdateInventoryBySite()
        {
            dataAccessInstance.UpdateInventoryBySite();
        }

        public static void UpdateInventoryBySite(int siteId)
        {
            dataAccessInstance.UpdateInventoryBySite(siteId);
        }

        public static void GetInventoryInBackground(bool isPresale = false)
        {
            dataAccessInstance.GetInventoryInBackground(isPresale);
        }

        public static List<InventorySettlementRow> ExtendedSendTheLeftOverInventory(bool fromSend = false, bool fromInventorySummary = false)
        {
            return dataAccessInstance.ExtendedSendTheLeftOverInventory(fromSend, fromInventorySummary);
        }

        public static void UpdateInventory()
        {
            dataAccessInstance.UpdateInventory();
        }

        public static void UpdateInventory(bool isPresale = false)
        {
            dataAccessInstance.UpdateInventory(isPresale);
        }
        #endregion

        #region Validation and Checks

        public static bool CheckIfShipdateIsValid(List<DateTime> shipDates, ref List<DateTime> lockedDates)
        {
            return dataAccessInstance.CheckIfShipdateIsValid(shipDates, ref lockedDates);
        }

        public static string CheckOrderChangesBeforeSaveRoute(List<DriverRouteOrder> orders)
        {
            return dataAccessInstance.CheckOrderChangesBeforeSaveRoute(orders);
        }

        #endregion

        #region Reports and Catalog

        public static bool GetCatalogPdf(int priceLevelId, bool printPrice, bool printUpc, bool printUom, List<int> categories)
        {
            return dataAccessInstance.GetCatalogPdf(priceLevelId, printPrice, printUpc, printUom, categories);
        }

        public static string GetCommissionReport(string command)
        {
            return dataAccessInstance.GetCommissionReport(command);
        }

        public static string GetLoadOrderReport(string command)
        {
            return dataAccessInstance.GetLoadOrderReport(command);
        }

        public static string GetQtyProdSalesReport(string command)
        {
            return dataAccessInstance.GetQtyProdSalesReport(command);
        }

        public static string GetSalesmenCommReport(string command)
        {
            return dataAccessInstance.GetSalesmenCommReport(command);
        }
        public static string GetSalesProdCatReport(string command)
        {
            return dataAccessInstance.GetSalesProdCatReport(command);
        }

        public static string GetSalesReportWithDetails(string command)
        {
            return dataAccessInstance.GetSalesReportWithDetails(command);
        }

        public static string GetSalesReport(string command)
        {
            return dataAccessInstance.GetSalesReport(command);
        }

        public static string GetSAPReport(string command)
        {
            return dataAccessInstance.GetSAPReport(command);
        }

        public static string GetTransmissionReport(string command)
        {
            return dataAccessInstance.GetTransmissionReport(command);
        }
        public static string GetPaymentsReport(string command)
        {
            return dataAccessInstance.GetPaymentsReport(command);
        }
        #endregion

        #region Notifications

        public static string GetTopic()
        {
            return dataAccessInstance.GetTopic();
        }

        public static void Unsubscribe()
        {
            dataAccessInstance.Unsubscribe();
        }

        #endregion
    }
}
