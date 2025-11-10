using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;








namespace LaceupMigration
{
    public enum ReasonType
    {
        No_Service = 1,
        ReShip = 2,
        No_Delivery = 4,
        No_Payment = 8,
        Return = 16,
        Dump = 32,
        Transfer_Of = 64,
        Transfer_On = 128,
        No_Picked = 256,
        No_Received = 512,
        Refuse_To_Accept_Product = 1024,
        Lowest_Price_Level = 2048,
        Void = 4096,
        FreeItem = 8192
    }

    public class Reason
    {
        public int Id { get; set; }

        public string Description { get; set; }

        public int AvailableIn { get; set; }

        public string Language { get; set; }

        public bool LoadingError { get; set; }

        static List<Reason> reasons = new List<Reason>();

        public List<Reason> Reasons { get { return reasons; } }

        public static void Clear()
        {
            reasons.Clear();
        }

        public static void Add(Reason reason)
        {
            reasons.Add(reason);
        }

        public static List<Reason> GetReasonsByType(ReasonType type)
        {
            return reasons.FindAll(x => (x.AvailableIn & (int)type) > 0);
        }

        public static Reason Find(int id)
        {
            return reasons.FirstOrDefault(x => x.Id == id);
        }
    }

    public class CreditType
    {
        public string Description { get; set; }
        public bool Damaged { get; set; }
        public int ReasonId { get; set; }
    }
}