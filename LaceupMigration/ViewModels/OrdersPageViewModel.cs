using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System;
using System.Globalization;

namespace LaceupMigration.ViewModels
{
	public partial class OrdersPageViewModel : ObservableObject
	{
		private enum SelectedOption
		{
			All = 0,
			Sales_Order = 1,
			Credit_Order = 2,
			Return_Order = 3,
			Quote = 4,
			Sales_Invoice = 5,
			Credit_Invoice = 6,
			Return_Invoice = 7,
			Consignment_Invoice = 8,
			ParLevel_Invoice = 9,
			No_Service = 10
		}

		internal enum SearchBy
		{
			ClientName = 0,
			InvoiceNum = 1
		}

		private readonly DialogService _dialogService;
		private readonly ILaceupAppService _appService;

		private Dictionary<string, TransactionSection> _transactions = new();
		private int _transactionCount = 0;
		private SelectedOption _whatIsVisible = SelectedOption.All;
		private string _searchCriteria = string.Empty;
		private SearchBy _searchBy = SearchBy.ClientName;

		[ObservableProperty] private ObservableCollection<TransactionSectionViewModel> _transactionSections = new();
		[ObservableProperty] private ObservableCollection<string> _transactionTypeOptions = new();
		[ObservableProperty] private string _selectedTransactionType = "All";
		[ObservableProperty] private string _searchQuery = string.Empty;
		[ObservableProperty] private bool _isSearchVisible;
		[ObservableProperty] private bool _showButtonsLayout;
		[ObservableProperty] private bool _showSelectAllLayout;
		[ObservableProperty] private bool _isSelectAllChecked;
		[ObservableProperty] private string _selectAllText = "Select All";
		[ObservableProperty] private string _totalText = string.Empty;
		[ObservableProperty] private bool _showTotal;
		[ObservableProperty] private bool _showDexButton;

		public OrdersPageViewModel(DialogService dialogService, ILaceupAppService appService)
		{
			_dialogService = dialogService;
			_appService = appService;
			BuildTransactionTypeOptions();
		}

		private void BuildTransactionTypeOptions()
		{
			var options = new List<string> { "All" };

			if (Config.PreSale)
			{
				options.Add("Sales Order");
				if (Config.AllowCreditOrders)
				{
					options.Add("Credit Order");
					if (Config.UseReturnInvoice)
						options.Add("Return Order");
				}
			}

			if (Config.UseQuote)
				options.Add("Quote");

			options.Add("Sales Invoice");

			if (Config.AllowCreditOrders)
			{
				options.Add("Credit Invoice");
				if (Config.UseReturnInvoice)
					options.Add("Return Invoice");
			}

			if (Config.Consignment)
				options.Add("Consignment");

			if (Config.ClientDailyPL)
				options.Add("Par Level Invoice");

			options.Add("No Service");

			TransactionTypeOptions = new ObservableCollection<string>(options);
			ShowDexButton = Config.DexAvailable;
		}

		public async Task OnAppearingAsync()
		{
			RefreshUI();
		}

		partial void OnSelectedTransactionTypeChanged(string value)
		{
			if (string.IsNullOrEmpty(value))
				return;

			_whatIsVisible = value switch
			{
				"All" => SelectedOption.All,
				"Sales Order" => SelectedOption.Sales_Order,
				"Credit Order" => SelectedOption.Credit_Order,
				"Return Order" => SelectedOption.Return_Order,
				"Quote" => SelectedOption.Quote,
				"Sales Invoice" => SelectedOption.Sales_Invoice,
				"Credit Invoice" => SelectedOption.Credit_Invoice,
				"Return Invoice" => SelectedOption.Return_Invoice,
				"Consignment" => SelectedOption.Consignment_Invoice,
				"Par Level Invoice" => SelectedOption.ParLevel_Invoice,
				"No Service" => SelectedOption.No_Service,
				_ => SelectedOption.All
			};

			SelectedOrders.Clear();
			RefreshListHeader();
			Filter();
		}

