using LaceupMigration.ViewModels;
using Microsoft.Maui.Graphics;
using System.Collections.Generic;

namespace LaceupMigration.Views
{
    public partial class OrderSignaturePage : IQueryAttributable
    {
        private readonly OrderSignaturePageViewModel _viewModel;

        public OrderSignaturePage(OrderSignaturePageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;

            // Wire up touch events for signature pad
            SignaturePad.StartInteraction += OnSignaturePadStart;
            SignaturePad.DragInteraction += OnSignaturePadDrag;
            SignaturePad.EndInteraction += OnSignaturePadEnd;
        }

        private void OnSignaturePadStart(object? sender, TouchEventArgs e)
        {
            if (e.Touches != null && e.Touches.Length > 0)
            {
                _viewModel.AddPoint(e.Touches[0]);
                SignaturePad.Invalidate(); // Trigger redraw
            }
        }

        private void OnSignaturePadDrag(object? sender, TouchEventArgs e)
        {
            if (e.Touches != null && e.Touches.Length > 0)
            {
                _viewModel.AddPoint(e.Touches[0]);
                SignaturePad.Invalidate(); // Trigger redraw
            }
        }

        private void OnSignaturePadEnd(object? sender, TouchEventArgs e)
        {
            _viewModel.EndStroke();
            SignaturePad.Invalidate(); // Trigger redraw
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query == null)
                return;

            string ordersId = string.Empty;

            if (query.TryGetValue("ordersId", out var ordersIdValue) && ordersIdValue != null)
            {
                ordersId = ordersIdValue.ToString() ?? string.Empty;
            }

            if (!string.IsNullOrEmpty(ordersId))
            {
                Dispatcher.Dispatch(async () => await _viewModel.InitializeAsync(ordersId));
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.OnAppearingAsync();
        }

        protected override bool OnBackButtonPressed()
        {
            // [ACTIVITY STATE]: Remove state when navigating away via back button
            Helpers.NavigationHelper.RemoveNavigationState("ordersignature");
            return false; // Allow navigation
        }
    }
}

