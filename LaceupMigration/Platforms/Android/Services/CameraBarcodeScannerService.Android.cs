using LaceupMigration.Business.Interfaces;
using LaceupMigration;
using LaceupMigration.Helpers;
using LaceupMigration.Views;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
