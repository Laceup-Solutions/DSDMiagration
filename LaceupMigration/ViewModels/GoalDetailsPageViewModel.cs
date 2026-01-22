using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using LaceupMigration;

namespace LaceupMigration.ViewModels
{
    public partial class GoalDetailsPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;
        private List<GoalDetailItemViewModel> _allDetails = new();
        private List<GoalDetailDTO> _allGoalDetails = new(); // Store original GoalDetailDTO list for filtering
        internal GoalProgressDTO? _goal; // Store the goal object to access Criteria (internal for GoalDetailItemViewModel access)
        private int _goalId;

        [ObservableProperty] private string _goalName = string.Empty;
        [ObservableProperty] private string _goalDescription = string.Empty;
        [ObservableProperty] private double _progressPercentage;
        [ObservableProperty] private string _progressText = string.Empty;
        [ObservableProperty] private Color _progressColor = Colors.Blue;
        [ObservableProperty] private DateTime _startDate = DateTime.Today.AddDays(-30);
        [ObservableProperty] private DateTime _endDate = DateTime.Today;
        [ObservableProperty] private ObservableCollection<GoalDetailItemViewModel> _goalDetails = new();
        [ObservableProperty] private string _searchText = string.Empty;
        
        // Checkbox properties (matching Xamarin GoalsActivity lines 113-115, 158-168)
        [ObservableProperty] private bool _includeSalesOrders = false;
        [ObservableProperty] private bool _includeDeviceOrders = false;
        [ObservableProperty] private bool _includeCreditOrders = true;
        
        // Goal Type text (matching Xamarin line 566)
        public string GoalTypeText => $"Goal Type: {(_goal?.Type.ToString() ?? "Unknown")}";
        
        // Show Include Sales Orders checkbox (hidden for Payment criteria, matching Xamarin line 573)
        public bool ShowIncludeSalesOrders => _goal?.Criteria != GoalCriteria.Payment;
        
        // Include Device Orders text (changes for Payment criteria, matching Xamarin line 574)
        public string IncludeDeviceOrdersText
        {
            get
            {
                if (_goal?.Criteria == GoalCriteria.Payment)
                    return "Include Payments In Device";
                return "Include Orders In Device";
            }
        }
        
        // Filter popup properties (matching Xamarin GoalsActivity Filter_Click)
        [ObservableProperty] private bool _isFilterPopupVisible = false;
        
        // GoalTimeFrame enum (matching Xamarin lines 41-45)
        public enum GoalTimeFrame
        {
            Monthly = 1,
            Weekly = 2
        }
        
        // GoalSortBy enum (matching Xamarin lines 47-53)
        public enum GoalSortBy
        {
            None = 0,
            Name = 1,
            Category = 2,
            Code = 3
        }
        
        // Filter properties (matching Xamarin lines 55-56)
        private GoalTimeFrame _whatToView = GoalTimeFrame.Monthly;
        private GoalSortBy _sortBy = GoalSortBy.None;
        
        // Filter popup UI properties
        [ObservableProperty] private bool _filterMonthly = true;
        [ObservableProperty] private bool _filterWeekly = false;
        [ObservableProperty] private bool _sortByNone = true;
        [ObservableProperty] private bool _sortByName = false;
        [ObservableProperty] private bool _sortByCategory = false;
        [ObservableProperty] private bool _sortByCode = false;
        
        // Show sort options only for Product/ProductsByCustomer criteria (matching Xamarin lines 244-247)
        public bool ShowSortOptions => _goal?.Criteria == GoalCriteria.Product || _goal?.Criteria == GoalCriteria.ProductsByCustomer;
        
        // Header text properties (matching Xamarin GoalsActivity)
        public string StartDateText => $"Start Date: {StartDate.ToShortDateString()}";
        public string EndDateText => $"End Date: {EndDate.ToShortDateString()}";
        public string DaysToCompleteText => $"{WorkingDays} Days to Complete Goal";
        public string PercentageText => $"% {Percentage:F0}";
        public string PendingDaysText => $"Pending Days: {PendingDays}";
        
        [ObservableProperty] private int _workingDays;
        [ObservableProperty] private int _pendingDays;
        [ObservableProperty] private double _percentage;
        [ObservableProperty] private double _progressValue; // For progress bar (1.0 when 0% to show full red bar)
        
