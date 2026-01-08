using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;








namespace LaceupMigration
{
    public class SalesmanSession
    {
        public DateTime? ClockIn { get; set; }

        public DateTime? ClockOut { get; set; }

        static List<SalesmanSession> sessions = new List<SalesmanSession>();

        public static List<SalesmanSession> Sessions { get { return sessions; } }

        public static bool ClockedOut
        {
            get
            {
                var last = sessions.Count > 0 ? sessions.Last() : null;
                return last == null || last.ClockOut != null;
            }
        }

        public static void StartSession()
        {
            if (!ClockedOut)
                return;

            sessions.Add(new SalesmanSession() { ClockIn = DateTime.Now });

            Save();
        }

        public static void CloseSession()
        {
            var last = sessions.Count > 0 ? sessions.Last() : null;

            if (last == null)
                return;

            last.ClockOut = DateTime.Now;

            Save();
        }

        public static void Save()
        {
            string tempFile = Config.SalesmanSessionsFile;

            lock (FileOperationsLocker.lockFilesObject)
            {
                try
                {
                    //FileOperationsLocker.InUse = true;

                    if (File.Exists(tempFile))
                        File.Delete(tempFile);

                    using (StreamWriter writer = new StreamWriter(tempFile, false))
                    {
                        foreach (var btq in sessions)
                        {
                            string s = btq.ClockIn.HasValue ? btq.ClockIn.Value.Ticks.ToString(CultureInfo.InvariantCulture) : "";

                            s += (char)20;

                            if (btq.ClockOut.HasValue)
                                s += btq.ClockOut.Value.Ticks.ToString(CultureInfo.InvariantCulture);

                            if (!string.IsNullOrEmpty(Config.SessionId))
                            {
                                s += (char)20;
                                s += Config.SessionId;
                            }

                            writer.WriteLine(s);
                        }
                        writer.Close();
                    }
                }
                catch (Exception ex)
                {
                    Logger.CreateLog(ex);
                }
                finally
                {
                    //FileOperationsLocker.InUse = false;
                }
            }
        }

        public static void LoadSessions()
        {
            if (File.Exists(Config.SalesmanSessionsFile))
            {
                sessions.Clear();
                using (StreamReader reader = new StreamReader(Config.SalesmanSessionsFile))
                {
                    string line;
                    string[] parts;

                    while ((line = reader.ReadLine()) != null)
                    {
                        try
                        {
                            parts = line.Split(new char[] { (char)20 });

                            DateTime? clockIn = null;
                            DateTime? clockOut = null;

                            if (!string.IsNullOrEmpty(parts[0]))
                                clockIn = new DateTime(Convert.ToInt64(parts[0], CultureInfo.InvariantCulture));

                            if (!string.IsNullOrEmpty(parts[1]))
                                clockOut = new DateTime(Convert.ToInt64(parts[1], CultureInfo.InvariantCulture));

                            if (clockIn == null && clockOut == null)
                                continue;

                            sessions.Add(new SalesmanSession() { ClockIn = clockIn, ClockOut = clockOut });
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

        public static DateTime GetFirstClockIn()
        {
            var session = sessions.FirstOrDefault(x => x.ClockIn.HasValue);
            if (session == null)
                return DateTime.Now;
            return session.ClockIn.Value;
        }

        public static DateTime GetLastClockOut()
        {
            var session = sessions.LastOrDefault(x => x.ClockOut.HasValue);
            if (session == null)
                return DateTime.Now;
            return session.ClockOut.Value;
        }

        public static TimeSpan GetTotalBreaks()
        {
            SalesmanSession previous = null;
            TimeSpan interval = new TimeSpan();
            foreach (var session in Sessions)
            {
                if(previous != null)
                    interval = interval.Add(session.ClockIn.Value.Subtract(previous.ClockOut.Value));

                previous = session;
            }

            return interval;
        }
    }
}