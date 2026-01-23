using LaceupMigration.Business.Interfaces;
using LaceupMigration;
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
#if ANDROID
using Android.Util;
#endif

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
                Console.WriteLine($"[CameraBarcodeScannerService] OnBarcodeScanned event received: '{(barcode ?? "NULL")}'");
                System.Diagnostics.Debug.WriteLine($"[CameraBarcodeScannerService] OnBarcodeScanned event received: '{(barcode ?? "NULL")}'");
                
                if (!string.IsNullOrEmpty(barcode))
                {
                    Console.WriteLine($"[CameraBarcodeScannerService] Setting _scannedResult and releasing semaphore");
                    _scannedResult = barcode;
                    _scanSemaphore.Release();
                    Console.WriteLine($"[CameraBarcodeScannerService] Semaphore released, _scannedResult = '{_scannedResult}'");
                }
                else
                {
                    Console.WriteLine("[CameraBarcodeScannerService] OnBarcodeScanned: Barcode is null or empty, ignoring");
                }
            };
            
            scannerPage.OnCancelled += (sender, e) =>
            {
                _scannedResult = null;
                _scanSemaphore.Release();
            };

            // Navigate to scanner page - use Application.Current.MainPage.Navigation (most reliable)
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                try
                {
                    // Use Application.Current.MainPage.Navigation as primary method (works for all pages)
                    if (Application.Current?.MainPage?.Navigation != null)
                    {
                        await Application.Current.MainPage.Navigation.PushModalAsync(scannerPage);
                    }
                    else
                    {
                        // Fallback: try getting current page
                        var currentPage = GetCurrentPage();
                        if (currentPage != null && currentPage.Navigation != null)
                        {
                            await currentPage.Navigation.PushModalAsync(scannerPage);
                        }
                        else
                        {
                            throw new InvalidOperationException("No navigation context available");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.CreateLog($"Error pushing scanner page: {ex.Message}");
                    Logger.CreateLog($"Stack trace: {ex.StackTrace}");
                    throw;
                }
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
                        try
                        {
                            if (Application.Current?.MainPage?.Navigation != null && Application.Current.MainPage.Navigation.ModalStack.Count > 0)
                            {
                                await Application.Current.MainPage.Navigation.PopModalAsync();
                            }
                            else
                            {
                                var currentPage = GetCurrentPage();
                                if (currentPage != null && currentPage.Navigation != null && currentPage.Navigation.ModalStack.Count > 0)
                                {
                                    await currentPage.Navigation.PopModalAsync();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.CreateLog($"Error popping scanner page: {ex.Message}");
                        }
                    });
                    return null;
                }
            }

            // Close the scanner page
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                try
                {
                    if (Application.Current?.MainPage?.Navigation != null && Application.Current.MainPage.Navigation.ModalStack.Count > 0)
                    {
                        await Application.Current.MainPage.Navigation.PopModalAsync();
                    }
                    else
                    {
                        var currentPage = GetCurrentPage();
                        if (currentPage != null && currentPage.Navigation != null && currentPage.Navigation.ModalStack.Count > 0)
                        {
                            await currentPage.Navigation.PopModalAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.CreateLog($"Error popping scanner page: {ex.Message}");
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

    private Page GetCurrentPage()
    {
        // Try Shell first (most common in MAUI)
        if (Shell.Current?.CurrentPage != null)
            return Shell.Current.CurrentPage;
        
        // Fallback to Application Windows
        return Application.Current?.Windows?.FirstOrDefault()?.Page;
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
            IsDetecting = false, // Start as false, will be set to true after page loads
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
        
        Console.WriteLine("[CameraBarcodeScannerPage] Constructor: Scanner view created");
        System.Diagnostics.Debug.WriteLine("[CameraBarcodeScannerPage] Constructor: Scanner view created");
        
        // Handle barcode detected - attach BEFORE adding to visual tree
        _scannerView.BarcodesDetected += OnBarcodesDetected;
        
        // Test: Try to manually trigger to see if event works (this won't work but shows handler is attached)
        Console.WriteLine("[CameraBarcodeScannerPage] Constructor: BarcodesDetected event handler attached");
        System.Diagnostics.Debug.WriteLine("[CameraBarcodeScannerPage] Constructor: BarcodesDetected event handler attached");
        Console.WriteLine("[CameraBarcodeScannerPage] Constructor: BarcodesDetected event handler attached to scanner view");
        System.Diagnostics.Debug.WriteLine("[CameraBarcodeScannerPage] Constructor: BarcodesDetected event handler attached to scanner view");
        
        // Use Grid to ensure scanner view fills available space
        var scannerContainer = new Grid
        {
            HeightRequest = 400,
            BackgroundColor = Colors.Black
        };
        scannerContainer.Children.Add(_scannerView);
        Console.WriteLine("[CameraBarcodeScannerPage] Constructor: Scanner view added to container, IsDetecting = " + _scannerView.IsDetecting);
        System.Diagnostics.Debug.WriteLine("[CameraBarcodeScannerPage] Constructor: Scanner view added to container, IsDetecting = " + _scannerView.IsDetecting);
        
        // Add barcode scanning guide overlay with corner brackets
        // Make it InputTransparent so it doesn't interfere with scanner detection
        var guideOverlay = new Grid
        {
            BackgroundColor = Colors.Transparent,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill,
            InputTransparent = true // Don't block scanner events
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
        this.Appearing += async (s, e) =>
        {
            Console.WriteLine("[CameraBarcodeScannerPage] Appearing event fired");
            System.Diagnostics.Debug.WriteLine("[CameraBarcodeScannerPage] Appearing event fired");
            
            // Small delay to ensure camera is ready
            await Task.Delay(300);
            
            if (_scannerView != null && !_isDisposed)
            {
                Console.WriteLine($"[CameraBarcodeScannerPage] Appearing: Setting IsDetecting = true (current: {_scannerView.IsDetecting})");
                System.Diagnostics.Debug.WriteLine($"[CameraBarcodeScannerPage] Appearing: Setting IsDetecting = true (current: {_scannerView.IsDetecting})");
                
                // Force restart detection
                _scannerView.IsDetecting = false;
                await Task.Delay(100);
                _scannerView.IsDetecting = true;
                
                Console.WriteLine($"[CameraBarcodeScannerPage] Appearing: IsDetecting set to {_scannerView.IsDetecting}");
                System.Diagnostics.Debug.WriteLine($"[CameraBarcodeScannerPage] Appearing: IsDetecting set to {_scannerView.IsDetecting}");
            }
            else
            {
                Console.WriteLine($"[CameraBarcodeScannerPage] Appearing: Scanner view is null or disposed (IsDisposed: {_isDisposed})");
            }
        };
        
        // Stop scanning when page disappears
        this.Disappearing += (s, e) =>
        {
            Console.WriteLine("[CameraBarcodeScannerPage] Disappearing event fired - cleaning up");
            System.Diagnostics.Debug.WriteLine("[CameraBarcodeScannerPage] Disappearing event fired - cleaning up");
            Cleanup();
        };
        
        // Handle page loaded
        this.Loaded += async (s, e) =>
        {
            Console.WriteLine("[CameraBarcodeScannerPage] Loaded event fired");
            System.Diagnostics.Debug.WriteLine("[CameraBarcodeScannerPage] Loaded event fired");
            
            // Delay to ensure everything is ready
            await Task.Delay(500);
            
            if (_scannerView != null && !_isDisposed)
            {
                Console.WriteLine($"[CameraBarcodeScannerPage] Loaded: Setting IsDetecting = true (current: {_scannerView.IsDetecting})");
                System.Diagnostics.Debug.WriteLine($"[CameraBarcodeScannerPage] Loaded: Setting IsDetecting = true (current: {_scannerView.IsDetecting})");
                
                // Force restart detection
                _scannerView.IsDetecting = false;
                await Task.Delay(100);
                _scannerView.IsDetecting = true;
                
                Console.WriteLine($"[CameraBarcodeScannerPage] Loaded: IsDetecting set to {_scannerView.IsDetecting}");
                System.Diagnostics.Debug.WriteLine($"[CameraBarcodeScannerPage] Loaded: IsDetecting set to {_scannerView.IsDetecting}");
            }
            else
            {
                Console.WriteLine($"[CameraBarcodeScannerPage] Loaded: Scanner view is null or disposed (IsDisposed: {_isDisposed})");
            }
        };
    }
    
    private void OnBarcodesDetected(object? sender, BarcodeDetectionEventArgs e)
    {
        // This should fire when ZXing detects a barcode
        Console.WriteLine($"[CameraBarcodeScannerPage] *** OnBarcodesDetected: EVENT FIRED! *** IsDisposed: {_isDisposed}, ScannerView null: {_scannerView == null}");
        System.Diagnostics.Debug.WriteLine($"[CameraBarcodeScannerPage] *** OnBarcodesDetected: EVENT FIRED! *** IsDisposed: {_isDisposed}, ScannerView null: {_scannerView == null}");
        
#if ANDROID
        // Also log to Android logcat for visibility
        Log.Info("BarcodeScanner", $"OnBarcodesDetected fired! IsDisposed: {_isDisposed}");
#endif
        
        // Prevent processing if disposed
        if (_isDisposed || _scannerView == null)
        {
            Console.WriteLine("[CameraBarcodeScannerPage] OnBarcodesDetected: Scanner disposed or null, returning");
            return;
        }
        
        var results = e.Results;
        var resultCount = results?.Count() ?? 0;
        Console.WriteLine($"[CameraBarcodeScannerPage] OnBarcodesDetected: Received {resultCount} barcode results");
        System.Diagnostics.Debug.WriteLine($"[CameraBarcodeScannerPage] OnBarcodesDetected: Received {resultCount} barcode results");
        
        if (results != null && results.Count() > 0)
        {
            var firstResult = results.First();
            var barcode = firstResult.Value;
            
            Console.WriteLine($"[CameraBarcodeScannerPage] OnBarcodesDetected: Barcode value: '{(barcode ?? "NULL")}'");
            System.Diagnostics.Debug.WriteLine($"[CameraBarcodeScannerPage] OnBarcodesDetected: Barcode value: '{(barcode ?? "NULL")}'");
            
            if (!string.IsNullOrEmpty(barcode) && !_isDisposed)
            {
                Console.WriteLine($"[CameraBarcodeScannerPage] OnBarcodesDetected: Invoking OnBarcodeScanned event with: '{barcode}'");
                System.Diagnostics.Debug.WriteLine($"[CameraBarcodeScannerPage] OnBarcodesDetected: Invoking OnBarcodeScanned event with: '{barcode}'");
                
                // Invoke on main thread to ensure UI updates work correctly
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (!_isDisposed)
                    {
                        Console.WriteLine($"[CameraBarcodeScannerPage] OnBarcodesDetected: Calling OnBarcodeScanned?.Invoke");
                        OnBarcodeScanned?.Invoke(this, barcode);
                    }
                    else
                    {
                        Console.WriteLine("[CameraBarcodeScannerPage] OnBarcodesDetected: Scanner was disposed before invoking");
                    }
                });
            }
            else
            {
                Console.WriteLine($"[CameraBarcodeScannerPage] OnBarcodesDetected: Barcode is empty or scanner disposed");
            }
        }
        else
        {
            Console.WriteLine("[CameraBarcodeScannerPage] OnBarcodesDetected: No results or empty results");
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
