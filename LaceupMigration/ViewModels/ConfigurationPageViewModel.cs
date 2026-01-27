using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Helpers;
using LaceupMigration.Services;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace LaceupMigration.ViewModels
{
    public partial class ConfigurationPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;

        [ObservableProperty] private string _version = string.Empty;
        [ObservableProperty] private string _internetServer = string.Empty;
        [ObservableProperty] private string _lanServer = string.Empty;
        [ObservableProperty] private string _port = string.Empty;
        [ObservableProperty] private string _ssid = string.Empty;
        [ObservableProperty] private string _vendorName = string.Empty;
        [ObservableProperty] private string _vendorId = string.Empty;
        [ObservableProperty] private string _deviceId = string.Empty;
        [ObservableProperty] private string _currentSSID = string.Empty;
        [ObservableProperty] private string _nextOrderId = string.Empty;
        [ObservableProperty] private bool _canModifyConnectionSettings;
        [ObservableProperty] private bool _canChangeSalesmanId;
        [ObservableProperty] private bool _showPrinterButton;
        [ObservableProperty] private bool _showScannerButton;

        public ConfigurationPageViewModel(DialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
        }

        public async Task OnAppearingAsync()
        {
            Version = Config.Version;
            InternetServer = ServerHelper.GetServerNumber(Config.IPAddressGateway);
            LanServer = Config.LanAddress;
            Port = Config.Port.ToString(CultureInfo.InvariantCulture);
            Ssid = Config.SSID;
            VendorName = Config.VendorName;
            VendorId = Config.SalesmanId.ToString(CultureInfo.InvariantCulture);
            DeviceId = Config.DeviceId;
            CurrentSSID = $"[{Config.GetSSID()}]";

            CanModifyConnectionSettings = Config.CanModifyConnectSett;
            CanChangeSalesmanId = Config.CanChangeSalesmanId && Config.SupervisorId == 0;
            ShowPrinterButton = Config.PrinterAvailable;
            // Show scanner button - matches Xamarin: only hide if ScannerToUse != 3
            // Default ScannerToUse is 3, so button should be visible by default
            ShowScannerButton = Config.ScannerToUse == 3;
        }

        [RelayCommand]
        private async Task ShowConnectionSettings()
        {
            // Show connection settings popup - matches Xamarin behavior
            await ShowConnectionSettingsDialogAsync();
        }

        private async Task ShowConnectionSettingsDialogAsync()
        {
            try
            {
                var page = Shell.Current?.CurrentPage ?? Application.Current?.Windows?.FirstOrDefault()?.Page;
                if (page == null)
                    return;

                // Create entry fields for connection settings
                var internetServerEntry = new Entry
                {
                    Text = InternetServer,
                    Placeholder = "Internet server",
                    FontSize = 16,
                    BackgroundColor = Colors.White,
                    IsEnabled = CanModifyConnectionSettings
                };

                var lanServerEntry = new Entry
                {
                    Text = LanServer,
                    Placeholder = "LAN server",
                    FontSize = 16,
                    BackgroundColor = Colors.White,
                    IsEnabled = CanModifyConnectionSettings
                };

                var ssidEntry = new Entry
                {
                    Text = Ssid,
                    Placeholder = "SSID",
                    FontSize = 16,
                    BackgroundColor = Colors.White,
                    IsEnabled = CanModifyConnectionSettings
                };

                // Create content layout
                var content = new VerticalStackLayout
                {
                    Spacing = 15,
                    Padding = 20,
                    BackgroundColor = Colors.White,
                    Children =
                    {
                        new Label
                        {
                            Text = "Internet server:",
                            FontSize = 14,
                            TextColor = Colors.Black
                        },
                        internetServerEntry,
                        new Label
                        {
                            Text = "LAN server:",
                            FontSize = 14,
                            TextColor = Colors.Black
                        },
                        lanServerEntry,
                        new Label
                        {
                            Text = "SSID:",
                            FontSize = 14,
                            TextColor = Colors.Black
                        },
                        ssidEntry
                    }
                };

                var scrollContent = new ScrollView
                {
                    Content = content
                };

                var mainContainer = new VerticalStackLayout
                {
                    Spacing = 0,
                    BackgroundColor = Colors.White,
                    Children =
                    {
                        new Label
                        {
                            Text = "Connection Settings",
                            FontSize = 18,
                            FontAttributes = FontAttributes.Bold,
                            TextColor = Microsoft.Maui.Graphics.Color.FromArgb("#017CBA"),
                            Padding = new Thickness(20, 20, 20, 15),
                            BackgroundColor = Colors.White
                        },
                        new BoxView { HeightRequest = 1, Color = Microsoft.Maui.Graphics.Color.FromArgb("#E0E0E0") },
                        scrollContent
                    }
                };

                // Set width to 80% of screen width (regardless of screen size)
                var screenWidth = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;
                var dialogBorder = new Border
                {
                    BackgroundColor = Colors.White,
                    StrokeThickness = 0,
                    Padding = 0,
                    Margin = new Thickness(20),
                    WidthRequest = screenWidth * 0.80,
                    MaximumHeightRequest = DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density * 0.9,
                    StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(8) },
                    Content = mainContainer
                };

                var overlayGrid = new Grid
                {
                    BackgroundColor = Microsoft.Maui.Graphics.Color.FromArgb("#80000000"),
                    RowDefinitions = new RowDefinitionCollection
                    {
                        new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                        new RowDefinition { Height = GridLength.Auto },
                        new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }
                    },
                    ColumnDefinitions = new ColumnDefinitionCollection
                    {
                        new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                        new ColumnDefinition { Width = GridLength.Auto },
                        new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
                    }
                };

                Grid.SetRow(dialogBorder, 1);
                Grid.SetColumn(dialogBorder, 1);
                overlayGrid.Children.Add(dialogBorder);

                // Create buttons
                var okButton = new Button
                {
                    Text = "OK",
                    BackgroundColor = Microsoft.Maui.Graphics.Color.FromArgb("#017CBA"),
                    TextColor = Colors.White,
                    FontSize = 16,
                    Margin = new Thickness(5, 0, 0, 10),
                    CornerRadius = 0
                };

                var cancelButton = new Button
                {
                    Text = "Cancel",
                    BackgroundColor = Colors.Gray,
                    TextColor = Colors.White,
                    FontSize = 16,
                    Margin = new Thickness(0, 0, 5, 10),
                    CornerRadius = 0
                };

                var buttonRow = new Grid
                {
                    ColumnDefinitions = new ColumnDefinitionCollection
                    {
                        new ColumnDefinition { Width = GridLength.Star },
                        new ColumnDefinition { Width = GridLength.Star }
                    },
                    ColumnSpacing = 0,
                    Padding = new Thickness(0),
                    BackgroundColor = Colors.White
                };

                Grid.SetColumn(cancelButton, 0);
                Grid.SetColumn(okButton, 1);
                buttonRow.Children.Add(cancelButton);
                buttonRow.Children.Add(okButton);

                content.Children.Add(new BoxView { HeightRequest = 1, Color = Microsoft.Maui.Graphics.Color.FromArgb("#E0E0E0") });
                content.Children.Add(buttonRow);

                var dialog = new ContentPage
                {
                    BackgroundColor = Colors.Transparent,
                    Content = overlayGrid
                };

                var tcs = new TaskCompletionSource<bool>();

                async Task SafePopModalAsync()
                {
                    try
                    {
                        if (page.Navigation?.ModalStack.Count > 0)
                        {
                            await page.Navigation.PopModalAsync();
                        }
                    }
                    catch { }
                }

                okButton.Clicked += async (s, e) =>
                {
                    // Update connection settings
                    Config.IPAddressGateway = InternetServer = internetServerEntry.Text;
                    Config.LanAddress = LanServer = lanServerEntry.Text;
                    Config.SSID = Ssid = ssidEntry.Text;
                    
                    await SafePopModalAsync();
                    tcs.SetResult(true);
                };

                cancelButton.Clicked += async (s, e) =>
                {
                    await SafePopModalAsync();
                    tcs.SetResult(false);
                };

                await page.Navigation.PushModalAsync(dialog);
                internetServerEntry.Focus();

                await tcs.Task;
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                await _dialogService.ShowAlertAsync(
                    $"Error showing connection settings: {ex.Message}",
                    "Error",
                    "OK");
            }
        }

        [RelayCommand]
        private async Task Save()
        {
            try
            {
                if (CanModifyConnectionSettings)
                {
                    // Save connection settings
                    if (!string.IsNullOrEmpty(InternetServer))
                    {
                        Config.IPAddressGateway = InternetServer;
                    }
                    if (!string.IsNullOrEmpty(LanServer))
                    {
                        Config.LanAddress = LanServer;
                    }
                    if (!string.IsNullOrEmpty(Ssid))
                    {
                        Config.SSID = Ssid;
                    }
                    
                    int _port = 0;
                    
                    Int32.TryParse(Port, out _port);
                    
                    Config.Port = _port;
                    
                    var server_port_config = await ServerHelper.GetIdForServer(Config.IPAddressGateway, Config.Port);
                    Config.IPAddressGateway = server_port_config.Item1;
                    Config.Port = server_port_config.Item2;
                    
                    Config.SaveSystemSettings();
                }

                if (CanChangeSalesmanId)
                {
                    if (int.TryParse(VendorId, out var salesmanId))
                    {
                        Config.SalesmanId = salesmanId;
                    }
                    
                    Config.VendorName = VendorName;
                }

                if (CanModifyConnectionSettings || CanChangeSalesmanId)
                {
                    Config.SaveSettings();
                    Config.SaveSystemSettings();
                }
                

                await _dialogService.ShowAlertAsync("Configuration saved successfully.", "Success", "OK");
                
                // Remove from navigation state so app doesn't restore to Configuration after force quit
                NavigationHelper.RemoveNavigationState("configuration");
                // Navigate back after successful save
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error saving configuration: {ex.Message}", "Error", "OK");
            }
        }

        [RelayCommand]
        private async Task SetupPrinter()
        {
            
            PrinterProvider.PrinterAddress = string.Empty;
            if (Config.PrinterAvailable)
            {
                var printers = PrinterProvider.AvailablePrinters();
                switch (printers.Count)
                {
                    case 0:
                        await _dialogService.ShowAlertAsync("No printers found.", "Info", "OK");
                        break;
                    case 1:
                        PrinterProvider.PrinterAddress = printers[0].Address;
                        ConfigurePrinter();
                        break;
                    default:
                        SelectPrinter(printers, true, ConfigurePrinter);
                        break;
                }
            }

        }
        
        async Task ConfigurePrinter()
        {
            await _dialogService.ShowLoadingAsync("Configuring printer...");
            try
            {
                var zebra = PrinterProvider.CurrentPrinter();

                var success = await Task.Run(() => zebra.ConfigurePrinter());
                
                await _dialogService.HideLoadingAsync();
                    
                if (!success)
                {
                   await _dialogService.ShowAlertAsync("Error setting up printer", "Info", "OK");
                }
                else
                  await _dialogService.ShowAlertAsync("Printer Configured", "OK");
            }
            catch (Exception ex)
            {
                await _dialogService.HideLoadingAsync();
                await _dialogService.ShowAlertAsync($"Error: {ex.Message}", "Info", "OK");
            }

        }
        
        async Task SelectPrinter(IList<PrinterDescription> printers, bool preOrder, Func<Task>  doit)
        {
            var printerNames = printers.Select(x => x.Name).ToArray();
    
            var selectedOption = await _dialogService.ShowActionSheetAsync(
                "Select Printer",
                null,
                "Cancel",
                printerNames);
    
            if (selectedOption != "Cancel" && selectedOption != null)
            {
                var selectedPrinter = printers.FirstOrDefault(p => p.Name == selectedOption);
                if (selectedPrinter != null)
                {
                    PrinterProvider.PrinterAddress = selectedPrinter.Address;
                    await doit();
                }
            }
        }

        [RelayCommand]
        private async Task SetupScanner()
        {
            try
            {
                // Matches Xamarin ConfigurationLayoutSelectScanner_Click exactly
                var scannerNames = new List<string>
                {
                    "Zebra Handheld",
                    "Socket Scanner",
                    "Honeywell",
                    "Zebra Bluetooth"
                };

                // Determine current selection based on Config.ScannerToUse - matches Xamarin switch statement
                int currentSelection = -1;
                switch (Config.ScannerToUse)
                {
                    case 2:
                        currentSelection = 0; // Zebra Handheld
                        break;
                    case 3:
                        currentSelection = 1; // Socket Scanner
                        break;
                    case 7:
                        currentSelection = 2; // Honeywell
                        break;
                    case 8:
                        currentSelection = 3; // Zebra Bluetooth
                        break;
                    default:
                        currentSelection = 1; // Default to Socket Scanner
                        break;
                }

                // Show single-choice selection dialog
                var selectedIndex = await ShowSingleChoiceDialogAsync("Select Scanner", scannerNames.ToArray(), currentSelection);
                
                if (selectedIndex < 0)
                {
                    // User cancelled
                    return;
                }

                var oldScanner = Config.ScannerToUse;
                var selected = scannerNames[selectedIndex];

                // Set Config.ScannerToUse based on selection - matches Xamarin switch statement
                switch (selected)
                {
                    case "Zebra Handheld":
                        Config.ScannerToUse = 2;
                        break;
                    case "Socket Scanner":
                        Config.ScannerToUse = 3;
                        break;
                    case "Honeywell":
                        Config.ScannerToUse = 7;
                        break;
                    case "Zebra Bluetooth":
                        Config.ScannerToUse = 8;
                        break;
                }

                // Save and send if changed - matches Xamarin behavior
                if (oldScanner != Config.ScannerToUse)
                {
                    Config.SaveSettings();
                    DataProvider.SendScannerToUse();
                    
                    // Update scanner button visibility
                    ShowScannerButton = Config.ScannerToUse == 3;
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                await _dialogService.ShowAlertAsync(
                    "Could not update scanner.",
                    "Error",
                    "OK");
            }
        }

        private async Task<int> ShowSingleChoiceDialogAsync(string title, string[] options, int selectedIndex)
        {
            try
            {
                var page = Shell.Current?.CurrentPage ?? Application.Current?.Windows?.FirstOrDefault()?.Page;
                if (page == null)
                    return -1;

                var tcs = new TaskCompletionSource<int>();
                int currentSelection = selectedIndex;

                // Create radio button list
                var radioButtons = new List<RadioButton>();
                var optionsLayout = new VerticalStackLayout
                {
                    Spacing = 10,
                    Padding = 20
                };

                for (int i = 0; i < options.Length; i++)
                {
                    var radioButton = new RadioButton
                    {
                        Content = options[i],
                        IsChecked = i == selectedIndex
                    };

                    int index = i; // Capture for closure
                    radioButton.CheckedChanged += (s, e) =>
                    {
                        if (radioButton.IsChecked)
                        {
                            // Uncheck others
                            foreach (var rb in radioButtons)
                            {
                                if (rb != radioButton)
                                    rb.IsChecked = false;
                            }
                            currentSelection = index;
                        }
                    };

                    radioButtons.Add(radioButton);
                    optionsLayout.Children.Add(radioButton);
                }

                var scrollContent = new ScrollView
                {
                    Content = optionsLayout,
                    MaximumWidthRequest = 400
                };

                var mainContainer = new VerticalStackLayout
                {
                    Spacing = 0,
                    BackgroundColor = Colors.White,
                    Children =
                    {
                        new Label
                        {
                            Text = title,
                            FontSize = 18,
                            FontAttributes = FontAttributes.Bold,
                            TextColor = Colors.Black,
                            Padding = new Thickness(20, 20, 20, 15),
                            BackgroundColor = Colors.White
                        },
                        new BoxView { HeightRequest = 1, Color = Microsoft.Maui.Graphics.Color.FromArgb("#E0E0E0") },
                        scrollContent
                    }
                };

                var dialogBorder = new Border
                {
                    BackgroundColor = Colors.White,
                    StrokeThickness = 0,
                    Padding = 0,
                    Margin = new Thickness(20),
                    StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(8) },
                    Content = mainContainer
                };

                var overlayGrid = new Grid
                {
                    BackgroundColor = Microsoft.Maui.Graphics.Color.FromArgb("#80000000"),
                    RowDefinitions = new RowDefinitionCollection
                    {
                        new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                        new RowDefinition { Height = GridLength.Auto },
                        new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }
                    },
                    ColumnDefinitions = new ColumnDefinitionCollection
                    {
                        new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                        new ColumnDefinition { Width = GridLength.Auto },
                        new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
                    }
                };

                Grid.SetRow(dialogBorder, 1);
                Grid.SetColumn(dialogBorder, 1);
                overlayGrid.Children.Add(dialogBorder);

                // Create buttons
                var okButton = new Button
                {
                    Text = "OK",
                    BackgroundColor = Microsoft.Maui.Graphics.Color.FromArgb("#017CBA"),
                    TextColor = Colors.White,
                    FontSize = 16,
                    Margin = new Thickness(5, 0, 0, 10),
                    CornerRadius = 0
                };

                var cancelButton = new Button
                {
                    Text = "Cancel",
                    BackgroundColor = Colors.Gray,
                    TextColor = Colors.White,
                    FontSize = 16,
                    Margin = new Thickness(0, 0, 5, 10),
                    CornerRadius = 0
                };

                var buttonRow = new Grid
                {
                    ColumnDefinitions = new ColumnDefinitionCollection
                    {
                        new ColumnDefinition { Width = GridLength.Star },
                        new ColumnDefinition { Width = GridLength.Star }
                    },
                    ColumnSpacing = 0,
                    Padding = new Thickness(0),
                    BackgroundColor = Colors.White
                };

                Grid.SetColumn(cancelButton, 0);
                Grid.SetColumn(okButton, 1);
                buttonRow.Children.Add(cancelButton);
                buttonRow.Children.Add(okButton);

                optionsLayout.Children.Add(new BoxView { HeightRequest = 1, Color = Microsoft.Maui.Graphics.Color.FromArgb("#E0E0E0") });
                optionsLayout.Children.Add(buttonRow);

                var dialog = new ContentPage
                {
                    BackgroundColor = Colors.Transparent,
                    Content = overlayGrid
                };

                async Task SafePopModalAsync()
                {
                    try
                    {
                        if (page.Navigation?.ModalStack.Count > 0)
                        {
                            await page.Navigation.PopModalAsync();
                        }
                    }
                    catch { }
                }

                okButton.Clicked += async (s, e) =>
                {
                    await SafePopModalAsync();
                    tcs.SetResult(currentSelection);
                };

                cancelButton.Clicked += async (s, e) =>
                {
                    await SafePopModalAsync();
                    tcs.SetResult(-1);
                };

                await page.Navigation.PushModalAsync(dialog);
                return await tcs.Task;
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                return -1;
            }
        }

        [RelayCommand]
        private async Task SelectLogo()
        {
            try
            {
                // Open file picker to select logo - matches Xamarin SelectLogo() method
                // Directly opens file picker without asking about deleting existing logo
                var fileResult = await FilePicker.Default.PickAsync(new PickOptions
                {
                    PickerTitle = "Select Logo",
                    FileTypes = new FilePickerFileType(
                        new Dictionary<DevicePlatform, IEnumerable<string>>
                        {
                            { DevicePlatform.Android, new[] { "image/*", ".jpg", ".jpeg", ".png", ".gif" } },
                            { DevicePlatform.iOS, new[] { "public.image", ".jpg", ".jpeg", ".png", ".gif" } },
                            { DevicePlatform.WinUI, new[] { ".jpg", ".jpeg", ".png", ".gif" } }
                        })
                });
                
                if (fileResult == null)
                {
                    // User cancelled file selection
                    return;
                }
                
                // Process and save the logo file (this will replace any existing logo)
                await ProcessLogoFile(fileResult.FullPath);
                
                await _dialogService.ShowAlertAsync("Logo selected successfully.", "Success", "OK");
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                await _dialogService.ShowAlertAsync(
                    $"Error selecting logo: {ex.Message}",
                    "Error",
                    "OK");
            }
        }
        
        private async Task ProcessLogoFile(string sourceFilePath)
        {
            await Task.Run(() =>
            {
                try
                {
                    // Copy file to LogoStorePath
                    if (File.Exists(Config.LogoStorePath))
                    {
                        File.Delete(Config.LogoStorePath);
                    }
                    
                    File.Copy(sourceFilePath, Config.LogoStorePath, true);
                    
                    // Process the image to convert it to hex format for Config.CompanyLogo
                    // This matches the logic in DataAccess.cs when receiving logo from server
                    ProcessLogoImage(Config.LogoStorePath);
                }
                catch (Exception ex)
                {
                    Logger.CreateLog(ex);
                    throw;
                }
            });
        }
        
        private void ProcessLogoImage(string logoFilePath)
        {
            try
            {
                // This matches the logo processing logic from DataAccess.cs
                // Convert image to grayscale bitmap and store as hex string
                const int MAX_W = 300;
                const int MAX_H = 300;
                
                var dOpt = new DecoderOptions
                {
                    TargetSize = new SixLabors.ImageSharp.Size(MAX_W, MAX_H),
                    SkipMetadata = true,
                    Sampler = KnownResamplers.NearestNeighbor
                };
                
                using var fs = File.OpenRead(logoFilePath);
                using var img = SixLabors.ImageSharp.Image.Load<L8>(dOpt, fs);
                
                int paddedW = (img.Width + 31) & ~31;
                int paddedH = (img.Height + 31) & ~31;
                
                img.Configuration.PreferContiguousImageBuffers = true;
                if (!img.DangerousTryGetSinglePixelMemory(out Memory<L8> mem))
                    throw new InvalidOperationException("Pixel memory not contiguous");
                
                int bytesPerRow = paddedW / 8;
                byte[] raw = new byte[paddedH * bytesPerRow];
                
                Span<L8> src = mem.Span;
                for (int y = 0; y < img.Height; y++)
                {
                    int srcRow = y * img.Width;
                    int dstIdx = y * bytesPerRow;
                    
                    byte acc = 0;
                    int bit = 7;
                    for (int x = 0; x < img.Width; x++)
                    {
                        if (src[srcRow + x].PackedValue < 128)
                            acc |= (byte)(1 << bit);
                        
                        if (--bit < 0)
                        {
                            raw[dstIdx++] = acc;
                            acc = 0;
                            bit = 7;
                        }
                    }
                    
                    if (bit != 7) raw[dstIdx] = acc;
                }
                
                // Store the processed logo data - matches DataAccess.cs logic
                Config.CompanyLogo = BitConverter.ToString(raw).Replace("-", "");
                Config.CompanyLogoWidth = bytesPerRow;
                Config.CompanyLogoHeight = paddedH;
                Config.CompanyLogoSize = raw.Length;
                
                // Save the config to persist the logo
                Config.SaveSettings();
            }
            catch (Exception ex)
            {
                Logger.CreateLog($"Error processing logo image: {ex.Message}");
                throw;
            }
        }

        [RelayCommand]
        private async Task ResetPassword()
        {
            try
            {
                // Show action sheet with two options - matches Xamarin ResetPassword_Click
                var options = new string[]
                {
                    "Reset Set Inv. password",
                    "Reset Add Inv. password"
                };

                var selectedOption = await _dialogService.ShowActionSheetAsync(
                    "Reset Password",
                    null,
                    "Cancel",
                    options);

                if (selectedOption == "Cancel" || string.IsNullOrEmpty(selectedOption))
                    return;

                // Determine which password to reset
                if (selectedOption == options[0])
                {
                    // Reset Set Inv. password (InventoryPassword)
                    await ShowChangePasswordDialog(Config.InventoryPassword, (newPassword) =>
                    {
                        Config.InventoryPassword = newPassword;
                        Config.SaveSettings();
                    });
                }
                else if (selectedOption == options[1])
                {
                    // Reset Add Inv. password (AddInventoryPassword)
                    await ShowChangePasswordDialog(Config.AddInventoryPassword, (newPassword) =>
                    {
                        Config.AddInventoryPassword = newPassword;
                        Config.SaveSettings();
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                await _dialogService.ShowAlertAsync(
                    $"Error resetting password: {ex.Message}",
                    "Error",
                    "OK");
            }
        }

        private async Task ShowChangePasswordDialog(string currentPassword, Action<string> positiveAction)
        {
            try
            {
                var page = Shell.Current?.CurrentPage ?? Application.Current?.Windows?.FirstOrDefault()?.Page;
                if (page == null)
                    return;

                // Create password entry fields - matches Xamarin ShowChangePasswordDialog
                var currentPasswordEntry = new Entry
                {
                    Placeholder = "Current Password",
                    IsPassword = true,
                    FontSize = 16,
                    BackgroundColor = Colors.White
                };

                var newPasswordEntry = new Entry
                {
                    Placeholder = "New Password",
                    IsPassword = true,
                    FontSize = 16,
                    BackgroundColor = Colors.White
                };

                var confirmPasswordEntry = new Entry
                {
                    Placeholder = "Confirm New Password",
                    IsPassword = true,
                    FontSize = 16,
                    BackgroundColor = Colors.White
                };

                // Create content layout
                var content = new VerticalStackLayout
                {
                    Spacing = 15,
                    Padding = 20,
                    BackgroundColor = Colors.White,
                    Children =
                    {
                        new Label
                        {
                            Text = "Current Password:",
                            FontSize = 14,
                            TextColor = Colors.Black
                        },
                        currentPasswordEntry,
                        new Label
                        {
                            Text = "New Password:",
                            FontSize = 14,
                            TextColor = Colors.Black
                        },
                        newPasswordEntry,
                        new Label
                        {
                            Text = "Confirm New Password:",
                            FontSize = 14,
                            TextColor = Colors.Black
                        },
                        confirmPasswordEntry
                    }
                };

                var scrollContent = new ScrollView
                {
                    Content = content,
                    MaximumWidthRequest = 400
                };

                var mainContainer = new VerticalStackLayout
                {
                    Spacing = 0,
                    BackgroundColor = Colors.White,
                    Children =
                    {
                        new Label
                        {
                            Text = "Change Password",
                            FontSize = 18,
                            FontAttributes = FontAttributes.Bold,
                            TextColor = Colors.Black,
                            Padding = new Thickness(20, 20, 20, 15),
                            BackgroundColor = Colors.White
                        },
                        new BoxView { HeightRequest = 1, Color = Microsoft.Maui.Graphics.Color.FromArgb("#E0E0E0") },
                        scrollContent
                    }
                };

                var dialogBorder = new Border
                {
                    BackgroundColor = Colors.White,
                    StrokeThickness = 0,
                    Padding = 0,
                    Margin = new Thickness(20),
                    StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(8) },
                    Content = mainContainer
                };

                var overlayGrid = new Grid
                {
                    BackgroundColor = Microsoft.Maui.Graphics.Color.FromArgb("#80000000"),
                    RowDefinitions = new RowDefinitionCollection
                    {
                        new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                        new RowDefinition { Height = GridLength.Auto },
                        new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }
                    },
                    ColumnDefinitions = new ColumnDefinitionCollection
                    {
                        new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                        new ColumnDefinition { Width = GridLength.Auto },
                        new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
                    }
                };

                Grid.SetRow(dialogBorder, 1);
                Grid.SetColumn(dialogBorder, 1);
                overlayGrid.Children.Add(dialogBorder);

                // Create buttons
                var changeButton = new Button
                {
                    Text = "Change",
                    BackgroundColor = Microsoft.Maui.Graphics.Color.FromArgb("#017CBA"),
                    TextColor = Colors.White,
                    FontSize = 16,
                    Margin = new Thickness(5, 0, 0, 10),
                    CornerRadius = 0
                };

                var cancelButton = new Button
                {
                    Text = "Cancel",
                    BackgroundColor = Colors.Gray,
                    TextColor = Colors.White,
                    FontSize = 16,
                    Margin = new Thickness(0, 0, 5, 10),
                    CornerRadius = 0
                };

                var buttonRow = new Grid
                {
                    ColumnDefinitions = new ColumnDefinitionCollection
                    {
                        new ColumnDefinition { Width = GridLength.Star },
                        new ColumnDefinition { Width = GridLength.Star }
                    },
                    ColumnSpacing = 0,
                    Padding = new Thickness(0),
                    BackgroundColor = Colors.White
                };

                Grid.SetColumn(cancelButton, 0);
                Grid.SetColumn(changeButton, 1);
                buttonRow.Children.Add(cancelButton);
                buttonRow.Children.Add(changeButton);

                content.Children.Add(new BoxView { HeightRequest = 1, Color = Microsoft.Maui.Graphics.Color.FromArgb("#E0E0E0") });
                content.Children.Add(buttonRow);

                var dialog = new ContentPage
                {
                    BackgroundColor = Colors.Transparent,
                    Content = overlayGrid
                };

                var tcs = new TaskCompletionSource<bool>();

                async Task SafePopModalAsync()
                {
                    try
                    {
                        if (page.Navigation?.ModalStack.Count > 0)
                        {
                            await page.Navigation.PopModalAsync();
                        }
                    }
                    catch { }
                }

                changeButton.Clicked += async (s, e) =>
                {
                    // Validate passwords - matches Xamarin ShowChangePasswordDialog logic
                    if (currentPasswordEntry.Text != currentPassword)
                    {
                        await SafePopModalAsync();
                        await _dialogService.ShowAlertAsync("The current password is incorrect", "Alert", "OK");
                        tcs.SetResult(false);
                        return;
                    }

                    if (newPasswordEntry.Text != confirmPasswordEntry.Text)
                    {
                        await SafePopModalAsync();
                        await _dialogService.ShowAlertAsync("The new passwords are different", "Alert", "OK");
                        tcs.SetResult(false);
                        return;
                    }

                    await SafePopModalAsync();
                    positiveAction(newPasswordEntry.Text);
                    tcs.SetResult(true);
                };

                cancelButton.Clicked += async (s, e) =>
                {
                    await SafePopModalAsync();
                    tcs.SetResult(false);
                };

                await page.Navigation.PushModalAsync(dialog);
                currentPasswordEntry.Focus();

                await tcs.Task;
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                await _dialogService.ShowAlertAsync(
                    $"Error showing password dialog: {ex.Message}",
                    "Error",
                    "OK");
            }
        }

        [RelayCommand]
        private async Task SeeLog()
        {
            await Shell.Current.GoToAsync("logviewer");
        }

        [RelayCommand]
        private async Task SendLog()
        {
            await _appService.SendLogAsync();
            await _dialogService.ShowAlertAsync("Log sent.", "Info", "OK");
        }

        [RelayCommand]
        private async Task ExportData()
        {
            await _appService.ExportDataAsync();
            await _dialogService.ShowAlertAsync("Data exported.", "Info", "OK");
        }

        [RelayCommand]
        private async Task RestoreData()
        {
            try
            {
                // Show confirmation dialog - matches Xamarin behavior
                var confirmed = await _dialogService.ShowConfirmationAsync(
                    "Restore Data",
                    "This will replace all current data with the data from the selected file. Are you sure?",
                    "Yes",
                    "No");

                if (!confirmed)
                    return;

                // Show loading while getting backup sessions from server
                await _dialogService.ShowLoadingAsync("Loading backup sessions...");

                // Get available backup sessions from server - matches Xamarin "Select session" popup
                List<(string sessionId, string displayName)> backupSessions = null;
                try
                {
                    backupSessions = await _appService.GetAvailableBackupSessionsAsync();
                }
                catch (Exception ex)
                {
                    // Log error but don't return - we still want to show the popup (even if empty)
                    Logger.CreateLog($"Error loading backup sessions: {ex.Message}");
                    _appService.TrackError(ex);
                    backupSessions = new List<(string sessionId, string displayName)>();
                }
                finally
                {
                    await _dialogService.HideLoadingAsync();
                }
                
                string selectedBackupPath = null;

                // Always show the "Select Session" popup, even if the list is empty - matches Xamarin behavior
                if (backupSessions == null)
                {
                    backupSessions = new List<(string sessionId, string displayName)>();
                }

                // Extract display names for the selection dialog (empty array if no sessions)
                var sessionOptions = backupSessions.Select(s => s.displayName).ToArray();

                // Show selection dialog - matches Xamarin "Select session" popup
                // This will show even with an empty list (will show just "Cancel" button)
                var selectedIndex = await _dialogService.ShowSelectionAsync("Select Session", sessionOptions);
                
                if (selectedIndex >= 0 && selectedIndex < backupSessions.Count)
                {
                    // Download the selected backup file from server
                    await _dialogService.ShowLoadingAsync("Downloading backup...");
                    try
                    {
                        selectedBackupPath = await _appService.DownloadBackupFileAsync(backupSessions[selectedIndex].sessionId);
                    }
                    catch (Exception ex)
                    {
                        await _dialogService.HideLoadingAsync();
                        await _dialogService.ShowAlertAsync(
                            $"Error downloading backup: {ex.Message}",
                            "Error",
                            "OK");
                        _appService.TrackError(ex);
                        return;
                    }
                    finally
                    {
                        await _dialogService.HideLoadingAsync();
                    }
                }
                else
                {
                    // User cancelled selection or no sessions available, fall back to file picker
                    var fileResult = await FilePicker.Default.PickAsync(new PickOptions
                    {
                        PickerTitle = "Select data file to restore",
                        FileTypes = new FilePickerFileType(
                            new Dictionary<DevicePlatform, IEnumerable<string>>
                            {
                                { DevicePlatform.Android, new[] { "application/zip", "application/x-zip-compressed", ".zip" } },
                                { DevicePlatform.iOS, new[] { "public.zip-archive", ".zip" } },
                                { DevicePlatform.WinUI, new[] { ".zip" } }
                            })
                    });

                    if (fileResult == null)
                        return;

                    selectedBackupPath = fileResult.FullPath;
                }

                // Show loading dialog
                await _dialogService.ShowLoadingAsync("Restoring data...");

                try
                {
                    // Restore data from selected backup file
                    await _appService.RestoreDataAsync(selectedBackupPath);

                    await _dialogService.HideLoadingAsync();

                    // Show success message
                    await _dialogService.ShowAlertAsync(
                        "Data restored successfully. The application will now reload.",
                        "Success",
                        "OK");

                    // Navigate to MainPage to reload the app - matches Xamarin behavior
                    await Shell.Current.GoToAsync("///MainPage");
                }
                catch (Exception ex)
                {
                    await _dialogService.HideLoadingAsync();
                    await _dialogService.ShowAlertAsync(
                        $"Error restoring data: {ex.Message}",
                        "Error",
                        "OK");
                    _appService.TrackError(ex);
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync(
                    $"Error selecting file: {ex.Message}",
                    "Error",
                    "OK");
                _appService.TrackError(ex);
            }
        }

        [RelayCommand]
        private async Task CleanData()
        {
            // Match Xamarin CleanButton_Click (line 769-793)
            // Show action sheet with 3 options: Clear Log, Clear Data, Clear All
            var selected = await _dialogService.ShowActionSheetAsync(
                "Clear",
                null,
                "Cancel",
                "Clear Log",
                "Clear Data",
                "Clear All");

            if (selected == "Cancel" || string.IsNullOrEmpty(selected))
                return;

            if (selected == "Clear Log")
            {
                // Match Xamarin ClearLogFile (line 795-799)
                _appService.RecordEvent("Clear log button");
                Logger.ClearFile();
                await _dialogService.ShowAlertAsync("Log cleared.", "Info", "OK");
            }
            else if (selected == "Clear Data")
            {
                // Match Xamarin ClearData(true, false) - line 785
                await ClearDataAsync(false);
            }
            else if (selected == "Clear All")
            {
                // Match Xamarin ClearData(true, true) - line 787
                await ClearDataAsync(true);
            }
        }

        private async Task ClearDataAsync(bool clearAll)
        {
            // Match Xamarin ClearData method (line 801-857)
            // Save config values that need to be preserved
            var acceptedTerms = Config.AcceptedTermsAndConditions;
            var loginConfig = Config.SignedIn;
            var enabledlogin = Config.EnableLogin;

            // Show confirmation dialog - match Xamarin line 816
            var confirmed = await _dialogService.ShowConfirmationAsync(
                "Warning",
                "Are you sure you want to clean your data?",
                "Yes",
                "No");

            if (!confirmed)
                return;

            // Show loading dialog - match Xamarin line 818
            await _dialogService.ShowLoadingAsync("Please wait...");

            try
            {
                await Task.Run(() =>
                {
                    try
                    {
                        // Force a backup before cleaning - match Xamarin line 825
                        BackgroundDataSync.ForceBackup();

                        // Remove activity state - match Xamarin line 827-829
                        var activityState = ActivityState.GetState(typeof(ConfigurationPageViewModel).FullName);
                        if (activityState != null)
                            ActivityState.RemoveState(activityState);

                        // Clear settings if "Clear All" - match Xamarin line 831-832
                        if (clearAll)
                            Config.ClearSettings();

                        // Clear data - match Xamarin line 834
                        DataProvider.ClearData();

                        // Reinitialize - match Xamarin line 835-836
                        Config.Initialize();
                        DataProvider.Initialize();

                        // Restore saved config values - match Xamarin line 838-840
                        Config.AcceptedTermsAndConditions = acceptedTerms;
                        Config.SignedIn = loginConfig;
                        Config.EnableLogin = enabledlogin;

                        // Save settings - match Xamarin line 842
                        Config.SaveSettings();
                    }
                    catch (Exception ex)
                    {
                        _appService.TrackError(ex);
                        throw;
                    }
                });

                await _dialogService.HideLoadingAsync();

                // Show success message - match Xamarin behavior (no app close message)
                await _dialogService.ShowAlertAsync(
                    "Data cleared successfully.",
                    "Success",
                    "OK");

                // Stay on Configuration screen after clear (do not navigate away)
            }
            catch (Exception ex)
            {
                await _dialogService.HideLoadingAsync();
                await _dialogService.ShowAlertAsync(
                    $"Error clearing data: {ex.Message}",
                    "Error",
                    "OK");
                _appService.TrackError(ex);
            }
        }
    }
}
