using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Graphics;
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
		private string _selectDayButtonText = DateTime.Today.ToString("d", CultureInfo.CurrentCulture);

		[ObservableProperty]
		private string _editButtonText = "Edit";

		[ObservableProperty]
		private string _searchQuery = string.Empty;

		[ObservableProperty]
		private DateTime _routeDate = DateTime.Today;

		[ObservableProperty]
		private string _infoMessage = string.Empty;

		public bool HasInfoMessage => !string.IsNullOrWhiteSpace(InfoMessage);

		public event Action? SelectDateRequested;

		public ClientsPageViewModel(DialogService dialogService, ILaceupAppService appService)
		{
			_dialogService = dialogService;
			_appService = appService;

			_displayMode = RouteEx.Routes.Count > 0 ? DisplayMode.Route : DisplayMode.All;
			UpdateViewButtonText();
			SelectDayButtonText = RouteDate.ToString("d", CultureInfo.CurrentCulture);
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

			if (Config.Delivery && DataAccess.PendingLoadToAccept && Config.TrackInventory)
			{
				await _dialogService.ShowAlertAsync("Please accept the pending load before continuing.", "Warning");
				DisableInteractions();
				return;
			}

			if (DataAccess.MustEndOfDay())
			{
				await _dialogService.ShowAlertAsync("End of Day process is required before continuing.", "Warning");
				RestrictToOpenClients();
			}
			else if (!DataAccess.CanUseApplication())
			{
				await _dialogService.ShowAlertAsync("You must sync data before continuing.", "Warning");
				RestrictToOpenClients();
			}
		}

		public async Task OnRouteDateSelectedAsync(DateTime date)
		{
			RouteDate = date.Date;
			SelectDayButtonText = RouteDate.ToString("d", CultureInfo.CurrentCulture);
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
			await Shell.Current.GoToAsync("clientdetails", parameters);
		}

		[RelayCommand]
		private void SelectDate()
		{
			SelectDateRequested?.Invoke();
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
			var options = new List<string>
			{
				"Update settings",
				"Send log file",
				"Export data",
				"Remote control",
				"Setup printer"
			};

			if (Config.GoToMain)
			{
				options.Add("Go to main activity");
			}

			var choice = await _dialogService.ShowActionSheetAsync("Advanced options", "", "Cancel", options.ToArray());

			switch (choice)
			{
				case "Update settings":
					await _appService.UpdateSalesmanSettingsAsync();
					await _dialogService.ShowAlertAsync("Settings updated.", "Info");
					break;
				case "Send log file":
					await _appService.SendLogAsync();
					await _dialogService.ShowAlertAsync("Log sent.", "Info");
					break;
				case "Export data":
					await _appService.ExportDataAsync();
					await _dialogService.ShowAlertAsync("Data exported.", "Info");
					break;
				case "Remote control":
					await _appService.RemoteControlAsync();
					break;
				case "Setup printer":
					await _dialogService.ShowAlertAsync("Printer setup is not yet implemented in the MAUI version.", "Info");
					break;
				case "Go to main activity":
					await _appService.GoBackToMainAsync();
					break;
			}
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

				if (!DataAccess.ReceivedData)
				{
					if (displayMessage && !_messageDisplayed)
					{
						await _dialogService.ShowAlertAsync("You must sync data to continue.", "Warning");
						_messageDisplayed = true;
					}

					RestrictToOpenClients();
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
			var comparer = StringComparison.InvariantCultureIgnoreCase;

			// Filter entries (done off UI thread)
			var filteredEntries = string.IsNullOrWhiteSpace(query)
				? _allEntries.ToList()
				: _allEntries.Where(entry =>
						(entry.Client?.ClientName?.Contains(query, comparer) ?? false) ||
						(entry.Client?.ShipToAddress?.Contains(query, comparer) ?? false))
					.ToList();

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
				Clients.Clear();
				ClientGroups.Clear();

				if (shouldGroup && groupViewModels != null)
				{
					foreach (var groupVm in groupViewModels)
					{
						ClientGroups.Add(groupVm);
					}
				}
				else if (flatViewModels != null)
				{
					foreach (var vm in flatViewModels)
					{
						Clients.Add(vm);
					}
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

		private void UpdateButtons()
		{
			var hasRoutes = RouteEx.Routes.Count > 0;
			var firstRouteDate = hasRoutes ? RouteEx.Routes[0].Date.Date : DateTime.Today;
			var differentDates = hasRoutes && RouteEx.Routes.Any(x => x.Date.Date != firstRouteDate);
			var hasRouteEntries = _allEntries.Any(x => x.Type != ClientEntryType.Regular);

			ShowViewButton = hasRoutes || (Config.CanModifyQuotes && !Config.OnlyPresale);
			ShowSelectDayButton = differentDates;
			ShowMapRouteButton = Config.CanChangeRoutesOrder && hasRouteEntries;
			ShowEditButton = Config.CanChangeRoutesOrder && hasRouteEntries;
			ShowSearchButton = DataAccess.ReceivedData;
			SearchButtonEnabled = DataAccess.ReceivedData;
			ShowButtonsLayout = ShowMapRouteButton || ShowViewButton || ShowSelectDayButton || ShowEditButton || ShowSearchButton;
			InfoMessage = string.Empty;

			if (!DataAccess.ReceivedData)
			{
				InfoMessage = "Data has not been synced.";
			}
		}

		private void UpdateSearchVisibility()
		{
			// Show search bar when data is loaded and there are entries to search
			IsSearchVisible = DataAccess.ReceivedData && _allEntries.Count > 0;
		}

		private void UpdateEditingState()
		{
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

		public Client Client => _entry.Client;
		public string Name => Client?.ClientName ?? string.Empty;
		public string Address => Client?.ShipToAddress?.Replace("|", " ") ?? string.Empty;
		public bool ShowAddress => Config.ShowAddrInClientList && !string.IsNullOrEmpty(Address);
		public string StopText => _entry.Stop > 0 ? _entry.Stop.ToString(_culture) : string.Empty;
		public bool ShowStop => !string.IsNullOrEmpty(StopText);
		public bool ShowCollectBadge => Client?.ClientBalanceInDevice > 0;
		public bool ShowPaymentBadge { get; }
		public bool ShowVisitBadge { get; }
		public bool HasOverdueInvoices { get; }

		[ObservableProperty]
		private bool _isEditing;

		public Color NameColor
		{
			get
			{
				if (HasOverdueInvoices)
					return Colors.Red;
				if (IsEditing)
					return Color.FromArgb("#1976D2");
				return Colors.Black;
			}
		}

		public Color AddressColor => IsEditing ? Color.FromArgb("#1976D2") : Color.FromArgb("#4A4A4A");

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

			if (Config.CannotOrderWithUnpaidInvoices)
			{
				var invoices = (Invoice.OpenInvoices ?? new List<Invoice>())
					.Where(x => x.Client.ClientId == Client.ClientId)
					.ToList();

				if (invoices.Count > 0)
				{
					HasOverdueInvoices = invoices.Any(x => x.DueDate.AddDays(90) < DateTime.Now.Date && x.Balance > 0);
				}
			}

			if (Config.TimeSheetCustomization && Session.sessionDetails != null)
			{
				ShowVisitBadge = Session.sessionDetails.Any(x =>
					x.clientId == Client.ClientId &&
					string.IsNullOrEmpty(x.orderUniqueId) &&
					x.detailType == SessionDetails.SessionDetailType.CustomerVisit &&
					x.endTime == DateTime.MinValue);
			}

			if (Config.ViewGoals)
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

