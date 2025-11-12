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
    public partial class SetupScannerPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;

        [ObservableProperty] private int _currentStep = 1;
        [ObservableProperty] private int _totalSteps = 4;
        [ObservableProperty] private string _stepTitle = string.Empty;
        [ObservableProperty] private string _stepDescription = string.Empty;
        [ObservableProperty] private View _stepContent = null!;
        [ObservableProperty] private string _nextButtonText = "Next";
        [ObservableProperty] private bool _canGoPrevious;

        [ObservableProperty] private string _selectedScannerType = string.Empty;
        [ObservableProperty] private string _scannerName = string.Empty;
        [ObservableProperty] private bool _testScanSuccessful;

        public SetupScannerPageViewModel(DialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
            UpdateStep();
        }

        public async Task OnAppearingAsync()
        {
            CurrentStep = 1;
            UpdateStep();
        }

        private void UpdateStep()
        {
            CanGoPrevious = CurrentStep > 1;
            NextButtonText = CurrentStep == TotalSteps ? "Finish" : "Next";

            switch (CurrentStep)
            {
                case 1:
                    StepTitle = "Scanner Type";
                    StepDescription = "Select the type of scanner you want to configure.";
                    StepContent = CreateScannerTypeStep();
                    break;
                case 2:
                    StepTitle = "Scanner Configuration";
                    StepDescription = "Enter the scanner name and connection details.";
                    StepContent = CreateConfigurationStep();
                    break;
                case 3:
                    StepTitle = "Test Scanner";
                    StepDescription = "Test the scanner to ensure it's working correctly.";
                    StepContent = CreateTestStep();
                    break;
                case 4:
                    StepTitle = "Complete Setup";
                    StepDescription = "Review your settings and complete the setup.";
                    StepContent = CreateCompleteStep();
                    break;
            }
        }

        private View CreateScannerTypeStep()
        {
            var picker = new Picker
            {
                Title = "Scanner Type",
                ItemsSource = new[] { "Symbol EMDK", "DataWedge", "Generic Barcode", "Camera" },
                SelectedItem = SelectedScannerType
            };
            picker.SelectedIndexChanged += (s, e) =>
            {
                if (picker.SelectedItem != null)
                    SelectedScannerType = picker.SelectedItem.ToString() ?? string.Empty;
            };
            return picker;
        }

        private View CreateConfigurationStep()
        {
            var stack = new VerticalStackLayout { Spacing = 12 };
            var entry = new Entry { Placeholder = "Scanner Name", Text = ScannerName };
            entry.TextChanged += (s, e) => ScannerName = e.NewTextValue ?? string.Empty;
            stack.Children.Add(entry);
            return stack;
        }

        private View CreateTestStep()
        {
            var stack = new VerticalStackLayout { Spacing = 12 };
            var testButton = new Button
            {
                Text = "Test Scan",
                Command = TestScanCommand
            };
            stack.Children.Add(testButton);
            
            var resultLabel = new Label
            {
                Text = TestScanSuccessful ? "Scan successful!" : "Press Test Scan to test the scanner",
                TextColor = TestScanSuccessful ? Colors.Green : Colors.Gray
            };
            stack.Children.Add(resultLabel);
            
            return stack;
        }

        private View CreateCompleteStep()
        {
            var stack = new VerticalStackLayout { Spacing = 12 };
            stack.Children.Add(new Label { Text = $"Scanner Type: {SelectedScannerType}", FontSize = 14 });
            stack.Children.Add(new Label { Text = $"Scanner Name: {ScannerName}", FontSize = 14 });
            return stack;
        }

        [RelayCommand]
        private void PreviousStep()
        {
            if (CurrentStep > 1)
            {
                CurrentStep--;
                UpdateStep();
            }
        }

        [RelayCommand]
        private async Task NextStep()
        {
            if (CurrentStep == 1 && string.IsNullOrWhiteSpace(SelectedScannerType))
            {
                await _dialogService.ShowAlertAsync("Please select a scanner type.", "Validation Error", "OK");
                return;
            }

            if (CurrentStep == 2 && string.IsNullOrWhiteSpace(ScannerName))
            {
                await _dialogService.ShowAlertAsync("Please enter a scanner name.", "Validation Error", "OK");
                return;
            }

            if (CurrentStep == TotalSteps)
            {
                await FinishSetup();
            }
            else
            {
                CurrentStep++;
                UpdateStep();
            }
        }

        [RelayCommand]
        private async Task TestScan()
        {
            try
            {
                // TODO: Implement actual scanner test
                // var result = await ScannerService.TestScan();
                // TestScanSuccessful = result != null;
                
                await Task.Delay(500); // Simulate scan
                TestScanSuccessful = true;
                UpdateStep(); // Refresh to show success
                
                await _dialogService.ShowAlertAsync("Scanner test successful!", "Success", "OK");
            }
            catch (Exception ex)
            {
                TestScanSuccessful = false;
                await _dialogService.ShowAlertAsync($"Scanner test failed: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
            }
        }

        private async Task FinishSetup()
        {
            try
            {
                // TODO: Save scanner configuration to Config
                // Config.ScannerType = SelectedScannerType;
                // Config.ScannerName = ScannerName;
                // Config.Save();
                
                await _dialogService.ShowAlertAsync("Scanner setup completed successfully!", "Success", "OK");
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error completing setup: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
            }
        }
    }
}

