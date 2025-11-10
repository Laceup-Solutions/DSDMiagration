






using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;






namespace LaceupMigration
{
    public class Session
    {

        public static Session session;

        public static List<SessionDetails> sessionDetails = new List<SessionDetails>();

        public static SessionDetails selectedDetail;

        public string fileName;

        public static int lastSessionId = 1;

        public string SessionUniqueId { get; set; }

        public int SessionId { get; set; }

        public int SalesmanId { get; set; }

        public double StartLatitude { get; set; }

        public double StartLongitude { get; set; }


        public double EndLatitude { get; set; }

        public double EndLongitude { get; set; }

        public DateTime ClockIn { get; set; }
        public DateTime ClockOut { get; set; }
        public DateTime LastModifiedSync { get; set; }
        public bool isClockedInClient(int c)
        {
            if (session == null || sessionDetails == null || sessionDetails.Count == 0)
                return false;

            return sessionDetails.Any(x =>
          x.clientId == c &&
          string.IsNullOrEmpty(x.orderUniqueId) &&
          x.detailType == SessionDetails.SessionDetailType.CustomerVisit &&
          x.endTime == DateTime.MinValue);
        }
        
        public bool OtherOpenClockin(int Clientid)
        {
            var open = sessionDetails.FirstOrDefault(x =>
              x.clientId != Clientid &&
              string.IsNullOrEmpty(x.orderUniqueId) &&
              x.detailType == SessionDetails.SessionDetailType.CustomerVisit &&
              x.endTime == DateTime.MinValue);

            if (sessionDetails.Any(x => x.detailType == SessionDetails.SessionDetailType.Break && x.endTime == DateTime.MinValue))
            {
                DialogService._dialogService.ShowAlertAsync("You cannot create a transaction while you are on break.");
                return true;
            }

            if (open != null)
            {
                var client = Client.Find(open.clientId);
                DialogService._dialogService.ShowAlertAsync( "You have an open visit at: " + client.ClientName
                    + ". Please end that visit to start a new one at this client.");
                return true;
            }

            return false;
        }

        public Session()
        {
        }

        public List<SessionDetails> GetDetails()
        {
            return sessionDetails;
        }

        public Session(int salesman, DateTime clockIn)
        {
            SessionId = lastSessionId++;
            SalesmanId = salesman;
            ClockIn = clockIn;
            SessionUniqueId = salesman.ToString() + clockIn.Ticks.ToString();
            LastModifiedSync = DateTime.Now;
        }

        public void AddDetailFromOrder(Order order)
        {
            var deliveryClockIn = DateTime.Now;
            var batch = Batch.List.FirstOrDefault(x => x.Id == order.BatchId);

            if (batch != null)
                deliveryClockIn = batch.ClockedIn;

            var startTime = order.Date;
            if (order.IsDelivery)
                startTime = deliveryClockIn;

            var detail = sessionDetails.FirstOrDefault(x => x.orderUniqueId == order.UniqueId);
            if (detail == null)
            {
                Session.sessionDetails.Add(new SessionDetails(order.Client.ClientId, SessionDetails.SessionDetailType.CustomerVisit)
                {
                    startTime = startTime,
                    endTime = DateTime.Now,
                    startLatitude = order.Latitude,
                    startLongitude = order.Longitude,
                    endLatitude = order.Latitude,
                    endLongitude = order.Longitude,
                    orderUniqueId = order.UniqueId,
                    transactionName = order.NameforTransactionType,
                    fromdelete = true
                });

                session.Save();
            }
        }

        public void DeleteDetailFromOrder(Order order)
        {
            var detail = sessionDetails.FirstOrDefault(x => x.orderUniqueId == order.UniqueId);
            if(detail != null)
            {
                sessionDetails.Remove(detail);
                session.Save();
            }
        }

        public void AddDetail(SessionDetails detail)
        {
            sessionDetails.Add(detail);
        }

        public void DeleteDetail(SessionDetails detail)
        {
            sessionDetails.Remove(detail);
        }

