using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using Microsoft.Maui.ApplicationModel;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LaceupMigration.ViewModels
{
    public partial class TimeSheetPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;
        private System.Timers.Timer? _timer;
        private bool _initialized;

        public ObservableCollection<SessionDetailViewModel> SessionDetails { get; } = new();

        [ObservableProperty]
        private string _greetingText = string.Empty;

        [ObservableProperty]
        private string _clockInOutButtonText = "Clock In";

        [ObservableProperty]
        private string _breakButtonText = "Start Break";

        [ObservableProperty]
        private bool _canBreak;

        [ObservableProperty]
        private bool _showBreakButton;

        [ObservableProperty]
        private bool _showSessionInfo;

        [ObservableProperty]
        private string _sessionInfoText = string.Empty;

        [ObservableProperty]
        private string _sessionDurationText = string.Empty;

        [ObservableProperty]
        private string _startAddressText = string.Empty;

        [ObservableProperty]
        private bool _showViewLocation;

        [ObservableProperty]
        private bool _showSessionDetails;

        [ObservableProperty]
        private bool _showNoDetails;

        public TimeSheetPageViewModel(DialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
        }

        public async Task OnAppearingAsync()
        {
            if (!_initialized)
            {
                await InitializeAsync();
            }
            else
            {
                await RefreshAsync();
            }
        }

        public void OnDisappearing()
        {
            StopTimer();
        }

        private async Task InitializeAsync()
        {
            _initialized = true;
            GreetingText = $"Hello {Config.VendorName}";

            await RefreshAsync();
            StartTimer();
        }

        private async Task RefreshAsync()
        {
            if (Session.session == null)
            {
                ClockInOutButtonText = "Clock In";
                ShowBreakButton = false;
                ShowSessionInfo = false;
                ShowSessionDetails = false;
                ShowNoDetails = true;
            }
            else
            {
                ClockInOutButtonText = "Clock Out";
                ShowBreakButton = true;
                ShowSessionInfo = true;
                ShowSessionDetails = true;
                ShowNoDetails = false;

                SessionInfoText = $"Day Started: {Session.session.ClockIn:g}";

                // Check if in break
                var activeBreak = Session.sessionDetails.FirstOrDefault(x => 
                    x.detailType == LaceupMigration.SessionDetails.SessionDetailType.Break && x.endTime.Ticks <= 0);

                if (activeBreak != null)
                {
                    BreakButtonText = "End Break";
                    CanBreak = true;
                    SessionInfoText = "Currently on break";
                }
                else
                {
                    BreakButtonText = "Start Break";
                    CanBreak = true;
                }

                // Get address if available
                if (Session.session.StartLatitude != 0 && Session.session.StartLongitude != 0)
                {
                    await GetAddressFromCoordinatesAsync(Session.session.StartLatitude, Session.session.StartLongitude);
                    ShowViewLocation = true;
                }
                else
                {
                    StartAddressText = "Location not available";
                    ShowViewLocation = false;
                }

                UpdateSessionDuration();
                LoadSessionDetails();
            }

            await Task.CompletedTask;
        }

        private void StartTimer()
        {
            if (_timer != null)
                return;

            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += (s, e) =>
            {
                if (Session.session != null)
                {
                    UpdateSessionDuration();
                }
            };
            _timer.AutoReset = true;
            _timer.Start();
        }

        private void StopTimer()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Dispose();
                _timer = null;
            }
        }

        private void UpdateSessionDuration()
        {
            if (Session.session == null)
                return;

            try
            {
                var duration = DateTime.Now - Session.session.ClockIn;
                SessionDurationText = $"Session Duration: {duration.Hours:00}:{duration.Minutes:00}:{duration.Seconds:00}";
            }
            catch
            {
                if (Session.session.ClockOut != DateTime.MinValue)
                {
                    SessionDurationText = $"Day Ended: {Session.session.ClockOut:g}";
                }
            }
        }

        private void LoadSessionDetails()
        {
            SessionDetails.Clear();

            var details = Session.sessionDetails.OrderBy(x => x.startTime).ToList();

            foreach (var detail in details)
            {
                var viewModel = CreateSessionDetailViewModel(detail);
                SessionDetails.Add(viewModel);
            }
        }

        private SessionDetailViewModel CreateSessionDetailViewModel(SessionDetails detail)
        {
            string customerName;
            if (detail.clientId == 0)
            {
                customerName = "Break";
            }
            else
            {
                var client = Client.Find(detail.clientId);
                customerName = client?.ClientName ?? "Unknown Client";
            }

            string timeRangeText;
            string durationText;
            string statusText = string.Empty;
            bool showStatus = false;

            if (detail.endTime.Ticks > 0)
            {
                timeRangeText = $"{detail.startTime:g} - {detail.endTime:g}";
                var duration = detail.endTime - detail.startTime;
                durationText = $"{duration.Hours:00}:{duration.Minutes:00}:{duration.Seconds:00}";
            }
            else
            {
                timeRangeText = $"Started: {detail.startTime:g}";
                durationText = "In Progress";
                statusText = "In Progress";
                showStatus = true;
            }

            string typeText = !string.IsNullOrEmpty(detail.transactionName) 
                ? detail.transactionName 
                : detail.detailType.ToString();

            return new SessionDetailViewModel
            {
                Detail = detail,
                CustomerName = customerName,
                TimeRangeText = timeRangeText,
                DurationText = durationText,
                TypeText = typeText,
                StatusText = statusText,
                ShowStatus = showStatus
            };
        }

        private async Task GetAddressFromCoordinatesAsync(double latitude, double longitude)
        {
            try
            {
                var placemarks = await Geocoding.GetPlacemarksAsync(latitude, longitude);
                var placemark = placemarks?.FirstOrDefault();

                if (placemark != null)
                {
                    StartAddressText = $"{placemark.Thoroughfare} {placemark.SubThoroughfare}, {placemark.Locality}, {placemark.AdminArea} {placemark.PostalCode}, {placemark.CountryName}";
                }
                else
                {
                    StartAddressText = "Address not available";
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog($"Error getting address: {ex.Message}");
                StartAddressText = "Address not available";
            }
        }

        [RelayCommand]
        private async Task ClockInOutAsync()
        {
            if (Session.session == null)
            {
                // Clock In
                var session = new Session((int)Config.SalesmanId, DateTime.Now);
                Session.session = session;

                Session.session.StartLatitude = DataAccess.LastLatitude;
                Session.session.StartLongitude = DataAccess.LastLongitude;

                Session.session.Save();

                await GetAddressFromCoordinatesAsync(Session.session.StartLatitude, Session.session.StartLongitude);
                await RefreshAsync();
            }
            else
            {
                // Clock Out
                if (Order.Orders.Count() > 0)
                {
                    await _dialogService.ShowAlertAsync("You have open transactions. Please finalize or void all orders before clocking out.", "Alert");
                    return;
                }

                var result = await _dialogService.ShowConfirmAsync("Are you sure you want to clock out for today?", "Confirm", "Yes", "No");
                if (!result)
                    return;

                Session.session.EndLatitude = DataAccess.LastLatitude;
                Session.session.EndLongitude = DataAccess.LastLongitude;
                Session.session.ClockOut = DateTime.Now;

                Session.session.Save();

                try
                {
                    // [MIGRATION]: Ensure SessionPath directory exists before accessing SessionFile
                    // This prevents "Could not find a part of the path" errors on Android tablet emulators
                    if (!System.IO.Directory.Exists(Config.SessionPath))
                    {
                        System.IO.Directory.CreateDirectory(Config.SessionPath);
                    }

                    bool success = DataAccess.SendCurrentSession(System.IO.Path.Combine(Config.SessionPath, "SessionFile.cvs"));
                    if (success)
                    {
                        if (System.IO.File.Exists(Session.session.fileName))
                            System.IO.File.Delete(Session.session.fileName);

                        Session.selectedDetail = null;
                        Session.sessionDetails.Clear();
                        Session.session.ClearDetails();
                        Session.session = null;

                        await RefreshAsync();
                        await _dialogService.ShowAlertAsync("Clocked out successfully.", "Success");
                    }
                    else
                    {
                        await _dialogService.ShowAlertAsync("Error clocking out. Please try again.", "Error");
                    }
                }
                catch (Exception ex)
                {
                    Logger.CreateLog($"Error in clock out: {ex.Message}");
                    await _dialogService.ShowAlertAsync("Error clocking out.", "Error");
                }
            }
        }

        [RelayCommand]
        private async Task BreakAsync()
        {
            if (Session.session == null)
            {
                await _dialogService.ShowAlertAsync("Please clock in to start the day.", "Alert");
                return;
            }

            try
            {
                var selectedDetail = Session.selectedDetail;
                if (selectedDetail != null && selectedDetail.detailType == LaceupMigration.SessionDetails.SessionDetailType.Break)
                {
                    // End Break
                    var clockout = DateTime.Now;
                    Session.session.EditDetail(selectedDetail, clockout, DataAccess.LastLatitude, DataAccess.LastLongitude);
                    Session.selectedDetail = null;
                    Session.session.Save();

                    await RefreshAsync();
                }
                else
                {
                    // Start Break
                    var sessionDetail = new SessionDetails(0, LaceupMigration.SessionDetails.SessionDetailType.Break);
                    sessionDetail.startTime = DateTime.Now;
                    sessionDetail.startLatitude = DataAccess.LastLatitude;
                    sessionDetail.startLongitude = DataAccess.LastLongitude;

                    Session.session.AddDetail(sessionDetail);
                    Session.selectedDetail = sessionDetail;
                    Session.session.Save();

                    await RefreshAsync();
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog($"Error in break: {ex.Message}");
                await _dialogService.ShowAlertAsync("Error managing break.", "Error");
            }
        }

        [RelayCommand]
        private async Task ViewLocationAsync()
        {
            if (Session.session == null || Session.session.StartLatitude == 0 || Session.session.StartLongitude == 0)
            {
                await _dialogService.ShowAlertAsync("No location available for this session.", "Info");
                return;
            }

            try
            {
                var location = new Location(Session.session.StartLatitude, Session.session.StartLongitude);
                await Map.OpenAsync(location);
            }
            catch (Exception ex)
            {
                Logger.CreateLog($"Error opening map: {ex.Message}");
                await _dialogService.ShowAlertAsync("Error opening map.", "Error");
            }
        }
    }

    public partial class SessionDetailViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _customerName = string.Empty;

        [ObservableProperty]
        private string _timeRangeText = string.Empty;

        [ObservableProperty]
        private string _durationText = string.Empty;

        [ObservableProperty]
        private string _typeText = string.Empty;

        [ObservableProperty]
        private string _statusText = string.Empty;

        [ObservableProperty]
        private bool _showStatus;

        public SessionDetails Detail { get; set; } = null!;
    }
}

