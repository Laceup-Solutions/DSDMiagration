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
    public partial class ManageDepartmentsPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;
        private Client? _client;
        private bool _initialized;

        public ObservableCollection<DepartmentViewModel> Departments { get; } = new();

        [ObservableProperty]
        private string _clientName = string.Empty;

        [ObservableProperty]
        private bool _showNoDepartments;

        public ManageDepartmentsPageViewModel(DialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
        }

        public async Task InitializeAsync(int clientId)
        {
            if (_initialized && _client?.ClientId == clientId)
            {
                await RefreshAsync();
                return;
            }

            _client = Client.Clients.FirstOrDefault(x => x.ClientId == clientId);
            if (_client == null)
            {
                await _dialogService.ShowAlertAsync("Client not found.", "Error");
                return;
            }

            _initialized = true;
            LoadDepartments();
        }

        public async Task OnAppearingAsync()
        {
            if (!_initialized)
                return;

            await RefreshAsync();
        }

        private async Task RefreshAsync()
        {
            LoadDepartments();
            await Task.CompletedTask;
        }

        private void LoadDepartments()
        {
            if (_client == null)
                return;

            ClientName = _client.ClientName;

            var departments = ClientDepartment.GetDepartmentsForClient(_client);
            Departments.Clear();

            foreach (var dept in departments.OrderBy(x => x.Name))
            {
                Departments.Add(new DepartmentViewModel { Department = dept, Name = dept.Name });
            }

            ShowNoDepartments = Departments.Count == 0;
        }

        [RelayCommand]
        private async Task AddDepartmentAsync()
        {
            if (_client == null)
                return;

            var name = await _dialogService.ShowPromptAsync("Add Department", "Enter department name:", placeholder: "Department name");
            if (string.IsNullOrWhiteSpace(name))
                return;

            var trimmedName = name.Trim();

            // Check if department already exists
            if (Departments.Any(x => x.Name.Equals(trimmedName, StringComparison.OrdinalIgnoreCase)))
            {
                await _dialogService.ShowAlertAsync($"Department '{trimmedName}' already exists for {_client.ClientName}.", "Alert");
                return;
            }

            var newDepartment = ClientDepartment.AddDepartment(trimmedName, _client);
            Departments.Add(new DepartmentViewModel { Department = newDepartment, Name = newDepartment.Name });
            ShowNoDepartments = false;
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            // Departments are saved automatically when added
            await Shell.Current.GoToAsync("..");
        }
    }

    public partial class DepartmentViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _name = string.Empty;

        public ClientDepartment Department { get; set; } = null!;
    }
}