		partial void OnSearchQueryChanged(string value)
		{
			if (_searchCriteria != value)
			{
				_searchCriteria = value;
				Filter();
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
			if (!IsSelectAllChecked)
			{
				SelectedOrders.Clear();
			}
			else
			{
				var ordersToAdd = new List<Order>();
				var sections = GetCurrentSections();
				foreach (var section in sections.Values)
				{
					ordersToAdd.AddRange(section.GetOrders());
				}

				if (!string.IsNullOrEmpty(_searchCriteria))
				{
					if (_searchBy == SearchBy.ClientName)
						ordersToAdd = ordersToAdd.Where(x => x.Client.ClientName.ToLowerInvariant().Contains(_searchCriteria.ToLowerInvariant())).ToList();
					else
						ordersToAdd = ordersToAdd.Where(x => !string.IsNullOrEmpty(x.PrintedOrderId) && x.PrintedOrderId.ToLowerInvariant().Contains(_searchCriteria.ToLowerInvariant())).ToList();
				}

				SelectedOrders.Clear();
				SelectedOrders.AddRange(ordersToAdd);
			}

			RefreshListHeader();
			UpdateSelectAllState();
			RefreshTransactionSections();
		}

		[RelayCommand]
		private async Task Print()
		{
			if (SelectedOrders.Count == 0)
			{
				await _dialogService.ShowAlertAsync("Select transactions to be printed.", "Alert", "OK");
				return;
			}

			// TODO: Implement print functionality
			await _dialogService.ShowAlertAsync("Print functionality to be implemented.", "Info", "OK");
		}

		[RelayCommand]
		private async Task Dex()
		{
			if (SelectedOrders.Count == 0)
			{
				await _dialogService.ShowAlertAsync("Select valid orders for DEX.", "Alert", "OK");
				return;
			}

			if (SelectedOrders.Any(x => x.Voided))
			{
				await _dialogService.ShowAlertAsync("Cannot DEX voided orders.", "Alert", "OK");
				return;
			}

			if (Config.ItemGroupedTemplate && SelectedOrders.Any(x => !x.ReadyToFinalize))
			{
				await _dialogService.ShowAlertAsync("All orders must be finalized before DEX.", "Alert", "OK");
				return;
			}

			try
			{
				var orders = SelectedOrders.Where(x => !x.Dexed).ToList();
				await DoDexAsync(orders);
			}
			catch (Exception ex)
			{
				await _dialogService.ShowAlertAsync("Error DEXing orders.", "Alert", "OK");
				Logger.CreateLog(ex);
			}
		}

		private async Task DoDexAsync(List<Order> orders)
		{
			try
			{
				if (Config.AskDEXUoM)
				{
					var uoms = new[] { "Each", "Case" };
					var choice = await _dialogService.ShowActionSheetAsync("Select UoM", "", "Cancel", uoms);
					if (string.IsNullOrEmpty(choice) || choice == "Cancel")
						return;

					var uom = choice == "Each" ? "EA" : "CA";
					var order = orders[0];
					SetCompanyForOrder(order);
					var dex = Order.DexUs(orders, uom);
					await CallDexAsync(dex);
				}
				else
				{
					var order = orders[0];
					SetCompanyForOrder(order);
					
					string errorMsg = string.Empty;
					if (!ValidateOrdersForDex(orders, ref errorMsg))
					{
						await _dialogService.ShowAlertAsync(errorMsg, "Warning", "OK");
						return;
					}

					var dex = Order.DexUs(orders);
					await CallDexAsync(dex);
				}
			}
			catch (Exception ex)
			{
				Logger.CreateLog(ex.ToString());
			}
		}

		private void SetCompanyForOrder(Order order)
		{
			if (order.CompanyId != 0)
				CompanyInfo.SelectedCompany = CompanyInfo.Companies.FirstOrDefault(c => c.CompanyId == order.CompanyId);
			else
				CompanyInfo.SelectedCompany = CompanyInfo.Companies.FirstOrDefault(c => c.CompanyName == order.CompanyName);

			if (CompanyInfo.SelectedCompany == null)
				CompanyInfo.SelectedCompany = CompanyInfo.GetMasterCompany();
		}

		private bool ValidateOrdersForDex(List<Order> orders, ref string errMsg)
		{
			if (orders.Any(x => string.IsNullOrEmpty(x.Client.DUNS)))
			{
				errMsg = "All clients must have a DUNS number for DEX.";
				return false;
			}

			foreach (var order in orders)
			{
				foreach (var detail in order.Details)
				{
					if (string.IsNullOrEmpty(detail.Product.Upc) || detail.Product.Upc.Length > Config.DexUpcCharacterLimits)
					{
						errMsg = $"Product {detail.Product.Name} has an invalid UPC for DEX.";
						return false;
					}
				}
			}

			var check = CheckIfCurrencyIsUSD();
			if (!string.IsNullOrEmpty(check))
			{
				errMsg = check;
				return false;
			}

			return true;
		}

		private string CheckIfCurrencyIsUSD()
		{
			var cultureInfo = CultureInfo.CurrentCulture;
			var regionInfo = new RegionInfo(cultureInfo.Name);
			if (regionInfo.ISOCurrencySymbol == "USD")
				return string.Empty;
			else
				return $"DEX requires USD currency. Current currency: {regionInfo.CurrencyEnglishName} ({regionInfo.ISOCurrencySymbol}).";
		}

		private async Task CallDexAsync(string dex)
		{
			// TODO: Implement DEX call - this is platform-specific Android functionality
			await _dialogService.ShowAlertAsync("DEX functionality requires platform-specific implementation.", "Info", "OK");
		}

		[RelayCommand]
		private async Task Send()
		{
			if (SelectedOrders.Count == 0)
			{
				await _dialogService.ShowAlertAsync("Select transactions to be sent.", "Alert", "OK");
				return;
			}

			if (Config.MustCreatePaymentDeposit)
			{
				await SendByEmailAsync();
				return;
			}

			var options = new[] { "Send to Back Office", "Send by Email" };
			var choice = await _dialogService.ShowActionSheetAsync("Send Options", "", "Cancel", options);

			if (choice == "Send to Back Office")
				await SendToBackOfficeAsync();
			else if (choice == "Send by Email")
				await SendByEmailAsync();
		}

		private async Task SendByEmailAsync()
		{
			if (SelectedOrders.Count == 0)
			{
				await _dialogService.ShowAlertAsync("Select transactions to be sent.", "Alert", "OK");
				return;
			}

			// TODO: Implement send by email functionality
			await _dialogService.ShowAlertAsync("Send by email functionality to be implemented.", "Info", "OK");
		}

		private async Task SendToBackOfficeAsync()
		{
			if (SelectedOrders.Count == 0)
			{
				await _dialogService.ShowAlertAsync("Select transactions to be sent.", "Alert", "OK");
				return;
			}

			var presaleOrders = SelectedOrders.Where(x => x.AsPresale).ToList();

			if (Config.CheckIfShipdateLocked && presaleOrders.Count > 0)
			{
				var lockedDates = new List<DateTime>();
				if (!DataAccess.CheckIfShipdateIsValid(presaleOrders.Select(x => x.ShipDate).ToList(), ref lockedDates))
				{
					var sb = string.Empty;
					foreach (var l in lockedDates)
						sb += '\n' + l.Date.ToShortDateString();
					await _dialogService.ShowAlertAsync($"The selected dates are currently locked\n{sb}\n\nPlease select a different shipdate", "Alert", "OK");
					return;
				}
			}

			if (SelectedOrders.Any(x => !x.AsPresale))
			{
				var result = await _dialogService.ShowConfirmationAsync("Send orders without invoices?", "Warning", "Yes", "No");
				if (!result)
					return;
			}

			// TODO: Implement send to back office functionality
			await _dialogService.ShowAlertAsync("Send to back office functionality to be implemented.", "Info", "OK");
		}

		private void RefreshUI()
		{
			if (!DataAccess.CanUseApplication() || !DataAccess.ReceivedData)
			{
				IsSearchVisible = false;
				ShowButtonsLayout = false;
				ShowSelectAllLayout = false;
				TransactionSections.Clear();
				return;
			}

			IsSearchVisible = true;
			ShowButtonsLayout = true;
			ShowTotal = !Config.HideTransactionsTotal && !Config.HidePriceInTransaction;

			Filter();
		}

		private void Filter()
		{
			if (_transactions.Count == 0 || _transactions.Sum(x => x.Value.Count) != Order.Orders.Count)
			{
				_transactions.Clear();
				_transactionCount = 0;

				foreach (var item in Order.Orders)
				{
					string name = GetTransactionTypeName(item);
					if (!string.IsNullOrEmpty(name))
					{
						if (!_transactions.ContainsKey(name))
							_transactions.Add(name, new TransactionSection());

						_transactions[name].Add(item);
					}
				}

				_transactionCount = Order.Orders.Count;
			}

			Dictionary<string, TransactionSection> toShow = new();

			switch (_whatIsVisible)
			{
				case SelectedOption.All:
					toShow = _transactions;
					break;
				case SelectedOption.Sales_Order:
					if (_transactions.ContainsKey("Sales Order"))
						toShow.Add("Sales Order", _transactions["Sales Order"]);
					break;
				case SelectedOption.Credit_Order:
					if (_transactions.ContainsKey("Credit Order"))
						toShow.Add("Credit Order", _transactions["Credit Order"]);
					break;
				case SelectedOption.Return_Order:
					if (_transactions.ContainsKey("Return Order"))
						toShow.Add("Return Order", _transactions["Return Order"]);
					break;
				case SelectedOption.Quote:
					if (_transactions.ContainsKey("Quote"))
						toShow.Add("Quote", _transactions["Quote"]);
					break;
				case SelectedOption.Sales_Invoice:
					if (_transactions.ContainsKey("Sales Invoice"))
						toShow.Add("Sales Invoice", _transactions["Sales Invoice"]);
					break;
				case SelectedOption.Credit_Invoice:
					if (_transactions.ContainsKey("Credit Invoice"))
						toShow.Add("Credit Invoice", _transactions["Credit Invoice"]);
					break;
				case SelectedOption.Return_Invoice:
					if (_transactions.ContainsKey("Return Invoice"))
						toShow.Add("Return Invoice", _transactions["Return Invoice"]);
					break;
				case SelectedOption.Consignment_Invoice:
					if (_transactions.ContainsKey("Consignment"))
						toShow.Add("Consignment", _transactions["Consignment"]);
					break;
				case SelectedOption.ParLevel_Invoice:
					if (_transactions.ContainsKey("Par Level Invoice"))
						toShow.Add("Par Level Invoice", _transactions["Par Level Invoice"]);
					break;
				case SelectedOption.No_Service:
					if (_transactions.ContainsKey("No Service"))
						toShow.Add("No Service", _transactions["No Service"]);
					break;
			}

			foreach (var item in toShow.ToList())
			{
				item.Value.Filter(_searchCriteria, _searchBy);
				if (item.Value.Count == 0)
					toShow.Remove(item.Key);
			}

			RefreshTransactionSections(toShow);
		}

		private string GetTransactionTypeName(Order order)
		{
			if (order.AsPresale)
			{
				if (order.OrderType == OrderType.Order)
				{
					if (order.IsQuote)
						return "Quote";
					else
						return "Sales Order";
				}
				else if (order.OrderType == OrderType.Credit)
					return "Credit Order";
				else if (order.OrderType == OrderType.Return)
					return "Return Order";
				else if (order.OrderType == OrderType.NoService)
					return "No Service";
			}
			else
			{
				if (order.OrderType == OrderType.Order)
				{
					if (order.IsParLevel)
						return "Par Level Invoice";
					else
						return "Sales Invoice";
				}
				else if (order.OrderType == OrderType.Credit)
					return "Credit Invoice";
				else if (order.OrderType == OrderType.Return)
					return "Return Invoice";
				else if (order.OrderType == OrderType.Consignment)
					return "Consignment";
				else if (order.OrderType == OrderType.Quote)
					return "Quote";
			}

			return string.Empty;
		}

		private void RefreshTransactionSections(Dictionary<string, TransactionSection> source = null)
		{
			if (source == null)
				source = GetCurrentSections();

			TransactionSections.Clear();
			foreach (var kvp in source)
			{
				var section = new TransactionSectionViewModel
				{
					SectionName = kvp.Key,
					SectionTotalText = $"Total: {kvp.Value.Total.ToCustomString()}",
					ShowTotal = !Config.HideTransactionsTotal && kvp.Key != "No Service" && !Config.HidePriceInTransaction
				};

				foreach (var clientGroup in kvp.Value.ToShow)
				{
					var clientGroupVm = new ClientOrderGroupViewModel
					{
						ClientName = clientGroup.Key.ClientName
					};

					foreach (var order in clientGroup.Value)
					{
						var orderItem = new OrderListItemViewModel(order, this);
						clientGroupVm.Orders.Add(orderItem);
					}

					section.ClientGroups.Add(clientGroupVm);
				}

				TransactionSections.Add(section);
			}

			ShowSelectAllLayout = TransactionSections.Count > 0;
			UpdateSelectAllState();
		}

		private Dictionary<string, TransactionSection> GetCurrentSections()
		{
			var result = new Dictionary<string, TransactionSection>();
			switch (_whatIsVisible)
			{
				case SelectedOption.All:
					result = _transactions;
					break;
				case SelectedOption.Sales_Order:
					if (_transactions.ContainsKey("Sales Order"))
						result.Add("Sales Order", _transactions["Sales Order"]);
					break;
				case SelectedOption.Credit_Order:
					if (_transactions.ContainsKey("Credit Order"))
						result.Add("Credit Order", _transactions["Credit Order"]);
					break;
				case SelectedOption.Return_Order:
					if (_transactions.ContainsKey("Return Order"))
						result.Add("Return Order", _transactions["Return Order"]);
					break;
				case SelectedOption.Quote:
					if (_transactions.ContainsKey("Quote"))
						result.Add("Quote", _transactions["Quote"]);
					break;
				case SelectedOption.Sales_Invoice:
					if (_transactions.ContainsKey("Sales Invoice"))
						result.Add("Sales Invoice", _transactions["Sales Invoice"]);
					break;
				case SelectedOption.Credit_Invoice:
					if (_transactions.ContainsKey("Credit Invoice"))
						result.Add("Credit Invoice", _transactions["Credit Invoice"]);
					break;
				case SelectedOption.Return_Invoice:
					if (_transactions.ContainsKey("Return Invoice"))
						result.Add("Return Invoice", _transactions["Return Invoice"]);
					break;
				case SelectedOption.Consignment_Invoice:
					if (_transactions.ContainsKey("Consignment"))
						result.Add("Consignment", _transactions["Consignment"]);
					break;
				case SelectedOption.ParLevel_Invoice:
					if (_transactions.ContainsKey("Par Level Invoice"))
						result.Add("Par Level Invoice", _transactions["Par Level Invoice"]);
					break;
				case SelectedOption.No_Service:
					if (_transactions.ContainsKey("No Service"))
						result.Add("No Service", _transactions["No Service"]);
					break;
			}
			return result;
		}

		private void UpdateSelectAllState()
		{
			var totalOrders = TransactionSections.Sum(s => s.ClientGroups.Sum(g => g.Orders.Count));
			IsSelectAllChecked = totalOrders > 0 && SelectedOrders.Count == totalOrders;
		}

		private void RefreshListHeader()
		{
			IsSelectAllChecked = SelectedOrders.Count > 0;
			if (IsSelectAllChecked)
			{
				SelectAllText = $"Selected Transactions: {SelectedOrders.Count}";
				TotalText = $"Total: {SelectedOrders.Sum(x => x.OrderTotalCost()).ToCustomString()}";
				ShowTotal = true;
			}
			else
			{
				SelectAllText = "Select All";
				TotalText = string.Empty;
				ShowTotal = false;
			}
		}

		public List<Order> SelectedOrders { get; } = new();

		public void ToggleOrderSelection(Order order)
		{
			if (!SelectedOrders.Contains(order))
				SelectedOrders.Add(order);
			else
				SelectedOrders.Remove(order);

			RefreshListHeader();
			UpdateSelectAllState();
			RefreshTransactionSections();
		}
	}

