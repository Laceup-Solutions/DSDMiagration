using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace LaceupMigration
{
    public class Client
    {
        public List<ProductOfferEx> AvailableOffersForListPrice { get; set; }

        public List<Invoice> Invoices = null;
        public void EnsureInvoicesAreLoaded()
        {
            if (Invoices != null)
                return;

            var clientPath = Path.Combine(Config.InvoicesPath, ClientId.ToString());
            if (Directory.Exists(clientPath))
            {
                var finalInvoiceFile = Path.Combine(clientPath, "invoices.xml");
                if (File.Exists(finalInvoiceFile))
                {
                    using (var reader = new StreamReader(finalInvoiceFile))
                    {
                        string currentline;
                        int currenttable = -1;
                        while ((currentline = reader.ReadLine()) != null)
                        {
                            if (currentline == "EndOfTable")
                                currenttable = 1;
                            else
                            {
                                var currentrow = currentline.Split(DataAccess.DataLineSplitter);
                                if (currenttable < 1)
                                {
                                    //DataAccess.CreateInvoice(currentrow);
                                }
                                else
                                {
                                    DataAccess.CreateInvoiceDetails(currentrow);
                                }
                            }
                        }
                    }
                }
            }

            Invoices = Invoice.OpenInvoices.Where(x => x.ClientId == ClientId).ToList();
        }

        IList<Tuple<string, string>> extraProperties;
        IList<Tuple<string, string>> nonVisibleExtraProperties;


        public int CopiesPerInvoice
        {
            get
            {
                int copies = 0;

                if (!string.IsNullOrEmpty(NonvisibleExtraPropertiesAsString) && NonvisibleExtraPropertiesAsString.ToLower().Contains("copiesperinvoice"))
                {
                    var copiesAsString = DataAccess.GetSingleUDF("copiesperinvoice", NonvisibleExtraPropertiesAsString);

                    if (!string.IsNullOrEmpty(copiesAsString))
                    {
                        if (Int32.TryParse(copiesAsString, out copies))
                            return copies;
                    }
                }

                if (!string.IsNullOrEmpty(ExtraPropertiesAsString) && ExtraPropertiesAsString.ToLower().Contains("copiesperinvoice"))
                {
                    var copiesAsString = DataAccess.GetSingleUDF("copiesperinvoice", ExtraPropertiesAsString);

                    if (!string.IsNullOrEmpty(copiesAsString))
                    {
                        if (Int32.TryParse(copiesAsString, out copies))
                            return copies;
                    }
                }

                return copies;
            }
        }

        public string RemitTo
        {
            get
            {
                if (!string.IsNullOrEmpty(NonvisibleExtraPropertiesAsString))
                {
                    var remitTo = DataAccess.GetSingleUDF("remitto", NonvisibleExtraPropertiesAsString);
                    if (!string.IsNullOrEmpty(remitTo))
                    {
                        return remitTo;
                    }
                    else
                        return string.Empty;
                }
                else
                    return string.Empty;
            }
        }

        public double ClientBalanceInDevice
        {
            get
            {
                var invoicesFinalized = Order.Orders.Where(x => x.Client == this && x.Finished == true).ToList();
                var total = invoicesFinalized.Sum(x => x.OrderTotalCost());
                var clientBalance = this.OpenBalance + total;
                var currentPayments = InvoicePayment.List.Where(x => x.Client.ClientId == this.ClientId).ToList();
                var currentPaymentsTotal = currentPayments.Sum(x => x.TotalPaid);
                clientBalance -= currentPaymentsTotal;

                return clientBalance;
            }
        }


        public bool UseBaseUoM
        {
            get
            {
                if (Config.ButlerCustomization)
                    return true;

                if (string.IsNullOrEmpty(NonvisibleExtraPropertiesAsString))
                    return false;

                var extraFields = DataAccess.GetSingleUDF("usebaseunitofmeasurebydefault", NonvisibleExtraPropertiesAsString);
                if (!string.IsNullOrEmpty(extraFields) && extraFields == "1")
                    return true;

                return false;
            }
        }

        private List<int> _splitList;

        public List<int> SplitInvoices
        {
            get
            {
                if (_splitList != null)
                    return _splitList;

                var list = new List<int>();

                if (string.IsNullOrEmpty(ExtraPropertiesAsString))
                    return list;

                var extraFields = DataAccess.GetSingleUDF("split", ExtraPropertiesAsString.ToLower());
                if (!string.IsNullOrEmpty(extraFields))
                {
                    var parts = extraFields.Split(",");

                    foreach (var p in parts)
                    {
                        int splitid = 0;
                        Int32.TryParse(p, out splitid);

                        if (splitid != 0)
                            list.Add(splitid);
                    }
                }

                _splitList = list;

                return list;
            }
        }

        public int TermId { get; set; }
        public int ClientId { get; set; }

        public string ClientName { get; set; }

        public string ShipToAddress { get; set; }

        public string BillToAddress { get; set; }

        public int PriceLevel { get; set; }

        public string Comment { get; set; }

        public string ContactName { get; set; }

        public string ContactPhone { get; set; }

        public double OpenBalance { get; set; }

        public bool OverCreditLimit { get; set; }

        public int CategoryId { get; set; }

        public string OriginalId { get; set; }

        public double TaxRate { get; set; }

        public bool Taxable { get; set; }

        public string DUNS { get; set; }

        public string Location { get; set; }

        public string CommId { get; set; }

        public string UniqueId { get; set; }

        public bool OneDoc { get; set; }

        public string LicenceNumber { get; set; }

        public string VendorNumber { get; set; }

        public string Notes { get; set; }

        public bool NotesChanged { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public double InsertedLatitude { get; set; }
        public double InsertedLongitude { get; set; }

        public bool Editable { get; set; }

        public bool ChangedInfo { get; set; }

        public double? CreditLimit { get; set; }

        public int RetailPriceLevelId { get; set; }

        public bool FromDelivery { get; set; }

        public DateTime StartTimeWindows1 { get; set; }
        public DateTime EndTimeWindows1 { get; set; }

        public DateTime StartTimeWindows2 { get; set; }
        public DateTime EndTimeWindows2 { get; set; }

        public List<Consignment> ConsignmentTemplate { get; set; }

        public IList<Offer> AvailableOffers { get; set; }

        public double MinimumOrderAmount { get; set; }

        public double MinimumOrderQty { get; set; }
        public double MinOrderQty
        {
            get
            {
                var data = DataAccess.GetSingleUDF("MinimumOrderQty", NonvisibleExtraPropertiesAsString);
                if (!string.IsNullOrEmpty(data))
                {
                    return Convert.ToInt32(data);
                }

                data = DataAccess.GetSingleUDF("MinimumOrderQty", ExtraPropertiesAsString);
                if (!string.IsNullOrEmpty(data))
                {
                    return Convert.ToDouble(data);
                }

                return 0;
            }
        }
        public int AreaId { get; set; }

        public bool PrintSkuOverUpc
        {
            get
            {
                if (!string.IsNullOrEmpty(NonvisibleExtraPropertiesAsString))
                {
                    var data = DataAccess.GetSingleUDF("PrintSKUInsteadUPC", NonvisibleExtraPropertiesAsString);
                    if (!string.IsNullOrEmpty(data) && data == "1")
                        return true;
                }

                if (!string.IsNullOrEmpty(ExtraPropertiesAsString))
                {
                    var data = DataAccess.GetSingleUDF("PrintSKUInsteadUPC", ExtraPropertiesAsString);
                    if (!string.IsNullOrEmpty(data) && data == "1")
                        return true;
                }

                return false;
            }
        }

        public bool OnCreditHold
        {
            get
            {
                if (!string.IsNullOrEmpty(NonvisibleExtraPropertiesAsString))
                {
                    var data = DataAccess.GetSingleUDF("CreditHold", NonvisibleExtraPropertiesAsString);
                    if (!string.IsNullOrEmpty(data) && data == "yes")
                        return true;
                }

                if (!string.IsNullOrEmpty(ExtraPropertiesAsString))
                {
                    var data = DataAccess.GetSingleUDF("CreditHold", ExtraPropertiesAsString);
                    if (!string.IsNullOrEmpty(data) && data == "yes")
                        return true;
                }

                return false;
            }
        }

        public IList<Tuple<string, string>> ExtraProperties
        {
            get
            {
                if (extraProperties == null)
                {
                    if (ExtraPropertiesAsString == null || ExtraPropertiesAsString.Length == 0)
                        return extraProperties;
                    extraProperties = new List<Tuple<string, string>>();
                    string extraPropertiesField = ExtraPropertiesAsString;
                    string[] extraPropertiesParts = extraPropertiesField.Split(DataAccess.ExtraPropertiesSeparator);
                    if (extraPropertiesParts.Length > 0)
                    {
                        foreach (string extraProperty in extraPropertiesParts)
                        {
                            string[] extraPropertyDetails = extraProperty.Split(DataAccess.ExtraPropertySeparator);
                            if (extraPropertyDetails.Length == 2)
                                extraProperties.Add(new Tuple<string, string>(extraPropertyDetails[0], extraPropertyDetails[1]));
                        }
                    }
                }
                return extraProperties;
            }
            set
            {
                extraProperties = value;
            }
        }

        public string ExtraPropertiesAsString
        {
            get;
            set;

        }

        public string NonvisibleExtraPropertiesAsString
        {
            get;
            set;

        }

        public IList<Tuple<string, string>> NonVisibleExtraProperties
        {
            get
            {
                if (nonVisibleExtraProperties == null)
                {
                    if (NonvisibleExtraPropertiesAsString == null || NonvisibleExtraPropertiesAsString.Length == 0)
                        return new List<Tuple<string, string>>();
                    nonVisibleExtraProperties = new List<Tuple<string, string>>();
                    string extraPropertiesField = NonvisibleExtraPropertiesAsString;
                    string[] extraPropertiesParts = extraPropertiesField.Split(DataAccess.ExtraPropertiesSeparator);
                    if (extraPropertiesParts.Length > 0)
                    {
                        foreach (string extraProperty in extraPropertiesParts)
                        {
                            string[] extraPropertyDetails = extraProperty.Split(DataAccess.ExtraPropertySeparator);
                            if (extraPropertyDetails.Length == 2)
                                nonVisibleExtraProperties.Add(new Tuple<string, string>(extraPropertyDetails[0], extraPropertyDetails[1]));
                        }
                    }
                }
                return nonVisibleExtraProperties;
            }
        }

        public IList<LastTwoDetails> OrderedList { get; set; }

        public Dictionary<int, LastTwoDetails> OrderedDictionary { get; set; }
        public Dictionary<int, LastTwoDetails> CreditedDictionary { get; set; }

        public Dictionary<int, LastTwoDetails> OrderedDictionaryForAllCustomers { get; set; }

        public Dictionary<Product, List<InvoiceDetail>> ClientProductHistory { get; set; }

        static Dictionary<int, Client> clientList = new Dictionary<int, Client>();

        public static Dictionary<int, Client> AllActiveClients = new Dictionary<int, Client>();

        public static IList<Client> AllClients
        {
            get { return AllActiveClients.Values.ToList(); }
        }

        public static void AddClient(Client client)
        {
            if (clientList.ContainsKey(client.ClientId))
            {
                Logger.CreateLog("Client not added because the id was already in the dictionary. Id:" + client.ClientId);
                return;
            }
            clientList.Add(client.ClientId, client);
        }

        public static IList<Client> Clients
        {
            get { return clientList.Values.ToList(); }
        }

        public static Client Find(int clientId)
        {
            if (clientList.ContainsKey(clientId))
                return clientList[clientId];

            return null;
        }

        public static void Clear()
        {
            clientList.Clear();
        }

        public static int NextAddedId()
        {
            int lastId = 0;
            foreach (var client in Clients)
                if (client.ClientId < lastId)
                    lastId = client.ClientId;

            return lastId;
        }

        public static void Save()
        {
            // save the temp clients
            lock (FileOperationsLocker.lockFilesObject)
            {
                try
                {
                    //FileOperationsLocker.InUse = true;

                    using (StreamWriter writer = new StreamWriter(Config.NewClientsStoreFile, false))
                    {
                        foreach (var client in Clients)
                            if (client.ClientId < 0)
                            {
                                writer.Write(client.ClientName.Replace((char)10, ' ').Replace((char)13, ' '));      //0
                                writer.Write((char)20);
                                writer.Write(client.CategoryId);                                                    //1
                                writer.Write((char)20);
                                writer.Write(client.ShipToAddress.Replace((char)10, ' ').Replace((char)13, ' '));   //2
                                writer.Write((char)20);
                                writer.Write(client.ClientId);                                                      //3
                                writer.Write((char)20);
                                writer.Write(client.ClientName.Replace((char)10, ' ').Replace((char)13, ' '));      //4
                                writer.Write((char)20);
                                writer.Write(client.Comment.Replace((char)10, ' ').Replace((char)13, ' '));         //5
                                writer.Write((char)20);
                                writer.Write(client.CommId.Replace((char)10, ' ').Replace((char)13, ' '));          //6
                                writer.Write((char)20);
                                writer.Write(client.ContactName.Replace((char)10, ' ').Replace((char)13, ' '));     //7
                                writer.Write((char)20);
                                writer.Write(client.ContactPhone.Replace((char)10, ' ').Replace((char)13, ' '));    //8
                                writer.Write((char)20);
                                writer.Write(client.DUNS.Replace((char)10, ' ').Replace((char)13, ' '));            //9
                                writer.Write((char)20);
                                writer.Write(client.Location.Replace((char)10, ' ').Replace((char)13, ' '));        //10
                                writer.Write((char)20);
                                writer.Write(client.OpenBalance);                                                   //11
                                writer.Write((char)20);
                                writer.Write(client.OriginalId.Replace((char)10, ' ').Replace((char)13, ' '));      //12
                                writer.Write((char)20);
                                writer.Write(client.OverCreditLimit ? "1" : "0");                                   //13
                                writer.Write((char)20);
                                writer.Write(client.PriceLevel);                                                    //14
                                writer.Write((char)20);
                                writer.Write(client.UniqueId.Replace((char)10, ' ').Replace((char)13, ' '));        //15
                                writer.Write((char)20);
                                writer.Write(client.TaxRate);                                                       //16
                                writer.Write((char)20);
                                writer.Write(client.Editable ? "1" : "0");                                          //17
                                writer.Write((char)20);
                                writer.Write(client.BillToAddress);                                                 //18
                                writer.Write((char)20);
                                writer.Write(client.LicenceNumber);                                                 //19
                                writer.Write((char)20);
                                writer.Write(client.VendorNumber);                                                  //20
                                writer.Write((char)20);
                                writer.Write(client.Notes);                                                         //21
                                writer.Write((char)20);
                                if (client.CreditLimit.HasValue)
                                    writer.Write(client.CreditLimit.Value);                                         //22
                                else
                                    writer.Write(string.Empty);
                                writer.Write((char)20);
                                writer.Write(client.ExtraPropertiesAsString ?? client.ExtraPropertiesAsString);     //23
                                writer.Write((char)20);
                                writer.Write(client.NonvisibleExtraPropertiesAsString ?? client.NonvisibleExtraPropertiesAsString);//24
                                writer.Write((char)20);
                                writer.Write(client.Taxable ? "1" : "0");                                                       //25
                                writer.Write((char)20);
                                writer.Write(client.InsertedLatitude);                                                       //26
                                writer.Write((char)20);
                                writer.Write(client.InsertedLongitude);                                                       //27

                                writer.Write((char)20);
                                writer.Write("0");                                                       //28
                                writer.Write((char)20);
                                writer.Write("0");                                                       //29
                                writer.Write((char)20);
                                writer.Write(client.TermId);                          //30

                                writer.WriteLine();
                            }
                    }
                }
                finally
                {
                    //FileOperationsLocker.InUse = false;
                }
            }
        }

        public static void SaveNotes()
        {
            // save the temp clients

            lock (FileOperationsLocker.lockFilesObject)
            {
                try
                {
                    //FileOperationsLocker.InUse = true;

                    using (StreamWriter writer = new StreamWriter(Config.ClientNotesStoreFile, false))
                    {
                        foreach (var client in Clients)
                            if (client.NotesChanged)
                            {
                                writer.Write(client.ClientId.ToString(CultureInfo.InvariantCulture));
                                writer.Write((char)20);
                                writer.Write(client.Notes.Replace((char)10, ' ').Replace((char)13, ' '));
                                writer.WriteLine();
                            }
                    }
                }
                finally
                {
                    //FileOperationsLocker.InUse = false;
                }
            }
        }

        public static void LoadNotes()
        {
            try
            {
                // save the temp clients
                if (File.Exists(Config.ClientNotesStoreFile))
                    using (StreamReader writer = new StreamReader(Config.ClientNotesStoreFile, false))
                    {
                        string line;
                        while ((line = writer.ReadLine()) != null)
                        {
                            var parts = line.Split(new char[] { (char)20 });
                            var id = Convert.ToInt32(parts[0], CultureInfo.InvariantCulture);
                            var note = parts[1];
                            var client = Client.Find(id);
                            if (client != null)
                            {
                                client.Notes = note;
                                client.NotesChanged = true;
                            }
                        }
                    }
            }
            catch (Exception ee)
            {
                Logger.CreateLog(ee);
            }
        }

        public static void DeleteNotes()
        {
            if (File.Exists(Config.ClientNotesStoreFile))
                File.Delete(Config.ClientNotesStoreFile);
        }

        public static void DeleteClients()
        {
            if (File.Exists(Config.NewClientsStoreFile))
                File.Delete(Config.NewClientsStoreFile);
        }

        public static void LoadClients()
        {

            // save the temp clients
            if (File.Exists(Config.NewClientsStoreFile))
                using (StreamReader writer = new StreamReader(Config.NewClientsStoreFile, false))
                {
                    string line;
                    while ((line = writer.ReadLine()) != null)
                    {
                        try
                        {
                            var parts = line.Split(new char[] { (char)20 });
                            var client = new Client();

                            client.ClientName = parts[0];
                            client.CategoryId = Convert.ToInt32(parts[1]);
                            client.ShipToAddress = parts[2];
                            client.ClientId = Convert.ToInt32(parts[3]);
                            client.ClientName = parts[4];
                            client.Comment = parts[5];
                            client.CommId = parts[6];
                            client.ContactName = parts[7];
                            client.ContactPhone = parts[8];
                            client.DUNS = parts[9];
                            client.Location = parts[10];
                            client.OpenBalance = Convert.ToDouble(parts[11]);
                            client.OriginalId = parts[12];
                            client.OverCreditLimit = Convert.ToInt32(parts[13]) > 0;
                            client.PriceLevel = Convert.ToInt32(parts[14]);
                            client.UniqueId = parts[15];
                            client.TaxRate = Convert.ToDouble(parts[16]);

                            if (client.ClientId < 0 && parts.Length > 17)
                            {
                                client.Editable = Convert.ToInt32(parts[17]) > 0;
                            }

                            if (parts.Length > 17)
                            {
                                int onedoc = 0;
                                Int32.TryParse(parts[17], out onedoc);
                                client.OneDoc = onedoc > 0;
                            }
                            else
                                client.OneDoc = false;

                            if (parts.Length > 18)
                            {
                                client.BillToAddress = parts[18];
                            }
                            else
                                client.BillToAddress = "";

                            if (parts.Length > 19)
                            {
                                client.LicenceNumber = parts[19];
                            }
                            else
                                client.LicenceNumber = "";

                            if (parts.Length > 20)
                            {
                                client.VendorNumber = parts[20];
                            }
                            else
                                client.VendorNumber = "";

                            if (parts.Length > 21)
                            {
                                client.Notes = parts[21];
                            }
                            else
                                client.Notes = "";

                            if (parts.Length > 22 && !string.IsNullOrEmpty(parts[22]))
                                client.CreditLimit = Convert.ToDouble(parts[22], CultureInfo.InvariantCulture);

                            if (parts.Length > 23)
                                client.ExtraPropertiesAsString = parts[23];

                            if (parts.Length > 24)
                                client.NonvisibleExtraPropertiesAsString = parts[24];

                            if (parts.Length > 25)
                                client.Taxable = Convert.ToInt32(parts[25], CultureInfo.InvariantCulture) > 0;

                            if (parts.Length > 26)
                                client.InsertedLatitude = Convert.ToSingle(parts[26]);

                            if (parts.Length > 27)
                                client.InsertedLongitude = Convert.ToSingle(parts[27]);

                            if (parts.Length > 30)
                                client.TermId = Convert.ToInt32(parts[30]);

                            var originalClient = Clients.FirstOrDefault(x => x.UniqueId == client.UniqueId || x.ClientName == client.ClientName);
                            if (originalClient == null)
                                Client.AddClient(client);
                            else if (originalClient.UniqueId == client.UniqueId)
                            {
                                //fix the order with the real customer
                                foreach (var order in Order.Orders.Where(x => x.Client.ClientId == client.ClientId))
                                {
                                    order.Client = originalClient;
                                    order.Save();
                                }
                                foreach (var batch in Batch.List.Where(x => x.Client.UniqueId == client.UniqueId))
                                {
                                    batch.Client = originalClient;
                                    batch.Save();
                                }
                                foreach (var payment in InvoicePayment.List.Where(x => x.Client.UniqueId == client.UniqueId))
                                {
                                    payment.Client = originalClient;
                                    payment.Save();
                                }
                            }
                        }
                        catch (Exception ee)
                        {
                            Logger.CreateLog("Error Loading New Customers Line=" + line);
                            Logger.CreateLog(ee);
                        }
                    }
                }
        }

        public static IQueryable<Client> SortedClients()
        {
            if (!string.IsNullOrEmpty(Config.SortClient))
            {
                switch (Config.SortClient.ToLower())
                {
                    case "name":
                        return Clients.OrderBy(c => c.ClientName).AsQueryable();
                    case "uniqueid":
                        return Clients.OrderByDescending(x => string.IsNullOrEmpty(x.UniqueId)).ThenBy(c => c.ClientName).AsQueryable();
                    default:
                        return Clients.OrderBy(c => c.ClientName).AsQueryable();
                }
            }
            else
            {
                switch (Config.PrintClientSort.ToLowerInvariant())
                {
                    case "licensenumber":
                        return Clients.OrderBy(x => x.LicenceNumber).ThenBy(x => x.ClientName).AsQueryable();
                    case "vendornumber":
                        return Clients.OrderBy(x => x.LicenceNumber).ThenBy(x => x.ClientName).AsQueryable();
                    case "categoryid":
                        return Clients.OrderBy(x => x.CategoryId).ThenBy(x => x.ClientName).AsQueryable();
                    default:
                        return Clients.OrderBy(x => x.ClientName).AsQueryable();
                }
            }
        }

        public static Client CreateTemporalClient(int id)
        {
            Client client = new Client();
            client.CategoryId = 0;
            client.ShipToAddress = string.Empty;
            client.ClientId = id;
            client.ClientName = "CUSTOMER NOT FOUND";
            client.Comment = string.Empty;
            client.CommId = string.Empty;
            client.ContactName = string.Empty;
            client.ContactPhone = string.Empty;
            client.DUNS = string.Empty;
            client.Location = string.Empty;
            client.OpenBalance = 0;
            client.OriginalId = string.Empty;
            client.OverCreditLimit = false;
            client.UniqueId = Guid.NewGuid().ToString("N");
            client.PriceLevel = 0; // TODO: make sure this works
            client.TaxRate = 0;
            client.Editable = true;
            client.ChangedInfo = false;
            client.CreditLimit = 0;
            client.Editable = false;
            client.ExtraPropertiesAsString = string.Empty;
            client.Latitude = 0;
            client.LicenceNumber = string.Empty;
            client.Location = string.Empty;
            client.Longitude = 0;
            client.InsertedLongitude = 0;
            client.InsertedLatitude = 0;
            client.NonvisibleExtraPropertiesAsString = string.Empty;
            client.Notes = string.Empty;
            client.NotesChanged = false;
            client.OneDoc = false;
            client.VendorNumber = string.Empty;

            AddClient(client);

            return client;
        }

        public static Client CreateSalesmanClient()
        {
            var salesman = Salesman.List.FirstOrDefault(x => x.Id == Config.SalesmanId);

            Client client = new Client();
            client.ClientName = "Salesman: ";

            if (salesman != null)
                client.ClientName += salesman.Name;

            client.CategoryId = 0;
            client.ShipToAddress = string.Empty;
            client.Comment = string.Empty;
            client.CommId = string.Empty;
            client.ContactName = string.Empty;
            client.ContactPhone = string.Empty;
            client.DUNS = string.Empty;
            client.Location = string.Empty;
            client.OpenBalance = 0;
            client.OriginalId = string.Empty;
            client.OverCreditLimit = false;
            client.UniqueId = Guid.NewGuid().ToString("N");
            client.PriceLevel = 0; // TODO: make sure this works
            client.TaxRate = 0;
            client.Editable = true;
            client.ChangedInfo = false;
            client.CreditLimit = 0;
            client.Editable = false;
            client.ExtraPropertiesAsString = string.Empty;
            client.Latitude = 0;
            client.LicenceNumber = string.Empty;
            client.Location = string.Empty;
            client.Longitude = 0;
            client.NonvisibleExtraPropertiesAsString = string.Empty;
            client.Notes = string.Empty;
            client.NotesChanged = false;
            client.OneDoc = false;
            client.VendorNumber = string.Empty;
            client.SalesmanClient = true;

            AddClient(client);

            return client;
        }

        public bool IsOverCreditLimit()
        {
            if (Config.AllowOrderForClientOverCreditLimit || !CreditLimit.HasValue)
                return false;

            //if (OverCreditLimit)
            //    return true;

            var currentBalance = CurrentBalance();
            
            return CreditLimit.Value - currentBalance < 0;
        }

        public double GetOverCreditLimit(OrderDetail detail, double cost, bool asCreditItem, bool asPresale)
        {
            if (Config.AllowOrderForClientOverCreditLimit || asCreditItem || !CreditLimit.HasValue)
                return 0;

            var currentBalance = CurrentBalance();

            if (detail != null)
            {
                var oldCost = detail.Qty * detail.Price;

                if (detail.Product.SoldByWeight)
                {
                    if (asPresale)
                        oldCost *= detail.Product.Weight;
                    else
                        oldCost = detail.Weight * detail.Price;
                }

                cost -= oldCost;
            }

            if (asCreditItem)
                cost *= -1;

            currentBalance += cost;

            return currentBalance - CreditLimit.Value;
        }

        public double CurrentBalance()
        {
            double currentBalance = 0;

            foreach (var order in Order.Orders.Where(x => x.Client.ClientId == ClientId))
            {
                double ordercost = 0;
                if (order.OrderType == OrderType.Order || order.OrderType == OrderType.Credit || order.OrderType == OrderType.Return)
                {
                    ordercost = order.OrderTotalCost();

                    if (order.OrderType == OrderType.Order)
                    {
                        var payment = InvoicePayment.List.FirstOrDefault(x => !string.IsNullOrEmpty(x.OrderId) && x.OrderId.Contains(order.UniqueId));
                        double paid = 0;
                        if (payment != null)
                        {
                            var parts = DataAccess.SplitPayment(payment).Where(x => x.UniqueId == order.UniqueId);
                            paid = parts.Sum(x => x.Amount);
                        }

                        ordercost -= paid;
                    }
                }

                currentBalance += ordercost;
            }

            currentBalance += OpenBalance;

            for (int i = 0; i < Invoice.OpenInvoices.Count; i++)
            {
                var item = Invoice.OpenInvoices[i];

                if (item.ClientId == ClientId && item.Balance != 0)
                {
                    // if (item.InvoiceType == 0)
                    //     currentBalance += item.Balance;
                    // else if (item.InvoiceType == 1)
                    //     currentBalance -= Math.Abs(item.Balance);
                    // else continue;

                    InvoicePayment payment = InvoicePayment.List.FirstOrDefault(x => string.IsNullOrEmpty(x.OrderId) && (x.Invoices().FirstOrDefault(y => y.InvoiceId == item.InvoiceId) != null));
                    if (payment != null)
                    {
                        var paid = payment.Components.Sum(x => x.Amount);
                        currentBalance -= paid;
                    }
                }
            }

            return currentBalance;
        }

        public DateTime LastVisitedDate
        {
            get
            {
                DateTime last = DateTime.MinValue;
                if (InvoiceDetail.Details.Count > 0)
                {
                    for (int i = 0; i < InvoiceDetail.Details.Count; i++)
                    {
                        var item = InvoiceDetail.Details[i];
                        if (item.ClientId == ClientId)
                        {
                            if (item.Date > last)
                                last = item.Date;
                        }
                    }
                }

                return last;
            }
        }

        public bool UseDiscount
        {
            get
            {
                var allow = DataAccess.GetSingleUDF("allowDiscount", ExtraPropertiesAsString);

                if (string.IsNullOrEmpty(allow) && Config.AllowDiscount)
                    return true;

                return allow == "1";
            }
        }

        public bool UseDiscountPerLine
        {
            get
            {
                var allow = DataAccess.GetSingleUDF("allowDiscountPerLine", ExtraPropertiesAsString);

                if (string.IsNullOrEmpty(allow) && Config.AllowDiscountPerLine)
                    return true;

                return allow == "1";
            }
        }

        public string LicenseStatus
        {
            get
            {
                var ls = DataAccess.GetSingleUDF("LicenseStatus", ExtraPropertiesAsString);
                return ls;
            }
        }

        public bool DollyPicked { get { return Batch.List.FirstOrDefault(x => x.Client.ClientId == ClientId && x.DollyPicked) != null; } }

        public bool AsPresale 
        { 
            get 
            { 
                // Check if there's an active presale order for this client
                var presaleOrder = Order.Orders.FirstOrDefault(x => 
                    x.Client.ClientId == ClientId && 
                    x.AsPresale && 
                    !x.Voided && 
                    !x.Finished);
                return presaleOrder != null;
            } 
        }

        public static void Remove(Client client)
        {
            clientList.Remove(client.ClientId);
        }

        public bool AllowOneDoc { get { return OneDoc || Config.OneDoc; } }

        public bool OneOrderPerDepartment { get; set; }

        Dictionary<int, double> averages;
        public double Average(int productId)
        {
            if (averages == null)
            {
                averages = new Dictionary<int, double>();
                var udf = DataAccess.ExplodeExtraProperties(NonvisibleExtraPropertiesAsString).FirstOrDefault(x => x.Key.ToLower() == "saleaverage");
                if (udf != null)
                {
                    var vars = udf.Value.Split(new char[] { ';' });
                    for (int i = 0; i < vars.Length; i++)
                    {
                        var id = vars[i];
                        i++;
                        var q = vars[i];
                        if (!averages.ContainsKey(Convert.ToInt32(id)))
                            averages.Add(Convert.ToInt32(id), Convert.ToDouble(q));
                    }
                }
            }
            double qty = -1;
            averages.TryGetValue(productId, out qty);
            return qty;
        }

        public bool SalesmanClient { get; set; }

        public bool POIsMandatory
        {
            get
            {
                var poIsMandatoryForCustomer = DataAccess.GetSingleUDF("poIsRequired", NonvisibleExtraPropertiesAsString);

                return Config.POIsMandatory || (!string.IsNullOrEmpty(poIsMandatoryForCustomer) && poIsMandatoryForCustomer == "1");
            }
        }

        public string Password
        {
            get
            {
                if (string.IsNullOrEmpty(NonvisibleExtraPropertiesAsString))
                    return string.Empty;

                return DataAccess.GetSingleUDF("password", NonvisibleExtraPropertiesAsString);
            }
        }

        public static string GetCustomerListString()
        {
            string result = string.Empty;
            foreach (var item in Client.Clients)
            {
                if (!string.IsNullOrEmpty(result))
                    result += ',';
                result += item.ClientId;
            }
            return result;
        }

        public void EnsurePreviouslyOrdered()
        {
            if (OrderedList == null)
            {
                var excludedProductsIds = new List<int>();
                var excludedExtraField = NonVisibleExtraProperties.FirstOrDefault(x => x != null && x.Item1.ToLowerInvariant() == "excludeditems");
                if (excludedExtraField != null)
                    foreach (var idAsString in excludedExtraField.Item2.Split(new char[] { ',' }).ToList())
                        excludedProductsIds.Add(Convert.ToInt32(idAsString));
                var lastList = InvoiceDetail.ClientOrderedItemsEx(ClientId);
                OrderedList = lastList.Where(x => !excludedProductsIds.Contains(x.Last.ProductId)).ToList();
            }
        }

        public static IOrderedEnumerable<KeyValuePair<string, List<Client>>> GroupClients(List<Client> clients)
        {
            var groupedClients = new SortedList<string, List<Client>>();

            try
            {
                var config = Config.GroupClientsByCat.Split('_');

                var filter = config[0].ToLowerInvariant();

                string extraField = string.Empty;
                if (config.Length > 1)
                    extraField = config[1].ToLowerInvariant();


                switch (filter)
                {
                    case "extrafield":
                        foreach (var client in clients)
                        {
                            Tuple<string, string> groupName = null;

                            if (client.ExtraProperties != null && client.ExtraProperties.Count > 0)
                                groupName = client.ExtraProperties.FirstOrDefault(x => x.Item1.ToLower() == extraField);

                            List<Client> tempClients = null;
                            if (groupName != null)
                            {
                                groupedClients.TryGetValue(groupName.Item2, out tempClients);

                                if (tempClients == null)
                                {
                                    tempClients = new List<Client>();
                                    tempClients.Add(client);
                                    groupedClients.Add(groupName.Item2, tempClients);
                                }
                                else
                                {
                                    tempClients.Add(client);
                                    groupedClients[groupName.Item2] = tempClients;
                                }
                            }
                            else
                            {
                                groupedClients.TryGetValue("Unassigned", out tempClients);

                                if (tempClients == null)
                                {
                                    tempClients = new List<Client>();
                                    tempClients.Add(client);
                                    groupedClients.Add("Unassigned", tempClients);
                                }
                                else
                                {
                                    tempClients.Add(client);
                                    groupedClients["Unassigned"] = tempClients;
                                }
                            }

                        }
                        break;
                    case "nonvisibleextrafield":
                        foreach (var client in clients)
                        {
                            Tuple<string, string> groupName = null;

                            if (client.NonVisibleExtraProperties != null && client.NonVisibleExtraProperties.Count > 0)
                                groupName = client.NonVisibleExtraProperties.FirstOrDefault(x => x.Item1.ToLower() == extraField);

                            List<Client> tempClients = null;
                            if (groupName != null)
                            {
                                groupedClients.TryGetValue(groupName.Item2, out tempClients);

                                if (tempClients == null)
                                {
                                    tempClients = new List<Client>();
                                    tempClients.Add(client);
                                    groupedClients.Add(groupName.Item2, tempClients);
                                }
                                else
                                {
                                    tempClients.Add(client);
                                    groupedClients[groupName.Item2] = tempClients;
                                }
                            }
                            else
                            {
                                groupedClients.TryGetValue("Unassigned", out tempClients);

                                if (tempClients == null)
                                {
                                    tempClients = new List<Client>();
                                    tempClients.Add(client);
                                    groupedClients.Add("Unassigned", tempClients);
                                }
                                else
                                {
                                    tempClients.Add(client);
                                    groupedClients["Unassigned"] = tempClients;
                                }
                            }

                        }
                        break;
                    case "clientcategory":
                        foreach (var client in clients)
                        {
                            var groupName = client.CategoryId.ToString();

                            List<Client> tempClients = null;
                            if (groupName != null)
                            {
                                groupedClients.TryGetValue(groupName, out tempClients);

                                if (tempClients == null)
                                {
                                    tempClients = new List<Client>();
                                    tempClients.Add(client);
                                    groupedClients.Add(groupName, tempClients);
                                }
                                else
                                {
                                    tempClients.Add(client);
                                    groupedClients[groupName] = tempClients;
                                }
                            }
                            else
                            {
                                groupedClients.TryGetValue("Unassigned", out tempClients);

                                if (tempClients == null)
                                {
                                    tempClients = new List<Client>();
                                    tempClients.Add(client);
                                    groupedClients.Add("Unassigned", tempClients);
                                }
                                else
                                {
                                    tempClients.Add(client);
                                    groupedClients["Unassigned"] = tempClients;
                                }
                            }

                        }
                        break;
                    case "availablecompany":
                        foreach (var client in clients)
                        {
                            var groupName = ClientAvailableCompany.GetCompanyInfoList(client.ClientId);

                            var clientCompany = groupName.FirstOrDefault().CompanyName;

                            List<Client> tempClients = null;
                            if (clientCompany != null)
                            {
                                groupedClients.TryGetValue(clientCompany, out tempClients);

                                if (tempClients == null)
                                {
                                    tempClients = new List<Client>();
                                    tempClients.Add(client);
                                    groupedClients.Add(clientCompany, tempClients);
                                }
                                else
                                {
                                    tempClients.Add(client);
                                    groupedClients[clientCompany] = tempClients;
                                }
                            }
                            else
                            {
                                groupedClients.TryGetValue("Unassigned", out tempClients);

                                if (tempClients == null)
                                {
                                    tempClients = new List<Client>();
                                    tempClients.Add(client);
                                    groupedClients.Add("Unassigned", tempClients);
                                }
                                else
                                {
                                    tempClients.Add(client);
                                    groupedClients["Unassigned"] = tempClients;
                                }
                            }

                        }
                        break;
                }

            }
            catch (Exception ex)
            {
                Logger.CreateLog("Error Grouping Customers => " + ex.ToString());
            }

            IOrderedEnumerable<KeyValuePair<string, List<Client>>> sortedGroups = null;
            if (groupedClients != null && groupedClients.Count > 0)
                sortedGroups = groupedClients.OrderBy(x => x.Key);


            return sortedGroups;
        }

        public bool AllowToCollectPayment
        {
            get
            {
                if (Config.HidePriceInTransaction)
                    return false;
                
                if (Config.PaymentAvailable)
                {
                    if (Config.DisablePaymentIfTermDaysMoreThan0 && TermId > 0)
                    {
                        var term = Term.List.FirstOrDefault(X => X.Id == TermId);

                        if (term != null && term.StandardDueDates > 0)
                            return false;
                    }

                    if (!string.IsNullOrEmpty(Config.EnablePaymentsByTerms) && !string.IsNullOrEmpty(ExtraPropertiesAsString))
                    {
                        var terms = DataAccess.GetSingleUDF("TERMS", ExtraPropertiesAsString);
                        if (!string.IsNullOrEmpty(terms))
                            return terms.ToLower() != Config.EnablePaymentsByTerms.ToLower();
                    }

                    if (!string.IsNullOrEmpty(Config.TrackTermsPaymentBotton) && !string.IsNullOrEmpty(ExtraPropertiesAsString))
                    {
                        var terms = DataAccess.GetSingleUDF("TERMS", ExtraPropertiesAsString);

                        var list = Config.TrackTermsPaymentBotton.Split(",");

                        var found = false;
                        foreach (var term in list)
                        {
                            if (term.ToLowerInvariant() == terms.ToLowerInvariant())
                            {
                                found = true;
                                break;
                            }
                        }

                        return found;
                    }

                    return true;
                }

                return false;
            }
        }


        #region client images
        public List<string> ImageList { get; set; }

        private string ImageListAsString()
        {
            string s = "";

            if (ImageList != null)
            {
                foreach (var item in ImageList)
                {
                    if (!string.IsNullOrEmpty(s))
                        s += "|";
                    s += item;
                }
            }
            return s;

        }

        public void LoadClientImages()
        {
            FileInfo file = null;
            FileInfo[] files = null;

            ImageList = new List<string>();

            var imagesZipFolder = DataAccess.GetClientImages(ClientId);

            if (!string.IsNullOrEmpty(imagesZipFolder))
            {
                string tempPathFolder = Path.Combine(Path.GetTempPath(), "OrdersImages");

                if (Directory.Exists(tempPathFolder))
                    Directory.Delete(tempPathFolder, true);

                List<Tuple<int, string>> imageMap = new List<Tuple<int, string>>();

                System.IO.Compression.ZipFile.ExtractToDirectory(imagesZipFolder, tempPathFolder, true);

                DirectoryInfo dir = new DirectoryInfo(tempPathFolder);

                files = dir.GetFiles();

                file = files.FirstOrDefault(x => x.Name.Contains("ordersImgMap"));

                if (file == null)
                {
                    dir.Delete(true);
                    return;
                }

                using (StreamReader reader = new StreamReader(Path.Combine(tempPathFolder, file.Name)))
                {
                    string line = null;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var parts = line.Split(',');
                        imageMap.Add(new Tuple<int, string>(Convert.ToInt32(parts[0]), parts[1]));
                    }

                }

                foreach (var img in imageMap)
                {
                    ImageList.Add(img.Item2);
                }
            }

            AddLocalImages(files, file);
        }

        private void AddLocalImages(FileInfo[] files1, FileInfo file1)
        {
            //add local images
            List<Tuple<int, string>> imageMap = new List<Tuple<int, string>>();

            DirectoryInfo dir = new DirectoryInfo(Path.Combine(Config.ClientPicturesPath, ClientId.ToString()));

            if (dir.Exists)
                dir.Delete(true);

            Directory.CreateDirectory(System.IO.Path.Combine(Config.ClientPicturesPath, ClientId.ToString()));

            FileInfo[] files = dir.GetFiles();

            FileInfo file = files.FirstOrDefault(x => x.Name.Contains("ordersImgMap"));

            if (file == null)
            {
                if (files1 != null)
                {
                    foreach (var f in files1)
                        f.CopyTo(Path.Combine(Config.ClientPicturesPath, ClientId.ToString()));
                }
            }
        }

        public void SendClientPictures(bool isOnline = true, int clientId = 0)
        {
            try
            {
                List<string> imagesPath = new List<string>();
                List<Tuple<int, string>> mapImages = new List<Tuple<int, string>>();

                foreach (var image in ImageList)
                {
                    mapImages.Add(new Tuple<int, string>(ClientId, image));
                    imagesPath.Add(image);
                }

                var tempPathFile = Path.Combine(Path.GetTempPath(), "ordersImages.zip");

                if (File.Exists(tempPathFile))
                    File.Delete(tempPathFile);

                string tempPathFolder = Path.Combine(Path.GetTempPath(), "OrdersImages");

                if (Directory.Exists(tempPathFolder))
                    Directory.Delete(tempPathFolder, true);

                Directory.CreateDirectory(tempPathFolder);

                DirectoryInfo dir = new DirectoryInfo(Path.Combine(Config.ClientPicturesPath, clientId.ToString()));
                if (!dir.Exists)
                {
                    Logger.CreateLog(Config.ClientPicturesPath + " directory not found");
                }

                // Get the files in the directory and copy them to the new location.
                FileInfo[] files = dir.GetFiles();
                foreach (FileInfo file in files)
                {
                    int index = file.Name.IndexOf(".png");

                    if (index == -1)
                        continue;

                    string formattedString = string.Empty;

                    formattedString = file.Name.Substring(0, index);

                    foreach (var img in imagesPath)
                    {
                        if (img.Contains(formattedString))
                        {
                            string temppath = Path.Combine(tempPathFolder, file.Name);
                            file.CopyTo(temppath, false);
                            break;
                        }
                    }
                }

                string mapFile = Path.Combine(tempPathFolder, "ordersImgMap");

                if (File.Exists(mapFile))
                    File.Delete(mapFile);

                using (var writer = new StreamWriter(mapFile))
                {
                    foreach (var item in mapImages)
                        writer.WriteLine(item.Item1 + "," + item.Item2);
                }

                List<string> FilesArray = new List<string>();
                var dirInfo = new DirectoryInfo(tempPathFolder);
                foreach (var myfile in dirInfo.GetFiles())
                {
                    FilesArray.Add(myfile.FullName);
                }

                try
                {
                    File.Copy(mapFile, Path.Combine(Config.ClientPicturesPath, clientId.ToString(), "ordersImgMap"), true);
                }
                catch (Exception ex) { }

                if (!isOnline)
                    return;

                using (var access = new NetAccess())
                {
                    access.OpenConnection();
                    access.WriteStringToNetwork(Config.GetAuthString());

                    access.CloseConnection();
                }

                try
                {
                    using (ZipArchive zip = System.IO.Compression.ZipFile.Open(tempPathFile, ZipArchiveMode.Create))
                    {
                        foreach (var file in FilesArray)
                        {
                            zip.CreateEntryFromFile(file, Path.GetFileName(file), CompressionLevel.Optimal);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.CreateLog("Exception zipping images" + ex.ToString());
                }

                if (!string.IsNullOrEmpty(tempPathFile) && File.Exists(tempPathFile))
                {
                    NetAccess.SendClientPictures(tempPathFile, ClientId);
                    File.Delete(tempPathFile);
                }

            }
            catch (Exception ex)
            {

            }
        }

        #endregion
    }

    public class ClientProdSort
    {
        class CPS
        {
            public CPS()
            {
                Sorting = new List<int>();
                OriginalSorting = new List<int>();
            }

            public List<int> Sorting { get; set; }

            public List<int> OriginalSorting { get; set; }

            public bool Changed
            {
                get
                {
                    if (OriginalSorting.Count != Sorting.Count)
                        return true;

                    for (int i = 0; i < OriginalSorting.Count; i++)
                    {
                        if (OriginalSorting[i] != Sorting[i])
                            return true;
                    }

                    return false;
                }
            }

            public override string ToString()
            {
                var result = "";

                foreach (var i in Sorting)
                {
                    string s = i.ToString();

                    if (!string.IsNullOrEmpty(result))
                        result += ",";

                    result += s;
                }

                return result;
            }
        }

        static Dictionary<int, CPS> clientsSort = new Dictionary<int, CPS>();

        public static List<int> GetSortForClient(int clientId)
        {
            if (!clientsSort.ContainsKey(clientId))
                return new List<int>();

            return clientsSort[clientId].Sorting;
        }

        public static void ChangeSort(int clientId, List<int> list)
        {
            if (!clientsSort.ContainsKey(clientId))
                clientsSort.Add(clientId, new CPS());

            clientsSort[clientId].Sorting = list;

            Save();
        }

        public static void ChangeSort(int clientId, List<OdLine> list)
        {
            var int_list = list.Select(x => x.Product.ProductId).ToList();

            ChangeSort(clientId, int_list);
        }

        public static void ChangeSort(int clientId, List<DailyParLevelLine> list)
        {
            var int_list = list.Select(x => x.Product.ProductId).ToList();

            ChangeSort(clientId, int_list);
        }

        public static void CreateSort(int clientId, string list)
        {
            if (clientsSort.ContainsKey(clientId))
            {
                var sorting = clientsSort[clientId];

                if (Config.UseDraggableTemplate && sorting.Changed)
                    return;
                else
                    clientsSort.Remove(clientId);
            }

            List<int> prods = new List<int>();

            var idString = list.Split(',');

            foreach (var item in idString)
            {
                if (string.IsNullOrEmpty(item))
                    continue;
                prods.Add(Convert.ToInt32(item));
            }

            clientsSort.Add(clientId, new CPS() { OriginalSorting = new List<int>(prods), Sorting = new List<int>(prods) });
        }

        private static void Save()
        {
            lock (FileOperationsLocker.lockFilesObject)
            {
                string tempFile = Config.ClientProdSortFile;

                try
                {
                    //FileOperationsLocker.InUse = true;

                    if (File.Exists(tempFile))
                        File.Delete(tempFile);

                    if (clientsSort.Count(x => x.Value.Changed) > 0)
                    {
                        using (StreamWriter writer = new StreamWriter(tempFile, false))
                        {
                            foreach (var item in clientsSort)
                            {
                                if (!item.Value.Changed)
                                    continue;

                                writer.Write(item.Key.ToString(CultureInfo.InvariantCulture));
                                writer.Write((char)20);

                                writer.WriteLine(item.Value.ToString());
                            }

                            writer.Close();
                        }
                    }
                }
                finally
                {
                    //FileOperationsLocker.InUse = false;
                }
            }
        }

        public static void Load()
        {
            if (File.Exists(Config.ClientProdSortFile))
            {
                clientsSort.Clear();

                using (StreamReader reader = new StreamReader(Config.ClientProdSortFile))
                {
                    string line = string.Empty;

                    while ((line = reader.ReadLine()) != null)
                    {
                        try
                        {
                            string[] parts = line.Split(new char[] { (char)20 });

                            int clientId = Convert.ToInt32(parts[0], CultureInfo.InvariantCulture);
                            List<int> prods = new List<int>();

                            var idString = parts[1].Split(',');

                            foreach (var item in idString)
                                prods.Add(Convert.ToInt32(item));

                            if (!clientsSort.ContainsKey(clientId))
                                clientsSort.Add(clientId, new CPS());
                            clientsSort[clientId].Sorting = prods;
                        }
                        catch (Exception ee)
                        {
                            Logger.CreateLog(ee);
                        }
                    }
                    reader.Close();
                }
            }
        }

        internal static void Clear()
        {
            clientsSort.Clear();
        }
    }
}
