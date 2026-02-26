using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Helpers;
using LaceupMigration.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Maui.ApplicationModel;

namespace LaceupMigration.ViewModels
{
	public partial class PaymentsPageViewModel : ObservableObject
	{
		private enum SearchBy
		{
			ClientName = 0,
			InvoiceNum = 1
		}

		private readonly DialogService _dialogService;
		private readonly ILaceupAppService _appService;

		private Dictionary<Client, List<InvoicePayment>> _payments = new();
		private string _searchCriteria = string.Empty;
		private SearchBy _searchBy = SearchBy.ClientName;
		private bool _isUpdatingSelectAll = false;
		private Timer? _searchDebounceTimer;
		private const int SearchDebounceMs = 300;

		[ObservableProperty] private ObservableCollection<ClientPaymentGroupViewModel> _clientGroups = new();
		[ObservableProperty] private string _searchQuery = string.Empty;
		[ObservableProperty] private bool _isSearchVisible;
		[ObservableProperty] private bool _showButtonsLayout;
		[ObservableProperty] private bool _showSelectAllLayout;
		[ObservableProperty] private bool _isSelectAllChecked;
		[ObservableProperty] private string _selectAllText = "Select All";
		[ObservableProperty] private string _totalText = string.Empty;
		[ObservableProperty] private bool _showTotal;
		[ObservableProperty] private bool _showSendButton = true;
		[ObservableProperty] private bool _showDepositButton;
		[ObservableProperty] private bool _showSummaryButton;
		[ObservableProperty] private bool _showDeleteButton = true;
		[ObservableProperty] private string _deleteButtonText = "Delete";
		[ObservableProperty] private string _searchByText = "Client Name";
		[ObservableProperty] private string _searchPlaceholder = "Search by client name";

		public PaymentsPageViewModel(DialogService dialogService, ILaceupAppService appService)
		{
			_dialogService = dialogService;
			_appService = appService;
		}

		public async Task OnAppearingAsync()
		{
			if (ClearDataState.ClearSelectionOnPaymentsAppear)
			{
				ClearDataState.ClearSelectionOnPaymentsAppear = false;
				ClearSelection();
			}
			RefreshUI();
		}

		private void ClearSelection()
		{
			SelectedPayments.Clear();
			_isUpdatingSelectAll = true;
			try
			{
				RefreshClientGroups();
				IsSelectAllChecked = false;
				RefreshListHeader();
				UpdateSelectAllState();
			}
			finally
			{
				_isUpdatingSelectAll = false;
			}
		}

		partial void OnSearchQueryChanged(string value)
		{
			// Debounce search
			_searchDebounceTimer?.Dispose();
			_searchDebounceTimer = new Timer(_ =>
			{
				MainThread.BeginInvokeOnMainThread(() =>
				{
					if (_searchCriteria != value)
					{
						_searchCriteria = value;
						Filter();
					}
				});
			}, null, SearchDebounceMs, Timeout.Infinite);
		}

		[RelayCommand]
		private async Task SearchMenu()
		{
			var options = new[] { "Client Name", "Invoice Number" };
			var choice = await _dialogService.ShowActionSheetAsync("Search By", "", "Cancel", options);

			if (choice == "Client Name")
			{
				_searchBy = SearchBy.ClientName;
				SearchByText = "Client Name";
				SearchPlaceholder = "Search by client name";
			}
			else if (choice == "Invoice Number")
			{
				_searchBy = SearchBy.InvoiceNum;
				SearchByText = "Invoice Number";
				SearchPlaceholder = "Search by invoice number";
			}

			if (!string.IsNullOrEmpty(_searchCriteria))
				Filter();
		}

