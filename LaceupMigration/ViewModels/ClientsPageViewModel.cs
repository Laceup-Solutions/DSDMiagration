using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Helpers;
using LaceupMigration.Services;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls;
using MauiIcons.Material.Outlined;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LaceupMigration.ViewModels
{
	public partial class ClientsPageViewModel : ObservableObject
	{
		private enum DisplayMode
		{
			All,
			Route
		}

		private readonly DialogService _dialogService;
		private readonly ILaceupAppService _appService;
		private readonly AdvancedOptionsService _advancedOptionsService;

		private readonly List<ClientEntry> _allEntries = new();
		private CancellationTokenSource? _searchDebounceTokenSource;

		private DisplayMode _displayMode;
		private bool _initialized;
		private bool _messageDisplayed;

		public ObservableCollection<ClientListItemViewModel> Clients { get; } = new();
		public ObservableCollection<ClientGroupViewModel> ClientGroups { get; } = new();

		[ObservableProperty]
		private bool _isBusy;

		[ObservableProperty]
		private bool _useGrouping;

		[ObservableProperty]
		private bool _showFlatList = true;

		[ObservableProperty]
		private bool _showGroupedList;

		[ObservableProperty]
		private bool _isSearchVisible;

		[ObservableProperty]
		private bool _showSearchButton;

		[ObservableProperty]
		private bool _searchButtonEnabled;

		[ObservableProperty]
		private bool _showViewButton;

		[ObservableProperty]
		private bool _showSelectDayButton;

		[ObservableProperty]
		private bool _showEditButton;

		[ObservableProperty]
		private bool _showMapRouteButton;

		[ObservableProperty]
		private bool _showButtonsLayout;

		[ObservableProperty]
		private bool _isEditing;

		[ObservableProperty]
		private string _viewButtonText = "View Route";

		[ObservableProperty]
		private string _selectDayButtonText = "Other Day";

		[ObservableProperty]
		private string _editButtonText = "Edit";

		[ObservableProperty]
		private string _searchQuery = string.Empty;

		[ObservableProperty]
		private DateTime _routeDate = DateTime.Today;

		[ObservableProperty]
		private string _infoMessage = string.Empty;

		public bool HasInfoMessage => !string.IsNullOrWhiteSpace(InfoMessage);

		public ClientsPageViewModel(DialogService dialogService, ILaceupAppService appService, AdvancedOptionsService advancedOptionsService)
		{
			_dialogService = dialogService;
			_appService = appService;
			_advancedOptionsService = advancedOptionsService;

			_displayMode = RouteEx.Routes.Count > 0 ? DisplayMode.Route : DisplayMode.All;
			UpdateViewButtonText();
			// Match Xamarin: Button text is static "Other Day", not the date
		}

		public async Task OnAppearingAsync()
		{
			// [MIGRATION]: Sign Out fix - prevent OnAppearing logic from running after sign-out
			// Check if user is signed out - if so, return immediately to prevent sync alerts
			// This matches Xamarin behavior where Finish() prevents OnResume from running
			if (!Config.SignedIn)
			{
				Console.WriteLine("[DEBUG] ClientsPage.OnAppearingAsync: User not signed in, skipping logic.");
				return;
			}

			if (!_initialized)
			{
				_initialized = true;
			}

			await RefreshAsync(true, true);

		if (Config.Delivery && Config.PendingLoadToAccept && Config.TrackInventory)
		{
			await _dialogService.ShowAlertAsync("Please accept the pending load before continuing.", "Warning");
			DisableInteractions();
			return;
		}

		// Only check DataProvider methods if DataProvider is initialized
		// This prevents NullReferenceException if the page appears before initialization
		try
		{
			if (DataProvider.MustEndOfDay())
			{
				await _dialogService.ShowAlertAsync("End of Day process is required before continuing.", "Warning");
				ClearClientListAndLock();
				return;
			}
			
			if (!DataProvider.CanUseApplication())
			{
				await _dialogService.ShowAlertAsync("You must sync data before continuing.", "Warning");
				ClearClientListAndLock();
			}
		}
		catch (NullReferenceException)
		{
			// DataProvider not initialized yet - skip these checks
		}
		}

		public async Task OnRouteDateSelectedAsync(DateTime date)
		{
			RouteDate = date.Date;
			// Match Xamarin: Button text stays "Other Day", doesn't show the date
			await RefreshAsync(false, false);
		}

		public async Task HandleClientSelectionAsync(ClientListItemViewModel item)
		{
			if (item == null || item.Client == null)
				return;

			_appService.RecordEvent("ClientsPage.CustomerSelected");

			if (Config.CannotOrderWithUnpaidInvoices && item.HasOverdueInvoices)
			{
				await _dialogService.ShowAlertAsync("Customers highlighted in red are 90 days late on their payments. Please contact the customer prior to visiting them.", "Alert");
			}

		var clientId = item.Client.ClientId;
		var parameters = new Dictionary<string, object>
		{
			{ "clientId", clientId }
		};
		await NavigationHelper.GoToAsync("clientdetails", parameters);
		}

		[RelayCommand]
		private async Task SelectDateAsync()
		{
			// Match Xamarin: Show date picker dialog
			_appService.RecordEvent("Change when to view button");
			
			var initialDate = RouteDate;
			if (initialDate == DateTime.MinValue)
				initialDate = DateTime.Today;
			
			var selectedDate = await _dialogService.ShowDatePickerAsync("Select Date", initialDate);
			if (selectedDate.HasValue)
			{
				await OnRouteDateSelectedAsync(selectedDate.Value);
			}
		}

		[RelayCommand]
		private async Task ToggleDisplayModeAsync()
		{
			_displayMode = _displayMode == DisplayMode.All ? DisplayMode.Route : DisplayMode.All;
			UpdateViewButtonText();
			await RefreshAsync(false, false);
		}

		[RelayCommand]
		private async Task OpenSearchDialogAsync()
		{
			var result = await _dialogService.ShowPromptAsync("Search", "Enter search criteria");
			if (result != null)
			{
				SearchQuery = result;
			}
		}

		[RelayCommand]
		private async Task ToggleEditAsync()
		{
			if (!ShowEditButton)
				return;

			await _dialogService.ShowAlertAsync("Route reordering is not yet available in the MAUI version.", "Info");
		}

		[RelayCommand]
		private async Task OpenMapRouteAsync()
		{
			if (!ShowMapRouteButton)
				return;

			await Shell.Current.GoToAsync("maproutes");
		}

		[RelayCommand]
		private async Task AdvancedOptionsAsync()
		{
			await _advancedOptionsService.ShowAdvancedOptionsAsync();
		}

		partial void OnSearchQueryChanged(string value)
		{
			// Cancel any pending search operation
			_searchDebounceTokenSource?.Cancel();
			_searchDebounceTokenSource?.Dispose();
			_searchDebounceTokenSource = new CancellationTokenSource();

			var token = _searchDebounceTokenSource.Token;

			// Debounce: Wait 300ms before executing the search
			Task.Run(async () =>
			{
				try
				{
					await Task.Delay(300, token);
					if (!token.IsCancellationRequested)
					{
						MainThread.BeginInvokeOnMainThread(() => ApplyFilter());
					}
				}
				catch (TaskCanceledException)
				{
					// Expected when user types quickly
				}
			});
		}

		partial void OnUseGroupingChanged(bool value)
		{
			ShowGroupedList = value;
			ShowFlatList = !value;
		}

		partial void OnIsEditingChanged(bool value)
		{
			// Only update if we have items to avoid unnecessary work
			if (Clients.Count == 0 && ClientGroups.Count == 0)
				return;

			// Batch update editing state for better performance
			foreach (var item in Clients)
			{
				item.IsEditing = value;
			}

			foreach (var group in ClientGroups)
			{
				foreach (var item in group.Clients)
				{
					item.IsEditing = value;
				}
			}
		}

		partial void OnInfoMessageChanged(string value)
		{
			OnPropertyChanged(nameof(HasInfoMessage));
		}

		private async Task RefreshAsync(bool updateList, bool displayMessage)
		{
			// [MIGRATION]: Sign Out fix - prevent RefreshAsync logic from running after sign-out
			if (!Config.SignedIn)
			{
				Console.WriteLine("[DEBUG] ClientsPage.RefreshAsync: User not signed in, skipping refresh.");
				return;
			}

			if (!_initialized)
				return;

			try
			{
				IsBusy = true;

				if (Config.ApplicationIsInDemoMode && displayMessage && !_messageDisplayed)
				{
					await _dialogService.ShowAlertAsync("Device is running in Demo Mode. Contact Laceup for production access.", "Warning");
					_messageDisplayed = true;
				}

				if (!Config.ReceivedData)
				{
					if (displayMessage && !_messageDisplayed)
					{
						await _dialogService.ShowAlertAsync("You must sync data to continue.", "Warning");
						_messageDisplayed = true;
					}

					ClearClientListAndLock();
					return;
				}

				if (updateList)
				{
					if (Config.CanModifyQuotes && !Config.OnlyPresale)
					{
						_displayMode = DisplayMode.Route;
					}
					else
					{
						_displayMode = RouteEx.Routes.Count > 0 ? DisplayMode.Route : DisplayMode.All;
					}

					UpdateViewButtonText();
				}

				BuildSource();
				ApplyFilter();
				UpdateButtons();
			}
			finally
			{
				IsBusy = false;
			}
		}

		private void BuildSource()
		{
			_allEntries.Clear();

			if (Config.AuthorizationFailed)
				return;

			if (_displayMode == DisplayMode.All)
			{
				foreach (var client in Client.SortedClients())
				{
					if (client == null || client.SalesmanClient)
						continue;

					_allEntries.Add(ClientEntry.CreateRegular(client));
				}
			}
			else
			{
				if (Config.CanModifyQuotes && !Config.OnlyPresale)
				{
					var addedClients = new HashSet<int>();
					int count = 1;

					foreach (var quote in Order.Orders.Where(x => x.IsQuote).OrderBy(x => x.Date))
					{
						if (quote.Client == null)
							continue;

						if (addedClients.Add(quote.Client.ClientId))
						{
							_allEntries.Add(ClientEntry.CreateQuote(quote.Client, count++));
						}
					}
				}
				else
				{
					var addedClients = new Dictionary<int, ClientEntry>();

					var filteredRoutes = RouteEx.Routes
						.Where(x => x.Date.Date.Subtract(RouteDate).Days <= 0)
						.OrderBy(x => x.LocallySavedStop)
						.ToList();

					foreach (var route in filteredRoutes)
					{
						if (route.Date.Date.Subtract(RouteDate).Days < 0 && route.Closed)
							continue;
						if (route.Date.Date > RouteDate.AddDays(1))
							continue;
						if (!route.FromDelivery && route.Date.Date != RouteDate)
							continue;
						if (route.Order == null && route.Date.Date != RouteDate)
							continue;

						var client = route.Client ?? route.Order?.Client;
						if (client == null)
						{
							Logger.CreateLog("Route entry missing client reference.");
							continue;
						}

						if (route.Order?.BatchId > 0)
						{
							var batch = Batch.List.FirstOrDefault(x => x.Id == route.Order.BatchId);
							if (batch?.Client != null)
							{
								client = batch.Client;
							}
						}

						var entry = ClientEntry.CreateFromRoute(client, route);

						if (addedClients.TryGetValue(client.ClientId, out var existing))
						{
							if (entry.TypePriority > existing.TypePriority)
							{
								addedClients[client.ClientId] = entry;
							}
						}
						else
						{
							addedClients[client.ClientId] = entry;
						}
					}

					if (addedClients.Count == 0)
					{
						foreach (var client in Client.Clients.OrderBy(x => x.ClientName))
						{
							if (client == null || client.SalesmanClient)
								continue;
							_allEntries.Add(ClientEntry.CreateRegular(client));
						}
					}
					else
					{
						_allEntries.AddRange(addedClients.Values.OrderBy(x => x.SortOrder));
					}
				}
			}
		}

		private void ApplyFilter()
		{
			var query = SearchQuery?.Trim() ?? string.Empty;
			
			// Fast path for empty query
			if (string.IsNullOrWhiteSpace(query))
			{
				UpdateCollectionFromEntries(_allEntries);
				return;
			}

			var comparer = StringComparison.InvariantCultureIgnoreCase;
			var queryLower = query.ToLowerInvariant();

			// Optimized filtering with pre-computed lower case comparison
			var filteredEntries = _allEntries.Where(entry =>
			{
				if (entry.Client == null)
					return false;
				
				var name = entry.Client.ClientName;
				var address = entry.Client.ShipToAddress;
				
				return (name != null && name.Contains(queryLower, comparer)) ||
				       (address != null && address.Contains(queryLower, comparer));
			}).ToList();

			UpdateCollectionFromEntries(filteredEntries);
		}

		private void UpdateCollectionFromEntries(List<ClientEntry> filteredEntries)
		{
			var query = SearchQuery?.Trim() ?? string.Empty;
			var shouldGroup = !string.IsNullOrEmpty(Config.GroupClientsByCat) && _displayMode == DisplayMode.All && Client.Clients.Count > 0;
			UseGrouping = shouldGroup;

			// Pre-build view models off UI thread for better performance
			List<ClientGroupViewModel>? groupViewModels = null;
			List<ClientListItemViewModel>? flatViewModels = null;

			if (shouldGroup)
			{
				// Create a dictionary for O(1) lookup instead of O(n) with FirstOrDefault
				var filteredEntriesDict = filteredEntries.ToDictionary(x => x.Client.ClientId, x => x);

				var dictionary = Client.GroupClients(Client.Clients.ToList());
				groupViewModels = new List<ClientGroupViewModel>();

				foreach (var group in dictionary)
				{
					var items = new List<ClientListItemViewModel>();
					foreach (var client in group.Value)
					{
						// Use dictionary lookup instead of FirstOrDefault for O(1) performance
						if (filteredEntriesDict.TryGetValue(client.ClientId, out var entry))
						{
							items.Add(CreateListItem(entry));
						}
						else if (string.IsNullOrWhiteSpace(query))
						{
							// Only create regular entry if no search query
							items.Add(CreateListItem(ClientEntry.CreateRegular(client)));
						}
					}

					if (items.Count > 0)
					{
						var groupVm = new ClientGroupViewModel(group.Key, new ObservableCollection<ClientListItemViewModel>(items));
						if (query.Length > 0)
							groupVm.IsExpanded = true;
						groupViewModels.Add(groupVm);
					}
				}
			}
			else
			{
				// Pre-create view models off UI thread
				flatViewModels = filteredEntries.Select(entry => CreateListItem(entry)).ToList();
			}

			// Update UI on main thread with pre-built view models
			MainThread.BeginInvokeOnMainThread(() =>
			{
				// Batch collection updates for better performance
				if (shouldGroup && groupViewModels != null)
				{
					ClientGroups.Clear();
					// Use AddRange if available, otherwise add in batch
					foreach (var groupVm in groupViewModels)
					{
						ClientGroups.Add(groupVm);
					}
					Clients.Clear();
				}
				else if (flatViewModels != null)
				{
					Clients.Clear();
					// Batch add items for better performance
					foreach (var vm in flatViewModels)
					{
						Clients.Add(vm);
					}
					ClientGroups.Clear();
				}

				UpdateSearchVisibility();
				UpdateEditingState();
			});
		}

		private void RestrictToOpenClients()
		{
			_allEntries.Clear();
			Clients.Clear();
			ClientGroups.Clear();
			UseGrouping = false;

			var openClients = Order.Orders
				.Where(o => !o.AsPresale && !o.Finished)
				.Select(o => o.Client)
				.Where(c => c != null)
				.DistinctBy(c => c.ClientId)
				.ToList();

			foreach (var client in openClients)
			{
				_allEntries.Add(ClientEntry.CreateDelivery(client));
			}

			foreach (var entry in _allEntries)
			{
				Clients.Add(CreateListItem(entry));
			}

			SearchButtonEnabled = false;
			ShowSearchButton = false;
			UpdateSearchVisibility();
			UpdateButtons();
			InfoMessage = "There are pending transactions that must be finalized.";
		}

		private void DisableInteractions()
		{
			Clients.Clear();
			ClientGroups.Clear();
			UseGrouping = false;
			ShowButtonsLayout = false;
			ShowMapRouteButton = false;
			ShowSelectDayButton = false;
			ShowViewButton = false;
			ShowEditButton = false;
			ShowSearchButton = false;
			SearchButtonEnabled = false;
			IsSearchVisible = false;
			InfoMessage = "Pending load must be accepted before continuing.";
		}

		public void ClearClientListAndLock()
		{
			// Clear client list completely and lock all interactions - matches EOD page behavior
			_allEntries.Clear();
			Clients.Clear();
			ClientGroups.Clear();
			UseGrouping = false;
			ShowButtonsLayout = false;
			ShowMapRouteButton = false;
			ShowSelectDayButton = false;
			ShowViewButton = false;
			ShowEditButton = false;
			ShowSearchButton = false;
			SearchButtonEnabled = false;
			IsSearchVisible = false;
			InfoMessage = "You must sync data before continuing.";
		}

		private void UpdateButtons()
		{
			var hasRoutes = RouteEx.Routes.Count > 0;
			// Match Xamarin: Check if routes have different dates
			// Xamarin compares x.Date != firstDate directly, but we should compare date parts to avoid time component issues
			var differentDates = false;
			if (hasRoutes)
			{
				var firstDate = RouteEx.Routes[0].Date.Date; // Get date part only
				differentDates = RouteEx.Routes.Any(x => x.Date.Date != firstDate);
			}
			var hasRouteEntries = _allEntries.Any(x => x.Type != ClientEntryType.Regular);

			ShowViewButton = hasRoutes || (Config.CanModifyQuotes && !Config.OnlyPresale);
			ShowSelectDayButton = differentDates;
			ShowMapRouteButton = Config.CanChangeRoutesOrder && hasRouteEntries;
			ShowEditButton = Config.CanChangeRoutesOrder && hasRouteEntries;
			ShowSearchButton = Config.ReceivedData;
			SearchButtonEnabled = Config.ReceivedData;
			ShowButtonsLayout = ShowMapRouteButton || ShowViewButton || ShowSelectDayButton || ShowEditButton || ShowSearchButton;
			InfoMessage = string.Empty;

			if (!Config.ReceivedData)
			{
				InfoMessage = "Data has not been synced.";
			}
		}

		private void UpdateSearchVisibility()
		{
			// Show search bar when data is loaded and there are entries to search
			IsSearchVisible = Config.ReceivedData && _allEntries.Count > 0;
		}

		private void UpdateEditingState()
		{
			// Only update if editing state changed and we have items
			if (Clients.Count == 0 && ClientGroups.Count == 0)
				return;

			foreach (var item in Clients)
			{
				item.IsEditing = IsEditing;
			}

			foreach (var group in ClientGroups)
			{
				foreach (var item in group.Clients)
				{
					item.IsEditing = IsEditing;
				}
			}
		}

		private void UpdateViewButtonText()
		{
			ViewButtonText = _displayMode == DisplayMode.All ? "View Route" : "View All";
		}

		private ClientListItemViewModel CreateListItem(ClientEntry entry)
		{
			return new ClientListItemViewModel(entry, _displayMode == DisplayMode.Route);
		}
	}

	public partial class ClientGroupViewModel : ObservableObject
	{
		public string Id { get; }
		public string DisplayName => $"{Name}";
		public string Name { get; }
		public ObservableCollection<ClientListItemViewModel> Clients { get; }

		[ObservableProperty]
		private bool _isExpanded;

		public ClientGroupViewModel(string name, ObservableCollection<ClientListItemViewModel> clients)
		{
			Id = name;
			Name = string.IsNullOrWhiteSpace(name) ? "Uncategorized" : name;
			Clients = clients;
		}

		[RelayCommand]
		private void ToggleExpansion()
		{
			IsExpanded = !IsExpanded;
		}
	}

	public partial class ClientListItemViewModel : ObservableObject
	{
		private readonly ClientEntry _entry;
		private readonly CultureInfo _culture = CultureInfo.CurrentCulture;
		private readonly bool _isRouteView; // Track if we're showing routes (not "All")
		
		// Cached properties for performance
		private readonly string _name;
		private readonly string _address;
		private readonly bool _showAddress;
		private readonly string _balance;
		private readonly Color _nameColor;
		private readonly Color _addressColor;

		public Client Client => _entry.Client;
		public string Name => _name;
		public string Address => _address;
		public bool ShowAddress => _showAddress;
		public string StopText => _entry.Stop > 0 ? _entry.Stop.ToString(_culture) : string.Empty;
		
		public bool ShowStop => _isRouteView && _entry.Type != ClientEntryType.Regular && !string.IsNullOrEmpty(StopText);
		
		public bool ShowIcon => _isRouteView && _entry.Type != ClientEntryType.Regular;
		
		public bool HasLeftContent => ShowStop || ShowIcon;
		
		public ImageSource IconImageSource
		{
			get
			{
				if (!ShowIcon)
					return null;
					
				var icon = _entry.Type switch
				{
					ClientEntryType.Route => MaterialOutlinedIcons.PersonPinCircle,
					ClientEntryType.Delivery => MaterialOutlinedIcons.LocalShipping,
					ClientEntryType.Completed => MaterialOutlinedIcons.CheckCircle,
					ClientEntryType.Quote => MaterialOutlinedIcons.Description,
					_ => (MaterialOutlinedIcons?)null
				};
				
				if (icon == null)
					return null;
				
				return MaterialIconHelper.GetImageSource(icon.Value, Colors.Black, 24);
			}
		}
		
		public bool ShowCollectBadge => Client?.ClientBalanceInDevice > 0;
		public bool ShowPaymentBadge { get; }
		public bool ShowVisitBadge { get; }
		public bool HasOverdueInvoices { get; }
		public string Balance => _balance;

		[ObservableProperty]
		private bool _isEditing;

		public Color NameColor
		{
			get
			{
				if (IsEditing)
					return Color.FromArgb("#1976D2");
				return _nameColor;
			}
		}

		public Color AddressColor => IsEditing ? Color.FromArgb("#1976D2") : _addressColor;

		public Color StopBadgeBackground => _entry.Type switch
		{
			ClientEntryType.Completed => Color.FromArgb("#4CAF50"),
			ClientEntryType.Delivery => Color.FromArgb("#FFA726"),
			ClientEntryType.Quote => Color.FromArgb("#42A5F5"),
			ClientEntryType.Route => Color.FromArgb("#90A4AE"),
			_ => Color.FromArgb("#CFD8DC")
		};

		public Color StopBadgeTextColor => Colors.White;

		public ClientListItemViewModel(ClientEntry entry, bool isRouteView)
		{
			_entry = entry;
			_isRouteView = isRouteView; // Store for ShowStop/ShowIcon logic

			// Cache expensive string operations
			_name = Client?.ClientName ?? string.Empty;
			_address = Client?.ShipToAddress?.Replace("|", " ") ?? string.Empty;
			_showAddress = Config.ShowAddrInClientList && !string.IsNullOrEmpty(_address);
			_balance = Client?.ClientBalanceInDevice.ToCustomString() ?? 0.0.ToCustomString();

			// Pre-calculate HasOverdueInvoices for color caching
			bool hasOverdue = false;
			if (Config.CannotOrderWithUnpaidInvoices && Client != null)
			{
				var invoices = (Invoice.OpenInvoices ?? new List<Invoice>())
					.Where(x => x.Client.ClientId == Client.ClientId)
					.ToList();

				if (invoices.Count > 0)
				{
					hasOverdue = invoices.Any(x => x.DueDate.AddDays(90) < DateTime.Now.Date && x.Balance > 0);
				}
			}
			HasOverdueInvoices = hasOverdue;

			// Cache colors based on overdue status
			_nameColor = hasOverdue ? Colors.Red : Colors.Black;
			_addressColor = Color.FromArgb("#4A4A4A");

			if (Config.TimeSheetCustomization && Session.sessionDetails != null && Client != null)
			{
				ShowVisitBadge = Session.sessionDetails.Any(x =>
					x.clientId == Client.ClientId &&
					string.IsNullOrEmpty(x.orderUniqueId) &&
					x.detailType == SessionDetails.SessionDetailType.CustomerVisit &&
					x.endTime == DateTime.MinValue);
			}

			if (Config.ViewGoals && Client != null)
			{
				var goals = (GoalProgressDTO.List ?? new List<GoalProgressDTO>())
					.Where(x => x.Criteria == GoalCriteria.Payment && x.PendingDays > 0)
					.ToList();

				var goalDetails = GoalDetailDTO.List ?? new List<GoalDetailDTO>();

				ShowPaymentBadge = goals.Any(goal =>
					goalDetails.Where(x => x.GoalId == goal.Id).Any(detail => detail.ClientId == Client.ClientId));
			}
		}
	}

	public enum ClientEntryType
	{
		Regular = 0,
		Route = 1,
		Delivery = 2,
		Completed = 3,
		Quote = 4
	}

	public sealed class ClientEntry
	{
		public Client Client { get; }
		public RouteEx? Route { get; }
		public ClientEntryType Type { get; }
		public int Stop { get; }
		public int LocalStop { get; }

		public int TypePriority => Type switch
		{
			ClientEntryType.Completed => 4,
			ClientEntryType.Delivery => 3,
			ClientEntryType.Route => 2,
			ClientEntryType.Quote => 1,
			_ => 0
		};

		public int SortOrder => LocalStop > 0 ? LocalStop : Stop;

		private ClientEntry(Client client, RouteEx? route, ClientEntryType type, int stop, int localStop)
		{
			Client = client;
			Route = route;
			Type = type;
			Stop = stop;
			LocalStop = localStop;
		}

		public static ClientEntry CreateRegular(Client client) =>
			new(client, null, ClientEntryType.Regular, 0, 0);

		public static ClientEntry CreateQuote(Client client, int stop) =>
			new(client, null, ClientEntryType.Quote, stop, stop);

		public static ClientEntry CreateDelivery(Client client) =>
			new(client, null, ClientEntryType.Delivery, 0, 0);

		public static ClientEntry CreateFromRoute(Client client, RouteEx route)
		{
			var type = ClientEntryType.Route;
			if (route.Closed)
				type = ClientEntryType.Completed;
			else if (route.Order != null)
				type = ClientEntryType.Delivery;

			return new ClientEntry(client, route, type, route.Stop, route.LocallySavedStop);
		}

		public bool MatchesQuery(string query)
		{
			if (string.IsNullOrWhiteSpace(query))
				return true;

			var comparer = StringComparison.InvariantCultureIgnoreCase;
			return (Client?.ClientName?.Contains(query, comparer) ?? false)
				|| (Client?.ShipToAddress?.Contains(query, comparer) ?? false);
		}
	}
}

