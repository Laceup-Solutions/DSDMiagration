





using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class VehicleInformation
    {
        public static VehicleInformation CurrentVehicleInformation { get; set; }
        public static VehicleInformation EODVehicleInformation { get; set; }
        public string PlateNumber { get; set; }
        public string Gas { get; set; }
        public string Assistant { get; set; }
        public double MilesFromDeparture { get; set; }
        public double MilesCompleted { get; set; }
        public string TireCondition { get; set; }
        public string SeatBelts { get; set; }
        public string SessionId { get; set; }

        public bool PutGas { get; set; }

        public bool isFromEOD { get; set; }

        public bool EngineOil {get; set; }
        public bool BrakeFluid { get; set; }
        public bool PowerSteeringFluid { get; set; }
        public bool TransmissionFluid { get; set; }
        public bool AntifreezeCoolant { get; set; }


        public static void Save(bool fromEOD = false)
        {
            if (fromEOD)
            {
                if (File.Exists(Config.EODVehicleInformationPath))
                    File.Delete(Config.EODVehicleInformationPath);

                using (var streamWriter = new StreamWriter(File.Create(Config.EODVehicleInformationPath)))
                    streamWriter.WriteLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13}", EODVehicleInformation.PlateNumber, EODVehicleInformation.Gas, EODVehicleInformation.Assistant, EODVehicleInformation.MilesFromDeparture,
                        EODVehicleInformation.TireCondition, EODVehicleInformation.SeatBelts, EODVehicleInformation.SessionId, EODVehicleInformation.isFromEOD ? "1" : "0", EODVehicleInformation.PutGas ? "1" : "0",
                        EODVehicleInformation.EngineOil, EODVehicleInformation.BrakeFluid, EODVehicleInformation.PowerSteeringFluid, EODVehicleInformation.TransmissionFluid, EODVehicleInformation.AntifreezeCoolant));
            }
            else
            {
                if (File.Exists(Config.VehicleInformationPath))
                    File.Delete(Config.VehicleInformationPath);


                using (var streamWriter = new StreamWriter(File.Create(Config.VehicleInformationPath)))
                    streamWriter.WriteLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13}", CurrentVehicleInformation.PlateNumber, CurrentVehicleInformation.Gas, CurrentVehicleInformation.Assistant, CurrentVehicleInformation.MilesFromDeparture,
                        CurrentVehicleInformation.TireCondition, CurrentVehicleInformation.SeatBelts, CurrentVehicleInformation.SessionId, CurrentVehicleInformation.isFromEOD ? "1" : "0",CurrentVehicleInformation.PutGas ? "1" : "0",
                        CurrentVehicleInformation.EngineOil, CurrentVehicleInformation.BrakeFluid, CurrentVehicleInformation.PowerSteeringFluid, CurrentVehicleInformation.TransmissionFluid, CurrentVehicleInformation.AntifreezeCoolant));
            }
        }

        public static void Load()
        {
            try
            {
                if (File.Exists(Config.VehicleInformationPath))
                {
                    using (var streamReader = new StreamReader(Config.VehicleInformationPath))
                    {
                        string line;
                        while ((line = streamReader.ReadLine()) != null)
                        {
                            var parts = line.Split(new char[] { ',' });

                            CurrentVehicleInformation = new VehicleInformation()
                            {
                                PlateNumber = parts[0],
                                Gas = parts[1],
                                Assistant = parts[2],
                                MilesFromDeparture = Convert.ToDouble(parts[3]),
                                TireCondition = parts[4],
                                SeatBelts = parts[5],
                                SessionId = parts[6],
                                isFromEOD = Convert.ToInt32(parts[7]) > 0,
                                PutGas = Convert.ToInt32(parts[8]) > 0
                            };

                            if (parts.Length > 9)
                                CurrentVehicleInformation.EngineOil = Convert.ToInt32(parts[9]) > 0;
                            if (parts.Length > 10)
                                CurrentVehicleInformation.BrakeFluid = Convert.ToInt32(parts[10]) > 0;
                            if (parts.Length > 11)
                                CurrentVehicleInformation.PowerSteeringFluid = Convert.ToInt32(parts[11]) > 0;
                            if (parts.Length > 12)
                                CurrentVehicleInformation.TransmissionFluid = Convert.ToInt32(parts[12]) > 0;
                            if (parts.Length > 13)
                                CurrentVehicleInformation.AntifreezeCoolant = Convert.ToInt32(parts[13]) > 0;
                        }
                    }
                }

                if (File.Exists(Config.EODVehicleInformationPath))
                {
                    using (var streamReader = new StreamReader(Config.EODVehicleInformationPath))
                    {
                        string line;
                        while ((line = streamReader.ReadLine()) != null)
                        {
                            var parts = line.Split(new char[] { ',' });

                            EODVehicleInformation = new VehicleInformation()
                            {
                                PlateNumber = parts[0],
                                Gas = parts[1],
                                Assistant = parts[2],
                                MilesFromDeparture = Convert.ToDouble(parts[3]),
                                TireCondition = parts[4],
                                SeatBelts = parts[5],
                                SessionId = parts[6],
                                isFromEOD = Convert.ToInt32(parts[7]) > 0,
                                PutGas = Convert.ToInt32(parts[8]) > 0
                            };

                            if (parts.Length > 9)
                                EODVehicleInformation.EngineOil = Convert.ToInt32(parts[9]) > 0;
                            if (parts.Length > 10)
                                EODVehicleInformation.BrakeFluid = Convert.ToInt32(parts[10]) > 0;
                            if (parts.Length > 11)
                                EODVehicleInformation.PowerSteeringFluid = Convert.ToInt32(parts[11]) > 0;
                            if (parts.Length > 12)
                                EODVehicleInformation.TransmissionFluid = Convert.ToInt32(parts[12]) > 0;
                            if (parts.Length > 13)
                                EODVehicleInformation.AntifreezeCoolant = Convert.ToInt32(parts[13]) > 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        public static void Clear()
        {
            CurrentVehicleInformation = null;
            EODVehicleInformation = null;

            if (File.Exists(Config.EODVehicleInformationPath))
                File.Delete(Config.EODVehicleInformationPath);

            if (File.Exists(Config.VehicleInformationPath))
                File.Delete(Config.VehicleInformationPath);
        }
    }

}