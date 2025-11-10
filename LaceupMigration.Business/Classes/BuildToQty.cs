using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace LaceupMigration
{
	public class BuildToQty
	{
		public int ProductId { get; set; }

		public int ClientId { get; set; }

		public float Qty { get; set; }

		public static IList<BuildToQty> List = new List<BuildToQty>();

		public static void LoadList()
		{
			if (File.Exists(Config.BuildToQtyFile))
			{
				List.Clear();
				using (StreamReader reader = new StreamReader(Config.BuildToQtyFile))
				{
					string line;
					while ((line = reader.ReadLine()) != null)
					{
						try
						{
							string[] parts = line.Split(new char[] { (char)20 });
							int productID = Convert.ToInt32(parts[0], CultureInfo.InvariantCulture);
							int clientId = Convert.ToInt32(parts[1], CultureInfo.InvariantCulture);
							float qtyty = Convert.ToSingle(parts[2], CultureInfo.InvariantCulture);

							BuildToQty btq = new BuildToQty();
							btq.ClientId = clientId;
							btq.ProductId = productID;
							btq.Qty = qtyty;

							List.Add(btq);
						}
						catch (Exception ee)
						{
							Logger.CreateLog(ee);
							//Xamarin.Insights.Report(ee);
						}
					}
					reader.Close();
				}
			}
		}

		public static void SaveList()
		{
			string tempFile = Path.GetTempFileName();

			lock (FileOperationsLocker.lockFilesObject)
			{
				try
				{
					//FileOperationsLocker.InUse = true;
					
					using (StreamWriter writer = new StreamWriter(tempFile, false))
					{
						foreach (var btq in List)
						{
							writer.Write(btq.ProductId);
							writer.Write((char)20);
							writer.Write(btq.ClientId);
							writer.Write((char)20);
							writer.Write(btq.Qty);
							writer.WriteLine();
						}
						writer.Close();
					}
					
					File.Copy(tempFile, Config.BuildToQtyFile, true);
					File.Delete(tempFile);
				}
				finally
				{
					//FileOperationsLocker.InUse = false;
				}
			}
		}
	}
}

