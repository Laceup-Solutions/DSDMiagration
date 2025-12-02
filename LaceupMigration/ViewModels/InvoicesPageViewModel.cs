using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

namespace LaceupMigration.ViewModels
{
	public partial class InvoicesPageViewModel : ObservableObject
	{
		private enum SelectedOption
		{
			All = 0,
			From1To30 = 1,
			From31to60 = 2,
			From61to90 = 3,
			Plus90 = 4
		}

		private enum SearchBy
		{
			ClientName = 0,
			InvoiceNum = 1
		}

		private readonly DialogService _dialogService;
		private readonly ILaceupAppService _appService;

		private Dictionary<Client, List<Invoice>> _all = new();
		private Dictionary<Client, List<Invoice>> _from1To30 = new();
		private Dictionary<Client, List<Invoice>> _from31To60 = new();
		private Dictionary<Client, List<Invoice>> _from61To90 = new();
		private Dictionary<Client, List<Invoice>> _over90 = new();

		private double _allAmount = 0;
		private double _from1To30Amount = 0;
		private double _from31To60Amount = 0;
		private double _from61To90Amount = 0;
		private double _over90Amount = 0;

		private SelectedOption _whatIsVisible = SelectedOption.All;
		private string _searchCriteria = string.Empty;
		private SearchBy _searchBy = SearchBy.ClientName;
		private bool _needRefresh = false;
		private bool _isUpdatingSelectAll = false;
		private Timer? _searchDebounceTimer;
		private const int SearchDebounceMs = 300;
		private readonly SemaphoreSlim _filterSemaphore = new SemaphoreSlim(1, 1);

		[ObservableProperty] private ObservableCollection<InvoiceGroupItemViewModel> _flatInvoiceList = new();
		[ObservableProperty] private ObservableCollection<string> _dateRangeOptions = new() { "All", "1-30", "31-60", "61-90", "90+" };
		[ObservableProperty] private string _selectedDateRange = "All";
		[ObservableProperty] private string _searchQuery = string.Empty;
		[ObservableProperty] private bool _isSearchVisible;
		[ObservableProperty] private bool _showButtonsLayout;
		[ObservableProperty] private bool _showSelectAllLayout;
		[ObservableProperty] private bool _isSelectAllChecked;
		[ObservableProperty] private string _selectAllText = "Select All";
		[ObservableProperty] private string _totalText = string.Empty;
		[ObservableProperty] private bool _showTotal;
		[ObservableProperty] private bool _showPaymentButton = true;

		public InvoicesPageViewModel(DialogService dialogService, ILaceupAppService appService)
		{
			_dialogService = dialogService;
			_appService = appService;
		}

		// Cleanup resources
		~InvoicesPageViewModel()
		{
			_searchDebounceTimer?.Dispose();
			_filterSemaphore?.Dispose();
		}

		public async Task OnAppearingAsync()
		{
			RefreshUI();
		}

		partial void OnSelectedDateRangeChanged(string value)
		{
			if (string.IsNullOrEmpty(value))
				return;

			_whatIsVisible = value switch
			{
				"All" => SelectedOption.All,
				"1-30" => SelectedOption.From1To30,
				"31-60" => SelectedOption.From31to60,
				"61-90" => SelectedOption.From61to90,
				"90+" => SelectedOption.Plus90,
				_ => SelectedOption.All
			};

			SelectedInvoices.Clear();
			RefreshListHeader();
			Filter();
		}

		partial void OnSearchQueryChanged(string value)
		{
			var newCriteria = value ?? string.Empty;
			if (_searchCriteria != newCriteria)
			{
				_searchCriteria = newCriteria;
				
				// Debounce search to prevent filtering on every keystroke
				_searchDebounceTimer?.Dispose();
				_searchDebounceTimer = new Timer(_ =>
				{
					MainThread.BeginInvokeOnMainThread(() =>
					{
						Filter();
					});
				}, null, SearchDebounceMs, Timeout.Infinite);
			}
		}