        public void EditDetail(SessionDetails detail, DateTime clockout, double endlongitude, double endlatitude)
        {

            foreach (var mydetail in sessionDetails)
            {
                if (mydetail == detail)
                {
                    mydetail.endTime = clockout;
                    mydetail.endLatitude = endlatitude;
                    mydetail.endLongitude = endlongitude;
                }

            }

            Save();
        }

        public void ClearDetails()
        {
            sessionDetails.Clear();
        }

        #region serializeSession
        public void Save()
        {
            EnsureFileNameCreated();

            if(session != null)
                session.LastModifiedSync = DateTime.Now;

            using (StreamWriter writer = new StreamWriter(this.fileName))
            {
                SerializeSession(writer);
            }

        }

        public bool LoadFromFile(string file)
        {
            try
            {
                this.fileName = file;

                using (StreamReader reader = new StreamReader(this.fileName))
                {
                    DeserializeSession(reader);
                }
                //check if the order is ok, otherwise, just delete it
                if (SessionId == 0 || (sessionDetails.Count == 0))
                {
                    return false;
                }
                return true;
            }
            catch
            {
                var msg = "XXX The session contained in file " + file + " was deleted as we got error loading it, It was created on " + new FileInfo(file).CreationTimeUtc.ToString();

                File.Delete(file);
                return false;
            }
        }

        private void DeserializeSession(StreamReader reader)
        {
            try
            {
                //Read the order line
                string line = reader.ReadLine();
                string[] parts = line.Split(new char[] { (char)20 });
                int sessionId = Convert.ToInt32(parts[0], System.Globalization.CultureInfo.InvariantCulture);
                int salesmanId = Convert.ToInt32(parts[1], System.Globalization.CultureInfo.InvariantCulture);
                DateTime clockIn = DateTime.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture);
                DateTime clockOut = DateTime.Parse(parts[3], System.Globalization.CultureInfo.InvariantCulture);
                double startlatitude = Convert.ToDouble(parts[4], System.Globalization.CultureInfo.InvariantCulture);
                double startlongitude = Convert.ToDouble(parts[5], System.Globalization.CultureInfo.InvariantCulture);
                double endlatitude = Convert.ToDouble(parts[6], System.Globalization.CultureInfo.InvariantCulture);
                double endlongitude = Convert.ToDouble(parts[7], System.Globalization.CultureInfo.InvariantCulture);
                string sessionUniqueId = parts[8].ToString();


                DateTime lastSync = DateTime.Now;

                if (parts.Length > 9)
                    lastSync = new DateTime(Convert.ToInt64(parts[9]));

                this.SessionId = sessionId;
                if (lastSessionId < sessionId)
                    lastSessionId = sessionId;
                this.SalesmanId = salesmanId;
                this.ClockIn = clockIn;
                this.ClockOut = clockOut;
                this.StartLatitude = startlatitude;
                this.StartLongitude = startlongitude;
                this.EndLatitude = endlatitude;
                this.EndLongitude = endlongitude;
                this.SessionUniqueId = sessionUniqueId;
                this.LastModifiedSync = lastSync;

                sessionDetails.Clear();

                while ((line = reader.ReadLine()) != null)
                {
                    parts = line.Split(new char[] { (char)20 });
                    int sessionDetailId = Convert.ToInt32(parts[0], System.Globalization.CultureInfo.InvariantCulture);
                    int clientID = Convert.ToInt32(parts[1], System.Globalization.CultureInfo.InvariantCulture);

                    SessionDetails.SessionDetailType type = (SessionDetails.SessionDetailType)Convert.ToInt32(parts[2], System.Globalization.CultureInfo.InvariantCulture);
                    DateTime startTime = DateTime.Parse(parts[3], System.Globalization.CultureInfo.InvariantCulture);
                    DateTime endTime = DateTime.Parse(parts[4], System.Globalization.CultureInfo.InvariantCulture);
                    double dstartlatitude = Convert.ToDouble(parts[5], System.Globalization.CultureInfo.InvariantCulture);
                    double dstartlongitude = Convert.ToDouble(parts[6], System.Globalization.CultureInfo.InvariantCulture);
                    double dendlatitude = Convert.ToDouble(parts[7], System.Globalization.CultureInfo.InvariantCulture);
                    double dendlongitude = Convert.ToDouble(parts[8], System.Globalization.CultureInfo.InvariantCulture);
                    string orderUniqueId = parts[9].ToString();

                    string uniqueId = string.Empty;

                    string transactionName = string.Empty;
                    if (parts.Length > 10)
                        transactionName = parts[10];

                    bool fromDelete = false;

                    if (parts.Length > 11)
                        fromDelete = Convert.ToInt32(parts[11]) > 0;

                    if (parts.Length > 12)
                        uniqueId = parts[12];

                    string extraFields = string.Empty;
                    if (parts.Length > 13)
                        extraFields = parts[13];

                    SessionDetails detail = new SessionDetails(clientID, type);
                    detail.sessionDetailId = sessionDetailId;
                    detail.startTime = startTime;
                    detail.endTime = endTime;
                    detail.startLatitude = dstartlatitude;
                    detail.startLongitude = dstartlongitude;
                    detail.endLatitude = dendlatitude;
                    detail.endLongitude = dendlongitude;
                    detail.orderUniqueId = orderUniqueId;
                    detail.transactionName = transactionName;
                    detail.fromdelete = fromDelete;
                    detail.uniqueId = uniqueId;
                    detail.extraFields = extraFields;
                    
                    AddDetail(detail);
                }


            }
            catch (Exception ex)
            {

            }

        }