		[RelayCommand]
		private void SelectAll()
		{
			if (_isUpdatingSelectAll) return;
			
			_isUpdatingSelectAll = true;
			try
			{
				// When user clicks checkbox, it's already toggled, so we use the current state
				// If checked, select all; if unchecked, clear all
				if (IsSelectAllChecked)
				{
					// Select all payments
					var paymentsToAdd = InvoicePayment.List.ToList();

					if (!string.IsNullOrEmpty(_searchCriteria))
					{
						if (_searchBy == SearchBy.ClientName)
							paymentsToAdd = paymentsToAdd.Where(x => x.Client.ClientName.ToLowerInvariant().Contains(_searchCriteria.ToLowerInvariant())).ToList();
						else
							paymentsToAdd = paymentsToAdd.Where(x => x.Orders().Any(y => y.PrintedOrderId.ToLowerInvariant().Contains(_searchCriteria.ToLowerInvariant()))).ToList();
					}

					SelectedPayments.Clear();
					SelectedPayments.AddRange(paymentsToAdd);
				}
				else
				{
					// Clear all selections
					SelectedPayments.Clear();
				}

				// RefreshClientGroups will recreate items with correct selection state
				RefreshClientGroups();
				RefreshListHeader();
				// Verify IsSelectAllChecked matches the actual state
				var totalPayments = ClientGroups.Sum(g => g.Payments.Count);
				var shouldBeChecked = totalPayments > 0 && SelectedPayments.Count == totalPayments;
				if (IsSelectAllChecked != shouldBeChecked)
				{
					IsSelectAllChecked = shouldBeChecked;
				}
			}
			finally
			{
				_isUpdatingSelectAll = false;
			}
		}

		[RelayCommand]
		private async Task Payment()
		{
			_appService.RecordEvent("AddPayments button");
			// Navigate to payment selection page when created
			await Shell.Current.GoToAsync("paymentselectclient");
		}

		[RelayCommand]
		private async Task Print()
		{
			if (SelectedPayments.Count == 0)
			{
				await _dialogService.ShowAlertAsync("Select payments to be printed.", "Alert", "OK");
				return;
			}

			try
			{
				PrinterProvider.PrintDocument((int copies) =>
				{
					CompanyInfo.SelectedCompany = CompanyInfo.Companies[0];
					IPrinter printer = PrinterProvider.CurrentPrinter();
					bool allWent = true;

					foreach (var invoicePayment in SelectedPayments)
					{
						for (int i = 0; i < copies; i++)
						{
							if (!printer.PrintPayment(invoicePayment))
								allWent = false;
							else
							{
								invoicePayment.Printed = true;
								invoicePayment.Save();
							}
						}
					}

					if (!allWent)
						return "Error printing payments";
					return string.Empty;
				});
			}
			catch (Exception ex)
			{
				await _dialogService.ShowAlertAsync($"Error printing: {ex.Message}", "Error", "OK");
				_appService.TrackError(ex);
			}
		}

		[RelayCommand]
		private async Task Send()
		{
			if (SelectedPayments.Count == 0)
			{
				await _dialogService.ShowAlertAsync("Select payments to be sent.", "Alert", "OK");
				return;
			}

			if (SelectedPayments.Any(x => !string.IsNullOrEmpty(x.OrderId)))
			{
				var result = await _dialogService.ShowConfirmationAsync("Warning", "Send only payments for open invoices?", "Yes", "No");
				if (result)
				{
					SelectedPayments.Clear();
					SelectedPayments.AddRange(SelectedPayments.Where(x => string.IsNullOrEmpty(x.OrderId)).ToList());
					await SendItAsync();
				}
				return;
			}

			var confirm = await _dialogService.ShowConfirmationAsync("Warning", "Continue sending payments?", "Yes", "No");
			if (confirm)
			{
				await SendItAsync();
			}
		}

		private async Task SendItAsync()
		{
			string title = "Info";
			string message = "Payments sent.";

			try
			{
				DataProvider.SendInvoicePaymentsBySource(SelectedPayments, true);
				InvoicePayment.LoadPayments();
			}
			catch (Exception ee)
			{
				Logger.CreateLog(ee);
				title = "Alert";
				message = "Error sending payments.";
			}

			await _dialogService.ShowAlertAsync(message, title, "OK");

			SelectedPayments.Clear();
			RefreshListHeader();
			RefreshUI();
		}

