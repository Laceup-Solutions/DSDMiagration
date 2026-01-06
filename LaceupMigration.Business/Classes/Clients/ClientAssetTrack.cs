using System.Collections.Generic;
using System;

namespace LaceupMigration
{
    public class ClientAssetTrack
    {
        public static List<ClientAssetTrack> List = new List<ClientAssetTrack>();

        public int Id { get; set; }
        public int AssetId { get; set; }
        public int ClientId { get; set; }
        public System.DateTime StartDate { get; set; }
        public double Qty { get; set; }
        public double Price { get; set; }
        public string Comments { get; set; }
        public bool Active { get; set; }
        public System.DateTime DeactivatedDate { get; set; }
        public string Extrafields { get; set; }
        public Nullable<int> SiteId { get; set; }

    }
}