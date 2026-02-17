namespace LaceupMigration.Helpers
{
	/// <summary>
	/// Global flags set after "Clear Data" from Configuration. When true, the corresponding main tab
	/// will clear any selected items (invoices, orders, payments) on its next OnAppearing.
	/// </summary>
	public static class ClearDataState
	{
		/// <summary>When true, InvoicesPage will clear selection on next OnAppearing.</summary>
		public static bool ClearSelectionOnInvoicesAppear { get; set; }

		/// <summary>When true, OrdersPage will clear selection on next OnAppearing.</summary>
		public static bool ClearSelectionOnOrdersAppear { get; set; }

		/// <summary>When true, PaymentsPage will clear selection on next OnAppearing.</summary>
		public static bool ClearSelectionOnPaymentsAppear { get; set; }

		/// <summary>Call after Clear Data from Configuration so all three tabs clear selection when next opened.</summary>
		public static void SetClearSelectionOnNextTabAppear()
		{
			ClearSelectionOnInvoicesAppear = true;
			ClearSelectionOnOrdersAppear = true;
			ClearSelectionOnPaymentsAppear = true;
		}
	}
}
