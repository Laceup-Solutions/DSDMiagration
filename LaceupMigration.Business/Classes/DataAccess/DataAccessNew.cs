using ICSharpCode.SharpZipLib.Zip;
using iText.Kernel.Pdf.Canvas.Parser.ClipperLib;
using Microsoft.Maui.Controls;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;

namespace LaceupMigration
{
    public class DataAccessNew : IDataAccess, IDisposable
    {
        #region Initialization and Setup

        public void Initialize()
        {
            using (var da = new DataAccessLegacy())
            {
                da.Initialize();
            }
        }
        public void GetUserSettingLine()
        {
            using (var da = new DataAccessLegacy())
            {
                da.GetUserSettingLine();
            }
        }

        public void GetSalesmanSettings(bool fromDownload = true)
        {
            using (var da = new DataAccessLegacy())
            {
                da.GetSalesmanSettings(fromDownload);
            }
        }
        public void GetSalesmanList()
        {
            using (var da = new DataAccessLegacy())
            {
                da.GetSalesmanList();
            }
        }
        public void GetLoginType()
        {
            try
            {
                using (NetAccess netaccess = new NetAccess())
                {
                    netaccess.OpenConnection();
                    netaccess.WriteStringToNetwork("HELO");
                    netaccess.WriteStringToNetwork(Config.GetAuthString());

                    netaccess.WriteStringToNetwork("GetFieldForLoginCommand");

                    var field = Convert.ToInt32(netaccess.ReadStringFromNetwork());

                    netaccess.WriteStringToNetwork("Goodbye");
                    netaccess.CloseConnection();

                    Config.LoginType = (LoginType)field;
                    Config.SaveAppStatus();
                }
            }
            catch (Exception e)
            {
                Logger.CreateLog(e);
                Config.LoginType = LoginType.UsernamePassword;
                Config.SaveAppStatus();
            }
        }
        public void ClearData()
        {
            using (var da = new DataAccessLegacy())
            {
                da.ClearData();
            }
        }

        #endregion

        #region Authorization and Validation

        public void CheckAuthorization()
        {
            using (var da = new DataAccessLegacy())
            {
                da.CheckAuthorization();
            }
        }
        public bool CheckSyncAuthInfo()
        {
            using (var da = new DataAccessLegacy())
            {
                return da.CheckSyncAuthInfo();
            }
        }
        public bool MustEndOfDay()
        {
            using (var da = new DataAccessLegacy())
            {
                return da.MustEndOfDay();
            }
        }
        public bool CanUseApplication()
        {
            using (var da = new DataAccessLegacy())
            {
                return da.CanUseApplication();
            }
        }

        #endregion

        #region Data Download/Sync

        public string DownloadData(bool getDeliveries, bool updateInventory)
        {
            using (var da = new DataAccessLegacy())
            {
                return da.DownloadData(getDeliveries, updateInventory);
            }
        }
        public string DownloadStaticData()
        {
            using (var da = new DataAccessLegacy())
            {
                return da.DownloadStaticData();
            }
        }
        public string DownloadProducts()
        {
            using (var da = new DataAccessLegacy())
            {
                return da.DownloadProducts();
            }
        }
        public bool GetPendingLoadOrders(DateTime date, bool getAll = false)
        {
            using (var da = new DataAccessLegacy())
            {
                return da.GetPendingLoadOrders(date, getAll);
            }
        }
        public string GetClientImages(int clientId)
        {
            using (var da = new DataAccessLegacy())
            {
                return da.GetClientImages(clientId);
            }
        }
        public void AddDeliveryClient(Client client)
        {
            using (var da = new DataAccessLegacy())
            {
                da.AddDeliveryClient(client);
            }
        }
        public DriverRoute GetRouteForDriverShipDate(int driverId, DateTime date)
        {
            using (var da = new DataAccessLegacy())
            {
                return da.GetRouteForDriverShipDate(driverId, date);
            }
        }
        public string GetSalesmanIdFromLogin(string login1, string login2)
        {
            try
            {
                Logger.CreateLog("getting salesmanId from login");
                using (NetAccess netaccess = new NetAccess())
                {
                    netaccess.OpenConnection();
                    netaccess.WriteStringToNetwork("HELO");
                    netaccess.WriteStringToNetwork(Config.GetAuthString());

                    // Get the orders
                    netaccess.WriteStringToNetwork("GetSalesmanIdFromLoginCommand");
                    netaccess.WriteStringToNetwork(login1 + "|" + login2);

                    var result = netaccess.ReadStringFromNetwork();

                    if (result == "routeNotFound")
                        return "No matching route.";

                    if (result == "salesmanNotFound")
                        return "The selected route has no salesman assigned.";

                    if (result == "invalidTruck")
                        return "The selected truck has orders in the route that are not yet finalized.";

                    Config.SalesmanId = Convert.ToInt32(netaccess.ReadStringFromNetwork());

                    //Close the connection and disconnect
                    netaccess.WriteStringToNetwork("Goodbye");
                    netaccess.CloseConnection();

                    return "";
                }
            }
            catch (Exception e)
            {
                Logger.CreateLog(e);
                return "Error processing request.";
            }
        }
        public List<SalesmanTruckDTO> GetSalesmanTrucks()
        {
            try
            {
                using (var access = new NetAccess())
                {
                    access.OpenConnection();
                    access.WriteStringToNetwork("HELO");
                    access.WriteStringToNetwork(Config.GetAuthString());

                    access.WriteStringToNetwork("GetSalesmanTrucksCommand");

                    var ack = access.ReadStringFromNetwork();

                    if (ack == "error")
                    {
                        access.CloseConnection();
                        throw new Exception("Error processing request");
                    }

                    string file = Path.GetTempFileName();

                    access.ReceiveFile(file);

                    access.CloseConnection();

                    using (StreamReader reader = new StreamReader(file))
                    {
                        return JsonConvert.DeserializeObject<List<SalesmanTruckDTO>>(reader.ReadToEnd());
                    }
                }
            }
            catch
            {
                throw;
            }
        }
        #endregion

