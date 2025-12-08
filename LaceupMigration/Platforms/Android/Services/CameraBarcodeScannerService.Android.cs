using LaceupMigration.Business.Interfaces;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;
using Task = System.Threading.Tasks.Task;
using CancellationToken = System.Threading.CancellationToken;

namespace LaceupMigration.Platforms.Android.Services;

public class CameraBarcodeScannerService : ICameraBarcodeScannerService
{
    private readonly SemaphoreSlim _scanSemaphore = new SemaphoreSlim(0, 1);
    private string? _scannedResult;
    private bool _isScanning = false;

    public async Task<string?> ScanBarcodeAsync(CancellationToken cancellationToken = default)
    {
        // Check and request permission
        if (!await CheckCameraPermissionAsync())
        {
            if (!await RequestCameraPermissionAsync())
            {
                return null;
            }
        }

        _scannedResult = null;
        _isScanning = true;

        try
        {
            // Create scanner page with ZXing CameraBarcodeReaderView
            var scannerPage = new CameraBarcodeScannerPage();
            
            // Set up barcode detection handler
            scannerPage.OnBarcodeScanned += (sender, barcode) =>
            {
                if (!string.IsNullOrEmpty(barcode))
                {
                    _scannedResult = barcode;
                    _scanSemaphore.Release();
                }
            };
            
            scannerPage.OnCancelled += (sender, e) =>
            {
                _scannedResult = null;
                _scanSemaphore.Release();
            };

            // Navigate to scanner page
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await Application.Current!.MainPage!.Navigation.PushModalAsync(scannerPage);
            });

            // Wait for scan result or cancellation
            using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            {
                cts.CancelAfter(TimeSpan.FromMinutes(5)); // 5 minute timeout
                
                try
                {
                    await _scanSemaphore.WaitAsync(cts.Token);
                }
                catch (OperationCanceledException)
                {
                    // Close the scanner page
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        if (Application.Current?.MainPage?.Navigation.ModalStack.Count > 0)
                        {
                            await Application.Current.MainPage.Navigation.PopModalAsync();
                        }
                    });
                    return null;
                }
            }

            // Close the scanner page
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (Application.Current?.MainPage?.Navigation.ModalStack.Count > 0)
                {
                    await Application.Current.MainPage.Navigation.PopModalAsync();
                }
            });

            return _scannedResult;
        }
        finally
        {
            _isScanning = false;
        }
    }

    public async Task<bool> CheckCameraPermissionAsync()
    {
        return await MainThread.InvokeOnMainThreadAsync(() =>
        {
            var status = Platform.CurrentActivity?.CheckSelfPermission(global::Android.Manifest.Permission.Camera);
            return status == global::Android.Content.PM.Permission.Granted;
        });
    }

    public async Task<bool> RequestCameraPermissionAsync()
    {
        return await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            var status = await Permissions.RequestAsync<Permissions.Camera>();
            return status == PermissionStatus.Granted;
        });
    }
}

// Custom page with ZXing CameraBarcodeReaderView for barcode scanning
public class CameraBarcodeScannerPage : ContentPage
{
    private CameraBarcodeReaderView? _scannerView;
    private bool _isDisposed = false;
    
    public event EventHandler<string>? OnBarcodeScanned;
    public event EventHandler? OnCancelled;

