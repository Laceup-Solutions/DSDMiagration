using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LaceupMigration.ViewModels
{
    public partial class SelectGoalPageViewModel : ObservableObject
    {
        private static readonly Color GoalRed = Color.FromRgb(229, 115, 115);
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;
        private List<GoalItemViewModel> _allGoals = new();
        private List<GoalCriteria> _currentFilter = new(); // Empty means "All"
        private DateTime _startDateFilter = DateTime.MinValue;
        private DateTime _endDateFilter = DateTime.MinValue;
        private bool _showExpiredGoals = false;

        [ObservableProperty] private ObservableCollection<GoalItemViewModel> _goals = new();
        [ObservableProperty] private ObservableCollection<GoalItemViewModel> _filteredGoals = new();
        [ObservableProperty] private bool _isLoading;
        [ObservableProperty] private string _searchText = string.Empty;
        [ObservableProperty] private bool _isFilterPopupVisible = false;
        
        // Filter state properties
        [ObservableProperty] private bool _filterAll = true;
        [ObservableProperty] private bool _filterRoute = false;
        [ObservableProperty] private bool _filterProduct = false;
        [ObservableProperty] private bool _filterPayment = false;
        [ObservableProperty] private bool _filterCustomer = false;
        [ObservableProperty] private bool _filterProductByCustomer = false;
        [ObservableProperty] private bool _showExpired = false;
        [ObservableProperty] private DateTime? _filterStartDate;
        [ObservableProperty] private DateTime? _filterEndDate;

        public SelectGoalPageViewModel(DialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
        }
        private bool _hasLoadedData = false;
        public async Task OnAppearingAsync()
        {
            if (!_hasLoadedData)
            {
                            IsLoading = true;
            try
            {
                // Initialize date filters to current month (matching Xamarin OnCreate logic)
                if (_startDateFilter == DateTime.MinValue && _endDateFilter == DateTime.MinValue)
                {
                    DateTime now = DateTime.Now;
                    _startDateFilter = new DateTime(now.Year, now.Month, 1); // First day of current month
                    _endDateFilter = _startDateFilter.AddMonths(1).AddDays(-1); // Last day of current month
                }

                await Task.Run(() =>
                {
                    DataProvider.GetGoalProgress();
                    DataProvider.GetGoalProgressDetail();
                });

                Goals.Clear();
                _allGoals.Clear();
                
                // Load goals from GoalProgressDTO.List
                foreach (var goal in GoalProgressDTO.List.OrderBy(x => x.Name))
                {
                    // Calculate percentage including credit invoice (matching Xamarin logic)
                    var sold = goal.Sold + goal.CreditInvoice;
                    var progressPercent = goal.QuantityOrAmount > 0 
                        ? Math.Round((sold / goal.QuantityOrAmount) * 100, Config.Round) 
                        : 0;
                    
                    var statusText = goal.Status switch
                    {
                        GoalProgressDTO.GoalStatus.Expired => "Expired",
                        GoalProgressDTO.GoalStatus.Progressing => "In Progress",
                        GoalProgressDTO.GoalStatus.NoProgress => "Not Started",
                        _ => "Unknown"
                    };

                    var goalItem = new GoalItemViewModel
                    {
                        GoalId = goal.Id,
                        Name = goal.Name ?? $"Goal {goal.Id}",
                        Description = $"{goal.Type} Goal - {goal.Criteria}",
                        Progress = $"{progressPercent}% Complete ({statusText})",
                        StartDate = goal.StartDate,
                        EndDate = goal.EndDate,
                        Sold = goal.Sold,
                        Target = goal.QuantityOrAmount,
                        Status = goal.Status,
                        WorkingDays = goal.WorkingDays,
                        PendingDays = goal.PendingDays,
                        CreditInvoice = goal.CreditInvoice,
                        Criteria = goal.Criteria,
                        Percentage = progressPercent,
                        ProgressColor = progressPercent >= 100 ? Colors.DarkGreen : GoalRed
                    };

                    Goals.Add(goalItem);
                    _allGoals.Add(goalItem);
                }

                _hasLoadedData = true; 
                
                if (_allGoals.Count == 0)
                {
                    await _dialogService.ShowAlertAsync("No goals available.", "Info", "OK");
                }

                FilterGoals(SearchText);
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error loading goals: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
            }
            finally
            {
                IsLoading = false;
            }
            
            }

            FilterGoals(SearchText);
        }

        public void FilterGoals(string searchText)
        {
            FilteredGoals.Clear();

            // Start with all goals
            var filtered = _allGoals.ToList();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var searchCriteria = searchText.ToLowerInvariant();
                filtered = filtered.Where(x =>
                    (x.Name != null && x.Name.ToLower().Contains(searchCriteria)) ||
                    (x.Description != null && x.Description.ToLower().Contains(searchCriteria)) ||
                    (x.Progress != null && x.Progress.ToLower().Contains(searchCriteria))
                ).ToList();
            }

            // Apply criteria filter (matching Xamarin logic - if filter is empty, show all)
            if (_currentFilter.Count > 0)
            {
                var newList = new List<GoalItemViewModel>();
                foreach (var filter in _currentFilter)
                {
                    newList.AddRange(filtered.Where(x => x.Criteria == filter).ToList());
                }
                filtered = newList;
            }

            // Apply date filters (matching Xamarin logic)
            if (_startDateFilter != DateTime.MinValue)
            {
                filtered = filtered.Where(x => x.StartDate.Date >= _startDateFilter.Date).ToList();
            }

            if (_endDateFilter != DateTime.MinValue)
            {
                filtered = filtered.Where(x => x.EndDate.Date <= _endDateFilter.Date).ToList();
            }

            // Apply expired filter
            if (!_showExpiredGoals)
            {
                filtered = filtered.Where(x => x.Status != GoalProgressDTO.GoalStatus.Expired).ToList();
            }

            foreach (var goal in filtered)
            {
                FilteredGoals.Add(goal);
            }
        }

        [RelayCommand]
        private void ShowFilterMenu()
        {
            // Load current filter state into properties
            FilterAll = _currentFilter.Count == 0;
            FilterRoute = _currentFilter.Contains(GoalCriteria.Route);
            FilterProduct = _currentFilter.Contains(GoalCriteria.Product);
            FilterPayment = _currentFilter.Contains(GoalCriteria.Payment);
            FilterCustomer = _currentFilter.Contains(GoalCriteria.Customer);
            FilterProductByCustomer = _currentFilter.Contains(GoalCriteria.ProductsByCustomer);
            ShowExpired = _showExpiredGoals;
            FilterStartDate = _startDateFilter == DateTime.MinValue ? null : _startDateFilter;
            FilterEndDate = _endDateFilter == DateTime.MinValue ? null : _endDateFilter;

            IsFilterPopupVisible = true;
        }

        [RelayCommand]
        private void CloseFilterPopup()
        {
            IsFilterPopupVisible = false;
        }

        [RelayCommand]
        private async Task SelectStartDate()
        {
            var initialDate = FilterStartDate ?? DateTime.Today;
            var date = await _dialogService.ShowDatePickerAsync("Start Date", initialDate, DateTime.MinValue, DateTime.MaxValue);
            if (date.HasValue)
            {
                FilterStartDate = date.Value;
            }
        }

        [RelayCommand]
        private async Task SelectEndDate()
        {
            var initialDate = FilterEndDate ?? DateTime.Today;
            var date = await _dialogService.ShowDatePickerAsync("End Date", initialDate, DateTime.MinValue, DateTime.MaxValue);
            if (date.HasValue)
            {
                FilterEndDate = date.Value;
            }
        }

        partial void OnFilterStartDateChanged(DateTime? value)
        {
            OnPropertyChanged(nameof(StartDateButtonText));
            OnPropertyChanged(nameof(StartDateButtonTextColor));
        }

        partial void OnFilterEndDateChanged(DateTime? value)
        {
            OnPropertyChanged(nameof(EndDateButtonText));
            OnPropertyChanged(nameof(EndDateButtonTextColor));
        }

        public string StartDateButtonText => FilterStartDate.HasValue ? FilterStartDate.Value.ToShortDateString() : string.Empty;
        public Color StartDateButtonTextColor => FilterStartDate.HasValue ? Colors.Black : Colors.Gray;
        public string EndDateButtonText => FilterEndDate.HasValue ? FilterEndDate.Value.ToShortDateString() : string.Empty;
        public Color EndDateButtonTextColor => FilterEndDate.HasValue ? Colors.Black : Colors.Gray;

        [RelayCommand]
        private void RemoveFilters()
        {
            FilterAll = true;
            FilterRoute = false;
            FilterProduct = false;
            FilterPayment = false;
            FilterCustomer = false;
            FilterProductByCustomer = false;
            ShowExpired = false;
            FilterStartDate = null;
            FilterEndDate = null;
        }

        [RelayCommand]
        private void ApplyFilters()
        {
            // Collect selected filters (matching Xamarin logic - all checkboxes are independent)
            _currentFilter.Clear();
            
            // If "All" is checked, add it to the filter list (matching Xamarin: if (all.Checked) filtersToAdd.Add(FilterBy.All))
            // Note: In Xamarin, FilterBy.All is a special value that means "show all" when present in the list
            bool hasAllFilter = FilterAll;
            
            if (FilterRoute) _currentFilter.Add(GoalCriteria.Route);
            if (FilterProduct) _currentFilter.Add(GoalCriteria.Product);
            if (FilterPayment) _currentFilter.Add(GoalCriteria.Payment);
            if (FilterCustomer) _currentFilter.Add(GoalCriteria.Customer);
            if (FilterProductByCustomer) _currentFilter.Add(GoalCriteria.ProductsByCustomer);
            
            // If "All" is checked, clear other filters (since All means show everything)
            // This matches Xamarin's behavior where if FilterBy.All is in the list, it shows all
            if (hasAllFilter)
            {
                _currentFilter.Clear(); // Empty list means "All" in our implementation
            }

            _startDateFilter = FilterStartDate ?? DateTime.MinValue;
            _endDateFilter = FilterEndDate ?? DateTime.MinValue;
            _showExpiredGoals = ShowExpired;

            FilterGoals(SearchText);
            IsFilterPopupVisible = false;
        }

        // Removed the partial methods that prevent multiple checkbox selections
        // In Xamarin, checkboxes are independent - users can check multiple at once
        // The filtering logic handles "All" separately

        public void ApplyFilters(List<GoalCriteria> filter, DateTime startDate, DateTime endDate, bool showExpired)
        {
            _currentFilter = filter ?? new List<GoalCriteria>(); // Empty means "All"
            _startDateFilter = startDate;
            _endDateFilter = endDate;
            _showExpiredGoals = showExpired;
            FilterGoals(SearchText);
        }

        partial void OnSearchTextChanged(string value)
        {
            FilterGoals(value);
        }

        [RelayCommand]
        private async Task SelectGoal(GoalItemViewModel goal)
        {
            if (goal != null)
            {
                var query = new Dictionary<string, object>
                {
                    { "goalId", goal.GoalId },
                    { "goalName", goal.Name }
                };
                await Shell.Current.GoToAsync("goaldetails", query);
            }
        }
    }

    public partial class GoalItemViewModel : ObservableObject
    {
        [ObservableProperty] private int _goalId;
        [ObservableProperty] private string _name = string.Empty;
        [ObservableProperty] private string _description = string.Empty;
        [ObservableProperty] private string _progress = string.Empty;
        [ObservableProperty] private DateTime _startDate;
        [ObservableProperty] private DateTime _endDate;
        [ObservableProperty] private double _sold;
        [ObservableProperty] private double _target;
        [ObservableProperty] private GoalProgressDTO.GoalStatus _status;
        [ObservableProperty] private int _workingDays;
        [ObservableProperty] private int _pendingDays;
        [ObservableProperty] private double _creditInvoice;
        [ObservableProperty] private GoalCriteria _criteria;
        [ObservableProperty] private double _percentage;
        [ObservableProperty] private Color _progressColor = Color.FromRgb(229, 115, 115);

        public string CriteriaText
        {
            get
            {
                return Criteria switch
                {
                    GoalCriteria.Route => "Route",
                    GoalCriteria.Product => "Product",
                    GoalCriteria.Payment => "Payment",
                    GoalCriteria.Customer => "Customer",
                    GoalCriteria.ProductsByCustomer => "Product By Customer",
                    _ => string.Empty
                };
            }
        }

        public string StartDateText => $"Start Date: {StartDate:MM/dd/yyyy}";
        public string EndDateText => $"End Date: {EndDate:MM/dd/yyyy}";
        public string DaysToCompleteText => $"{WorkingDays} Business Days to Complete Goal";
        public string PendingDaysText => $"Pending Days: {PendingDays}";
        public string PercentageText => $"% {Percentage.ToString()}";
        // MAUI ProgressBar expects 0.0-1.0, so convert percentage (0-100) to 0.0-1.0
        // When percentage is 0, show full bar (1.0) with Red color to match Xamarin behavior
        // (Xamarin shows entire bar as red when progress is 0 due to color filter on ProgressDrawable)
        // When percentage > 0, convert to 0.0-1.0 range, capped at 1.0 for values > 100%
        public double ProgressValue => Percentage == 0 ? 1.0 : Math.Min(1.0, Percentage / 100.0);

        // Update ProgressColor when Percentage changes (matching Xamarin color change logic)
        partial void OnPercentageChanged(double value)
        {
            ProgressColor = value >= 100 ? Colors.DarkGreen : Color.FromRgb(229, 115, 115);
            OnPropertyChanged(nameof(ProgressValue));
        }
    }
}