        #region Data Loading

        public void GetGoalProgress()
        {
            using (var da = new DataAccessLegacy())
            {
                da.GetGoalProgress();
            }
        }
        public void GetGoalProgressDetail()
        {
            using (var da = new DataAccessLegacy())
            {
                da.GetGoalProgressDetail();
            }
        }
        public void EnsureInvoicesAreLoadedForClient(Client client)
        {
            using (var da = new DataAccessLegacy())
            {
                da.EnsureInvoicesAreLoadedForClient(client);
            }
        }
        public void GetExcelFile(string source, string destination)
        {
            using (var da = new DataAccessLegacy())
            {
                da.GetExcelFile(source, destination);
            }
        }
        public void LoadDeliveriesInSite(string file)
        {
            using (var da = new DataAccessLegacy())
            {
                da.LoadDeliveriesInSite(file);
            }
        }
        public void DeletePengingLoads()
        {
            using (var da = new DataAccessLegacy())
            {
                da.DeletePengingLoads();
            }
        }
        #endregion

        #region Data Sending/Upload

        public void SendTheOrders(string fileName)
        {
            using (var da = new DataAccessLegacy())
            {
                da.SendTheOrders(fileName);
            }
        }
        public void SendTheOrders(IEnumerable<Batch> source, List<string> ordersId = null, bool deleteOrders = true, bool sendPayment = false)
        {
            using (var da = new DataAccessLegacy())
            {
                da.SendTheOrders(source, ordersId, deleteOrders, sendPayment);
            }
        }
        public void SendTheSignatures(string file)
        {
            using (var da = new DataAccessLegacy())
            {
                da.SendTheSignatures(file);
            }
        }
        public void SendAll()
        {
            using (var da = new DataAccessLegacy())
            {
                da.SendAll();
            }
        }
        public bool SendClientLocation(Client client)
        {
            using (var da = new DataAccessLegacy())
            {
                return da.SendClientLocation(client);
            }
        }
        public void SendSelfServiceInvitation(int clientId, string name, string email, string phone)
        {
            using (var da = new DataAccessLegacy())
            {
                da.SendSelfServiceInvitation(clientId, name, email, phone);
            }
        }
        public void UpdateClientNote(Client client)
        {
            using (var da = new DataAccessLegacy())
            {
                da.UpdateClientNote(client);
            }
        }
        public bool SendCurrentSession(string file)
        {
            using (var da = new DataAccessLegacy())
            {
                return da.SendCurrentSession(file);
            }
        }
        public void SendInvoicePaymentsBySource(List<InvoicePayment> source, bool delete = false, bool inBackground = false)
        {
            using (var da = new DataAccessLegacy())
            {
                da.SendInvoicePaymentsBySource(source, delete, inBackground);
            }
        }
        public void UpdateProductImagesMap()
        {
            using (var da = new DataAccessLegacy())
            {
                da.UpdateProductImagesMap();
            }
        }
        public void SendClientPictures(string tempPathFile, int clientId)
        {
            using (var da = new DataAccessLegacy())
            {
                da.SendClientPictures(tempPathFile, clientId);
            }
        }
        public void AcceptLoadOrders(Order order, string valuesChanged)
        {
            using (var da = new DataAccessLegacy())
            {
                da.AcceptLoadOrders(order, valuesChanged);
            }
        }
        public void AcceptLoadOrders(List<int> ids, string valuesChanged)
        {
            using (var da = new DataAccessLegacy())
            {
                da.AcceptLoadOrders(ids, valuesChanged);
            }
        }
        public void SendClientDepartments(bool delete = true)
        {
            using (var da = new DataAccessLegacy())
            {
                da.SendClientDepartments(delete);
            }
        }
        public void SendScannerToUse()
        {
            using (var da = new DataAccessLegacy())
            {
                da.SendScannerToUse();
            }
        }
        public void ExportData(string subject = "")
        {
            using (var da = new DataAccessLegacy())
            {
                da.ExportData(subject);
            }
        }
        public void SendEmailSequenceNotification(string text)
        {
            using (var da = new DataAccessLegacy())
            {
                da.SendEmailSequenceNotification(text);
            }
        }
        public void SaveRoute(string filename)
        {
            using (var da = new DataAccessLegacy())
            {
                da.SaveRoute(filename);
            }
        }
        public void SendTransfer(string transferFile)
        {
            using (var da = new DataAccessLegacy())
            {
                da.SendTransfer(transferFile);
            }
        }
        #endregion

