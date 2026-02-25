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

				ZipMethods.UnzipFile(PackagePath, tempFile);

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

		/// <summary>Decodes the package file and creates a temporal InvoicePayment for the given SentPayment (for print/email).</summary>
		public static InvoicePayment CreateTemporalPaymentFromFile(string packagePath, SentPayment sentPayment)
		{
			if (string.IsNullOrEmpty(packagePath) || !File.Exists(packagePath) || sentPayment == null)
				return null;

			string tempFile = Path.Combine(Config.OrderPath, "temppayment.xml");
			try
			{
				if (File.Exists(tempFile))
					File.Delete(tempFile);
				ZipMethods.UnzipFile(packagePath, tempFile);

				using (var stream = new StreamReader(tempFile))
				using (var reader = new XmlTextReader(stream))
				{
					var ds = new DataSet { Locale = CultureInfo.InvariantCulture };
					ds.ReadXml(reader, XmlReadMode.ReadSchema);
					if (ds.Tables.Count == 0)
						return null;

					DataTable table = ds.Tables[0];
					if (!table.Columns.Contains("ClientId") || !table.Columns.Contains("Amount"))
						return null;

					bool hasOrderUniqueId = table.Columns.Contains("OrderUniqueId");
					var rows = table.Rows.Cast<DataRow>().Where(r =>
					{
						int cid = Convert.ToInt32(r["ClientId"], CultureInfo.InvariantCulture);
						if (cid != sentPayment.ClientId) return false;
						if (hasOrderUniqueId)
						{
							var ouid = r["OrderUniqueId"]?.ToString() ?? string.Empty;
							if (!string.Equals(ouid, sentPayment.OrderUniqueId ?? string.Empty, StringComparison.Ordinal))
								return false;
						}
						else
						{
							var rowDate = r["DateCreated"] is DateTime dt ? dt : Convert.ToDateTime(r["DateCreated"], CultureInfo.InvariantCulture);
							if (Math.Abs((rowDate - sentPayment.Date).TotalSeconds) > 1)
								return false;
						}
						return true;
					}).ToList();

					if (rows.Count == 0)
						return null;

					var client = Client.Clients.FirstOrDefault(x => x.ClientId == sentPayment.ClientId);
					if (client == null)
						return null;

					var components = new List<PaymentComponent>();
					string uniqueId = null;
					string invoicesId = null;
					DateTime dateCreated = sentPayment.Date;

					foreach (DataRow row in rows)
					{
						var comp = new PaymentComponent();
						comp.Amount = Convert.ToDouble(row["Amount"], CultureInfo.InvariantCulture);
						comp.Comments = row["Comments"]?.ToString() ?? string.Empty;
						comp.Ref = table.Columns.Contains("CheckNumber") ? row["CheckNumber"]?.ToString() ?? string.Empty : string.Empty;
						if (table.Columns.Contains("ExtraFields"))
							comp.ExtraFields = row["ExtraFields"]?.ToString() ?? string.Empty;

						string methodStr = row["PaymentMethod"]?.ToString() ?? "Cash";
						if (Enum.TryParse<InvoicePaymentMethod>(methodStr.Replace(" ", "_"), out var method))
							comp.PaymentMethod = method;
						else
							comp.PaymentMethod = InvoicePaymentMethod.Cash;

						components.Add(comp);

						if (uniqueId == null && table.Columns.Contains("UniqueId"))
						{
							var uid = row["UniqueId"]?.ToString() ?? string.Empty;
							uniqueId = uid.Contains("_") ? uid.Substring(0, uid.IndexOf('_')) : uid;
						}
						if (invoicesId == null && table.Columns.Contains("RefTransactions"))
							invoicesId = row["RefTransactions"]?.ToString();
						if (table.Columns.Contains("DateCreated"))
							dateCreated = row["DateCreated"] is DateTime d ? d : Convert.ToDateTime(row["DateCreated"], CultureInfo.InvariantCulture);
					}

					if (string.IsNullOrEmpty(uniqueId))
						uniqueId = sentPayment.OrderUniqueId ?? Guid.NewGuid().ToString();

					return InvoicePayment.CreateTemporal(client, sentPayment.OrderUniqueId ?? string.Empty, uniqueId, dateCreated, components, invoicesId, true);
				}
			}
			catch (Exception ex)
			{
				Logger.CreateLog(ex);
				return null;
			}
			finally
			{
				if (File.Exists(tempFile))
					File.Delete(tempFile);
			}
		}
	}
}