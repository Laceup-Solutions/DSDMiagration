using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;








namespace LaceupMigration
{
    public enum TransferAction
    {
        On,
        Off
    }

    public class Transfer
    {
        public int LastDetailId { get; set; }
        public string UniqueId { get; set; }
        public DateTime ClockIn { get; set; }
        public DateTime ClockOut { get; set; }
        public string ExtraFields { get; set; }
        public int SourceSiteId { get; set; }
        public int TargetSiteId { get; set; }
        public int SalesmanId { get; set; }
        public TransferAction Type { get; set; }

        public string Comment { get; set; }
        public List<TransferDetail> Details { get; set; }

        public Transfer(TransferAction action, int sourceId, int targetId, string comment)
        {
            UniqueId = Guid.NewGuid().ToString();
            ClockIn = DateTime.Now;
            ClockOut = DateTime.Now;
            ExtraFields = UDFHelper.SyncSingleUDF("uniqueId", UniqueId, "");
            SalesmanId = Config.SalesmanId;
            Type = action;
            SourceSiteId = sourceId;
            TargetSiteId = targetId;
            Comment = comment;
            Details = new List<TransferDetail>();
            LastDetailId = 0;
        }

        public void AddDetail(int prod, float qty, int uom, double Weight)
        {
            Details.Add(new TransferDetail()
            {
                Product = prod,
                Qty = qty,
                UoM = uom,
                Weight = Weight
                
            });
        }

        public void AddDetail(int prod, float qty, int uom, string lot, double Weight)
        {
            Details.Add(new TransferDetail()
            {
                Product = prod,
                Qty = qty,
                UoM = uom,
                Lot = lot,
                 Weight = Weight
            });
        } 
        public void AddDetail(int prod, float qty, int uom, string lot, DateTime lotExp, double Weight)
        {
            Details.Add(new TransferDetail()
            {
                Product = prod,
                Qty = qty,
                UoM = uom,
                Lot = lot,
                LotExp = lotExp,
                Weight = Weight
            });
        }

        TransferDetail GetSimilar(int prod, int uomId)
        {
            foreach (var item in Details.Where(x => x.Product == prod))
            {
                if (item.UoM == uomId)
                    return item;
            }

            return null;
        }

        public string SaveInFile()
        {
            var file = Type == TransferAction.On ? Config.TransferOnFile : Config.TransferOffFile;

            if (Config.ButlerCustomization)
            {
                if (!Directory.Exists(Config.ButlerTransfersOff))
                    Directory.CreateDirectory(Config.ButlerTransfersOff);

                if (!Directory.Exists(Config.ButlerTransfersOn))
                    Directory.CreateDirectory(Config.ButlerTransfersOn);
                file = Type == TransferAction.On ? Path.Combine(Config.ButlerTransfersOn, this.UniqueId) : Path.Combine(Config.ButlerTransfersOff, this.UniqueId);
            }

            if (File.Exists(file))
            {
                using (StreamReader reader = new StreamReader(file))
                {
                    string line = reader.ReadLine();
                    while ((line = reader.ReadLine()) != null)
                    {
                        var parts = line.Split((char)20);

                        var prodId = Convert.ToInt32(parts[2]);
                        float qty = Convert.ToSingle(parts[3]);
                        var uomId = Convert.ToInt32(parts[6]);

                        var detail = GetSimilar(prodId, uomId);

                        double Weight = 0;
                        if(parts.Length > 10)
                            Weight = Convert.ToDouble(parts[10]);

                        if (detail == null)
                        {
                            detail = new TransferDetail() { Product = prodId, UoM = uomId, Weight = Weight };
                            Details.Add(detail);
                        }
                        detail.Qty += qty;
                    }
                }

                File.Delete(file);
            }

            using (StreamWriter writer = new StreamWriter(file, false))
            {
                var clockIn = DateTime.Now;
                var clockOut = DateTime.Now;
                string uniqueId = Guid.NewGuid().ToString();
                string extrafields = UDFHelper.SyncSingleUDF("uniqueId", uniqueId, "");
                
                string commentTemp = this.Comment == null ? string.Empty : this.Comment.Replace((char)13, (char)32).Replace((char)10, (char)32);


                string line = string.Format(CultureInfo.InvariantCulture, "{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}{0}{8}" +
            "{0}{9}{0}{10}{0}{11}{0}{12}{0}{13}{0}{14}{0}{15}{0}{16}{0}{17}{0}{18}{0}{19}{0}{20}{0}{21}",
                                    (char)20,
                                    0,              //0
                                    SourceSiteId,     //1
                                    TargetSiteId,     //2
                                    "", "", "", "", "", "", "",
                                    SalesmanId,                                  //10
                                    ClockIn.ToString(CultureInfo.InvariantCulture),     //11
                                    ClockOut.ToString(CultureInfo.InvariantCulture),    //12
                                    ClockIn.Ticks,                                      //13
                                    ClockOut.Ticks,                                     //14
                                    UniqueId,                                           //15
                                    "", "",
                                    ExtraFields,                                        //18
                                    (int)Type,                                           //19
                                    commentTemp
                                    );

                writer.WriteLine(line);

                foreach (var btq in Details)
                {
                    line = string.Format(CultureInfo.InvariantCulture, "{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}{0}{8}{0}{9}{0}{10}{0}{11}",
                                    (char)20,
                                    0,                                  //0
                                    0,                                  //1
                                    btq.Product,                        //2
                                    btq.Qty,                            //3
                                    string.Empty,                       //4
                                    1,                                  //5
                                    btq.UoM,                            //6
                                    "",                                 //7
                                    btq.Lot,                            //8
                                    btq.LotExp.Ticks,                   //9
                                    btq.Weight                          //10
                                    );

                    writer.WriteLine(line);
                }
                writer.Close();
            }

            return file;
        }
    }

    public class TransferDetail
    {
        public int Product { get; set; }
        public float Qty { get; set; }
        public int UoM { get; set; }
        public string Lot { get; set; }
        public DateTime LotExp { get; set; }

        public double Weight { get; set; }
        public TransferDetail()
        {
            Lot = "";
        }
    }

    public class TransferLine 
    {
        public Product Product { get; set; }

        public UnitOfMeasure UoM { get; set; }

        public List<TransferLineDet> Details { get; set; }

        public bool CurrentFocus { get; set; }

        public double Weight
        {
            get
            {
                var det = Details.FirstOrDefault();
                if (det != null)
                    return det.Weight;
                else
                    return 0;
            }
        }

        public float QtyTransferred
        {
            get
            {
                float qty = 0;
                foreach (var item in Details)
                {
                    var x = item.Qty;
                    if (item.UoM != null)
                        x *= item.UoM.Conversion;

                    qty += x;
                }

                return qty;
            }
        }

        public TransferLine()
        {
            Details = new List<TransferLineDet>();
        }
    }

    public class TransferLineDet 
    {
        public Product Product { get; set; }

        public float Qty { get; set; }

        public UnitOfMeasure UoM { get; set; }

        public string Lot { get; set; }

        public DateTime LotExp { get; set; }

        public double Weight { get; set; }

        public int Id { get; set; }

        public TransferLineDet()
        {
            Lot = "";
            Weight = 0;
        }
    }
}