		[RelayCommand]
		private async Task Deposit()
		{
			if (_payments.Count == 0)
			{
				await _dialogService.ShowAlertAsync("Cannot create deposit.", "Alert", "OK");
				return;
			}

			if (SelectedPayments.Count == 0)
			{
				await _dialogService.ShowAlertAsync("Select at least one payment.", "Alert", "OK");
				return;
			}

			var checkComponents = new List<PaymentComponent>();
			foreach (var p in SelectedPayments)
			{
				if (!string.IsNullOrEmpty(p.OrderId))
				{
					await _dialogService.ShowAlertAsync("Cannot send deposit until end of day.", "Alert", "OK");
					return;
				}
				checkComponents.AddRange(p.Components.Where(x => x.PaymentMethod == InvoicePaymentMethod.Check));
			}

			var postedDate = DateTime.MinValue;
			var first = checkComponents.FirstOrDefault();
			if (first != null)
				postedDate = first.PostedDate.Date;

			if (checkComponents.Count > 0 && !Config.CanDepositChecksWithDifDates && !checkComponents.All(x => x.PostedDate.Date == postedDate.Date))
			{
				await _dialogService.ShowAlertAsync("You cannot make a deposit with checks if they have different posted dates.", "Alert", "OK");
				return;
			}

			if (BankDeposit.currentDeposit == null)
			{
				var deposit = new BankDeposit();
				deposit.Payments.AddRange(SelectedPayments);
				deposit.PostedDate = postedDate != DateTime.MinValue ? postedDate : DateTime.Now.Date;
				BankDeposit.currentDeposit = deposit;
			}

			// Navigate to create deposit page when created
			await Shell.Current.GoToAsync("createdeposit");
		}

		[RelayCommand]
		private async Task Summary()
		{
			// Navigate to payment summary page when created
			await Shell.Current.GoToAsync("paymentsummary");
		}

		[RelayCommand]
		private async Task Delete()
		{
			if (SelectedPayments.Count == 0)
			{
				return;
			}

			var result = await _dialogService.ShowConfirmationAsync("Are you sure you want to delete these payments?", "Warning", "Yes", "No");
			if (result)
			{
				foreach (var payment in SelectedPayments)
				{
					if (Config.VoidPayments)
						payment.Void();
					else
						payment.Delete();
				}

				SelectedPayments.Clear();
				RefreshListHeader();
				RefreshUI();
			}
		}

		private void RefreshUI()
		{
			_payments = new Dictionary<Client, List<InvoicePayment>>();

			if (!DataProvider.CanUseApplication() || !Config.ReceivedData)
			{
				IsSearchVisible = false;
				ShowButtonsLayout = false;
				ShowSelectAllLayout = false;
				ClientGroups.Clear();
				return;
			}

			IsSearchVisible = true;
			ShowButtonsLayout = true;
			ShowTotal = !Config.HidePaymentsTotal;
			ShowSendButton = !Config.MustCreatePaymentDeposit;
			ShowSummaryButton = Config.ShowPaymentSummary;
			ShowDeleteButton = Config.DeletePaymentsInTab;
			DeleteButtonText = Config.VoidPayments ? "Void" : "Delete";

			if (Config.CommunicatorVersion == null)
				NetAccess.GetCommunicatorVersion();

			var showDeposit = Config.CheckCommunicatorVersion("46.0.0");
			ShowDepositButton = showDeposit && Config.ShowInvoicesCreditsInPayments;

			foreach (var item in InvoicePayment.List)
			{
				var key = _payments.Keys.FirstOrDefault(x => x.ClientId == item.Client.ClientId);
				if (key == null)
				{
					_payments.Add(item.Client, new List<InvoicePayment>());
					key = item.Client;
				}
				_payments[key].Add(item);
			}

			Filter();
			ShowSelectAllLayout = ClientGroups.Count > 0;
			UpdateButtonState();
		}

