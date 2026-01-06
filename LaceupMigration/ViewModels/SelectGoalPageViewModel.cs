using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace LaceupMigration.ViewModels
{
    public partial class SelectGoalPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;

        [ObservableProperty] private ObservableCollection<GoalItemViewModel> _goals = new();
        [ObservableProperty] private bool _isLoading;

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
                
                // Load goals from GoalProgressDTO.List
                foreach (var goal in GoalProgressDTO.List.OrderBy(x => x.Name))
                {
                    var progressPercent = goal.WorkingDays > 0 
                        ? Math.Round((goal.Sold / goal.QuantityOrAmount) * 100, 1) 
                        : 0;
                    
                    var statusText = goal.Status switch
                    {
                        GoalProgressDTO.GoalStatus.Expired => "Expired",
                        GoalProgressDTO.GoalStatus.Progressing => "In Progress",
                        GoalProgressDTO.GoalStatus.NoProgress => "Not Started",
                        _ => "Unknown"
                    };

                    Goals.Add(new GoalItemViewModel
                    {
                        GoalId = goal.Id,
                        Name = goal.Name ?? $"Goal {goal.Id}",
                        Description = $"{goal.Type} Goal - {goal.Criteria}",
                        Progress = $"{progressPercent}% Complete ({statusText})",
                        StartDate = goal.StartDate,
                        EndDate = goal.EndDate,
                        Sold = goal.Sold,
                        Target = goal.QuantityOrAmount,
                        Status = goal.Status
                    });
                }

                if (Goals.Count == 0)
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
    }
}
