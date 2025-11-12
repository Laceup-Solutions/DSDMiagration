using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LaceupMigration.ViewModels
{
    public partial class SelectTermsPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;
        private List<Term> _allTerms = new();

        [ObservableProperty] private ObservableCollection<TermViewModel> _terms = new();
        [ObservableProperty] private string _searchText = string.Empty;

        public SelectTermsPageViewModel(DialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
        }

        public async Task OnAppearingAsync()
        {
            try
            {
                _allTerms = Term.List.Where(x => x.IsActive).ToList();
                FilterTerms(string.Empty);
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error loading terms: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
            }
        }

        public void FilterTerms(string searchText)
        {
            Terms.Clear();

            var filtered = string.IsNullOrWhiteSpace(searchText)
                ? _allTerms
                : _allTerms.Where(x => x.Name?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true).ToList();

            foreach (var term in filtered)
            {
                var description = $"Due: {term.StandardDueDates} days";
                if (term.StandardDiscountDays > 0)
                {
                    description += $", Discount: {term.DiscountPercentage}% in {term.StandardDiscountDays} days";
                }

                Terms.Add(new TermViewModel
                {
                    Id = term.Id,
                    Name = term.Name ?? $"Term {term.Id}",
                    Description = description
                });
            }
        }

        [RelayCommand]
        private async Task SelectTerm(TermViewModel term)
        {
            if (term != null)
            {
                var result = new Dictionary<string, object>
                {
                    { "termId", term.Id },
                    { "termName", term.Name }
                };

                await Shell.Current.GoToAsync("..", result);
            }
        }
    }

    public partial class TermViewModel : ObservableObject
    {
        [ObservableProperty] private int _id;
        [ObservableProperty] private string _name = string.Empty;
        [ObservableProperty] private string _description = string.Empty;
    }
}

