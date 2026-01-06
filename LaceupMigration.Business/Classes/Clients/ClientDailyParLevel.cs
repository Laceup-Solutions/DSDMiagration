using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;








namespace LaceupMigration
{
    public class ClientDailyParLevel
    {
        public int Id { get; set; }

        public int ClientId { get; set; }

        public int ProductId { get; set; }

        public int DayOfWeek { get; set; }

        public float qty;
        public float Qty
        {
            get
            {
                if (qty == -1)
                    return 0;
                return qty;
            }
            set
            {
                qty = value;
            }
        }

        float newQty;
        public float NewQty
        {
            get
            {
                if (newQty == -1)
                    return 0;
                return newQty;
            }
            set
            {
                newQty = value;
            }
        }

        public float Counted { get; set; }

        public bool Visited { get; set; }

        public bool CountedFlag { get; set; }

        public float Sold { get; set; }

        public float Return { get; set; }

        public float Dump { get; set; }

        public Product Product { get; set; }

        public Client Client { get; set; }

        public DateTime LastModified { get; set; }

        public int OrderId { get; set; }

        public string Department { get; set; }

        static List<ClientDailyParLevel> cdpls = new List<ClientDailyParLevel>();

        public static List<ClientDailyParLevel> List { get { return cdpls; } }

        public static void Add(ClientDailyParLevel x)
        {
            cdpls.Add(x);
        }

        public static void Remove(ClientDailyParLevel x)
        {
            cdpls.Remove(x);
        }

        public static void Clear()
        {
            cdpls.Clear();
        }

        public static void CreateNewParLevel(Client client, Product product, float qty, string department = "")
        {
            GetNewParLevel(client, product, qty, department);
        }

        public static ClientDailyParLevel GetNewParLevel(Client client, Product product, float qty, string department = "")
        {
            var newpl = new ClientDailyParLevel();
            newpl.Id = NextAddedId() - 1;
            newpl.ProductId = product.ProductId;
            newpl.ClientId = client.ClientId;
            newpl.Product = product;
            newpl.Client = client;
            newpl.DayOfWeek = (int)DateTime.Now.DayOfWeek;
            newpl.NewQty = qty;
            newpl.Visited = true;
            newpl.LastModified = DateTime.Now;
            newpl.Department = department;

            Add(newpl);

            Save();

            return newpl;
        }

        public static ClientDailyParLevel GetParLevel(Client client, Product product)
        {
            return List.FirstOrDefault(x => x.ClientId == client.ClientId && x.ProductId == product.ProductId);
        }

        public static ClientDailyParLevel GetParLevel(Client client, Product product, DayOfWeek day)
        {
            return List.FirstOrDefault(x => x.ClientId == client.ClientId && x.ProductId == product.ProductId && x.MatchDayOfWeek(DateTime.Now.DayOfWeek));
        }

        public static int NextAddedId()
        {
            int lastId = 0;
            foreach (var item in List)
                if (item.Id < lastId)
                    lastId = item.Id;

            return lastId;
        }

        public static void Save()
        {
            string tempFile = Config.SavedDailyParLevelFile;

            lock (FileOperationsLocker.lockFilesObject)
            {
                try
                {
                    //FileOperationsLocker.InUse = true;

                    Serialize(tempFile, cdpls.Where(x => x.Visited).ToList());
                }
                finally
                {
                    //FileOperationsLocker.InUse = false;
                }
            }
        }

        public static void Save(string fileName)
        {
            lock (FileOperationsLocker.lockFilesObject)
            {
                try
                {
                    //FileOperationsLocker.InUse = true;

                    Serialize(fileName, cdpls);
                }
                finally
                {
                    //FileOperationsLocker.InUse = false;
                }
            }
        }

        public static void Serialize(string file, List<ClientDailyParLevel> list)
        {
            if (File.Exists(file))
                File.Delete(file);

            using (StreamWriter writer = new StreamWriter(file, false))
            {
                foreach (var item in list)
                {
                    writer.Write(item.Id);                          //0
                    writer.Write((char)20);
                    writer.Write(item.ProductId);                   //1
                    writer.Write((char)20);
                    writer.Write(item.ClientId);                    //2
                    writer.Write((char)20);
                    writer.Write(item.qty);                         //3
                    writer.Write((char)20);
                    writer.Write(item.DayOfWeek);                   //4
                    writer.Write((char)20);
                    writer.Write(item.newQty);                      //5
                    writer.Write((char)20);
                    writer.Write(item.Counted);                     //6
                    writer.Write((char)20);
                    writer.Write(item.Visited ? "1" : "0");         //7
                    writer.Write((char)20);
                    writer.Write(item.CountedFlag ? "1" : "0");     //8
                    writer.Write((char)20);
                    writer.Write(item.Sold);                        //9
                    writer.Write((char)20);
                    writer.Write(item.Return);                      //10
                    writer.Write((char)20);
                    writer.Write(!string.IsNullOrEmpty(item.Client.UniqueId) ? item.Client.UniqueId : string.Empty);                      //11
                    writer.Write((char)20);
                    writer.Write(item.LastModified.Ticks);          //12
                    writer.Write((char)20);
                    writer.Write(item.Dump);                        //13
                    writer.Write((char)20);
                    writer.Write(item.OrderId);                        //14
                    writer.Write((char)20);
                    writer.Write(item.Department ?? string.Empty);                        //15
                    writer.Write((char)20);
                    writer.Write(item.Deleted ? "1" : "0");
                    writer.WriteLine();
                }
            }
        }

