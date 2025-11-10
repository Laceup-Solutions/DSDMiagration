


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace LaceupMigration
{
    [Serializable]
    public class ActivityState : System.Object
    {

        static public List<ActivityState> States = new List<ActivityState>();

        public string ActivityType { get; set; }
        public SerializableDictionary<string, string> State { get; set; }

        static ActivityState()
        {
        }

        public ActivityState()
        {
            State = new SerializableDictionary<string, string>();
        }

        public static ActivityState GetState(string activityType)
        {
            foreach (var state in States)
                if (state.ActivityType == activityType)
                    return state;
            return null;
        }

        public static void AddState(ActivityState newState)
        {
            foreach (var s in States)
                if (s.ActivityType == newState.ActivityType)
                {
                    Logger.CreateLog("Activity type [" + newState.ActivityType + "] is already in the stack");
                    return;
                }
            //Logger.CreateLog ("Adding state: " + newState.ActivityType);
            States.Add(newState);
            ActivityState.Save();
        }

        public static void AddFirstState(ActivityState newState)
        {
            foreach (var s in States)
                if (s.ActivityType == newState.ActivityType)
                {
                    Logger.CreateLog("Activity type [" + newState.ActivityType + "] is already in the stack");
                    return;
                }
            States.Insert(0, newState);
            ActivityState.Save();
        }

        public static void RemoveState(ActivityState newState)
        {
            //Logger.CreateLog ("removing state: " + newState.ActivityType);
            States.Remove(newState);
            ActivityState.Save();
        }

        public static void RemoveAll()
        {
            //Logger.CreateLog ("removing state: " + newState.ActivityType);
            States.Clear();
            ActivityState.Save();
        }

        public static void RemoveStatesUntilGetType(string type)
        {
            if (States.Count > 1)
                States.RemoveAt(States.Count - 1);

            for (int index = States.Count - 1; index > -1; index--)
                if (States[index].ActivityType == type)
                {
                    States = States.Take(index + 1).ToList();
                }
            ActivityState.Save();
        }

        public static void Save()
        {
            lock (FileOperationsLocker.lockFilesObject)
            {
                string datalocation1 = Config.ActivitiesStateFile;
                
                try
                {
                    //FileOperationsLocker.InUse = true;
                    
                    using (StreamWriter writer = new StreamWriter(datalocation1, false))
                    {
                        System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(typeof(List<ActivityState>));
                        x.Serialize(writer, States);
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

            if (File.Exists(Config.ActivitiesStateFile))
            {
                System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(typeof(List<ActivityState>));

                bool error = false;
                try
                {
                    using (Stream stream = new FileStream(Config.ActivitiesStateFile, FileMode.Open))
                    {
                        States = x.Deserialize(stream) as List<ActivityState>;
                    }
                }
                catch (Exception ee)
                {
                    Logger.CreateLog(ee);
                    error = true;
                }
                if (error)
                    try {

                        using (StreamReader textStreamReader = new StreamReader(Config.ActivitiesStateFile))
                        {
                            var text = textStreamReader.ReadToEnd();
                            Logger.CreateLog("Activity State file");
                            Logger.CreateLog(text);
                        }
                    } catch (Exception ee) { Logger.CreateLog(ee); }
            }
            else
            {
                Logger.CreateLog("no previous loaded");
            }
        }

    }

    [XmlRoot("dictionary")]
    public class SerializableDictionary<TKey, TValue>
        : Dictionary<TKey, TValue>, IXmlSerializable
    {
        #region IXmlSerializable Members
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));

            bool wasEmpty = reader.IsEmptyElement;
            reader.Read();

            if (wasEmpty)
                return;

            while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
            {
                reader.ReadStartElement("item");

                reader.ReadStartElement("key");
                TKey key = (TKey)keySerializer.Deserialize(reader);
                reader.ReadEndElement();

                reader.ReadStartElement("value");
                TValue value = (TValue)valueSerializer.Deserialize(reader);
                reader.ReadEndElement();

                this.Add(key, value);

                reader.ReadEndElement();
                reader.MoveToContent();
            }
            reader.ReadEndElement();
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));

            foreach (TKey key in this.Keys)
            {
                writer.WriteStartElement("item");

                writer.WriteStartElement("key");
                keySerializer.Serialize(writer, key);
                writer.WriteEndElement();

                writer.WriteStartElement("value");
                TValue value = this[key];
                valueSerializer.Serialize(writer, value);
                writer.WriteEndElement();

                writer.WriteEndElement();
            }
        }
        #endregion
    }
}

