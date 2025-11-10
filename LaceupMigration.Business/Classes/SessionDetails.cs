





using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class SessionDetails
    {
        public static int lastSessionId = 1;

        public enum SessionDetailType
        {
            Break = 1,
            CustomerVisit = 2,
            MidDayStop = 3
        }

        public int sessionDetailId { get; set; }

        public int clientId { get; set; }

        public string orderUniqueId { get; set; }

        public SessionDetailType detailType { get; set; }

        public DateTime startTime { get; set; }

        public DateTime endTime { get; set; }

        public double startLatitude { get; set; }
        public double startLongitude { get; set; }

        public double endLatitude { get; set; }
        public double endLongitude { get; set; }
        public string transactionName { get; set; }

        public bool fromdelete { get; set; }
        public string uniqueId { get; set; }
        
        public string extraFields { get; set; }


        public SessionDetails(int clientId, SessionDetailType type)
        {
            this.sessionDetailId = lastSessionId++;
            this.clientId = clientId;
            this.detailType = type;
            orderUniqueId = string.Empty;
            transactionName = string.Empty;
            uniqueId = Guid.NewGuid().ToString();
            extraFields = string.Empty;
        }
    }
}