        public static void LoadCreatedParLevels(bool fromInitialize = false)
        {
            if (fromInitialize)
            {
                if (File.Exists(Config.DailyParLevelFile))
                {
                    using (StreamReader reader = new StreamReader(Config.DailyParLevelFile))
                    {
                        string line;

                        while ((line = reader.ReadLine()) != null)
                        {
                            var parts = line.Split((char)20);
                            CreateClientDailyParLevel(parts);
                        }
                        reader.Close();
                    }
                }
            }

            if (File.Exists(Config.SavedDailyParLevelFile))
            {
                using (StreamReader reader = new StreamReader(Config.SavedDailyParLevelFile))
                {
                    string line;

                    while ((line = reader.ReadLine()) != null)
                        CreateParLevelFromLine(line, true);
                    reader.Close();
                }
            }
        }

        public static void CreateClientDailyParLevel(string[] currentrow)
        {
            var id = Convert.ToInt32(currentrow[0], CultureInfo.InvariantCulture);
            var clientId = Convert.ToInt32(currentrow[1], CultureInfo.InvariantCulture);
            var productId = Convert.ToInt32(currentrow[2], CultureInfo.InvariantCulture);
            var dayOfWeek = Convert.ToInt32(currentrow[3], CultureInfo.InvariantCulture);
            var qty = Convert.ToSingle(currentrow[4], CultureInfo.InvariantCulture);
            string department = string.Empty;

            if (currentrow.Length > 5)
                department = currentrow[5];

            var client = Client.Find(clientId);
            if (client == null)
            {
                Logger.CreateLog("ClientDailyParLevel. Client not found with id=" + clientId);
                return;
            }

            var product = Product.Find(productId);
            if (product == null)
            {
                Logger.CreateLog("ClientDailyParLevel. Product not found with id=" + productId);
                return;
            }

            var cdpl = new ClientDailyParLevel()
            {
                Id = id,
                ProductId = productId,
                ClientId = clientId,
                DayOfWeek = dayOfWeek,
                Qty = qty,
                NewQty = qty,
                Product = product,
                Client = client,
                Department = department
            };

            ClientDailyParLevel.Add(cdpl);

        }

        static void CreateParLevelFromLine(string line, bool checkIfExists = false)
        {
            try
            {
                var parts = line.Split(new char[] { (char)20 });

                int iD = Convert.ToInt32(parts[0], CultureInfo.InvariantCulture);
                int prodID = Convert.ToInt32(parts[1], CultureInfo.InvariantCulture);
                int clientId = Convert.ToInt32(parts[2], CultureInfo.InvariantCulture);
                float qty = Convert.ToSingle(parts[3], CultureInfo.InvariantCulture);
                int day = Convert.ToInt32(parts[4], CultureInfo.InvariantCulture);
                float newqty = Convert.ToSingle(parts[5], CultureInfo.InvariantCulture);
                float counted = Convert.ToSingle(parts[6], CultureInfo.InvariantCulture);
                bool visited = Convert.ToInt32(parts[7], CultureInfo.InvariantCulture) > 0;
                bool countedFlag = Convert.ToInt32(parts[8], CultureInfo.InvariantCulture) > 0;

                float sold = 0;
                float returns = 0;
                float dump = 0;
                int orderId = 0;
                string department = string.Empty;

                if (parts.Length > 9)
                    sold = Convert.ToSingle(parts[9], CultureInfo.InvariantCulture);

                if (parts.Length > 10)
                    returns = Convert.ToSingle(parts[10], CultureInfo.InvariantCulture);

                if (parts.Length > 13)
                    dump = Convert.ToSingle(parts[13], CultureInfo.InvariantCulture);

                if (parts.Length > 14)
                    orderId = Convert.ToInt32(parts[14], CultureInfo.InvariantCulture);

                if (parts.Length > 15)
                    department = parts[15];

                bool deleted = false;
                if (parts.Length > 16)
                    deleted = Convert.ToInt32(parts[16]) > 0;

                var product = Product.Find(prodID);
                if (product == null)
                    return;

                var client = Client.Find(clientId);
                if (client == null)
                    return;

                ClientDailyParLevel pl = null;
                if (checkIfExists)
                    pl = List.FirstOrDefault(x => x.Product.ProductId == prodID && x.Client.ClientId == clientId && x.DayOfWeek == day && x.Department == department);

                if (pl == null)
                {
                    pl = new ClientDailyParLevel()
                    {
                        Id = iD,
                        ProductId = prodID,
                        ClientId = clientId,
                        Qty = qty,
                        DayOfWeek = day,
                        Product = product,
                        Client = client
                    };

                    List.Add(pl);
                }

                pl.NewQty = newqty;
                pl.Counted = counted;
                pl.Visited = visited;
                pl.CountedFlag = countedFlag;
                pl.Sold = sold;
                pl.Return = returns;
                pl.Dump = dump;
                pl.OrderId = orderId;
                pl.Department = department;
                pl.Deleted = deleted;

                if (parts.Length > 12)
                    pl.LastModified = DateTime.FromBinary(Convert.ToInt64(parts[12], CultureInfo.InvariantCulture));
            }
            catch (Exception ee)
            {
                Logger.CreateLog(ee);
            }
        }