        #region Inventory Operations

        public void UpdateInventoryBySite()
        {
            using (var da = new DataAccessLegacy())
            {
                da.UpdateInventoryBySite();
            }
        }
        public void UpdateInventoryBySite(int siteId)
        {
            using (var da = new DataAccessLegacy())
            {
                da.UpdateInventoryBySite(siteId);
            }
        }
        public void GetInventoryInBackground(bool isPresale = false)
        {
            using (var da = new DataAccessLegacy())
            {
                da.GetInventoryInBackground(isPresale);
            }
        }
        public List<InventorySettlementRow> ExtendedSendTheLeftOverInventory(bool fromSend = false, bool fromInventorySummary = false)
        {
            using (var da = new DataAccessLegacy())
            {
                return da.ExtendedSendTheLeftOverInventory(fromSend, fromInventorySummary);
            }
        }
        public void UpdateInventory()
        {
            using (var da = new DataAccessLegacy())
            {
                da.UpdateInventory();
            }
        }
        public void UpdateInventory(bool isPresale = false)
        {
            using (var da = new DataAccessLegacy())
            {
                da.UpdateInventory(isPresale);
            }
        }
        #endregion

        #region Validation and Checks

        public bool CheckIfShipdateIsValid(List<DateTime> shipDates, ref List<DateTime> lockedDates)
        {
            using (var da = new DataAccessLegacy())
            {
                return da.CheckIfShipdateIsValid(shipDates, ref lockedDates);
            }
        }

        public string CheckOrderChangesBeforeSaveRoute(List<DriverRouteOrder> orders)
        {
            using (var da = new DataAccessLegacy())
            {
                return da.CheckOrderChangesBeforeSaveRoute(orders);
            }
        }

        public string GetQtyProdSalesReport(string command)
        {
            using (var da = new DataAccessLegacy())
            {
                return da.GetQtyProdSalesReport(command);
            }
        }
        public string GetSalesmenCommReport(string command)
        {
            using (var da = new DataAccessLegacy())
            {
                return da.GetSalesmenCommReport(command);
            }
        }
        public string GetSalesProdCatReport(string command)
        {
            using (var da = new DataAccessLegacy())
            {
                return da.GetSalesProdCatReport(command);
            }
        }
        public string GetSalesReportWithDetails(string command)
        {
            using (var da = new DataAccessLegacy())
            {
                return da.GetSalesReportWithDetails(command);
            }
        }
        public string GetSalesReport(string command)
        {
            using (var da = new DataAccessLegacy())
            {
                return da.GetSalesReport(command);
            }
        }
        public string GetSAPReport(string command)
        {
            using (var da = new DataAccessLegacy())
            {
                return da.GetSAPReport(command);
            }
        }
        public string GetTransmissionReport(string command)
        {
            using (var da = new DataAccessLegacy())
            {
                return da.GetTransmissionReport(command);
            }
        }
        public string GetPaymentsReport(string command)
        {
            using (var da = new DataAccessLegacy())
            {
                return da.GetPaymentsReport(command);
            }
        }
        #endregion

        #region Reports and Catalog

        public bool GetCatalogPdf(int priceLevelId, bool printPrice, bool printUpc, bool printUom, List<int> categories)
        {
            using (var da = new DataAccessLegacy())
            {
                return da.GetCatalogPdf(priceLevelId, printPrice, printUpc, printUom, categories);
            }
        }

        public string GetCommissionReport(string command)
        {
            using (var da = new DataAccessLegacy())
            {
                return da.GetCommissionReport(command);
            }
        }

        public string GetLoadOrderReport(string command)
        {
            using (var da = new DataAccessLegacy())
            {
                return da.GetLoadOrderReport(command);
            }
        }

        #endregion

        #region Notifications

        public string GetTopic()
        {
            using (var da = new DataAccessLegacy())
            {
                return da.GetTopic();
            }
        }
        public void Unsubscribe()
        {
            using (var da = new DataAccessLegacy())
            {
                da.Unsubscribe();
            }
        }

        #endregion


        public void Dispose()
        {

        }

    }
}