	public partial class TransactionSectionViewModel : ObservableObject
	{
		[ObservableProperty] private string _sectionName = string.Empty;
		[ObservableProperty] private string _sectionTotalText = string.Empty;
		[ObservableProperty] private bool _showTotal;
		[ObservableProperty] private ObservableCollection<ClientOrderGroupViewModel> _clientGroups = new();
	}

	public partial class ClientOrderGroupViewModel : ObservableObject
	{
		[ObservableProperty] private string _clientName = string.Empty;
		[ObservableProperty] private ObservableCollection<OrderListItemViewModel> _orders = new();
	}

	public partial class OrderListItemViewModel : ObservableObject
	{
		private readonly Order _order;
		private readonly OrdersPageViewModel _parent;

		[ObservableProperty] private bool _isSelected;

		public OrderListItemViewModel(Order order, OrdersPageViewModel parent)
		{
			_order = order;
			_parent = parent;
			IsSelected = _parent.SelectedOrders.Contains(_order);
		}

		public string OrderNumberText => !string.IsNullOrEmpty(_order.PrintedOrderId) ? _order.PrintedOrderId : $"Order #{_order.OrderId}";
		public string DateText => $"Date: {_order.Date.ToShortDateString()}";
		public string TotalText => _order.OrderType == OrderType.NoService ? string.Empty : $"Total: {_order.OrderTotalCost().ToCustomString()}";
		public bool ShowTotal => !Config.HidePriceInTransaction && _order.OrderType != OrderType.NoService;