		private void Filter()
		{
			var source = _payments;

			if (!string.IsNullOrEmpty(_searchCriteria))
			{
				if (_searchBy == SearchBy.ClientName)
					source = new Dictionary<Client, List<InvoicePayment>>(source.Where(x => x.Key.ClientName.ToLowerInvariant().Contains(_searchCriteria.ToLowerInvariant())));
				else
					source = new Dictionary<Client, List<InvoicePayment>>(source.Where(x =>
						x.Value.Any(y => y.Orders().Any(z => z.PrintedOrderId.ToLowerInvariant().Contains(_searchCriteria.ToLowerInvariant()))
							|| y.Invoices().Any(z => z.InvoiceNumber.ToLowerInvariant().Contains(_searchCriteria.ToLowerInvariant())))));
			}

			RefreshClientGroups(source.OrderBy(x => x.Key.ClientName).ToDictionary(x => x.Key, y => y.Value));
		}

		private void RefreshClientGroups(Dictionary<Client, List<InvoicePayment>> source = null)
		{
			if (source == null)
				source = _payments;

			ClientGroups.Clear();
			foreach (var kvp in source)
			{
				var group = new ClientPaymentGroupViewModel
				{
					ClientName = kvp.Key.ClientName
				};

				foreach (var payment in kvp.Value)
				{
					var item = new PaymentListItemViewModel(payment, this);
					group.Payments.Add(item);
				}

				ClientGroups.Add(group);
			}

			ShowSelectAllLayout = ClientGroups.Count > 0;
			UpdateSelectAllState();
		}

		private void UpdateSelectAllState()
		{
			var totalPayments = ClientGroups.Sum(g => g.Payments.Count);
			var shouldBeChecked = totalPayments > 0 && SelectedPayments.Count == totalPayments;
			if (IsSelectAllChecked != shouldBeChecked)
			{
				IsSelectAllChecked = shouldBeChecked;
			}
		}

		private void RefreshListHeader()
		{
			if (SelectedPayments.Count > 0)
			{
				SelectAllText = $"Selected Payments: {SelectedPayments.Count}";
				TotalText = $"Total: {SelectedPayments.Sum(x => x.TotalPaid).ToCustomString()}";
				ShowTotal = true;
			}
			else
			{
				SelectAllText = "Select All";
				TotalText = string.Empty;
				ShowTotal = false;
			}
			UpdateButtonState();
		}

		private void UpdateButtonState()
		{
			// Delete button enabled state is handled by IsVisible
		}

		public List<InvoicePayment> SelectedPayments { get; } = new();

		public void TogglePaymentSelection(InvoicePayment payment)
		{
			if (!SelectedPayments.Contains(payment))
				SelectedPayments.Add(payment);
			else
				SelectedPayments.Remove(payment);

			RefreshListHeader();
			UpdateSelectAllState();
			
			// Update the payment item's selection state (skip handler to prevent infinite loop)
			// Only update if the value is actually different
			foreach (var group in ClientGroups)
			{
				foreach (var item in group.Payments)
				{
					if (item.Payment == payment)
					{
						var shouldBeSelected = SelectedPayments.Contains(payment);
						if (item.IsSelected != shouldBeSelected)
						{
							item.SetIsSelected(shouldBeSelected, skipHandler: true);
						}
						break;
					}
				}
			}
		}
	}

	public partial class ClientPaymentGroupViewModel : ObservableObject
	{
		[ObservableProperty] private string _clientName = string.Empty;
		[ObservableProperty] private ObservableCollection<PaymentListItemViewModel> _payments = new();
	}

	public partial class PaymentListItemViewModel : ObservableObject
	{
		private readonly InvoicePayment _payment;
		private readonly PaymentsPageViewModel _parent;
		private bool _isUpdatingSelection = false;

		[ObservableProperty] private bool _isSelected;

