using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LaceupMigration.ViewModels
{
    public partial class OrderSignaturePageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;
        private string _ordersId = string.Empty;
        private List<Order> _orders = new();

        [ObservableProperty]
        private string _signatureName = string.Empty;

        [ObservableProperty]
        private SignatureDrawable _signatureDrawable;

        public void AddPoint(PointF point)
        {
            _signatureDrawable.AddPoint(point);
        }

        public void EndStroke()
        {
            _signatureDrawable.EndStroke();
        }

        public OrderSignaturePageViewModel(DialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
            _signatureDrawable = new SignatureDrawable();
        }

        public async Task InitializeAsync(string ordersId)
        {
            _ordersId = ordersId;

            // Parse order IDs
            var orderIds = new List<int>();
            foreach (var idAsString in ordersId.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (int.TryParse(idAsString, out var id))
                {
                    orderIds.Add(id);
                }
            }

            if (orderIds.Count == 0)
            {
                await _dialogService.ShowAlertAsync("No valid orders found.", "Error", "OK");
                await Shell.Current.GoToAsync("..");
                return;
            }

            // Load orders
            _orders = Order.Orders.Where(x => orderIds.Contains(x.OrderId)).ToList();
            if (_orders.Count == 0)
            {
                await _dialogService.ShowAlertAsync("Orders not found.", "Error", "OK");
                await Shell.Current.GoToAsync("..");
                return;
            }

            // Load existing signature if available (Xamarin lines 88-91)
            if (_orders.Count > 0 && _orders[0].SignaturePoints != null && _orders[0].SignaturePoints.Count > 0)
            {
                // Convert SixLabors.ImageSharp.Point to PointF for GraphicsView
                var points = _orders[0].SignaturePoints
                    .Where(p => p != SixLabors.ImageSharp.Point.Empty)
                    .Select(p => new PointF(p.X, p.Y))
                    .ToList();
                _signatureDrawable.LoadPoints(points);
            }

            if (_orders.Count > 0 && !string.IsNullOrEmpty(_orders[0].SignatureName))
            {
                SignatureName = _orders[0].SignatureName;
            }

            // Save navigation state
            Helpers.NavigationHelper.SaveNavigationState($"ordersignature?ordersId={Uri.EscapeDataString(ordersId)}");
        }

        public async Task OnAppearingAsync()
        {
            await Task.CompletedTask;
        }

        [RelayCommand]
        private async Task ClearAsync()
        {
            _appService.RecordEvent("ClearSignature selected");

            // Xamarin ClearSignature_Click (lines 160-172)
            foreach (var order in _orders)
            {
                order.SignatureName = string.Empty;
                order.SignaturePoints = null;
                order.Save();
            }

            SignatureName = string.Empty;
            _signatureDrawable.Clear();
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            _appService.RecordEvent("SaveSignature selected");

            // Xamarin SaveSignature_Click (lines 174-250)
            var sName = SignatureName;

            if (Config.SignatureNameRequired && string.IsNullOrEmpty(sName))
            {
                await _dialogService.ShowAlertAsync("You must type the name.", "Alert", "OK");
                return;
            }

            // Get signature points from drawable
            var signaturePoints = _signatureDrawable.GetPoints();
            if (signaturePoints == null || signaturePoints.Count == 0)
            {
                await _dialogService.ShowAlertAsync("Signature is too small.", "Alert", "OK");
                return;
            }

            // Calculate bounding box (Xamarin lines 189-205)
            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float width = 0;
            float height = 0;

            foreach (var point in signaturePoints)
            {
                if (point.IsEmpty)
                    continue;
                if (minX > point.X)
                    minX = point.X;
                if (minY > point.Y)
                    minY = point.Y;
                if (width < point.X)
                    width = point.X;
                if (height < point.Y)
                    height = point.Y;
            }

            if (minX == float.MaxValue || minY == float.MaxValue)
            {
                await _dialogService.ShowAlertAsync("Signature is too small.", "Alert", "OK");
                return;
            }

            // Round up to 32 (Xamarin lines 211-213)
            int x = Convert.ToInt32((width + 31 - minX)) / 32 * 32;
            int y = Convert.ToInt32((height + 31 - minY)) / 32 * 32;
            if (x < 10 || y < 10)
            {
                await _dialogService.ShowAlertAsync("Signature is too small.", "Alert", "OK");
                return;
            }

            // Calculate factor and scale down if needed (Xamarin lines 219-234)
            int factor = 1;
            if (x > 320 && x < 641)
                factor = 2;
            else if (x > 640 && x < 961)
                factor = 3;
            else if (x > 960)
                factor = 4;

            // Convert PointF to SixLabors.ImageSharp.Point and scale if needed
            var scaledPoints = new List<SixLabors.ImageSharp.Point>();
            if (x > 300)
            {
                foreach (var point in signaturePoints)
                {
                    if (point.IsEmpty)
                    {
                        scaledPoints.Add(SixLabors.ImageSharp.Point.Empty);
                    }
                    else
                    {
                        scaledPoints.Add(new SixLabors.ImageSharp.Point(
                            (int)(point.X / factor),
                            (int)(point.Y / factor)));
                    }
                }
            }
            else
            {
                foreach (var point in signaturePoints)
                {
                    if (point.IsEmpty)
                    {
                        scaledPoints.Add(SixLabors.ImageSharp.Point.Empty);
                    }
                    else
                    {
                        scaledPoints.Add(new SixLabors.ImageSharp.Point(
                            (int)point.X,
                            (int)point.Y));
                    }
                }
            }

            // Save signature to all orders (Xamarin lines 236-244)
            foreach (var order in _orders)
            {
                if (string.IsNullOrEmpty(order.SignatureUniqueId))
                    order.SignatureUniqueId = Guid.NewGuid().ToString("N");

                order.SignatureName = sName;
                order.SignaturePoints = scaledPoints;
                order.Save();
            }

            // Remove navigation state
            Helpers.NavigationHelper.RemoveNavigationState($"ordersignature?ordersId={Uri.EscapeDataString(_ordersId)}");

            // Navigate back
            await Shell.Current.GoToAsync("..");
        }
    }
}