        partial void OnWorkingDaysChanged(int value)
        {
            OnPropertyChanged(nameof(DaysToCompleteText));
        }
        
        partial void OnPendingDaysChanged(int value)
        {
            OnPropertyChanged(nameof(PendingDaysText));
        }
        
        partial void OnPercentageChanged(double value)
        {
            OnPropertyChanged(nameof(PercentageText));
        }
        
        partial void OnStartDateChanged(DateTime value)
        {
            OnPropertyChanged(nameof(StartDateText));
        }
        
        partial void OnEndDateChanged(DateTime value)
        {
            OnPropertyChanged(nameof(EndDateText));
        }

        public GoalDetailsPageViewModel(DialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
        }

        public void OnNavigatedTo(IDictionary<string, object> query)
        {
            if (query != null)
            {
                if (query.TryGetValue("goalId", out var goalId) && goalId != null)
                {
                    _goalId = Convert.ToInt32(goalId);
                }
                if (query.TryGetValue("goalName", out var goalName))
                {
                    GoalName = goalName?.ToString() ?? "Goal";
                }
            }
        }

        public async Task OnAppearingAsync()
        {
            try
            {
                IsLoading = true;
                
                await Task.Run(() =>
                {
                    // Load goal progress detail if not already loaded
                    if (GoalDetailDTO.List.Count == 0)
                    {
                        DataProvider.GetGoalProgressDetail();
                    }
                });
                
                // Get the goal
                _goal = GoalProgressDTO.List.FirstOrDefault(x => x.Id == _goalId);
                if (_goal != null)
                {
                    GoalName = _goal.Name ?? "Goal";
                    GoalDescription = $"{_goal.Type} Goal - {_goal.Criteria}";
                    StartDate = _goal.StartDate;
                    EndDate = _goal.EndDate;
                    WorkingDays = _goal.WorkingDays;
                    PendingDays = _goal.PendingDays;
                    
                    // Set initial checkbox states (matching Xamarin OnCreate lines 570-575)
                    if (_goal.Criteria == GoalCriteria.Payment)
                    {
                        IncludeSalesOrders = false; // Hidden for Payment criteria
                    }
                    
                    // Notify property changes for computed properties
                    OnPropertyChanged(nameof(GoalTypeText));
                    OnPropertyChanged(nameof(IncludeDeviceOrdersText));
                    OnPropertyChanged(nameof(ShowIncludeSalesOrders));
                    OnPropertyChanged(nameof(ShowSortOptions));
                    
                    // Load goal details - store both GoalDetailDTO and ViewModel items
                    _allDetails.Clear();
                    _allGoalDetails.Clear();
                    var goalDetails = GoalDetailDTO.List.Where(x => x.GoalId == _goalId).ToList();
                    _allGoalDetails = goalDetails;
                    
                    foreach (var detail in goalDetails)
                    {
                        var item = new GoalDetailItemViewModel
                        {
                            Detail = detail // Store reference to original DTO for filtering
                        };
                        item.UpdateCalculations(this, _whatToView);
                        _allDetails.Add(item);
                    }
                }
                else
                {
                    await _dialogService.ShowAlertAsync("Goal not found.", "Error", "OK");
                    await Shell.Current.GoToAsync("..");
                    return;
                }

                UpdateProgress();
                FilterDetails(string.Empty);
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error loading goal details: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void UpdateProgress()
        {
            if (_goal != null)
            {
                // Match Xamarin RefreshLabels logic (lines 333-498)
                var sold = _goal.Sold;
                
                // Include Sales Orders if checked (matching Xamarin line 334-335)
                if (IncludeSalesOrders)
                    sold += _goal.SalesOrder;
                
                // Include Credit Orders if checked (matching Xamarin line 337-338)
                if (IncludeCreditOrders)
                    sold += _goal.CreditInvoice;
                
                // Include Device Orders if checked (matching Xamarin lines 342-498)
                // This is complex logic that depends on goal criteria and type
                // For now, we'll implement the basic structure - full implementation would require
                // iterating through Order.Orders and calculating based on criteria
                // TODO: Implement full IncludeDeviceOrders logic if needed
                
                // Calculate percentage (matching Xamarin line 500)
                Percentage = Math.Round(((_goal.QuantityOrAmount > 0 ? sold / _goal.QuantityOrAmount : 0) * 100), Config.Round);
                
                // Progress bar value: 1.0 when 0% to show full red bar (matching Xamarin visual behavior)
                ProgressValue = Percentage == 0 ? 1.0 : Math.Min(1.0, Percentage / 100.0);
                
                // Progress color: red when < 100%, green when >= 100% (matching Xamarin lines 504-515)
                ProgressColor = Percentage >= 100 ? Colors.DarkGreen : Colors.Red;
                
                ProgressPercentage = Percentage / 100.0;
                ProgressText = $"{sold:F0} / {_goal.QuantityOrAmount:F0} ({ProgressPercentage:P0})";
            }
            else if (_allDetails.Count > 0)
            {
                // Fallback: calculate from goal details if goal is not available
                // This should not normally happen, but provides a fallback
                var totalProgress = _allGoalDetails.Sum(x => x.SoldValue);
                var totalTarget = _allGoalDetails.Sum(x => x.QuantityOrAmountValue);
                ProgressPercentage = totalTarget > 0 ? totalProgress / totalTarget : 0;
                ProgressText = $"{totalProgress:F0} / {totalTarget:F0} ({ProgressPercentage:P0})";
                
                ProgressColor = ProgressPercentage >= 1.0 ? Colors.Green : 
                               ProgressPercentage >= 0.75 ? Colors.Blue : Colors.Orange;
            }
        }
        
        // Checkbox change handlers (matching Xamarin IncludeSales_CheckedChange and IncludeCreditInvoices_CheckedChange)
        partial void OnIncludeSalesOrdersChanged(bool value)
        {
            UpdateProgress();
            RefreshDetails();
        }
        
        partial void OnIncludeDeviceOrdersChanged(bool value)
        {
            UpdateProgress();
            RefreshDetails();
        }
        
        partial void OnIncludeCreditOrdersChanged(bool value)
        {
            UpdateProgress();
            RefreshDetails();
        }
        
        private void RefreshDetails()
        {
            // Update calculations for all details when checkboxes change
            foreach (var detail in _allDetails)
            {
                detail.UpdateCalculations(this, _whatToView);
            }
            
            // Re-filter details when checkboxes change (matching Xamarin Refresh method)
            FilterDetails(SearchText);
        }
        
        // Filter popup commands (matching Xamarin Filter_Click)
        [RelayCommand]
        private void ShowFilterPopup()
        {
            // Load current filter state into properties
            FilterMonthly = _whatToView == GoalTimeFrame.Monthly;
            FilterWeekly = _whatToView == GoalTimeFrame.Weekly;
            
            SortByNone = _sortBy == GoalSortBy.None;
            SortByName = _sortBy == GoalSortBy.Name;
            SortByCategory = _sortBy == GoalSortBy.Category;
            SortByCode = _sortBy == GoalSortBy.Code;
            
            IsFilterPopupVisible = true;
        }
        
        [RelayCommand]
        private void CloseFilterPopup()
        {
            IsFilterPopupVisible = false;
        }
        
        [RelayCommand]
        private void ApplyFilter()
        {
            // Apply time frame filter (matching Xamarin lines 280-286)
            if (FilterWeekly)
            {
                _whatToView = GoalTimeFrame.Weekly;
            }
            else if (FilterMonthly)
            {
                _whatToView = GoalTimeFrame.Monthly;
            }
            
            // Apply sort by filter (matching Xamarin lines 289-299)
            if (SortByNone)
                _sortBy = GoalSortBy.None;
            else if (SortByName)
                _sortBy = GoalSortBy.Name;
            else if (SortByCategory)
                _sortBy = GoalSortBy.Category;
            else if (SortByCode)
                _sortBy = GoalSortBy.Code;
            
            // Update calculations for all details with new time frame
            foreach (var detail in _allDetails)
            {
                detail.UpdateCalculations(this, _whatToView);
            }
            
            // Refresh the details list (matching Xamarin line 301)
            RefreshDetails();
            
            IsFilterPopupVisible = false;
        }

        public void FilterDetails(string searchText)
        {
            GoalDetails.Clear();

            // Start with all goal details (matching Xamarin Refresh line 118)
            var list = _allGoalDetails.ToList();

            // Apply search filter (matching Xamarin's search logic from GoalsActivity.Refresh() lines 120-135)
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var searchCriteria = searchText.ToLowerInvariant();

                if (_goal != null)
                {
                    // Filter based on goal criteria (matching Xamarin logic)
                    if (_goal.Criteria == GoalCriteria.Product || _goal.Criteria == GoalCriteria.ProductsByCustomer)
                    {
                        // For Product/ProductsByCustomer: search in Product.Name, Upc, Code, Description, Sku
                        var productsIds = Product.Products
                            .Where(x => 
                                (x.Name != null && x.Name.ToLower().Contains(searchCriteria)) ||
                                (x.Upc != null && x.Upc.Contains(searchCriteria)) ||
                                (x.Code != null && x.Code.Contains(searchCriteria)) ||
                                (x.Description != null && x.Description.Contains(searchCriteria)) ||
                                (x.Sku != null && x.Sku.Contains(searchCriteria)))
                            .Select(x => x.ProductId)
                            .ToList();
                        
                        list = list
                            .Where(x => x.ProductId.HasValue && productsIds.Contains(x.ProductId.Value))
                            .ToList();
                    }
                    else if (_goal.Criteria == GoalCriteria.Customer || _goal.Criteria == GoalCriteria.Route)
                    {
                        // For Customer/Route: search in Client.ClientName
                        var clientsId = Client.Clients
                            .Where(x => x.ClientName != null && x.ClientName.ToLower().Contains(searchCriteria))
                            .Select(x => x.ClientId)
                            .ToList();
                        
                        list = list
                            .Where(x => clientsId.Contains(x.ClientId))
                            .ToList();
                    }
                }
                else
                {
                    // Fallback: simple name search if goal is not available
                    list = list
                        .Where(x => 
                            (x.Name != null && x.Name.ToLower().Contains(searchCriteria)) ||
                            (x.Product != null && x.Product.Name != null && x.Product.Name.ToLower().Contains(searchCriteria)))
                        .ToList();
                }
            }

            // Apply sort by filter (matching Xamarin Refresh lines 142-156)
            switch (_sortBy)
            {
                case GoalSortBy.None:
                    list = list.ToList();
                    break;
                case GoalSortBy.Name:
                    list = list.OrderBy(x => x.Product != null ? x.Product.Name : "").ToList();
                    break;
                case GoalSortBy.Category:
                    list = list.OrderBy(x => x.Product != null ? x.Product.CategoryId : 0).ToList();
                    break;
                case GoalSortBy.Code:
                    list = list.OrderBy(x => x.Product != null ? x.Product.Code : "").ToList();
                    break;
            }

            // Convert filtered GoalDetailDTO to ViewModel items - maintain sorted order
            // Create a dictionary for quick lookup of ViewModel items by DTO Id
            var viewModelLookup = _allDetails
                .Where(x => x.Detail != null)
                .ToDictionary(x => x.Detail.Id, x => x);

            // Iterate through sorted list to maintain sort order
            foreach (var dto in list)
            {
                if (viewModelLookup.TryGetValue(dto.Id, out var viewModelItem))
                {
                    GoalDetails.Add(viewModelItem);
                }
            }
        }

        [RelayCommand]
        private async Task Filter()
        {
            try
            {
                IsLoading = true;
                
                // Reload goal progress detail (it may have date filtering built in)
                await Task.Run(() =>
                {
                    DataProvider.GetGoalProgressDetail();
                });
                
                // Filter details by date range
                var goalDetails = GoalDetailDTO.List
                    .Where(x => x.GoalId == _goalId)
                    .Where(x => x.Goal != null && x.Goal.StartDate >= StartDate && x.Goal.EndDate <= EndDate)
                    .ToList();
                
                _allDetails.Clear();
                _allGoalDetails.Clear();
                _allGoalDetails = goalDetails;
                
                foreach (var detail in goalDetails)
                {
                    var item = new GoalDetailItemViewModel
                    {
                        Detail = detail // Store reference to original DTO for filtering
                    };
                    item.UpdateCalculations(this, _whatToView);
                    _allDetails.Add(item);
                }
                
                UpdateProgress();
                FilterDetails(SearchText);
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error filtering: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task SendGoalByEmailAsync()
        {
            if (_goal == null)
            {
                await _dialogService.ShowAlertAsync("Goal not found.", "Error", "OK");
                return;
            }

            try
            {
                // Use PdfHelper to send goal by email (matches Xamarin GoalsActivity.SendByEmail)
                await PdfHelper.SendGoalByEmail(_goal);
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                await _dialogService.ShowAlertAsync("Error occurred sending email.", "Alert", "OK");
            }
        }

        [ObservableProperty] private bool _isLoading;
    }

    public partial class GoalDetailItemViewModel : ObservableObject
    {
        public GoalDetailDTO? Detail { get; set; } // Store reference to original DTO for filtering
        private GoalDetailsPageViewModel? _parentViewModel; // Reference to parent for checkbox states
        
        [ObservableProperty] private string _itemName = string.Empty;
        [ObservableProperty] private string _clientName = string.Empty;
        [ObservableProperty] private string _uomText = string.Empty;
        [ObservableProperty] private bool _showClientName = false;
        [ObservableProperty] private bool _showUom = false;
        
        // Calculated properties (matching Xamarin GoalsActivity.GetView logic)
        private string _goalAmountText = "0";
        private string _soldText = "0";
        private string _missingText = "0";
        private string _dailySalesText = "0";
        private string _percentageText = "0%";
        private double _progressValue = 0;
        private Color _progressColor = Colors.Red;
        private string _goalAmountLabel = "Goal Amount";
        private string _soldLabel = "Sold";
        private string _dailySalesLabel = "Daily Sales To Goal";
        
        public string GoalAmountText
        {
            get => _goalAmountText;
            private set
            {
                if (SetProperty(ref _goalAmountText, value))
                    OnPropertyChanged();
            }
        }
        
        public string SoldText
        {
            get => _soldText;
            private set
            {
                if (SetProperty(ref _soldText, value))
                    OnPropertyChanged();
            }
        }
        
        public string MissingText
        {
            get => _missingText;
            private set
            {
                if (SetProperty(ref _missingText, value))
                    OnPropertyChanged();
            }
        }
        
        public string DailySalesText
        {
            get => _dailySalesText;
            private set
            {
                if (SetProperty(ref _dailySalesText, value))
                    OnPropertyChanged();
            }
        }
        
        public string PercentageText
        {
            get => _percentageText;
            private set
            {
                if (SetProperty(ref _percentageText, value))
                    OnPropertyChanged();
            }
        }
        
        public double ProgressValue
        {
            get => _progressValue;
            private set
            {
                if (SetProperty(ref _progressValue, value))
                    OnPropertyChanged();
            }
        }
        
        public Color ProgressColor
        {
            get => _progressColor;
            private set
            {
                if (SetProperty(ref _progressColor, value))
                    OnPropertyChanged();
            }
        }
        
        // Label texts (change based on criteria)
        public string GoalAmountLabel
        {
            get => _goalAmountLabel;
            private set
            {
                if (SetProperty(ref _goalAmountLabel, value))
                    OnPropertyChanged();
            }
        }
        
        public string SoldLabel
        {
            get => _soldLabel;
            private set
            {
                if (SetProperty(ref _soldLabel, value))
                    OnPropertyChanged();
            }
        }
        
        public string DailySalesLabel
        {
            get => _dailySalesLabel;
            private set
            {
                if (SetProperty(ref _dailySalesLabel, value))
                    OnPropertyChanged();
            }
        }
        
        public void UpdateCalculations(GoalDetailsPageViewModel parent, GoalDetailsPageViewModel.GoalTimeFrame timeFrame)
        {
            _parentViewModel = parent;
            if (Detail == null) return;
            
            var line = Detail;
            var goal = parent._goal;
            if (goal == null) return;
            
            // Calculate factor for weekly/monthly (matching Xamarin lines 654-660)
            double factor = 1;
            if (timeFrame == GoalDetailsPageViewModel.GoalTimeFrame.Weekly)
            {
                if (goal.WorkingDays <= 7)
                    factor = 1;
                else
                    factor = 4;
            }
            
            // Calculate UoM factor (matching Xamarin lines 663-696)
            double uomFactor = 1;
            var product = line.Product;
            if (product != null && product.UnitOfMeasures.Count > 0 && line.Type == GoalType.Qty && line.UoM != null)
            {
                ShowUom = true;
                if (line.ChangedUoM != null)
                {
                    bool baseAsDefault = line.UoM.IsBase;
                    if (baseAsDefault)
                        uomFactor = 1 / line.ChangedUoM.Conversion;
                    else
                        uomFactor = line.UoM.Conversion;
                    UomText = $"UoM: {line.ChangedUoM.Name}";
                }
                else
                {
                    UomText = $"UoM: {line.UoM.Name}";
                }
            }
            else
            {
                ShowUom = false;
            }
            
            // Set header text (matching Xamarin line 699-700)
            ItemName = product != null ? product.Name : (line.Name ?? "Item");
            
            // Set label texts based on criteria (matching Xamarin lines 703-731)
            if (goal.Criteria == GoalCriteria.Payment)
            {
                GoalAmountLabel = "Collect";
                SoldLabel = "Paid";
                DailySalesLabel = "Daily Paid To Goal";
                ItemName = "DOC: " + ItemName;
                
                if (line.client != null)
                {
                    ClientName = " " + line.client.ClientName;
                    ShowClientName = true;
                }
                else
                {
                    ShowClientName = false;
                }
            }
            else if (goal.Criteria == GoalCriteria.ProductsByCustomer)
            {
                if (line.client != null)
                {
                    ClientName = " " + line.client.ClientName;
                    ShowClientName = true;
                }
                else
                {
                    ShowClientName = false;
                }
            }
            else
            {
                ShowClientName = false;
            }
            
            // Calculate Sold (matching Xamarin lines 733-934)
            double sold = line.SoldValue * uomFactor;
            
            // Add sales order if checked
            double salesOrder = 0;
            if (parent.IncludeSalesOrders)
                salesOrder = line.SalesOrderValue;
            
            // Add credit invoice if checked
            if (parent.IncludeCreditOrders)
                salesOrder += line.CreditInvoice;
            
            salesOrder *= uomFactor;
            sold += salesOrder;
            
            // Add device orders if checked (simplified - full logic would require iterating Order.Orders)
            // TODO: Implement full IncludeDeviceOrders logic if needed
            
            // Calculate Goal Amount (matching Xamarin line 920)
            double goalAmount = (line.QuantityOrAmountValue / factor) * uomFactor;
            GoalAmountText = GoalToString((int)line.Type, Math.Round(goalAmount, Config.Round));
            
            // Calculate Sold (matching Xamarin line 934)
            SoldText = GoalToString((int)line.Type, Math.Round(sold / factor, Config.Round));
            
            // Calculate Missing (matching Xamarin line 936-937)
            double missingQty = goalAmount - sold;
            MissingText = missingQty > 0 ? GoalToString((int)line.Type, Math.Round(missingQty, Config.Round)) : GoalToString((int)line.Type, 0);
            
            // Calculate Daily Sales (matching Xamarin lines 943-947)
            double dailySales = Math.Round(line.DailySalesToGoal * uomFactor, Config.Round);
            if (parent.IncludeSalesOrders)
                dailySales = Math.Round(line.DailySalesToGoalIncludeSales(parent.IncludeCreditOrders) * uomFactor, Config.Round);
            DailySalesText = GoalToString((int)line.Type, dailySales);
            
            // Calculate Percentage (matching Xamarin line 950)
            double percent = Math.Round((sold / (line.QuantityOrAmountValue * uomFactor)) * 100, Config.Round);
            PercentageText = percent + "%";
            
            // Progress bar value and color (matching Xamarin lines 954-967)
            ProgressValue = percent == 0 ? 1.0 : Math.Min(1.0, percent / 100.0); // Full bar when 0% to show red
            ProgressColor = percent >= 100 ? Colors.DarkGreen : Colors.Red;
        }
        
        // GoalToString method (matching Xamarin line 321-329)
        private string GoalToString(int type, double value)
        {
            if (type == (int)GoalType.Qty)
                return value.ToString();
            else
                return value.ToCustomString();
        }
    }
}