		public PaymentListItemViewModel(InvoicePayment payment, PaymentsPageViewModel parent)
		{
			_payment = payment;
			_parent = parent;
			SetIsSelected(_parent.SelectedPayments.Contains(_payment), skipHandler: true);

			// Populate components
			foreach (var component in _payment.Components)
			{
				Components.Add(new PaymentComponentDisplayViewModel(component, _payment));
			}
		}

		public string InvoiceNumberLabel
		{
			get
			{
				if (string.IsNullOrEmpty(_payment.InvoicesId) && string.IsNullOrEmpty(_payment.OrderId))
					return "Credit Account:";
				else
					return "Invoice #:";
			}
		}

		public string InvoiceNumberText
		{
			get
			{
				var ids = new List<string>();
				ids.AddRange(_payment.Invoices().Select(x => x.InvoiceNumber));
				if (_payment.Orders().Count() > 0)
				{
					ids.AddRange(_payment.Orders().Select(x => x.PrintedOrderId));
				}
				return string.Join(", ", ids);
			}
		}

		public string TotalText => $"Total: {_payment.Components.Sum(x => x.Amount).ToCustomString()}";

		[ObservableProperty] private ObservableCollection<PaymentComponentDisplayViewModel> _components = new();

		partial void OnIsSelectedChanged(bool value)
		{
			if (!_isUpdatingSelection)
			{
				_parent.TogglePaymentSelection(_payment);
			}
		}

		public void SetIsSelected(bool value, bool skipHandler = false)
		{
			if (skipHandler)
			{
				_isUpdatingSelection = true;
				IsSelected = value;
				_isUpdatingSelection = false;
			}
			else
			{
				IsSelected = value;
			}
		}

		[RelayCommand]
		private async Task ViewDetails()
		{
			await Shell.Current.GoToAsync($"paymentsetvalues?paymentId={_payment.Id}&detailViewPayments=1&goBackToMain=1");
		}

		public InvoicePayment Payment => _payment;

		partial void OnComponentsChanged(ObservableCollection<PaymentComponentDisplayViewModel> value)
		{
			// Components are populated in constructor
		}
	}

	public partial class PaymentComponentDisplayViewModel : ObservableObject
	{
		private readonly PaymentComponent _component;
		private readonly InvoicePayment _payment;

		public PaymentComponentDisplayViewModel(PaymentComponent component, InvoicePayment payment)
		{
			_component = component;
			_payment = payment;
			UpdateProperties();
		}

		public string PaymentMethodText
		{
			get
			{
				var fullText = GetPaymentMethodName(_component.PaymentMethod);
				if (!string.IsNullOrEmpty(_component.BankName))
					fullText += ", Bank";
				if (!string.IsNullOrEmpty(_component.Comments))
					fullText += ", Comments";
				if (_component.ExtraFields.Contains("Image"))
					fullText += ", Image";
				return fullText;
			}
		}

		public string AmountText => _component.Amount.ToCustomString();

		private string GetPaymentMethodName(InvoicePaymentMethod method)
		{
			return method switch
			{
				InvoicePaymentMethod.Cash => "Cash",
				InvoicePaymentMethod.Check => "Check",
				InvoicePaymentMethod.Credit_Card => "Credit Card",
				InvoicePaymentMethod.Money_Order => "Money Order",
				InvoicePaymentMethod.Transfer => "Transfer",
				InvoicePaymentMethod.Zelle_Transfer => "Zelle Transfer",
				InvoicePaymentMethod.ACH => "ACH",
				_ => "Cash"
			};
		}

		private void UpdateProperties()
		{
			OnPropertyChanged(nameof(PaymentMethodText));
			OnPropertyChanged(nameof(AmountText));
		}

		[RelayCommand]
		private async Task ViewDetails()
		{
			// Navigate to payment details using parent payment's ID
			await Shell.Current.GoToAsync($"paymentsetvalues?paymentId={_payment.Id}&detailViewPayments=1&goBackToMain=1");
		}
	}
}