        private void EnsureFileNameCreated()
        {
            if (string.IsNullOrEmpty(fileName))
                this.fileName = Path.Combine(Config.SessionPath, "SessionFile.cvs");
        }

        private void SerializeSession(StreamWriter stream)
        {
            SerializeSessionLine(stream);
            foreach (var detail in sessionDetails)
                SerializeSessionDetails(stream, detail);
        }

        private void SerializeSessionLine(StreamWriter stream)
        {

            string line = string.Format(CultureInfo.InvariantCulture, "{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}{0}{8}{0}{9}{0}{10}",
                (char)20,
                this.SessionId,
                this.SalesmanId,
                this.ClockIn,
                this.ClockOut,
                this.StartLatitude,
                this.StartLongitude,
                this.EndLatitude,
                this.EndLongitude,
                this.SessionUniqueId,
                this.LastModifiedSync.Ticks

                );

            stream.WriteLine(line);
        }

        static void SerializeSessionDetails(StreamWriter stream, SessionDetails detail)
        {

            stream.WriteLine(string.Format(CultureInfo.InvariantCulture, "{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}{0}{8}{0}{9}{0}{10}{0}{11}{0}{12}{0}{13}{0}{14}",
                (char)20,
                detail.sessionDetailId,
                detail.clientId,
                (int)detail.detailType,
                detail.startTime,
                detail.endTime,
                detail.startLatitude,
                detail.startLongitude,
                detail.endLatitude,
                detail.endLongitude,
                detail.orderUniqueId,
                detail.transactionName,
                detail.fromdelete ? "1" : "0",
                detail.uniqueId,
                detail.extraFields ?? ""
                ));

        }

        public static void Clear()
        {
            Session.session = null;
            Session.sessionDetails.Clear();

            var path = Path.Combine(Config.SessionPath, "SessionFile.cvs");
            if (File.Exists(path))
                File.Delete(path);
        }


        public static void InitializeSession()
        {
            try
            {
                string filepath;
                if (File.Exists(Path.Combine(Config.SessionPath, "SessionFile.cvs")))
                {
                    filepath = Path.Combine(Config.SessionPath, "SessionFile.cvs");

                    var tempSession = new Session();
                    tempSession.LoadFromFile(filepath);

                    if (tempSession != null)
                        Session.session = tempSession;
                }
            }
            catch (Exception ex)
            {

            }
        }


