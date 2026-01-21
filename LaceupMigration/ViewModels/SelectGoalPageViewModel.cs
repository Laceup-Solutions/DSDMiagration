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
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;
        private List<GoalItemViewModel> _allGoals = new();

        [ObservableProperty] private ObservableCollection<GoalItemViewModel> _goals = new();
        [ObservableProperty] private ObservableCollection<GoalItemViewModel> _filteredGoals = new();
        [ObservableProperty] private bool _isLoading;
        [ObservableProperty] private string _searchText = string.Empty;

        public SelectGoalPageViewModel(DialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
        }

        public async Task OnAppearingAsync()
        {
            IsLoading = true;
            try
            {
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
                        ? Math.Round((sold / goal.QuantityOrAmount) * 100, 1) 
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
                        Percentage = progressPercent
                    };
                    
                    Goals.Add(goalItem);
                    _allGoals.Add(goalItem);
                }

                // Apply initial filter
                FilterGoals(SearchText);

                if (_allGoals.Count == 0)
                {
                    await _dialogService.ShowAlertAsync("No goals available.", "Info", "OK");
                }
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

        public void FilterGoals(string searchText)
        {
            FilteredGoals.Clear();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                // No search - show all
                foreach (var goal in _allGoals)
                {
                    FilteredGoals.Add(goal);
                }
                return;
            }

            var searchCriteria = searchText.ToLowerInvariant();
            var filtered = _allGoals.Where(x =>
                (x.Name != null && x.Name.ToLower().Contains(searchCriteria)) ||
                (x.Description != null && x.Description.ToLower().Contains(searchCriteria)) ||
                (x.Progress != null && x.Progress.ToLower().Contains(searchCriteria))
            ).ToList();

            foreach (var goal in filtered)
            {
                FilteredGoals.Add(goal);
            }
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
        public string DaysToCompleteText => $"{WorkingDays} Days to Complete Goal";
        public string PendingDaysText => $"Pending Days: {PendingDays}";
        public string PercentageText => $"% {Percentage:F0}";
        public double ProgressValue => Math.Min(100, Math.Max(0, Percentage));
        public Color ProgressColor => Percentage >= 100 ? Colors.DarkGreen : Colors.Red;
    }
}
