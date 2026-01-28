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

            // Get signature points from drawable (same format as Laceup AddSignature: raw points, Point.Empty between strokes)
            var signaturePoints = _signatureDrawable.GetPoints();
            if (signaturePoints == null || signaturePoints.Count == 0)
            {
                await _dialogService.ShowAlertAsync("Signature is too small.", "Alert", "OK");
                return;
            }

            // Save raw points like Laceup - no scaling, no rounding to 32 (scaling caused black square in ZPL)
            var pointsToSave = new List<SixLabors.ImageSharp.Point>();
            foreach (var point in signaturePoints)
            {
                if (point.IsEmpty)
                    pointsToSave.Add(SixLabors.ImageSharp.Point.Empty);
                else
                    pointsToSave.Add(new SixLabors.ImageSharp.Point((int)point.X, (int)point.Y));
            }

            // Save signature to all orders
            foreach (var order in _orders)
            {
                if (string.IsNullOrEmpty(order.SignatureUniqueId))
                    order.SignatureUniqueId = Guid.NewGuid().ToString("N");

                order.SignatureName = sName;
                order.SignaturePoints = pointsToSave;
                order.Save();
            }

            // Remove navigation state
            Helpers.NavigationHelper.RemoveNavigationState($"ordersignature?ordersId={Uri.EscapeDataString(_ordersId)}");

            // Navigate back
            await Shell.Current.GoToAsync("..");
        }
    }
}