		[RelayCommand]
		private async Task SearchMenu()
		{
			var options = new[] { "Client Name", "Invoice Number" };
			var choice = await _dialogService.ShowActionSheetAsync("Search By", "", "Cancel", options);
			
			if (choice == "Client Name")
				_searchBy = SearchBy.ClientName;
			else if (choice == "Invoice Number")
				_searchBy = SearchBy.InvoiceNum;

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
					// Select all invoices
					var toShow = GetCurrentDictionary();
					if (toShow != null)
					{
						var invoicesToAdd = new List<Invoice>();
						foreach (var item in toShow)
							invoicesToAdd.AddRange(item.Value);

						if (!string.IsNullOrEmpty(_searchCriteria))
						{
							if (_searchBy == SearchBy.ClientName)
								invoicesToAdd = invoicesToAdd.Where(x => x.Client.ClientName.ToLowerInvariant().Contains(_searchCriteria.ToLowerInvariant())).ToList();
							else
								invoicesToAdd = invoicesToAdd.Where(x => x.InvoiceNumber.ToLowerInvariant().Contains(_searchCriteria.ToLowerInvariant())).ToList();
						}

						SelectedInvoices.Clear();
						SelectedInvoices.AddRange(invoicesToAdd);
					}
				}
				else
				{
					// Clear all selections
					SelectedInvoices.Clear();
				}

				// Update all invoice items' selection state (skip handler to prevent recursive calls)
				foreach (var flatItem in FlatInvoiceList.Where(x => !x.IsGroupHeader && x.InvoiceItem != null))
				{
					var shouldBeSelected = SelectedInvoices.Contains(flatItem.InvoiceItem.Invoice);
					flatItem.InvoiceItem.SetIsSelected(shouldBeSelected, skipHandler: true);
				}
				
				RefreshListHeader();
				// Don't update IsSelectAllChecked here - it's already set by the user's click
				// Just verify it matches the actual state
				var totalInvoices = FlatInvoiceList.Count(x => !x.IsGroupHeader);
				var shouldBeChecked = totalInvoices > 0 && SelectedInvoices.Count == totalInvoices;
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
			if (SelectedInvoices.Count == 0)
			{
				await _dialogService.ShowAlertAsync("Select invoices to be paid.", "Alert", "OK");
				return;
			}

			if (Config.BlockMultipleCollectPaymets && SelectedInvoices.Count > 1)
			{
				await _dialogService.ShowAlertAsync("One invoice per payment.", "Alert", "OK");
				return;
			}

			var first = SelectedInvoices[0];
			if (!SelectedInvoices.All(x => x.Client.ClientId == first.Client.ClientId))
			{
				await _dialogService.ShowAlertAsync("Cannot pay invoices from different clients.", "Alert", "OK");
				return;
			}

			if (Config.ShowInvoicesCreditsInPayments)
			{
				var selectedCredits = SelectedInvoices.Where(x => x.Amount < 0);
				foreach (var inv in selectedCredits)
				{
					var payments = InvoicePayment.List.FirstOrDefault(x => x.Invoices().Any(x => inv.InvoiceId == x.InvoiceId));
					if (payments != null)
					{
						await _dialogService.ShowAlertAsync("Cannot pay invoice twice.", "Alert", "OK");
						return;
					}
				}
			}

			if (SelectedInvoices.Any(x => x.Paid > 0))
			{
				await _dialogService.ShowAlertAsync("Cannot pay invoice twice.", "Alert", "OK");
				return;
			}

			var sb = new System.Text.StringBuilder();
			for (int index = 0; index < SelectedInvoices.Count; index++)
			{
				if (sb.Length > 0)
					sb.Append(",");
				var value = Config.SavePaymentsByInvoiceNumber ? SelectedInvoices[index].InvoiceNumber.ToString() : SelectedInvoices[index].InvoiceId.ToString();
				sb.Append(value);
			}

			if (sb.Length > 0)
			{
				// Navigate to payment page when created
				var route = CompanyInfo.ShowNewPayments() ? "//paymentnew" : "//payment";
				await Shell.Current.GoToAsync($"{route}?invoiceIds={sb}");
			}
		}