    public CameraBarcodeScannerPage()
    {
        Title = "Scan Barcode";
        BackgroundColor = Colors.Black;

        var stackLayout = new VerticalStackLayout
        {
            Spacing = 0,
            BackgroundColor = Colors.Black
        };

        // Create ZXing scanner view (CameraBarcodeReaderView)
        _scannerView = new CameraBarcodeReaderView
        {
            IsDetecting = true,
            IsTorchOn = false,
            Options = new BarcodeReaderOptions
            {
                Formats = BarcodeFormats.All,
                TryHarder = true,
                AutoRotate = true
            },
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill
        };
        
        // Handle barcode detected
        _scannerView.BarcodesDetected += OnBarcodesDetected;
        
        // Use Grid to ensure scanner view fills available space
        var scannerContainer = new Grid
        {
            HeightRequest = 400,
            BackgroundColor = Colors.Black
        };
        scannerContainer.Children.Add(_scannerView);
        
        // Add barcode scanning guide overlay with corner brackets
        var guideOverlay = new Grid
        {
            BackgroundColor = Colors.Transparent,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill,
        };
        
        // Semi-transparent overlay for the entire camera view
        var darkOverlay = new BoxView
        {
            BackgroundColor = Color.FromArgb("#80000000"), // 50% transparent black
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill
        };
        guideOverlay.Children.Add(darkOverlay);
        
        // Create a transparent center area (the scanning window)
        var scanningWindowContainer = new Grid
        {
            BackgroundColor = Colors.Transparent,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            WidthRequest = 300,
            HeightRequest = 200,
            Margin = new Thickness(0)
        };
        
        // Transparent box in the center (cut-out effect)
        var transparentBox = new BoxView
        {
            BackgroundColor = Colors.Transparent,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill
        };

        // Use InputTransparent to create a "window" effect
        transparentBox.InputTransparent = true;
        scanningWindowContainer.Children.Add(transparentBox);
        
        // Create corner brackets frame
        var cornerBracketFrame = new Grid
        {
            BackgroundColor = Colors.Transparent,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Center
        };
        
        // Corner bracket size and thickness
        var bracketLength = 50.0;
        var bracketThickness = 4.0;
        var bracketColor = Color.FromArgb("#00FF00"); 
        
        // Top-left corner
        var topLeftHorizontal = new BoxView
        {
            BackgroundColor = bracketColor,
            WidthRequest = bracketLength,
            HeightRequest = bracketThickness,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Start
        };
        var topLeftVertical = new BoxView
        {
            BackgroundColor = bracketColor,
            WidthRequest = bracketThickness,
            HeightRequest = bracketLength,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Start
        };

// Top-right corner
        var topRightHorizontal = new BoxView
        {
            BackgroundColor = bracketColor,
            WidthRequest = bracketLength,
            HeightRequest = bracketThickness,
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.Start
        };
        var topRightVertical = new BoxView
        {
            BackgroundColor = bracketColor,
            WidthRequest = bracketThickness,
            HeightRequest = bracketLength,
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.Start
        };

// Bottom-left corner
        var bottomLeftHorizontal = new BoxView
        {
            BackgroundColor = bracketColor,
            WidthRequest = bracketLength,
            HeightRequest = bracketThickness,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.End
        };
        var bottomLeftVertical = new BoxView
        {
            BackgroundColor = bracketColor,
            WidthRequest = bracketThickness,
            HeightRequest = bracketLength,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.End
        };

// Bottom-right corner
        var bottomRightHorizontal = new BoxView
        {
            BackgroundColor = bracketColor,
            WidthRequest = bracketLength,
            HeightRequest = bracketThickness,
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.End
        };
        var bottomRightVertical = new BoxView
        {
            BackgroundColor = bracketColor,
            WidthRequest = bracketThickness,
            HeightRequest = bracketLength,
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.End
        };
        
        // Add all corner brackets to the scanning window
        scanningWindowContainer.Children.Add(topLeftHorizontal);
        scanningWindowContainer.Children.Add(topLeftVertical);
        scanningWindowContainer.Children.Add(topRightHorizontal);
        scanningWindowContainer.Children.Add(topRightVertical);
        scanningWindowContainer.Children.Add(bottomLeftHorizontal);
        scanningWindowContainer.Children.Add(bottomLeftVertical);
        scanningWindowContainer.Children.Add(bottomRightHorizontal);
        scanningWindowContainer.Children.Add(bottomRightVertical);
        
        guideOverlay.Children.Add(scanningWindowContainer);
        scannerContainer.Children.Add(guideOverlay);
        stackLayout.Children.Add(scannerContainer); 

        // Instructions label
        var instructionsLabel = new Label
        {
            Text = "Point camera at barcode",
            TextColor = Colors.White,
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 20, 0, 0),
            FontSize = 16
        };
        stackLayout.Children.Add(instructionsLabel);

        // Cancel button
        var cancelButton = new Button
        {
            Text = "Cancel",
            BackgroundColor = Colors.Red,
            TextColor = Colors.White,
            Margin = new Thickness(20, 20, 20, 20),
            HeightRequest = 50
        };
        cancelButton.Clicked += (s, e) =>
        {
            OnCancelled?.Invoke(this, EventArgs.Empty);
        };
        stackLayout.Children.Add(cancelButton);

        Content = stackLayout;
        
        // Start scanning when page appears
        this.Appearing += (s, e) =>
        {
            if (_scannerView != null)
            {
                _scannerView.IsDetecting = true;
            }
        };
        
        // Stop scanning when page disappears
        this.Disappearing += (s, e) =>
        {
            Cleanup();
        };
        
        // Handle page loaded
        this.Loaded += (s, e) =>
        {
            if (_scannerView != null && !_isDisposed)
            {
                _scannerView.IsDetecting = true;
            }
        };
    }
    
    private void OnBarcodesDetected(object? sender, BarcodeDetectionEventArgs e)
    {
        // Prevent processing if disposed
        if (_isDisposed || _scannerView == null)
        {
            return;
        }
        
        var results = e.Results;
        if (results != null && results.Count() > 0)
        {
            var firstResult = results.First();
            var barcode = firstResult.Value;
            
            if (!string.IsNullOrEmpty(barcode) && !_isDisposed)
            {
                // Invoke on main thread to ensure UI updates work correctly
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (!_isDisposed)
                    {
                        OnBarcodeScanned?.Invoke(this, barcode);
                    }
                });
            }
        }
    }
    
    private void Cleanup()
    {
        if (_isDisposed)
            return;
            
        _isDisposed = true;
        
        if (_scannerView != null)
        {
            // Stop detection first
            _scannerView.IsDetecting = false;
            
            // Unsubscribe from events
            _scannerView.BarcodesDetected -= OnBarcodesDetected;
            
            // Remove from visual tree
            if (_scannerView.Parent is Layout parentLayout)
            {
                parentLayout.Children.Remove(_scannerView);
            }
            
            _scannerView = null;
        }
    }
    
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        Cleanup();
    }
}