        public void SetNewPar(float qty)
        {
            NewQty = qty;
            Visited = true;
            LastModified = DateTime.Now;
            Save();
        }

        public void SetCountedQty(float qty)
        {
            Counted = qty;
            Visited = true;
            CountedFlag = true;
            LastModified = DateTime.Now;
            Save();
        }

        public void SetSoldQty(float qty)
        {
            Sold = qty;
            Visited = true;
            LastModified = DateTime.Now;
        }

        public void SetReturnQty(float qty)
        {
            Return = qty;
            Visited = true;
            LastModified = DateTime.Now;
        }

        public void SetDumpQty(float qty)
        {
            Dump = qty;
            Visited = true;
            LastModified = DateTime.Now;
        }

        public void Delete()
        {
            SetInitialValue();

            NewQty = 0;
            Visited = true;
            Deleted = true;

            Save();
        }

        public void Void()
        {
            SetInitialValue();

            Save();
        }

        public void SetInitialValue()
        {
            if (Id > 0)
            {
                NewQty = Qty;
                Counted = 0;
                Visited = false;
                CountedFlag = false;
                LastModified = DateTime.Now;
                OrderId = 0;
            }
            else
                List.Remove(this);
        }

        public static void Void(int clientId, DayOfWeek dayOfWeek)
        {
            var set = cdpls.Where(x => x.ClientId == clientId && x.MatchDayOfWeek(DateTime.Now.DayOfWeek)).ToList();

            for (int i = 0; i < set.Count; i++)
                set[i].SetInitialValue();

            Save();
        }

        public static void Void(int clientId, DayOfWeek dayOfWeek, string department)
        {
            var set = cdpls.Where(x => x.ClientId == clientId && x.MatchDayOfWeek(DateTime.Now.DayOfWeek) && x.Department == department).ToList();

            for (int i = 0; i < set.Count; i++)
                set[i].SetInitialValue();

            Save();
        }

        public static bool ProductInPar(int clientId, DayOfWeek dayOfWeek, int productId)
        {
            return cdpls.FirstOrDefault(x => x.ClientId == clientId && x.ProductId == productId && x.MatchDayOfWeek(dayOfWeek)) != null;
        }

        public bool MatchDayOfWeek(DayOfWeek d)
        {
            if (Config.UseAllDayParLevel)
                return true;

            return DayOfWeek == (int)d;
        }

        public bool Deleted { get; set; }
    }

    public class DailyParLevelLine 
    {
        public Product Product { get; set; }

        public string ProductName { get; set; }

        public OrderDetail Sold { get; set; }

        public OrderDetail Return { get; set; }

        public OrderDetail Dump { get; set; }

        public ClientDailyParLevel ParLevel { get; set; }

        public double OriginalPrice { get; set; }

        public double Price { get; set; }

        public bool CurrentFocus { get; set; }

        public int PositionInList { get; set; }

        public int OrginalPosInList { get; set; }

        public override string ToString()
        {
            return string.Format("{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}{0}{8}{0}{9}{0}{10}", "|",
                Product.ProductId,
                Sold != null ? Sold.OrderDetailId : 0,
                Return != null ? Return.OrderDetailId : 0,
                Dump != null ? Dump.OrderDetailId : 0,
                ParLevel != null ? ParLevel.Id : 0,
                OriginalPrice,
                Price,
                CurrentFocus ? "1" : "0",
                PositionInList,
                OrginalPosInList);
        }

        public bool Enable { get; set; }

        public DateTime LastCountedDate { get; set; }

        public float LastCountedQty { get; set; }

        public DateTime LastDeliveryDate { get; set; }

        public float LastDeliveryQty { get; set; }
    }
}