        public static bool ClockOutCurrentSession()
        {
            Session.session.EndLatitude = DataAccess.LastLatitude;
            Session.session.EndLongitude = DataAccess.LastLongitude;

            Session.session.ClockOut = DateTime.Now;

            Session.session.Save();

            try
            {
                bool success = DataAccess.SendCurrentSession(Path.Combine(Config.SessionPath, "SessionFile.cvs"));

                if (success == true)
                {
                    if (File.Exists(Session.session.fileName))
                        File.Delete(Session.session.fileName);

                    Session.selectedDetail = null;
                    Session.sessionDetails.Clear();
                    Session.session.ClearDetails();
                    Session.session = null;
                }

                return success;
            }
            catch (Exception ex)
            {
                Logger.CreateLog("Error sending session => " + ex.ToString());
                return false;
            }
        }


        public static void CreateSession()
        {
            var session = new Session((int)Config.SalesmanId, DateTime.Now);
            Session.session = session;

            //locationhere and add it
            Session.session.StartLatitude = DataAccess.LastLatitude;
            Session.session.StartLongitude = DataAccess.LastLongitude;

            Session.session.Save();
        }

        public static bool CanUserOperateApp(out TimeSpan remaining)
        {
            remaining = TimeSpan.Zero;

            if (session == null)
                return true;

            var now = DateTime.Now;

            var activeBreak = sessionDetails
                .FirstOrDefault(x => x.detailType == SessionDetails.SessionDetailType.Break
                                     && x.endTime == DateTime.MinValue);
            
            if (activeBreak != null)
            {
                alreadyWarned = false;

                var mustEndAt = activeBreak.startTime.AddMinutes(Config.MandatoryBreakDuration);
                if (now < mustEndAt)
                {
                    remaining = mustEndAt - now;
                    return false;
                }

                activeBreak.endTime = mustEndAt;
                session.Save();
            }

            var lastStart = GetLastWorkStartTime();
            var worked = now - lastStart;

            var forceBreakMinutes = Config.ForceBreakInMinutes;

            var threshold = TimeSpan.FromMinutes(forceBreakMinutes);

            if (worked >= threshold)
            {
                remaining = TimeSpan.Zero;
                return false;
            }

            remaining = threshold - worked;
            return true;
        }

        
        public static DateTime GetLastWorkStartTime()
        {
            if (session == null)
                return DateTime.MinValue;

            var finishedBreaks = sessionDetails
                .Where(x => x.detailType == SessionDetails.SessionDetailType.Break &&
                            x.endTime != DateTime.MinValue)
                .OrderBy(x => x.endTime)
                .ToList();

            if (finishedBreaks.Count == 0)
                return session.ClockIn;

            var lastBreak = finishedBreaks.Last();

            if (lastBreak.endTime > session.ClockIn)
                return lastBreak.endTime;

            return session.ClockIn;
        }

        
        public static bool alreadyWarned = false;
        public static bool ShouldWarnForBreak()
        {
            if (session == null || alreadyWarned) 
                return false;

            if (sessionDetails.Any(x => x.detailType == SessionDetails.SessionDetailType.Break && x.endTime == DateTime.MinValue))
                return false;

            var now = DateTime.Now;
            var totalWorked = now - GetLastWorkStartTime();
            
            return totalWorked.TotalMinutes >= (Config.ForceBreakInMinutes - 10) && totalWorked.TotalMinutes < Config.ForceBreakInMinutes;
        }
        
        public static void StartMandatoryBreak()
        {
            var activeBreak = sessionDetails
                .FirstOrDefault(x => x.detailType == SessionDetails.SessionDetailType.Break 
                                     && x.endTime == DateTime.MinValue);

            if (activeBreak == null)
            {
                var breakDetail = new SessionDetails(0, SessionDetails.SessionDetailType.Break)
                {
                    startTime = DateTime.Now,
                    endTime = DateTime.MinValue
                };
                sessionDetails.Add(breakDetail);
                session.Save();
            }
        }

        public static void EndMandatoryBreak()
        {
            var activeBreak = sessionDetails
                .FirstOrDefault(x => x.detailType == SessionDetails.SessionDetailType.Break 
                                     && x.endTime == DateTime.MinValue);

            if (activeBreak != null)
            {
                if (DateTime.Now >= activeBreak.startTime.AddMinutes(Config.MandatoryBreakDuration))
                {
                    activeBreak.endTime = DateTime.Now;
                    session.Save();
                }
            }
        }
        
        #endregion
    }
}