		[RelayCommand]
		private async Task Print()
		{
			if (SelectedInvoices.Count == 0)
			{
				await _dialogService.ShowAlertAsync("Select invoices to be printed.", "Alert", "OK");
				return;
			}

			try
			{
				PrinterProvider.PrintDocument((int copies) =>
				{
					IPrinter printer = PrinterProvider.CurrentPrinter();
					bool allWent = true;

					foreach (var invoice in SelectedInvoices)
					{
						for (int i = 0; i < copies; i++)
						{
							if (!printer.PrintOpenInvoice(invoice))
								allWent = false;
						}
					}

					if (!allWent)
						return "Error printing invoices";
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
			if (SelectedInvoices.Count == 0)
			{
				await _dialogService.ShowAlertAsync("Select invoices to be sent.", "Alert", "OK");
				return;
			}

			try
			{
				// Use EmailHelper to send invoices by email (matches Xamarin PdfHelper.SendInvoicesByEmail)
				PdfHelper.SendInvoicesByEmail(SelectedInvoices);
			}
			catch (Exception ex)
			{
				await _dialogService.ShowAlertAsync($"Error sending: {ex.Message}", "Error", "OK");
				_appService.TrackError(ex);
			}
		}

		private void RefreshUI()
		{
			if (!DataAccess.CanUseApplication() || !DataAccess.ReceivedData)
			{
			IsSearchVisible = false;
			ShowButtonsLayout = false;
			ShowSelectAllLayout = false;
			FlatInvoiceList.Clear();
				return;
			}

			IsSearchVisible = true;
			ShowButtonsLayout = true;
			ShowPaymentButton = Config.PaymentAvailable && !Config.HidePriceInTransaction;
			ShowTotal = !Config.HideOpenInvoiceTotal;

			if (Config.dataDownloaded)
				_needRefresh = true;

			if (_needRefresh)
			{
				Invoice.AllOpenAmount = 0;
				Invoice.Over1Amount = 0;
				Invoice.Over30Amount = 0;
				Invoice.Over60Amount = 0;
				Invoice.Over90Amount = 0;

				_all.Clear();
				_from1To30.Clear();
				_from31To60.Clear();
				_from61To90.Clear();
				_over90.Clear();
			}

			Config.dataDownloaded = false;
			_needRefresh = false;
			_whatIsVisible = SelectedOption.All;
			SelectedDateRange = "All";
			Filter();
		}

		private void Filter()
		{
			// Run filtering on background thread for better performance
			Task.Run(async () =>
			{
				await _filterSemaphore.WaitAsync();
				try
				{
					Dictionary<Client, List<Invoice>> toShow = null;

					switch (_whatIsVisible)
					{
				case SelectedOption.All:
					if (_all.Count == 0)
					{
						_allAmount = 0;
						foreach (var inv in Invoice.OpenInvoices)
						{
							if (inv.Balance != 0)
							{
								var key = _all.Keys.FirstOrDefault(x => x.ClientId == inv.Client.ClientId);
								if (key == null)
								{
									_all.Add(inv.Client, new List<Invoice>());
									key = inv.Client;
								}
								_all[key].Add(inv);
								_allAmount += inv.Balance;
							}
						}
						foreach (var item in _all.ToList())
						{
							_all[item.Key] = item.Value.OrderBy(x => x.Date).ToList();
						}
					}
					toShow = _all;
					break;

				case SelectedOption.From1To30:
					if (_from1To30.Count == 0)
					{
						_from1To30Amount = 0;
						foreach (var inv in Invoice.OpenInvoices)
						{
							if (inv.Balance != 0)
							{
								var days = DateTime.Today.Subtract(inv.DueDate).Days;
								if (days > 0 && days < 31)
								{
									var key = _from1To30.Keys.FirstOrDefault(x => x.ClientId == inv.Client.ClientId);
									if (key == null)
									{
										_from1To30.Add(inv.Client, new List<Invoice>());
										key = inv.Client;
									}
									_from1To30[key].Add(inv);
									_from1To30Amount += inv.Balance;
								}
							}
						}
						foreach (var item in _from1To30.ToList())
						{
							_from1To30[item.Key] = item.Value.OrderBy(x => x.Date).ToList();
						}
					}
					toShow = _from1To30;
					break;

				case SelectedOption.From31to60:
					if (_from31To60.Count == 0)
					{
						_from31To60Amount = 0;
						foreach (var inv in Invoice.OpenInvoices)
						{
							if (inv.Balance != 0)
							{
								var days = DateTime.Today.Subtract(inv.DueDate).Days;
								if (days > 30 && days < 61)
								{
									var key = _from31To60.Keys.FirstOrDefault(x => x.ClientId == inv.Client.ClientId);
									if (key == null)
									{
										_from31To60.Add(inv.Client, new List<Invoice>());
										key = inv.Client;
									}
									_from31To60[key].Add(inv);
									_from31To60Amount += inv.Balance;
								}
							}
						}
						foreach (var item in _from31To60.ToList())
						{
							_from31To60[item.Key] = item.Value.OrderBy(x => x.Date).ToList();
						}
					}
					toShow = _from31To60;
					break;

				case SelectedOption.From61to90:
					if (_from61To90.Count == 0)
					{
						_from61To90Amount = 0;
						foreach (var inv in Invoice.OpenInvoices)
						{
							if (inv.Balance != 0)
							{
								var days = DateTime.Today.Subtract(inv.DueDate).Days;
								if (days > 60 && days < 91)
								{
									var key = _from61To90.Keys.FirstOrDefault(x => x.ClientId == inv.Client.ClientId);
									if (key == null)
									{
										_from61To90.Add(inv.Client, new List<Invoice>());
										key = inv.Client;
									}
									_from61To90[key].Add(inv);
									_from61To90Amount += inv.Balance;
								}
							}
						}
						foreach (var item in _from61To90.ToList())
						{
							_from61To90[item.Key] = item.Value.OrderBy(x => x.Date).ToList();
						}
					}
					toShow = _from61To90;
					break;

				case SelectedOption.Plus90:
					if (_over90.Count == 0)
					{
						_over90Amount = 0;
						foreach (var inv in Invoice.OpenInvoices)
						{
							if (inv.Balance != 0)
							{
								var days = DateTime.Today.Subtract(inv.DueDate).Days;
								if (days > 90)
								{
									var key = _over90.Keys.FirstOrDefault(x => x.ClientId == inv.Client.ClientId);
									if (key == null)
									{
										_over90.Add(inv.Client, new List<Invoice>());
										key = inv.Client;
									}
									_over90[key].Add(inv);
									_over90Amount += inv.Balance;
								}
							}
						}
						foreach (var item in _over90.ToList())
						{
							_over90[item.Key] = item.Value.OrderBy(x => x.Date).ToList();
						}
					}
					toShow = _over90;
					break;
			}

					if (toShow != null)
					{
						if (!string.IsNullOrEmpty(_searchCriteria))
						{
							var searchLower = _searchCriteria.ToLowerInvariant();
							if (_searchBy == SearchBy.ClientName)
								toShow = new Dictionary<Client, List<Invoice>>(toShow.Where(x => x.Key.ClientName.ToLowerInvariant().Contains(searchLower)));
							else
								toShow = new Dictionary<Client, List<Invoice>>(toShow.Where(x => x.Value.Any(y => y.InvoiceNumber.ToLowerInvariant().Contains(searchLower))));
						}

						// Update UI on main thread
						MainThread.BeginInvokeOnMainThread(() =>
						{
							RefreshClientGroups(toShow);
						});
					}
				}
				finally
				{
					_filterSemaphore.Release();
				}
			});
		}

		private void RefreshClientGroups(Dictionary<Client, List<Invoice>> source = null)
		{
			if (source == null)
				source = GetCurrentDictionary() ?? new Dictionary<Client, List<Invoice>>();

			// Build flat list with group headers for better performance (no nested CollectionViews)
			var flatList = new List<InvoiceGroupItemViewModel>();
			foreach (var kvp in source)
			{
				// Add group header
				flatList.Add(new InvoiceGroupItemViewModel { IsGroupHeader = true, ClientName = kvp.Key.ClientName });
				
				// Add invoice items
				foreach (var invoice in kvp.Value)
				{
					flatList.Add(new InvoiceGroupItemViewModel 
					{ 
						IsGroupHeader = false, 
						InvoiceItem = new InvoiceListItemViewModel(invoice, this) 
					});
				}
			}

			// Replace entire collection at once for smoother UI update
			FlatInvoiceList.Clear();
			foreach (var item in flatList)
			{
				FlatInvoiceList.Add(item);
			}

			ShowSelectAllLayout = FlatInvoiceList.Any(x => !x.IsGroupHeader);
			UpdateSelectAllState();
		}

		private void UpdateSelectAllState()
		{
			var totalInvoices = FlatInvoiceList.Count(x => !x.IsGroupHeader);
			var shouldBeChecked = totalInvoices > 0 && SelectedInvoices.Count == totalInvoices;
			if (IsSelectAllChecked != shouldBeChecked)
			{
				IsSelectAllChecked = shouldBeChecked;
			}
		}

		private Dictionary<Client, List<Invoice>> GetCurrentDictionary()
		{
			return _whatIsVisible switch
			{
				SelectedOption.All => _all,
				SelectedOption.From1To30 => _from1To30,
				SelectedOption.From31to60 => _from31To60,
				SelectedOption.From61to90 => _from61To90,
				SelectedOption.Plus90 => _over90,
				_ => _all
			};
		}

		private void RefreshListHeader()
		{
			if (Config.ShowInvoicesCreditsInPayments)
			{
				double realBalance = 0;
				foreach (var inv in SelectedInvoices)
				{
					var payments = InvoicePayment.List.FirstOrDefault(x => x.Invoices().Any(x => inv.InvoiceId == x.InvoiceId));
					if (payments != null && payments.Invoices().Any(x => x.Amount < 0))
						continue;
					realBalance += inv.Balance;
				}

				if (SelectedInvoices.Count > 0)
				{
					SelectAllText = $"Selected Invoices: {SelectedInvoices.Count}";
					TotalText = $"Total: {realBalance.ToCustomString()}";
					ShowTotal = true;
				}
				else
				{
					SelectAllText = "Select All";
					TotalText = string.Empty;
					ShowTotal = false;
				}
			}
			else
			{
				if (SelectedInvoices.Count > 0)
				{
					SelectAllText = $"Selected Invoices: {SelectedInvoices.Count}";
					TotalText = $"Total: {SelectedInvoices.Sum(x => x.Balance).ToCustomString()}";
					ShowTotal = true;
				}
				else
				{
					SelectAllText = "Select All";
					TotalText = string.Empty;
					ShowTotal = false;
				}
			}
		}

		public List<Invoice> SelectedInvoices { get; } = new();

		public void ToggleInvoiceSelection(Invoice invoice)
		{
			if (!SelectedInvoices.Contains(invoice))
				SelectedInvoices.Add(invoice);
			else
				SelectedInvoices.Remove(invoice);

			RefreshListHeader();
			UpdateSelectAllState();
			
			// Update the invoice item's selection state (skip handler to prevent infinite loop)
			// Only update if the value is actually different
			foreach (var flatItem in FlatInvoiceList.Where(x => !x.IsGroupHeader && x.InvoiceItem != null && x.InvoiceItem.Invoice == invoice))
			{
				var shouldBeSelected = SelectedInvoices.Contains(invoice);
				if (flatItem.InvoiceItem.IsSelected != shouldBeSelected)
				{
					flatItem.InvoiceItem.SetIsSelected(shouldBeSelected, skipHandler: true);
				}
				break;
			}
		}
	}

	// Flat list item that can be either a group header or an invoice item
	public partial class InvoiceGroupItemViewModel : ObservableObject
	{
		[ObservableProperty] private bool _isGroupHeader;
		[ObservableProperty] private string _clientName = string.Empty;
		[ObservableProperty] private InvoiceListItemViewModel? _invoiceItem;
	}

	public partial class InvoiceListItemViewModel : ObservableObject
	{
		private readonly Invoice _invoice;
		private readonly InvoicesPageViewModel _parent;
		private bool _isUpdatingSelection = false;

		[ObservableProperty] private bool _isSelected;

		public InvoiceListItemViewModel(Invoice invoice, InvoicesPageViewModel parent)
		{
			_invoice = invoice;
			_parent = parent;
			UpdateProperties();
		}

		public string InvoiceNumberText
		{
			get
			{
				var label = _invoice.InvoiceType switch
				{
					1 => "Credit:",
					2 => "Quote:",
					3 => "Sales Order:",
					4 => "Credit Invoice:",
					_ => "Invoice:"
				};
				return $"{label} {_invoice.InvoiceNumber}";
			}
		}

		public string DateText => $"Date: {_invoice.Date.ToShortDateString()}";
		public string TotalText => $"Total: {_invoice.Amount.ToCustomString()}";

		// Cached values to avoid expensive recalculations during scrolling
		private string? _cachedOpenBalanceText;
		private Color? _cachedOpenBalanceColor;
		private bool _isFullyPaid;
		private double _openBalance;

		public string OpenBalanceText
		{
			get
			{
				if (_cachedOpenBalanceText == null)
				{
					CalculateOpenBalance();
					_cachedOpenBalanceText = $"Open: {_openBalance.ToCustomString()}";
				}
				return _cachedOpenBalanceText;
			}
		}

		public Color OpenBalanceColor
		{
			get
			{
				if (_cachedOpenBalanceColor == null)
				{
					// Ensure CalculateOpenBalance is called if not already cached
					if (_cachedOpenBalanceText == null)
						CalculateOpenBalance();
					
					if (_openBalance == 0)
						_cachedOpenBalanceColor = Colors.Green;
					else if (_invoice.DueDate < DateTime.Today)
						_cachedOpenBalanceColor = Colors.Red;
					else
						_cachedOpenBalanceColor = Colors.Black;
				}
				return _cachedOpenBalanceColor;
			}
		}

		private void CalculateOpenBalance()
		{
			_isFullyPaid = false;
			if (Config.ShowInvoicesCreditsInPayments)
			{
				// Optimize: cache the invoice ID to avoid repeated property access
				var invoiceId = _invoice.InvoiceId;
				var payments = InvoicePayment.List.FirstOrDefault(x => x.Invoices().Any(x => x.InvoiceId == invoiceId));
				if (payments != null && payments.Invoices().Any(x => x.Amount < 0))
					_isFullyPaid = true;
			}

			_openBalance = _isFullyPaid ? 0 : _invoice.Balance - _invoice.Paid;
		}

		public bool ShowAmounts => !Config.HidePriceInTransaction;
		public bool ShowDiscount { get; private set; }
		public string DiscountText { get; private set; } = string.Empty;
		public bool ShowGoalInfo { get; private set; }
		public string GoalPaymentText { get; private set; } = string.Empty;
		public string PendingDaysText { get; private set; } = string.Empty;

		private void UpdateProperties()
		{
			// Update discount visibility
			bool isDiscountVisible = false;
			if (_invoice.Client != null && _invoice.Client.TermId > 0)
			{
				var term = Term.List.Where(x => x.IsActive).FirstOrDefault(x => x.Id == _invoice.Client.TermId);
				if (term != null)
				{
					var daysRemainingForDiscount = Math.Abs((_invoice.Date - DateTime.Now.Date).TotalDays);
					if (term.StandardDiscountDays >= daysRemainingForDiscount && term.DiscountPercentage > 0)
					{
						var discountAmount = _invoice.Balance * (term.DiscountPercentage / 100);
						DiscountText = $"Apply for {term.DiscountPercentage}% Total: {discountAmount.ToCustomString()}";
						isDiscountVisible = true;
					}
				}
			}

			if (!Config.UsePaymentDiscount)
				isDiscountVisible = false;

			bool isCredit = _invoice != null && (_invoice.InvoiceType == 1 || _invoice.InvoiceType == 4);
			ShowDiscount = isDiscountVisible && !isCredit;

			// Update goal info
			if (Config.ViewGoals && GoalDetailDTO.List.Count > 0)
			{
				var details = GoalDetailDTO.List.Where(x => x.Goal != null && x.Goal.Criteria == GoalCriteria.Payment && x.ExternalInvoice != null).ToList();
				var d = details.FirstOrDefault(x => x.ExternalInvoice == _invoice);
				if (d != null)
				{
					ShowGoalInfo = true;
					GoalPaymentText = $"Payment Goal: {d.QuantityOrAmountValue.ToCustomString()}";
					PendingDaysText = $"Pending Days: {d.WorkingDays - d.WorkedDays}";
				}
				else
				{
					ShowGoalInfo = false;
				}
			}
			else
			{
				ShowGoalInfo = false;
			}

			// Update selection state (skip handler during initialization)
			SetIsSelected(_parent.SelectedInvoices.Contains(_invoice), skipHandler: true);
		}

		partial void OnIsSelectedChanged(bool value)
		{
			if (!_isUpdatingSelection)
			{
				_parent.ToggleInvoiceSelection(_invoice);
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
			await Shell.Current.GoToAsync($"invoicedetails?invoiceId={_invoice.InvoiceId}");
		}

		public Invoice Invoice => _invoice;
	}
}