		partial void OnIsSelectedChanged(bool value)
		{
			_parent.ToggleOrderSelection(_order);
		}

		[RelayCommand]
		private async Task ViewDetails()
		{
			// TODO: Navigate to order details based on order type
			await Shell.Current.GoToAsync($"//orderdetails?orderId={_order.OrderId}");
		}

		public Order Order => _order;
	}

	internal class TransactionSection
	{
		private Dictionary<Client, List<Order>> _section = new();
		public Dictionary<Client, List<Order>> ToShow = new();
		private double _total = 0;

		public void Filter(string searchCriteria, OrdersPageViewModel.SearchBy searchBy)
		{
			ToShow = _section;

			if (!string.IsNullOrEmpty(searchCriteria))
			{
				if (searchBy == OrdersPageViewModel.SearchBy.ClientName)
					ToShow = new Dictionary<Client, List<Order>>(ToShow.Where(x => x.Key.ClientName.ToLowerInvariant().Contains(searchCriteria.ToLowerInvariant())));
				else
					ToShow = new Dictionary<Client, List<Order>>(ToShow.Where(x =>
						x.Value.Any(y => !string.IsNullOrEmpty(y.PrintedOrderId) && y.PrintedOrderId.ToLowerInvariant().Contains(searchCriteria.ToLowerInvariant()))));
			}

			_total = CalculateTotal();
		}

		public void Add(Order order)
		{
			var key = _section.Keys.FirstOrDefault(x => x.ClientId == order.Client.ClientId);
			if (key == null)
			{
				_section.Add(order.Client, new List<Order>());
				key = order.Client;
			}
			_section[key].Add(order);
		}

		public int Count => ToShow.Count;
		public double Total => _total;

		private double CalculateTotal()
		{
			double total = 0;
			foreach (var item in ToShow.Values)
				foreach (var order in item)
					total += order.OrderTotalCost();
			return total;
		}

		public List<Order> GetOrders()
		{
			var result = new List<Order>();
			foreach (var item in ToShow.Values)
				result.AddRange(item);
			return result;
		}
	}
}

