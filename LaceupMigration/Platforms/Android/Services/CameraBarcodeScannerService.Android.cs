using LaceupMigration.Business.Interfaces;
using LaceupMigration;
using LaceupMigration.Helpers;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;
using MauiIcons.Material.Outlined;
using Task = System.Threading.Tasks.Task;
using CancellationToken = System.Threading.CancellationToken;
#if ANDROID
using Android.Util;
#endif

namespace LaceupMigration.Platforms.Android.Services;

public class CameraBarcodeScannerService : ICameraBarcodeScannerService
{
    private readonly SemaphoreSlim _scanSemaphore = new SemaphoreSlim(0, 1);
    private volatile string? _scannedResult;
    private bool _isScanning = false;
    private readonly object _resultLock = new object();

    public async Task<string?> ScanBarcodeAsync(CancellationToken cancellationToken = default)
    {
        // Check and request permission (match Xamarin: camera required for scan)
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
            var scannerPage = new CameraBarcodeScannerPage();

            // One result per scan session, then close – same as Xamarin BarcodeScannerActivity (SetResult + Finish)
            scannerPage.OnBarcodeScanned += (sender, barcode) =>
            {
                if (string.IsNullOrEmpty(barcode)) return;
                lock (_resultLock)
                {
                    if (_scannedResult != null) return; // already reported
                    _scannedResult = barcode;
                    try { _scanSemaphore.Release(); } catch (SemaphoreFullException) { }
                }
            };

            scannerPage.OnCancelled += (sender, e) =>
            {
                lock (_resultLock)
                {
                    if (_scannedResult != null) return; // already got a barcode
                    try { _scanSemaphore.Release(); } catch (SemaphoreFullException) { }
                }
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

            // Match Xamarin: return barcode string on success, null on cancel or error
            var result = _scannedResult;
            return string.IsNullOrEmpty(result) ? null : result;
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
    private bool _hasReportedResult = false; // one result per scan, like Xamarin BarcodeScannerActivity
    private ImageButton? _flashButton;

    public event EventHandler<string>? OnBarcodeScanned;
    public event EventHandler? OnCancelled;

    public CameraBarcodeScannerPage()
    {
        Title = "Scan Barcode";
        BackgroundColor = Colors.Black;

        // Root grid: full-screen camera with overlay on top
        var rootGrid = new Grid
        {
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill,
            BackgroundColor = Colors.Black
        };

        // Full-screen camera (fills entire grid)
        _scannerView = new CameraBarcodeReaderView
        {
            IsDetecting = false,
            IsTorchOn = false,
            Options = new BarcodeReaderOptions
            {
                Formats = BarcodeFormats.All,
                TryHarder = false,  // false = faster detection; true = more thorough but slower
                AutoRotate = true   // keep true so angled barcodes still read
            },
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill
        };
        _scannerView.BarcodesDetected += OnBarcodesDetected;
        rootGrid.Children.Add(_scannerView);

        // Overlay: top bar, scanning frame, instruction text (all transparent to touches where needed)
        var overlay = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(56) },
                new RowDefinition { Height = GridLength.Star },
                new RowDefinition { Height = GridLength.Auto }
            },
            BackgroundColor = Colors.Transparent,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill,
            InputTransparent = false
        };

        // Frame dimensions (must match scanning frame below)
        var frameWidth = 280.0;
        var frameHeight = 160.0;
        var darkColor = Color.FromArgb("#B3000000");

        // Row 0: dark strip + top bar
        var topDarkStrip = new BoxView { BackgroundColor = darkColor, HorizontalOptions = LayoutOptions.Fill, VerticalOptions = LayoutOptions.Fill };
        overlay.Children.Add(topDarkStrip);
        Grid.SetRow(topDarkStrip, 0);

