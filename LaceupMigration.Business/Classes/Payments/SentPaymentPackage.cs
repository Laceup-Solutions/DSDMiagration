using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;


namespace LaceupMigration
{

	public class SentPayment
	{
		public int ClientId { get; set; }

		public Client GetClient
		{
			get
			{
				return Client.Clients.FirstOrDefault(x => x.ClientId == this.ClientId);
			}
		}

		public string PaymentType { get; set; }

		public double Amount { get; set; }

		public string Comment { get; set; }

        public string OrderUniqueId { get; set; }

		public DateTime Date { get; set; }

		public string PackagePath { get; set; }
    }

	public class SentPaymentPackage
	{
		private DataSet dataSet;

		public string PackagePath
		{
			get;
			set;
		}

		public DateTime CreatedDate
		{
			get;
			set;
		}

		public IEnumerable<SentPayment> PackageContent()
		{
			List<SentPayment> details = new List<SentPayment>();

			// unpack the dataset
			string tempFile = System.IO.Path.Combine(Config.OrderPath, "temporder.xml");
			try
			{
				if (File.Exists(tempFile))
					File.Delete(tempFile);
				DataAccess.UnzipFile(PackagePath, tempFile);

				using (StreamReader stream = new StreamReader(tempFile))
				{
					using (XmlTextReader reader = new XmlTextReader(stream))
					{
						dataSet = new DataSet();
						dataSet.Locale = CultureInfo.InvariantCulture;
						dataSet.ReadXml(reader, XmlReadMode.ReadSchema);
					}
				}
				// Interprete the DS
				foreach (DataRow oRow in dataSet.Tables[0].Rows)
				{
					SentPayment payment = new SentPayment();
					payment.ClientId = Convert.ToInt32(oRow["ClientId"], CultureInfo.InvariantCulture);
					payment.PaymentType = oRow["PaymentMethod"].ToString();
					payment.Amount = Convert.ToDouble(oRow["Amount"], CultureInfo.InvariantCulture);
					payment.Comment = oRow["Comments"].ToString();
                    payment.OrderUniqueId = oRow["OrderUniqueId"].ToString();
					payment.Date = (DateTime)oRow["DateCreated"];
					payment.PackagePath = PackagePath;
					details.Add(payment);
				}
			}
			catch
			{
			}
			finally
			{
				File.Delete(tempFile);
			}

			return details;
   		}

		public static IList<SentPaymentPackage> Packages()
		{
			List<SentPaymentPackage> list = new List<SentPaymentPackage>();

			foreach (string file in Directory.GetFiles(Config.SentPaymentPath))
			{
				SentPaymentPackage package = new SentPaymentPackage();
				package.CreatedDate = new FileInfo(file).CreationTime;
				package.PackagePath = file;
				list.Add(package);
			}

			return list;
		}
	}
}