        // Top bar (back + flash) – darker
        var topBar = new Grid
        {
            BackgroundColor = Color.FromArgb("#B3000000"),
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            },
            Padding = new Thickness(8, 8, 8, 8),
            VerticalOptions = LayoutOptions.Start
        };
        var backButton = new ImageButton
        {
            Source = MaterialIconHelper.GetImageSource(MaterialOutlinedIcons.ArrowBack, Colors.White, 28),
            BackgroundColor = Colors.Transparent,
            Padding = new Thickness(8),
            VerticalOptions = LayoutOptions.Center
        };
        backButton.Clicked += (s, e) => OnCancelled?.Invoke(this, EventArgs.Empty);
        Grid.SetColumn(backButton, 0);
        topBar.Children.Add(backButton);

        _flashButton = new ImageButton
        {
            Source = MaterialIconHelper.GetImageSource(MaterialOutlinedIcons.FlashOff, Colors.White, 28),
            BackgroundColor = Colors.Transparent,
            Padding = new Thickness(8),
            VerticalOptions = LayoutOptions.Center
        };
        _flashButton.Clicked += OnFlashClicked;
        Grid.SetColumn(_flashButton, 2);
        topBar.Children.Add(_flashButton);

        overlay.Children.Add(topBar);
        Grid.SetRow(topBar, 0);

        // Row 1: one grid containing dark overlay with hole AND white frame – same layout so they align exactly
        var centerGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Star },
                new RowDefinition { Height = new GridLength(frameHeight) },
                new RowDefinition { Height = GridLength.Star }
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = new GridLength(frameWidth) },
                new ColumnDefinition { Width = GridLength.Star }
            },
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill
        };
        for (int row = 0; row < 3; row++)
        for (int col = 0; col < 3; col++)
        {
            if (row == 1 && col == 1) continue; // center = transparent (hole)
            var patch = new BoxView { BackgroundColor = darkColor, HorizontalOptions = LayoutOptions.Fill, VerticalOptions = LayoutOptions.Fill };
            centerGrid.Children.Add(patch);
            Grid.SetRow(patch, row);
            Grid.SetColumn(patch, col);
        }

        var bracketLength = 36.0;
        var bracketThickness = 4.0;
        var bracketColor = Colors.White;
        var scanningFrame = new Grid
        {
            WidthRequest = frameWidth,
            HeightRequest = frameHeight,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill,
            InputTransparent = true
        };
        var tlH = new BoxView { BackgroundColor = bracketColor, WidthRequest = bracketLength, HeightRequest = bracketThickness, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Start };
        var tlV = new BoxView { BackgroundColor = bracketColor, WidthRequest = bracketThickness, HeightRequest = bracketLength, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Start };
        var trH = new BoxView { BackgroundColor = bracketColor, WidthRequest = bracketLength, HeightRequest = bracketThickness, HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.Start };
        var trV = new BoxView { BackgroundColor = bracketColor, WidthRequest = bracketThickness, HeightRequest = bracketLength, HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.Start };
        var blH = new BoxView { BackgroundColor = bracketColor, WidthRequest = bracketLength, HeightRequest = bracketThickness, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.End };
        var blV = new BoxView { BackgroundColor = bracketColor, WidthRequest = bracketThickness, HeightRequest = bracketLength, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.End };
        var brH = new BoxView { BackgroundColor = bracketColor, WidthRequest = bracketLength, HeightRequest = bracketThickness, HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.End };
        var brV = new BoxView { BackgroundColor = bracketColor, WidthRequest = bracketThickness, HeightRequest = bracketLength, HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.End };
        scanningFrame.Children.Add(tlH);
        scanningFrame.Children.Add(tlV);
        scanningFrame.Children.Add(trH);
        scanningFrame.Children.Add(trV);
        scanningFrame.Children.Add(blH);
        scanningFrame.Children.Add(blV);
        scanningFrame.Children.Add(brH);
        scanningFrame.Children.Add(brV);
        centerGrid.Children.Add(scanningFrame);
        Grid.SetRow(scanningFrame, 1);
        Grid.SetColumn(scanningFrame, 1);

        overlay.Children.Add(centerGrid);
        Grid.SetRow(centerGrid, 1);

        // Row 2: dark strip + instruction text
        var bottomDarkStrip = new BoxView { BackgroundColor = darkColor, HorizontalOptions = LayoutOptions.Fill, VerticalOptions = LayoutOptions.Fill };
        overlay.Children.Add(bottomDarkStrip);
        Grid.SetRow(bottomDarkStrip, 2);

        // Instruction text below frame
        var instructionsLabel = new Label
        {
            Text = "Align the barcode within the frame",
            TextColor = Colors.White,
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(16, 24, 16, 32),
            FontSize = 16
        };
        overlay.Children.Add(instructionsLabel);
        Grid.SetRow(instructionsLabel, 2);

        rootGrid.Children.Add(overlay);
        Content = rootGrid;
        
        // Start detection as soon as the page is shown (shorter delay = faster first scan)
        this.Appearing += async (s, e) =>
        {
            await Task.Delay(150);
            if (_scannerView != null && !_isDisposed && !_hasReportedResult)
                _scannerView.IsDetecting = true;
        };

        this.Disappearing += (s, e) => Cleanup();
    }
    
    private void OnFlashClicked(object? sender, EventArgs e)
    {
        if (_scannerView == null || _flashButton == null || _isDisposed)
            return;
        _scannerView.IsTorchOn = !_scannerView.IsTorchOn;
        _flashButton.Source = _scannerView.IsTorchOn
            ? MaterialIconHelper.GetImageSource(MaterialOutlinedIcons.FlashOn, Colors.White, 28)
            : MaterialIconHelper.GetImageSource(MaterialOutlinedIcons.FlashOff, Colors.White, 28);
    }

    private void OnBarcodesDetected(object? sender, BarcodeDetectionEventArgs e)
    {
        if (_isDisposed || _scannerView == null || _hasReportedResult)
            return;

        var results = e.Results;
        if (results == null || !results.Any())
            return;

        var barcode = results.First().Value;
        if (string.IsNullOrEmpty(barcode))
            return;

        // Single result then close – same as Xamarin BarcodeScannerActivity (HandleScanResult -> SetResult -> Finish)
        _hasReportedResult = true;
        _scannerView.IsDetecting = false;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (!_isDisposed)
                OnBarcodeScanned?.Invoke(this, barcode);
        });
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
